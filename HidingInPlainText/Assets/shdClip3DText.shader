Shader "Custom/Clip 3D Text"
{
  Properties
  {
	_MainTex ("Font Texture", 2D) = "white" {} 
    _Color ("Text Color", Color) = (1.0, 1.0, 1.0, 1.0)
  	_ClipTop ("Clip Top", Float) = 0.0
  	_ClipBottom ("Clip Bottom", Float) = 0.0
  }
  SubShader
  {
    Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "IgnoreProjector" = "True" "PreviewType" = "Plane"}
	Lighting Off Cull Off ZWrite Off Fog { Mode Off }
	Blend SrcAlpha OneMinusSrcAlpha
 
    CGPROGRAM
    	#pragma surface surf Lambert
	 
	    struct Input
	    {
	    	float2 uv_MainTex;
	    	float3 worldPos;
	    };
	    
    	sampler2D _MainTex;
	   	fixed4 _Color;
	    float _ClipTop;
	    float _ClipBottom;
	 
	    void surf(Input IN, inout SurfaceOutput o)
	    {
	    	clip((IN.worldPos.y < _ClipBottom || IN.worldPos.y > _ClipTop) ? -1 : 1);
	    	
			o.Albedo = tex2D (_MainTex, IN.uv_MainTex).rgb * _Color.rgb;
			o.Emission = _Color.rgb; // * _Color.a;
			o.Alpha = tex2D (_MainTex, IN.uv_MainTex).a * _Color.a;
	    }
    ENDCG
  } 
  FallBack "Diffuse"
}