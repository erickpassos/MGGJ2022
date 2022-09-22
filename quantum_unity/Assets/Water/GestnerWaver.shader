Shader "Custom/GestnerWaves"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _Glossiness("Glossiness", Range(0,1)) = 0
        _Specular("Specular", Range(0,1)) = 0
        // waves
        _Gravity ("Gravity", float) = 9.8
        
        _SteepnessA ("SteepnessA", Range(0, 1)) = 0.5
        _WaveLengthA("WavelengthA", Float) = 10
        _DirectionA ("DirectionA (2D)", Vector) = (1,0,0,0)
        
        _SteepnessB ("SteepnessB", Range(0, 1)) = 0.5
        _WaveLengthB("WavelengthB", Float) = 10
        _DirectionB ("DirectionB (2D)", Vector) = (1,0,0,0)
        
        _SteepnessC ("SteepnessC", Range(0, 1)) = 0.5
        _WaveLengthC("WavelengthC", Float) = 10
        _DirectionC ("DirectionC (2D)", Vector) = (1,0,0,0)

        [Header(WaterFog)]
        _WaterFogDensity ("Water Fog Density", Range(0, 2)) = 0.1
        _WaterFoamDepthCut ("Water Foam Depth", Range(0, 1)) = 0.75

        [Header(Runtime)]
        // runtime params
        _RenderTime("_RenderTime", Float) = 1
        _X("_X", Float) = 1
        _Z("_Z", Float) = 1
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent" "RenderType"="Transparent"
        }
        LOD 200
        Cull Back
        ZWrite On
        ZTest Less
        GrabPass
        {
            "_WaterBackground"
        }
        CGPROGRAM
        #pragma surface surf StandardSpecular vertex:vert
        #pragma target 3.0
        #pragma multi_compile_fog

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            float4 screenPos;
        };

        struct GestnerResult
        {
            float3 normal;
            float3 pos;
        };

        half _Glossiness, _Specular;
        fixed4 _Color;
        float2 _DirectionA, _DirectionB, _DirectionC;
        float _Gravity, _SteepnessA, _WaveLengthA, _SteepnessB, _WaveLengthB, _SteepnessC, _WaveLengthC, _RenderTime, _X, _Z;
        sampler2D _CameraDepthTexture, _WaterBackground;
        float4 _CameraDepthTexture_TexelSize;
        float _WaterFogDensity, _WaterFoamDepthCut, _FogDistance;

        GestnerResult Gestner(float3 pos, float2 uv, float wavelength, float steepness, float2 direction)
        {
            GestnerResult result;
			float3 p = pos;
            pos.x += _X;
            pos.z += _Z;

            const float k = 2 * UNITY_PI / wavelength;
            const float c = sqrt(_Gravity / k);
            const float a = steepness / k;
            float2 d = normalize(direction);
			float f = k * (dot(d, pos.xz) - c * _RenderTime);

            // local space
            p.x = d.x * (a * cos(f));
			p.y = a * sin(f);
			p.z = d.y * (a * cos(f));

            // flat shading
            uv.x += _X;
            uv.y += _Z;
            f = k * (dot(d, uv) - c * _RenderTime);
   
            float3 tangent = float3(
				1 - d.x * d.x * (steepness * sin(f)),
				d.x * (steepness * cos(f)),
				-d.x * d.y * (steepness * sin(f))
			);
			float3 binormal = float3(
				-d.x * d.y * (steepness * sin(f)),
				d.y * (steepness * cos(f)),
				1 - d.y * d.y * (steepness * sin(f))
			);
			result.normal = normalize(cross(binormal, tangent));
        	result.pos = p;
            return result;
        }

        void vert(inout appdata_full vertexData)
        {
            float3 p = vertexData.vertex.xyz;
            float2 uv = vertexData.texcoord.xy;
            GestnerResult gestnerA = Gestner(p, uv, _WaveLengthA, _SteepnessA, _DirectionA);
            GestnerResult gestnerB = Gestner(p, uv, _WaveLengthB, _SteepnessB, _DirectionB);
            GestnerResult gestnerC = Gestner(p, uv, _WaveLengthC, _SteepnessC, _DirectionC);
            vertexData.vertex.xyz += gestnerA.pos + gestnerB.pos + gestnerC.pos;
            vertexData.normal = (gestnerA.normal + gestnerB.normal + gestnerC.normal) / 3;

            //float3 tangent = normalize(float3(1, vertexData.vertex.y, 0));
			//vertexData.normal = float3(-tangent.y, tangent.x, 0);
        }

        

        float3 ColorBelowWater(float4 screenPos, float3 waterColor)
        {
            float2 uv = screenPos.xy / screenPos.w;
            #if UNITY_UV_STARTS_AT_TOP
            if (_CameraDepthTexture_TexelSize.y < 0)
            {
                uv.y = 1 - uv.y;
            }
            #endif
            float backgroundDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));
            float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(screenPos.z);
            float depthDifference = backgroundDepth - surfaceDepth;

            float3 backgroundColor = tex2D(_WaterBackground, uv).rgb;
            float fogFactor = exp2(-_WaterFogDensity * depthDifference);
            float3 white = float3(1, 1, 1);
            if (fogFactor > _WaterFoamDepthCut) return white;
            return lerp(waterColor, backgroundColor, fogFactor);
        }

        float3 ApplyFog(float3 color, float3 pos, inout float gloss)
        {
            float viewDistance = length(_WorldSpaceCameraPos - pos);
            UNITY_CALC_FOG_FACTOR_RAW(viewDistance);
            float alpha = saturate(unityFogFactor);
            gloss = lerp(0, gloss, alpha);
            color.rgb = lerp(unity_FogColor.rgb, color.rgb, alpha);
            return color;
        }


        void surf(Input IN, inout SurfaceOutputStandardSpecular o)
        {
            fixed4 c = _Color;
            c.b = o.Normal.y - 0.1;
            c.r = 0.8 - o.Normal.y;

            float3 c2 = ColorBelowWater(IN.screenPos, c.rgb);

            o.Albedo = c2; //ApplyFog(c2, IN.worldPos, _Glossiness);
            o.Emission = c2 * (1 - c.a); //ApplyFog(c2 * (1 - c.a), IN.worldPos, _Glossiness);
            o.Smoothness = _Glossiness;
            o.Specular = _Specular;
        }
        ENDCG
    }
    FallBack "Diffuse"
}