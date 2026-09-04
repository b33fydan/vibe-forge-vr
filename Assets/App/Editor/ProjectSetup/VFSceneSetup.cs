using System.Collections.Generic;
using System.Linq;
using Oculus.Interaction;
using Oculus.Interaction.Input;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using VibeForge.Runtime;

// Idempotent scene setup: builds Assets/App/Scenes/MainMR.unity with exactly
// one OVRCameraRig, passthrough underlay, OVR controller tracking plus an
// Interaction SDK HMD record, and a ray interactor on the RIGHT controller.
// Mirrors the SDKs' own wiring (ControllerRef/HmdRef injection plus
// InteractorGroup membership, as their editor wizards do).
// Safe to re-run; callable from the editor menu or batch mode.
public static class VFSceneSetup
{
    const string k_ScenePath = "Assets/App/Scenes/MainMR.unity";
    const string k_RigPrefab = "Packages/com.meta.xr.sdk.core/Prefabs/OVRCameraRig.prefab";
    const string k_ControllersPrefab =
        "Packages/com.meta.xr.sdk.interaction/Runtime/Prefabs/Controllers.prefab";
    const string k_HmdPrefab =
        "Packages/com.meta.xr.sdk.interaction/Runtime/Prefabs/Hmd.prefab";
    const string k_RayPrefab =
        "Packages/com.meta.xr.sdk.interaction/Runtime/Prefabs/Ray/ControllerRayInteractor.prefab";

    [MenuItem("VibeForge/Setup/Build MainMR Scene")]
    public static void EnsureFromMenu()
    {
        Ensure();
    }

    // Entry point for batch mode: -executeMethod VFSceneSetup.Ensure
    public static void Ensure()
    {
        Scene scene = GetOrCreateScene();

        OVRCameraRig rig = Object.FindFirstObjectByType<OVRCameraRig>();
        if (rig == null)
        {
            GameObject rigPrefab = LoadPrefab(k_RigPrefab);
            if (rigPrefab == null)
            {
                return;
            }

            rig = ((GameObject)PrefabUtility.InstantiatePrefab(rigPrefab)).GetComponent<OVRCameraRig>();
            Debug.Log("VF_SCENE_ADD OVRCameraRig");
        }

        CullStrayCamerasAndListeners(rig.transform);

        Transform trackingSpace = rig.trackingSpace;
        if (trackingSpace == null)
        {
            Debug.LogError("VF_SCENE_FAIL rig has no TrackingSpace");
            EditorApplication.Exit(1);
            return;
        }

        Controller rightController = FindController(Handedness.Right);
        if (rightController == null)
        {
            GameObject controllersPrefab = LoadPrefab(k_ControllersPrefab);
            if (controllersPrefab == null)
            {
                return;
            }

            var controllers =
                (GameObject)PrefabUtility.InstantiatePrefab(controllersPrefab, trackingSpace);
            controllers.transform.localPosition = Vector3.zero;
            controllers.transform.localRotation = Quaternion.identity;
            Debug.Log("VF_SCENE_ADD Controllers");
            rightController = FindController(Handedness.Right);
        }

        if (rightController == null)
        {
            Debug.LogError("VF_SCENE_FAIL no right Controller after setup");
            EditorApplication.Exit(1);
            return;
        }

        Hmd hmd = Object.FindFirstObjectByType<Hmd>();
        if (hmd == null)
        {
            GameObject hmdPrefab = LoadPrefab(k_HmdPrefab);
            if (hmdPrefab == null)
            {
                return;
            }

            var hmdGo = (GameObject)PrefabUtility.InstantiatePrefab(hmdPrefab, trackingSpace);
            hmdGo.transform.localPosition = Vector3.zero;
            hmdGo.transform.localRotation = Quaternion.identity;
            hmd = hmdGo.GetComponent<Hmd>();
            Debug.Log("VF_SCENE_ADD Hmd");
        }

        EnsureRightRay(rightController, hmd);
        EnsurePassthrough();
        EnsurePassthroughCamera(rig);
        EnsureBuildScene();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("VF_SCENE_OK scene=" + k_ScenePath);
    }

    static Scene GetOrCreateScene()
    {
        Scene active = EditorSceneManager.GetActiveScene();
        if (active.IsValid() && GetScenePath(active) == k_ScenePath)
        {
            return active;
        }

        Scene existing = EditorSceneManager.GetSceneByPath(k_ScenePath);
        if (existing.IsValid())
        {
            EditorSceneManager.OpenScene(k_ScenePath, OpenSceneMode.Single);
            return EditorSceneManager.GetActiveScene();
        }

        AssetDatabase.Refresh();
        var created = AssetDatabase.LoadAssetAtPath<SceneAsset>(k_ScenePath);
        if (created != null)
        {
            EditorSceneManager.OpenScene(k_ScenePath, OpenSceneMode.Single);
            return EditorSceneManager.GetActiveScene();
        }

        Scene fresh = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        EditorSceneManager.SaveScene(fresh, k_ScenePath);
        Debug.Log("VF_SCENE_ADD scene=" + k_ScenePath);
        return EditorSceneManager.GetSceneByPath(k_ScenePath);
    }

    static string GetScenePath(Scene scene)
    {
        return string.IsNullOrEmpty(scene.path) ? null : scene.path;
    }

    static GameObject LoadPrefab(string path)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogError("VF_SCENE_FAIL missing prefab " + path);
            EditorApplication.Exit(1);
        }

        return prefab;
    }

    static void CullStrayCamerasAndListeners(Transform rigRoot)
    {
        foreach (Camera camera in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
        {
            if (!camera.transform.IsChildOf(rigRoot))
            {
                Object.DestroyImmediate(camera.gameObject);
                Debug.Log("VF_SCENE_REMOVE stray camera");
            }
        }

        foreach (AudioListener listener in Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None))
        {
            if (!listener.transform.IsChildOf(rigRoot))
            {
                Object.DestroyImmediate(listener.gameObject);
                Debug.Log("VF_SCENE_REMOVE stray audio listener");
            }
        }
    }

    static Controller FindController(Handedness handedness)
    {
        // NOTE: never read Controller.Handedness in the editor; the getter
        // throws when the input data stack is unwired. The Controllers
        // prefab names its roots deterministically instead.
        string want = handedness == Handedness.Right ? "RightController" : "LeftController";
        Scene scene = EditorSceneManager.GetActiveScene();
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Controller found = FindNamedController(root.transform, want);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    static Controller FindNamedController(Transform transform, string want)
    {
        if (transform.name == want)
        {
            Controller controller = transform.GetComponent<Controller>();
            if (controller != null)
            {
                return controller;
            }
        }

        foreach (Transform child in transform)
        {
            Controller found = FindNamedController(child, want);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    static void EnsureRightRay(Controller rightController, Hmd hmd)
    {
        Transform holder = rightController.transform.Find("ControllerInteractors");
        if (holder == null)
        {
            holder = rightController.transform;
        }

        if (holder.GetComponentInChildren<RayInteractor>() != null)
        {
            return;
        }

        GameObject rayPrefab = LoadPrefab(k_RayPrefab);
        if (rayPrefab == null)
        {
            return;
        }

        var rayGo = (GameObject)PrefabUtility.InstantiatePrefab(rayPrefab, holder);
        rayGo.transform.localPosition = Vector3.zero;
        rayGo.transform.localRotation = Quaternion.identity;

        var controllerRef = rayGo.GetComponent<ControllerRef>();
        if (controllerRef != null)
        {
            controllerRef.InjectController(rightController);
        }

        var hmdRef = rayGo.GetComponent<HmdRef>();
        if (hmdRef != null && hmd != null)
        {
            hmdRef.InjectHmd(hmd);
        }

        InteractorGroup group = holder.GetComponent<InteractorGroup>();
        IInteractor interactor = rayGo.GetComponent<IInteractor>();
        if (group != null && interactor != null)
        {
            var members = new List<IInteractor>();
            if (group.Interactors != null)
            {
                members.AddRange(group.Interactors);
            }

            members.Add(interactor);
            group.InjectInteractors(members);
            EditorUtility.SetDirty(group);
        }

        Debug.Log("VF_SCENE_ADD right ControllerRayInteractor");
    }

    static void EnsurePassthrough()
    {
        if (Object.FindFirstObjectByType<OVRPassthroughLayer>() != null)
        {
            return;
        }

        var go = new GameObject("Passthrough");
        OVRPassthroughLayer layer = go.AddComponent<OVRPassthroughLayer>();
        layer.overlayType = OVROverlay.OverlayType.Underlay;
        Debug.Log("VF_SCENE_ADD Passthrough underlay");
    }

    static void EnsurePassthroughCamera(OVRCameraRig rig)
    {
        Camera[] eyes =
        {
            rig.centerEyeAnchor != null ? rig.centerEyeAnchor.GetComponent<Camera>() : null,
            rig.leftEyeAnchor != null ? rig.leftEyeAnchor.GetComponent<Camera>() : null,
            rig.rightEyeAnchor != null ? rig.rightEyeAnchor.GetComponent<Camera>() : null,
        };
        if (eyes[0] == null || eyes[1] == null || eyes[2] == null)
        {
            Debug.LogError("VF_SCENE_FAIL rig eye anchors missing cameras");
            EditorApplication.Exit(1);
            return;
        }

        var camera = Object.FindFirstObjectByType<VFPassthroughCamera>();
        GameObject go;
        if (camera == null)
        {
            go = new GameObject("PassthroughCamera");
            camera = go.AddComponent<VFPassthroughCamera>();
            Debug.Log("VF_SCENE_ADD PassthroughCamera");
        }
        else
        {
            go = camera.gameObject;
        }

        camera.Inject(eyes);
        EditorUtility.SetDirty(go);
    }

    static void EnsureBuildScene()
    {
        List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
        if (scenes.Any(s => s.path == k_ScenePath))
        {
            return;
        }

        scenes.Add(new EditorBuildSettingsScene(k_ScenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log("VF_SCENE_ADD build scene enabled");
    }
}
