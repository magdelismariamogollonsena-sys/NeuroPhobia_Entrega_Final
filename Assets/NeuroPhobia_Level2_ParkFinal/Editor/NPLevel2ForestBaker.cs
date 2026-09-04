#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class NPLevel2ForestBaker
{
    private const string ScenePath = "Assets/Scenes/Nivel2.unity";
    private const string RootName = "NP_ENV_NIVEL2_PARQUE_GRANDE_FINAL";
    private const string OldRootName = "NP_ENV_NIVEL2_JARDIN_REALISTA";
    private const string ModelRoot = "Assets/NeuroPhobia_Level2_ParkFinal/Models/Nature/";

    private static readonly string[] TreeModels =
    {
        "tree_oak.fbx", "tree_detailed.fbx", "tree_default.fbx", "tree_tall.fbx",
        "tree_pineTallA.fbx", "tree_pineRoundC.fbx", "tree_fat.fbx", "tree_small.fbx"
    };

    private static readonly string[] BushModels =
    {
        "plant_bush.fbx", "plant_bushDetailed.fbx", "plant_bushLarge.fbx"
    };

    private static readonly string[] PlantModels =
    {
        "flower_purpleA.fbx", "flower_redA.fbx", "flower_yellowA.fbx",
        "grass_large.fbx", "grass_leafsLarge.fbx"
    };

    private static readonly string[] RockModels =
    {
        "rock_largeA.fbx", "rock_largeB.fbx", "rock_largeC.fbx",
        "rock_smallA.fbx", "rock_smallB.fbx", "rock_smallC.fbx"
    };

    [MenuItem("Tools/NeuroPhobia/Nivel 2/Hornear bosque final en escena", priority = 192)]
    public static void Bake()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject root = FindRoot(scene, RootName);
        if (root == null) throw new InvalidOperationException("No se encontró " + RootName);

        RemoveRuntimeDecorator(root);
        Transform trees = FindOrCreateGroup(root.transform, "02_ARBOLES");
        Transform bushes = FindOrCreateGroup(root.transform, "03_ARBUSTOS_FLORES");
        Transform rocks = FindOrCreateGroup(root.transform, "04_ROCAS");

        RemoveGeneratedChildren(trees, "Arbol_Bosque_");
        RemoveGeneratedChildren(bushes, "Arbusto_Bosque_");
        RemoveGeneratedChildren(bushes, "Vegetacion_Bosque_");
        RemoveGeneratedChildren(rocks, "Roca_Bosque_");

        PlaceSet(scene, TreeModels, trees, "Arbol_Bosque_", 72, 16.5f, 29f, .82f, 3.4f, 6.8f, 137.508f);
        PlaceSet(scene, BushModels, bushes, "Arbusto_Bosque_", 52, 11.5f, 25.5f, .84f, 1.25f, 2.45f, 111.247f);
        PlaceSet(scene, PlantModels, bushes, "Vegetacion_Bosque_", 18, 8.5f, 23f, .86f, .65f, 1.15f, 83.731f);
        PlaceSet(scene, RockModels, rocks, "Roca_Bosque_", 12, 13f, 26f, .84f, 1.1f, 2.4f, 149.119f);

        RestoreExposureObjects(scene, root);
        GameObject oldRoot = FindRoot(scene, OldRootName);
        if (oldRoot != null) oldRoot.SetActive(false);
        DisableWaterObjects(scene);

        EditorUtility.SetDirty(root);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("No se pudo guardar Nivel2.unity");
        Debug.Log("[Nivel2] Bosque serializado: 72 árboles, 52 arbustos, 18 plantas y 12 rocas.");
    }

    [MenuItem("Tools/NeuroPhobia/Nivel 2/Restaurar araña y roca de exposición", priority = 193)]
    public static void RestoreExposureObjects()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject root = FindRoot(scene, RootName);
        if (root == null) throw new InvalidOperationException("No se encontró " + RootName);

        RestoreExposureObjects(scene, root);
        GameObject oldRoot = FindRoot(scene, OldRootName);
        if (oldRoot != null) oldRoot.SetActive(false);

        EditorUtility.SetDirty(root);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("No se pudo guardar Nivel2.unity");
        Debug.Log("[Nivel2] Araña y roca originales restauradas en 08_ZONA_EXPOSICION.");
    }

    private static void RestoreExposureObjects(Scene scene, GameObject environmentRoot)
    {
        Transform exposureGroup = FindOrCreateGroup(environmentRoot.transform, "08_ZONA_EXPOSICION");
        Transform spider = FindSceneTransform(scene, "NP_WolfSpider_Nivel2_Static");
        Transform rock = FindSceneTransform(scene, "Roca_Realista");

        if (spider == null) throw new InvalidOperationException("No se encontró la araña original del Nivel 2.");
        if (rock == null) throw new InvalidOperationException("No se encontró la roca original del Nivel 2.");

        spider.SetParent(exposureGroup, true);
        rock.SetParent(exposureGroup, true);
        spider.gameObject.SetActive(true);
        rock.gameObject.SetActive(true);
        EditorUtility.SetDirty(spider.gameObject);
        EditorUtility.SetDirty(rock.gameObject);
    }

    private static void PlaceSet(Scene scene, string[] models, Transform parent, string prefix, int count,
        float minimumRadius, float maximumRadius, float zCompression,
        float minimumSize, float maximumSize, float angleStep)
    {
        int placed = 0;
        int candidate = 0;
        while (placed < count && candidate < count * 8)
        {
            float angle = (candidate * angleStep + Hash01(candidate + 31) * 24f) * Mathf.Deg2Rad;
            float radius = Mathf.Lerp(minimumRadius, maximumRadius, Hash01(candidate * 7 + 17));
            Vector3 position = new Vector3(
                Mathf.Sin(angle) * radius + (Hash01(candidate * 5 + 3) - .5f) * 2.8f,
                0f,
                Mathf.Cos(angle) * radius * zCompression + (Hash01(candidate * 11 + 9) - .5f) * 2.2f);
            candidate++;
            if (!IsClearOfGameplay(position)) continue;

            string path = ModelRoot + models[placed % models.Length];
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) throw new InvalidOperationException("Falta el modelo: " + path);

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject;
            if (instance == null) throw new InvalidOperationException("No se pudo instanciar: " + path);
            instance.name = prefix + (placed + 1).ToString("00");
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = position;
            instance.transform.localRotation = Quaternion.Euler(0f, Hash01(candidate * 13) * 360f, 0f);
            instance.transform.localScale = Vector3.one;

            float size = Mathf.Lerp(minimumSize, maximumSize, Hash01(candidate * 19 + 5));
            NormalizeHorizontalSize(instance, size);
            SnapToGround(instance, parent.root.position.y);
            placed++;
        }

        if (placed != count) throw new InvalidOperationException("Solo se ubicaron " + placed + " de " + count + " objetos " + prefix);
    }

    private static bool IsClearOfGameplay(Vector3 position)
    {
        if (Mathf.Abs(position.x) < 3.2f && position.z < 19f) return false;
        Vector2 fromSpider = new Vector2(position.x, position.z - 10f);
        if (fromSpider.sqrMagnitude < 11.5f * 11.5f) return false;
        if (Mathf.Abs(position.z + 4f) < 2.6f && Mathf.Abs(position.x) < 20f) return false;
        return true;
    }

    private static void NormalizeHorizontalSize(GameObject instance, float targetSize)
    {
        if (!TryGetBounds(instance, out Bounds bounds)) return;
        float horizontalSize = Mathf.Max(bounds.size.x, bounds.size.z);
        if (horizontalSize > .001f) instance.transform.localScale *= targetSize / horizontalSize;
    }

    private static void SnapToGround(GameObject instance, float floorY)
    {
        if (TryGetBounds(instance, out Bounds bounds))
            instance.transform.position += Vector3.up * (floorY - bounds.min.y);
    }

    private static bool TryGetBounds(GameObject instance, out Bounds bounds)
    {
        Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) { bounds = default; return false; }
        bounds = renderers[0].bounds;
        for (int index = 1; index < renderers.Length; index++) bounds.Encapsulate(renderers[index].bounds);
        return true;
    }

    private static Transform FindOrCreateGroup(Transform root, string name)
    {
        Transform group = root.Find(name);
        if (group != null) return group;
        GameObject created = new GameObject(name);
        created.transform.SetParent(root, false);
        return created.transform;
    }

    private static void RemoveGeneratedChildren(Transform parent, string prefix)
    {
        for (int index = parent.childCount - 1; index >= 0; index--)
            if (parent.GetChild(index).name.StartsWith(prefix, StringComparison.Ordinal))
                UnityEngine.Object.DestroyImmediate(parent.GetChild(index).gameObject);
    }

    private static void RemoveRuntimeDecorator(GameObject root)
    {
        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root);
    }

    private static void DisableWaterObjects(Scene scene)
    {
        string[] words = { "water", "agua", "lake", "lago", "ocean", "mar", "river", "rio", "laguna", "pond", "pool" };
        foreach (GameObject root in scene.GetRootGameObjects()) DisableWaterRecursive(root.transform, words);
    }

    private static void DisableWaterRecursive(Transform current, string[] words)
    {
        string lowerName = current.name.ToLowerInvariant();
        foreach (string word in words)
            if (lowerName.Contains(word)) { current.gameObject.SetActive(false); return; }
        foreach (Transform child in current) DisableWaterRecursive(child, words);
    }

    private static GameObject FindRoot(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
            if (string.Equals(root.name, name, StringComparison.OrdinalIgnoreCase)) return root;
        return null;
    }

    private static Transform FindSceneTransform(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform found = FindRecursive(root.transform, name);
            if (found != null) return found;
        }
        return null;
    }

    private static Transform FindRecursive(Transform current, string name)
    {
        if (string.Equals(current.name, name, StringComparison.OrdinalIgnoreCase)) return current;
        foreach (Transform child in current)
        {
            Transform found = FindRecursive(child, name);
            if (found != null) return found;
        }
        return null;
    }

    private static float Hash01(int value)
    {
        float hashed = Mathf.Sin(value * 12.9898f + 78.233f) * 43758.5453f;
        return hashed - Mathf.Floor(hashed);
    }
}
#endif
