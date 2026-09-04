#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class NeuroPhobiaLevel4CasaOrganizada
{
    private const string ScenePath = "Assets/Scenes/Nivel4.unity";
    private const string BackupPath = "Assets/Scenes/Nivel4_BACKUP_AntesCasaBonita.unity";
    private const string HouseRootName = "NP_ENV_NIVEL4_HOGAR_ORGANIZADO";
    private const string OldPrefix = "NP_OLD_L4_HIDDEN__";
    private const string SoftpackBase = "Assets/nappin/House Props Softpack/Prefabs/";

    [MenuItem("Tools/NeuroPhobia/Nivel 4/Construir CASA bonita y organizada", priority = 210)]
    public static void Build()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog(
                "NeuroPhobia",
                "Detén Play antes de organizar el Nivel 4.",
                "Aceptar");
            return;
        }

        if (!File.Exists(ScenePath))
        {
            EditorUtility.DisplayDialog(
                "NeuroPhobia",
                "No encontré Assets/Scenes/Nivel4.unity",
                "Aceptar");
            return;
        }

        bool softpackReady =
            AssetDatabase.LoadAssetAtPath<GameObject>(
                SoftpackBase + "(Prb)Sofa_pillowT.prefab") != null;

        if (!softpackReady)
        {
            EditorUtility.DisplayDialog(
                "NeuroPhobia",
                "Todavía no encuentro House Props Softpack.\n\n" +
                "Primero importa:\nHouse Softpack - nappin.unitypackage\n\n" +
                "Después vuelve a ejecutar este comando.",
                "Aceptar");
            return;
        }

        // Copia de seguridad automática una sola vez.
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(BackupPath) == null)
        {
            AssetDatabase.CopyAsset(ScenePath, BackupPath);
            AssetDatabase.SaveAssets();
        }

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        GameObject previous = FindSceneObject(scene, HouseRootName);
        if (previous != null)
            UnityEngine.Object.DestroyImmediate(previous);

        AutoHideOldEnvironmentRoots(scene);

        Vector3 playerPos = FindBestPlayerPosition(scene);
        Transform spider = FindSpider(scene);

        Vector3 center = playerPos;

        // Si la araña está cerca, centra la casa entre jugador y araña
        // para conservar la lógica existente sin mover el gameplay.
        if (spider != null)
        {
            Vector3 spiderPos = spider.position;
            float d = Vector2.Distance(
                new Vector2(playerPos.x, playerPos.z),
                new Vector2(spiderPos.x, spiderPos.z));

            if (d < 14f)
            {
                center.x = (playerPos.x + spiderPos.x) * 0.5f;
                center.z = (playerPos.z + spiderPos.z) * 0.5f;
            }
        }

        float floorY = playerPos.y;

        GameObject root = new GameObject(HouseRootName);
        SceneManager.MoveGameObjectToScene(root, scene);
        root.transform.position = new Vector3(center.x, floorY, center.z);

        Transform architecture = NewGroup("01_ARQUITECTURA", root.transform);
        Transform furniture = NewGroup("02_MUEBLES", root.transform);
        Transform decoration = NewGroup("03_DECORACION", root.transform);
        Transform lights = NewGroup("04_LUCES", root.transform);

        Material floorMat = GetOrCreateMaterial(
            "Assets/NeuroPhobia_Level45/Generated/Materials/M_L4_Hogar_Madera.mat",
            new Color(0.33f, 0.20f, 0.11f, 1f),
            0.15f);

        Material wallMat = GetOrCreateMaterial(
            "Assets/NeuroPhobia_Level45/Generated/Materials/M_L4_Hogar_Pared_Crema.mat",
            new Color(0.82f, 0.76f, 0.66f, 1f),
            0.05f);

        Material lowerWallMat = GetOrCreateMaterial(
            "Assets/NeuroPhobia_Level45/Generated/Materials/M_L4_Hogar_Moldura.mat",
            new Color(0.56f, 0.48f, 0.39f, 1f),
            0.08f);

        Material ceilingMat = GetOrCreateMaterial(
            "Assets/NeuroPhobia_Level45/Generated/Materials/M_L4_Hogar_Techo.mat",
            new Color(0.90f, 0.88f, 0.82f, 1f),
            0.02f);

        Material rugMat = GetOrCreateMaterial(
            "Assets/NeuroPhobia_Level45/Generated/Materials/M_L4_Hogar_Alfombra.mat",
            new Color(0.20f, 0.28f, 0.31f, 1f),
            0.1f);

        Material trimMat = GetOrCreateMaterial(
            "Assets/NeuroPhobia_Level45/Generated/Materials/M_L4_Hogar_Marco.mat",
            new Color(0.20f, 0.13f, 0.09f, 1f),
            0.1f);

        // CASA MÁS GRANDE: 16 x 12 m, altura 3.4 m.
        CreateCube("Piso_Seguro_16x12",
            architecture,
            new Vector3(0f, -0.10f, 0f),
            new Vector3(16f, 0.20f, 12f),
            floorMat,
            true);

        CreateCube("Techo",
            architecture,
            new Vector3(0f, 3.45f, 0f),
            new Vector3(16f, 0.15f, 12f),
            ceilingMat,
            true);

        // Pared de fondo.
        CreateCube("Pared_Fondo",
            architecture,
            new Vector3(0f, 1.70f, 5.90f),
            new Vector3(16f, 3.40f, 0.20f),
            wallMat,
            true);

        // Laterales.
        CreateCube("Pared_Izquierda",
            architecture,
            new Vector3(-7.90f, 1.70f, 0f),
            new Vector3(0.20f, 3.40f, 12f),
            wallMat,
            true);

        CreateCube("Pared_Derecha",
            architecture,
            new Vector3(7.90f, 1.70f, 0f),
            new Vector3(0.20f, 3.40f, 12f),
            wallMat,
            true);

        // Frente con una abertura amplia de entrada.
        CreateCube("Pared_Frente_Izquierda",
            architecture,
            new Vector3(-5.15f, 1.70f, -5.90f),
            new Vector3(5.70f, 3.40f, 0.20f),
            wallMat,
            true);

        CreateCube("Pared_Frente_Derecha",
            architecture,
            new Vector3(5.15f, 1.70f, -5.90f),
            new Vector3(5.70f, 3.40f, 0.20f),
            wallMat,
            true);

        CreateCube("Pared_Frente_SobreEntrada",
            architecture,
            new Vector3(0f, 3.00f, -5.90f),
            new Vector3(4.60f, 0.80f, 0.20f),
            wallMat,
            true);

        // Diseño hogareño: zócalo/moldura inferior.
        CreateTrimSet(architecture, lowerWallMat);

        // Moldura superior.
        CreateCrownSet(architecture, trimMat);

        // Paneles decorativos de pared, sin collider.
        CreateWallPanels(decoration, trimMat, wallMat);

        // Alfombra grande.
        CreateCube("Alfombra_Central",
            decoration,
            new Vector3(0f, 0.025f, 0.80f),
            new Vector3(6.2f, 0.05f, 4.1f),
            rugMat,
            false);

        // Muebles principales: pocos, separados y con zonas libres.
        List<string> missing = new List<string>();

        PlacePrefab("(Prb)Sofa_pillowT.prefab",
            "Sofa_Principal",
            furniture,
            new Vector3(0f, 0f, 3.65f),
            new Vector3(0f, 180f, 0f),
            true,
            missing);

        PlacePrefab("(Prb)Chair1.prefab",
            "Sillon_Izquierdo",
            furniture,
            new Vector3(-4.45f, 0f, 1.20f),
            new Vector3(0f, 90f, 0f),
            true,
            missing);

        PlacePrefab("(Prb)Chair2.prefab",
            "Sillon_Derecho",
            furniture,
            new Vector3(4.45f, 0f, 1.20f),
            new Vector3(0f, -90f, 0f),
            true,
            missing);

        PlacePrefab("(Prb)CoffeTable_smallPlant.prefab",
            "Mesa_Centro",
            furniture,
            new Vector3(0f, 0f, 1.05f),
            Vector3.zero,
            true,
            missing);

        PlacePrefab("(Prb)MediaConsole.prefab",
            "Mueble_TV",
            furniture,
            new Vector3(0f, 0f, -4.95f),
            Vector3.zero,
            true,
            missing);

        PlacePrefab("(Prb)Shelf1.prefab",
            "Estanteria",
            furniture,
            new Vector3(6.45f, 0f, 3.75f),
            new Vector3(0f, -90f, 0f),
            true,
            missing);

        PlacePrefab("(Prb)Lamp.prefab",
            "Lampara_Pie",
            decoration,
            new Vector3(-6.10f, 0f, 3.75f),
            Vector3.zero,
            true,
            missing);

        PlacePrefab("(Prb)LaunchTable_vase2.prefab",
            "Mesa_Lateral_Planta",
            decoration,
            new Vector3(-6.15f, 0f, -3.90f),
            new Vector3(0f, 90f, 0f),
            true,
            missing);

        PlacePrefab("(Prb)Dresser.prefab",
            "Comoda",
            furniture,
            new Vector3(6.25f, 0f, -3.85f),
            new Vector3(0f, -90f, 0f),
            true,
            missing);

        // Luces cálidas organizadas. No crea cámaras ni AudioListeners.
        CreateWarmPointLight(
            "Luz_Calida_Centro",
            lights,
            new Vector3(0f, 2.85f, 0.8f),
            2.2f,
            7.5f);

        CreateWarmPointLight(
            "Luz_Calida_Fondo",
            lights,
            new Vector3(0f, 2.75f, 4.0f),
            1.7f,
            6.0f);

        CreateWarmPointLight(
            "Luz_Calida_Lateral",
            lights,
            new Vector3(-5.2f, 2.55f, -2.8f),
            1.25f,
            5.5f);

        EditorUtility.SetDirty(root);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeGameObject = root;
        if (SceneView.lastActiveSceneView != null)
            SceneView.lastActiveSceneView.FrameSelected();

        string missingText = missing.Count == 0
            ? "Todos los muebles principales fueron encontrados."
            : "No encontré estos prefabs: " + string.Join(", ", missing);

        EditorUtility.DisplayDialog(
            "NeuroPhobia · Nivel 4",
            "Casa organizada creada.\n\n" +
            "• Espacio: 16 x 12 m\n" +
            "• Piso sólido con collider\n" +
            "• Paredes y techo con collider\n" +
            "• Molduras y paneles hogareños\n" +
            "• Muebles separados y ordenados\n" +
            "• Iluminación cálida\n" +
            "• Gameplay, araña, jugador, selector PC/RV y UI NO se movieron\n\n" +
            missingText +
            "\n\nSe creó un respaldo en:\n" + BackupPath,
            "Aceptar");
    }

    [MenuItem("Tools/NeuroPhobia/Nivel 4/Ocultar objeto seleccionado del escenario viejo", priority = 211)]
    public static void HideSelectedOldObject()
    {
        if (EditorApplication.isPlaying)
            return;

        GameObject go = Selection.activeGameObject;

        if (go == null)
        {
            EditorUtility.DisplayDialog(
                "NeuroPhobia",
                "Selecciona en Hierarchy el objeto viejo que quieres ocultar.",
                "Aceptar");
            return;
        }

        if (IsProtectedGameplayName(go.name))
        {
            EditorUtility.DisplayDialog(
                "NeuroPhobia",
                "Ese objeto parece ser de gameplay, jugador, araña, UI o controles.\nNo lo oculté.",
                "Aceptar");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(go, "Ocultar entorno viejo Nivel 4");

        if (!go.name.StartsWith(OldPrefix, StringComparison.Ordinal))
            go.name = OldPrefix + go.name;

        go.SetActive(false);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        EditorUtility.DisplayDialog(
            "NeuroPhobia",
            "Objeto viejo ocultado. No fue borrado.",
            "Aceptar");
    }

    [MenuItem("Tools/NeuroPhobia/Nivel 4/Restaurar objetos viejos ocultos", priority = 212)]
    public static void RestoreOldObjects()
    {
        Scene scene = SceneManager.GetActiveScene();
        int restored = 0;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name.StartsWith(OldPrefix, StringComparison.Ordinal))
            {
                Undo.RegisterFullObjectHierarchyUndo(root, "Restaurar entorno viejo Nivel 4");
                root.name = root.name.Substring(OldPrefix.Length);
                root.SetActive(true);
                restored++;
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);

        EditorUtility.DisplayDialog(
            "NeuroPhobia",
            "Objetos restaurados: " + restored,
            "Aceptar");
    }

    private static void AutoHideOldEnvironmentRoots(Scene scene)
    {
        string[] visualHints =
        {
            "np_env_nivel4",
            "np_env_level4",
            "nivel4_environment",
            "level4_environment",
            "nivel4_house",
            "nivel4_casa",
            "np_l4_environment",
            "np_l4_visual",
            "np_level4_visual"
        };

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name == HouseRootName)
                continue;

            string lower = root.name.ToLowerInvariant();

            if (IsProtectedGameplayName(lower))
                continue;

            bool match = false;

            foreach (string hint in visualHints)
            {
                if (lower.Contains(hint))
                {
                    match = true;
                    break;
                }
            }

            if (!match)
                continue;

            if (!root.name.StartsWith(OldPrefix, StringComparison.Ordinal))
                root.name = OldPrefix + root.name;

            root.SetActive(false);
            EditorUtility.SetDirty(root);
        }
    }

    private static bool IsProtectedGameplayName(string name)
    {
        string lower = name.ToLowerInvariant();

        string[] protectedWords =
        {
            "player",
            "desktop",
            "xr",
            "camera",
            "spider",
            "arana",
            "araña",
            "exposure",
            "checkpoint",
            "controller",
            "control",
            "selector",
            "ui",
            "canvas",
            "hud",
            "manager",
            "eventsystem",
            "levelmanager"
        };

        foreach (string word in protectedWords)
            if (lower.Contains(word))
                return true;

        return false;
    }

    private static Vector3 FindBestPlayerPosition(Scene scene)
    {
        GameObject pc = FindSceneObject(scene, "NP_DesktopPlayer_Normalizado");

        if (pc == null)
            pc = FindSceneObject(scene, "NP_DesktopPlayer_Niveles45");

        if (pc == null)
            pc = FindSceneObject(scene, "DesktopTestRig");

        if (pc != null)
            return pc.transform.position;

        GameObject xr = FindSceneObject(scene, "XR Origin (XR Rig)");

        if (xr == null)
            xr = FindSceneObject(scene, "XR Origin");

        if (xr != null)
            return xr.transform.position;

        return Vector3.zero;
    }

    private static Transform FindSpider(Scene scene)
    {
        Transform best = null;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform candidate = FindSpiderRecursive(root.transform);

            if (candidate != null)
                return candidate;
        }

        return best;
    }

    private static Transform FindSpiderRecursive(Transform current)
    {
        string lower = current.name.ToLowerInvariant();

        if (lower.Contains("spider") ||
            lower.Contains("arana") ||
            lower.Contains("araña"))
        {
            Renderer r = current.GetComponentInChildren<Renderer>(true);

            if (r != null)
                return current;
        }

        foreach (Transform child in current)
        {
            Transform result = FindSpiderRecursive(child);

            if (result != null)
                return result;
        }

        return null;
    }

    private static Transform NewGroup(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    private static GameObject CreateCube(
        string name,
        Transform parent,
        Vector3 localPosition,
        Vector3 localScale,
        Material material,
        bool colliderEnabled)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = localScale;

        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null && material != null)
            renderer.sharedMaterial = material;

        Collider c = go.GetComponent<Collider>();

        if (c != null)
            c.enabled = colliderEnabled;

        return go;
    }

    private static void CreateTrimSet(Transform parent, Material mat)
    {
        CreateCube("Zocalo_Fondo",
            parent,
            new Vector3(0f, 0.48f, 5.76f),
            new Vector3(15.75f, 0.95f, 0.10f),
            mat,
            false);

        CreateCube("Zocalo_Izquierda",
            parent,
            new Vector3(-7.76f, 0.48f, 0f),
            new Vector3(0.10f, 0.95f, 11.75f),
            mat,
            false);

        CreateCube("Zocalo_Derecha",
            parent,
            new Vector3(7.76f, 0.48f, 0f),
            new Vector3(0.10f, 0.95f, 11.75f),
            mat,
            false);

        CreateCube("Zocalo_Frente_Izq",
            parent,
            new Vector3(-5.15f, 0.48f, -5.76f),
            new Vector3(5.55f, 0.95f, 0.10f),
            mat,
            false);

        CreateCube("Zocalo_Frente_Der",
            parent,
            new Vector3(5.15f, 0.48f, -5.76f),
            new Vector3(5.55f, 0.95f, 0.10f),
            mat,
            false);
    }

    private static void CreateCrownSet(Transform parent, Material mat)
    {
        float y = 3.18f;

        CreateCube("Moldura_Superior_Fondo",
            parent,
            new Vector3(0f, y, 5.72f),
            new Vector3(15.70f, 0.12f, 0.12f),
            mat,
            false);

        CreateCube("Moldura_Superior_Izquierda",
            parent,
            new Vector3(-7.72f, y, 0f),
            new Vector3(0.12f, 0.12f, 11.70f),
            mat,
            false);

        CreateCube("Moldura_Superior_Derecha",
            parent,
            new Vector3(7.72f, y, 0f),
            new Vector3(0.12f, 0.12f, 11.70f),
            mat,
            false);
    }

    private static void CreateWallPanels(
        Transform parent,
        Material frameMat,
        Material insideMat)
    {
        // Tres cuadros/paneles decorativos en la pared de fondo.
        float[] xs = { -4.4f, 0f, 4.4f };

        foreach (float x in xs)
        {
            CreateCube(
                "Marco_Pared_" + x.ToString("0.0"),
                parent,
                new Vector3(x, 2.05f, 5.72f),
                new Vector3(2.20f, 1.25f, 0.055f),
                frameMat,
                false);

            CreateCube(
                "Panel_Pared_" + x.ToString("0.0"),
                parent,
                new Vector3(x, 2.05f, 5.66f),
                new Vector3(1.92f, 0.97f, 0.055f),
                insideMat,
                false);
        }
    }

    private static void PlacePrefab(
        string prefabFile,
        string objectName,
        Transform parent,
        Vector3 localPosition,
        Vector3 localEuler,
        bool addColliderIfMissing,
        List<string> missing)
    {
        string path = SoftpackBase + prefabFile;
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

        if (prefab == null)
        {
            missing.Add(prefabFile);
            return;
        }

        GameObject instance =
            PrefabUtility.InstantiatePrefab(prefab) as GameObject;

        if (instance == null)
        {
            missing.Add(prefabFile);
            return;
        }

        instance.name = objectName;
        instance.transform.SetParent(parent, false);
        instance.transform.localPosition = localPosition;
        instance.transform.localRotation = Quaternion.Euler(localEuler);
        instance.transform.localScale = Vector3.one;

        SnapToFloor(instance, parent.position.y);

        if (addColliderIfMissing)
            AddSimpleColliderIfMissing(instance);
    }

    private static void SnapToFloor(GameObject go, float worldFloorY)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);

        if (renderers == null || renderers.Length == 0)
            return;

        Bounds b = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
            b.Encapsulate(renderers[i].bounds);

        float delta = worldFloorY - b.min.y;
        go.transform.position += Vector3.up * delta;
    }

    private static void AddSimpleColliderIfMissing(GameObject go)
    {
        Collider[] existing = go.GetComponentsInChildren<Collider>(true);

        if (existing != null && existing.Length > 0)
            return;

        Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);

        if (renderers == null || renderers.Length == 0)
            return;

        Bounds b = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
            b.Encapsulate(renderers[i].bounds);

        BoxCollider box = go.AddComponent<BoxCollider>();
        box.center = go.transform.InverseTransformPoint(b.center);

        Vector3 scale = go.transform.lossyScale;

        float sx = Mathf.Abs(scale.x) < 0.0001f ? 1f : Mathf.Abs(scale.x);
        float sy = Mathf.Abs(scale.y) < 0.0001f ? 1f : Mathf.Abs(scale.y);
        float sz = Mathf.Abs(scale.z) < 0.0001f ? 1f : Mathf.Abs(scale.z);

        box.size = new Vector3(
            b.size.x / sx,
            b.size.y / sy,
            b.size.z / sz);

        box.isTrigger = false;
    }

    private static void CreateWarmPointLight(
        string name,
        Transform parent,
        Vector3 localPosition,
        float intensity,
        float range)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;

        Light light = go.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1.0f, 0.78f, 0.58f, 1f);
        light.intensity = intensity;
        light.range = range;
        light.shadows = LightShadows.Soft;
    }

    private static Material GetOrCreateMaterial(
        string path,
        Color color,
        float smoothness)
    {
        EnsureFolder(Path.GetDirectoryName(path).Replace("\\", "/"));

        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

        if (mat == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");

            if (shader == null)
                shader = Shader.Find("Standard");

            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }

        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color);
        else
            mat.color = color;

        if (mat.HasProperty("_Smoothness"))
            mat.SetFloat("_Smoothness", smoothness);

        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static void EnsureFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder))
            return;

        string[] parts = folder.Split('/');
        string current = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];

            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);

            current = next;
        }
    }

    private static GameObject FindSceneObject(
        Scene scene,
        string exactName)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform found = FindRecursive(root.transform, exactName);

            if (found != null)
                return found.gameObject;
        }

        return null;
    }

    private static Transform FindRecursive(
        Transform current,
        string exactName)
    {
        if (current == null)
            return null;

        if (string.Equals(
                current.name,
                exactName,
                StringComparison.OrdinalIgnoreCase))
            return current;

        foreach (Transform child in current)
        {
            Transform result = FindRecursive(child, exactName);

            if (result != null)
                return result;
        }

        return null;
    }
}
#endif
