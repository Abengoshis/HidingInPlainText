Shader "Saye/OutlineThing"
{
	Properties
	{
		_Color ("Main Colour", Color) = (1.0, 1.0, 1.0, 1.0)
		_GlowColor("Glow Colour", Color) = (1.0, 1.0, 1.0, 1.0)
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "Queue"="Geometry+2" "RenderType"="Opaque" }
 
		CGPROGRAM		
		#pragma surface surf Lambert

		struct Input
		{
			float2 uv_MainTex;
	   	};

		half4 _Color;
		half4 _GlowColor;
		sampler2D _MainTex;


		void surf (Input IN, inout SurfaceOutput o)
		{
			half4 c;
			if (IN.uv_MainTex.x < 0.08 || IN.uv_MainTex.x > 0.92 || IN.uv_MainTex.y < 0.08 || IN.uv_MainTex.y > 0.92)
			{
				c = _GlowColor;
				o.Albedo = c.rgb * 100;
			}
			else
			{
				c = _Color;
				o.Albedo = c.rgb;
			}
		}
		ENDCG
	} 
}