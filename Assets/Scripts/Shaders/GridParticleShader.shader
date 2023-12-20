Shader "Custom/GridParticleShader"
{
    //shown in inspector
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        //_MainTex ("Albedo (RGB)", 2D) = "blue" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        _Density("Density", Range(0,100)) = 1.0

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows
        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0
        // for instancing support due to many particles
        #pragma multi_compile_instancing
        //  for generating instances of objects
        #pragma instancing_options procedural:setup

        //sampler2D _MainTex;
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;  // vector (r,g,b,a)
        //to declare density
        float _Density;
        float _Size;

        struct Input
        {
            float2 uv_MainTex;
        };

        struct Particle
        {
            float density;
            float3 velocity;
            float3 predictedPosition;
            float3 position;

        };

        //check if instancing is enabled before adding in buffer shader
        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                //created ID from SPH.cs
                StructuredBuffer<Particle> _ParticlesBuffer;
        #endif

        void setup() 
        {
            #ifdef  UNITY_PROCEDURAL_INSTANCING_ENABLED
                //assigning the values
                //unity_InstanceID is for creating a unique id for each instanciated object
                float3 pos = _ParticlesBuffer[unity_InstanceID].position;
                float size = _Size;

                //scaling
                unity_ObjectToWorld._11_21_31_41 = float4(size, 0, 0, 0);
                unity_ObjectToWorld._12_22_32_42 = float4(0, size, 0, 0);
                unity_ObjectToWorld._13_23_33_43 = float4(0, 0, size, 0);
                //translation
                unity_ObjectToWorld._14_24_34_44 = float4(pos.xyz,1);
                //pass the trnsformation matrix to unity_WorldToObject
                unity_WorldToObject = unity_ObjectToWorld;
                //put to origin
                unity_WorldToObject._14_24_34 *= -1;
                //scaling
                unity_WorldToObject._11_22_33 = 1.0f / unity_WorldToObject._11_22_33;

            #endif
        }

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = _Color;
            o.Albedo = float3(c.r, c.g, c.b);
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;


        }
        ENDCG
    }
    FallBack "Diffuse"
}
