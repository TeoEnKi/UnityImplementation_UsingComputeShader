using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

//instances of this struct can be converted into a stream of bytes to send to custom shader
[System.Serializable]
//give size to keep order of data
[StructLayout(LayoutKind.Sequential, Size = 44)]
public struct Particle
{
    //parameterless properties to set later
    public float pressure;
    public float density;
    public float nearPressure;
    public float nearDensity;
    public Vector3 velocity;
    public Vector3 position;
}

public class SPH : MonoBehaviour
{
    [Header("Particle Spawn")]
    public bool showSpheres = true;
    //number of particles to spawn at each axis
    public Vector3Int numToSpawn = new Vector3Int(10,10,10);
    private int totalParticles
    {
        get
        {
            return numToSpawn.x * numToSpawn.y * numToSpawn.z;
        }
    }
    public Vector3 boxSize = new Vector3(4, 10, 3);
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

    //store and manage arguement data
    private ComputeBuffer argsBuffer;
    //store and manage particle data
    private ComputeBuffer particlesBuffer;
    private void Awake()
    {
        //Setup Args for Instanced Particle Rendering
        uint[] args =
        {
            //get index of mesh (ref to vertices needed to draw mesh)
            particleMesh.GetIndexCount(0),
            //num of particles to instanciate
            (uint)totalParticles,
            //to know where to begin drawing mesh
            particleMesh.GetIndexStart(0),


        };
    }
}

