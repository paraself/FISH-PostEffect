/// A cusotmized and optimised post effect bundle for FISH
// camera setup from bottom to top
// 0. GPU Particles Camera - background color
// 1. Main Camera - all the other besides glowing plant
// 		In the OnRenderImage callback do these
//		a. - first blur the glowRT to glowRT_blurred, [passs 1,pass 2]
//		b. - multiply source with a tint color to simulate day-night light cycle [pass 3]
//		c. - then add glowRT_blurred with the source to light up areas [pass 3]
//		d. - then blend with the not glow plant - glowrt [pass 3]
//		e. - then do the corner blur [pass 4,pass 5]
//		f. - then do the vignetting [pass 6]
//		
// 2. Effect Camera - all the post effect attached - glow plant and the occuldee foreground branches are only visible in this camera
//		Render to a RenderTexture glowRT.


using System;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
    [ExecuteInEditMode]
    [RequireComponent (typeof(Camera))]
    [AddComponentMenu ("FISH")]
    public class FISH_PostEffect : PostEffectsBase
    {
    	//vignette
    	public bool isVignetteOn = true;
        public float intensity = 0.375f;                    // intensity == 0 disables pre pass (optimization) vignetting intensity

        //corner blur
        public bool isCornerBlurOn = true;
        public float blur = 0.0f;                           // blur == 0 disables blur pass (optimization) //corner blur mask radius
        public float blurSpread = 0.75f;					// corner blur radius
        public int iteration = 2;

        public Shader vignetteShader;
        public Shader separableBlurShader;
		public Shader unityBlurShader;
		public Shader composeShader;
        private Material m_VignetteMaterial;
        private Material m_SeparableBlurMaterial;
		private Material m_unityBlurMtl;
		private Material m_composeMtl;

        //multiply color
        public bool isMultiplyColorOn = true;
		public Color multiplyColor = Color.white;

		//glow
		public bool isGlowOn = true;
		public Camera glowCamera;
		public float glowRadius = 3f;					// glow blur radius
		public int glowDownsample = 1;
        public int glowIteration = 2;
		public FISH_PostEffectHelper.BlurType glowBlurType;
		private RenderTexture glowBlurredRT,glowCameraRT;






        public override bool CheckResources ()
        {
            CheckSupport (false);

            m_VignetteMaterial = CheckShaderAndCreateMaterial (vignetteShader, m_VignetteMaterial);
            m_SeparableBlurMaterial = CheckShaderAndCreateMaterial (separableBlurShader, m_SeparableBlurMaterial);
			m_unityBlurMtl = CheckShaderAndCreateMaterial ( unityBlurShader , m_unityBlurMtl);
			m_composeMtl = CheckShaderAndCreateMaterial ( composeShader , m_composeMtl);

            if (!isSupported)
                ReportAutoDisable ();
            return isSupported;
        }

        void Update() {
        	//check and assigne rt to glow camera
        	if (glowCamera!=null ) {
        		if (glowCamera.targetTexture == null || glowCameraRT == null || (glowCameraRT.width != Screen.width || glowCameraRT.height != Screen.height)) {
        			Debug.Log("Glow Camera's target RT is created");
        			glowCameraRT = new RenderTexture (Screen.width,Screen.height,0,RenderTextureFormat.Default);
        			glowCameraRT.Create();
        			//discard the old RT
					RenderTexture rt = glowCamera.targetTexture;
					glowCamera.targetTexture = glowCameraRT;
					if (rt) rt.Release();
        		}
        	}

        }

        void OnDestroy() {
        	if (glowCameraRT) {
        		//glowCameraRT.Release();
        		//GameObject.Destroy(glowCameraRT);
        	}
        }

        void OnPreRender() {
//        	if (glowCamera!=null && isGlowOn)
				//glowCameraRT = RenderTexture.GetTemporary(Screen.width,Screen.height,0,RenderTextureFormat.Default);
//				glowCamera.targetTexture = RenderTexture.GetTemporary(Screen.width,Screen.height,0,RenderTextureFormat.Default);
        }

        void OnPostRender () {
//        	if (isGlowOn)
//        		RenderTexture.ReleaseTemporary(glowCameraRT);
        }

        void OnRenderImage (RenderTexture source, RenderTexture destination)
        {
            if ( CheckResources () == false)
            {
                Graphics.Blit (source, destination);
                return;
            }

            //parameters preparation
            int rtW = source.width;
            int rtH = source.height;

            bool  doPrepass = (Mathf.Abs(blur)>0.0f || Mathf.Abs(intensity)>0.0f);

            float widthOverHeight = (1.0f * rtW) / (1.0f * rtH);
            const float oneOverBaseSize = 1.0f / 512.0f;

           
          

			//blur the glow camera's rendertexture
			if (glowCameraRT!=null && isGlowOn) {
				glowBlurredRT = RenderTexture.GetTemporary(glowCameraRT.width,glowCameraRT.height,0,glowCameraRT.format);
				FISH_PostEffectHelper.UnityBlur(glowCameraRT,glowBlurredRT,glowBlurType,glowDownsample,glowRadius,glowIteration,m_unityBlurMtl);
			}

			//compose
			RenderTexture sourceCompose = RenderTexture.GetTemporary(source.width,source.height,0,source.format);
			FISH_PostEffectHelper.Compose(source,sourceCompose,isGlowOn,glowCameraRT,glowBlurredRT,isMultiplyColorOn,multiplyColor,m_composeMtl);
			if (glowCameraRT!=null && isGlowOn) RenderTexture.ReleaseTemporary(glowBlurredRT);

			//corner blur
			RenderTexture color2A = isCornerBlurOn ? RenderTexture.GetTemporary (rtW / 2, rtH / 2, 0, source.format) : sourceCompose;

			if (isCornerBlurOn) {
				Graphics.Blit(sourceCompose, color2A);
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
				m_VignetteMaterial.SetTexture ("_VignetteTex", color2A);	// blurred texture
				m_VignetteMaterial.SetFloat ("_Blur", blur);					// blur intensity
            }

            if (isVignetteOn) {
                m_VignetteMaterial.SetFloat ("_Intensity", intensity);		// intensity for vignette
			}

			//final compose
			if ( isVignetteOn && isCornerBlurOn ) {
				Graphics.Blit(sourceCompose,destination,m_VignetteMaterial,0);
			} else if ( !isVignetteOn && isCornerBlurOn ) {
				Graphics.Blit(sourceCompose,destination,m_VignetteMaterial,1);
			} else if ( isVignetteOn && !isCornerBlurOn ) {
				Graphics.Blit(sourceCompose,destination,m_VignetteMaterial,2);
			} else if ( !isVignetteOn && !isCornerBlurOn ) {
				Graphics.Blit(sourceCompose,destination,m_VignetteMaterial,3);
			}

			if (isCornerBlurOn) RenderTexture.ReleaseTemporary (color2A);
			if (glowCameraRT) glowCameraRT.DiscardContents();
			RenderTexture.ReleaseTemporary(sourceCompose);

				
        }
    }
}
