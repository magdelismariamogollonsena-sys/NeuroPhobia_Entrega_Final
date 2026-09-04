#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class NeuroPhobiaNivel3SotanoEnormeFinal
{
    const string ScenePath = "Assets/Scenes/Nivel3.unity";
    const string BackupPath = "Assets/Scenes/Nivel3_BACKUP_Antes_SotanoEnorme.unity";
    const string AssetRoot = "Assets/NeuroPhobia_Level3_BasementFinal";
    const string RootName = "NP_ENV_NIVEL3_SOTANO_ENORME_FINAL";
    const string OldPrefix = "NP_OLD_N3_HIDDEN__";

    static readonly Dictionary<Material, Material> converted =
        new Dictionary<Material, Material>();

    [MenuItem("Tools/NeuroPhobia/Nivel 3/CREAR SOTANO ENORME FINAL", priority = 195)]
    public static void Build()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("NeuroPhobia", "Detén Play primero.", "Aceptar");
            return;
        }

        if (!File.Exists(ScenePath))
        {
            EditorUtility.DisplayDialog("NeuroPhobia", "No encontré Assets/Scenes/Nivel3.unity", "Aceptar");
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

        Vector3 center = ComputeGameplayCenter(scene);
        center.y = 0f;

        HideSafeOldBasement(scene);

        GameObject root = new GameObject(RootName);
        SceneManager.MoveGameObjectToScene(root, scene);
        root.transform.position = center;

        Transform architecture = Group("01_ARQUITECTURA", root.transform);
        Transform storage = Group("02_ALMACENAMIENTO", root.transform);
        Transform pipes = Group("03_TUBERIAS_Y_DUCTOS", root.transform);
        Transform rooms = Group("04_ZONAS_LATERALES", root.transform);
        Transform props = Group("05_ACCESORIOS", root.transform);
        Transform lighting = Group("06_ILUMINACION", root.transform);

        Material floorMat = MakeTexturedMat(
            "M_N3_Concreto_Piso",
            AssetRoot + "/Textures/concrete_floor_diff_1k.jpg",
            new Color(.68f,.65f,.60f,1), new Vector2(10,8), .03f);

        Material wallMat = MakeTexturedMat(
            "M_N3_Concreto_Pared",
            AssetRoot + "/Textures/concrete_wall_007_diff_1k.jpg",
            new Color(.62f,.64f,.63f,1), new Vector2(8,4), .02f);

        Material darkMat = MakeMat("M_N3_Metal_Oscuro", new Color(.11f,.13f,.14f,1), .20f);
        Material warningMat = MakeMat("M_N3_Advertencia", new Color(.65f,.27f,.08f,1), .08f);

        // 64 x 48 m: mucho más grande que el Nivel 3 anterior.
        CreateBox("PISO_CONCRETO_64x48", architecture,
            new Vector3(0,-.10f,0), new Vector3(64,.20f,48), floorMat, true);

        CreateBox("TECHO_SOTANO", architecture,
            new Vector3(0,4.65f,0), new Vector3(64,.18f,48), darkMat, true);

        CreateBox("Pared_Norte", architecture,
            new Vector3(0,2.25f,23.9f), new Vector3(64,4.5f,.25f), wallMat, true);
        CreateBox("Pared_Sur", architecture,
            new Vector3(0,2.25f,-23.9f), new Vector3(64,4.5f,.25f), wallMat, true);
        CreateBox("Pared_Oeste", architecture,
            new Vector3(-31.9f,2.25f,0), new Vector3(.25f,4.5f,48), wallMat, true);
        CreateBox("Pared_Este", architecture,
            new Vector3(31.9f,2.25f,0), new Vector3(.25f,4.5f,48), wallMat, true);

        // Corredores internos: paredes parciales con pasos amplios.
        CreateBox("Muro_Interno_Oeste_A", rooms,
            new Vector3(-10.5f,2.1f,-15f), new Vector3(.30f,4.2f,14f), wallMat, true);
        CreateBox("Muro_Interno_Oeste_B", rooms,
            new Vector3(-10.5f,2.1f,15f), new Vector3(.30f,4.2f,14f), wallMat, true);
        CreateBox("Muro_Interno_Este_A", rooms,
            new Vector3(10.5f,2.1f,-15f), new Vector3(.30f,4.2f,14f), wallMat, true);
        CreateBox("Muro_Interno_Este_B", rooms,
            new Vector3(10.5f,2.1f,15f), new Vector3(.30f,4.2f,14f), wallMat, true);

        // Vigas y columnas.
        for (int x=-24; x<=24; x+=12)
        {
            Place("Building/column-wide.fbx", "Columna_" + x,
                architecture, new Vector3(x,0,-8f), 0f, 1.1f, true);
            Place("Building/column-wide.fbx", "Columna_Fondo_" + x,
                architecture, new Vector3(x,0,9f), 0f, 1.1f, true);
        }

        for (int z=-18; z<=18; z+=12)
        {
            CreateBox("Viga_" + z, architecture,
                new Vector3(0,4.25f,z), new Vector3(63,.28f,.35f), darkMat, false);
        }

        // Almacén grande a la izquierda.
        for (int i=0;i<10;i++)
        {
            float x = -25f + (i%2)*3.3f;
            float z = -18f + (i/2)*7.0f;
            Place("Props/cardboardBoxClosed.fbx", "Caja_Cerrada_" + (i+1),
                storage, new Vector3(x,0,z), i*21f, 1.4f + (i%3)*.25f, true);
        }

        for (int i=0;i<5;i++)
        {
            float z = -17f + i*8.2f;
            Place("Props/bookcaseClosedWide.fbx", "Estante_" + (i+1),
                storage, new Vector3(-29.2f,0,z), 90f, 3.2f, true);
        }

        Place("Props/desk.fbx", "Mesa_Trabajo", storage,
            new Vector3(-20.5f,0,17.0f), 180f, 2.8f, true);
        Place("Props/chairDesk.fbx", "Silla_Trabajo", storage,
            new Vector3(-20.5f,0,15.4f), 0f, 1.4f, true);

        // Tuberías largas en zona derecha.
        for (int i=0;i<7;i++)
        {
            float z = -19f + i*6.2f;
            GameObject p = Place("Building/detail-pipe.fbx", "Tuberia_Muro_" + (i+1),
                pipes, new Vector3(29.7f,1.65f,z), 90f, 3.8f, false);
            if (p != null) p.transform.localScale *= 1.35f;
        }

        for (int i=0;i<5;i++)
        {
            float x = 15f + i*3.0f;
            Place("Building/gutter-vertical-wall.fbx", "Ducto_Vertical_" + (i+1),
                pipes, new Vector3(x,0,19.5f), 180f, 3.5f, false);
        }

        // Dos zonas laterales con entradas y barricadas.
        Place("Building/wall-doorway-wide-square.fbx", "Marco_Zona_Oeste",
            rooms, new Vector3(-10.5f,0,0), 90f, 4.4f, true);
        Place("Building/wall-doorway-wide-square.fbx", "Marco_Zona_Este",
            rooms, new Vector3(10.5f,0,0), -90f, 4.4f, true);

        Place("Building/barricade-doorway-a.fbx", "Barricada_Decorativa",
            rooms, new Vector3(22f,0,20.5f), 180f, 3.2f, true);

        // Escalera decorativa en esquina, fuera de la ruta central.
        Place("Building/stairs-closed.fbx", "Escalera_Sotano",
            rooms, new Vector3(25.2f,0,-18.3f), 0f, 7.0f, true);

        // Placas metálicas y señalización.
        for (int i=0;i<8;i++)
        {
            float x = -24f + i*7.0f;
            Place("Building/plating-detailed-wide.fbx", "Placa_Metal_" + (i+1),
                props, new Vector3(x,.03f,-22.7f), 0f, 3.0f, false);
        }

        for (int i=0;i<6;i++)
        {
            CreateBox("Marca_Advertencia_" + (i+1), props,
                new Vector3(-3.5f + i*1.4f,.025f,-8f),
                new Vector3(.65f,.045f,5.0f), warningMat, false);
        }

        Place("Props/trashcan.fbx", "Caneca_Industrial_1", props,
            new Vector3(27f,0,12f), 0f, 1.2f, true);
        Place("Props/trashcan.fbx", "Caneca_Industrial_2", props,
            new Vector3(-26f,0,-12f), 0f, 1.2f, true);
        Place("Props/coatRackStanding.fbx", "Perchero_Viejo", props,
            new Vector3(-28f,0,19f), 0f, 1.8f, true);

        // Iluminación: tenue con luces blancas frías y rojas de emergencia.
        SetupDirectional(scene);

        Vector3[] whiteLights = {
            new Vector3(0,3.8f,-17), new Vector3(0,3.8f,-6),
            new Vector3(0,3.8f,6), new Vector3(0,3.8f,17),
            new Vector3(-21,3.6f,0), new Vector3(21,3.6f,0)
        };
        for (int i=0;i<whiteLights.Length;i++)
            AddPoint(lighting, "Luz_Fria_" + (i+1), whiteLights[i],
                new Color(.65f,.78f,1f), 1.15f, 8.5f);

        Vector3[] redLights = {
            new Vector3(-29,3.2f,-20), new Vector3(29,3.2f,-20),
            new Vector3(-29,3.2f,20), new Vector3(29,3.2f,20)
        };
        for (int i=0;i<redLights.Length;i++)
            AddPoint(lighting, "Emergencia_Roja_" + (i+1), redLights[i],
                new Color(1f,.14f,.08f), 1.3f, 7.5f);

        // Hacer el recorrido real más largo: mover SOLO los orbes.
        GameObject orb1 = Find(scene, "NP_Orbe_1_Azul");
        GameObject orb2 = Find(scene, "NP_Orbe_2_Naranja");

        if (orb1 != null)
        {
            Undo.RecordObject(orb1.transform, "Mover Orbe Azul Nivel 3");
            orb1.transform.position = center + new Vector3(-21f,.60f,15f);
            orb1.SetActive(true);
            EditorUtility.SetDirty(orb1);
        }

        if (orb2 != null)
        {
            Undo.RecordObject(orb2.transform, "Mover Orbe Naranja Nivel 3");
            orb2.transform.position = center + new Vector3(22f,.60f,-15f);
            orb2.SetActive(false);
            EditorUtility.SetDirty(orb2);
        }

        EditorUtility.SetDirty(root);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeGameObject = root;
        if (SceneView.lastActiveSceneView != null)
            SceneView.lastActiveSceneView.FrameSelected();

        EditorUtility.DisplayDialog(
            "NeuroPhobia · Nivel 3",
            "Sótano enorme creado.\n\n" +
            "Tamaño: 64 x 48 metros.\n" +
            "Incluye almacén, estantes, cajas, corredores, columnas, tuberías, ductos, barricadas, escalera, luces frías y luces rojas.\n\n" +
            "Los orbes fueron separados para que el nivel dure mucho más:\n" +
            "AZUL: zona lejana izquierda/fondo\n" +
            "NARANJA: zona opuesta\n\n" +
            "No se cambió el script de los orbes ni el selector PC/RV.\n\n" +
            "Respaldo: " + BackupPath,
            "Aceptar");
    }

    [MenuItem("Tools/NeuroPhobia/Nivel 3/Restaurar sotano visual anterior", priority = 196)]
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

    static void HideSafeOldBasement(Scene scene)
    {
        foreach (GameObject go in scene.GetRootGameObjects())
        {
            if (go.name == RootName) continue;
            string n = go.name.ToLowerInvariant();

            bool exactKnown = string.Equals(go.name, "NP_ENV_NIVEL3_SOTANO",
                StringComparison.OrdinalIgnoreCase);

            bool candidate =
                exactKnown ||
                ((n.Contains("nivel3") || n.Contains("level3")) &&
                 (n.Contains("env") || n.Contains("sotano") || n.Contains("basement")));

            if (!candidate || HasProtectedDescendant(go.transform))
                continue;

            if (!go.name.StartsWith(OldPrefix, StringComparison.Ordinal))
                go.name = OldPrefix + go.name;

            go.SetActive(false);
            EditorUtility.SetDirty(go);
        }
    }

    static bool HasProtectedDescendant(Transform t)
    {
        string[] words = {
            "spider","arana","araña","orb","orbe","player","desktop","xr","camera",
            "hud","ui","controller","manager","selector","route","ruta","patrol",
            "eventsystem","trigger"
        };

        string n = t.name.ToLowerInvariant();
        foreach (string w in words)
            if (n.Contains(w)) return true;

        foreach (Transform c in t)
            if (HasProtectedDescendant(c)) return true;

        return false;
    }

    static Vector3 ComputeGameplayCenter(Scene scene)
    {
        List<Vector3> pts = new List<Vector3>();

        string[] names = {
            "NP_DesktopPlayer_Normalizado",
            "DesktopTestRig",
            "XR Origin (XR Rig)",
            "NP_Orbe_1_Azul",
            "NP_Orbe_2_Naranja"
        };

        foreach (string n in names)
        {
            GameObject go = Find(scene, n);
            if (go != null) pts.Add(go.transform.position);
        }

        Transform spider = FindSpider(scene);
        if (spider != null) pts.Add(spider.position);

        if (pts.Count == 0) return Vector3.zero;

        Vector3 sum = Vector3.zero;
        foreach (Vector3 p in pts) sum += p;
        Vector3 c = sum / pts.Count;
        c.y = 0f;
        return c;
    }

    static GameObject Place(string rel, string name, Transform parent, Vector3 localPos,
        float yaw, float targetHorizontalSize, bool collider)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetRoot + "/Models/" + rel);
        if (prefab == null)
        {
            Debug.LogWarning("[N3 Sotano] Falta: " + rel);
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
            b.size.x / Mathf.Max(.0001f, Mathf.Abs(s.x)),
            b.size.y / Mathf.Max(.0001f, Mathf.Abs(s.y)),
            b.size.z / Mathf.Max(.0001f, Mathf.Abs(s.z)));
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
                    dst[i] = MakeMat("M_N3_Default", new Color(.35f,.37f,.37f,1), .05f);
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
                if (src[i].mainTexture != null && m.HasProperty("_BaseMap"))
                    m.SetTexture("_BaseMap", src[i].mainTexture);
                AssetDatabase.CreateAsset(m, path);
                converted[src[i]] = m;
                dst[i] = m;
            }
            r.sharedMaterials = dst;
        }
    }

    static Material MakeTexturedMat(string name, string texPath, Color tint, Vector2 tiling, float smooth)
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

        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
        if (m.HasProperty("_BaseMap"))
        {
            m.SetTexture("_BaseMap", tex);
            m.SetColor("_BaseColor", tint);
            m.SetTextureScale("_BaseMap", tiling);
        }
        else
        {
            m.mainTexture = tex;
            m.color = tint;
            m.mainTextureScale = tiling;
        }
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smooth);
        EditorUtility.SetDirty(m);
        return m;
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

    static void AddPoint(Transform parent, string name, Vector3 localPos,
        Color color, float intensity, float range)
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

    static void SetupDirectional(Scene scene)
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
        sun.color = new Color(.42f,.48f,.58f);
        sun.intensity = .18f;
        sun.transform.rotation = Quaternion.Euler(55f,-25f,0f);

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(.075f,.085f,.10f);
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
        if (string.Equals(t.name, exact, StringComparison.OrdinalIgnoreCase))
            return t;
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
