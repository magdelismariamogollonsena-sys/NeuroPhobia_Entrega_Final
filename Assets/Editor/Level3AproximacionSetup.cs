#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class Level3AproximacionSetup
{
    [MenuItem("Tools/NeuroPhobia/3. Configurar Nivel3 Aproximacion Progresiva")]
    public static void Configure()
    {
        string path = "Assets/Scenes/Nivel3.unity";
        if (File.Exists(path) && SceneManager.GetActiveScene().name != "Nivel3")
            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

        GameObject oldHud = GameObject.Find("CanvasHUD_Nivel3");
        if (oldHud != null) Object.DestroyImmediate(oldHud);

        GameObject root = GameObject.Find("Nivel3_AutoSetup") ?? new GameObject("Nivel3_AutoSetup");
        GameObject spider = FindSpider();
        GameObject xrOrigin = GameObject.Find("XR Origin (XR Rig)");
        Camera cam = Camera.main;

        GameObject hudGo = CreateHUD(root.transform, cam);
        Level3HUD hud = hudGo.GetComponent<Level3HUD>();

        Level3SpiderMovement movement = spider.GetComponent<Level3SpiderMovement>() ?? spider.AddComponent<Level3SpiderMovement>();
        Level3ProgressController progress = root.GetComponent<Level3ProgressController>() ?? root.AddComponent<Level3ProgressController>();
        MetricsCollector metrics = root.GetComponent<MetricsCollector>() ?? root.AddComponent<MetricsCollector>();

        metrics.level = 3;
        metrics.player = cam != null ? cam.transform : null;
        metrics.spider = spider.transform;

        progress.hud = hud;
        progress.spiderMovement = movement;
        progress.metricsCollector = metrics;

        CreateCheckpoint(root.transform, "Checkpoint_3m", 1, spider.transform, 3f, progress);
        CreateCheckpoint(root.transform, "Checkpoint_1m", 2, spider.transform, 1f, progress);

        Level3AvoidanceMonitor monitor = root.GetComponent<Level3AvoidanceMonitor>() ?? root.AddComponent<Level3AvoidanceMonitor>();
        monitor.playerRoot = xrOrigin != null ? xrOrigin.transform : null;
        monitor.spiderTarget = spider.transform;
        monitor.hud = hud;

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Nivel 3", "HUD reconstruido correctamente.", "OK");
    }

    private static GameObject FindSpider()
    {
        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            string n = go.name.ToLowerInvariant();
            if (n.Contains("spider") || n.Contains("arana") || n.Contains("araña"))
                return go;
        }

        GameObject fallback = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        fallback.name = "CaveSpider_Nivel3_SlowPatrol";
        fallback.transform.position = new Vector3(0f, 0.35f, 5f);
        return fallback;
    }

    private static void CreateCheckpoint(Transform parent, string name, int order, Transform spider, float distance, Level3ProgressController progress)
    {
        GameObject checkpoint = GameObject.Find(name) ?? GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        checkpoint.name = name;
        checkpoint.transform.SetParent(parent);
        checkpoint.transform.position = spider.position + Vector3.back * distance;
        checkpoint.transform.localScale = new Vector3(1.2f, 0.08f, 1.2f);

        Collider col = checkpoint.GetComponent<Collider>();
        col.isTrigger = true;

        Renderer r = checkpoint.GetComponent<Renderer>();
        if (r != null) r.enabled = false;

        Level3Checkpoint cp = checkpoint.GetComponent<Level3Checkpoint>() ?? checkpoint.AddComponent<Level3Checkpoint>();
        cp.checkpointOrder = order;
        cp.progressController = progress;
    }

    private static GameObject CreateHUD(Transform parent, Camera cam)
    {
        GameObject canvasGo = new GameObject(
            "CanvasHUD_Nivel3",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster),
            typeof(Level3HUD)
        );

        canvasGo.transform.SetParent(parent, false);
        canvasGo.transform.position = new Vector3(0f, 2.1f, 2.4f);
        canvasGo.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        canvasGo.transform.localScale = Vector3.one * 0.006f;

        RectTransform canvasRect = canvasGo.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(700f, 380f);

        Canvas canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = cam;

        Level3HUD hud = canvasGo.GetComponent<Level3HUD>();
        hud.instructionText = CreateText(canvasGo.transform, "InstructionText", new Vector2(0, 120), 28);
        hud.checkpointText = CreateText(canvasGo.transform, "CheckpointText", new Vector2(0, 35), 26);
        hud.warningText = CreateText(canvasGo.transform, "WarningText", new Vector2(0, -50), 24);
        hud.completionText = CreateText(canvasGo.transform, "CompletionText", new Vector2(0, -115), 26);
        hud.continueButton = CreateButton(canvasGo.transform, hud);
        hud.nextSceneName = "Nivel4";

        hud.warningText.gameObject.SetActive(false);
        hud.completionText.gameObject.SetActive(false);
        hud.continueButton.gameObject.SetActive(false);

        return canvasGo;
    }

    private static TMP_Text CreateText(Transform parent, string name, Vector2 pos, int fontSize)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(650f, 80f);

        TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = fontSize;
        text.color = Color.white;
        return text;
    }

    private static Button CreateButton(Transform parent, Level3HUD hud)
    {
        GameObject go = new GameObject("Button_Continuar_Nivel4", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0f, -180f);
        rt.sizeDelta = new Vector2(300f, 60f);

        Button button = go.GetComponent<Button>();

        GameObject textGo = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textGo.transform.SetParent(go.transform, false);

        RectTransform tr = textGo.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.offsetMin = Vector2.zero;
        tr.offsetMax = Vector2.zero;

        TextMeshProUGUI label = textGo.GetComponent<TextMeshProUGUI>();
        label.text = "Continuar a Nivel 4";
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 24;
        label.color = Color.black;

        UnityEventTools.AddPersistentListener(button.onClick, hud.GoToNextLevel);
        return button;
    }
}
#endif
