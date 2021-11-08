// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/gloss"
{
	Properties
	{
		_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
		[HDR]_Color("Color",Color) = (1,1,1,1)
		_NormalMap("Normals", 2D) = "bump" {}
		_BumpScale("Bump Scale", Float) = 1
		_SurfNormalMap("Normals", 2D) = "bump" {}

		
		_alphaChennal("_alpha Power",Range(0,1)) = 1
		_Smoothness("Smoothness", Range(0, 1)) = 0.5
		[HDR]_Color2("Color",Color) = (1,1,1,1)

		_SpecValue("SpecValue", Range(0, 80)) = 2
		_AlphaM_ap(" Alpha Map", 2D) = "white" {}
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
				Tags {"LightMode" = "ForwardBase" }
				CGPROGRAM

				#pragma vertex VertexProgram
				#pragma fragment fragProgram 
				#pragma fragmentoption ARB_precision_hint_fastest
				#include "UnityStandardBRDF.cginc"
				#include "UnityStandardUtils.cginc"	
				sampler2D _MainTex;
				sampler2D _NormalMap,_SurfNormalMap;
				sampler2D _AlphaMap;

				float4 _MainTex_ST;
				
				float _alphaChennal;
				float _BumpScale;

				float _Smoothness;
				float4 _Color;
				float4 _Color2;

				float _GraValue;
				float _UsingGra;
				float _SpecValue;
				struct VertexData {
					float4 position : POSITION;
					float4 color : COLOR;//정점 색
					float3 normal : NORMAL;
					float4 tangent : TANGENT;
					float2 uv : TEXCOORD0;
				};

				struct Interpolators {
					float4 position : SV_POSITION;
					float4 color:COLOR;
					float2 uv : TEXCOORD0;
					float3 normal : TEXCOORD1;
					float4 tangent : TEXCOORD2;
					float3 worldPos : TEXCOORD3;
					#if defined(VERTEXLIGHT_ON)
						float3 vertexLightColor : TEXCOORD4;
					#endif
				};

				Interpolators VertexProgram(VertexData v) {
					Interpolators i;
					i.uv = v.uv;//TRANSFORM_TEX(v.uv, _MainTex);
					i.color = v.color;
					i.position = UnityObjectToClipPos(v.position);
					i.normal = UnityObjectToWorldNormal(v.normal); 
					i.tangent = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);
					i.worldPos = mul(unity_ObjectToWorld, v.position);
					
					return i;
				}
				void SetNormal(inout Interpolators i) {
					float3 mainNormal = UnpackNormal(tex2D(_NormalMap, i.uv));
					float3 surfNormal = UnpackNormal(tex2D(_SurfNormalMap, i.uv));
					mainNormal = normalize(lerp(mainNormal,surfNormal,0.40)); //(mainNormal + (-surfNormal) / 3);
					float3 tangentSpaceNormal; 
					
					tangentSpaceNormal = mainNormal;
					tangentSpaceNormal = tangentSpaceNormal.xzy;

					float3 binormal = cross(i.normal, i.tangent.xyz) * i.tangent.w;
					i.normal = normalize(
						tangentSpaceNormal.x * i.tangent +
						tangentSpaceNormal.y * i.normal +
						tangentSpaceNormal.z * binormal);
				}
				float4 fragProgram(Interpolators i) : SV_TARGET
				{
					SetNormal(i);
					float3 lightDir = _WorldSpaceLightPos0.xyz;
					float3 lightColor = _LightColor0.rgb;
					float3 albedo = tex2D(_MainTex, i.uv.xy).rgb * _Color.rgb;
					
					/*float3 diffuse = albedo*lightColor * max(0,dot(lightDir, i.normal));*/
					//diffuse 난반사
					float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
					float3 halfVector = normalize(lightDir + viewDir);
					
					float3 specular = saturate(lightColor* pow(DotClamped(halfVector,i.normal), _Smoothness * 100)*_SpecValue);
					
					//specular 정반사
					
					float4 tex = tex2D(_MainTex, i.uv);

					float4 final;
					final.rgb = (i.color.rgb*_Color * 1)+specular*1.6;
				
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
					
					
					//float4 r;
					//r.rgb = diffuse + specular;
					//r.rgb = _Color.rgb + specular;
					//r.a = 1;
					return lerp(float4(0.5f, 0.5f, 0.5f, 1.0f), final, final.a);
				}

				ENDCG

			}


		}
}
