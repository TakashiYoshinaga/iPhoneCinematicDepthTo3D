//Copyright 2017-2021 Looking Glass Factory Inc.
//All rights reserved.
//Unauthorized copying or distribution of this file, and the source code contained herein, is strictly prohibited.

Shader "Holoplay/Lightfield" {

	Properties {
		_MainTex ("Texture", 2D) = "white" {}
	}

	SubShader {
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			// default vars
			sampler2D _MainTex;
			float4 _MainTex_ST;
			// Holoplay vars
			float pitch;
			float slope;
			float center;
			float subpixelSize;
			float tx;
			float4 tile;
			float4 viewPortion;
			float4 aspect;
			float verticalOffset; // just a dumb fix for macos in 2019.3
			
			// pass thru vert shader
			v2f vert (appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target {
				// just a dumb fix for macos in 2019.3
				i.uv.y += verticalOffset;

				// first handle aspect
				// note: recreated this using step functions because my mac didn't like the conditionals
				// if ((aspect.x > aspect.y && aspect.z < 0.5) || (aspect.x < aspect.y && aspect.z > 0.5))
				// 	viewUV.x *= aspect.x / aspect.y;
				// else 
				// 	viewUV.y *= aspect.y / aspect.x;
				float2 viewUV = i.uv;
				viewUV -= 0.5;
				float modx = saturate(
					step(aspect.y, aspect.x) * step(aspect.z, 0.5) +
					step(aspect.x, aspect.y) * step(0.5, aspect.z));
				viewUV.x = modx * viewUV.x * aspect.x / aspect.y +
						   (1.0 - modx) * viewUV.x;
				viewUV.y = modx * viewUV.y +
						   (1.0 - modx) * viewUV.y * aspect.y / aspect.x;
				viewUV += 0.5;
				clip(viewUV);
				clip(-viewUV + 1.0);

				// then sample quilt
				fixed4 col = fixed4(0,0,0,1);
				[unroll]
				for (int subpixel = 0; subpixel < 3; subpixel++) {
					// determine view for this subpixel based on pitch, slope, center
					float viewLerp = i.uv.x + subpixel * subpixelSize;
					viewLerp += i.uv.y * slope;
					viewLerp *= pitch;
					viewLerp -= center;
					viewLerp += 0.5 / tile.z;
					// make sure it's positive and between 0-1
					viewLerp = 1.0 - fmod(viewLerp + ceil(abs(viewLerp)), 1.0);
					viewLerp = clamp(viewLerp, 0.00001, 0.99999);	// another dumb bugfix

					// translate to quilt coordinates
					float view1 = floor(viewLerp * tile.z); // multiply by total views
					view1 = view1 == tile.z ? 0.0 : view1;
					float view2 = view1 + 1;
					view2 = view2 == tile.z ? 0.0 : view2;
					float viewFilter = viewLerp * tile.z - view1;
					float tx = tile.x - 0.00001;	// just an incredibly dumb bugfix
					float2 quiltCoords1 = float2(
						(fmod(view1, tx) + viewUV.x) / tx,
						(floor(view1 / tx) + viewUV.y) / tile.y
					) * viewPortion.xy;
					float2 quiltCoords2 = float2(
						(fmod(view2, tx) + viewUV.x) / tx,
						(floor(view2 / tx) + viewUV.y) / tile.y
					) * viewPortion.xy;
					float col1 = tex2D(_MainTex, quiltCoords1)[subpixel];
					float col2 = tex2D(_MainTex, quiltCoords2)[subpixel];
					col[subpixel] = lerp(col1, col2, viewFilter);
				}

				return col;
			}
			ENDCG
		}
	}
}