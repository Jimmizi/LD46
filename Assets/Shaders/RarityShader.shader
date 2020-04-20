Shader "Fx/RarityShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Tint ("Glow Tint", Color) = (1,1,1,1)
        _Intensity ("Intensity", Range (0, 1)) = 0.5
        
        [MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		_PixelCountU ("Pixel Count U", float) = 100
		_PixelCountV ("Pixel Count V", float) = 100
    }
    SubShader
    {
        Tags {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="False"
        }
        
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _PixelCountU;
			float _PixelCountV;
            float4 _Tint;
            float _Intensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                #ifdef PIXELSNAP_ON
				    o.vertex = UnityPixelSnap(o.vertex);
	            #endif
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }
            
            static const float PI = 3.141592653589793238462;
            static const float3 iMouse = float3(25.0, 25.85, 80.85);
            
 
            float areyouready(float3 p) {
                p = floor(p);
                p = frac(p * float3(283.343, 251.691, 634.127));
                p += dot(p, p + 23.453);
                return frac(p.x * p.y);
            }
            
            float isaid_areyouready(float2 p2) {
                float3 p = float3(p2, 0.0);
                float2 off = float2(1.0, 0.0);
                return lerp(
                    lerp(
                        lerp(areyouready(p), areyouready(p + off.xyy), frac(p.x)),
                        lerp(areyouready(p + off.yxy), areyouready(p + off.xxy), frac(p.x)), frac(p.y)),
                        lerp(
                            lerp(areyouready(p + off.yyx), areyouready(p + off.xyx), frac(p.x)),
                            lerp(areyouready(p + off.yxx), areyouready(p + off.xxx), frac(p.x)), frac(p.y)),
                        frac(p.z));
                
            }
            
            float4 ramp(float t) {
                float4 r = (t <= 0.4) ?
                    float4(_Tint.r - t * 0.2, _Tint.g, _Tint.b, max(0.0, 1.0 - t * 4.2)) :
                    float4(_Tint.r * 0.7 * (1.0 - t), _Tint.g * 0.7, _Tint.b * 0.7, max(0.0, 1.4 - t * 1.4));
                r /= t;
                r.a *= _Tint.a;
                return r;
            }
            
            float2 polar_angle(float2 uv, float shift) {
                uv = float2(0.5, 0.5) - uv;                   
                return 1.0 - frac(atan2(uv.y, uv.x) / 6.28 + 0.25) + shift;
            }
            
            float aura(float2 n) {
                return isaid_areyouready(n) + isaid_areyouready(n * 2.1) * .6 + isaid_areyouready(n * 5.4) * .42;
            }
            
            float shade(float2 uv, float t) {
                uv.x += uv.y < 0.5 ? 23.0 + t * 0.035 : -11.0 + t * 0.03;    
                uv.y = abs(uv.y - 0.5);
                uv.x *= 35.0;
                
                float q = aura(uv - t * 0.013) / 2.0;
                float2 r = float2(aura(uv + q / 2.0 + t - uv.x - uv.y), aura(uv + q - t));
                
                return pow((r.y + r.y) * max(0.0, uv.y) + 0.1, 4.0);
            }
            
            float4 color(float grad) {                
                float m2 = lerp(4.0, 0.8, _Intensity);
                grad = sqrt( grad);
                float4 color = float4(1.0 / (pow(float4(0.5, 0.0, 0.1, 1.0) + 2.61, float4(2.0, 2.0, 2.0, 1.0))));
                float4 color2 = color;
                color = ramp(grad);               
                color /= (m2 + max(float4(0.0, 0.0, 0.0, 0.0), color));
                return color;
            
            }
            
            float sd2_circle(float2 p, float r) {
                return length(p) - r;
            }
            
            float sd2_box(float2 p, float2 b) {
                float2 d = abs(p) - b;
                return length(max(d, float2(0.0, 0.0))) + min(max(d.x, d.y), 0.0);
            }
            
            float heat_map(float2 p, float r, float spread) {
            
                float stfs = 0.45;
                float dist = sd2_circle(p, r);
                dist = sd2_box(p, float2(0.2, 0.2)) - 0.02;

                dist = dist * 0.5 + 0.5;
                dist = (dist < stfs) ? 0.0 : dist;
                dist = (dist > (stfs + spread)) ? (dist - (stfs + spread)) + 1.0 : dist;
                dist = (dist >= stfs && dist <= (stfs + spread)) ? (dist - stfs) * (1.0 / spread) : dist;
                
                return dist;
            }
            
   
            fixed4 frag (v2f i) : SV_Target {
 
                float m1 = 2.0; 
                
                float t = _Time.y;
                float pixelWidth = 1.0f / _PixelCountU;
				float pixelHeight = 1.0f / _PixelCountV;
				
				half2 uv_s = half2(round(i.uv.x / pixelWidth) * pixelWidth,
				                 round(i.uv.y / pixelHeight) * pixelHeight);		
				
				
                float ff = 1.0 - uv_s.y;
                
                float dist = heat_map(uv_s - 0.5, 0.3, 0.10);
                float2 uv_a = float2(1.0, dist);
                dist = heat_map(uv_s - 0.5, 0.3, 0.09);
                float2 uv_b = float2(1.0, dist);
                
                uv_a.x = polar_angle(uv_s, 1.3);
                uv_b.x = polar_angle(float2(uv_s.x, 1.0 - uv_s.y), 1.9);
            
                float4 c1 = color(shade(uv_a, t)) * ff;
                float4 c2 = color(shade(uv_b, t)) * (1.0 - ff);
                
                float4 col = c1 + c2;

                
                // Ramp colors
                float color_r = 0.15;
                col = float4(color_r * floor(col.rgb / color_r), col.a);
                col.a *= (1.0 - abs(dist - 0.4)); //1.0 - min(1.0, grad);
                col.a *= 0.98;
                
                
                return col;
            }
            ENDCG
        }
    }
}
