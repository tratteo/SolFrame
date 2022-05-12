using Solnet.Rpc.Types;
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
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Streaming", EditorStyles.boldLabel);
            serProp = PropertyField("connectOnInit", "Connect on init", "Connect to the WebSocket on initialization");

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Batch", EditorStyles.boldLabel);
            serProp = PropertyField("batchAutoExecuteMode", "Batch execution mode", "How to trigger the execution of the batch composer");
            if (serProp.enumValueIndex != (int)BatchAutoExecuteMode.Manual)
            {
                serProp = PropertyField("triggerCount", "Trigger count", "Number of requests that trigger the batch execution");
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