#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Preparación visual reversible de Nivel5. No configura gameplay ni XR.</summary>
public static class NeuroPhobiaNivel5VisualPrep
{
    private const string ScenePath = "Assets/Scenes/Nivel5.unity";
    private const string House = "Assets/nappin/House Props Softpack/Prefabs/";
    private const string Interior = "Assets/UnityTechnologies/Basic Asset Pack Interior/Prefabs/";

    [MenuItem("Tools/NeuroPhobia/Nivel 5/Preparar apartamento visual", priority = 450)]
    public static void Prepare()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("Detén Play Mode antes de preparar Nivel5."); return; }
        Scene active = SceneManager.GetActiveScene();
        if (active.isDirty && active.path != ScenePath)
        {
            Debug.LogError("Hay otra escena con cambios sin guardar. Guárdala antes de abrir Nivel5.");
            return;
        }
        if (active.path != ScenePath) active = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        if (!active.IsValid()) throw new InvalidOperationException("No se pudo abrir Nivel5.");

        GameObject existing = FindRoot(active, "NP_L5_APARTAMENTO_VISUAL");
        if (existing != null)
        {
            if (EnsureDetails(existing.transform))
            {
                EditorSceneManager.MarkSceneDirty(active);
                EditorSceneManager.SaveScene(active);
                Debug.Log("Nivel5 visual: detalles faltantes añadidos sin duplicar el apartamento.");
            }
            else Debug.Log("Nivel5 visual ya preparado: no se duplicó nada.");
            return;
        }

        // Verificar los prefabs antes de crear objetos: un fallo no deja un apartamento parcial guardado.
        string[] required = {
            House + "(Prb)Sofa.prefab", House + "(Prb)CoffeTable.prefab",
            House + "(Prb)MediaConsole.prefab", House + "(Prb)Lamp.prefab",
            House + "(Prb)Shelf1.prefab", House + "(Prb)Fridge.prefab",
            House + "(Prb)Stove.prefab", House + "(Prb)KitchenSink.prefab",
            House + "(Prb)KitchenIsland.prefab", House + "(Prb)DiningChair.prefab",
            House + "(Prb)DoubleBed.prefab", House + "(Prb)BedsideTable.prefab",
            House + "(Prb)Wardrobe.prefab", House + "(Prb)Dresser.prefab",
            House + "(Prb)Toilet.prefab", House + "(Prb)BathroomSink.prefab",
            House + "(Prb)Mirror1.prefab", House + "(Prb)ShowerFace.prefab",
            House + "(Prb)ShowerTemperature.prefab", House + "(Prb)Door.prefab",
            House + "(Prb)RoomLight.prefab", House + "(Prb)SoapDispenser.prefab",
            House + "(Prb)Storage1.prefab",
            Interior + "Walls/WallWindow2m.prefab", Interior + "Floor/Floor4m.prefab",
            Interior + "Floor/RugRectangleMedium.prefab", Interior + "Props/PlantPotMedium.prefab"
        };
        foreach (string path in required)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) throw new InvalidOperationException("Prefab faltante: " + path);
            if (prefab.GetComponentsInChildren<MonoBehaviour>(true).Length > 0)
                throw new InvalidOperationException("El prefab trae scripts; revisar antes de usar: " + path);
        }

        GameObject root = new GameObject("NP_L5_APARTAMENTO_VISUAL");
        SceneManager.MoveGameObjectToScene(root, active);
        Transform sala = Child(root.transform, "01_SALA");
        Transform cocina = Child(root.transform, "02_COCINA");
        Transform cuarto = Child(root.transform, "03_CUARTO");
        Transform bano = Child(root.transform, "04_BANO");
        Transform architecture = Child(root.transform, "05_PUERTAS_VENTANAS");
        Transform decor = Child(root.transform, "06_DECORACION");
        Transform extra = Child(root.transform, "07_MOBILIARIO_EXTRA");

        // Cuatro habitaciones de 6 x 6 m. El spawn PC existente (0, 0, -7.8) cae en sala.
        Place(sala, Interior + "Floor/Floor4m.prefab", "Piso_Sala", -0.02f, 0f, -9f, 6f);
        Place(cocina, Interior + "Floor/Floor4m.prefab", "Piso_Cocina", -0.02f, 6f, -9f, 6f);
        Place(cuarto, Interior + "Floor/Floor4m.prefab", "Piso_Cuarto", -0.02f, 0f, -3f, 6f);
        Place(bano, Interior + "Floor/Floor4m.prefab", "Piso_Bano", -0.02f, 6f, -3f, 6f);

        // Muros de 3.2 m sin colliders. Aberturas de 1.6–2 m para puerta y pasos VR.
        Wall(architecture, "Exterior_Oeste", -3f, -6f, .12f, 12f);
        Wall(architecture, "Exterior_Este", 9f, -6f, .12f, 12f);
        Wall(architecture, "Exterior_Sur_Izq", -1.9f, -12f, 2.2f, .12f);
        Wall(architecture, "Exterior_Sur_Der", 4.9f, -12f, 8.2f, .12f);
        Wall(architecture, "Exterior_Norte_Izq", -2f, 0f, 2f, .12f);
        Wall(architecture, "Exterior_Norte_Der", 5f, 0f, 8f, .12f);

        Wall(architecture, "Divisor_Sala_Cocina_A", 3f, -11f, .12f, 2f);
        Wall(architecture, "Divisor_Sala_Cocina_B", 3f, -7.2f, .12f, 2.4f);
        Wall(architecture, "Divisor_Cuarto_Bano_A", 3f, -5f, .12f, 2f);
        Wall(architecture, "Divisor_Cuarto_Bano_B", 3f, -1.2f, .12f, 2.4f);
        Wall(architecture, "Divisor_Sala_Cuarto_A", -2.05f, -6f, 1.9f, .12f);
        Wall(architecture, "Divisor_Sala_Cuarto_B", 1.95f, -6f, 2.1f, .12f);
        Wall(architecture, "Divisor_Cocina_Bano_A", 4f, -6f, 2f, .12f);
        Wall(architecture, "Divisor_Cocina_Bano_B", 7.8f, -6f, 2.4f, .12f);

        Place(architecture, House + "(Prb)Door.prefab", "NP_L5_DOOR_VISUAL", 0f, 0f, -12f, 2.3f, 70f);
        Place(architecture, Interior + "Walls/WallWindow2m.prefab", "NP_L5_WINDOW_VISUAL", 0f, 0f, 0f, 3.2f);

        Place(sala, House + "(Prb)Sofa.prefab", "Sofa_Sala", 0f, -1.5f, -10.5f, 2.1f, 90f);
        Place(sala, House + "(Prb)CoffeTable.prefab", "Mesa_Centro_Sala", 0f, -1.1f, -9.1f, 1.2f);
        Place(sala, House + "(Prb)MediaConsole.prefab", "Mueble_TV_Sala", 0f, 1.8f, -10.5f, 1.7f, 270f);
        Place(sala, House + "(Prb)Lamp.prefab", "NP_L5_LAMP_VISUAL", 0f, -2.4f, -7.0f, 1.6f);
        Place(sala, House + "(Prb)Shelf1.prefab", "Estanteria_Sala", 0f, -2.3f, -11.4f, 1.9f);
        Place(sala, Interior + "Floor/RugRectangleMedium.prefab", "Alfombra_Sala", 0.015f, -1.0f, -9.1f, 2.2f);

        Place(cocina, House + "(Prb)Fridge.prefab", "NP_L5_FRIDGE_VISUAL", 0f, 8.15f, -10.75f, 1.9f, 270f);
        Place(cocina, House + "(Prb)Stove.prefab", "Estufa_Cocina", 0f, 8.15f, -8.8f, 1.3f, 270f);
        Place(cocina, House + "(Prb)KitchenSink.prefab", "Fregadero_Cocina", 0f, 5.0f, -11.1f, 1.4f);
        Place(cocina, House + "(Prb)KitchenIsland.prefab", "Isla_Cocina", 0f, 6.1f, -8.7f, 1.7f);
        Place(cocina, House + "(Prb)DiningChair.prefab", "Silla_Cocina_01", 0f, 4.5f, -7.3f, 1.0f);
        Place(cocina, House + "(Prb)DiningChair.prefab", "Silla_Cocina_02", 0f, 7.6f, -7.3f, 1.0f);

        Place(cuarto, House + "(Prb)DoubleBed.prefab", "Cama_Cuarto", 0f, -1.0f, -2.3f, 2.4f, 180f);
        Place(cuarto, House + "(Prb)BedsideTable.prefab", "Mesa_Noche_01", 0f, -2.2f, -3.9f, .7f);
        Place(cuarto, House + "(Prb)BedsideTable.prefab", "Mesa_Noche_02", 0f, .2f, -3.9f, .7f);
        Place(cuarto, House + "(Prb)Wardrobe.prefab", "Armario_Cuarto", 0f, 2.0f, -1.0f, 2.0f, 270f);
        Place(cuarto, House + "(Prb)Dresser.prefab", "Comoda_Cuarto", 0f, -2.1f, -1.0f, 1.4f);
        Place(cuarto, House + "(Prb)RoomLight.prefab", "Lampara_Cuarto", 0f, .7f, -1.1f, 1.4f);

        Place(bano, House + "(Prb)Toilet.prefab", "Inodoro_Bano", 0f, 8.1f, -1.1f, .85f, 270f);
        Place(bano, House + "(Prb)BathroomSink.prefab", "Lavamanos_Bano", 0f, 5.0f, -1.0f, 1.0f);
        PlaceMounted(bano, House + "(Prb)Mirror1.prefab", "Espejo_Bano", 5.0f, 1.55f, -.2f, 1.0f);
        PlaceMounted(bano, House + "(Prb)ShowerFace.prefab", "Ducha_Griferia_Bano", 8.2f, 1.9f, -5.3f, .8f);
        PlaceMounted(bano, House + "(Prb)ShowerTemperature.prefab", "Ducha_Control_Bano", 8.2f, 1.0f, -5.3f, .35f);
        Place(bano, House + "(Prb)SoapDispenser.prefab", "Accesorio_Jabon_Bano", 0f, 5.7f, -.7f, .25f);
        EnsureDetails(root.transform);

        Place(decor, Interior + "Props/PlantPotMedium.prefab", "Planta_Sala", 0f, 2.3f, -7.1f, 1.1f);
        Place(extra, House + "(Prb)Lamp.prefab", "Lampara_Extra", 0f, 4.0f, -10.8f, 1.5f);
        AmbientLight(decor, "Luz_Sala", 0f, -9f);
        AmbientLight(decor, "Luz_Cocina", 6f, -9f);
        AmbientLight(decor, "Luz_Cuarto", 0f, -3f);
        AmbientLight(decor, "Luz_Bano", 6f, -3f);

        GameObject spawns = new GameObject("NP_L5_SPAWN_POINTS_FUTUROS");
        SceneManager.MoveGameObjectToScene(spawns, active);
        EmptyAt(spawns.transform, "Spawn_01", -2.5f, .1f, -11.3f);
        EmptyAt(spawns.transform, "Spawn_02", 8.4f, .1f, -7.0f);
        EmptyAt(spawns.transform, "Spawn_03", -2.5f, .1f, -.8f);
        EmptyAt(spawns.transform, "Spawn_04", 8.3f, .1f, -.8f);
        EmptyAt(spawns.transform, "Spawn_05", 2.2f, .1f, -10.8f);

        // Conserva el suelo antiguo como soporte físico. Solo oculta visuales de fábrica sin scripts.
        GameObject factory = FindDeep(active, "FactoryProps");
        DisableScriptFree(factory);
        GameObject environment = FindDeep(active, "Environment");
        if (environment != null)
        {
            foreach (Transform child in environment.transform)
            {
                if (child.name == "PISO_FABRICA_60x44") continue;
                DisableScriptFree(child.gameObject);
            }
        }

        if (root.GetComponentsInChildren<MonoBehaviour>(true).Length != 0 ||
            spawns.GetComponentsInChildren<MonoBehaviour>(true).Length != 0)
            throw new InvalidOperationException("Se detectó un script dentro de los roots visuales.");
        foreach (Collider collider in root.GetComponentsInChildren<Collider>(true)) collider.enabled = false;

        EditorSceneManager.MarkSceneDirty(active);
        if (!EditorSceneManager.SaveScene(active)) throw new InvalidOperationException("No se pudo guardar Nivel5.");
        Selection.activeGameObject = root;
        Debug.Log("Nivel5: apartamento visual preparado y guardado. No se agregaron tareas, arañas ni scripts de gameplay.");
    }

    private static Transform Child(Transform parent, string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    private static void EmptyAt(Transform parent, string name, float x, float y, float z)
    {
        Transform empty = Child(parent, name);
        empty.position = new Vector3(x, y, z);
    }

    private static void Wall(Transform parent, string name, float x, float z, float width, float depth)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, 1.6f, z);
        go.transform.localScale = new Vector3(width, 3.2f, depth);
        UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
    }

    private static GameObject Place(Transform parent, string path, string name, float floorY,
        float x, float z, float desiredLongest, float yaw = 0f)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null || prefab.GetComponentsInChildren<MonoBehaviour>(true).Length != 0)
            throw new InvalidOperationException("Prefab faltante o con scripts: " + path);
        GameObject instance = PrefabUtility.InstantiatePrefab(prefab, parent.gameObject.scene) as GameObject;
        if (instance == null) throw new InvalidOperationException("No se pudo instanciar " + path);
        instance.name = name;
        instance.transform.SetParent(parent, false);
        instance.transform.position = Vector3.zero;
        instance.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        Bounds bounds = BoundsOf(instance);
        float longest = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        if (longest <= .0001f) throw new InvalidOperationException("Prefab sin renderers: " + path);
        float factor = Mathf.Clamp(desiredLongest / longest, .1f, 10f);
        instance.transform.localScale *= factor;
        bounds = BoundsOf(instance);
        instance.transform.position += new Vector3(x - bounds.center.x, floorY - bounds.min.y, z - bounds.center.z);
        DisableComponents(instance);
        return instance;
    }

    private static void PlaceMounted(Transform parent, string path, string name,
        float x, float centerY, float z, float desiredLongest)
    {
        GameObject instance = Place(parent, path, name, 0f, x, z, desiredLongest);
        Bounds bounds = BoundsOf(instance);
        instance.transform.position += Vector3.up * (centerY - bounds.center.y);
    }

    private static Bounds BoundsOf(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return new Bounds(Vector3.zero, Vector3.zero);
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        return bounds;
    }

    private static void DisableComponents(GameObject go)
    {
        foreach (Collider collider in go.GetComponentsInChildren<Collider>(true)) collider.enabled = false;
        foreach (Animator animator in go.GetComponentsInChildren<Animator>(true)) animator.enabled = false;
    }

    private static void AmbientLight(Transform parent, string name, float x, float z)
    {
        Transform item = Child(parent, name);
        item.position = new Vector3(x, 2.75f, z);
        Light light = item.gameObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = 7f;
        light.intensity = 1.1f;
        light.color = new Color(1f, .91f, .78f);
        light.shadows = LightShadows.None;
    }

    private static GameObject FindRoot(Scene scene, string name)
    {
        foreach (GameObject go in scene.GetRootGameObjects()) if (go.name == name) return go;
        return null;
    }

    private static GameObject FindDeep(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                if (child.name == name) return child.gameObject;
        return null;
    }

    private static void DisableScriptFree(GameObject go)
    {
        if (go == null) return;
        if (go.GetComponentsInChildren<MonoBehaviour>(true).Length > 0)
        {
            Debug.LogWarning("Visual antiguo conservado activo por contener scripts: " + go.name);
            return;
        }
        go.SetActive(false);
    }

    private static bool EnsureDetails(Transform root)
    {
        bool changed = false;
        Transform kitchen = root.Find("02_COCINA");
        Transform bathroom = root.Find("04_BANO");
        if (kitchen != null && kitchen.Find("Gabinete_Cocina") == null)
        {
            Place(kitchen, House + "(Prb)Storage1.prefab", "Gabinete_Cocina", 0f, 8.0f, -7.0f, 1.35f, 270f);
            changed = true;
        }
        Transform soap = bathroom != null ? bathroom.Find("Accesorio_Jabon_Bano") : null;
        if (soap != null && BoundsOf(soap.gameObject).center.y < .6f)
        {
            Bounds bounds = BoundsOf(soap.gameObject);
            soap.position += Vector3.up * (.95f - bounds.center.y);
            changed = true;
        }
        return changed;
    }
}
#endif
