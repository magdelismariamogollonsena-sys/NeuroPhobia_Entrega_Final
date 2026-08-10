#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class NeuroPhobiaLevel2SafeDistanceInstaller
{
    private const string ScenePath = "Assets/Scenes/Nivel2.unity";

    [MenuItem("Tools/NeuroPhobia/Reparar LIMITE de distancia Nivel 2", priority = 157)]
    public static void Install()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog(
                "NeuroPhobia",
                "Detén Play antes de reparar el límite.",
                "Aceptar");
            return;
        }

        if (!File.Exists(ScenePath))
        {
            EditorUtility.DisplayDialog(
                "NeuroPhobia",
                "No encontré Assets/Scenes/Nivel2.unity",
                "Aceptar");
            return;
        }

        Scene scene = EditorSceneManager.OpenScene(
            ScenePath,
            OpenSceneMode.Single);

        GameObject root = FindSceneObject(
            scene,
            "NP_Nivel2_Limite_DistanciaSegura");

        if (root == null)
        {
            root = new GameObject("NP_Nivel2_Limite_DistanciaSegura");
            SceneManager.MoveGameObjectToScene(root, scene);
        }

        NeuroPhobiaLevel2SafeDistance limiter =
            root.GetComponent<NeuroPhobiaLevel2SafeDistance>();

        if (limiter == null)
            limiter = root.AddComponent<NeuroPhobiaLevel2SafeDistance>();

        GameObject desktop =
            FindSceneObject(scene, "NP_DesktopPlayer_Normalizado");

        if (desktop == null)
            desktop = FindSceneObject(scene, "NP_DesktopPlayer_Niveles45");

        GameObject xr =
            FindSceneObject(scene, "XR Origin (XR Rig)");

        if (xr == null)
            xr = FindSceneObject(scene, "XR Origin");

        limiter.desktopPlayer = desktop;
        limiter.xrOrigin = xr;
        limiter.minimumDistance = 3.0f;
        limiter.boundaryPadding = 0.08f;
        limiter.enabled = true;

        EditorUtility.SetDirty(root);
        EditorUtility.SetDirty(limiter);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Selection.activeGameObject = root;

        EditorUtility.DisplayDialog(
            "NeuroPhobia",
            "Límite de distancia restaurado en Nivel 2.\n\n" +
            "Distancia mínima: 3.0 m\n" +
            "Si intentas acercarte más, el jugador se mantiene en el borde seguro.\n\n" +
            "Funciona con Computador y RV.",
            "Aceptar");
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

        if (current.name == exactName)
            return current;

        foreach (Transform child in current)
        {
            Transform found = FindRecursive(child, exactName);

            if (found != null)
                return found;
        }

        return null;
    }
}
#endif
