Shader "Custom/Ball5"
{
	Properties
	{
		//_MainTex ("Texture", 2D) = "white" {}
		_Color ("Ting", Color) = (1, 1, 1, 1)
		_OutlineColor ("Outline Color", Color) = (1, 1, 1, 1)
		_Scale ("Scale", Range(0.0, 1.0)) = 1.0
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
			// make fog work
			//#pragma multi_compile_fog

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				//UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			/*sampler2D _MainTex;
			float4 _MainTex_ST;*/
			float4 _OutlineColor;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				/*o.uv = TRANSFORM_TEX(v.uv, _MainTex);*/
				//UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				// sample the texture
				//fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 col = _OutlineColor;
				// apply fog
				//UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			struct v_in
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			float4 _Color;
			float _Scale;

			v2f vert(v_in v)
			{
				v2f o;
				o.uv = v.uv;
				o.vertex = UnityObjectToClipPos(v.vertex + _Scale);
				return o;
			}

			fixed4 frag(v2f v) : SV_TARGET
			{
				fixed4 col = _Color;
				return col;
			}
			ENDCG
		}
	}
	
	Fallback "Unlit/Color"
}
