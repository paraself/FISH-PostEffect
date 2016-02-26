using System;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
    [ExecuteInEditMode]
    [RequireComponent (typeof(Camera))]
    [AddComponentMenu ("FISH")]
    public class FISH_PostEffect : PostEffectsBase
    {

        public float intensity = 0.375f;                    // intensity == 0 disables pre pass (optimization)
        public float blur = 0.0f;                           // blur == 0 disables blur pass (optimization)
        public float blurSpread = 0.75f;
        public int iteration = 2;

        public Color multiplyColor = Color.white;

        public Shader vignetteShader;
        public Shader separableBlurShader;
        
        private Material m_VignetteMaterial;
        private Material m_SeparableBlurMaterial;


        public override bool CheckResources ()
        {
            CheckSupport (false);

            m_VignetteMaterial = CheckShaderAndCreateMaterial (vignetteShader, m_VignetteMaterial);
            m_SeparableBlurMaterial = CheckShaderAndCreateMaterial (separableBlurShader, m_SeparableBlurMaterial);

            if (!isSupported)
                ReportAutoDisable ();
            return isSupported;
        }


        void OnRenderImage (RenderTexture source, RenderTexture destination)
        {
            if ( CheckResources () == false)
            {
                Graphics.Blit (source, destination);
                return;
            }

            int rtW = source.width;
            int rtH = source.height;

            bool  doPrepass = (Mathf.Abs(blur)>0.0f || Mathf.Abs(intensity)>0.0f);

            float widthOverHeight = (1.0f * rtW) / (1.0f * rtH);
            const float oneOverBaseSize = 1.0f / 512.0f;

            RenderTexture color = null;
            RenderTexture color2A = null;

            if (doPrepass)
            {
      			// Blur corners
                if (Mathf.Abs (blur)>0.0f)
                {
                    color2A = RenderTexture.GetTemporary (rtW / 2, rtH / 2, 0, source.format);

                    Graphics.Blit (source, color2A);

					for(int i = 0; i < iteration; i++)
                    {	// maybe make iteration count tweakable
                        m_SeparableBlurMaterial.SetVector ("offsets",new Vector4 (0.0f, blurSpread * oneOverBaseSize, 0.0f, 0.0f));
                        RenderTexture color2B = RenderTexture.GetTemporary (rtW / 2, rtH / 2, 0, source.format);
                        Graphics.Blit (color2A, color2B, m_SeparableBlurMaterial);
                        RenderTexture.ReleaseTemporary (color2A);

                        m_SeparableBlurMaterial.SetVector ("offsets",new Vector4 (blurSpread * oneOverBaseSize / widthOverHeight, 0.0f, 0.0f, 0.0f));
                        color2A = RenderTexture.GetTemporary (rtW / 2, rtH / 2, 0, source.format);
                        Graphics.Blit (color2B, color2A, m_SeparableBlurMaterial);
                        RenderTexture.ReleaseTemporary (color2B);
                    }
                }

                //vignette
                m_VignetteMaterial.SetFloat ("_Intensity", intensity);		// intensity for vignette
                m_VignetteMaterial.SetFloat ("_Blur", blur);					// blur intensity
                m_VignetteMaterial.SetTexture ("_VignetteTex", color2A);	// blurred texture
				m_VignetteMaterial.SetColor("_MultiplyColor",multiplyColor);
            }

			if (doPrepass) 
				Graphics.Blit(source,destination,m_VignetteMaterial,0);// prepass blit: darken & blur corners
			else
				Graphics.Blit(source,destination);

            RenderTexture.ReleaseTemporary (color2A);
        }
    }
}
