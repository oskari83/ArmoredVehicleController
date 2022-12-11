using UnityEditor;
using UnityEngine;
using System.Diagnostics;

public class MMV_Editor : EditorWindow
{
    private static string bannerPath = "Assets/MMV/System/Scripts/Editor/img/mmv-main-page.png";
    private static Texture banner;

    [MenuItem("MMV/MMV")]
    private static void ShowWindow()
    {
        banner = (Texture)AssetDatabase.LoadAssetAtPath(bannerPath, typeof(Texture));

        if (banner == null)
        {
            UnityEngine.Debug.LogError("banner texture not found");
        }

        var window = GetWindow<MMV_Editor>();
        window.titleContent = new GUIContent("Modern Military Vehicle");

        window.Show();
        window.maxSize = new Vector2(banner.width * 0.5f, (banner.height * 0.5f) + 100);
        window.minSize = window.maxSize;
    }

    private void OnGUI()
    {
        if (banner != null)
        {
            var textureStyle = new GUIStyle();

            textureStyle.fixedHeight = banner.height * 0.5f;
            textureStyle.fixedWidth = banner.width * 0.5f;

            GUILayout.Box((Texture2D)banner, textureStyle);
        }

        if (GUILayout.Button("Docs"))
        {
            Process.Start("https://mmv-docs.readthedocs.io/");
        }

        if (GUILayout.Button("Tutorials"))
        {
            Process.Start("https://www.youtube.com/watch?v=Am2GUjTNHls&list=PLHpkt9fDv8d8N3xUk_dTgQSnpcTYzsMXs");
        }

        if (GUILayout.Button("Report bug or request implementation"))
        {
            Process.Start("https://github.com/RuanLucasGD/MMV-Docs/issues");
        }
    }
}