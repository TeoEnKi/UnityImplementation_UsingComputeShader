using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

//instances of this struct can be converted into a stream of bytes to send to custom shader
[System.Serializable]
//give size to keep order of data
[StructLayout(LayoutKind.Sequential, Size = 40)]
public struct Particle
{
    //parameterless properties to set later
    public float pressure; // 4 bytes
    public float density;
    public float nearPressure;
    public float nearDensity;
    public Vector3 velocity; // 4 bytes * 3 = 12
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
    public Vector3 boxSize;
    public Vector3 spawnPoint = Vector3.zero;
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

    //store and manage arguement data
    private ComputeBuffer argsBuffer;
    //store and manage particle data
    private ComputeBuffer particlesBuffer;
    private void Awake()
    {
        boundary = FindAnyObjectByType<Boundary>();
        boxSize = boundary.boxSize;

        SpawnParticles();

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
            0
        };

        //number of elements, byte size 
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);

        //Setup Particle Buffer for intancing particles
        particlesBuffer = new ComputeBuffer(totalParticles, 40);
        //list of particle objects in buffer list
        particlesBuffer.SetData(particles);
    }

    //converting the prperty to id for efficiency
    private static readonly int SizeProperty = Shader.PropertyToID("size");
    private static readonly int ParticleBufferProperty = Shader.PropertyToID("particlesBuffer");
    private void Update()
    {
        //render the particles (show it in game scene)
        particleMaterial.SetFloat(SizeProperty, particleRenderSize);
        particleMaterial.SetBuffer(ParticleBufferProperty, particlesBuffer);

        if (showSpheres)
        {
            //many instances of sphere meshes
            Graphics.DrawMeshInstancedIndirect(
                particleMesh,
                0,
                particleMaterial,
                new Bounds(Vector3.zero, boxSize),
                argsBuffer,
                castShadows: UnityEngine.Rendering.ShadowCastingMode.Off
                );

        }
    }
    //spawn particles in a grid within a boundary
    //add them in compute buffer
    private void SpawnParticles()
    {
        List<Particle> tempParticles = new List<Particle>();

        for (int x = 0; x < numToSpawn.x; x++)
        {
            for (int y = 0; y < numToSpawn.y; y++)
            {
                for (int z = 0; z < numToSpawn.z; z++)
                {
                    Vector3 spawnPos = spawnPoint + new Vector3(x * particleRadius * 2, y * particleRadius * 2, z * particleRadius * 2);
                    Particle p = new Particle
                    {
                        position = spawnPos,
                    };
                    tempParticles.Add(p);
                }
            }
        }
        particles = tempParticles.ToArray();
    }

}

