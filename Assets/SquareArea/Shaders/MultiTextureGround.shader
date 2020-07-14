Shader "Custom/MultiTextureGround"
{
	Properties
	{
		[NoScaleOffset] _MainTex ("Main Texture", 2D) = "white" {}
		[NoScaleOffset] _SubTex ("Sub Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				fixed4 color : COLOR;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				fixed4 color : COLOR;
			};

			sampler2D _MainTex;
			sampler2D _SubTex;

			v2f vert (appdata v)
			{
				v2f o = (v2f)0;
				o.vertex = UnityObjectToClipPos( v.vertex);
				o.uv = v.uv;
				o.color = v.color;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 c1 = tex2D(_MainTex, i.uv);
				fixed4 c2 = tex2D(_SubTex, i.uv);
				c1 = lerp( c1, c2, i.color.a) * i.color;
				return c1;
			}
			ENDCG
		}
	}
	FallBack OFF
}
