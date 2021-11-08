// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/Test_Shader"
{
	Properties
	{
		_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
		_Color("Color",Color) = (1,1,1,1)
		
	}
	SubShader
	{
		// Draw after all opaque geometry
		Tags {"Queue" = "Transparent+1" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
		Blend DstColor SrcColor
		// Grab the screen behind the object into _BackgroundTexture
		GrabPass
		{
		}

		// Render the object with the texture generated above, and invert the colors
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"
			struct VertexData {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				fixed4 color : COLOR;
			};
			struct Interpolators
			{
				float4 grabPos : TEXCOORD0;
				float2 uv : TEXCOORD1;
				float4 positon : SV_POSITION;
				fixed4 color : COLOR;
			};

			Interpolators vert(VertexData v) {
				Interpolators o;
				o.positon = UnityObjectToClipPos(v.vertex);
				o.grabPos = ComputeGrabScreenPos(o.positon);
				o.uv = v.uv;
				o.color = v.color;
				return o;

			}

			sampler2D _GrabTexture;
			sampler2D _MainTex;
			uniform float4 _Color;
			fixed4 _GrabTexture_TexelSize;
			void MaxMin(fixed4 color, inout float cmax, inout float cmin) {
				
				float list[] = { color.r,color.g,color.b };
				for (int i = 1; i < list.Length; i++) {
					if (cmax < list[i]) 
						cmax = list[i];
					if (cmin > list[i])
						cmin = list[i];
				}
				
			}
			half4 frag(Interpolators i) : COLOR
			{
				fixed4 col=fixed4(0,0,0,0);
				float4 pos = i.grabPos;
		
				float2 pixel =float2(0,0);
				int size = 1; int size_step = 0; bool check_around_white=false;
				int pixel_X = 0; int pixel_Y = 0; int step = 0; int rotationcount = 3;
				while (1) {
					step++;
					col = tex2Dproj(_GrabTexture, pos + float4(pixel_X * _GrabTexture_TexelSize.x, pixel_Y * _GrabTexture_TexelSize.y, 0, 0));
					float cmax, cmin, H, S, V, delta; cmin = col.r; cmax = col.r;
					MaxMin(col, cmax, cmin);
					delta = cmax - cmin;
					S = delta / (float)cmax;
					V = cmax;
					//각끝점 한번씩 도달후 사이즈 ++최종적으로 
					if (V > (float)200 / 255 && S < 0.2)
					{
						pixel.x = pixel_X;
						pixel.y = pixel_Y;
						check_around_white = true;
						break;
						//return lerp(col2, (1, 1, 1, 1), 0.8f*pow(0.9, max(pixel_X, pixel_Y)));
					}
					
					
					switch (rotationcount) {
						case 0:
							pixel_Y += 1;
							if (pixel_Y == size_step) 
								rotationcount = 1;
							break;
						
						case 1:
							pixel_X += 1;
							if (pixel_X == size_step) 
								rotationcount = 2;
							break;
						
						case 2:
							pixel_Y -= 1;
							if (pixel_Y == -size_step)
								rotationcount = 3;
							break;
						case 3: 
							if (pixel_X == -size_step){ 
								size_step += 1;
								pixel_X -= 1;
								rotationcount = 0;
							}
							else {
								pixel_X -= 1;
							}
							break;
						
				
					}
					
					if (step == pow(size*2+1, 2))
						break;
				}
			
				
				fixed4 final = tex2Dproj(_GrabTexture, pos + float4(pixel.x* _GrabTexture_TexelSize.x, pixel.y * _GrabTexture_TexelSize.y, 0, 0));
				fixed4 final2 = tex2Dproj(_GrabTexture, pos);
				
				
				float4 tex = tex2D(_MainTex, i.uv);
				if (check_around_white==true) {
					float4 c = float4(0.8, 0.8, 0.8, 1.);
					final = lerp(c, final, 0.5);
					return lerp(float4(0.5f, 0.5f, 0.5f, 0.5f),final,tex.a);
				}
				else {
					final.rgb = final.rgb* _Color.rgb;
					
					return lerp(float4(0.5f, 0.5f, 0.5f, 0.5f), final, tex.a* _Color.a);
					
				}
				
			}
			ENDCG
		}

	}
		
}
