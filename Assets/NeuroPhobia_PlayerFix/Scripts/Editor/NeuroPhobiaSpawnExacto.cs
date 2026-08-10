#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class NeuroPhobiaSpawnExacto
{
    const string Marker = "NP_PUNTO_INICIO_EXACTO";

    [MenuItem("Tools/NeuroPhobia/Punto de inicio/1. Crear marcador exacto")]
    public static void Crear()
    {
        if (EditorApplication.isPlaying) return;
        Scene s = SceneManager.GetActiveScene();
        GameObject m = Buscar(s, Marker);
        if (!m)
        {
            m = new GameObject(Marker);
            SceneManager.MoveGameObjectToScene(m, s);
            if (SceneView.lastActiveSceneView != null)
                m.transform.position = SceneView.lastActiveSceneView.pivot;
        }
        m.SetActive(true);
        Selection.activeGameObject = m;
        if (SceneView.lastActiveSceneView != null)
            SceneView.lastActiveSceneView.FrameSelected();
        EditorSceneManager.MarkSceneDirty(s);
        EditorUtility.DisplayDialog("NeuroPhobia",
            "Mueve NP_PUNTO_INICIO_EXACTO con W hasta dejarlo EXACTAMENTE sobre el piso. Gíralo con E y luego ejecuta el paso 2.",
            "Aceptar");
    }

    [MenuItem("Tools/NeuroPhobia/Punto de inicio/2. Aplicar marcador exacto a PC y RV")]
    public static void Aplicar()
    {
        if (EditorApplication.isPlaying) return;
        Scene s = SceneManager.GetActiveScene();
        GameObject m = Buscar(s, Marker);
        if (!m)
        {
            EditorUtility.DisplayDialog("NeuroPhobia","Primero crea el marcador exacto.","Aceptar");
            return;
        }

        GameObject pc = Buscar(s, "NP_DesktopPlayer_Normalizado");
        if (!pc) pc = Buscar(s, "NP_DesktopPlayer_Niveles45");
        GameObject xr = Buscar(s, "XR Origin (XR Rig)");
        if (!xr) xr = Buscar(s, "XR Origin");

        Vector3 pos = m.transform.position;
        float yaw = m.transform.eulerAngles.y;

        if (pc)
        {
            Undo.RegisterFullObjectHierarchyUndo(pc, "Spawn exacto PC");
            CharacterController cc = pc.GetComponent<CharacterController>();
            if (cc) cc.enabled = false;
            pc.transform.position = pos;
            pc.transform.rotation = Quaternion.Euler(0, yaw, 0);
            if (cc) cc.enabled = true;
            EditorUtility.SetDirty(pc);
        }

        if (xr)
        {
            Undo.RegisterFullObjectHierarchyUndo(xr, "Spawn exacto RV");
            xr.transform.position = pos;
            xr.transform.rotation = Quaternion.Euler(0, yaw, 0);
            EditorUtility.SetDirty(xr);
        }

        EditorSceneManager.MarkSceneDirty(s);
        EditorSceneManager.SaveScene(s);
        EditorUtility.DisplayDialog("NeuroPhobia",
            "Inicio aplicado exactamente al marcador.\nX=" + pos.x.ToString("0.00") +
            " Y=" + pos.y.ToString("0.00") + " Z=" + pos.z.ToString("0.00"),
            "Aceptar");
    }

    static GameObject Buscar(Scene s, string n)
    {
        foreach (GameObject r in s.GetRootGameObjects())
        {
            Transform t = BuscarT(r.transform, n);
            if (t) return t.gameObject;
        }
        return null;
    }

    static Transform BuscarT(Transform t, string n)
    {
        if (string.Equals(t.name, n, StringComparison.OrdinalIgnoreCase)) return t;
        foreach (Transform c in t)
        {
            Transform r = BuscarT(c, n);
            if (r) return r;
        }
        return null;
    }
}
#endif
