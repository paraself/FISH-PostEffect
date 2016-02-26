using System;
using UnityEditor;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
	[CustomEditor (typeof(FISH_PostEffect))]
    class VignetteAndChromaticAberrationEditor : Editor
    {
        private SerializedObject m_SerObj;
        private SerializedProperty m_Intensity;             // intensity == 0 disables pre pass (optimization)
       	private SerializedProperty m_Blur;                  // blur == 0 disables blur pass (optimization)
        private SerializedProperty m_BlurSpread;
		private SerializedProperty m_Iteration;
		private SerializedProperty m_multiplyColor;



        void OnEnable ()
        {
            m_SerObj = new SerializedObject (target);
			m_Intensity = m_SerObj.FindProperty ("intensity");
			m_Blur = m_SerObj.FindProperty ("blur");
            m_BlurSpread = m_SerObj.FindProperty ("blurSpread");
			m_Iteration = m_SerObj.FindProperty ("iteration");
			m_multiplyColor = m_SerObj.FindProperty ("multiplyColor");
        }


        public override void OnInspectorGUI ()
        {
            m_SerObj.Update ();

            EditorGUILayout.HelpBox("Corner blur and vignetting",MessageType.None);

            EditorGUILayout.PropertyField (m_Intensity, new GUIContent("Vignetting"));
            EditorGUILayout.PropertyField (m_Blur, new GUIContent("Blurred Corners"));
            if (m_Blur.floatValue>0.0f)
                EditorGUILayout.PropertyField (m_BlurSpread, new GUIContent("Blur Distance"));
			EditorGUILayout.IntSlider (m_Iteration, 1, 5, new GUIContent("Iteration"));
			EditorGUILayout.PropertyField (m_multiplyColor, new GUIContent("Multiply Color"));

            m_SerObj.ApplyModifiedProperties();
        }
    }
}
