using UnityEditor;
using UnityEngine;

namespace SolFrame.Editor
{
    [CustomEditor(typeof(SolanaEndpointManager))]
    internal class SolanaEndpointManagerCustomEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            SerializedProperty serProp;
            EditorGUILayout.LabelField("Singleton", EditorStyles.boldLabel);
            serProp = PropertyField("dontDestroyOnLoad", "Dont destroy on load");
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Endpoint", EditorStyles.boldLabel);
            serProp = PropertyField("useCustomEndpoint", "Use custom endpoint");
            if (serProp.boolValue)
            {
                serProp = PropertyField("customEndpoint", "Endpoint");
            }
            else
            {
                serProp = PropertyField("cluster", "Cluster");
            }
            serializedObject.ApplyModifiedProperties();
        }

        private SerializedProperty PropertyField(string propName, string label = null, string tooltip = null)
        {
            var prop = serializedObject.FindProperty(propName);
            label ??= propName;
            tooltip ??= string.Empty;
            EditorGUILayout.PropertyField(prop, new GUIContent(label, tooltip));
            return prop;
        }
    }
}