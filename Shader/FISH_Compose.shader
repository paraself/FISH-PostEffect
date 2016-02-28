
Shader "FISH/Compose_MultiplyColor_Glow" {
	Properties {
	    _MainTex ("", 2D) = "white" {}
	    _MultiplyColor ("",Color) = (0,0,0,0)
	    _GlowRT ("", 2D) = "white" {}
	    _GlowBlurRT ("", 2D) = "white" {}

	}
       
    CGINCLUDE
   
    #include "UnityCG.cginc"
   	sampler2D _MainTex;
    uniform fixed4 _MultiplyColor;
    uniform sampler2D _GlowRT;
    uniform sampler2D _GlowBlurRT;

	fixed4 frag_UnityBlur_Glow(v2f_img pixelData) : COLOR
    {
    	fixed4 sourceColor = tex2D(_MainTex, pixelData.uv);
       	fixed4 glowBlurColor = tex2D(_GlowBlurRT, pixelData.uv);
       	fixed4 glowPlantColor = tex2D ( _GlowRT , pixelData.uv );
       	half4 glowLight = _MultiplyColor + ( glowBlurColor * 5 + glowPlantColor * 0.3);
       	half3 sourceLight = sourceColor.rgb * (1 - glowPlantColor.a) + glowPlantColor.rgb ;//premultiply alpha blending
       	//half3 sourceLight = sourceColor.rgb + glowPlantColor.rgb ;//premultiply alpha blending
       	return fixed4(glowLight.rgb * sourceLight,1);
    }

    fixed4 frag_UnityBlur_NoGlow(v2f_img pixelData) : COLOR
    {
    	fixed4 sourceColor = tex2D(_MainTex, pixelData.uv);
       	fixed4 glowPlantColor = tex2D ( _GlowRT , pixelData.uv );
       	half4 glowLight = _MultiplyColor + glowPlantColor * 1;
       	half3 sourceLight = sourceColor.rgb * (1 - glowPlantColor.a) + glowPlantColor.rgb ;//premultiply alpha blending
       	//half3 sourceLight = sourceColor.rgb + glowPlantColor.rgb ;//additive
       	//half3 sourceLight = sourceColor.rgb * glowLight.rgb;
       	return fixed4(glowLight.rgb * sourceLight,1);
    }

    fixed4 frag_HDBlur_Glow(v2f_img pixelData) : COLOR
    {
    	fixed4 sourceColor = tex2D(_MainTex, pixelData.uv);
       	fixed4 glowBlurColor = tex2D(_GlowBlurRT, pixelData.uv);
       	fixed4 glowPlantColor = tex2D ( _GlowRT , pixelData.uv );
       	half4 glowLight = _MultiplyColor + ( glowBlurColor  + glowPlantColor );
       	half3 sourceLight = sourceColor.rgb * (1 - glowPlantColor.a) + glowPlantColor.rgb ;//premultiply alpha blending
       	//half3 sourceLight = sourceColor.rgb + glowPlantColor.rgb ;//premultiply alpha blending
       	return fixed4(glowLight.rgb * sourceLight,1);
    }

    fixed4 frag_HDBlur_NoGlow(v2f_img pixelData) : COLOR
    {
    	fixed4 sourceColor = tex2D(_MainTex, pixelData.uv);
       	fixed4 glowPlantColor = tex2D ( _GlowRT , pixelData.uv );
       	half4 glowLight = _MultiplyColor + glowPlantColor * 1;
       	half3 sourceLight = sourceColor.rgb * (1 - glowPlantColor.a) + glowPlantColor.rgb ;//premultiply alpha blending
       	//half3 sourceLight = sourceColor.rgb + glowPlantColor.rgb ;//additive
       	//half3 sourceLight = sourceColor.rgb * glowLight.rgb;
       	return fixed4(glowLight.rgb * sourceLight,1);
    }
    
    ENDCG
       
    Subshader {

    	//unity glow on
        Pass {
            // Additive
            Blend off
            ZTest Always Cull Off ZWrite Off Fog { Mode Off }
            ColorMask RGB
            
            CGPROGRAM
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma vertex vert_img
            #pragma fragment frag_UnityBlur_Glow
            ENDCG
        }

        //unity glow off
        Pass {
            // Additive
            Blend off
            ZTest Always Cull Off ZWrite Off Fog { Mode Off }
            ColorMask RGB
            
            CGPROGRAM
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma vertex vert_img
            #pragma fragment frag_UnityBlur_NoGlow
            ENDCG
        }

        //HD glow on
        Pass {
            // Additive
            Blend off
            ZTest Always Cull Off ZWrite Off Fog { Mode Off }
            ColorMask RGB
            
            CGPROGRAM
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma vertex vert_img
            #pragma fragment frag_HDBlur_Glow
            ENDCG
        }

        //HD glow off
        Pass {
            // Additive
            Blend off
            ZTest Always Cull Off ZWrite Off Fog { Mode Off }
            ColorMask RGB
            
            CGPROGRAM
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma vertex vert_img
            #pragma fragment frag_HDBlur_NoGlow
            ENDCG
        }
    }
    
    Fallback off
}