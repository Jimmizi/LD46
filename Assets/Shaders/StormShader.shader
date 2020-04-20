Shader "Fx/StormShader" {

    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Turnitup ("Storm Factor", Range (0, 1)) = 0.75
        
        [MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		_PixelCountU ("Pixel Count U", float) = 100
		_PixelCountV ("Pixel Count V", float) = 100
    }
    
    SubShader {
    
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

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Turnitup;
            float _PixelCountU;
			float _PixelCountV;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                #ifdef PIXELSNAP_ON
				    o.vertex = UnityPixelSnap(o.vertex);
	            #endif
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            float areyouready(float3 p) {
                p = floor(p);
                p = frac(p * float3(283.343, 251.691, 634.127));
                p += dot(p, p + 23.453);
                return frac(p.x * p.y);
            }
            
            float isaid_areyouready(float3 p) {
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
            
            float letherrip(float3 p, float tm) {
                p *= 4.0;
                float3 dp = float3(p.xy, p.z + tm * 0.25);
                float inc = 0.75;
                float div = 1.75;
                float3 octs = dp * 2.13;
                float n = isaid_areyouready(dp);
                
                for(float i = 0.0; i < 5.0; i++) {
                    float ns = isaid_areyouready(octs);
                    n += inc * ns;
                    
                    octs *= 2.0 + (float3(ns, isaid_areyouready(octs + float3(n, 0.0, 0.1)), 0.0));
                    inc *= 0.5 * n;
                    div += inc;
                }
                
                float v = n / div;
                v *= 1.0 - max(0.0, 1.2 - length(float3(0.5, 0.0, 6.0) - p));
                return v;
            }


            fixed4 frag (v2f input) : SV_Target {
  
                
                float pixelWidth = 1.0f / _PixelCountU;
				float pixelHeight = 1.0f / _PixelCountV;
				
				half2 uv = half2(round(input.uv.x / pixelWidth) * pixelWidth,
				                 round(input.uv.y / pixelHeight) * pixelHeight);
                
                float2 storm_sink = float2(0.5, 0.5);
                float time = floor(_Time.y * 14.0) / 14.0 * 0.8;
                time = fmod(time, 10.0);

                float2 uv_m = uv * (1.0 + 0.2 * length(uv));
                float uvlen = 1.0 - length(uv_m);
                float tt = 0.5 * time + (0.3 - 0.3 * uvlen * uvlen);
                float2 rot = float2(sin(tt), cos(tt));
                uv_m = float2(uv_m.x * rot.x + uv_m.y * rot.y, uv_m.x * rot.y - uv_m.y * rot.x);
                storm_sink = float2(storm_sink.x * rot.x + storm_sink.y * rot.y, storm_sink.x * rot.y - storm_sink.y * rot.x);
                float3 ro = float3(storm_sink, -1.0);
                float3 rd = normalize(float3(uv_m, 5.0) - ro);
    
                float3 col = float3(0.0, 0.0, 0.0);
                rd.z += tt * 0.01;
                float nv = letherrip(rd, time);
                
                for (float i = 0.0; i < 1.0; i += 0.2) {
                    nv *= 0.5;
                    nv = letherrip(float3(rd.xy, rd.z + i), time);
    	            col += (1.5 - i) * float3(nv, nv * nv * (3.0 - 2.0 * nv), nv * nv);
                 }
                 
                 col /= 5.0;

            
                float4 rcl = float4(col, 1.0);
                
                if (rcl.r > 0.2) {
                  float cl = length(uv - 0.5 + (letherrip(float3(uv * 30.0 - 20.0 + float2(0.0, time * 20.0), time), time) - 0.5) * 0.5);
                  cl = floor(cl * 100.0) / 100.0;
                  rcl.a *= min(1.0, lerp(10.0, 1000.0, _Turnitup) * cl * cl); 
                  rcl.a = (cl < lerp(0.2, 0.0, _Turnitup)) ? 0.0 : rcl.a;
                
                }
                
                
                float color_r = 0.05;
                // Ramp colors
                float brightness = sqrt(
                    0.299 * (rcl.r * rcl.r) +
                    0.587 * (rcl.g * rcl.g) +
                    0.114 * (rcl.b * rcl.b) );
                    brightness = rcl.r;
                float target_c = color_r * floor(brightness / color_r);
                rcl.g *= 0.7;
                rcl += 0.2;
                rcl = float4(color_r * floor(rcl.rgb / color_r), rcl.a);
    
                float3 altc = float3(0.455, 0.369, 0.329);
                
                
                uv *=  1.0 - uv.yx;   
    float vig = uv.x*uv.y * lerp(120.0, 0.0, _Turnitup); //min(_PixelCountU, _PixelCountV) * 0.5; // multiply with sth for intensity
    vig = pow(vig, 0.8); // change pow for modifying the extend of the  vignette
    vig += (isaid_areyouready(float3(uv_m * 90.0 - 20.0 + float2(0.0, time * 10.0), time)) - 0.5) * 0.255;


                
                rcl = float4(lerp(altc, rcl.rgb, rcl.r), lerp(rcl.a * rcl.r * 0.8 * (1.0 - vig), 1.0 * (1.0 - vig), _Turnitup));
                //rcl =float4(vig, 0.0, 0.0, 1.0);
    
                return rcl;
            }
            ENDCG
        }
    }
}
