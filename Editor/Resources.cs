using UnityEditor;
using UnityEngine;

namespace SolFrame.Editor
{
    internal static class Resources
    {
        public const string FundPublicKey = "9wMKssWkgmkzPLFyvLaqAPGnx9hZzNhQwTSihdApGBMu";
        public const string Path = "Packages/com.siamango.solframe/Editor/Icons";
        public static readonly Texture2D Solframe = AssetDatabase.LoadAssetAtPath<Texture2D>($"{Path}/solframe.png");
        public static readonly Texture2D Github = AssetDatabase.LoadAssetAtPath<Texture2D>($"{Path}/github.png");
        public static readonly Texture2D Solana = AssetDatabase.LoadAssetAtPath<Texture2D>($"{Path}/solana.png");
        public static readonly Texture2D HeartSmall = AssetDatabase.LoadAssetAtPath<Texture2D>($"{Path}/heart_small.png");
    }
}