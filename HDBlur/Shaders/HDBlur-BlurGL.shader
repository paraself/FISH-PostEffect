// ----------------------------------------------------------------------------
//  FISH - HDBlur Effect - All rights reserved - Yves Wang @ FISH Team 2016
// ----------------------------------------------------------------------------
Shader "FISH/HDBlur/Blur GL" {
    Properties {
        _MainTex ("", 2D) = "" {}
    }
   
    CGINCLUDE
   
    #include "UnityCG.cginc"
   
    sampler2D _MainTex;
    half2 _MainTex_TexelSize;
    const half al = 1.38461542h;
    const half bl = 3.23076916h;
  
  	fixed4 frag_vertical(v2f_img pixelData) : COLOR
    {
        fixed4 c1 = tex2D(_MainTex, pixelData.uv);
        fixed4 c2 = tex2D(_MainTex, pixelData.uv + half2(0,_MainTex_TexelSize.y * al));
        fixed4 c3 = tex2D(_MainTex, pixelData.uv - half2(0,_MainTex_TexelSize.y * al));
        fixed4 c4 = tex2D(_MainTex, pixelData.uv + half2(0,_MainTex_TexelSize.y * bl));
        fixed4 c5 = tex2D(_MainTex, pixelData.uv - half2(0,_MainTex_TexelSize.y * bl));
        
        return c1 * 0.2270270270 + (c2 + c3) * 0.3162162162 + (c4 + c5) * 0.0702702703;
        
    }
    
    fixed4 frag_horizontal(v2f_img pixelData) : COLOR
    {
        fixed4 c1 = tex2D(_MainTex, pixelData.uv);
        fixed4 c2 = tex2D(_MainTex, pixelData.uv + half2(_MainTex_TexelSize.x * al,0));
        fixed4 c3 = tex2D(_MainTex, pixelData.uv - half2(_MainTex_TexelSize.x * al,0));
        fixed4 c4 = tex2D(_MainTex, pixelData.uv + half2(_MainTex_TexelSize.x * bl,0));
        fixed4 c5 = tex2D(_MainTex, pixelData.uv - half2(_MainTex_TexelSize.x * bl,0));
        
        return c1 * 0.2270270270 + (c2 + c3) * 0.3162162162 + (c4 + c5) * 0.0702702703;
        
    }
   
    ENDCG
   
    Subshader {
    	//pass 0
        Pass {
          ZTest Always Cull Off ZWrite Off
          Fog { Mode off }
          ColorMask RGB
        
          CGPROGRAM
          //#pragma debug
          #pragma glsl
          #pragma only_renderers gles opengl
          #pragma fragmentoption ARB_precision_hint_fastest
          #pragma vertex vert_img
          #pragma fragment frag_vertical
          ENDCG
        }
        
        //pass 1
        Pass {
          ZTest Always Cull Off ZWrite Off
          Fog { Mode off }
          ColorMask RGB
        
          CGPROGRAM
          //#pragma debug
          #pragma glsl
          #pragma only_renderers gles opengl
          #pragma fragmentoption ARB_precision_hint_fastest
          #pragma vertex vert_img
          #pragma fragment frag_horizontal
          ENDCG
        }
    }
     
    Fallback off
   
}
