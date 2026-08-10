#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class NeuroPhobiaFiveLevelControlInstaller
{
    private static readonly string[] ScenePaths =
    {
        "Assets/Scenes/Nivel1.unity",
        "Assets/Scenes/Nivel2.unity",
        "Assets/Scenes/Nivel3.unity",
        "Assets/Scenes/Nivel4.unity",
        "Assets/Scenes/Nivel5.unity"
    };

    private static readonly string[] DesktopNames =
    {
        "NP_DesktopPlayer_Normalizado",
        "NP_DesktopPlayer_Niveles45"
    };

    private static readonly string[] XRNames =
    {
        "XR Origin (XR Rig)",
        "XR Origin",
        "XROrigin"
    };

    [MenuItem(
        "Tools/NeuroPhobia/Instalar selector Computador-RV en Niveles 1-5",
        priority = 145)]
    public static void Install()
    {
        if (!CanEdit())
        {
            return;
        }

        int scenesConfigured = 0;
        int desktopPlayersFixed = 0;
        int xrRigsFoundOrCreated = 0;
        List<string> warnings =
            new List<string>();

        try
        {
            foreach (string scenePath in ScenePaths)
            {
                if (!File.Exists(scenePath))
                {
                    warnings.Add(
                        "No existe " + scenePath);
                    continue;
                }

                Scene scene =
                    EditorSceneManager.OpenScene(
                        scenePath,
                        OpenSceneMode.Single);

                GameObject desktop =
                    FindDesktopPlayer(scene);

                if (desktop == null)
                {
                    desktop =
                        CreateDesktopPlayerFromSceneCamera(
                            scene);
                }

                if (desktop == null)
                {
                    warnings.Add(
                        scene.name +
                        ": no fue posible crear/encontrar el jugador de computador.");
                    continue;
                }

                RepairDesktopPlayer(desktop);
                desktopPlayersFixed++;

                GameObject xr =
                    FindXRRoot(scene);

                if (xr == null)
                {
                    xr =
                        CreateXRRootFromStarterPrefab(
                            scene);
                }

                if (xr != null)
                {
                    AlignXRWithDesktop(
                        xr,
                        desktop);
                    xr.SetActive(false);
                    EditorUtility.SetDirty(xr);
                    xrRigsFoundOrCreated++;
                }
                else
                {
                    warnings.Add(
                        scene.name +
                        ": no encontré XR Origin ni el prefab Starter Assets.");
                }

                DisableIfPresent(
                    scene,
                    "XR Device Simulator");

                GameObject selector =
                    FindSceneObject(
                        scene,
                        "NP_Seleccion_Modo_Control");

                if (selector == null)
                {
                    selector =
                        new GameObject(
                            "NP_Seleccion_Modo_Control");

                    SceneManager.MoveGameObjectToScene(
                        selector,
                        scene);
                }

                NeuroPhobiaControlModeSelector component =
                    selector.GetComponent<
                        NeuroPhobiaControlModeSelector>();

                if (component == null)
                {
                    component =
                        selector.AddComponent<
                            NeuroPhobiaControlModeSelector>();
                }

                component.desktopPlayer = desktop;
                component.xrOrigin = xr;
                component.desktopCamera =
                    desktop.GetComponentInChildren<Camera>(true);
                component.askEveryTimeSceneStarts = true;
                component.inputGuardSeconds = 0.65f;
                component.enabled = true;

                desktop.SetActive(true);

                EditorUtility.SetDirty(desktop);
                EditorUtility.SetDirty(selector);
                EditorUtility.SetDirty(component);

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);

                scenesConfigured++;
            }

            AddAllLevelsToBuild();

            if (File.Exists(ScenePaths[0]))
            {
                EditorSceneManager.OpenScene(
                    ScenePaths[0],
                    OpenSceneMode.Single);
            }

            string message =
                "Selector PC/RV instalado.\n\n" +
                "Escenas configuradas: " +
                scenesConfigured + " / 5\n" +
                "Jugadores de PC reparados: " +
                desktopPlayersFixed + "\n" +
                "XR Origin encontrados/creados: " +
                xrRigsFoundOrCreated + "\n\n" +
                "Ahora pulsa Play en Nivel1.\n" +
                "Antes de iniciar aparecerá:\n" +
                "USAR COMPUTADOR / USAR REALIDAD VIRTUAL.";

            if (warnings.Count > 0)
            {
                message +=
                    "\n\nAvisos:\n• " +
                    string.Join(
                        "\n• ",
                        warnings.ToArray());
            }

            EditorUtility.DisplayDialog(
                "NeuroPhobia",
                message,
                "Aceptar");
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);

            EditorUtility.DisplayDialog(
                "NeuroPhobia",
                "La instalación se detuvo.\n\n" +
                ex.Message +
                "\n\nEnvía el primer error rojo completo de Console.",
                "Aceptar");
        }
    }

    [MenuItem(
        "Tools/NeuroPhobia/Verificar controles PC-RV Niveles 1-5",
        priority = 146)]
    public static void Verify()
    {
        if (!CanEdit())
        {
            return;
        }

        List<string> results =
            new List<string>();

        foreach (string scenePath in ScenePaths)
        {
            if (!File.Exists(scenePath))
            {
                results.Add(
                    Path.GetFileNameWithoutExtension(
                        scenePath) +
                    ": FALTA ESCENA");
                continue;
            }

            Scene scene =
                EditorSceneManager.OpenScene(
                    scenePath,
                    OpenSceneMode.Single);

            GameObject desktop =
                FindDesktopPlayer(scene);

            GameObject xr =
                FindXRRoot(scene);

            GameObject selector =
                FindSceneObject(
                    scene,
                    "NP_Seleccion_Modo_Control");

            bool hasController =
                desktop != null &&
                desktop.GetComponent<
                    NeuroPhobiaDesktopController>() != null;

            string state =
                scene.name + ": " +
                "PC " +
                (hasController ? "OK" : "FALTA") +
                " | RV " +
                (xr != null ? "OK" : "FALTA") +
                " | Selector " +
                (selector != null ? "OK" : "FALTA");

            results.Add(state);
        }

        if (File.Exists(ScenePaths[0]))
        {
            EditorSceneManager.OpenScene(
                ScenePaths[0],
                OpenSceneMode.Single);
        }

        EditorUtility.DisplayDialog(
            "NeuroPhobia",
            string.Join(
                "\n",
                results.ToArray()),
            "Aceptar");
    }

    private static bool CanEdit()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog(
                "NeuroPhobia",
                "Detén Play antes de ejecutar este comando.",
                "Aceptar");
            return false;
        }

        return true;
    }

    private static void RepairDesktopPlayer(
        GameObject desktop)
    {
        desktop.SetActive(true);

        GameObjectUtility
            .RemoveMonoBehavioursWithMissingScript(
                desktop);

        CharacterController character =
            desktop.GetComponent<
                CharacterController>();

        if (character == null)
        {
            character =
                desktop.AddComponent<
                    CharacterController>();

            character.height = 1.8f;
            character.radius = 0.30f;
            character.center =
                new Vector3(0f, 0.9f, 0f);
            character.stepOffset = 0.30f;
            character.slopeLimit = 45f;
        }

        NeuroPhobiaDesktopController controller =
            desktop.GetComponent<
                NeuroPhobiaDesktopController>();

        if (controller == null)
        {
            controller =
                desktop.AddComponent<
                    NeuroPhobiaDesktopController>();
        }

        Camera camera =
            desktop.GetComponentInChildren<Camera>(true);

        if (camera == null)
        {
            camera =
                CreateDesktopCamera(
                    desktop);
        }

        Transform pivot =
            FindChildRecursive(
                desktop.transform,
                "CameraPivot");

        controller.cameraPivot = pivot;
        controller.playerCamera = camera;
        controller.moveSpeed = 2.8f;
        controller.runMultiplier = 1.55f;
        controller.gravity = -18f;
        controller.mouseSensitivity = 0.10f;
        controller.lockCursorOnEnable = true;
        controller.enabled = true;

        if (camera != null)
        {
            camera.gameObject.SetActive(true);
            camera.enabled = true;

            try
            {
                camera.gameObject.tag =
                    "MainCamera";
            }
            catch
            {
            }

            AudioListener listener =
                camera.GetComponent<AudioListener>();

            if (listener == null)
            {
                listener =
                    camera.gameObject.AddComponent<
                        AudioListener>();
            }

            listener.enabled = true;
        }

        MonoBehaviour[] behaviours =
            desktop.GetComponents<
                MonoBehaviour>();

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour == null ||
                behaviour == controller)
            {
                continue;
            }

            string typeName =
                behaviour.GetType().Name;

            if (typeName ==
                    "NPDesktopFallbackPlayer" ||
                typeName ==
                    "DesktopPlayerController" ||
                typeName ==
                    "DesktopController")
            {
                behaviour.enabled = false;
                EditorUtility.SetDirty(behaviour);
            }
        }

        EditorUtility.SetDirty(controller);
        EditorUtility.SetDirty(desktop);
    }

    private static Camera CreateDesktopCamera(
        GameObject desktop)
    {
        GameObject pivot =
            new GameObject("CameraPivot");

        pivot.transform.SetParent(
            desktop.transform,
            false);

        GameObject cameraObject =
            new GameObject("DesktopCamera");

        cameraObject.transform.SetParent(
            pivot.transform,
            false);

        cameraObject.transform.localPosition =
            new Vector3(0f, 1.65f, 0f);

        Camera camera =
            cameraObject.AddComponent<Camera>();

        cameraObject.AddComponent<AudioListener>();

        try
        {
            cameraObject.tag =
                "MainCamera";
        }
        catch
        {
        }

        return camera;
    }

    private static GameObject
        CreateDesktopPlayerFromSceneCamera(
            Scene scene)
    {
        Camera existingCamera =
            FindBestSceneCamera(scene);

        Vector3 position =
            existingCamera != null
                ? existingCamera.transform.position
                : new Vector3(0f, 1.65f, 0f);

        Quaternion rotation =
            existingCamera != null
                ? Quaternion.Euler(
                    0f,
                    existingCamera.transform.eulerAngles.y,
                    0f)
                : Quaternion.identity;

        GameObject player =
            new GameObject(
                "NP_DesktopPlayer_Normalizado");

        SceneManager.MoveGameObjectToScene(
            player,
            scene);

        player.transform.position =
            new Vector3(
                position.x,
                0f,
                position.z);

        player.transform.rotation = rotation;

        player.AddComponent<
            CharacterController>();

        Camera camera =
            CreateDesktopCamera(player);

        if (existingCamera != null)
        {
            camera.transform.position =
                existingCamera.transform.position;
            camera.transform.rotation =
                existingCamera.transform.rotation;
        }

        return player;
    }

    private static GameObject FindDesktopPlayer(
        Scene scene)
    {
        foreach (string name in DesktopNames)
        {
            GameObject exact =
                FindSceneObject(
                    scene,
                    name);

            if (exact != null)
            {
                return exact;
            }
        }

        foreach (GameObject root in
                 scene.GetRootGameObjects())
        {
            Transform[] transforms =
                root.GetComponentsInChildren<
                    Transform>(true);

            foreach (Transform candidate in transforms)
            {
                string lower =
                    candidate.name.ToLowerInvariant();

                if (lower.Contains("desktop") &&
                    lower.Contains("player"))
                {
                    return candidate.gameObject;
                }
            }
        }

        return null;
    }

    private static GameObject FindXRRoot(
        Scene scene)
    {
        foreach (string name in XRNames)
        {
            GameObject exact =
                FindSceneObject(
                    scene,
                    name);

            if (exact != null)
            {
                return exact;
            }
        }

        foreach (GameObject root in
                 scene.GetRootGameObjects())
        {
            Transform[] transforms =
                root.GetComponentsInChildren<
                    Transform>(true);

            foreach (Transform candidate in transforms)
            {
                string lower =
                    candidate.name.ToLowerInvariant();

                if ((lower.Contains("xr origin") ||
                     lower.Contains("xrorigin")) &&
                    !lower.Contains("simulator"))
                {
                    return candidate.gameObject;
                }
            }
        }

        return null;
    }

    private static GameObject
        CreateXRRootFromStarterPrefab(
            Scene scene)
    {
        string exactPath =
            "Assets/Samples/XR Interaction Toolkit/3.3.1/Starter Assets/Prefabs/XR Origin (XR Rig).prefab";

        GameObject prefab =
            AssetDatabase.LoadAssetAtPath<
                GameObject>(exactPath);

        if (prefab == null)
        {
            string[] guids =
                AssetDatabase.FindAssets(
                    "\"XR Origin (XR Rig)\" t:Prefab");

            foreach (string guid in guids)
            {
                string path =
                    AssetDatabase.GUIDToAssetPath(
                        guid);

                GameObject candidate =
                    AssetDatabase.LoadAssetAtPath<
                        GameObject>(path);

                if (candidate != null)
                {
                    prefab = candidate;
                    break;
                }
            }
        }

        if (prefab == null)
        {
            return null;
        }

        GameObject instance =
            PrefabUtility.InstantiatePrefab(
                prefab) as GameObject;

        if (instance == null)
        {
            instance =
                UnityEngine.Object.Instantiate(
                    prefab);
        }

        SceneManager.MoveGameObjectToScene(
            instance,
            scene);

        instance.name =
            "XR Origin (XR Rig)";

        return instance;
    }

    private static void AlignXRWithDesktop(
        GameObject xr,
        GameObject desktop)
    {
        if (xr == null || desktop == null)
        {
            return;
        }

        Vector3 desktopPosition =
            desktop.transform.position;

        xr.transform.position =
            new Vector3(
                desktopPosition.x,
                0f,
                desktopPosition.z);

        Vector3 desktopEuler =
            desktop.transform.eulerAngles;

        xr.transform.rotation =
            Quaternion.Euler(
                0f,
                desktopEuler.y,
                0f);
    }

    private static Camera FindBestSceneCamera(
        Scene scene)
    {
        Camera fallback = null;

        foreach (GameObject root in
                 scene.GetRootGameObjects())
        {
            Camera[] cameras =
                root.GetComponentsInChildren<
                    Camera>(true);

            foreach (Camera camera in cameras)
            {
                if (camera == null)
                {
                    continue;
                }

                if (fallback == null)
                {
                    fallback = camera;
                }

                if (camera.enabled &&
                    camera.gameObject.activeInHierarchy)
                {
                    return camera;
                }
            }
        }

        return fallback;
    }

    private static void DisableIfPresent(
        Scene scene,
        string objectName)
    {
        GameObject go =
            FindSceneObject(
                scene,
                objectName);

        if (go != null)
        {
            go.SetActive(false);
            EditorUtility.SetDirty(go);
        }
    }

    private static GameObject FindSceneObject(
        Scene scene,
        string exactName)
    {
        foreach (GameObject root in
                 scene.GetRootGameObjects())
        {
            Transform found =
                FindChildRecursive(
                    root.transform,
                    exactName);

            if (found != null)
            {
                return found.gameObject;
            }
        }

        return null;
    }

    private static Transform FindChildRecursive(
        Transform current,
        string exactName)
    {
        if (string.Equals(
            current.name,
            exactName,
            StringComparison.OrdinalIgnoreCase))
        {
            return current;
        }

        foreach (Transform child in current)
        {
            Transform result =
                FindChildRecursive(
                    child,
                    exactName);

            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private static void AddAllLevelsToBuild()
    {
        List<EditorBuildSettingsScene> existing =
            EditorBuildSettings.scenes.ToList();

        foreach (string scenePath in ScenePaths)
        {
            if (!File.Exists(scenePath))
            {
                continue;
            }

            int index =
                existing.FindIndex(
                    item =>
                        item.path == scenePath);

            if (index < 0)
            {
                existing.Add(
                    new EditorBuildSettingsScene(
                        scenePath,
                        true));
            }
            else
            {
                existing[index] =
                    new EditorBuildSettingsScene(
                        scenePath,
                        true);
            }
        }

        EditorBuildSettings.scenes =
            existing.ToArray();
    }
}
#endif
