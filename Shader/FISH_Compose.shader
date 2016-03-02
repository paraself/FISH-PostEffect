
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
    uniform float _UnityGlowIntensity;

    //helpers
    fixed Darkness ( fixed4 c) {
    	return 0.375 * c.r + 0.5 * c.g + 0.125 * c.b;
    }

    fixed3 GlowCompose(fixed4 sourceColor,fixed4 glowBlurColor,fixed4 glowPlantColor) {
    	half4 glowLight;
       	half3 sourceLight;
       	half darkness = Darkness(_MultiplyColor);//0 = darkest, 1 = lightest
       	glowLight = glowBlurColor * (1-darkness * darkness) + glowPlantColor * 0.1 ;//glowlight is the blured plant add a bit of the plant
       	sourceLight = sourceColor.rgb * (1 - glowPlantColor.a ) + glowPlantColor.rgb ;//premultiply alpha blending the source and the not blurred plant
       	half3 lightUp = sourceLight * (glowLight + _MultiplyColor); //light up the source
       	half3 lightOverFlow = glowLight * glowPlantColor.a * 0.5;//add the over flow light, 0.5 the more the light flow more
       	return lightUp + lightOverFlow;
    }


    fixed3 NoGlowCompose(fixed4 sourceColor,fixed4 glowPlantColor) {
    	fixed plantVisibleInNight = 0.2;//how not glowed plant is visible in night
    	half4 glowLight = _MultiplyColor + glowPlantColor * (1-_MultiplyColor) * plantVisibleInNight;
       	half3 sourceLight = sourceColor.rgb * (1 - glowPlantColor.a) + glowPlantColor.rgb ;//premultiply alpha blending
       	return glowLight.rgb * sourceLight;
    }

    fixed4 BlurColor ( half2 uv ) {
		//if its dx, uv starts from top
    	#if UNITY_UV_STARTS_AT_TOP
		uv = 1 - uv;
		#endif
		return tex2D(_GlowBlurRT, uv);
    }

    fixed4 PlantColor ( half2 uv) {
    	#if UNITY_UV_STARTS_AT_TOP
		uv = 1 - uv;
		#endif
		return tex2D(_GlowRT, uv);
    }

    //helpers

	fixed3 frag_UnityBlur_Glow(v2f_img pixelData) : COLOR
    {
    	fixed4 sourceColor = tex2D(_MainTex, pixelData.uv);
       	fixed4 glowBlurColor = BlurColor ( pixelData.uv) * _UnityGlowIntensity;
       	fixed4 glowPlantColor = PlantColor( pixelData.uv );
		return GlowCompose(sourceColor,glowBlurColor,glowPlantColor);
    }

    fixed3 frag_UnityBlur_NoGlow(v2f_img pixelData) : COLOR
    {
    	fixed4 sourceColor = tex2D(_MainTex, pixelData.uv);
       	fixed4 glowPlantColor = PlantColor( pixelData.uv );
		return NoGlowCompose(sourceColor,glowPlantColor);
    }

	fixed3 frag_HDBlur_Glow(v2f_img pixelData) : COLOR
    {
    	fixed4 sourceColor = tex2D(_MainTex, pixelData.uv);
       	fixed4 glowBlurColor = BlurColor ( pixelData.uv);
       	fixed4 glowPlantColor = PlantColor( pixelData.uv );
		return GlowCompose(sourceColor,glowBlurColor,glowPlantColor);

    }

    fixed3 frag_HDBlur_NoGlow(v2f_img pixelData) : COLOR
    {
    	fixed4 sourceColor = tex2D(_MainTex, pixelData.uv);
       	fixed4 glowPlantColor = PlantColor( pixelData.uv );
       	return NoGlowCompose(sourceColor,glowPlantColor);
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