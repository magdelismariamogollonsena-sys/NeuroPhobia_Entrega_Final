#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class NeuroPhobiaNivel2ParqueGrandeFinal
{
    const string ScenePath = "Assets/Scenes/Nivel2.unity";
    const string BackupPath = "Assets/Scenes/Nivel2_BACKUP_Antes_ParqueGrande.unity";
    const string AssetRoot = "Assets/NeuroPhobia_Level2_ParkFinal";
    const string RootName = "NP_ENV_NIVEL2_PARQUE_GRANDE_FINAL";
    const string OldPrefix = "NP_OLD_N2_HIDDEN__";

    static readonly Dictionary<Material, Material> converted =
        new Dictionary<Material, Material>();

    [MenuItem("Tools/NeuroPhobia/Nivel 2/CREAR PARQUE GRANDE FINAL", priority = 190)]
    public static void Build()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("NeuroPhobia", "Detén Play primero.", "Aceptar");
            return;
        }

        if (!File.Exists(ScenePath))
        {
            EditorUtility.DisplayDialog("NeuroPhobia", "No encontré Assets/Scenes/Nivel2.unity", "Aceptar");
            return;
        }

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(BackupPath) == null)
        {
            AssetDatabase.CopyAsset(ScenePath, BackupPath);
            AssetDatabase.SaveAssets();
        }

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        GameObject previous = Find(scene, RootName);
        if (previous != null)
            UnityEngine.Object.DestroyImmediate(previous);

        HideSafeOldVisualRoots(scene);

        GameObject player = Find(scene, "NP_DesktopPlayer_Normalizado");
        if (player == null) player = Find(scene, "DesktopTestRig");

        Transform spider = FindSpider(scene);

        Vector3 center = Vector3.zero;
        int centerCount = 0;

        if (player != null)
        {
            center += player.transform.position;
            centerCount++;
        }

        if (spider != null)
        {
            center += spider.position;
            centerCount++;
        }

        if (centerCount > 0)
            center /= centerCount;

        center.y = 0f;

        GameObject root = new GameObject(RootName);
        SceneManager.MoveGameObjectToScene(root, scene);
        root.transform.position = center;

        Transform ground = Group("01_TERRENO_Y_CAMINOS", root.transform);
        Transform trees = Group("02_ARBOLES", root.transform);
        Transform bushes = Group("03_ARBUSTOS_FLORES", root.transform);
        Transform rocks = Group("04_ROCAS", root.transform);
        Transform furniture = Group("05_BANCAS_Y_MOBILIARIO", root.transform);
        Transform fences = Group("06_CERCAS", root.transform);
        Transform lighting = Group("07_ILUMINACION", root.transform);

        Material grassMat = MakeMat("M_N2_Cesped", new Color(0.20f, 0.42f, 0.18f, 1f), 0.02f);
        Material pathMat = MakeMat("M_N2_Camino", new Color(0.48f, 0.39f, 0.28f, 1f), 0.03f);
        Material stoneMat = MakeMat("M_N2_BordeCamino", new Color(0.44f, 0.46f, 0.43f, 1f), 0.04f);

        CreateBox("Suelo_Parque_60x50", ground,
            new Vector3(0f, -0.10f, 0f), new Vector3(60f, 0.20f, 50f),
            grassMat, true);

        // Camino principal ancho y recorrido lateral.
        Segment(ground, new Vector3(0f, 0.015f, -22f), new Vector3(0f, 0.015f, 20f), 4.2f, pathMat);
        Segment(ground, new Vector3(-18f, 0.018f, -4f), new Vector3(18f, 0.018f, -4f), 3.4f, pathMat);
        Segment(ground, new Vector3(-18f, 0.018f, -4f), new Vector3(-21f, 0.018f, 12f), 3.0f, pathMat);
        Segment(ground, new Vector3(18f, 0.018f, -4f), new Vector3(21f, 0.018f, 12f), 3.0f, pathMat);

        // Zona central despejada para la terapia y la araña.
        CreateBox("Plaza_Central", ground,
            new Vector3(0f, 0.022f, 4.5f), new Vector3(11f, 0.045f, 9f),
            pathMat, false);

        // Árboles de diferentes especies, principalmente en el perímetro.
        string[] treeModels = {
            "tree_oak.fbx", "tree_detailed.fbx", "tree_default.fbx",
            "tree_tall.fbx", "tree_pineTallA.fbx", "tree_pineRoundC.fbx",
            "tree_fat.fbx"
        };

        Vector3[] treePos = {
            new Vector3(-25,0,-19), new Vector3(-19,0,-20), new Vector3(-12,0,-21),
            new Vector3(12,0,-21), new Vector3(19,0,-20), new Vector3(25,0,-18),
            new Vector3(-27,0,-10), new Vector3(-27,0,0), new Vector3(-27,0,10),
            new Vector3(-26,0,19), new Vector3(27,0,-10), new Vector3(27,0,0),
            new Vector3(27,0,10), new Vector3(26,0,19),
            new Vector3(-20,0,21), new Vector3(-13,0,22), new Vector3(-6,0,21),
            new Vector3(7,0,22), new Vector3(14,0,21), new Vector3(21,0,20)
        };

        for (int i = 0; i < treePos.Length; i++)
        {
            float size = 5.0f + (i % 4) * 0.65f;
            Place("Nature/" + treeModels[i % treeModels.Length],
                "Arbol_" + (i + 1), trees, treePos[i], (i * 37f) % 360f, size, true);
        }

        // Árboles pequeños de transición.
        for (int i = 0; i < 8; i++)
        {
            float x = -20f + i * 5.8f;
            float z = (i % 2 == 0) ? 16f : -15f;
            Place("Nature/tree_small.fbx", "Arbol_Pequeno_" + (i + 1),
                trees, new Vector3(x,0,z), i * 43f, 2.8f, true);
        }

        Vector3[] bushPos = {
            new Vector3(-22,0,-13), new Vector3(-16,0,-16), new Vector3(-9,0,-17),
            new Vector3(9,0,-17), new Vector3(16,0,-16), new Vector3(22,0,-13),
            new Vector3(-23,0,15), new Vector3(-15,0,17), new Vector3(15,0,17),
            new Vector3(23,0,15), new Vector3(-8,0,18), new Vector3(8,0,18)
        };

        string[] bushModels = { "plant_bush.fbx", "plant_bushDetailed.fbx", "plant_bushLarge.fbx" };
        for (int i = 0; i < bushPos.Length; i++)
            Place("Nature/" + bushModels[i % bushModels.Length], "Arbusto_" + (i+1),
                bushes, bushPos[i], i * 29f, 2.0f + (i%3)*0.35f, false);

        // Flores en grupos pequeños cerca del camino.
        string[] flowerModels = { "flower_purpleA.fbx", "flower_redA.fbx", "flower_yellowA.fbx" };
        for (int i = 0; i < 18; i++)
        {
            float x = -17f + (i % 9) * 4.2f;
            float z = (i < 9) ? 14.3f : -12.7f;
            Place("Nature/" + flowerModels[i % 3], "Flores_" + (i+1),
                bushes, new Vector3(x,0,z), i * 21f, 0.8f, false);
        }

        Vector3[] rockPos = {
            new Vector3(-24,0,-5), new Vector3(-21,0,7), new Vector3(-17,0,19),
            new Vector3(24,0,-5), new Vector3(21,0,7), new Vector3(18,0,19)
        };
        string[] rockModels = { "rock_largeA.fbx", "rock_largeB.fbx", "rock_largeC.fbx" };
        for (int i = 0; i < rockPos.Length; i++)
            Place("Nature/" + rockModels[i % 3], "Roca_" + (i+1),
                rocks, rockPos[i], i*53f, 2.2f, true);

        // Bancas enfrentadas hacia la zona central.
        Place("Furniture/bench.fbx", "Banca_1", furniture, new Vector3(-7.5f,0,10.5f), 145f, 3.2f, true);
        Place("Furniture/bench.fbx", "Banca_2", furniture, new Vector3(7.5f,0,10.5f), -145f, 3.2f, true);
        Place("Furniture/bench.fbx", "Banca_3", furniture, new Vector3(-9f,0,-9.5f), 35f, 3.2f, true);
        Place("Furniture/bench.fbx", "Banca_4", furniture, new Vector3(9f,0,-9.5f), -35f, 3.2f, true);

        Place("Furniture/trashcan.fbx", "Caneca_1", furniture, new Vector3(-10.5f,0,9.7f), 0f, 1.1f, true);
        Place("Furniture/trashcan.fbx", "Caneca_2", furniture, new Vector3(10.5f,0,-9.0f), 0f, 1.1f, true);

        Vector3[] lamps = {
            new Vector3(-5.0f,0,-15), new Vector3(5.0f,0,-15),
            new Vector3(-5.0f,0,-6), new Vector3(5.0f,0,-6),
            new Vector3(-5.0f,0,5), new Vector3(5.0f,0,5),
            new Vector3(-5.0f,0,15), new Vector3(5.0f,0,15)
        };
        for (int i = 0; i < lamps.Length; i++)
        {
            Place("Furniture/lampSquareFloor.fbx", "Farol_" + (i+1),
                furniture, lamps[i], 0f, 2.4f, false);
            AddPoint(lighting, "Luz_Farol_" + (i+1),
                lamps[i] + new Vector3(0,2.0f,0), new Color(1f,0.82f,0.62f), 0.65f, 5.5f);
        }

        // Cerca parcial, dejando entrada frontal.
        for (int i = -5; i <= 5; i++)
        {
            if (i >= -1 && i <= 1) continue;
            Place("Nature/fence_simple.fbx", "Cerca_Frente_" + i,
                fences, new Vector3(i*4.7f,0,-24.4f), 0f, 4.8f, true);
        }
        for (int i = -4; i <= 4; i++)
        {
            Place("Nature/fence_simple.fbx", "Cerca_Fondo_" + i,
                fences, new Vector3(i*5.8f,0,24.3f), 0f, 5.8f, true);
        }

        SetupSkybox();
        SetupSun(scene);

        EditorUtility.SetDirty(root);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeGameObject = root;
        if (SceneView.lastActiveSceneView != null)
            SceneView.lastActiveSceneView.FrameSelected();

        EditorUtility.DisplayDialog(
            "NeuroPhobia · Nivel 2",
            "Parque grande creado.\n\n" +
            "Tamaño: 60 x 50 metros.\n" +
            "Incluye árboles, arbustos, flores, rocas, caminos, plaza central, bancas, faroles, cercas y HDR exterior.\n\n" +
            "No se movieron la araña, el jugador, el temporizador ni el límite de distancia.\n\n" +
            "Respaldo: " + BackupPath,
            "Aceptar");
    }

    [MenuItem("Tools/NeuroPhobia/Nivel 2/Restaurar entorno visual anterior", priority = 191)]
    public static void RestoreOld()
    {
        Scene scene = SceneManager.GetActiveScene();
        int count = 0;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name.StartsWith(OldPrefix, StringComparison.Ordinal))
            {
                root.name = root.name.Substring(OldPrefix.Length);
                root.SetActive(true);
                count++;
            }
        }
        EditorSceneManager.MarkSceneDirty(scene);
        EditorUtility.DisplayDialog("NeuroPhobia", "Entornos restaurados: " + count, "Aceptar");
    }

    static void HideSafeOldVisualRoots(Scene scene)
    {
        foreach (GameObject go in scene.GetRootGameObjects())
        {
            if (go.name == RootName) continue;

            string n = go.name.ToLowerInvariant();
            bool visualCandidate =
                (n.Contains("nivel2") || n.Contains("level2")) &&
                (n.Contains("env") || n.Contains("parque") || n.Contains("park") ||
                 n.Contains("bosque") || n.Contains("forest") || n.Contains("garden"));

            if (!visualCandidate || HasProtectedDescendant(go.transform))
                continue;

            if (!go.name.StartsWith(OldPrefix, StringComparison.Ordinal))
                go.name = OldPrefix + go.name;

            go.SetActive(false);
            EditorUtility.SetDirty(go);
        }
    }

    static bool HasProtectedDescendant(Transform t)
    {
        string[] protectedWords = {
            "spider","arana","araña","player","desktop","xr","camera","hud","ui",
            "controller","manager","selector","gaze","mirada","timer","limite","limit",
            "eventsystem"
        };

        string n = t.name.ToLowerInvariant();
        foreach (string word in protectedWords)
            if (n.Contains(word)) return true;

        foreach (Transform c in t)
            if (HasProtectedDescendant(c)) return true;

        return false;
    }

    static void Segment(Transform parent, Vector3 a, Vector3 b, float width, Material mat)
    {
        Vector3 mid = (a+b)*0.5f;
        Vector3 d = b-a;
        float len = new Vector2(d.x,d.z).magnitude;
        GameObject go = CreateBox("Camino", parent, mid, new Vector3(width,0.04f,len), mat, false);
        go.transform.rotation = Quaternion.Euler(0f, Mathf.Atan2(d.x,d.z)*Mathf.Rad2Deg, 0f);
    }

    static GameObject Place(string rel, string name, Transform parent, Vector3 localPos,
        float yaw, float targetHorizontalSize, bool collider)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetRoot + "/Models/" + rel);
        if (prefab == null)
        {
            Debug.LogWarning("[N2 Parque] Falta: " + rel);
            return null;
        }

        GameObject go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        if (go == null) return null;

        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localRotation = Quaternion.Euler(0,yaw,0);
        go.transform.localScale = Vector3.one;

        ConvertMaterials(go);

        Bounds b;
        if (BoundsOf(go, out b))
        {
            float horizontal = Mathf.Max(b.size.x, b.size.z);
            if (horizontal > 0.001f)
                go.transform.localScale *= targetHorizontalSize / horizontal;
        }

        Snap(go, parent.position.y);

        if (collider) AddBoundsCollider(go);
        return go;
    }

    static void Snap(GameObject go, float floorY)
    {
        Bounds b;
        if (!BoundsOf(go, out b)) return;
        go.transform.position += Vector3.up * (floorY - b.min.y);
    }

    static void AddBoundsCollider(GameObject go)
    {
        if (go.GetComponentsInChildren<Collider>(true).Length > 0) return;
        Bounds b;
        if (!BoundsOf(go, out b)) return;

        BoxCollider box = go.AddComponent<BoxCollider>();
        box.center = go.transform.InverseTransformPoint(b.center);
        Vector3 s = go.transform.lossyScale;
        box.size = new Vector3(
            b.size.x / Mathf.Max(0.0001f, Mathf.Abs(s.x)),
            b.size.y / Mathf.Max(0.0001f, Mathf.Abs(s.y)),
            b.size.z / Mathf.Max(0.0001f, Mathf.Abs(s.z)));
    }

    static bool BoundsOf(GameObject go, out Bounds b)
    {
        Renderer[] rs = go.GetComponentsInChildren<Renderer>(true);
        if (rs.Length == 0)
        {
            b = new Bounds(go.transform.position, Vector3.zero);
            return false;
        }
        b = rs[0].bounds;
        for (int i=1;i<rs.Length;i++) b.Encapsulate(rs[i].bounds);
        return true;
    }

    static void ConvertMaterials(GameObject root)
    {
        Shader urp = Shader.Find("Universal Render Pipeline/Lit");
        if (urp == null) return;

        foreach (Renderer r in root.GetComponentsInChildren<Renderer>(true))
        {
            Material[] src = r.sharedMaterials;
            Material[] dst = new Material[src.Length];

            for (int i=0;i<src.Length;i++)
            {
                if (src[i] == null)
                {
                    dst[i] = MakeMat("M_N2_Default", new Color(.45f,.55f,.35f,1), .03f);
                    continue;
                }

                if (src[i].shader != null && src[i].shader.name.Contains("Universal Render Pipeline"))
                {
                    dst[i] = src[i];
                    continue;
                }

                Material found;
                if (converted.TryGetValue(src[i], out found))
                {
                    dst[i] = found;
                    continue;
                }

                string folder = AssetRoot + "/Generated/Materials";
                EnsureFolder(folder);
                string path = AssetDatabase.GenerateUniqueAssetPath(folder + "/Imported_" + Safe(src[i].name) + ".mat");
                Material m = new Material(urp);
                Color c = src[i].HasProperty("_Color") ? src[i].color : Color.white;
                if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
                AssetDatabase.CreateAsset(m, path);
                converted[src[i]] = m;
                dst[i] = m;
            }
            r.sharedMaterials = dst;
        }
    }

    static Material MakeMat(string name, Color color, float smooth)
    {
        string folder = AssetRoot + "/Generated/Materials";
        EnsureFolder(folder);
        string path = folder + "/" + name + ".mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");

        if (m == null)
        {
            m = new Material(shader);
            AssetDatabase.CreateAsset(m, path);
        }
        else m.shader = shader;

        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
        else m.color = color;
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smooth);
        EditorUtility.SetDirty(m);
        return m;
    }

    static GameObject CreateBox(string name, Transform parent, Vector3 pos,
        Vector3 scale, Material mat, bool collider)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        Renderer r = go.GetComponent<Renderer>();
        if (r != null) r.sharedMaterial = mat;
        Collider c = go.GetComponent<Collider>();
        if (c != null) c.enabled = collider;
        return go;
    }

    static Transform Group(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    static void AddPoint(Transform parent, string name, Vector3 localPos, Color color, float intensity, float range)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        Light l = go.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = color;
        l.intensity = intensity;
        l.range = range;
        l.shadows = LightShadows.Soft;
    }

    static void SetupSkybox()
    {
        Texture2D hdr = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetRoot + "/HDR/suburban_soccer_park_2k.hdr");
        Shader shader = Shader.Find("Skybox/Panoramic");
        if (hdr == null || shader == null) return;

        string folder = AssetRoot + "/Generated/Materials";
        EnsureFolder(folder);
        string path = folder + "/M_N2_Skybox_Park.mat";
        Material sky = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (sky == null)
        {
            sky = new Material(shader);
            AssetDatabase.CreateAsset(sky, path);
        }
        else sky.shader = shader;

        sky.SetTexture("_MainTex", hdr);
        if (sky.HasProperty("_Exposure")) sky.SetFloat("_Exposure", 0.85f);
        if (sky.HasProperty("_Rotation")) sky.SetFloat("_Rotation", 35f);

        RenderSettings.skybox = sky;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
        RenderSettings.ambientIntensity = 0.85f;
        DynamicGI.UpdateEnvironment();
    }

    static void SetupSun(Scene scene)
    {
        Light sun = null;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Light l in root.GetComponentsInChildren<Light>(true))
                if (l.type == LightType.Directional) { sun = l; break; }
            if (sun != null) break;
        }
        if (sun == null)
        {
            GameObject go = new GameObject("Directional Light");
            SceneManager.MoveGameObjectToScene(go, scene);
            sun = go.AddComponent<Light>();
            sun.type = LightType.Directional;
        }
        sun.enabled = true;
        sun.gameObject.SetActive(true);
        sun.color = new Color(1f,.96f,.87f);
        sun.intensity = 0.95f;
        sun.transform.rotation = Quaternion.Euler(48f,-32f,0f);
    }

    static Transform FindSpider(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform t = FindSpiderRec(root.transform);
            if (t != null) return t;
        }
        return null;
    }

    static Transform FindSpiderRec(Transform t)
    {
        string n = t.name.ToLowerInvariant();
        if ((n.Contains("spider") || n.Contains("arana") || n.Contains("araña")) &&
            t.GetComponentInChildren<Renderer>(true) != null)
            return t;
        foreach (Transform c in t)
        {
            Transform f = FindSpiderRec(c);
            if (f != null) return f;
        }
        return null;
    }

    static GameObject Find(Scene scene, string exact)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform f = FindRec(root.transform, exact);
            if (f != null) return f.gameObject;
        }
        return null;
    }

    static Transform FindRec(Transform t, string exact)
    {
        if (string.Equals(t.name, exact, StringComparison.OrdinalIgnoreCase)) return t;
        foreach (Transform c in t)
        {
            Transform f = FindRec(c, exact);
            if (f != null) return f;
        }
        return null;
    }

    static void EnsureFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder)) return;
        string[] parts = folder.Split('/');
        string current = parts[0];
        for (int i=1;i<parts.Length;i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }

    static string Safe(string s)
    {
        foreach (char c in Path.GetInvalidFileNameChars()) s = s.Replace(c,'_');
        return s.Replace('/','_').Replace('\\','_');
    }
}
#endif
