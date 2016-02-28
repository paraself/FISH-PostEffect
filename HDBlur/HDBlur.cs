using System;
using UnityEngine;
namespace FISH.ImageEffects
{
	public class HDBlur : HDBlurBase
	{
		private bool maxFallback = false;
		protected Material composeMaxMaterial = null;

		public float totalPixelsOutput = 0f;

		public HDBlur()
		{

			Shader.SetGlobalFloat("_GlobalGlowStrength",1f);
			Shader shader = Shader.Find("FISH/HDBlur/Compose Max");
			if (!shader.isSupported)
			{
				shader = Shader.Find("FISH/HDBlur/Compose Max Fallback");
				this.maxFallback = true;
			}
			this.composeMaxMaterial = new Material(shader);
			this.composeMaxMaterial.hideFlags = HideFlags.DontSaveInEditor;
		}

		void RecoredPixels ( RenderTexture rt) {
			totalPixelsOutput += rt.width * rt.height;
		}
		public void BlurAndBlitBuffer(RenderTexture rbuffer, RenderTexture destination, HDBlurSettings settings)
		{
			totalPixelsOutput = 0f;
			totalPixelsOutput += (Screen.width / (int) settings.rerenderResolution) * (Screen.height / (int) settings.rerenderResolution);//re-render pixels

			//Debug.Log(settings.outerStrength);
			int baseResolution = (int)settings.baseResolution;
			int downsampleResolution = (int)settings.downsampleResolution;
			RenderTexture[] array = new RenderTexture[settings.downsampleSteps * 2];
			RenderTextureFormat format = (settings.highPrecision) ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.Default;
			this.downsampleMaterial.SetFloat("_Strength", settings.innerStrength / (float)((baseResolution != 4) ? 1 : 4));
			RenderTexture temporary = RenderTexture.GetTemporary(rbuffer.width / baseResolution, rbuffer.height / baseResolution, 0, format);
			RenderTexture temporary2 = RenderTexture.GetTemporary(temporary.width, temporary.height, 0, format);
			Graphics.Blit(rbuffer, temporary, this.downsampleMaterial, (baseResolution != 4) ? 1 : 0);
			RecoredPixels(temporary);
			this.downsampleMaterial.SetFloat("_Strength", settings.innerStrength / (float)((downsampleResolution != 4) ? 1 : 4));
			RenderTexture renderTexture = temporary;
			for (int i = 0; i < settings.downsampleSteps; i++)
			{
				int num = renderTexture.width / downsampleResolution;
				int num2 = renderTexture.height / downsampleResolution;
				if (num == 0 || num2 == 0)
				{
					break;
				}
				array[i * 2] = RenderTexture.GetTemporary(num, num2, 0, format);
				array[i * 2 + 1] = RenderTexture.GetTemporary(num, num2, 0, format);
				Graphics.Blit(renderTexture, array[i * 2], this.downsampleMaterial, (downsampleResolution != 4) ? 1 : 0);
				RecoredPixels(array[i * 2]);
				renderTexture = array[i * 2];
			}
			for (int j = settings.downsampleSteps - 1; j >= 0; j--)
			{
				if (!(array[j * 2] == null))
				{
					base.BlurBuffer(array[j * 2], array[j * 2 + 1]);  
					RenderTexture renderTexture2 = (j <= 0) ? temporary : array[(j - 1) * 2];
					renderTexture2.MarkRestoreExpected();
					if (settings.downsampleBlendMode == DownsampleBlendMode.Max)
					{
						if (this.maxFallback)
						{
							this.composeMaxMaterial.SetTexture("_DestTex", renderTexture2);
						}
						this.composeMaxMaterial.SetFloat("_Strength", settings.outerStrength / ((float)j / 2f + 1f));//Debug.Log(downsampleMaterial.shader);
						Graphics.Blit(array[j * 2], renderTexture2, this.composeMaxMaterial);
						RecoredPixels(renderTexture2);
					}
					else
					{
						this.composeMaterial.SetFloat("_Strength", settings.outerStrength / ((float)j / 2f + 1f));
						Graphics.Blit(array[j * 2], renderTexture2, this.composeMaterial, (int)settings.downsampleBlendMode);
						RecoredPixels(renderTexture2);
					}
				}
			}
			base.BlurBuffer(temporary, temporary2);
			this.composeMaterial.SetFloat("_Strength", settings.boostStrength);
			if (QualitySettings.antiAliasing > 0)
			{
				string[] shaderKeywords = new string[]
				{
					"ANTIALIAS"
				};
				this.composeMaterial.shaderKeywords = shaderKeywords;
			}


			//final compose
			//Graphics.Blit(temporary, destination, this.composeMaterial, (int)settings.blendMode);

			Graphics.Blit(temporary, destination,composeMaterial,2);//only output the blurred image, we then do the compose manually
			RecoredPixels(destination);
			totalPixelsOutput += (Screen.width * Screen.height);//final compose

			//Graphics.Blit(temporary, destination);

			this.composeMaterial.shaderKeywords = null;
			RenderTexture.ReleaseTemporary(temporary);
			RenderTexture.ReleaseTemporary(temporary2);
			for (int k = 0; k < settings.downsampleSteps; k++)
			{
				RenderTexture.ReleaseTemporary(array[k * 2]);
				RenderTexture.ReleaseTemporary(array[k * 2 + 1]);
			}
		}
	}
}
