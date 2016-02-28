using UnityEngine;
using System.Collections;

public class FISH_PostEffectHelper  {

	public enum BlurType {
            StandardGauss = 0,
            SgxGauss = 1,
    }

	public static void UnityBlur (RenderTexture source, RenderTexture destination, BlurType blurType, int downsample, float blurSize, int blurIterations, Material blurMtl) {
		if (blurMtl == null) {
			blurMtl = new Material (Shader.Find("FISH/UnityBlur"));
		}
		float widthMod = 1.0f / (1.0f * (1<<downsample));
		blurMtl.SetVector ("_Parameter", new Vector4 (blurSize * widthMod, -blurSize * widthMod, 0.0f, 0.0f));
		source.filterMode = FilterMode.Bilinear;
		int rtW = source.width >> downsample;
		int rtH = source.height >> downsample;

		// downsample
		RenderTexture rt = RenderTexture.GetTemporary (rtW, rtH, 0, source.format);
		rt.filterMode = FilterMode.Bilinear;
		Graphics.Blit (source, rt, blurMtl, 0);//downsample pass

		var passOffs= blurType == BlurType.StandardGauss ? 0 : 2;

		for(int i = 0; i < blurIterations; i++) {

			float iterationOffs = (i*1.0f);
			blurMtl.SetVector ("_Parameter", new Vector4 (blurSize * widthMod + iterationOffs, -blurSize * widthMod - iterationOffs, 0.0f, 0.0f));

			// vertical blur
			RenderTexture rt2 = RenderTexture.GetTemporary (rtW, rtH, 0, source.format);
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
				rt2 = RenderTexture.GetTemporary (rtW, rtH, 0, source.format);
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
	public static void Compose ( RenderTexture source, RenderTexture destination, bool isGlowOn, RenderTexture glowRT, RenderTexture glowBlurRT, bool isColorOn, Color multiplyColor,Material composeMtl) {
		if (composeMtl == null) {
			composeMtl = new Material (Shader.Find("FISH/Compose_MultiplyColor_Glow"));
		}
		Color mColor = isColorOn ? multiplyColor : Color.white;
		Texture blurRT;
		if (isGlowOn) blurRT = glowBlurRT; else blurRT = Texture2D.blackTexture;
		composeMtl.SetColor("_MultiplyColor",mColor);
		composeMtl.SetTexture("_GlowRT",glowRT);
		composeMtl.SetTexture("_GlowBlurRT",blurRT);
		Graphics.Blit(source,destination,composeMtl);
	}
}
