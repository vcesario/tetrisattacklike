Shader "Custom/Ball5"
{
	Properties
	{
		_Color ("Tint", Color) = (1, 1, 1, 1)
		_Brightness("Brightness", Range(0.0, 1.0)) = 0.3
		_RampFactor("Ramp Factor", Range(0.0, 1.0)) = 0.25
		_Scale ("Scale", Range(0.0, 1.0)) = 0.8
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100	

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct v_in
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				half3 normal : NORMAL;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				half3 worldNormal : TEXCOORD1;
			};

			float4 _Color;
			float _Brightness;
			float _RampFactor;
			float _Scale;

			float Toon(float3 normal, float3 lightDir)
			{
				float d = max(0.0, dot(normalize(normal), normalize(lightDir)));
				return floor(d / _RampFactor);
			}

			v2f vert(v_in v)
			{
				v2f o;
				o.uv = v.uv;
				o.vertex = UnityObjectToClipPos(v.vertex * _Scale);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				return o;
			}

			fixed4 frag(v2f v) : SV_TARGET
			{
				fixed4 col = _Color;
				col *= Toon(v.worldNormal, _WorldSpaceLightPos0.xyz) + _Brightness;
				return col;
			}
			ENDCG
		}
	}
	
	Fallback "Unlit/Color"
}
