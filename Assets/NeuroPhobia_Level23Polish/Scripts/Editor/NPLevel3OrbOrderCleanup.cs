#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class NPLevel3OrbOrderCleanup
{
    const string ScenePath = "Assets/Scenes/Nivel3.unity";

    [MenuItem("Tools/NeuroPhobia/Corregir ORDEN azul luego naranja Nivel 3", priority = 159)]
    public static void Fix()
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

        // Oculta los orbes del sistema viejo y desactiva ese controlador.
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (MonoBehaviour b in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (b == null) continue;

                if (b.GetType().Name == "Level3OrbSequenceFlow")
                {
                    SerializedObject so = new SerializedObject(b);
                    SerializedProperty orbs = so.FindProperty("orbs");

                    if (orbs != null && orbs.isArray)
                    {
                        for (int i = 0; i < orbs.arraySize; i++)
                        {
                            Object obj = orbs.GetArrayElementAtIndex(i).objectReferenceValue;
                            GameObject go = obj as GameObject;

                            if (go == null && obj is Component c)
                                go = c.gameObject;

                            if (go != null && !IsUnder(go.transform, "NP_NIVEL3_ORBES_REALES"))
                            {
                                go.SetActive(false);
                                EditorUtility.SetDirty(go);
                            }
                        }
                    }

                    b.enabled = false;
                    EditorUtility.SetDirty(b);
                }

                if (b.GetType().Name == "NeuroPhobiaLevel3OrbCompatibility")
                {
                    b.enabled = false;
                    EditorUtility.SetDirty(b);
                }
            }
        }

        GameObject newRoot = Find(scene, "NP_NIVEL3_ORBES_REALES");
        GameObject blue = newRoot ? FindUnder(newRoot.transform, "NP_Orbe_1_Azul") : null;
        GameObject orange = newRoot ? FindUnder(newRoot.transform, "NP_Orbe_2_Naranja") : null;

        if (!newRoot || !blue || !orange)
        {
            EditorUtility.DisplayDialog(
                "NeuroPhobia",
                "No encontré los orbes reales.\nPrimero ejecuta: Recrear orbes REALES Nivel 3",
                "Aceptar");
            return;
        }

        // Orden correcto guardado en la escena.
        blue.SetActive(true);
        orange.SetActive(false);

        NPLevel3RealOrbs flow = newRoot.GetComponent<NPLevel3RealOrbs>();
        if (flow != null)
        {
            flow.orb1 = blue;
            flow.orb2 = orange;
            flow.collectDistance = 1.6f;
            flow.enabled = true;
            EditorUtility.SetDirty(flow);
        }

        EditorUtility.SetDirty(blue);
        EditorUtility.SetDirty(orange);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Selection.activeGameObject = blue;
        if (SceneView.lastActiveSceneView != null)
            SceneView.lastActiveSceneView.FrameSelected();

        EditorUtility.DisplayDialog(
            "NeuroPhobia",
            "Listo.\n\nAl iniciar SOLO aparece el AZUL.\nAl recogerlo aparece el NARANJA.\nLos orbes viejos quedaron desactivados.",
            "Aceptar");
    }

    static bool IsUnder(Transform t, string ancestor)
    {
        while (t != null)
        {
            if (t.name == ancestor) return true;
            t = t.parent;
        }
        return false;
    }

    static GameObject Find(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform t = FindT(root.transform, name);
            if (t != null) return t.gameObject;
        }
        return null;
    }

    static GameObject FindUnder(Transform root, string name)
    {
        Transform t = FindT(root, name);
        return t != null ? t.gameObject : null;
    }

    static Transform FindT(Transform t, string name)
    {
        if (t.name == name) return t;
        foreach (Transform c in t)
        {
            Transform r = FindT(c, name);
            if (r != null) return r;
        }
        return null;
    }
}
#endif
