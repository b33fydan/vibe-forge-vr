using UnityEditor;
using UnityEditor.XR.Management;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;

// Idempotent project setup: switches to the Android build target and assigns
// the OpenXR loader for Android. The Meta XR feature set enables itself on
// editor load via Meta's MetaXRFeatureEnabler, which is verified separately.
// Safe to re-run; callable from the editor menu or batch mode.
public static class VFQuestSetup
{
    const string k_ManagerPath = "Assets/App/Settings/XRManagerSettings.asset";

    [MenuItem("VibeForge/Setup/Configure Android XR")]
    public static void EnsureFromMenu()
    {
        Ensure();
    }

    // Entry point for batch mode: -executeMethod VFQuestSetup.Ensure
    public static void Ensure()
    {
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            Debug.Log("VF_QUEST_SWITCHING activeTarget=" + EditorUserBuildSettings.activeBuildTarget);
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
            {
                Debug.LogError("VF_QUEST_FAIL build target switch refused");
                EditorApplication.Exit(1);
                return;
            }

            AssetDatabase.SaveAssets();
            Debug.Log("VF_QUEST_SWITCHED target=Android (re-run to configure XR)");
            return;
        }

        XRGeneralSettingsPerBuildTarget perTarget = FindOrCreatePerTarget();
        if (perTarget == null)
        {
            Debug.LogError("VF_QUEST_FAIL no XR per-target settings asset");
            EditorApplication.Exit(1);
            return;
        }

        if (!perTarget.HasSettingsForBuildTarget(BuildTargetGroup.Android))
        {
            perTarget.CreateDefaultSettingsForBuildTarget(BuildTargetGroup.Android);
        }

        XRGeneralSettings general = perTarget.SettingsForBuildTarget(BuildTargetGroup.Android);
        if (general == null)
        {
            Debug.LogError("VF_QUEST_FAIL no XR general settings for Android");
            EditorApplication.Exit(1);
            return;
        }

        general.InitManagerOnStart = true;

        XRManagerSettings manager = general.AssignedSettings;
        if (manager == null)
        {
            manager = ScriptableObject.CreateInstance<XRManagerSettings>();
            AssetDatabase.CreateAsset(manager, k_ManagerPath);
            general.AssignedSettings = manager;
        }

        if (!HasLoader(manager))
        {
            var loader = ScriptableObject.CreateInstance<OpenXRLoader>();
            AssetDatabase.AddObjectToAsset(loader, manager);
            if (!manager.TryAddLoader(loader))
            {
                Debug.LogError("VF_QUEST_FAIL TryAddLoader refused OpenXRLoader");
                EditorApplication.Exit(1);
                return;
            }
        }

        EditorUtility.SetDirty(general);
        EditorUtility.SetDirty(manager);
        AssetDatabase.SaveAssets();
        Debug.Log("VF_QUEST_OK target=Android loader=OpenXR initOnStart=true");

        ApplyRequiredFixes();
        EnsureInsightPassthrough();

        VerifyMetaFeatureSet();
    }

    // Insight passthrough stays dark without this: the flag declares the
    // com.oculus.feature.PASSTHROUGH feature and lets the runtime start
    // the passthrough service the underlay layer composites.
    static void EnsureInsightPassthrough()
    {
        OVRProjectConfig config = OVRProjectConfig.CachedProjectConfig;
        if (config == null)
        {
            Debug.LogError("VF_PASSTHROUGH_FAIL no OVRProjectConfig");
            EditorApplication.Exit(1);
            return;
        }

        if (config.insightPassthroughSupport ==
            OVRProjectConfig.FeatureSupport.None)
        {
            config.insightPassthroughSupport =
                OVRProjectConfig.FeatureSupport.Supported;
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            Debug.Log("VF_PASSTHROUGH_OK insightPassthroughSupport=Supported");
        }
        else
        {
            Debug.Log("VF_PASSTHROUGH known=" + config.insightPassthroughSupport);
        }
    }

    // Ensures Meta's OpenXR feature set is enabled for Android, mirroring
    // Meta's own MetaXRFeatureEnabler (whose load hook does not fire
    // reliably in batch mode). Standalone is best-effort only.
    static void VerifyMetaFeatureSet()
    {
        try
        {
            var featureSet = OpenXRFeatureSetManager.GetFeatureSetWithId(
                BuildTargetGroup.Android, "com.meta.openxr.featureset.metaxr");
            if (featureSet == null)
            {
                Debug.LogError("VF_FEATURESET_FAIL unknown feature set");
                EditorApplication.Exit(1);
                return;
            }

            if (!featureSet.isEnabled)
            {
                featureSet.isEnabled = true;
                OpenXRFeatureSetManager.SetFeaturesFromEnabledFeatureSets(BuildTargetGroup.Android);
                Debug.Log("VF_FEATURESET_ENABLED group=Android");
            }
            else
            {
                Debug.Log("VF_FEATURESET known=true enabled=True");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("VF_FEATURESET_FAIL " + e.GetType().Name + ": " + e.Message);
            EditorApplication.Exit(1);
        }
    }

    // Applies only the two outstanding REQUIRED Project Setup tasks:
    // minimum Android API 32 and the single GameActivity entry.
    // Recommended tasks stay untouched until explicitly reviewed.
    static void ApplyRequiredFixes()
    {
        if ((int)PlayerSettings.Android.minSdkVersion < 32)
        {
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel32;
            Debug.Log("VF_FIX_OK minSdkVersion=32");
        }

        if (PlayerSettings.Android.applicationEntry != AndroidApplicationEntry.GameActivity)
        {
            PlayerSettings.Android.applicationEntry = AndroidApplicationEntry.GameActivity;
            Debug.Log("VF_FIX_OK applicationEntry=GameActivity");
        }

        AssetDatabase.SaveAssets();
    }

    static XRGeneralSettingsPerBuildTarget FindOrCreatePerTarget()
    {
        if (EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.settingsKey, out Object configured))
        {
            var existing = configured as XRGeneralSettingsPerBuildTarget;
            if (existing != null)
            {
                return existing;
            }
        }

        string[] guids = AssetDatabase.FindAssets("t:XRGeneralSettingsPerBuildTarget");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<XRGeneralSettingsPerBuildTarget>(path);
            if (asset != null)
            {
                EditorBuildSettings.AddConfigObject(XRGeneralSettings.settingsKey, asset, true);
                return asset;
            }
        }

        var created = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
        AssetDatabase.CreateAsset(created, "Assets/App/Settings/XRGeneralSettingsPerBuildTarget.asset");
        EditorBuildSettings.AddConfigObject(XRGeneralSettings.settingsKey, created, true);
        return created;
    }

    static bool HasLoader(XRManagerSettings manager)
    {
        string path = AssetDatabase.GetAssetPath(manager);
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }

        Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(path);
        foreach (Object subAsset in subAssets)
        {
            if (subAsset is OpenXRLoader)
            {
                return true;
            }
        }

        return false;
    }
}
