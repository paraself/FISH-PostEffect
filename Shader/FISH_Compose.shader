
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



    fixed4 frag(v2f_img pixelData) : COLOR
    {
    	fixed4 sourceColor = tex2D(_MainTex, pixelData.uv);
       	fixed4 glowBlurColor = tex2D(_GlowBlurRT, pixelData.uv);
       	fixed4 glowPlantColor = tex2D ( _GlowRT , pixelData.uv );
       	half4 glowLight = _MultiplyColor + ( glowBlurColor * 4 + glowPlantColor * 0.3);
       	half3 sourceLight = sourceColor.rgb * (1 - glowPlantColor.a) + glowPlantColor.rgb ;//premultiply alpha blending
       	return fixed4(glowLight.rgb * sourceLight,1);
    }
    
    
    ENDCG
       
    Subshader {
        Pass {
            // Additive
            Name "Add"
            Blend off
            ZTest Always Cull Off ZWrite Off Fog { Mode Off }
            ColorMask RGB
            
            CGPROGRAM
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma vertex vert_img
            #pragma fragment frag
            ENDCG
        }
    }
    
    Fallback off
}