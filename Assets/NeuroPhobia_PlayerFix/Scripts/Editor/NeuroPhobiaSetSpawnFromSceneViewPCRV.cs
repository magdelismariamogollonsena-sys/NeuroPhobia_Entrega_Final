#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class NeuroPhobiaSetSpawnFromSceneViewPCRV
{
    [MenuItem("Tools/NeuroPhobia/Fijar inicio desde la vista de Scene (PC y RV)", priority = 170)]
    public static void SetSpawn()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("NeuroPhobia","Detén Play antes de fijar el punto de inicio.","Aceptar");
            return;
        }

        SceneView sv = SceneView.lastActiveSceneView;
        if (sv == null || sv.camera == null)
        {
            EditorUtility.DisplayDialog("NeuroPhobia","Haz clic dentro de Scene, colócate donde quieres iniciar y vuelve a ejecutar el comando.","Aceptar");
            return;
        }

        Scene scene = SceneManager.GetActiveScene();
        GameObject pc = Find(scene, "NP_DesktopPlayer_Normalizado");
        if (pc == null) pc = Find(scene, "NP_DesktopPlayer_Niveles45");

        GameObject xr = Find(scene, "XR Origin (XR Rig)");
        if (xr == null) xr = Find(scene, "XR Origin");

        if (pc == null && xr == null)
        {
            EditorUtility.DisplayDialog("NeuroPhobia","No encontré jugador PC ni XR Origin.","Aceptar");
            return;
        }

        Vector3 scenePos = sv.camera.transform.position;
        float yaw = sv.camera.transform.eulerAngles.y;

        if (pc != null)
        {
            Undo.RegisterFullObjectHierarchyUndo(pc, "Fijar inicio PC");
            Vector3 p = pc.transform.position;
            p.x = scenePos.x;
            p.z = scenePos.z;
            pc.transform.position = p;
            pc.transform.rotation = Quaternion.Euler(0f, yaw, 0f);

            Transform pitch = FindT(pc.transform, "NP_MousePitchPivot");
            if (pitch != null) pitch.localRotation = Quaternion.identity;
            else
            {
                Transform pivot = FindT(pc.transform, "CameraPivot");
                if (pivot != null) pivot.localRotation = Quaternion.identity;
            }
            EditorUtility.SetDirty(pc);
        }

        if (xr != null)
        {
            Undo.RegisterFullObjectHierarchyUndo(xr, "Fijar inicio RV");
            Vector3 p = xr.transform.position;
            p.x = scenePos.x;
            p.z = scenePos.z;
            xr.transform.position = p;
            xr.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            EditorUtility.SetDirty(xr);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        EditorUtility.DisplayDialog(
            "NeuroPhobia",
            "Inicio guardado en " + scene.name +
            "\nX: " + scenePos.x.ToString("0.00") +
            "   Z: " + scenePos.z.ToString("0.00") +
            "\nDirección: " + yaw.ToString("0.0") + "°" +
            "\n\nAhora pulsa Play.",
            "Aceptar");
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

    static Transform FindT(Transform t, string name)
    {
        if (t == null) return null;
        if (string.Equals(t.name, name, StringComparison.OrdinalIgnoreCase)) return t;

        foreach (Transform child in t)
        {
            Transform found = FindT(child, name);
            if (found != null) return found;
        }
        return null;
    }
}
#endif
