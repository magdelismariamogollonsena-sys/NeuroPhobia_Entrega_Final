#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class Level2DistanciaSeguraSetup
{
    [MenuItem("Tools/NeuroPhobia/2. Configurar Nivel2 Distancia Segura")]
    public static void ConfigureLevel2()
    {
        string path = "Assets/Scenes/Nivel2.unity";
        if (File.Exists(path) && SceneManager.GetActiveScene().name != "Nivel2")
        {
            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        }

        GameObject root = FindOrCreate("Nivel2_AutoSetup");
        GameObject spider = FindSpider();
        GameObject xrOrigin = GameObject.Find("XR Origin (XR Rig)");
        Camera cam = Camera.main;

        GameObject hudObject = FindOrCreateCanvas(root.transform);
        Level2HUD hud = ConfigureHUD(hudObject);

        GameObject zone = FindOrCreate("ZonaExclusion_8m", root.transform);
        ConfigureZone(zone, spider, xrOrigin, hud);

        Level2GazeTimer gaze = root.GetComponent<Level2GazeTimer>();
        if (gaze == null) gaze = root.AddComponent<Level2GazeTimer>();
        gaze.playerCamera = cam;
        gaze.spiderTarget = spider != null ? spider.transform : null;
        gaze.hud = hud;
        gaze.requiredGazeSeconds = 30f;
        gaze.gazeAngleThreshold = 10f;
        gaze.requireLineOfSight = false;

        MetricsCollector metrics = root.GetComponent<MetricsCollector>();
        if (metrics == null) metrics = root.AddComponent<MetricsCollector>();
        metrics.level = 2;
        metrics.player = cam != null ? cam.transform : null;
        metrics.spider = spider != null ? spider.transform : null;
        gaze.metricsCollector = metrics;

        if (spider != null)
        {
            AranaController arana = spider.GetComponent<AranaController>();
            if (arana == null) arana = spider.AddComponent<AranaController>();
            arana.defaultPattern = MovementPattern.IDLE;

            if (spider.GetComponentInChildren<Collider>() == null)
            {
                spider.AddComponent<SphereCollider>();
            }
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("Nivel 2", "Nivel 2 configurado. Ahora prueba Play.", "OK");
    }

    private static GameObject FindOrCreate(string name, Transform parent = null)
    {
        GameObject go = GameObject.Find(name);
        if (go != null) return go;

        go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent, false);
        return go;
    }

    private static GameObject FindSpider()
    {
        GameObject spider = GameObject.Find("CaveSpider_Nivel2_Static");
        if (spider != null) return spider;

        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            string lower = go.name.ToLowerInvariant();
            if (lower.Contains("spider") || lower.Contains("arana") || lower.Contains("araña"))
            {
                return go;
            }
        }

        return null;
    }

    private static GameObject FindOrCreateCanvas(Transform parent)
    {
        GameObject canvasObject = GameObject.Find("CanvasHUD_Nivel2");
        if (canvasObject == null)
        {
            canvasObject = new GameObject("CanvasHUD_Nivel2");
            canvasObject.transform.SetParent(parent, false);
            canvasObject.transform.position = new Vector3(0, 2.1f, 2.4f);
            canvasObject.transform.rotation = Quaternion.Euler(0, 180, 0);
            canvasObject.transform.localScale = Vector3.one * 0.006f;
        }

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        if (canvas == null) canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;

        RectTransform rt = canvasObject.GetComponent<RectTransform>();
        if (rt == null) rt = canvasObject.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(650, 360);

        if (canvasObject.GetComponent<CanvasScaler>() == null) canvasObject.AddComponent<CanvasScaler>();
        if (canvasObject.GetComponent<GraphicRaycaster>() == null) canvasObject.AddComponent<GraphicRaycaster>();

        return canvasObject;
    }

    private static Level2HUD ConfigureHUD(GameObject canvasObject)
    {
        Level2HUD hud = canvasObject.GetComponent<Level2HUD>();
        if (hud == null) hud = canvasObject.AddComponent<Level2HUD>();

        TMP_Text instruction = CreateTMP(canvasObject.transform, "InstructionText", new Vector2(0, 125), new Vector2(620, 90), 28);
        TMP_Text progress = CreateTMP(canvasObject.transform, "GazeProgressText", new Vector2(0, 35), new Vector2(620, 80), 24);
        TMP_Text warning = CreateTMP(canvasObject.transform, "WarningText", new Vector2(0, -55), new Vector2(620, 55), 24);
        TMP_Text complete = CreateTMP(canvasObject.transform, "CompletionText", new Vector2(0, -115), new Vector2(620, 60), 26);

        Slider slider = CreateSlider(canvasObject.transform);
        Button button = CreateButton(canvasObject.transform, hud);

        instruction.text = "Nivel 2: Distancia segura\nObserva la arana 3D durante 30 segundos continuos.";
        progress.text = "Mirada continua: 0.0 / 30s\nMejor intento: 0.0s";
        warning.text = "Manten una distancia segura.";
        complete.text = "Nivel 2 completado.";

        warning.gameObject.SetActive(false);
        complete.gameObject.SetActive(false);
        button.gameObject.SetActive(false);

        hud.instructionText = instruction;
        hud.gazeProgressText = progress;
        hud.warningText = warning;
        hud.completionText = complete;
        hud.gazeProgressSlider = slider;
        hud.continueButton = button;
        hud.nextSceneName = "Nivel3";
        return hud;
    }

    private static TMP_Text CreateTMP(Transform parent, string name, Vector2 pos, Vector2 size, int fontSize)
    {
        Transform existing = parent.Find(name);
        GameObject go = existing != null ? existing.gameObject : new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt == null) rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        TMP_Text text = go.GetComponent<TMP_Text>();
        if (text == null) text = go.AddComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = fontSize;
        text.color = Color.white;
        return text;
    }

    private static Slider CreateSlider(Transform parent)
    {
        Transform existing = parent.Find("GazeProgressSlider");
        GameObject go = existing != null ? existing.gameObject : new GameObject("GazeProgressSlider");
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt == null) rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0, -15);
        rt.sizeDelta = new Vector2(420, 20);

        Slider slider = go.GetComponent<Slider>();
        if (slider == null) slider = go.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 30;
        return slider;
    }

    private static Button CreateButton(Transform parent, Level2HUD hud)
    {
        Transform existing = parent.Find("Button_Continuar_Nivel3");
        GameObject go = existing != null ? existing.gameObject : new GameObject("Button_Continuar_Nivel3");
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt == null) rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0, -180);
        rt.sizeDelta = new Vector2(300, 60);

        Image img = go.GetComponent<Image>();
        if (img == null) img = go.AddComponent<Image>();
        img.color = Color.white;

        Button button = go.GetComponent<Button>();
        if (button == null) button = go.AddComponent<Button>();

        TMP_Text label = CreateTMP(go.transform, "Text", Vector2.zero, new Vector2(300, 60), 24);
        label.text = "Continuar a Nivel 3";
        label.color = Color.black;

        button.onClick.RemoveAllListeners();
        UnityEventTools.AddPersistentListener(button.onClick, hud.GoToNextLevel);
        return button;
    }

    private static void ConfigureZone(GameObject zone, GameObject spider, GameObject xrOrigin, Level2HUD hud)
    {
        if (spider != null) zone.transform.position = spider.transform.position;
        zone.transform.localScale = new Vector3(16, 0.05f, 16);

        Renderer r = zone.GetComponent<Renderer>();
        if (r != null) r.enabled = false;

        Collider col = zone.GetComponent<Collider>();
        if (col == null)
        {
            SphereCollider sphere = zone.AddComponent<SphereCollider>();
            sphere.radius = 1f;
            sphere.isTrigger = true;
        }
        else
        {
            col.isTrigger = true;
        }

        Level2ExclusionZone exclusion = zone.GetComponent<Level2ExclusionZone>();
        if (exclusion == null) exclusion = zone.AddComponent<Level2ExclusionZone>();
        exclusion.playerRoot = xrOrigin != null ? xrOrigin.transform : null;
        exclusion.protectedTarget = spider != null ? spider.transform : null;
        exclusion.hud = hud;
        exclusion.safeDistance = 8f;
    }
}
#endif
