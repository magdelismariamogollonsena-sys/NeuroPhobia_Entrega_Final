#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class Levels4And5Setup
{
    [MenuItem("Tools/NeuroPhobia/4. Configurar Nivel4 Contacto Virtual")]
    public static void ConfigureLevel4()
    {
        OpenScene("Nivel4");

        GameObject root = FindOrCreate("Nivel4_AutoSetup");
        GameObject spider = FindSpider("Nivel4");
        GameObject xrOrigin = GameObject.Find("XR Origin (XR Rig)");
        Camera cam = Camera.main;

        GameObject hudObject = CreateLevel4HUD(root.transform, cam);
        Level4HUD hud = hudObject.GetComponent<Level4HUD>();

        GameObject terrarium = FindOrCreate("Terrario_Nivel4", root.transform);
        GameObject lid = FindOrCreate("Terrario_Tapa", terrarium.transform);
        ConfigureTerrariumVisuals(terrarium, lid);

        Level4TerrariumController terrariumController =
            terrarium.GetComponent<Level4TerrariumController>() ??
            terrarium.AddComponent<Level4TerrariumController>();

        terrariumController.lid = lid.transform;
        terrariumController.hud = hud;

        Level4ContactTimer timer =
            root.GetComponent<Level4ContactTimer>() ??
            root.AddComponent<Level4ContactTimer>();

        timer.spiderTarget = spider.transform;
        timer.terrarium = terrariumController;
        timer.hud = hud;
        timer.metricsCollector = root.GetComponent<MetricsCollector>() ??
                                 root.AddComponent<MetricsCollector>();

        MetricsCollector metrics = root.GetComponent<MetricsCollector>();
        metrics.level = 4;
        metrics.player = cam != null ? cam.transform : null;
        metrics.spider = spider.transform;

        Level4SpiderHandCrawl crawl =
            spider.GetComponent<Level4SpiderHandCrawl>() ??
            spider.AddComponent<Level4SpiderHandCrawl>();

        crawl.terrarium = terrariumController;

        if (xrOrigin != null)
        {
            Transform left = FindChildContains(xrOrigin.transform, "Left Controller");
            Transform right = FindChildContains(xrOrigin.transform, "Right Controller");
            timer.leftHand = left;
            timer.rightHand = right;
            crawl.handTarget = right != null ? right : left;
        }

        CreateTerrariumButton(root.transform, terrariumController);

        Save();
        EditorUtility.DisplayDialog("Nivel 4", "Nivel 4 configurado.", "OK");
    }

    [MenuItem("Tools/NeuroPhobia/5. Configurar Nivel5 Entorno Domestico")]
    public static void ConfigureLevel5()
    {
        OpenScene("Nivel5");

        GameObject root = FindOrCreate("Nivel5_AutoSetup");
        Camera cam = Camera.main;

        GameObject hudObject = CreateLevel5HUD(root.transform, cam);
        Level5HUD hud = hudObject.GetComponent<Level5HUD>();

        Level5DomesticTaskManager manager =
            root.GetComponent<Level5DomesticTaskManager>() ??
            root.AddComponent<Level5DomesticTaskManager>();

        MetricsCollector metrics =
            root.GetComponent<MetricsCollector>() ??
            root.AddComponent<MetricsCollector>();

        metrics.level = 5;
        metrics.player = cam != null ? cam.transform : null;

        manager.hud = hud;
        manager.metricsCollector = metrics;
        manager.requiredTaskCount = 4;

        CreateTask(root.transform, "TASK_LAMP", Level5TaskType.Lamp, new Vector3(-3f, 1f, 3f), manager);
        CreateTask(root.transform, "TASK_DOOR", Level5TaskType.Door, new Vector3(-1f, 1f, 3f), manager);
        CreateTask(root.transform, "TASK_FRIDGE", Level5TaskType.Fridge, new Vector3(1f, 1f, 3f), manager);
        CreateTask(root.transform, "TASK_WINDOW", Level5TaskType.Window, new Vector3(3f, 1f, 3f), manager);

        CreateOrConfigureSpiders(root.transform);

        Save();
        EditorUtility.DisplayDialog("Nivel 5", "Nivel 5 configurado.", "OK");
    }

    private static void OpenScene(string name)
    {
        string path = $"Assets/Scenes/{name}.unity";

        if (File.Exists(path) && SceneManager.GetActiveScene().name != name)
            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
    }

    private static GameObject FindOrCreate(string name, Transform parent = null)
    {
        GameObject go = GameObject.Find(name);

        if (go == null)
            go = new GameObject(name);

        if (parent != null)
            go.transform.SetParent(parent);

        return go;
    }

    private static GameObject FindSpider(string levelName)
    {
        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            string n = go.name.ToLowerInvariant();

            if ((n.Contains("spider") || n.Contains("arana") || n.Contains("araña")) &&
                n.Contains(levelName.ToLowerInvariant().Replace("nivel", "")))
                return go;
        }

        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            string n = go.name.ToLowerInvariant();

            if (n.Contains("spider") || n.Contains("arana") || n.Contains("araña"))
                return go;
        }

        GameObject fallback = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        fallback.name = $"CaveSpider_{levelName}";
        fallback.transform.position = new Vector3(0f, 1.3f, 2.5f);
        fallback.transform.localScale = Vector3.one * 0.5f;
        return fallback;
    }

    private static void ConfigureTerrariumVisuals(GameObject terrarium, GameObject lid)
    {
        if (terrarium.GetComponent<BoxCollider>() == null)
            terrarium.AddComponent<BoxCollider>();

        if (terrarium.GetComponent<MeshRenderer>() == null)
        {
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Terrario_Cuerpo";
            body.transform.SetParent(terrarium.transform);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(2f, 1.2f, 1.2f);
        }

        if (lid.GetComponent<BoxCollider>() == null)
            lid.AddComponent<BoxCollider>();

        if (lid.GetComponent<MeshRenderer>() == null)
        {
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Tapa_Visual";
            visual.transform.SetParent(lid.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(2f, 0.12f, 1.2f);
        }

        terrarium.transform.position = new Vector3(0f, 1.25f, 2.5f);
        lid.transform.localPosition = new Vector3(0f, 0.7f, -0.55f);
    }

    private static GameObject CreateLevel4HUD(Transform parent, Camera cam)
    {
        GameObject old = GameObject.Find("CanvasHUD_Nivel4");
        if (old != null) Object.DestroyImmediate(old);

        GameObject canvasGo = CreateCanvas("CanvasHUD_Nivel4", parent, cam);
        Level4HUD hud = canvasGo.AddComponent<Level4HUD>();

        hud.instructionText = CreateText(canvasGo.transform, "InstructionText", new Vector2(0, 120), 28);
        hud.contactProgressText = CreateText(canvasGo.transform, "ContactProgressText", new Vector2(0, 35), 26);
        hud.warningText = CreateText(canvasGo.transform, "WarningText", new Vector2(0, -45), 24);
        hud.completionText = CreateText(canvasGo.transform, "CompletionText", new Vector2(0, -105), 26);
        hud.contactSlider = CreateSlider(canvasGo.transform, "ContactSlider", new Vector2(0, -15), 10f);
        hud.continueButton = CreateButton(canvasGo.transform, "Button_Continuar_Nivel5", "Continuar a Nivel 5", new Vector2(0, -175), hud.GoToNextLevel);

        hud.warningText.gameObject.SetActive(false);
        hud.completionText.gameObject.SetActive(false);
        hud.continueButton.gameObject.SetActive(false);

        return canvasGo;
    }

    private static GameObject CreateLevel5HUD(Transform parent, Camera cam)
    {
        GameObject old = GameObject.Find("CanvasHUD_Nivel5");
        if (old != null) Object.DestroyImmediate(old);

        GameObject canvasGo = CreateCanvas("CanvasHUD_Nivel5", parent, cam);
        Level5HUD hud = canvasGo.AddComponent<Level5HUD>();

        hud.instructionText = CreateText(canvasGo.transform, "InstructionText", new Vector2(0, 100), 28);
        hud.taskProgressText = CreateText(canvasGo.transform, "TaskProgressText", new Vector2(0, 10), 26);
        hud.completionText = CreateText(canvasGo.transform, "CompletionText", new Vector2(0, -70), 26);
        hud.finishButton = CreateButton(canvasGo.transform, "Button_Finalizar", "Finalizar sesión", new Vector2(0, -155), hud.FinishSession);

        hud.completionText.gameObject.SetActive(false);
        hud.finishButton.gameObject.SetActive(false);

        return canvasGo;
    }

    private static GameObject CreateCanvas(string name, Transform parent, Camera cam)
    {
        GameObject canvasGo = new GameObject(
            name,
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );

        canvasGo.transform.SetParent(parent, false);
        canvasGo.transform.position = new Vector3(0f, 2.1f, 2.4f);
        canvasGo.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        canvasGo.transform.localScale = Vector3.one * 0.006f;

        RectTransform rt = canvasGo.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(700f, 380f);

        Canvas canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = cam;

        return canvasGo;
    }

    private static TMP_Text CreateText(Transform parent, string name, Vector2 pos, int fontSize)
    {
        GameObject go = new GameObject(
            name,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );

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

    private static Slider CreateSlider(Transform parent, string name, Vector2 pos, float maxValue)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Slider));
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(420f, 20f);

        Slider slider = go.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = maxValue;
        slider.value = 0f;

        return slider;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 pos, UnityEngine.Events.UnityAction action)
    {
        GameObject go = new GameObject(
            name,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button)
        );

        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(300f, 60f);

        Button button = go.GetComponent<Button>();

        GameObject textGo = new GameObject(
            "Text",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );

        textGo.transform.SetParent(go.transform, false);

        RectTransform textRt = textGo.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textGo.GetComponent<TextMeshProUGUI>();
        text.text = label;
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 24;
        text.color = Color.black;

        UnityEventTools.AddPersistentListener(button.onClick, action);

        return button;
    }

    private static void CreateTerrariumButton(Transform parent, Level4TerrariumController controller)
    {
        GameObject canvas = GameObject.Find("CanvasHUD_Nivel4");
        if (canvas == null) return;

        Button button = CreateButton(
            canvas.transform,
            "Button_Abrir_Terrario",
            "Abrir / cerrar terrario",
            new Vector2(0f, -230f),
            controller.ToggleTerrarium
        );

        button.gameObject.SetActive(true);
    }

    private static void CreateTask(Transform parent, string name, Level5TaskType type, Vector3 position, Level5DomesticTaskManager manager)
    {
        GameObject task = GameObject.Find(name);

        if (task == null)
            task = GameObject.CreatePrimitive(PrimitiveType.Cube);

        task.name = name;
        task.transform.SetParent(parent);
        task.transform.position = position;
        task.transform.localScale = new Vector3(0.7f, 1.2f, 0.7f);

        Level5TaskObject taskObject =
            task.GetComponent<Level5TaskObject>() ??
            task.AddComponent<Level5TaskObject>();

        taskObject.taskType = type;
        taskObject.taskManager = manager;

        Level5SimpleInteractable interaction =
            task.GetComponent<Level5SimpleInteractable>() ??
            task.AddComponent<Level5SimpleInteractable>();

        interaction.taskObject = taskObject;
        interaction.movablePart = task.transform;
        interaction.completedLocalEuler = type == Level5TaskType.Door
            ? new Vector3(0f, 90f, 0f)
            : new Vector3(-25f, 0f, 0f);
    }

    private static void CreateOrConfigureSpiders(Transform parent)
    {
        int found = 0;

        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            string n = go.name.ToLowerInvariant();

            if (n.Contains("spider") || n.Contains("arana") || n.Contains("araña"))
            {
                if (go.GetComponent<Level5SpiderRoam>() == null)
                    go.AddComponent<Level5SpiderRoam>();

                found++;
            }
        }

        for (int i = found; i < 5; i++)
        {
            GameObject spider = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            spider.name = $"CaveSpider_Nivel5_{i + 1}";
            spider.transform.SetParent(parent);
            spider.transform.position = new Vector3(-4f + i * 2f, 0.25f, 1.5f + (i % 2) * 2f);
            spider.transform.localScale = Vector3.one * 0.25f;
            spider.AddComponent<Level5SpiderRoam>();
        }
    }

    private static Transform FindChildContains(Transform root, string text)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name.Contains(text))
                return child;
        }

        return null;
    }

    private static void Save()
    {
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
    }
}
#endif
