using Oculus.Interaction;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VibeForge.Terminal;

// Idempotent terminal setup: builds Assets/App/Prefabs/FloatingTerminal.prefab
// (Breadstick instrument-panel styling, 0.90 x 0.52 m world-space Canvas),
// places one instance in MainMR, and wires every serialized reference in code.
// Safe to re-run; callable from the editor menu or batch mode.
public static class VFTerminalSetup
{
    const string k_ScenePath = "Assets/App/Scenes/MainMR.unity";
    const string k_PrefabPath = "Assets/App/Prefabs/FloatingTerminal.prefab";

    const float k_CanvasScale = 0.001f;
    const float k_Width = 900f;
    const float k_Height = 520f;

    static readonly Color k_Gold = new Color(0.85f, 0.68f, 0.25f, 0.80f);
    static readonly Color k_Panel = new Color(0.09f, 0.08f, 0.07f, 0.91f);
    static readonly Color k_Text = new Color(0.93f, 0.90f, 0.85f, 1f);
    static readonly Color k_DimText = new Color(0.55f, 0.52f, 0.47f, 1f);
    static readonly Color k_Button = new Color(0.20f, 0.18f, 0.15f, 1f);

    [MenuItem("VibeForge/Setup/Build Floating Terminal")]
    public static void EnsureFromMenu()
    {
        Ensure();
    }

    // Entry point for batch mode: -executeMethod VFTerminalSetup.Ensure
    public static void Ensure()
    {
        EditorSceneManager.OpenScene(k_ScenePath, OpenSceneMode.Single);
        EnsureEventSystem();

        GameObject built = BuildPanel();
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(built, k_PrefabPath);
        Object.DestroyImmediate(built);
        if (prefab == null)
        {
            Debug.LogError("VF_TERMINAL_FAIL prefab save failed");
            EditorApplication.Exit(1);
            return;
        }

        FloatingTerminalController existing =
            Object.FindFirstObjectByType<FloatingTerminalController>();
        if (existing != null)
        {
            Object.DestroyImmediate(existing.gameObject);
        }

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        WireSceneReferences(instance);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Debug.Log("VF_TERMINAL_OK prefab=" + k_PrefabPath);
    }

    static void EnsureEventSystem()
    {
        EventSystem events = Object.FindFirstObjectByType<EventSystem>();
        if (events == null)
        {
            events = new GameObject("EventSystem",
                typeof(EventSystem), typeof(PointableCanvasModule)).GetComponent<EventSystem>();
            Debug.Log("VF_TERMINAL_ADD EventSystem+PointableCanvasModule");
            return;
        }

        if (events.GetComponent<PointableCanvasModule>() == null)
        {
            events.gameObject.AddComponent<PointableCanvasModule>();
            Debug.Log("VF_TERMINAL_ADD PointableCanvasModule");
        }
    }

    static GameObject BuildPanel()
    {
        var root = new GameObject("FloatingTerminal");
        root.AddComponent<CanvasGroup>();
        Rigidbody body = root.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;

        var panel = new GameObject("Panel");
        panel.transform.SetParent(root.transform, false);

        Canvas canvas = panel.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.additionalShaderChannels =
            AdditionalCanvasShaderChannels.TexCoord1 |
            AdditionalCanvasShaderChannels.TexCoord2 |
            AdditionalCanvasShaderChannels.Normal |
            AdditionalCanvasShaderChannels.Tangent;
        panel.AddComponent<GraphicRaycaster>();
        var pointable = panel.AddComponent<PointableCanvas>();
        pointable.InjectCanvas(canvas);

        RectTransform canvasRect = panel.GetComponent<RectTransform>();
        canvasRect.anchorMin = new Vector2(0.5f, 0.5f);
        canvasRect.anchorMax = new Vector2(0.5f, 0.5f);
        canvasRect.pivot = new Vector2(0.5f, 0.5f);
        canvasRect.sizeDelta = new Vector2(k_Width, k_Height);
        panel.transform.localScale = Vector3.one * k_CanvasScale;

        Sprite uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        GameObject border = Mk("Border", panel.transform,
            new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0), new Vector2(0, 0));
        MkImage(border, k_Gold, uiSprite);

        GameObject background = Mk("Background", panel.transform,
            new Vector2(0, 0), new Vector2(1, 1), new Vector2(6, 6), new Vector2(-6, -6));
        MkImage(background, k_Panel, uiSprite);

        // Title bar doubles as the grab handle.
        GameObject titleBar = Mk("TitleBar", panel.transform,
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(12, -68), new Vector2(-12, -12));
        Image titleImage = MkImage(titleBar, new Color(0.13f, 0.12f, 0.10f, 1f), uiSprite);
        GameObject titleText = Mk("TitleText", titleBar.transform,
            new Vector2(0, 0), new Vector2(1, 1), new Vector2(16, 0), new Vector2(-16, 0));
        MkText(titleText, "● TERMINAL ZERO      LOCAL / READY", 20,
            TextAlignmentOptions.Left, false);
        var grabBox = titleBar.AddComponent<BoxCollider>();
        grabBox.size = new Vector3(0.876f, 0.056f, 0.05f);
        DistanceGrabInteractable grab = titleBar.AddComponent<DistanceGrabInteractable>();

        GameObject output = Mk("Output", panel.transform,
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(16, -360), new Vector2(-16, -76));
        TextMeshProUGUI outputText = MkText(output, string.Empty, 15,
            TextAlignmentOptions.TopLeft, true);

        GameObject inputBg = Mk("InputField", panel.transform,
            new Vector2(0, 0), new Vector2(1, 0), new Vector2(16, 76), new Vector2(-136, 128));
        MkImage(inputBg, new Color(0.05f, 0.05f, 0.045f, 1f), uiSprite);
        GameObject inputText = Mk("Text", inputBg.transform,
            new Vector2(0, 0), new Vector2(1, 1), new Vector2(12, 0), new Vector2(-12, 0));
        TextMeshProUGUI inputLabel = MkText(inputText, string.Empty, 18,
            TextAlignmentOptions.Left, false);
        GameObject placeholder = Mk("Placeholder", inputBg.transform,
            new Vector2(0, 0), new Vector2(1, 1), new Vector2(12, 0), new Vector2(-12, 0));
        TextMeshProUGUI placeholderLabel = MkText(placeholder, "> type command…", 18,
            TextAlignmentOptions.Left, false);
        placeholderLabel.color = k_DimText;
        TMP_InputField inputField = inputBg.AddComponent<TMP_InputField>();
        inputField.textViewport = inputBg.GetComponent<RectTransform>();
        inputField.textComponent = inputLabel;
        inputField.placeholder = placeholderLabel;
        inputField.lineType = TMP_InputField.LineType.SingleLine;
        inputField.characterLimit = 120;

        Button runButton = MkButton(panel.transform, "RUN", 20,
            new Vector2(1, 0), new Vector2(1, 0), new Vector2(-128, 76), new Vector2(-16, 128));
        Button helpButton = MkButton(panel.transform, "HELP", 20,
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(16, 12), new Vector2(221, 68));
        Button clearButton = MkButton(panel.transform, "CLEAR", 20,
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(233, 12), new Vector2(438, 68));
        Button recenterButton = MkButton(panel.transform, "RECENTER", 20,
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(450, 12), new Vector2(655, 68));
        Button closeButton = MkButton(panel.transform, "CLOSE", 20,
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(667, 12), new Vector2(872, 68));

        TerminalView view = panel.AddComponent<TerminalView>();
        view.Inject(outputText, inputField, border.GetComponent<Image>(), titleImage,
            helpButton, clearButton, recenterButton, runButton, closeButton);

        var summon = root.AddComponent<TerminalSummonController>();
        var controller = root.AddComponent<FloatingTerminalController>();
        var grabWatcher = root.AddComponent<TerminalGrabController>();
        grabWatcher.Inject(grab, view);
        controller.Inject(view, summon, null);
        summon.Inject(panel, null, root.GetComponent<CanvasGroup>());

        return root;
    }

    static void WireSceneReferences(GameObject instance)
    {
        OVRCameraRig rig = Object.FindFirstObjectByType<OVRCameraRig>();
        Camera eye = rig != null && rig.centerEyeAnchor != null
            ? rig.centerEyeAnchor.GetComponent<Camera>()
            : null;
        Canvas canvas = instance.GetComponentInChildren<Canvas>();
        if (eye != null)
        {
            canvas.worldCamera = eye;
        }

        var summon = instance.GetComponent<TerminalSummonController>();
        var controller = instance.GetComponent<FloatingTerminalController>();
        OVRPassthroughLayer passthrough = Object.FindFirstObjectByType<OVRPassthroughLayer>();
        TerminalView view = instance.GetComponentInChildren<TerminalView>();
        controller.Inject(view, summon, passthrough);
        summon.Inject(summonPanel(instance), HeadAnchor(rig), instance.GetComponent<CanvasGroup>());
        summon.PlaceInFrontOfHead();
    }

    static GameObject summonPanel(GameObject instance)
    {
        return instance.transform.Find("Panel").gameObject;
    }

    static Transform HeadAnchor(OVRCameraRig rig)
    {
        if (rig != null && rig.centerEyeAnchor != null)
        {
            return rig.centerEyeAnchor;
        }

        return null;
    }

    static GameObject Mk(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        return go;
    }

    static Image MkImage(GameObject go, Color color, Sprite sprite)
    {
        Image image = go.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        return image;
    }

    static TextMeshProUGUI MkText(GameObject go, string text, int size,
        TextAlignmentOptions align, bool wrap)
    {
        TextMeshProUGUI label = go.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = size;
        label.alignment = align;
        label.enableWordWrapping = wrap;
        label.raycastTarget = false;
        label.color = k_Text;
        return label;
    }

    static Button MkButton(Transform parent, string label, int fontSize,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        GameObject go = Mk(label + "Button", parent, anchorMin, anchorMax, offsetMin, offsetMax);
        Image image = MkImage(go, k_Button,
            AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"));
        GameObject textGo = Mk("Label", go.transform,
            new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
        TextMeshProUGUI text = MkText(textGo, label, fontSize,
            TextAlignmentOptions.Center, false);
        text.color = k_Text;
        Button button = go.AddComponent<Button>();
        button.targetGraphic = image;
        return button;
    }
}
