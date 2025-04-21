
Shader "Rokid/Fresnel"
{
    Properties
    {
        _Color0 ("Color 0", Color) = (1, 1, 1, 0.5882353)
        [Header(Refraction)]
        _ChromaticAberration ("Chromatic Aberration", Range(0, 0.3)) = 0.1
        _Power ("Power", Float) = 0
        _Scale ("Scale", Float) = 0
        _Bias ("Bias", Float) = 0.01
        [HideInInspector] __dirty ("", Int) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Geometry+0" "IsEmissive" = "true" }
        ZWrite Off
        Blend SrcColor OneMinusSrcColor
        Cull Back
        GrabPass { }
        CGINCLUDE
        #include "UnityPBSLighting.cginc"
        #include "Lighting.cginc"
        #pragma target 3.0
        #pragma multi_compile _ALPHAPREMULTIPLY_ON
        #ifdef UNITY_PASS_SHADOWCASTER
            #undef INTERNAL_DATA
            #undef WorldReflectionVector
            #undef WorldNormalVector
            #define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
            #define WorldReflectionVector(data, normal) reflect(data.worldRefl, half3(dot(data.internalSurfaceTtoW0, normal), dot(data.internalSurfaceTtoW1, normal), dot(data.internalSurfaceTtoW2, normal)))
            #define WorldNormalVector(data, normal) half3(dot(data.internalSurfaceTtoW0, normal), dot(data.internalSurfaceTtoW1, normal), dot(data.internalSurfaceTtoW2, normal))
        #endif
        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
            INTERNAL_DATA
            float4 screenPos;
        };

        struct SurfaceOutputStandardCustom
        {
            half3 Albedo;
            half3 Normal;
            half3 Emission;
            half Metallic;
            half Smoothness;
            half Occlusion;
            half Alpha;
            half3 Transmission;
        };

        uniform float4 _Color0;
        uniform float _Bias;
        uniform float _Scale;
        uniform float _Power;
        uniform sampler2D _GrabTexture;
        uniform float _ChromaticAberration;

        inline half4 LightingStandardCustom(SurfaceOutputStandardCustom s, half3 viewDir, UnityGI gi)
        {
            half3 transmission = max(0, -dot(s.Normal, gi.light.dir)) * gi.light.color * s.Transmission;
            half4 d = half4(s.Albedo * transmission, 0);

            SurfaceOutputStandard r;
            r.Albedo = s.Albedo;
            r.Normal = s.Normal;
            r.Emission = s.Emission;
            r.Metallic = s.Metallic;
            r.Smoothness = s.Smoothness;
            r.Occlusion = s.Occlusion;
            r.Alpha = s.Alpha;
            return LightingStandard(r, viewDir, gi) + d;
        }

        inline void LightingStandardCustom_GI(SurfaceOutputStandardCustom s, UnityGIInput data, inout UnityGI gi)
        {
            #if defined(UNITY_PASS_DEFERRED) && UNITY_ENABLE_REFLECTION_BUFFERS
                gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal);
            #else
                UNITY_GLOSSY_ENV_FROM_SURFACE(g, s, data);
                gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal, g);
            #endif
        }

        inline float4 Refraction(Input i, SurfaceOutputStandardCustom o, float indexOfRefraction, float chomaticAberration)
        {
            float3 worldNormal = o.Normal;
            float4 screenPos = i.screenPos;
            #if UNITY_UV_STARTS_AT_TOP
                float scale = -1.0;
            #else
                float scale = 1.0;
            #endif
            float halfPosW = screenPos.w * 0.5;
            screenPos.y = (screenPos.y - halfPosW) * _ProjectionParams.x * scale + halfPosW;
            #if SHADER_API_D3D9 || SHADER_API_D3D11
                screenPos.w += 0.00000000001;
            #endif
            float2 projScreenPos = (screenPos / screenPos.w).xy;
            float3 worldViewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
            float3 refractionOffset = (indexOfRefraction - 1.0) * mul(UNITY_MATRIX_V, float4(worldNormal, 0.0)) * (1.0 - dot(worldNormal, worldViewDir));
            float2 cameraRefraction = float2(refractionOffset.x, refractionOffset.y);
            float4 redAlpha = tex2D(_GrabTexture, (projScreenPos + cameraRefraction));
            float green = tex2D(_GrabTexture, (projScreenPos + (cameraRefraction * (1.0 - chomaticAberration)))).g;
            float blue = tex2D(_GrabTexture, (projScreenPos + (cameraRefraction * (1.0 + chomaticAberration)))).b;
            return float4(redAlpha.r, green, blue, redAlpha.a);
        }

        void RefractionF(Input i, SurfaceOutputStandardCustom o, inout half4 color)
        {
            #ifdef UNITY_PASS_FORWARDBASE
                float3 ase_worldPos = i.worldPos;
                float3 ase_worldViewDir = normalize(UnityWorldSpaceViewDir(ase_worldPos));
                float3 ase_worldNormal = WorldNormalVector(i, float3(0, 0, 1));
                float fresnelNdotV2 = dot(ase_worldNormal, ase_worldViewDir);
                float fresnelNode2 = (_Bias + _Scale * pow(1.0 - fresnelNdotV2, _Power));
                float4 temp_output_3_0 = (_Color0 * fresnelNode2);
                color.rgb = color.rgb + Refraction(i, o, temp_output_3_0.r, _ChromaticAberration) * (1 - color.a);
                color.a = 1;
            #endif
        }

        void surf(Input i, inout SurfaceOutputStandardCustom o)
        {
            o.Normal = float3(0, 0, 1);
            float3 ase_worldPos = i.worldPos;
            float3 ase_worldViewDir = normalize(UnityWorldSpaceViewDir(ase_worldPos));
            float3 ase_worldNormal = WorldNormalVector(i, float3(0, 0, 1));
            float fresnelNdotV2 = dot(ase_worldNormal, ase_worldViewDir);
            float fresnelNode2 = (_Bias + _Scale * pow(1.0 - fresnelNdotV2, _Power));
            float4 temp_output_3_0 = (_Color0 * fresnelNode2);
            o.Albedo = temp_output_3_0.rgb;
            o.Emission = temp_output_3_0.rgb;
            o.Transmission = temp_output_3_0.rgb;
            o.Alpha = 1;
            o.Normal = o.Normal + 0.00001 * i.screenPos * i.worldPos;
        }

        ENDCG


        CGPROGRAM

        #pragma surface surf StandardCustom keepalpha finalcolor:RefractionF fullforwardshadows exclude_path:deferred

        ENDCG

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite Off
            Blend SrcColor OneMinusSrcColor
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_shadowcaster
            #pragma multi_compile UNITY_PASS_SHADOWCASTER
            #pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
            #include "HLSLSupport.cginc"
            #if (SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN)
                #define CAN_SKIP_VPOS
            #endif
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            struct v2f
            {
                V2F_SHADOW_CASTER;
                float4 screenPos: TEXCOORD1;
                float4 tSpace0: TEXCOORD2;
                float4 tSpace1: TEXCOORD3;
                float4 tSpace2: TEXCOORD4;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };
            v2f vert(appdata_full v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                half3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
                half3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
                o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
                o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
                o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                o.screenPos = ComputeScreenPos(o.pos);
                return o;
            }
            half4 frag(v2f IN): SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                Input surfIN;
                UNITY_INITIALIZE_OUTPUT(Input, surfIN);
                float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
                half3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                surfIN.worldPos = worldPos;
                surfIN.worldNormal = float3(IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z);
                surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
                surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
                surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
                surfIN.screenPos = IN.screenPos;
                SurfaceOutputStandardCustom o;
                UNITY_INITIALIZE_OUTPUT(SurfaceOutputStandardCustom, o)
                surf(surfIN, o);
                #if defined(CAN_SKIP_VPOS)
                    float2 vpos = IN.pos;
                #endif
                SHADOW_CASTER_FRAGMENT(IN)
            }
            ENDCG

        }
    }
    Fallback "Diffuse"
}
/*ASEBEGIN
Version=18935
2445.333;72.66667;691.3333;1326.333;341.6791;1034.048;1.504851;True;False
Node;AmplifyShaderEditor.RangedFloatNode;9;-320.4344,260.7581;Inherit;False;Property;_Power;Power;4;0;Create;True;0;0;0;False;0;False;0;0.75;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;10;-338.2071,138.8885;Inherit;False;Property;_Scale;Scale;5;0;Create;True;0;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;11;-322.9734,36.0611;Inherit;False;Property;_Bias;Bias;6;0;Create;True;0;0;0;False;0;False;0.01;0.01;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;1;-133.6667,-244.1667;Inherit;False;Property;_Color0;Color 0;1;0;Create;True;0;0;0;False;0;False;1,1,1,0.5882353;0,0.7403493,1,0.5882353;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FresnelNode;2;-122.2464,-1.188284;Inherit;True;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0.01;False;2;FLOAT;1;False;3;FLOAT;0.75;False;1;FLOAT;0
Node;AmplifyShaderEditor.VoronoiNode;8;-605.8656,-53.57059;Inherit;False;0;0;1;0;1;False;1;False;False;False;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;3;FLOAT;0;FLOAT2;1;FLOAT2;2
Node;AmplifyShaderEditor.TexCoordVertexDataNode;7;-730.8656,-382.5706;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;5;142.6356,-335.817;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;6;-360.3655,-142.0706;Inherit;False;Simplex2D;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;4;-254.6667,-484.1667;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;3;447.2924,-179.9303;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;760.7999,-385.1999;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Fresnel;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;Transparent;;Geometry;ForwardOnly;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;0;-1;2;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;2;1;11;0
WireConnection;2;2;10;0
WireConnection;2;3;9;0
WireConnection;6;0;7;0
WireConnection;3;0;1;0
WireConnection;3;1;2;0
WireConnection;0;0;3;0
WireConnection;0;2;3;0
WireConnection;0;6;3;0
WireConnection;0;8;3;0
ASEEND*/
// CHKSM = A96CCB55C6F9E902D18F9651945017E61DA527F9