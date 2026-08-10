#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class NeuroPhobiaFixDesktopCameraHeight
{
    [MenuItem("Tools/NeuroPhobia/Punto de inicio/3. Alinear camara al cuerpo", priority = 183)]
    public static void FixCamera()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("NeuroPhobia",
                "Detén Play antes de alinear la cámara.", "Aceptar");
            return;
        }

        Scene scene = SceneManager.GetActiveScene();

        GameObject player = Find(scene, "NP_DesktopPlayer_Normalizado");
        if (player == null)
            player = Find(scene, "NP_DesktopPlayer_Niveles45");

        if (player == null)
        {
            EditorUtility.DisplayDialog("NeuroPhobia",
                "No encontré el jugador de computador.", "Aceptar");
            return;
        }

        Transform cameraPivot = FindT(player.transform, "CameraPivot");
        Camera desktopCamera = player.GetComponentInChildren<Camera>(true);

        if (cameraPivot == null || desktopCamera == null)
        {
            EditorUtility.DisplayDialog("NeuroPhobia",
                "No encontré CameraPivot o DesktopCamera.", "Aceptar");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(player, "Alinear cámara NeuroPhobia");

        cameraPivot.localPosition = new Vector3(0f, 1.60f, 0f);
        cameraPivot.localRotation = Quaternion.identity;
        cameraPivot.localScale = Vector3.one;

        Transform pitchPivot = FindT(cameraPivot, "NP_MousePitchPivot");

        if (pitchPivot != null)
        {
            pitchPivot.localPosition = Vector3.zero;
            pitchPivot.localRotation = Quaternion.identity;
            pitchPivot.localScale = Vector3.one;
            desktopCamera.transform.SetParent(pitchPivot, false);
        }
        else
        {
            desktopCamera.transform.SetParent(cameraPivot, false);
        }

        desktopCamera.transform.localPosition = Vector3.zero;
        desktopCamera.transform.localRotation = Quaternion.identity;
        desktopCamera.transform.localScale = Vector3.one;
        desktopCamera.gameObject.SetActive(true);
        desktopCamera.enabled = true;

        EditorUtility.SetDirty(player);
        EditorUtility.SetDirty(cameraPivot);
        EditorUtility.SetDirty(desktopCamera);
        if (pitchPivot != null) EditorUtility.SetDirty(pitchPivot);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Selection.activeGameObject = desktopCamera.gameObject;

        EditorUtility.DisplayDialog("NeuroPhobia",
            "Cámara alineada a 1.60 m sobre el cuerpo.\n\nAhora pulsa Play.",
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
