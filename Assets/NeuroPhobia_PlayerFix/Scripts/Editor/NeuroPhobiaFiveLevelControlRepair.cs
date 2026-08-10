#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class NeuroPhobiaFiveLevelControlRepair
{
    private static readonly string[] ScenePaths =
    {
        "Assets/Scenes/Nivel1.unity",
        "Assets/Scenes/Nivel2.unity",
        "Assets/Scenes/Nivel3.unity",
        "Assets/Scenes/Nivel4.unity",
        "Assets/Scenes/Nivel5.unity"
    };

    [MenuItem(
        "Tools/NeuroPhobia/Reparar controles, selector y orbes Niveles 1-5",
        priority = 155)]
    public static void RepairAll()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog(
                "NeuroPhobia",
                "Detén Play antes de ejecutar la reparación.",
                "Aceptar");
            return;
        }

        int scenes = 0;
        int selectors = 0;
        int players = 0;
        List<string> warnings = new List<string>();

        try
        {
            foreach (string path in ScenePaths)
            {
                if (!File.Exists(path))
                {
                    warnings.Add("No existe: " + path);
                    continue;
                }

                Scene scene =
                    EditorSceneManager.OpenScene(
                        path,
                        OpenSceneMode.Single);

                GameObject desktop =
                    FindSceneObject(
                        scene,
                        "NP_DesktopPlayer_Normalizado");

                if (desktop == null)
                {
                    desktop =
                        FindSceneObject(
                            scene,
                            "NP_DesktopPlayer_Niveles45");
                }

                if (desktop == null)
                {
                    warnings.Add(
                        scene.name +
                        ": no encontré jugador de computador.");
                    continue;
                }

                desktop.SetActive(true);
                SafeSetTag(desktop, "Player");

                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(
                    desktop);

                CharacterController character =
                    desktop.GetComponent<CharacterController>();

                if (character == null)
                {
                    character =
                        desktop.AddComponent<CharacterController>();

                    character.height = 1.8f;
                    character.radius = 0.35f;
                    character.center =
                        new Vector3(0f, 0.9f, 0f);
                    character.stepOffset = 0.3f;
                    character.slopeLimit = 45f;
                }

                Camera camera =
                    desktop.GetComponentInChildren<Camera>(true);

                if (camera == null)
                {
                    warnings.Add(
                        scene.name +
                        ": el jugador PC no contiene cámara.");
                    continue;
                }

                Transform cameraPivot =
                    FindRecursive(
                        desktop.transform,
                        "CameraPivot");

                if (cameraPivot == null)
                {
                    GameObject pivotObject =
                        new GameObject("CameraPivot");

                    cameraPivot = pivotObject.transform;
                    cameraPivot.SetParent(
                        desktop.transform,
                        false);
                }

                Transform pitchPivot =
                    FindRecursive(
                        cameraPivot,
                        "NP_MousePitchPivot");

                if (pitchPivot == null)
                {
                    GameObject pitchObject =
                        new GameObject("NP_MousePitchPivot");

                    pitchPivot = pitchObject.transform;
                    pitchPivot.SetParent(cameraPivot, false);
                    pitchPivot.localPosition = Vector3.zero;
                    pitchPivot.localRotation = Quaternion.identity;
                    pitchPivot.localScale = Vector3.one;
                }

                if (!camera.transform.IsChildOf(pitchPivot))
                {
                    camera.transform.SetParent(
                        pitchPivot,
                        true);
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

                controller.cameraPivot = cameraPivot;
                controller.pitchPivot = pitchPivot;
                controller.playerCamera = camera;
                controller.moveSpeed = 2.8f;
                controller.runMultiplier = 1.55f;
                controller.gravity = -18f;
                controller.mouseSensitivityX = 0.10f;
                controller.mouseSensitivityY = 0.10f;
                controller.minPitch = -80f;
                controller.maxPitch = 80f;
                controller.lockCursorOnEnable = true;
                controller.enabled = true;

                camera.gameObject.SetActive(true);
                camera.enabled = true;

                GameObject xr =
                    FindSceneObject(
                        scene,
                        "XR Origin (XR Rig)");

                if (xr == null)
                {
                    xr =
                        FindSceneObject(
                            scene,
                            "XR Origin");
                }

                if (xr != null)
                {
                    SafeSetTag(xr, "Player");
                    xr.SetActive(false);
                }

                DisableIfPresent(
                    scene,
                    "XR Device Simulator");

                GameObject oldSelector =
                    FindSceneObject(
                        scene,
                        "NP_Seleccion_Modo_Control");

                if (oldSelector != null)
                {
                    UnityEngine.Object.DestroyImmediate(
                        oldSelector);
                }

                GameObject selectorObject =
                    new GameObject(
                        "NP_Seleccion_Modo_Control");

                SceneManager.MoveGameObjectToScene(
                    selectorObject,
                    scene);

                NeuroPhobiaControlModeSelector selector =
                    selectorObject.AddComponent<
                        NeuroPhobiaControlModeSelector>();

                selector.desktopPlayer = desktop;
                selector.xrOrigin = xr;
                selector.desktopCamera = camera;
                selector.askEveryTimeSceneStarts = true;
                selector.inputGuardSeconds = 0.45f;
                selector.enabled = true;

                selectors++;
                players++;

                if (scene.name == "Nivel3")
                {
                    RepairLevel3Orbs(
                        scene,
                        desktop,
                        xr);
                }

                EditorUtility.SetDirty(desktop);
                EditorUtility.SetDirty(controller);
                EditorUtility.SetDirty(selectorObject);
                EditorUtility.SetDirty(selector);

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                scenes++;
            }

            AddScenesToBuild();

            string level3Path =
                "Assets/Scenes/Nivel3.unity";

            if (File.Exists(level3Path))
            {
                EditorSceneManager.OpenScene(
                    level3Path,
                    OpenSceneMode.Single);
            }

            string message =
                "Reparación terminada.\n\n" +
                "Escenas guardadas: " + scenes + "\n" +
                "Jugadores PC reparados: " + players + "\n" +
                "Selectores recreados: " + selectors + "\n\n" +
                "Se dejó abierto Nivel3 para probar los orbes.";

            if (warnings.Count > 0)
            {
                message +=
                    "\n\nAvisos:\n" +
                    string.Join("\n", warnings.ToArray());
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
                "La reparación se detuvo.\n\n" +
                ex.Message +
                "\n\nEnvía el primer error rojo de Console.",
                "Aceptar");
        }
    }

    private static void RepairLevel3Orbs(
        Scene scene,
        GameObject desktop,
        GameObject xr)
    {
        GameObject root =
            FindSceneObject(
                scene,
                "NP_Level3_OrbCompatibility");

        if (root == null)
        {
            root =
                new GameObject(
                    "NP_Level3_OrbCompatibility");

            SceneManager.MoveGameObjectToScene(
                root,
                scene);
        }

        NeuroPhobiaLevel3OrbCompatibility compatibility =
            root.GetComponent<
                NeuroPhobiaLevel3OrbCompatibility>();

        if (compatibility == null)
        {
            compatibility =
                root.AddComponent<
                    NeuroPhobiaLevel3OrbCompatibility>();
        }

        compatibility.desktopPlayer = desktop;
        compatibility.xrOrigin = xr;
        compatibility.collectRadius = 1.35f;
        compatibility.enabled = true;

        EditorUtility.SetDirty(root);
        EditorUtility.SetDirty(compatibility);
    }

    private static void DisableIfPresent(
        Scene scene,
        string name)
    {
        GameObject go =
            FindSceneObject(scene, name);

        if (go != null)
        {
            go.SetActive(false);
            EditorUtility.SetDirty(go);
        }
    }

    private static void SafeSetTag(
        GameObject go,
        string tagName)
    {
        if (go == null)
        {
            return;
        }

        try
        {
            go.tag = tagName;
        }
        catch
        {
            Debug.LogWarning(
                "NeuroPhobia: no se pudo asignar Tag " +
                tagName +
                " a " +
                go.name);
        }
    }

    private static void AddScenesToBuild()
    {
        List<EditorBuildSettingsScene> scenes =
            new List<EditorBuildSettingsScene>(
                EditorBuildSettings.scenes);

        foreach (string path in ScenePaths)
        {
            if (!File.Exists(path))
            {
                continue;
            }

            bool exists = false;

            foreach (EditorBuildSettingsScene existing in scenes)
            {
                if (existing.path == path)
                {
                    exists = true;
                    existing.enabled = true;
                    break;
                }
            }

            if (!exists)
            {
                scenes.Add(
                    new EditorBuildSettingsScene(
                        path,
                        true));
            }
        }

        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static GameObject FindSceneObject(
        Scene scene,
        string exactName)
    {
        foreach (GameObject root in
                 scene.GetRootGameObjects())
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

        if (string.Equals(
                current.name,
                exactName,
                StringComparison.OrdinalIgnoreCase))
        {
            return current;
        }

        foreach (Transform child in current)
        {
            Transform found =
                FindRecursive(
                    child,
                    exactName);

            if (found != null)
            {
                return found;
            }
        }

        return null;
    }
}
#endif
