using UnityEngine;
using System.Collections;
using FISH.ImageEffects;

namespace FISH.ImageEffects {
	public class FISH_PostEffectHelper  {

		static int _width,_height;
		public static void Init() {
			_width = Screen.width;
			_height = Screen.height;
		}

		//return true in the first frame when resolution changed
		public static bool IsResolutionChanged() {
			if (Screen.width != _width || Screen.height != _height) {
				_width = Screen.width;
				_height = Screen.height;
				Debug.Log("Resolution Changed!");
				return true;
			} else return false;
		}

		public enum BlurType {
	            StandardGauss = 0,
	            SgxGauss = 1,
	    }

		public static void UnityBlur (RenderTexture source, RenderTexture destination, BlurType blurType, int downsample, float blurSize, int blurIterations,Material blurMtl) {
			if (blurMtl == null) {
				blurMtl = new Material (Shader.Find("FISH/UnityBlur"));
			}
			float widthMod = 1.0f / (1.0f * (1<<downsample));
			blurMtl.SetVector ("_Parameter", new Vector4 (blurSize * widthMod, -blurSize * widthMod, 0.0f, 0.0f));
			source.filterMode = FilterMode.Bilinear;
			int rtW = source.width >> downsample;
			int rtH = source.height >> downsample;

			RenderTextureFormat format = source.format;

			// downsample
			RenderTexture rt = RenderTexture.GetTemporary (rtW, rtH, 0, format);
			rt.filterMode = FilterMode.Bilinear;
			Graphics.Blit (source, rt, blurMtl, 0);//downsample pass

			var passOffs= blurType == BlurType.StandardGauss ? 0 : 2;

			for(int i = 0; i < blurIterations; i++) {

				float iterationOffs = (i*1.0f);
				blurMtl.SetVector ("_Parameter", new Vector4 (blurSize * widthMod + iterationOffs, -blurSize * widthMod - iterationOffs, 0.0f, 0.0f));

				// vertical blur
				RenderTexture rt2 = RenderTexture.GetTemporary (rtW, rtH, 0, format);
				rt2.filterMode = FilterMode.Bilinear;
				Graphics.Blit (rt, rt2, blurMtl, 1 + passOffs);
				RenderTexture.ReleaseTemporary (rt);
				rt = rt2;

				// horizontal blur
				// if its the last blit
				if (i==blurIterations-1) {
					Graphics.Blit(rt,destination,blurMtl, 2 + passOffs);
					RenderTexture.ReleaseTemporary (rt);
				} else {
					rt2 = RenderTexture.GetTemporary (rtW, rtH, 0, format);
					rt2.filterMode = FilterMode.Bilinear;
					Graphics.Blit (rt, rt2, blurMtl, 2 + passOffs);
					RenderTexture.ReleaseTemporary (rt);
					rt = rt2;
				}

			}
		
			
		
		}//blur

	//		b. - multiply source with a tint color to simulate day-night light cycle [pass 3]
	//		c. - then add glowRT_blurred with the source to light up areas [pass 3]
	//		d. - then blend with the not glow plant - glowrt [pass 3]
		public static void Compose ( RenderTexture source, RenderTexture destination, bool isGlowOn, GlowType glowType, RenderTexture glowRT, RenderTexture glowBlurRT, float unityGlowIntensity ,bool isColorOn, Color multiplyColor,Material composeMtl) {
			if (composeMtl == null) {
				composeMtl = new Material (Shader.Find("FISH/Compose_MultiplyColor_Glow"));
			}
			Color mColor = isColorOn ? multiplyColor : Color.white;
			composeMtl.SetColor("_MultiplyColor",mColor);
			composeMtl.SetTexture("_GlowRT",glowRT);
			if (glowType == GlowType.UnityBlur) composeMtl.SetFloat("_UnityGlowIntensity",unityGlowIntensity);
			if (isGlowOn) {
				composeMtl.SetTexture("_GlowBlurRT",glowBlurRT);
				Graphics.Blit(source,destination,composeMtl,(int)glowType * 2 + 0);
			} else {
				Graphics.Blit(source,destination,composeMtl,(int)glowType * 2 + 1);
			}
		}



		//given that glowing plant is rendered into glowRT, we blur and glow the plant and compose with the source, render into dest
		public static void UnityBlurGlow ( RenderTexture source , RenderTexture dest , bool isGlowOn , RenderTexture glowRT , BlurType glowBlurType, int downsample, float blurSize, int blurIterations, float blurIntensity, Material blurMtl,  bool isColorOn, Color multiplyColor , Material composeMtl) {
			RenderTexture glowBlurredRT = null;
			if (glowRT == null) isGlowOn = false;
			if (isGlowOn) {
				glowBlurredRT = RenderTexture.GetTemporary(glowRT.width,glowRT.height,0,glowRT.format);
				FISH_PostEffectHelper.UnityBlur(glowRT,glowBlurredRT,glowBlurType,downsample,blurSize,blurIterations,blurMtl);
			}
			Compose(source,dest,isGlowOn,GlowType.UnityBlur, glowRT,glowBlurredRT,blurIntensity,isColorOn,multiplyColor,composeMtl);
			if (isGlowOn) RenderTexture.ReleaseTemporary(glowBlurredRT);
		}

		//------------------------------------------------------------------------------------------------------------

		static HDBlur hdBlur;
		public static float totalPixels {
			get {
				if (hdBlur!=null)
					return hdBlur.totalPixelsOutput;
				else return 0f;
			}
		}
		public static void HDBlurGlow ( RenderTexture source, RenderTexture dest, bool isGlowOn, RenderTexture glowRT, HDBlurSettings hdBlurSetting, bool isColorOn, Color multiplyColor,Material composeMtl) {

			if (hdBlur==null) hdBlur = new HDBlur();

			RenderTexture glowBlurredRT = null;
			if (glowRT==null) isGlowOn = false;
			if (isGlowOn) {
				glowBlurredRT = RenderTexture.GetTemporary(glowRT.width,glowRT.height,0,glowRT.format);
				RenderTexture rerenderRT = RenderTexture.GetTemporary(
					Screen.width / (int)hdBlurSetting.rerenderResolution ,
					Screen.height / (int)hdBlurSetting.rerenderResolution,0,glowRT.format);
				Graphics.Blit(glowRT,rerenderRT);
				hdBlur.BlurAndBlitBuffer(rerenderRT,glowBlurredRT,hdBlurSetting);
				RenderTexture.ReleaseTemporary(rerenderRT);
			}

			Compose(source,dest,isGlowOn,GlowType.HDBlur,glowRT,glowBlurredRT,1f,isColorOn,multiplyColor,composeMtl);
			if (isGlowOn) RenderTexture.ReleaseTemporary(glowBlurredRT);
		}


	}
}

