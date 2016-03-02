// ----------------------------------------------------------------------------
//  FISH - HDBlur Effect - All rights reserved - Yves Wang @ FISH Team 2016
// ----------------------------------------------------------------------------

Shader "FISH/HDBlur/Downsample" {

    Properties {
        _MainTex ("MainTex", 2D) = "white" {}
        _Strength ("Strength", Float) = 0.25
    }
    
    CGINCLUDE
    #include "UnityCG.cginc"
    
    struct v2f {
        half4 pos : POSITION;
        half4 uv[4] : TEXCOORD0;
    };
    
    float4 _MainTex_TexelSize;

    // Luma function with Rec.709 HDTV Standard
    half Luma(half3 c)
    {
        return dot(c, half3(0.2126, 0.7152, 0.0722));
    }
    
    v2f vert (appdata_img v)
    {
        v2f o;
        o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
        float4 uv;
        uv.xy = MultiplyUV (UNITY_MATRIX_TEXTURE0, v.texcoord);
        uv.zw = 0;
        float offX = _MainTex_TexelSize.x;
        float offY = _MainTex_TexelSize.y;
        
        // Direct3D9 needs some texel offset!
        #ifdef UNITY_HALF_TEXEL_OFFSET
        uv.x += offX * 2.0f;
        uv.y += offY * 2.0f;
        #endif
        o.uv[0] = uv + float4(-offX,-offY,0,1);
        o.uv[1] = uv + float4( offX,-offY,0,1);
        o.uv[2] = uv + float4( offX, offY,0,1);
        o.uv[3] = uv + float4(-offX, offY,0,1);
        return o;
    }

    ENDCG
    
    
    Category {
        ZTest Always Cull Off ZWrite Off Fog { Mode Off }
        ColorMask RGB
        
        Subshader { 
            Pass {

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma fragmentoption ARB_precision_hint_fastest
                
                sampler2D _MainTex;
                uniform fixed _Strength;
                
                fixed4 frag( v2f i ) : COLOR
                {
                    

                    fixed4 c;
                    c  = tex2D( _MainTex, i.uv[0].xy );
                    c += tex2D( _MainTex, i.uv[1].xy );
                    c += tex2D( _MainTex, i.uv[2].xy );
                    c += tex2D( _MainTex, i.uv[3].xy );
                    c *= _Strength;
                    return c;


                    /*
                    fixed4 c0 = tex2D( _MainTex, i.uv[0].xy );
                    fixed4 c1 = tex2D( _MainTex, i.uv[1].xy );
                    fixed4 c2 = tex2D( _MainTex, i.uv[2].xy );
                    fixed4 c3 = tex2D( _MainTex, i.uv[3].xy );

                    // Karis's luma weighted average
			        half s1w = 1 / (Luma(c0) + 1);
			        half s2w = 1 / (Luma(c1) + 1);
			        half s3w = 1 / (Luma(c2) + 1);
			        half s4w = 1 / (Luma(c3) + 1);
			        half one_div_wsum = 1.0 / (s1w + s2w + s3w + s4w);

			        return (c0 * s1w + c1 * s2w + c2 * s3w + c3 * s4w) * one_div_wsum;
                    */
                }
                ENDCG
    
            }
            Pass {

                CGPROGRAM
                #pragma vertex vertSimple
                #pragma fragment fragSimple
                #pragma fragmentoption ARB_precision_hint_fastest

                struct v2fSimple {
                    half4 pos : POSITION;
                    half2 uv : TEXCOORD0;
                };
            
                v2fSimple vertSimple( appdata_img v )
                {
                    v2fSimple o;
                    o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                    o.uv = v.texcoord.xy;
                    return o;
                }

                sampler2D _MainTex;
                uniform fixed _Strength;

                fixed4 fragSimple( v2fSimple i ) : COLOR
                {
                    fixed4 c;
                    c  = tex2D( _MainTex, i.uv );
                    c *= _Strength;
                    return c;
                }
                ENDCG

            }
        }
    }
    
    Fallback off

}