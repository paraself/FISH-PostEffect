using System;
using UnityEngine;
namespace FISH.ImageEffects
{
	public abstract class HDBlurBase
	{
		protected Material blurMaterial = null;
		protected Material composeMaterial = null;
		protected Material downsampleMaterial = null;
//		protected Material multiplyMaterial = null;
//		protected Material addMaterial = null;
		private bool glMode = true;
		//public Color multiplyColor = Color.white;
		
		protected Vector2[] blurOffsetsHorizontal = new Vector2[]
		{
			new Vector2(0f, 0f),
			new Vector2(-1.38461542f, 0f),
			new Vector2(1.38461542f, 0f),
			new Vector2(-3.23076916f, 0f),
			new Vector2(3.23076916f, 0f)
		};
		protected Vector2[] blurOffsetsVertical = new Vector2[]
		{
			new Vector2(0f, 0f),
			new Vector2(0f, -1.38461542f),
			new Vector2(0f, 1.38461542f),
			new Vector2(0f, -3.23076916f),
			new Vector2(0f, 3.23076916f)
		};
		public HDBlurBase()
		{
			Shader shader = Shader.Find("FISH/HDBlur/Compose");
			Shader shader2 = Shader.Find("FISH/HDBlur/Blur GL");
			if (!shader2.isSupported)
			{
				this.glMode = false;
				shader2 = Shader.Find("FISH/HDBlur/Blur");
			}
			Shader shader3 = Shader.Find("FISH/HDBlur/Downsample");
			this.composeMaterial = new Material(shader);
			this.composeMaterial.hideFlags = HideFlags.DontSaveInEditor;
			this.blurMaterial = new Material(shader2);
			this.blurMaterial.hideFlags = HideFlags.DontSaveInEditor;
			this.downsampleMaterial = new Material(shader3);
			this.downsampleMaterial.hideFlags = HideFlags.DontSaveInEditor;
//			this.multiplyMaterial = new Material (Shader.Find("Hidden/Glow 12/Multiply"));
//			this.multiplyMaterial.hideFlags = HideFlags.DontSaveInEditor;
//			addMaterial = new Material ( Shader.Find("Hidden/Glow 12/Add"));
//			addMaterial.hideFlags = HideFlags.DontSaveInEditor;
		}
		protected void BlurBuffer(RenderTexture buffer, RenderTexture buffer2)
		{
			buffer2.DiscardContents();
			if (this.glMode)
			{
				/*
				Graphics.BlitMultiTap(buffer, buffer2, this.blurMaterial, this.blurOffsetsHorizontal);
				buffer.DiscardContents();
				Graphics.BlitMultiTap(buffer2, buffer, this.blurMaterial, this.blurOffsetsVertical);
				*/
				Graphics.Blit(buffer,buffer2,this.blurMaterial,0);
				buffer.DiscardContents();
				Graphics.Blit(buffer2,buffer,this.blurMaterial,1);
			}
			else
			{
				this.blurMaterial.SetFloat("_offset1", 1.38461542f);
				this.blurMaterial.SetFloat("_offset2", 3.23076916f);
				Graphics.Blit(buffer, buffer2, this.blurMaterial, 0);
				buffer.DiscardContents();
				Graphics.Blit(buffer2, buffer, this.blurMaterial, 1);
			}
		}
	}

}



