// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/TerrainOpt" {
    Properties
    {
        // we have removed support for texture tiling/offset,
        // so make them not be displayed in material inspector
        [NoScaleOffset] _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            // use "vert" function as the vertex shader
            #pragma vertex vert
            // use "frag" function as the pixel (fragment) shader
            #pragma fragment frag

			float3 mod289(float3 x)
			{
				return x - floor(x / 289.0) * 289.0;
			}

			float2 mod289(float2 x)
			{
				return x - floor(x / 289.0) * 289.0;
			}

			float3 permute(float3 x)
			{
				return mod289((x * 34.0 + 1.0) * x);
			}

			float3 taylorInvSqrt(float3 r)
			{
				return 1.79284291400159 - 0.85373472095314 * r;
			}

			float snoise(float2 v)
			{
				const float4 C = float4( 0.211324865405187,  // (3.0-sqrt(3.0))/6.0
										 0.366025403784439,  // 0.5*(sqrt(3.0)-1.0)
										-0.577350269189626,  // -1.0 + 2.0 * C.x
										 0.024390243902439); // 1.0 / 41.0
				// First corner
				float2 i  = floor(v + dot(v, C.yy));
				float2 x0 = v -   i + dot(i, C.xx);

				// Other corners
				float2 i1;
				i1.x = step(x0.y, x0.x);
				i1.y = 1.0 - i1.x;

				// x1 = x0 - i1  + 1.0 * C.xx;
				// x2 = x0 - 1.0 + 2.0 * C.xx;
				float2 x1 = x0 + C.xx - i1;
				float2 x2 = x0 + C.zz;

				// Permutations
				i = mod289(i); // Avoid truncation effects in permutation
				float3 p =
				  permute(permute(i.y + float3(0.0, i1.y, 1.0))
								+ i.x + float3(0.0, i1.x, 1.0));

				float3 m = max(0.5 - float3(dot(x0, x0), dot(x1, x1), dot(x2, x2)), 0.0);
				m = m * m;
				m = m * m;

				// Gradients: 41 points uniformly over a line, mapped onto a diamond.
				// The ring size 17*17 = 289 is close to a multiple of 41 (41*7 = 287)
				float3 x = 2.0 * frac(p * C.www) - 1.0;
				float3 h = abs(x) - 0.5;
				float3 ox = floor(x + 0.5);
				float3 a0 = x - ox;

				// Normalise gradients implicitly by scaling m
				m *= taylorInvSqrt(a0 * a0 + h * h);

				// Compute final noise value at P
				float3 g;
				g.x = a0.x * x0.x + h.x * x0.y;
				g.y = a0.y * x1.x + h.y * x1.y;
				g.z = a0.z * x2.x + h.z * x2.y;
				return 130.0 * dot(m, g);
			}

			float fractalNoise(float2 v)
			{
			  float total = 0;
			  float frequency = 1;
			  float amplitude = 1;
			  float maxValue = 0;
			  for (int i = 0; i < 8; i++)
				{
					total += snoise(v*frequency) * amplitude;
					maxValue += amplitude;
					amplitude *= 0.5;
					frequency *= 2;
				}

				return total / maxValue;			
			}

			float noise01(float2 v) 
			{
				return (fractalNoise(v) + 1)/2;
			}

			float2 offset;
			float scale;

            // vertex shader inputs
            struct appdata
            {
                float4 vertex : POSITION; // vertex position
                float2 uv : TEXCOORD0; // texture coordinate
            };

            // vertex shader outputs ("vertex to fragment")
            struct v2f
            {
                float2 uv : TEXCOORD0; // texture coordinate
                float4 vertex : SV_POSITION; // clip space position
            };

            // vertex shader
            v2f vert (appdata v)
            {
                v2f o;
                // transform position to clip space
                // (multiply with model*view*projection matrix)
				float2 uv = ((v.uv) * 2 - 1)*scale + 0.5*offset;
				v.vertex += float4(0.0, 100 * noise01(uv), 0.0, 0.0);
                o.vertex = UnityObjectToClipPos(v.vertex);
                // just pass the texture coordinate
                o.uv = uv;
                return o;
            }
            
            // texture we will sample
            sampler2D _MainTex;

            // pixel shader; returns low precision ("fixed4" type)
            // color ("SV_Target" semantic)
            fixed4 frag (v2f i) : SV_Target
            {
                // sample texture and return it
                fixed4 col = noise01(i.uv) * fixed4(1, 1, 1, 1);
                return col;
            }
            ENDCG
        }
    }
}