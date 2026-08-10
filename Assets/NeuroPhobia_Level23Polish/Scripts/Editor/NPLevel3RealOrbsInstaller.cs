
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class NPLevel3RealOrbsInstaller
{
    const string ScenePath = "Assets/Scenes/Nivel3.unity";

    [MenuItem("Tools/NeuroPhobia/Recrear orbes REALES Nivel 3", priority = 158)]
    public static void Install()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("NeuroPhobia","Detén Play primero.","Aceptar");
            return;
        }

        if (!File.Exists(ScenePath))
        {
            EditorUtility.DisplayDialog("NeuroPhobia","No encontré Nivel3.unity.","Aceptar");
            return;
        }

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        DisableOld(scene, "Level3OrbSequenceFlow");
        DisableOld(scene, "NeuroPhobiaLevel3OrbCompatibility");

        GameObject oldHud = Find(scene, "NP_UI_NIVEL3_FIJA");
        if (oldHud) oldHud.SetActive(false);

        GameObject player = Find(scene, "NP_DesktopPlayer_Normalizado");
        if (!player)
        {
            EditorUtility.DisplayDialog("NeuroPhobia","No encontré NP_DesktopPlayer_Normalizado.","Aceptar");
            return;
        }

        GameObject xr = Find(scene, "XR Origin (XR Rig)");
        if (!xr) xr = Find(scene, "XR Origin");

        GameObject root = Find(scene, "NP_NIVEL3_ORBES_REALES");
        if (!root)
        {
            root = new GameObject("NP_NIVEL3_ORBES_REALES");
            SceneManager.MoveGameObjectToScene(root, scene);
        }

        GameObject o1 = Child(root.transform, "NP_Orbe_1_Azul");
        GameObject o2 = Child(root.transform, "NP_Orbe_2_Naranja");

        Vector3 f = player.transform.forward; f.y = 0; f.Normalize();
        Vector3 r = player.transform.right; r.y = 0; r.Normalize();
        Vector3 p = player.transform.position;

        if (!o1)
        {
            o1 = CreateOrb("NP_Orbe_1_Azul", root.transform, new Color(.08f,.55f,1f,1));
            o1.transform.position = p + f * 3f + r * 1f + Vector3.up * .6f;
        }

        if (!o2)
        {
            o2 = CreateOrb("NP_Orbe_2_Naranja", root.transform, new Color(1f,.38f,.06f,1));
            o2.transform.position = p + f * 6f - r * 1.3f + Vector3.up * .6f;
        }

        o1.SetActive(true);
        o2.SetActive(true);

        NPLevel3RealOrbs flow = root.GetComponent<NPLevel3RealOrbs>();
        if (!flow) flow = root.AddComponent<NPLevel3RealOrbs>();

        flow.desktopPlayer = player;
        flow.xrOrigin = xr;
        flow.orb1 = o1;
        flow.orb2 = o2;
        flow.collectDistance = 1.6f;
        flow.completionDelay = 5f;
        flow.nextScene = "Nivel4";
        flow.enabled = true;

        AddNivel4();

        EditorUtility.SetDirty(root);
        EditorUtility.SetDirty(o1);
        EditorUtility.SetDirty(o2);
        EditorUtility.SetDirty(flow);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Selection.activeGameObject = o1;
        if (SceneView.lastActiveSceneView != null)
            SceneView.lastActiveSceneView.FrameSelected();

        EditorUtility.DisplayDialog(
            "NeuroPhobia",
            "Listo.\n\nAhora los orbes son objetos reales y editables:\n" +
            "NP_NIVEL3_ORBES_REALES\n  NP_Orbe_1_Azul\n  NP_Orbe_2_Naranja",
            "Aceptar");
    }

    static void DisableOld(Scene scene, string typeName)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (MonoBehaviour b in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (b != null && b.GetType().Name == typeName)
                {
                    b.enabled = false;
                    EditorUtility.SetDirty(b);
                }
            }
        }
    }

    static GameObject CreateOrb(string name, Transform parent, Color color)
    {
        GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        orb.name = name;
        orb.transform.SetParent(parent, true);
        orb.transform.localScale = Vector3.one * .55f;

        Renderer ren = orb.GetComponent<Renderer>();
        Shader sh = Shader.Find("Universal Render Pipeline/Lit");
        if (!sh) sh = Shader.Find("Standard");

        if (ren && sh)
        {
            Material m = new Material(sh);
            m.name = name + "_Mat";
            m.color = color;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
            if (m.HasProperty("_EmissionColor"))
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", color * 2f);
            }
            ren.sharedMaterial = m;
        }

        SphereCollider col = orb.GetComponent<SphereCollider>();
        if (col) col.isTrigger = true;

        Light l = orb.AddComponent<Light>();
        l.type = LightType.Point; l.color = color;
        l.intensity = 2f; l.range = 3.5f;
        l.shadows = LightShadows.None;

        return orb;
    }

    static GameObject Child(Transform t, string n)
    {
        Transform f = FindT(t,n);
        return f ? f.gameObject : null;
    }

    static GameObject Find(Scene scene, string n)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform f = FindT(root.transform,n);
            if (f) return f.gameObject;
        }
        return null;
    }

    static Transform FindT(Transform t, string n)
    {
        if (t.name == n) return t;
        foreach (Transform c in t)
        {
            Transform f = FindT(c,n);
            if (f) return f;
        }
        return null;
    }

    static void AddNivel4()
    {
        string path = "Assets/Scenes/Nivel4.unity";
        if (!File.Exists(path)) return;

        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (var s in scenes)
        {
            if (s.path == path)
            {
                s.enabled = true;
                EditorBuildSettings.scenes = scenes.ToArray();
                return;
            }
        }

        scenes.Add(new EditorBuildSettingsScene(path,true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
#endif
