// ----------------------------------------------------------------------------
//  FISH - HDBlur Effect - All rights reserved - Yves Wang @ FISH Team 2016
// ----------------------------------------------------------------------------
Shader "FISH/HDBlur/Compose" {
	Properties {
	    _MainTex ("", 2D) = "white" {}
	    _Strength ("Strength", Float) = 1.0
	    //_ColorBuffer ("Color", 2D) = "" {}
	    //_GlobalStrength ("Global Strength" , Float) = 1.0
	}
       
    CGINCLUDE
   
    #include "UnityCG.cginc"
    struct v2f {
        half4 pos : POSITION;
        half2 uv : TEXCOORD0;
    };
   
    sampler2D _MainTex;
   	uniform fixed _Strength;
    uniform fixed _GlobalGlowStrength = 1;//global
  
   
    fixed4 frag(v2f_img pixelData) : COLOR
    {
        return tex2D(_MainTex, pixelData.uv) * _Strength * _GlobalGlowStrength;
    }

    ENDCG
       
    Subshader {
        Pass {
            // Additive
            Name "Add"
            Blend One One
            ZTest Always Cull Off ZWrite Off Fog { Mode Off }
            ColorMask RGB
            
            CGPROGRAM
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma vertex vert_img
            #pragma fragment frag
            ENDCG
        }
        
        Pass {
            // Screen
            Name "Screen"
            Blend One OneMinusSrcColor          
            ZTest Always Cull Off ZWrite Off Fog { Mode Off }
            ColorMask RGB
            
            CGPROGRAM
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma vertex vert_img
            #pragma fragment frag
            ENDCG
        }

        Pass {
            // off
            Name "off"
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