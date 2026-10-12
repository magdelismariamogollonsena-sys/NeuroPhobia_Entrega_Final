#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public static class NeuroPhobiaNivel5Finalize
{
    private const string ScenePath = "Assets/Scenes/Nivel5.unity";
    private const string MaterialFolder = "Assets/NeuroPhobia_Level5/Materials";

    [MenuItem("Tools/NeuroPhobia/Nivel 5/Finalizar nivel 5")]
    public static void Run()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Transform apartment = Find(scene, "NP_L5_APARTAMENTO_VISUAL");
        if (apartment == null) throw new Exception("Falta el apartamento aprobado.");
        Dictionary<Transform, string> originalTransforms = apartment.GetComponentsInChildren<Transform>(true)
            .ToDictionary(t => t, t => t.localPosition + "|" + t.localRotation + "|" + t.localScale);

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) throw new Exception("Falta URP/Lit.");
        Texture plaster = AssetDatabase.LoadAssetAtPath<Texture>("Assets/NeuroPhobia_Level4_Rebuild/Textures/painted_plaster_wall_diff_1k.jpg");
        Material[] walls = {
            MaterialFor("L5_Pared_Sala", shader, plaster, new Color(.91f,.87f,.80f)),
            MaterialFor("L5_Pared_Cocina", shader, plaster, new Color(.91f,.91f,.87f)),
            MaterialFor("L5_Pared_Cuarto", shader, plaster, new Color(.88f,.88f,.85f)),
            MaterialFor("L5_Pared_Bano", shader, plaster, new Color(.92f,.93f,.91f))
        };
        Material ceiling = MaterialFor("L5_Techo", shader, plaster, new Color(.94f,.93f,.90f));
        Transform architecture = apartment.Find("05_PUERTAS_VENTANAS");
        if (architecture == null) throw new Exception("Falta la arquitectura.");
        foreach (Transform wall in architecture)
        {
            if (!wall.name.StartsWith("Exterior_") && !wall.name.StartsWith("Divisor_")) continue;
            Renderer renderer = wall.GetComponent<Renderer>();
            if (renderer == null) continue;
            int room = wall.name.Contains("Cocina") ? 1 : wall.name.Contains("Bano") ? 3 : wall.name.Contains("Cuarto") ? 2 : 0;
            renderer.sharedMaterial = walls[room];
            BoxCollider collider = wall.GetComponent<BoxCollider>();
            if (collider == null) collider = wall.gameObject.AddComponent<BoxCollider>();
            collider.enabled = true;
        }
        Transform roof = apartment.Find("08_TECHO");
        if (roof == null) roof = new GameObject("08_TECHO").transform;
        roof.SetParent(apartment, false);
        string[] rooms = { "01_SALA", "02_COCINA", "03_CUARTO", "04_BANO" };
        float height = architecture.GetComponentsInChildren<Renderer>(true)
            .Where(r => r.name.StartsWith("Exterior_") || r.name.StartsWith("Divisor_"))
            .Max(r => r.bounds.max.y) + .04f;
        foreach (string roomName in rooms)
        {
            Transform room = apartment.Find(roomName);
            Renderer floor = room.GetComponentsInChildren<Renderer>(true).FirstOrDefault(r => r.name.StartsWith("Piso_"));
            if (floor == null) throw new Exception("Falta piso en " + roomName);
            string name = "Techo_" + roomName;
            Transform piece = roof.Find(name);
            if (piece == null)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.name = name;
                piece = cube.transform;
                piece.SetParent(roof, true);
            }
            Bounds bounds = floor.bounds;
            piece.position = new Vector3(bounds.center.x, height, bounds.center.z);
            piece.rotation = Quaternion.identity;
            piece.localScale = new Vector3(bounds.size.x, .08f, bounds.size.z);
            piece.GetComponent<Renderer>().sharedMaterial = ceiling;
            Collider roofCollider = piece.GetComponent<Collider>();
            if (roofCollider != null) UnityEngine.Object.DestroyImmediate(roofCollider);
        }

        foreach (string name in new[] { "L5_Orb_1", "L5_Orb_2", "L5_Orb_3", "L5_Orb_4", "L5_FinalZone", "FactoryProps", "NP_Spider_Level5_A", "NP_Spider_Level5_B", "NP_Spider_Level5_Central", "NP_Level5_HUD", "SpiderRoutes" })
        {
            Transform old = Find(scene, name);
            if (old != null) old.gameObject.SetActive(false);
        }
        foreach (GameObject root in scene.GetRootGameObjects())
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                if (t.name.StartsWith("L5_Route_") || t.name == "CanvasHUD_Nivel5") t.gameObject.SetActive(false);
        foreach (Level5DomesticTaskManager legacy in UnityEngine.Object.FindObjectsByType<Level5DomesticTaskManager>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            legacy.enabled = false;
        foreach (Level5SimpleInteractable legacy in UnityEngine.Object.FindObjectsByType<Level5SimpleInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            legacy.enabled = false;
        foreach (Level5TaskObject legacy in UnityEngine.Object.FindObjectsByType<Level5TaskObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            legacy.enabled = false;

        MetricsCollector metrics = UnityEngine.Object.FindFirstObjectByType<MetricsCollector>();
        if (metrics == null) metrics = new GameObject("MetricsCollector_Nivel5").AddComponent<MetricsCollector>();
        metrics.level = 5;
        TaskManager[] managers = UnityEngine.Object.FindObjectsByType<TaskManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        TaskManager manager = managers.FirstOrDefault();
        if (manager == null) manager = new GameObject("TaskManager_Nivel5").AddComponent<TaskManager>();
        foreach (TaskManager extra in managers.Skip(1)) extra.gameObject.SetActive(false);
        manager.metricsCollector = metrics;
        manager.maxAvoidanceAttempts = 5;
        Transform hudRoot = Find(scene, "HUD_Tareas_Nivel5");
        Level5HUD hud = hudRoot != null ? hudRoot.GetComponent<Level5HUD>() : null;
        if (hud == null) hud = new GameObject("HUD_Tareas_Nivel5").AddComponent<Level5HUD>();
        manager.hud = hud;

        string[] objectNames = { "NP_L5_LAMP_VISUAL", "NP_L5_DOOR_VISUAL", "NP_L5_FRIDGE_VISUAL" };
        string[] taskIds = { "TASK_LAMP", "TASK_DOOR", "TASK_FRIDGE" };
        for (int i = 0; i < objectNames.Length; i++)
        {
            Transform visual = apartment.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == objectNames[i]);
            if (visual == null && i == 1) visual = Find(scene, "puerta sala a cuarto");
            if (visual == null) throw new Exception("Falta " + objectNames[i]);
            BoxCollider collider = visual.GetComponent<BoxCollider>();
            if (collider == null) collider = visual.gameObject.AddComponent<BoxCollider>();
            Bounds bounds = BoundsOf(visual);
            collider.center = visual.InverseTransformPoint(bounds.center);
            collider.size = new Vector3(bounds.size.x / Mathf.Abs(visual.lossyScale.x), bounds.size.y / Mathf.Abs(visual.lossyScale.y), bounds.size.z / Mathf.Abs(visual.lossyScale.z));
            collider.enabled = true;
            XRSimpleInteractable xr = visual.GetComponent<XRSimpleInteractable>();
            if (xr == null) xr = visual.gameObject.AddComponent<XRSimpleInteractable>();
            Level5TaskInteraction interaction = visual.GetComponent<Level5TaskInteraction>();
            if (interaction == null) interaction = visual.gameObject.AddComponent<Level5TaskInteraction>();
            interaction.taskId = taskIds[i];
            interaction.taskManager = manager;
            if (i == 0) interaction.movingPart = null;
            if (i == 0)
            {
                Light lampLight = visual.GetComponent<Light>();
                if (lampLight == null) lampLight = visual.gameObject.AddComponent<Light>();
                lampLight.type = LightType.Point;
                lampLight.color = new Color(1f, .85f, .68f);
                lampLight.range = 2.5f;
                lampLight.intensity = .6f;
                interaction.controlledLight = lampLight;
            }
        }
        Transform window = apartment.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "NP_L5_WINDOW_VISUAL");
        if (window == null) throw new Exception("Falta ventana.");
        Transform gazeTarget = window.Find("L5_GazeCollider");
        if (gazeTarget == null) gazeTarget = new GameObject("L5_GazeCollider").transform;
        gazeTarget.SetParent(window, false);
        Bounds windowBounds = BoundsOf(window);
        gazeTarget.position = windowBounds.center;
        BoxCollider gazeCollider = gazeTarget.GetComponent<BoxCollider>();
        if (gazeCollider == null) gazeCollider = gazeTarget.gameObject.AddComponent<BoxCollider>();
        gazeCollider.size = new Vector3(windowBounds.size.x / Mathf.Abs(gazeTarget.lossyScale.x), windowBounds.size.y / Mathf.Abs(gazeTarget.lossyScale.y), .08f);
        Level5WindowGaze gaze = gazeTarget.GetComponent<Level5WindowGaze>();
        if (gaze == null) gaze = gazeTarget.gameObject.AddComponent<Level5WindowGaze>();
        gaze.windowTarget = window;
        gaze.taskManager = manager;

        Transform spawns = Find(scene, "NP_L5_SPAWN_POINTS_FUTUROS");
        if (spawns == null) throw new Exception("Faltan los puntos de spawn.");
        Level5SpawnManager spawnManager = UnityEngine.Object.FindFirstObjectByType<Level5SpawnManager>();
        if (spawnManager == null) spawnManager = new GameObject("SpawnManager_Nivel5").AddComponent<Level5SpawnManager>();
        spawnManager.spawnPoints = Enumerable.Range(1, 5).Select(i => spawns.Find("Spawn_" + i.ToString("00"))).ToArray();
        if (spawnManager.spawnPoints.Any(t => t == null)) throw new Exception("Deben existir cinco puntos de spawn.");
        spawnManager.spiderPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Modelosdetodo/escenario 1/cave-spider/source/CaveSpider.fbx");
        if (spawnManager.spiderPrefab == null) throw new Exception("Falta CaveSpider.fbx.");
        spawnManager.metricsCollector = metrics;

        foreach (var pair in originalTransforms)
            if (pair.Key != null && pair.Value != pair.Key.localPosition + "|" + pair.Key.localRotation + "|" + pair.Key.localScale)
                throw new Exception("Transform aprobado modificado: " + pair.Key.name);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("NIVEL5_FINALIZED: apartamento conservado, 5 spawns, 4 tareas.");
    }

    private static Transform Find(Scene scene, string name) => scene.GetRootGameObjects()
        .SelectMany(go => go.GetComponentsInChildren<Transform>(true)).FirstOrDefault(t => t.name == name);

    private static Bounds BoundsOf(Transform root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) throw new Exception("Sin malla visible: " + root.name);
        Bounds result = renderers[0].bounds;
        foreach (Renderer r in renderers.Skip(1)) result.Encapsulate(r.bounds);
        return result;
    }

    private static Material MaterialFor(string name, Shader shader, Texture texture, Color color)
    {
        string path = MaterialFolder + "/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }
        material.SetColor("_BaseColor", color);
        if (texture != null) material.SetTexture("_BaseMap", texture);
        EditorUtility.SetDirty(material);
        return material;
    }
}
#endif
