using UnityEditor;
using UnityEngine;

namespace SolFrame.Editor
{
    internal class CreditsEditorWindow : EditorWindow
    {
        private GUIStyle titleStyle;
        private GUIStyle textStyle;

        private void OnEnable()
        {
            maxSize = new Vector2(400, 512);
            minSize = new Vector2(400, 512);
            titleStyle = new GUIStyle()
            {
                fontSize = 16,
                richText = true,
                clipping = TextClipping.Clip,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                imagePosition = ImagePosition.ImageAbove
            };
            titleStyle.normal.textColor = new Color(20 / 255F, 220 / 255F, 165 / 255F);

            textStyle = new GUIStyle()
            {
                alignment = TextAnchor.MiddleLeft,
                imagePosition = ImagePosition.ImageLeft,
                richText = true,
                fontSize = 14
            };
            textStyle.normal.textColor = Color.white;
        }

        private void OnGUI()
        {
            var currentY = 0F;
            var titlePos = new Rect(0, 25, position.width, 256);
            GUI.Label(titlePos, new GUIContent() { image = Resources.Solframe, text = "\nSolFrame by Siamango" }, titleStyle);
            currentY += titlePos.y + titlePos.height;
            currentY += 15;
            var githubPos = new Rect(position.width / 2F - 24, currentY, 48, 48);
            if (GUI.Button(githubPos, Resources.Github, GUI.skin.button))
            {
                Application.OpenURL("https://github.com/Siamango/SolFrame");
            }
            currentY += githubPos.height;
            currentY += 25;

            var donationsRect = new Rect(20, currentY, position.width, 20);
            GUI.Label(donationsRect, new GUIContent() { image = Resources.Solana, text = "Donations:" }, textStyle);
            currentY += donationsRect.height;
            currentY += 10;

            var addressRect = new Rect(20, currentY, position.width, 20);
            var addressStyle = new GUIStyle() { fontSize = 12 };
            addressStyle.normal.textColor = Color.white;
            currentY += 2;
            GUI.Label(addressRect, Resources.FundPublicKey, addressStyle);
            currentY += addressRect.height;
            var copyButtonRect = new Rect(20, currentY, position.width - 40, 20);
            if (GUI.Button(copyButtonRect, "Copy", GUI.skin.button))
            {
                GUIUtility.systemCopyBuffer = Resources.FundPublicKey;
                ShowNotification(new GUIContent() { text = "Copied to clipboard", image = Resources.HeartSmall }, 0.5F);
            }
        }
    }
}