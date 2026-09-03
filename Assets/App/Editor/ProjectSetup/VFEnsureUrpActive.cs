using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Idempotent project setup: activates URP with a default renderer and a
// Linear color space, mirroring the Universal 3D template layout.
// Safe to re-run; callable from the editor menu or batch mode.
public static class VFEnsureUrpActive
{
    const string k_AppDir = "Assets/App";
    const string k_SettingsDir = "Assets/App/Settings";
    const string k_PipelinePath = "Assets/App/Settings/UniversalRenderPipelineAsset.asset";
    const string k_RendererPath = "Assets/App/Settings/UniversalRendererData.asset";

    [MenuItem("VibeForge/Setup/Ensure URP Active")]
    public static void EnsureFromMenu()
    {
        Ensure();
    }

    // Entry point for batch mode: -executeMethod VFEnsureUrpActive.Ensure
    public static void Ensure()
    {
        if (GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset)
        {
            Debug.Log("VF_URP_OK already-active");
            return;
        }

        if (!AssetDatabase.IsValidFolder(k_AppDir))
        {
            Debug.LogError("VF_URP_FAIL missing " + k_AppDir);
            EditorApplication.Exit(1);
            return;
        }

        if (!AssetDatabase.IsValidFolder(k_SettingsDir))
        {
            AssetDatabase.CreateFolder(k_AppDir, "Settings");
        }

        UniversalRendererData renderer =
            AssetDatabase.LoadAssetAtPath<UniversalRendererData>(k_RendererPath);
        if (renderer == null)
        {
            renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
            AssetDatabase.CreateAsset(renderer, k_RendererPath);
        }

        UniversalRenderPipelineAsset pipeline =
            AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(k_PipelinePath);
        if (pipeline == null)
        {
            pipeline = UniversalRenderPipelineAsset.Create(renderer);
            AssetDatabase.CreateAsset(pipeline, k_PipelinePath);
        }

        GraphicsSettings.renderPipelineAsset = pipeline;
        PlayerSettings.colorSpace = ColorSpace.Linear;
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("VF_URP_OK active pipeline=" + k_PipelinePath);
    }
}
