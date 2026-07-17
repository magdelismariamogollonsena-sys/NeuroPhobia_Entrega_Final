#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class NeuroPhobiaAutoSetup
{
    [MenuItem("Tools/NeuroPhobia/1. Ejecutar configuracion completa v4")]
    public static void RunCompleteSetup()
    {
        CreateFolders();
        CreateOrRepairScenes();
        ConfigureBuildSettings();
        ConfigureMainMenu();
        ConfigureAllLevels();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "NeuroPhobia",
            "Configuracion v4 terminada. Revisa la consola. Si no hay errores rojos, abre MainMenu y prueba Play.",
            "OK"
        );
    }

    [MenuItem("Tools/NeuroPhobia/2. Crear carpetas base")]
    public static void CreateFolders()
    {
        string[] folders =
        {
            "Assets/Scenes",
            "Assets/Scripts",
            "Assets/Scripts/Control",
            "Assets/Scripts/Entidades",
            "Assets/Scripts/UI",
            "Assets/Scripts/Metricas",
            "Assets/Scripts/Comunicacion",
            "Assets/Scripts/Menu",
            "Assets/Modelos",
            "Assets/Modelos/Aranas",
            "Assets/Modelos/Entorno",
            "Assets/Modelos/Entorno/Nivel1",
            "Assets/Modelos/Entorno/Comun",
            "Assets/Prefabs",
            "Assets/Prefabs/Aranas",
            "Assets/Prefabs/UI",
            "Assets/Audio",
            "Assets/Editor"
        };

        foreach (string folder in folders) EnsureFolder(folder);
        WriteReadmeIfMissing("Assets/Audio/README_AUDIOS.txt", "Audios esperados: respiracion_478.ogg, narracion_nivel1.ogg, calma_adaptativa.ogg, ambiente_nivel1.ogg");
    }

    [MenuItem("Tools/NeuroPhobia/3. Crear o reparar escenas")]
    public static void CreateOrRepairScenes()
    {
        EnsureFolder("Assets/Scenes");

        for (int level = 1; level <= 5; level++)
        {
            string path = $"Assets/Scenes/Nivel{level}.unity";
            if (!File.Exists(path)) CreateSceneFromBestTemplate(path, level);
        }

        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/NeuroPhobia/4. Configurar Build Settings")]
    public static void ConfigureBuildSettings()
    {
        string[] scenePaths =
        {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/Nivel1.unity",
            "Assets/Scenes/Nivel2.unity",
            "Assets/Scenes/Nivel3.unity",
            "Assets/Scenes/Nivel4.unity",
            "Assets/Scenes/Nivel5.unity"
        };

        EditorBuildSettings.scenes = scenePaths
            .Where(File.Exists)
            .Select(path => new EditorBuildSettingsScene(path, true))
            .ToArray();

        Debug.Log("NeuroPhobia: Build Settings configurado.");
    }

    [MenuItem("Tools/NeuroPhobia/5. Configurar MainMenu")]
    public static void ConfigureMainMenu()
    {
        string mainMenuPath = "Assets/Scenes/MainMenu.unity";
        if (!File.Exists(mainMenuPath))
        {
            Debug.LogWarning("NeuroPhobia: no se encontro Assets/Scenes/MainMenu.unity.");
            return;
        }

        Scene scene = EditorSceneManager.OpenScene(mainMenuPath, OpenSceneMode.Single);
        GameObject canvas = GameObject.Find("mainMenuCanvas");
        if (canvas == null)
        {
            Canvas existingCanvas = Object.FindFirstObjectByType<Canvas>();
            if (existingCanvas != null) canvas = existingCanvas.gameObject;
        }

        if (canvas != null)
        {
            FixMenuTitleTypo(canvas);
            NeuroPhobiaMenuBridge bridge = canvas.GetComponent<NeuroPhobiaMenuBridge>();
            if (bridge == null) bridge = canvas.AddComponent<NeuroPhobiaMenuBridge>();

            Button startButton = FindStartButton();
            if (startButton != null)
            {
                ClearPersistentListeners(startButton);
                UnityEventTools.AddPersistentListener(startButton.onClick, bridge.LoadNivel1);
                EditorUtility.SetDirty(startButton);
                EditorUtility.SetDirty(bridge);
                Debug.Log("NeuroPhobia: boton de inicio conectado a Nivel1.");
            }
            else
            {
                Debug.LogWarning("NeuroPhobia: no se encontro boton de inicio.");
            }
        }
        else
        {
            Debug.LogWarning("NeuroPhobia: no se encontro Canvas del menu.");
        }

        GameObject managers = GameObject.Find("NeuroPhobiaManagers");
        if (managers == null) managers = new GameObject("NeuroPhobiaManagers");
        if (managers.GetComponent<FastAPIClient>() == null) managers.AddComponent<FastAPIClient>();
        if (managers.GetComponent<SessionManager>() == null) managers.AddComponent<SessionManager>();
        if (managers.GetComponent<GameStateManager>() == null) managers.AddComponent<GameStateManager>();
        if (managers.GetComponent<LevelManager>() == null) managers.AddComponent<LevelManager>();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("NeuroPhobia: MainMenu configurado.");
    }

    [MenuItem("Tools/NeuroPhobia/6. Configurar todos los niveles")]
    public static void ConfigureAllLevels()
    {
        for (int level = 1; level <= 5; level++)
        {
            string path = $"Assets/Scenes/Nivel{level}.unity";
            if (!File.Exists(path)) continue;

            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            ConfigureLevelScene(scene, level);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        Debug.Log("NeuroPhobia: niveles configurados.");
    }

    private static void ConfigureLevelScene(Scene scene, int level)
    {
        GameObject managers = GameObject.Find("LevelManagers");
        if (managers == null) managers = new GameObject("LevelManagers");

        LevelBootstrap bootstrap = managers.GetComponent<LevelBootstrap>();
        if (bootstrap == null) bootstrap = managers.AddComponent<LevelBootstrap>();
        SetPrivateSerializedInt(bootstrap, "level", level);

        MetricsCollector metrics = managers.GetComponent<MetricsCollector>();
        if (metrics == null) metrics = managers.AddComponent<MetricsCollector>();
        metrics.level = level;

        AdaptiveCommandPoller poller = managers.GetComponent<AdaptiveCommandPoller>();
        if (poller == null) poller = managers.AddComponent<AdaptiveCommandPoller>();

        if (level == 1)
        {
            RemoveObjectsByNameContains(scene, "cavespider", "spider", "arana", "araña");
            DecorateNivel1();
        }
        else
        {
            RemoveObjectsByNameContains(scene, "cavespider", "spider", "arana", "araña");
            DecorateLevel(level);
            EnsureSpidersForLevel(level);
        }

        Transform player = FindLikelyPlayerCamera();
        if (player != null) metrics.player = player;

        AranaController firstSpider = Object.FindFirstObjectByType<AranaController>();
        if (firstSpider != null) metrics.spider = firstSpider.transform;

        EnsureEventSystemExists();
    }

    private static void CreateSceneFromBestTemplate(string destinationPath, int level)
    {
        string templatePath = File.Exists("Assets/Scenes/SampleScene.unity")
            ? "Assets/Scenes/SampleScene.unity"
            : File.Exists("Assets/Scenes/MainMenu.unity") ? "Assets/Scenes/MainMenu.unity" : null;

        if (templatePath != null && AssetDatabase.CopyAsset(templatePath, destinationPath))
        {
            AssetDatabase.Refresh();
            Scene scene = EditorSceneManager.OpenScene(destinationPath, OpenSceneMode.Single);
            ConfigureLevelScene(scene, level);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"NeuroPhobia: {destinationPath} creado desde {templatePath}.");
            return;
        }

        CreateMinimalScene(destinationPath, level);
    }

    private static void CreateMinimalScene(string destinationPath, int level)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject root = new GameObject($"Nivel{level}_Root");

        GameObject light = new GameObject("Directional Light");
        Light lightComponent = light.AddComponent<Light>();
        lightComponent.type = LightType.Directional;
        lightComponent.intensity = level <= 2 ? 1.1f : 0.55f;
        light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        light.transform.SetParent(root.transform);

        GameObject camera = new GameObject("Fallback Camera");
        Camera cameraComponent = camera.AddComponent<Camera>();
        cameraComponent.tag = "MainCamera";
        camera.transform.position = new Vector3(0f, 1.7f, -6f);
        camera.transform.rotation = Quaternion.Euler(8f, 0f, 0f);
        camera.transform.SetParent(root.transform);

        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "BaseFloor";
        floor.transform.localScale = new Vector3(8f, 1f, 8f);
        floor.transform.SetParent(root.transform);

        ConfigureLevelScene(scene, level);
        EditorSceneManager.SaveScene(scene, destinationPath);
    }

    private static void DecorateNivel1()
    {
        GameObject root = GameObject.Find("Nivel1_AutoSetup") ?? new GameObject("Nivel1_AutoSetup");
        CreateWorldTextCanvas(root.transform, "CanvasPsicoeducacion_WorldSpace", "Nivel 1: Psicoeducacion\nRespira 4-7-8 y observa las imagenes de forma gradual.", new Vector3(0f, 1.7f, 2.2f));

        GameObject breathingSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        breathingSphere.name = "BreathingSphere_Placeholder";
        breathingSphere.transform.position = new Vector3(0f, 1.2f, 1.7f);
        breathingSphere.transform.localScale = Vector3.one * 0.25f;
        breathingSphere.transform.SetParent(root.transform);
        if (breathingSphere.GetComponent<BreathingGuide>() == null) breathingSphere.AddComponent<BreathingGuide>();

        for (int i = 0; i < 4; i++)
        {
            GameObject card = CreateOrMovePrimitive(root.transform, $"Spider2D_Card_{i + 1}", PrimitiveType.Cube, new Vector3(-1.2f + i * 0.8f, 1.9f, 2.1f), new Vector3(0.45f, 0.32f, 0.03f));
            card.tag = "Untagged";
        }
    }

    private static void DecorateLevel(int level)
    {
        GameObject root = GameObject.Find($"Nivel{level}_AutoSetup") ?? new GameObject($"Nivel{level}_AutoSetup");

        if (level == 2)
        {
            CreateWorldTextCanvas(root.transform, "CanvasHUD_Nivel2", "Nivel 2: mira la arana durante 30 segundos sin acercarte.", new Vector3(0f, 2.2f, 3f));
            CreateOrMovePrimitive(root.transform, "Roca_DistanciaSegura", PrimitiveType.Sphere, new Vector3(0f, 0.45f, 10f), new Vector3(2f, 0.8f, 1.6f));
            GameObject zone = CreateOrMovePrimitive(root.transform, "ZonaExclusion_8m", PrimitiveType.Sphere, new Vector3(0f, 0f, 10f), Vector3.one * 8f);
            Collider c = zone.GetComponent<Collider>();
            if (c != null) c.isTrigger = true;
            if (zone.GetComponent<ExclusionZoneMessage>() == null) zone.AddComponent<ExclusionZoneMessage>();
        }
        else if (level == 3)
        {
            CreateWorldTextCanvas(root.transform, "CanvasHUD_Nivel3", "Nivel 3: acercamiento progresivo. Llega a 3m y luego a 1m.", new Vector3(0f, 2.2f, 2.5f));
            for (int i = 0; i < 4; i++)
            {
                Vector3 scale = i < 2 ? new Vector3(12f, 3f, 0.25f) : new Vector3(0.25f, 3f, 12f);
                Vector3 pos = i == 0 ? new Vector3(0f, 1.5f, 6f) : i == 1 ? new Vector3(0f, 1.5f, -6f) : i == 2 ? new Vector3(6f, 1.5f, 0f) : new Vector3(-6f, 1.5f, 0f);
                CreateOrMovePrimitive(root.transform, $"Sotano_Wall_{i + 1}", PrimitiveType.Cube, pos, scale);
            }
            CreateCheckpoint(root.transform, "CP1_3m", new Vector3(0f, 0.5f, 3f), 3, false);
            CreateCheckpoint(root.transform, "CP2_1m", new Vector3(0f, 0.5f, 1f), 3, true);
        }
        else if (level == 4)
        {
            CreateWorldTextCanvas(root.transform, "CanvasHUD_Nivel4", "Nivel 4: observacion y contacto virtual controlado.", new Vector3(0f, 2.2f, 2.5f));
            CreateOrMovePrimitive(root.transform, "Mesa_Terrario", PrimitiveType.Cube, new Vector3(0f, 0.75f, 2.5f), new Vector3(3f, 0.2f, 1.5f));
            GameObject terrarium = CreateOrMovePrimitive(root.transform, "Terrario_Placeholder", PrimitiveType.Cube, new Vector3(0f, 1.35f, 2.5f), new Vector3(2.2f, 1f, 1.1f));
            Collider c = terrarium.GetComponent<Collider>();
            if (c != null) c.isTrigger = false;
            GameObject completeZone = CreateCheckpoint(root.transform, "ContactoVirtual_10s_Placeholder", new Vector3(0f, 1.2f, 1.3f), 4, true);
            completeZone.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        }
        else if (level == 5)
        {
            CreateWorldTextCanvas(root.transform, "CanvasHUD_Nivel5", "Nivel 5: completa las 4 tareas cotidianas.", new Vector3(0f, 2.2f, 2.5f));
            string[] tasks = { "TASK_LAMP", "TASK_DOOR", "TASK_FRIDGE", "TASK_WINDOW" };
            for (int i = 0; i < tasks.Length; i++)
            {
                GameObject task = CreateOrMovePrimitive(root.transform, tasks[i], PrimitiveType.Cube, new Vector3(-3f + i * 2f, 1f, 3f), new Vector3(0.6f, 1.2f, 0.6f));
                SimpleTaskInteractable simpleTask = task.GetComponent<SimpleTaskInteractable>();
                if (simpleTask == null) simpleTask = task.AddComponent<SimpleTaskInteractable>();
                SetPrivateSerializedString(simpleTask, "taskId", tasks[i]);
            }

            GameObject taskManager = GameObject.Find("TaskManager") ?? new GameObject("TaskManager");
            if (taskManager.GetComponent<TaskManager>() == null) taskManager.AddComponent<TaskManager>();
        }
    }

    private static void EnsureSpidersForLevel(int level)
    {
        int count = level == 5 ? 5 : 1;
        for (int i = 0; i < count; i++)
        {
            GameObject spider = CreateSpiderObject(level == 5 ? $"CaveSpider_Nivel5_FreeRoam_{i + 1}" : $"CaveSpider_Nivel{level}");
            AranaController controller = spider.GetComponent<AranaController>();
            if (controller == null) controller = spider.AddComponent<AranaController>();

            if (level == 2)
            {
                spider.name = "CaveSpider_Nivel2_Static";
                spider.transform.position = new Vector3(0f, 1.2f, 10f);
                spider.transform.localScale = Vector3.one * 0.8f;
                controller.defaultPattern = MovementPattern.IDLE;
            }
            else if (level == 3)
            {
                spider.name = "CaveSpider_Nivel3_SlowPatrol";
                spider.transform.position = new Vector3(0f, 0.35f, 5f);
                spider.transform.localScale = Vector3.one * 0.75f;
                controller.defaultPattern = MovementPattern.SLOW_PATROL;
            }
            else if (level == 4)
            {
                spider.name = "CaveSpider_Nivel4_Terrario";
                spider.transform.position = new Vector3(0f, 1.35f, 2.5f);
                spider.transform.localScale = Vector3.one * 0.7f;
                controller.defaultPattern = MovementPattern.IDLE;
            }
            else if (level == 5)
            {
                spider.name = $"CaveSpider_Nivel5_FreeRoam_{i + 1}";
                spider.transform.position = new Vector3(-4f + i * 2f, 0.25f, 1.5f + (i % 2) * 2f);
                spider.transform.localScale = Vector3.one * 0.3f;
                controller.defaultPattern = MovementPattern.FREE_ROAM;
            }

            EnsureCollider(spider);
        }
    }

    private static GameObject CreateSpiderObject(string name)
    {
        string[] guids = AssetDatabase.FindAssets("CaveSpider t:GameObject");
        if (guids != null && guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset != null)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(asset);
                instance.name = name;
                if (instance.GetComponent<AranaController>() == null) instance.AddComponent<AranaController>();
                return instance;
            }
        }

        GameObject fallback = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        fallback.name = name + "_Fallback";
        fallback.transform.localScale = Vector3.one * 0.35f;
        fallback.AddComponent<AranaController>();
        return fallback;
    }

    private static void CreateWorldTextCanvas(Transform parent, string name, string message, Vector3 position)
    {
        GameObject existing = GameObject.Find(name);
        if (existing != null) Object.DestroyImmediate(existing);

        GameObject canvasObject = new GameObject(name);
        canvasObject.transform.SetParent(parent);
        canvasObject.transform.position = position;
        canvasObject.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        canvasObject.transform.localScale = Vector3.one * 0.008f;

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(500f, 220f);
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject background = new GameObject("Background");
        background.transform.SetParent(canvasObject.transform, false);
        Image image = background.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.55f);
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(canvasObject.transform, false);
        Text text = textObject.AddComponent<Text>();
        text.text = message;
        text.alignment = TextAnchor.MiddleCenter;
        text.fontSize = 30;
        text.color = Color.white;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(20f, 20f);
        textRect.offsetMax = new Vector2(-20f, -20f);
    }

    private static GameObject CreateCheckpoint(Transform parent, string name, Vector3 position, int level, bool completesLevel)
    {
        GameObject checkpoint = CreateOrMovePrimitive(parent, name, PrimitiveType.Sphere, position, Vector3.one * 0.6f);
        Collider collider = checkpoint.GetComponent<Collider>();
        if (collider != null) collider.isTrigger = true;
        LevelCompletionTrigger trigger = checkpoint.GetComponent<LevelCompletionTrigger>();
        if (trigger == null) trigger = checkpoint.AddComponent<LevelCompletionTrigger>();
        SetPrivateSerializedInt(trigger, "level", level);
        SetPrivateSerializedBool(trigger, "completeLevel", completesLevel);
        return checkpoint;
    }

    private static GameObject CreateOrMovePrimitive(Transform parent, string name, PrimitiveType type, Vector3 position, Vector3 scale)
    {
        GameObject obj = GameObject.Find(name);
        if (obj == null) obj = GameObject.CreatePrimitive(type);
        obj.name = name;
        obj.transform.position = position;
        obj.transform.localScale = scale;
        obj.transform.SetParent(parent);
        return obj;
    }

    private static void RemoveObjectsByNameContains(Scene scene, params string[] needles)
    {
        List<GameObject> toDestroy = new List<GameObject>();
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in transforms)
            {
                if (t == null) continue;
                string name = t.name.ToLowerInvariant();
                if (needles.Any(needle => name.Contains(needle))) toDestroy.Add(t.gameObject);
            }
        }

        foreach (GameObject obj in toDestroy.Distinct().ToList())
        {
            if (obj != null) Object.DestroyImmediate(obj);
        }
    }

    private static Button FindStartButton()
    {
        Button[] buttons = Object.FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (Button button in buttons)
        {
            string label = GetButtonText(button).ToLowerInvariant();
            string objectName = button.gameObject.name.ToLowerInvariant();
            if (label.Contains("iniciar") || label.Contains("sesion") || label.Contains("sesión") || objectName.Contains("start") || objectName.Contains("iniciar")) return button;
        }
        return buttons.Length > 0 ? buttons[0] : null;
    }

    private static string GetButtonText(Button button)
    {
        Text[] texts = button.GetComponentsInChildren<Text>(true);
        if (texts.Length > 0) return texts[0].text;

        Component[] components = button.GetComponentsInChildren<Component>(true);
        foreach (Component component in components)
        {
            if (component == null) continue;
            var prop = component.GetType().GetProperty("text");
            if (prop != null && prop.PropertyType == typeof(string))
            {
                object value = prop.GetValue(component, null);
                if (value != null) return value.ToString();
            }
        }
        return button.gameObject.name;
    }

    private static void FixMenuTitleTypo(GameObject canvas)
    {
        Component[] components = canvas.GetComponentsInChildren<Component>(true);
        foreach (Component component in components)
        {
            if (component == null) continue;
            var prop = component.GetType().GetProperty("text");
            if (prop == null || prop.PropertyType != typeof(string) || !prop.CanWrite) continue;
            string value = prop.GetValue(component, null) as string;
            if (string.IsNullOrEmpty(value)) continue;
            string lower = value.ToLowerInvariant();
            if (lower.Contains("bienbenido") || lower.Contains("bienbenidoh") || lower.Contains("bienvenidoh"))
            {
                prop.SetValue(component, "Bienvenido", null);
                EditorUtility.SetDirty(component);
            }
        }
    }

    private static void ClearPersistentListeners(Button button)
    {
        int count = button.onClick.GetPersistentEventCount();
        for (int i = count - 1; i >= 0; i--) UnityEventTools.RemovePersistentListener(button.onClick, i);
    }

    private static void EnsureFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder)) return;
        string parent = Path.GetDirectoryName(folder).Replace("\\", "/");
        string name = Path.GetFileName(folder);
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }

    private static void WriteReadmeIfMissing(string path, string content)
    {
        if (File.Exists(path)) return;
        File.WriteAllText(path, content);
        AssetDatabase.ImportAsset(path);
    }

    private static void EnsureCollider(GameObject obj)
    {
        if (obj.GetComponentInChildren<Collider>() != null) return;
        obj.AddComponent<BoxCollider>();
    }

    private static Transform FindLikelyPlayerCamera()
    {
        if (Camera.main != null) return Camera.main.transform;
        Camera cam = Object.FindFirstObjectByType<Camera>();
        return cam != null ? cam.transform : null;
    }

    private static void EnsureEventSystemExists()
    {
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() != null) return;
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
    }

    private static void SetPrivateSerializedInt(Object target, string fieldName, int value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(fieldName);
        if (property != null)
        {
            property.intValue = value;
            serializedObject.ApplyModifiedProperties();
        }
    }

    private static void SetPrivateSerializedBool(Object target, string fieldName, bool value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(fieldName);
        if (property != null)
        {
            property.boolValue = value;
            serializedObject.ApplyModifiedProperties();
        }
    }

    private static void SetPrivateSerializedString(Object target, string fieldName, string value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(fieldName);
        if (property != null)
        {
            property.stringValue = value;
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
