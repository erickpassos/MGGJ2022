Shader "Custom/DepthMask" {

	SubShader{
		
		Tags {"Queue" = "Geometry+10" }

		Cull Back
		ColorMask 0
		ZWrite On

		
		Pass {}
	}
}