using System;
using UnityEngine;
namespace FISH.ImageEffects
{
	[System.Serializable]
	public class HDBlurSettings
	{
		#region default blur
		[Tooltip("Use ARGEHalf or RenderTexture.Default")]
		public bool highPrecision = true;

		[Tooltip("Re-render and downsample the glow camera's content")]
		public Resolution rerenderResolution = Resolution.Full;


		public Resolution baseResolution = Resolution.Quarter;
		public BlendMode blendMode = BlendMode.Additive;

		public DownsampleResolution downsampleResolution = DownsampleResolution.Quarter;
		public DownsampleBlendMode downsampleBlendMode = DownsampleBlendMode.Max;
		[Range(0,10)] public int downsampleSteps = 2;

		[Range(0.01f,10f)] public float innerStrength = 1f;
		[Range(0.01f,10f)] public float outerStrength = 1f;
		[Range(0.01f,10f)] public float boostStrength = 1f;


		#endregion

		//public int iterations = 3;
		//public float blurSpread = 0.6f;
		//public AnimationCurve falloff = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
		//public float falloffScale = 1f;
		//public int radius = 4;
		//public bool normalize = true;
	}



	public enum BlendMode
	{
		Additive,
		Screen
	}

	public enum DownsampleBlendMode
	{
		Additive,
		Screen,
		Max = 100
	}

	public enum DownsampleResolution
	{
		Half = 2,
		Quarter = 4
	}

	public enum Resolution
	{
		Full = 1,
		Half = 2,
		Quarter = 4
	}

}


