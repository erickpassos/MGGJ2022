Shader "Custom/Waves" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Glossiness", Range(0,1)) = 0
		_Specular("Specular", Range(0,1)) = 0
		// waves
		_AmplitudeA("AmplitudeA", Float) = 1
		_WaveLengthA("WavelengthA", Float) = 10
		_SpeedA("SpeedA", Float) = 1
		_AmplitudeB("AmplitudeB", Float) = 1
		_WaveLengthB("WavelengthB", Float) = 10
		_SpeedB("SpeedB", Float) = 1
		_AmplitudeC("AmplitudeC", Float) = 1
		_WaveLengthC("WavelengthC", Float) = 10
		_SpeedC("SpeedC", Float) = 1
        [Header(WaterFog)]
		_WaterFogDensity ("Water Fog Density", Range(0, 2)) = 0.1
		_WaterFoamDepthCut ("Water Foam Depth", Range(0, 1)) = 0.75

        [Header(Runtime)]
		// runtime params
		_RenderTime("_RenderTime", Float) = 1
		_X("_X", Float) = 1
		_Z("_Z", Float) = 1
	}
		SubShader{
			Tags {"Queue"="Transparent" "RenderType"="Transparent" }
			LOD 200
			Cull Back
			ZWrite On
			ZTest Less
            GrabPass { "_WaterBackground" }
			CGPROGRAM

			#pragma surface surf StandardSpecular vertex:vert
			#pragma target 3.0
			#pragma multi_compile_fog

			sampler2D _MainTex;

			struct Input {
				float2 uv_MainTex;
				float3 worldPos;
				float4 screenPos;
			};

			half _Glossiness, _Specular;
			fixed4 _Color;
			float _AmplitudeA, _WaveLengthA, _SpeedA, _AmplitudeB, _WaveLengthB, _SpeedB, _AmplitudeC, _WaveLengthC, _SpeedC, _RenderTime, _X, _Z;
			sampler2D _CameraDepthTexture, _WaterBackground;
			float4 _CameraDepthTexture_TexelSize;
            float _WaterFogDensity, _WaterFoamDepthCut, _FogDistance;

			void vert(inout appdata_full vertexData) {
				float3 p = vertexData.vertex.xyz;
				float3 pos = p;
				
				pos.x += _X;
				pos.z += _Z;

				float kA = 2 * UNITY_PI / _WaveLengthA;
				float kB = 2 * UNITY_PI / _WaveLengthB;
				float kC = 2 * UNITY_PI / _WaveLengthC;

				float f = kA * (pos.x + pos.z - _SpeedA * _RenderTime);
				p.y = _AmplitudeA * sin(f);

				f = kB * (pos.x - _SpeedB * _RenderTime);
				p.y += _AmplitudeB * sin(f);

				f = kC * (pos.z - _SpeedC * _RenderTime);
				p.y += _AmplitudeC * sin(f);


				float2 uv = vertexData.texcoord.xy;
				uv.x += _X;
				uv.y += _Z;

				f = kA * (uv.x + uv.y - _SpeedA * _RenderTime);
				float height = kA * _AmplitudeA * cos(f);
				f = kB * (uv.x  - _SpeedB * _RenderTime);
				height += kB * _AmplitudeB * cos(f);
				f = kC * (uv.y - _SpeedC * _RenderTime);
				height += kC * _AmplitudeC * cos(f);

				float3 tangent = normalize(float3(1, height, 0));
				float3 normal = float3(-tangent.y, tangent.x, 0);

				vertexData.vertex.xyz = p;
				vertexData.normal = normal;
			}
			
			float3 ColorBelowWater (float4 screenPos, float3 waterColor) {
	            float2 uv = screenPos.xy / screenPos.w;
	            #if UNITY_UV_STARTS_AT_TOP
		        if (_CameraDepthTexture_TexelSize.y < 0) {
			        uv.y = 1 - uv.y;
		        }
	            #endif
	            float backgroundDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));
	            float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(screenPos.z);
	            float depthDifference = backgroundDepth - surfaceDepth;
	            
	            float3 backgroundColor = tex2D(_WaterBackground, uv).rgb;
	            float fogFactor = exp2(-_WaterFogDensity * depthDifference);
	            float3 white = float3(1,1,1);
	            if (fogFactor > _WaterFoamDepthCut) return white;
	            return lerp(waterColor, backgroundColor, fogFactor); 
            }

			float3 ApplyFog(float3 color, float3 pos, inout float gloss) {
				float viewDistance = length(_WorldSpaceCameraPos - pos);
				UNITY_CALC_FOG_FACTOR_RAW(viewDistance);
				float alpha = saturate(unityFogFactor);
				gloss = lerp(0, gloss, alpha);
				color.rgb = lerp(unity_FogColor.rgb, color.rgb, alpha);
				return color;
			}


			void surf(Input IN, inout SurfaceOutputStandardSpecular o) {
				fixed4 c = _Color;
				c.b = o.Normal.y - 0.1;
				c.r = 0.8 - o.Normal.y;

				float3 c2 = ColorBelowWater(IN.screenPos, c.rgb);
				
				o.Albedo = c2;//ApplyFog(c2, IN.worldPos, _Glossiness);
				o.Emission = c2 * (1 - c.a);//ApplyFog(c2 * (1 - c.a), IN.worldPos, _Glossiness);
				o.Smoothness = _Glossiness;
				o.Specular = _Specular;
			}
			ENDCG
		}
			FallBack "Diffuse"
}