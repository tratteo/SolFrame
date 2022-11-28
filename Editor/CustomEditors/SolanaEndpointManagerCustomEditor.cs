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

            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", target, typeof(SerializedProperty), true);

            GUI.enabled = true;
            EditorGUILayout.LabelField("Endpoint", EditorStyles.boldLabel);
            serProp = PropertyField("useCustomEndpoint", "Use custom endpoints");
            if (serProp.boolValue)
            {
                serProp = PropertyField("customEndpoint", "Endpoint");
                serProp = PropertyField("customStreamingEndpoint", "Streaming endpoint");
            }
            else
            {
                serProp = PropertyField("cluster", "Cluster");
            }
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Streaming", EditorStyles.boldLabel);
            serProp = PropertyField("connectOnInit", "Connect on private set", "Connect to the WebSocket on initialization");

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Batch", EditorStyles.boldLabel);
            serProp = PropertyField("batchAutoExecuteMode", "Batch execution mode", "How to trigger the execution of the batch composer");
            if (serProp.enumValueIndex != (int)BatchAutoExecuteMode.Manual)
            {
                serProp = PropertyField("triggerCount", "Trigger count", "Number of requests that trigger the batch execution");
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
            serProp = PropertyField("enableLogs", "Enable logs");
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