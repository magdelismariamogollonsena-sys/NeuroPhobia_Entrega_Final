#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class NeuroPhobiaMainMenuModernInstaller
{
    private const string MainMenuPath =
        "Assets/Scenes/MainMenu.unity";

    private static readonly string[] LevelPaths =
    {
        "Assets/Scenes/Nivel1.unity",
        "Assets/Scenes/Nivel2.unity",
        "Assets/Scenes/Nivel3.unity",
        "Assets/Scenes/Nivel4.unity",
        "Assets/Scenes/Nivel5.unity"
    };

    [MenuItem(
        "Tools/NeuroPhobia/Mejorar menu principal FINAL",
        priority = 156)]
    public static void Install()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog(
                "NeuroPhobia",
                "Detén Play antes de mejorar el menú.",
                "Aceptar");
            return;
        }

        if (!File.Exists(MainMenuPath))
        {
            EditorUtility.DisplayDialog(
                "NeuroPhobia",
                "No encontré Assets/Scenes/MainMenu.unity",
                "Aceptar");
            return;
        }

        Scene scene = EditorSceneManager.OpenScene(
            MainMenuPath,
            OpenSceneMode.Single);

        GameObject root =
            FindSceneObject(
                scene,
                "NP_MainMenu_Moderno");

        if (root == null)
        {
            root = new GameObject("NP_MainMenu_Moderno");
            SceneManager.MoveGameObjectToScene(root, scene);
        }

        DisableOldMenuCanvases(scene, root);

        NeuroPhobiaMainMenuModern menu =
            root.GetComponent<NeuroPhobiaMainMenuModern>();

        if (menu == null)
        {
            menu =
                root.AddComponent<NeuroPhobiaMainMenuModern>();
        }

        menu.enabled = true;
        menu.RebuildMenu();

        EnsureMenuCamera(scene);
        AddScenesToBuild();

        EditorUtility.SetDirty(root);
        EditorUtility.SetDirty(menu);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Selection.activeGameObject = root;

        EditorUtility.DisplayDialog(
            "NeuroPhobia",
            "Menú principal mejorado.\n\n" +
            "Se creó una interfaz moderna con:\n" +
            "• Iniciar experiencia\n" +
            "• Elegir Nivel 1-5\n" +
            "• Controles e instrucciones\n" +
            "• Salir\n\n" +
            "La escena MainMenu quedó guardada y abierta.",
            "Aceptar");
    }

    private static void DisableOldMenuCanvases(
        Scene scene,
        GameObject modernRoot)
    {
        Canvas[] canvases =
            Object.FindObjectsByType<Canvas>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

        foreach (Canvas canvas in canvases)
        {
            if (canvas == null)
            {
                continue;
            }

            if (canvas.gameObject.scene != scene)
            {
                continue;
            }

            if (canvas.transform.IsChildOf(modernRoot.transform))
            {
                continue;
            }

            canvas.gameObject.SetActive(false);
            EditorUtility.SetDirty(canvas.gameObject);
        }
    }

    private static void EnsureMenuCamera(Scene scene)
    {
        Camera[] cameras =
            Object.FindObjectsByType<Camera>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

        Camera sceneCamera = null;

        foreach (Camera camera in cameras)
        {
            if (camera != null &&
                camera.gameObject.scene == scene)
            {
                sceneCamera = camera;
                break;
            }
        }

        if (sceneCamera == null)
        {
            GameObject cameraObject =
                new GameObject("NP_MenuCamera");

            SceneManager.MoveGameObjectToScene(
                cameraObject,
                scene);

            sceneCamera =
                cameraObject.AddComponent<Camera>();

            sceneCamera.clearFlags =
                CameraClearFlags.SolidColor;

            sceneCamera.backgroundColor =
                new Color(0.025f, 0.035f, 0.055f, 1f);

            sceneCamera.cullingMask = 0;

            cameraObject.AddComponent<AudioListener>();
        }

        sceneCamera.gameObject.SetActive(true);
        sceneCamera.enabled = true;

        try
        {
            sceneCamera.gameObject.tag = "MainCamera";
        }
        catch
        {
        }

        EditorUtility.SetDirty(sceneCamera);
    }

    private static void AddScenesToBuild()
    {
        List<EditorBuildSettingsScene> scenes =
            new List<EditorBuildSettingsScene>(
                EditorBuildSettings.scenes);

        AddOrEnable(scenes, MainMenuPath);

        foreach (string level in LevelPaths)
        {
            AddOrEnable(scenes, level);
        }

        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static void AddOrEnable(
        List<EditorBuildSettingsScene> scenes,
        string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        foreach (EditorBuildSettingsScene existing in scenes)
        {
            if (existing.path == path)
            {
                existing.enabled = true;
                return;
            }
        }

        scenes.Add(
            new EditorBuildSettingsScene(
                path,
                true));
    }

    private static GameObject FindSceneObject(
        Scene scene,
        string exactName)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform found =
                FindRecursive(
                    root.transform,
                    exactName);

            if (found != null)
            {
                return found.gameObject;
            }
        }

        return null;
    }

    private static Transform FindRecursive(
        Transform current,
        string exactName)
    {
        if (current == null)
        {
            return null;
        }

        if (current.name == exactName)
        {
            return current;
        }

        foreach (Transform child in current)
        {
            Transform result =
                FindRecursive(
                    child,
                    exactName);

            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
}
#endif
