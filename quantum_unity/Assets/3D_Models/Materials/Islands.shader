Shader "Custom/Islands" {
	Properties {
      _MainTex ("Texture", 2D) = "white" {}
      _RimColor ("Rim Color", Color) = (0.26,0.19,0.16,0.0)
      _RimPower ("Rim Power", Range(0.5,8.0)) = 3.0
      _WhiteBalance ("White Balance", Color) = (0.26,0.19,0.16,0.0)
    }
    SubShader {
      Tags { "RenderType" = "Opaque" }
      CGPROGRAM
      #pragma surface surf Lambert
      #include "custom-utils.cginc"
      struct Input {
          float2 uv_MainTex;
          float2 uv_BumpMap;
          float3 viewDir;
      };
      sampler2D _MainTex;
      sampler2D _BumpMap;
      float4 _RimColor;
      float _RimPower;
      float4 _WhiteBalance;

      void surf (Input IN, inout SurfaceOutput o) {
          float4 color = tex2D (_MainTex, IN.uv_MainTex).rgba;
          float projection = dot(_WorldSpaceLightPos0, -o.Normal) ;
          color = color * clamp(1 - projection, 0.2, 1);        
          color = applyHSBEffect(color, _WhiteBalance);  
          o.Albedo = color;
          half rim = 1.0 - saturate(dot (normalize(IN.viewDir), o.Normal));
          o.Emission = _RimColor.rgb * pow (rim, _RimPower);
      }
      ENDCG
    } 
    Fallback "Diffuse"
}