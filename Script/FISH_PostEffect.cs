/// A cusotmized and optimised post effect bundle for FISH
// camera setup from bottom to top
// 0. GPU Particles Camera - background color
// 1. Main Camera - all the other besides glowing plant
// 		In the OnRenderImage callback do these
//		a. - first blur the glowRT to glowRT_blurred, [passs 1,pass 2]
//		b. - multiply source with a tint color to simulate day-night light cycle [pass 3]
//		c. - then add glowRT_blurred with the source to light up areas [pass 3]
//		d. - then blend with the not glow plant - glowrt [pass 3] // b,c,d are merged into a composed shader, params are tinkered inside.
//		e. - then do the corner blur [pass 4,pass 5]
//		f. - then do the vignetting [pass 6]
//		
// 2. Effect Camera - all the post effect attached - glow plant and the occuldee foreground branches are only visible in this camera
//		Render to a RenderTexture glowRT.


// Different Glow Blur performance comparison
// Unity Blur - 0.08 ms - very good


using System;
using UnityEngine;
using FISH.ImageEffects;

namespace FISH.ImageEffects
{

	public enum GlowType { UnityBlur = 0, HDBlur = 1}

    [ExecuteInEditMode]
    [RequireComponent (typeof(Camera))]
    [AddComponentMenu ("FISH/PostEffect")]
	public class FISH_PostEffect : FISH.ImageEffects.PostEffectsBase
    {
    	//vignette
    	public bool isVignetteOn = true;
        public float intensity = 3f;                    // intensity == 0 disables pre pass (optimization) vignetting intensity

        //corner blur
        public bool isCornerBlurOn = true;
        public float blur = 1f;                           // blur == 0 disables blur pass (optimization) //corner blur mask radius
        public float blurSpread = 1.5f;					// corner blur radius
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
		public Color multiplyColor = new Color (0f,20f/255f,20f/255f);

		//glow

		public GlowType glowType;
		public Camera glowCamera;
		private RenderTexture glowBlurredRT,glowCameraRT;

		//->unity blur
		public bool isGlowOn = true;
		public float glowRadius = 3f;					// glow blur radius
		public int glowDownsample = 2;
        public int glowIteration = 3;
		public float glowIntensity = 2.5f;
		public FISH_PostEffectHelper.BlurType glowBlurType = FISH_PostEffectHelper.BlurType.SgxGauss;

		//->HDBlur
		public HDBlurSettings hdBlurSettings;

		public ShaderVariantCollection preloadShaders;



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

		protected override void Start() {
			base.Start();
        	FISH_PostEffectHelper.Init();
			if (preloadShaders.isWarmedUp==false) preloadShaders.WarmUp();
			//Debug.Log("Is Compute Shader supported?"+SystemInfo.supportsComputeShaders);
        }

        bool glowCameraInit = false;
        void Update() {
        	//check and assigne rt to glow camera
        	if (glowCamera!=null ) {
        		/*
        		if (glowCamera.targetTexture == null) 
        			glowCamera.targetTexture = glowCameraRT;
				
				if ( FISH_PostEffectHelper.IsResolutionChanged()) {
					RenderTexture t = new RenderTexture (Screen.width,Screen.height,0,RenderTextureFormat.Default);
					glowCamera.targetTexture = t;
					glowCameraRT.Release();
					glowCameraRT = t;
				}*/
				if (glowCamera.targetTexture == null || glowCameraRT == null || FISH_PostEffectHelper.IsResolutionChanged() ) {
        			Debug.Log("Glow Camera's target RT is created");
        			glowCameraRT = new RenderTexture (Screen.width,Screen.height,0,RenderTextureFormat.Default);
        			glowCameraRT.Create();
        			//discard the old RT
					RenderTexture rt = glowCamera.targetTexture;
					glowCamera.targetTexture = glowCameraRT;
					if (rt!=glowCameraRT && rt!=null) {
						rt.DiscardContents();
						if (Application.isEditor) DestroyImmediate (rt); else Destroy(rt);
					}
        		}

        		//if we just assign glow camera
        		if (glowCameraInit==false) {
					glowCameraInit = true;
					glowCamera.clearFlags = CameraClearFlags.SolidColor;
					glowCamera.backgroundColor = Color.clear;
					//if (glowCamera.orthographic==false) {
					glowCamera.orthographic = false;
					glowCamera.transparencySortMode = TransparencySortMode.Orthographic;
					Camera cam = this.GetComponent<Camera>();

					glowCamera.fieldOfView = 2f * Mathf.Rad2Deg * Mathf.Atan2 (cam.orthographicSize,Mathf.Abs(cam.transform.position.z));
					
        		}
			}

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

            float widthOverHeight = (1.0f * rtW) / (1.0f * rtH);
            const float oneOverBaseSize = 1.0f / 512.0f;

			RenderTexture sourceCompose;
			sourceCompose = RenderTexture.GetTemporary(source.width,source.height,0,source.format);

			//here we have glowCameraRT, source, and multiply Color, we need to use UnityBlur or HDBlur to blur the plant
			//then compose these rt into composeSource

			if (glowCamera != null) {
				if (glowType == GlowType.UnityBlur) {
					FISH_PostEffectHelper.UnityBlurGlow(
						source,
						sourceCompose,
						isGlowOn,
						glowCameraRT,
						glowBlurType,
						glowDownsample,
						glowRadius,
						glowIteration,
						glowIntensity,
						m_unityBlurMtl,
						isMultiplyColorOn,
						multiplyColor,
						m_composeMtl
					);
				} else if (glowType == GlowType.HDBlur){
					FISH_PostEffectHelper.HDBlurGlow(
						source,
						sourceCompose,
						isGlowOn,
						glowCameraRT,
						hdBlurSettings,
						isMultiplyColorOn,
						multiplyColor,
						m_composeMtl
					);

				}
			} else {
				RenderTexture.ReleaseTemporary(sourceCompose);
				sourceCompose = source;
			}


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
			if (glowCamera!=null) RenderTexture.ReleaseTemporary(sourceCompose);

				
        }
    }
}
