using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

//instances of this struct can be converted into a stream of bytes to send to custom shader
[System.Serializable]
//give size to keep order of data
[StructLayout(LayoutKind.Sequential, Size = 40)]
public struct Particle
{
    //parameterless properties to set later
    public float density;
    public Vector3 velocity; // 4 bytes * 3 = 12
    public Vector3 predictedPosition;
    public Vector3 position;
}

public class SPH : MonoBehaviour
{
    [Header("Particle Spawn")]
    public bool showSpheres = true;
    //number of particles to spawn at each axis
    public Vector3Int numToSpawn = new Vector3Int(10, 10, 10);
    private int totalParticles
    {
        get
        {
            return numToSpawn.x * numToSpawn.y * numToSpawn.z;
        }
    }
    Boundary boundary;
    Vector3 boxSize;
    Vector3 boxSpawn;
    public Vector3 spawnCenter = Vector3.zero;
    //ensure smoothing radius is bigger than particle radius
    public float particleRadius = 0.1f;

    [Header("Particle Rendering")]
    public Mesh particleMesh;
    public float particleRenderSize = 8f;
    public Material particleMaterial;

    [Header("Compute")]
    //to get the particle instances from the compute shader
    public ComputeShader shader;
    public Particle[] particles;

    [Header("External Factors")]
    public float gravity = 9.8f;

    [Header("Fluid Constants")]
    [Range(-0.9f, 0)] public float boundDamping = -0.5f;
    public float viscosityStrength = 200f;
    public float pressureMultiplier = 1.2f;
    public float particleMass = 2.5f;
    public float restingDensity = 300.0f; // Water
    public float timeStep = 0.007f;

    //store and manage arguement data
    private ComputeBuffer argsBuffer;
    //store and manage particle data
    private ComputeBuffer particlesBuffer;

    private void Awake()
    {
        boundary = FindAnyObjectByType<Boundary>();

        SpawnParticles();

        //set up the buffers for the kernel in the compute shader
        SetupComputeBuffers();
    }

    //converting the prperty to id for efficiency
    //_Size and is found _ParticlesBuffer in shader //keys
    private static readonly int SizeProperty = Shader.PropertyToID("_Size");
    private static readonly int ParticleBufferProperty = Shader.PropertyToID("_ParticlesBuffer");
    private void Update()
    {
        if (showSpheres)
        {
            //many instances of sphere meshes cretaed
            Graphics.DrawMeshInstancedIndirect
            (
                particleMesh,
                0,
                //material moves the mesh
                particleMaterial,
                new Bounds(boxSpawn, boxSize),
                argsBuffer,
                castShadows: UnityEngine.Rendering.ShadowCastingMode.Off
            );
        }
    }
    private void FixedUpdate()
    {

        boxSize = boundary.boxSize;
        boxSpawn = boundary.transform.position;

        //size and time may be changing due to use input
        shader.SetFloat("timeStep", timeStep);
        shader.SetVector("boxSize", boxSize);
        shader.SetVector("boxSpawn", boxSpawn);

        //run the kernels //!!ENSURE TOTALPARTICALS IS DIVISIBLE BY 50
        shader.Dispatch(integrateKernel, totalParticles / 50, 1, 1);
        shader.Dispatch(densityKernel, totalParticles / 50, 1, 1);
        shader.Dispatch(pressureKernel, totalParticles / 50, 1, 1);
        shader.Dispatch(viscosityKernel, totalParticles / 50, 1, 1);
        shader.Dispatch(externalKernal, totalParticles / 50, 1, 1);


        //render the particles (show it in game scene)
        particleMaterial.SetFloat(SizeProperty, particleRenderSize);
        particleMaterial.SetBuffer(ParticleBufferProperty, particlesBuffer);

        particlesBuffer.GetData(particles);

    }

    //TODO:
    //include the user inputs and set density and pressure and boxsize
    //shader.SetFloat()

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(spawnCenter, 1f);
        }
    }

    //spawn particles in a grid within a boundary
    //add them in compute buffer
    private void SpawnParticles()
    {
        Vector3 spawnBox = new Vector3(
            numToSpawn.x * (particleRadius * 2),
            numToSpawn.y * (particleRadius * 2),
            numToSpawn.z * (particleRadius * 2)
            );
        Vector3 spawnTopLeft = spawnCenter - spawnBox / 2;
        List<Particle> tempParticles = new List<Particle>();

        for (int x = 0; x < numToSpawn.x; x++)
        {
            for (int y = 0; y < numToSpawn.y; y++)
            {
                for (int z = 0; z < numToSpawn.z; z++)
                {
                    Vector3 spawnPos = spawnTopLeft + new Vector3(x * particleRadius * 2, y * particleRadius * 2, z * particleRadius * 2) + Random.onUnitSphere * particleRadius * 0.5f;
                    Particle p = new Particle
                    {
                        predictedPosition = spawnPos,
                        position = spawnPos
                    };
                    tempParticles.Add(p);
                }
            }
        }
        particles = tempParticles.ToArray();

    }
    private int integrateKernel;
    private int densityKernel;
    private int pressureKernel;
    private int viscosityKernel;
    private int externalKernal;

    //link this to the SPHcomputeshader file and add the values to the variables that have their names match to the strings (keys)
    private void SetupComputeBuffers()
    {
        //Setup Arguements for Instanced Particle Rendering
        uint[] args =
        {
            //get index of mesh (ref to vertices needed to draw mesh)
            particleMesh.GetIndexCount(0),
            //num of particles to instanciate
            (uint)totalParticles,
            //to know where to begin drawing mesh
            particleMesh.GetIndexStart(0),
            particleMesh.GetBaseVertex(0),
            0 , //offset 
        };

        //number of elements, byte size 
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);

        //Setup Particle Buffer for intancing particles
        particlesBuffer = new ComputeBuffer(totalParticles, 40);
        //list of particle objects in buffer list
        particlesBuffer.SetData(particles);

        integrateKernel = shader.FindKernel("Integrate");
        densityKernel = shader.FindKernel("ComputeDensity");
        pressureKernel = shader.FindKernel("ComputePressureForce");
        viscosityKernel = shader.FindKernel("ComputeViscosityForce");
        externalKernal = shader.FindKernel("ExternalForces");

        shader.SetFloat("renderRadius", particleRenderSize);

        shader.SetFloat("gravity", gravity);

        shader.SetInt("particleLength", totalParticles);
        shader.SetFloat("particleMass", particleMass);
        shader.SetFloat("viscosityStrength", viscosityStrength);
        shader.SetFloat("pressureMultiplier", pressureMultiplier);
        shader.SetFloat("restDensity", restingDensity);
        shader.SetFloat("boundDamping", boundDamping);
        shader.SetFloat("pi", Mathf.PI);

        //intense power operations are expensive so they are calculated and stored into the variables
        shader.SetFloat("radius", particleRadius);
        shader.SetFloat("radius2", Mathf.Pow(particleRadius, 2));
        shader.SetFloat("radius3", Mathf.Pow(particleRadius, 3));
        shader.SetFloat("radius4", Mathf.Pow(particleRadius, 4));
        shader.SetFloat("radius5", Mathf.Pow(particleRadius, 5));
        shader.SetFloat("radius6", Mathf.Pow(particleRadius, 6));

        //in the shader, in the Integrate kernel, the particlesBuffer value is passed into a read/write buffer particle list called "particles"
        shader.SetBuffer(integrateKernel, "particles", particlesBuffer);
        shader.SetBuffer(densityKernel, "particles", particlesBuffer);
        shader.SetBuffer(pressureKernel, "particles", particlesBuffer);
        shader.SetBuffer(viscosityKernel, "particles", particlesBuffer);
        shader.SetBuffer(externalKernal, "particles", particlesBuffer);

    }
    void ReleaseBuffer()
    {
        // Release the computeBuffer when it's no longer needed
        if (argsBuffer != null)
        {
            argsBuffer.Release();
            argsBuffer = null; // Set to null to prevent further use
        }
        if (particlesBuffer != null)
        {
            particlesBuffer.Release();
            particlesBuffer = null; // Set to null to prevent further use
        }
    }

    void OnDestroy()
    {
        // Make sure to release the buffer when the GameObject is destroyed to prevent leak
        ReleaseBuffer();
    }
}

