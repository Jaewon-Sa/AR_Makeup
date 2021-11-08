// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Imagefilter"
{
	Properties
	{
		_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
		[HDR]_Color("Color",Color) = (1,1,1,1)
		_alphaChennal("alpha Power",Range(0,1)) = 1
		_AlphaMap("Alpha Map", 2D) = "white" {}
		[Toggle] _UsingGra("UsingGra", Float) = 1
		_GraValue("GraValue",Range(0,2)) = 1
		
	}

	SubShader
	{
		Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
		ZWrite Off Lighting On Cull Off Fog { Mode Off } Blend DstColor SrcColor
		LOD 110

		Pass
		{
			CGPROGRAM
			#pragma vertex vert_vct
			#pragma fragment fragProgram 
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _AlphaMap;
			float4 _MainTex_ST;
			float _alphaChennal;
			float _GraValue;
			float _UsingGra;
			uniform float4 _Color;
			struct VertexData
			{
				float4 vertex : POSITION;//정점좌표
				float4 color : COLOR;//정점 색
				float2 uv : TEXCOORD0;//uv
			};

			struct Interpolators
			{
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
				half2 uv : TEXCOORD0;//uv
			};

			Interpolators vert_vct(VertexData v)
			{
				Interpolators o;
				o.vertex = UnityObjectToClipPos(v.vertex);//메쉬좌표를 월드좌표가아닌
				o.color = v.color;
				o.uv = v.uv;
				return o;
			}

			float4 fragProgram(Interpolators i) : COLOR
			{
				float4 tex = tex2D(_MainTex, i.uv);
				float4 final;
				if (_UsingGra) {
					float alpha = tex2D(_AlphaMap, i.uv).r;

					if (alpha >= 1)
						alpha *= _GraValue * 1;
					else if (alpha > 0.9)
						alpha *= _GraValue * 0.8;
					else if (alpha > 0.8)
						alpha *= _GraValue * 0.5;
					else if (alpha > 0.4)
						alpha *= _GraValue * 0.3;
					else if (alpha < 0.4)
						alpha *= _GraValue * 0.2;
					else if (alpha < 0.2)
						alpha = 0;
					if (alpha > 1)
						alpha = 1;
					final.a = i.color.a * tex.a*_alphaChennal*alpha;
				}
				else {
					final.a = i.color.a * tex.a*_alphaChennal;
				}

				
				final.rgb = (i.color.rgb*_Color);
				
				
				return lerp(float4(0.5f,0.5f,0.5f,0.5f), final, final.a);

			}

			ENDCG

		}


	}
}
