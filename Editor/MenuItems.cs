using UnityEditor;
using UnityEngine;

namespace SolFrame.Editor
{
    internal class MenuItems
    {
        [MenuItem("SolFrame/Credits", false, 22)]
        internal static void ShowPathBuilderWindow()
        {
            EditorWindow.GetWindow(typeof(CreditsEditorWindow)).titleContent = new GUIContent("SolFrame");
        }
    }
}