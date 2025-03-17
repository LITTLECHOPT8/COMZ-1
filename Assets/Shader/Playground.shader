Shader "Zweronz/Playground" {
    Properties {
        _TintColor ("Main Color", Color) = (0.8,0.8,0.8,1)
        _MainTex ("Base (RGB)", 2D) = "white" { }
        _GridTex ("Base (RGB)", 2D) = "white" { }
        _GridSpeed ("Grid Move Speed", Float) = 4
        _FlickerSpeed ("Flicker Cooldown", Float) = 2
        _FlickerDuration ("Flicker Duration", Float) = 0.02
    }
	
    SubShader {
		/*
		Material {

			Ambient [_TintColor]

		}
		

		ColorMaterial [_TintColor]
		*/
        Pass {



            Lighting Off
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            float3 mod289(float3 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
            float2 mod289(float2 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
            float3 permute(float3 x) { return mod289(((x*34.0) + 1.0)*x); }

            float snoise(float2 v)
            {
            	// Precompute values for skewed triangular grid
            	const float4 C = float4(0.211324865405187,  // (3.0-sqrt(3.0))/6.0
            		0.366025403784439,	// 0.5*(sqrt(3.0)-1.0)
            		-0.577350269189626, // -1.0 + 2.0 * C.x
            		0.024390243902439	// 1.0 / 41.0
            		);


            	// First corner (x0)
            	float2 i = floor(v + dot(v, C.yy));
            	float2 x0 = v - i + dot(i, C.xx);

            	// Other two corners (x1, x2)
            	float2 i1;
            	i1 = (x0.x > x0.y) ? float2(1.0, 0.0) : float2(0.0, 1.0);
            	float2 x1 = x0.xy + C.xx - i1;
            	float2 x2 = x0.xy + C.zz;

            	// Do some permutations to avoid
            	// truncation effects in permutation
            	i = mod289(i);
            	float3 p = permute(
            		permute(i.y + float3(0.0, i1.y, 1.0))
            		+ i.x + float3(0.0, i1.x, 1.0));

            	float3 m = max(0.5 - float3(
            		dot(x0, x0),
            		dot(x1, x1),
            		dot(x2, x2)
            		), 0.0);

            	m = m*m;
            	m = m*m;

            	// Gradients: 
            	//  41 pts uniformly over a line, mapped onto a diamond
            	//  The ring size 17*17 = 289 is close to a multiple of 41 (41*7 = 287)

            	float3 x = 2.0 * frac(p * C.www) - 1.0;
            	float3 h = abs(x) - 0.5;
            	float3 ox = floor(x + 0.5);
            	float3 a0 = x - ox;

            	// Normalise gradients implicitly by scaling m
            	// Approximation of: m *= inversesqrt(a0*a0 + h*h);
            	m *= 1.79284291400159 - 0.85373472095314 *(a0*a0 + h*h);

            	// Compute final noise value at P
            	float3 g;
            	g.x = a0.x  * x0.x + h.x  * x0.y;
            	g.yz = a0.yz * float2(x1.x, x2.x) + h.yz * float2(x1.y, x2.y);
            	return 130.0 * dot(m, g);
            }

            sampler2D _MainTex, _GridTex;
            float4 _TintColor, _GridTex_ST;
            float _GridSpeed, _FlickerDuration, _FlickerSpeed;

            struct v2f {
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float4 vertex : POSITION;
            };

            struct appdata_t {
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float4 vertex : POSITION;
            };


            v2f vert(appdata_t v) {
                v2f o;
                o.texcoord0 = v.texcoord0;
                o.texcoord1 = v.texcoord1;
                float mult = 0.04;
                if (frac(_Time.y / _FlickerSpeed) <= _FlickerDuration) {
                    mult = 0.2;
                }
                v.vertex.xyz += mult * snoise(v.vertex.xyz + sin(_Time.y * 4) * 6);
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            float4 frag(v2f i) : SV_TARGET {
                float4 color = (tex2D(_MainTex, i.texcoord0));
                float mult = 1;
                if (frac(_Time.y / _FlickerSpeed) <= _FlickerDuration) {
                    mult = 5;
                }
                return (float4((((color.x * 0.1) + (color.y * 0.6)) + (color.z * 0.3)).xxx, color.w) + (tex2D(_GridTex, (i.texcoord1 * _GridTex_ST.xy + (_Time * _GridSpeed)) * 7))) * (_TintColor * mult);
            }

            ENDCG

        }
    }
} 