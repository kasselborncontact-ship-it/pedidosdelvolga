// Hojas de árboles con viento suave para URP (copas que se mecen). Facetado low poly.
// Iluminación: sol con sombras + luz ambiente + niebla. Proyecta sombra.
Shader "LaTranquera/Hojas"
{
    Properties
    {
        _BaseColor ("Color", Color) = (0.3, 0.5, 0.2, 1)
        _Variation ("Variación de color", Range(0, 0.5)) = 0.18
        _Wind ("Fuerza del viento", Float) = 0.12
        _WindSpeed ("Velocidad del viento", Float) = 1.1
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            half _Variation;
            float _Wind;
            float _WindSpeed;
        CBUFFER_END

        float3 WindOffset(float3 ws, float3 originWS)
        {
            float h = saturate((ws.y - originWS.y) / 8.0);
            float t = _Time.y * _WindSpeed;
            float w = sin(t + ws.x * 0.25 + ws.z * 0.2) + 0.5 * sin(t * 2.7 + ws.y * 0.8 + ws.x * 0.5);
            return float3(w, 0.15 * sin(t * 3.1 + ws.z), w * 0.6) * _Wind * h;
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; float3 normalWS : TEXCOORD1; float fogFactor : TEXCOORD2; float tint : TEXCOORD3; };

            Varyings vert (Attributes i)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(i);
                float3 origin = TransformObjectToWorld(float3(0, 0, 0));
                float3 ws = TransformObjectToWorld(i.positionOS.xyz);
                ws += WindOffset(ws, origin);
                o.positionWS = ws;
                o.normalWS = TransformObjectToWorldNormal(i.normalOS);
                o.positionCS = TransformWorldToHClip(ws);
                o.fogFactor = ComputeFogFactor(o.positionCS.z);
                o.tint = frac(sin(dot(floor(i.positionOS.xyz * 3.0), float3(12.9898, 78.233, 37.719))) * 43758.5453);
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                float3 n = normalize(i.normalWS);
                half3 albedo = _BaseColor.rgb * (1.0 + (i.tint - 0.5) * 2.0 * _Variation);
                // un poco más claro arriba, más oscuro abajo (luz que pasa entre las hojas)
                albedo *= lerp(0.75, 1.1, saturate(n.y * 0.5 + 0.5));
                Light l = GetMainLight(TransformWorldToShadowCoord(i.positionWS));
                half ndl = saturate(dot(n, l.direction) * 0.6 + 0.4); // luz "envolvente": las hojas dejan pasar luz
                half3 col = albedo * (l.color * ndl * l.shadowAttenuation + SampleSH(n));
                col = MixFog(col, i.fogFactor);
                return half4(col, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On ZTest LEqual ColorMask 0
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            float3 _LightDirection;
            float3 _LightPosition;

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; };

            Varyings vert (Attributes i)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(i);
                float3 origin = TransformObjectToWorld(float3(0, 0, 0));
                float3 ws = TransformObjectToWorld(i.positionOS.xyz);
                ws += WindOffset(ws, origin);
                float3 nws = TransformObjectToWorldNormal(i.normalOS);
                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 ld = normalize(_LightPosition - ws);
                #else
                    float3 ld = _LightDirection;
                #endif
                float4 cs = TransformWorldToHClip(ApplyShadowBias(ws, nws, ld));
                #if UNITY_REVERSED_Z
                    cs.z = min(cs.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    cs.z = max(cs.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                o.positionCS = cs;
                return o;
            }

            half4 frag (Varyings i) : SV_Target { return 0; }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On ColorMask R
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            struct Attributes { float4 positionOS : POSITION; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; };
            Varyings vert (Attributes i)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(i);
                float3 origin = TransformObjectToWorld(float3(0, 0, 0));
                float3 ws = TransformObjectToWorld(i.positionOS.xyz);
                ws += WindOffset(ws, origin);
                o.positionCS = TransformWorldToHClip(ws);
                return o;
            }
            half4 frag (Varyings i) : SV_Target { return 0; }
            ENDHLSL
        }
    }
    FallBack Off
}
