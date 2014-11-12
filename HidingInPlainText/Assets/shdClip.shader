Shader "Custom/Clip"
{
  Properties
  {
	_Color ("Main Color", Color) = (1.0, 1.0, 1.0, 1.0)
  	_ClipTop ("Clip Top", Float) = 0.0
  	_ClipBottom ("Clip Bottom", Float) = 0.0
  }
  SubShader
  {
    Tags { "RenderType" = "Opaque" }
    Cull Off
    LOD 200
 
    CGPROGRAM
    	#pragma surface surf Lambert
	 
	    fixed4 _Color;
	    float _ClipTop;
	    float _ClipBottom;
	 
	    struct Input
	    {
	    	float2 uv_MainTex;
	    	float3 worldPos;
	    };
	 
	    void surf(Input IN, inout SurfaceOutput o)
	    {
	    	clip((IN.worldPos.y < _ClipBottom || IN.worldPos.y > _ClipTop) ? -1 : 1);
	    	
			o.Albedo = _Color.rgb;
			o.Emission = _Color.rgb; // * _Color.a;
			o.Alpha = _Color.a;
	    }
    ENDCG
  } 
  FallBack "Diffuse"
}