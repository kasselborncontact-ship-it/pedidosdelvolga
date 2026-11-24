// Pasto con viento para URP. Lo usa GrassField (dibujado con GPU instancing).
// Color: de _ColorBase (al pie) a _ColorTip (en la punta). Recibe la sombra del sol.
Shader "LaTranquera/Pasto"
{
    Properties
    {
        _ColorBase ("Color al pie", Color) = (0.18, 0.32, 0.1, 1)
        _ColorTip ("Color en la punta", Color) = (0.55, 0.7, 0.3, 1)
        _Wind ("Fuerza del viento", Float) = 0.12
        _WindSpeed ("Velocidad del viento", Float) = 1.6
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _ColorBase;
                half4 _ColorTip;
                float _Wind;
                float _WindSpeed;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float height : TEXCOORD1;
                float fogFactor : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert (Attributes input)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, o);
                float3 ws = TransformObjectToWorld(input.positionOS.xyz);
                float h = input.uv.y;
                float t = _Time.y * _WindSpeed;
                float w = sin(t + ws.x * 0.35 + ws.z * 0.27) + 0.45 * sin(t * 2.3 + ws.x * 0.9 + ws.z * 0.6);
                float bend = w * _Wind * h * h;
                ws.x += bend;
                ws.z += bend * 0.5;
                o.positionWS = ws;
                o.positionCS = TransformWorldToHClip(ws);
                o.height = h;
                o.fogFactor = ComputeFogFactor(o.positionCS.z);
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                half3 albedo = lerp(_ColorBase.rgb, _ColorTip.rgb, saturate(i.height));
                float4 shadowCoord = TransformWorldToShadowCoord(i.positionWS);
                Light l = GetMainLight(shadowCoord);
                half sunUp = saturate(l.direction.y * 1.5 + 0.2);
                half3 light = l.color * sunUp * (0.55 + 0.45 * l.shadowAttenuation);
                half3 ambient = SampleSH(half3(0, 1, 0));
                half3 col = albedo * (light + ambient);
                col = MixFog(col, i.fogFactor);
                return half4(col, 1);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
