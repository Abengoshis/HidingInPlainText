Shader "Saye/Glow"
{
	Properties
	{
		_Color ("Main Colour", Color) = (1.0, 1.0, 1.0, 1.0)
	}
	SubShader
	{
		Tags { "Queue"="Geometry+2" "RenderType"="Transparent" }
        
		Blend SrcAlpha One
		ZWrite Off
 
		CGPROGRAM		
		#pragma surface surf Unlit vertex:vert

		half4 LightingUnlit(SurfaceOutput s, half3 lightDir, half atten)
		{
			fixed4 c;
			c.rgb = s.Albedo;
			c.a = s.Alpha;
			return c;
		}

		struct Input
		{
			float uv;
	   	};

		half4 _Color;
		
		void vert (inout appdata_full v)
		{
		}

		void surf (Input IN, inout SurfaceOutput o)
		{
			half4 c = _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
        
	} 
	FallBack "Particles/Additive"
}