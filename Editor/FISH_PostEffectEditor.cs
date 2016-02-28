using System;
using UnityEditor;
using UnityEngine;


namespace UnityStandardAssets.ImageEffects
{
	[CustomEditor (typeof(FISH.ImageEffects.FISH_PostEffect))]
	class FISH_PostEffectEditor : Editor
    {
        private SerializedObject m_SerObj;
        private SerializedProperty m_Intensity;             // intensity == 0 disables pre pass (optimization)
       	private SerializedProperty m_Blur;                  // blur == 0 disables blur pass (optimization)
        private SerializedProperty m_BlurSpread;
		private SerializedProperty m_Iteration;
		private SerializedProperty m_multiplyColor;
		private SerializedProperty m_glowCamera;
		private SerializedProperty m_glowRadius;
		private SerializedProperty m_glowIteration;
		private SerializedProperty m_glowBlurType;
		private SerializedProperty m_glowDownsample;

		private SerializedProperty m_isVignetteOn;
		private SerializedProperty m_isCornerBlurOn;
		private SerializedProperty m_isMultiplyColorOn;
		private SerializedProperty m_isGlowOn;

		private SerializedProperty m_glowType;
		private SerializedProperty m_hdBlurSettings;



        void OnEnable ()
        {
            m_SerObj = new SerializedObject (target);
			m_Intensity = m_SerObj.FindProperty ("intensity");
			m_Blur = m_SerObj.FindProperty ("blur");
            m_BlurSpread = m_SerObj.FindProperty ("blurSpread");
			m_Iteration = m_SerObj.FindProperty ("iteration");
			m_multiplyColor = m_SerObj.FindProperty ("multiplyColor");
			m_glowCamera = m_SerObj.FindProperty ("glowCamera");
			m_glowRadius = m_SerObj.FindProperty ("glowRadius");
			m_glowIteration = m_SerObj.FindProperty ("glowIteration");
			m_glowBlurType = m_SerObj.FindProperty ("glowBlurType");
			m_glowDownsample = m_SerObj.FindProperty ("glowDownsample");

			m_isVignetteOn = m_SerObj.FindProperty("isVignetteOn");
			m_isCornerBlurOn = m_SerObj.FindProperty("isCornerBlurOn");
			m_isMultiplyColorOn = m_SerObj.FindProperty("isMultiplyColorOn");
			m_isGlowOn = m_SerObj.FindProperty("isGlowOn");

			m_glowType = m_SerObj.FindProperty("glowType");
			m_hdBlurSettings = m_SerObj.FindProperty("hdBlurSettings");
        }


        public override void OnInspectorGUI ()
        {
            m_SerObj.Update ();

            Color bkgColor = GUI.backgroundColor;

            //vignette
			EditorGUILayout.BeginHorizontal();
			GUI.backgroundColor = Color.cyan;
            EditorGUILayout.HelpBox("1. Vignetting",MessageType.None);
			GUI.backgroundColor = bkgColor;
			m_isVignetteOn.boolValue = EditorGUILayout.ToggleLeft ("",m_isVignetteOn.boolValue,GUILayout.Width(12f));
			EditorGUILayout.EndHorizontal();
			//EditorGUILayout.Separator();

            if (!m_isVignetteOn.boolValue) GUI.enabled = false; else GUI.enabled = true;
			EditorGUILayout.PropertyField (m_Intensity, new GUIContent("Intensity"));
			GUI.enabled = true;

			//Corner Blur
			EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = Color.cyan;
            EditorGUILayout.HelpBox("2. Corner Blur",MessageType.None);
			GUI.backgroundColor = bkgColor;
			m_isCornerBlurOn.boolValue = EditorGUILayout.ToggleLeft ("",m_isCornerBlurOn.boolValue,GUILayout.Width(12f));
			EditorGUILayout.EndHorizontal();

			if (!m_isCornerBlurOn.boolValue) GUI.enabled = false; else GUI.enabled = true;
            EditorGUILayout.PropertyField (m_Blur, new GUIContent("Blurred Corners","Blur mask radius"));
            if (m_Blur.floatValue>0.0f)
                EditorGUILayout.PropertyField (m_BlurSpread, new GUIContent("Blur Distance","Blur radius"));
			EditorGUILayout.IntSlider (m_Iteration, 1, 5, new GUIContent("Iteration"));
			GUI.enabled = true;

			//multiply color
			EditorGUILayout.BeginHorizontal();
			GUI.backgroundColor = Color.cyan;
			EditorGUILayout.HelpBox("3. Multiply Color",MessageType.None);
			GUI.backgroundColor = bkgColor;
			m_isMultiplyColorOn.boolValue = EditorGUILayout.ToggleLeft ("",m_isMultiplyColorOn.boolValue,GUILayout.Width(12f));
			EditorGUILayout.EndHorizontal();

			if (!m_isMultiplyColorOn.boolValue) GUI.enabled = false; else GUI.enabled = true;
			EditorGUILayout.PropertyField (m_multiplyColor, new GUIContent("Multiply Color"));
			GUI.enabled = true;

			//glow
			EditorGUILayout.BeginHorizontal();
			GUI.backgroundColor = Color.cyan;
			EditorGUILayout.HelpBox("4. Glow Effect",MessageType.None);
			GUI.backgroundColor = bkgColor;
			m_isGlowOn.boolValue = EditorGUILayout.ToggleLeft ("",m_isGlowOn.boolValue,GUILayout.Width(12f));
			EditorGUILayout.EndHorizontal();
			if (!m_isGlowOn.boolValue) GUI.enabled = false; else GUI.enabled = true;

			//glowtype
			EditorGUILayout.PropertyField(m_glowType);
			//if use unity blur
			if (m_glowType.intValue == 0) {
				EditorGUILayout.PropertyField (m_glowCamera, new GUIContent("Glow Camera"));
				EditorGUILayout.Slider (m_glowRadius,0f,10f,new GUIContent("Radius"));
				EditorGUILayout.IntSlider (m_glowDownsample, 0, 2, new GUIContent("Downsample"));
				EditorGUILayout.IntSlider (m_glowIteration, 1, 5, new GUIContent("Iteration"));
				EditorGUILayout.PropertyField (m_glowBlurType, new GUIContent("Blur Type"));
			} 
			//if use hd blur
			else {
				EditorGUILayout.PropertyField(m_hdBlurSettings,true);
				EditorGUILayout.HelpBox("Total Pixels: " + FISH_PostEffectHelper.totalPixels , MessageType.None);
				EditorGUILayout.HelpBox("Screen X " + FISH_PostEffectHelper.totalPixels / (Screen.width * Screen.height) , MessageType.None);
			}
			GUI.enabled = true;

            m_SerObj.ApplyModifiedProperties();
        }
    }
}
