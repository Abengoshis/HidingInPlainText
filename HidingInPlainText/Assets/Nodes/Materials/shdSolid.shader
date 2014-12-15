Shader "Saye/Solid"
{
	Properties
	{
		_Color ("Main Colour", Color) = (1.0, 1.0, 1.0, 1.0)
		_GlossColor ("Gloss Colour", Color) = (1.0, 1.0, 1.0, 1.0)
		_Shininess ("Shininess", Range(0, 1.0)) = 1.0
	}
	SubShader
	{        
		Tags { "Queue"="Geometry+1" "RenderType"="Opaque" }
        Blend Off
        ZWrite On
        
		CGPROGRAM
		#pragma surface surf Specular
		
		struct SpecularSurfaceOutput
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half3 Gloss;
			half Specular;
			half Alpha;
		};
		
		half4 LightingSpecular (SpecularSurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
		{
	        half3 h = normalize (lightDir + viewDir);

	        half diff = max (0, dot (s.Normal, lightDir));

	        float nh = max (0, dot (s.Normal, h));
	        float spec = pow (nh, 32.0 * s.Specular);
	        half3 specColour = spec * s.Gloss;

	        half4 c;
	        c.rgb = (s.Albedo * _LightColor0.rgb * diff + _LightColor0.rgb * specColour) * (atten * 2);
	        c.a = s.Alpha;
	        return c;
    	}
		
		struct Input
		{
			float uv;
	   	};
	   	
		half _Shininess;
		half4 _GlossColor;
		half4 _Color;

		void surf (Input IN, inout SpecularSurfaceOutput o)
		{
			half4 c = _Color;
			o.Albedo = c.rgb;
			o.Gloss = _GlossColor.rgb;
			o.Specular = _Shininess;
		}
		ENDCG
	} 
	FallBack "Specular"
}