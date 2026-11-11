// MarkerOnTop.shader
// Owner: Joe
// Layer: Feature, Movement (the material of the teleport ghost markers)
//
// What this is
// A see-through unlit colour that is drawn on top of everything (ZTest Always)
// The ghost markers must stay visible through walls and floors, so you can aim at a pad behind cover
// URP's own Unlit shader always hides what is behind a wall, so the markers need this small shader
//
// Colour
// One colour, _BaseColor, GhostMarkers tints each marker with a MaterialPropertyBlock (normal or selected)
//
// Editor setup
//   1. Create a material with the shader Game/Marker On Top, use it on the disc and arrow marker prefabs

Shader "Game/Marker On Top"
{
    Properties
    {
        _BaseColor ("Color", Color) = (1, 1, 1, 0.6)
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Overlay" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            ZTest Always
            ZWrite Off
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // Both eyes on Quest (single pass instanced), so the marker shows in each eye
            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                return _BaseColor;
            }
            ENDHLSL
        }
    }
}
