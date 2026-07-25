#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class NeuroPhobiaNivel1PositionFix
{
    private static readonly Vector3 SpawnPosition = new Vector3(0f, 0.20f, 12.22f);

    [MenuItem("Tools/NeuroPhobia/Corregir posicion Nivel 1 DEFINITIVO")]
    public static void FixDesktopPosition()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog(
                "NeuroPhobia",
                "Deten el modo Play antes de ejecutar este comando.",
                "Aceptar");
            return;
        }

        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            EditorUtility.DisplayDialog("NeuroPhobia", "No hay una escena abierta.", "Aceptar");
            return;
        }

        GameObject environment = FindObject(scene, "NP_ENV_NIVEL1_CONSULTORIO", "CONSULTORIO");
        GameObject desktopRig = FindObject(scene, "NP_DesktopPlayer_Normalizado", "DesktopPlayer_Normalizado");
        GameObject xrOrigin = FindObject(scene, "XR Origin (XR Rig)", "XR Origin");
        GameObject xrSimulator = FindObject(scene, "XR Device Simulator", "XR Device Simulator");
        GameObject oldDesktopRig = FindObject(scene, "DesktopTestRig", "DesktopTestRig");
        GameObject oldFloor = FindObject(scene, "PisoCollider", "PisoCollider");
        GameObject normalizedFloor = FindObject(scene, "NP_PisoCollider_Normalizado", "NP_PisoCollider_Normalizado");

        if (desktopRig == null)
        {
            desktopRig = new GameObject("NP_DesktopPlayer_Normalizado");
            SceneManager.MoveGameObjectToScene(desktopRig, scene);
        }

        desktopRig.SetActive(true);
        RemoveRigidbody(desktopRig);

        CharacterController controller = desktopRig.GetComponent<CharacterController>();
        if (controller == null)
        {
            controller = desktopRig.AddComponent<CharacterController>();
        }

        controller.enabled = false;
        controller.center = new Vector3(0f, 0.9f, 0f);
        controller.height = 1.8f;
        controller.radius = 0.30f;
        controller.slopeLimit = 45f;
        controller.stepOffset = 0.30f;
        controller.skinWidth = 0.08f;
        controller.minMoveDistance = 0f;
        controller.enableOverlapRecovery = true;

        desktopRig.transform.position = SpawnPosition;
        desktopRig.transform.rotation = FaceEnvironment(environment, SpawnPosition);
        desktopRig.transform.localScale = Vector3.one;

        Transform cameraPivot = FindChild(desktopRig.transform, "CameraPivot");
        if (cameraPivot == null)
        {
            GameObject pivotObject = new GameObject("CameraPivot");
            pivotObject.transform.SetParent(desktopRig.transform, false);
            cameraPivot = pivotObject.transform;
        }

        cameraPivot.localPosition = new Vector3(0f, 1.60f, 0f);
        cameraPivot.localRotation = Quaternion.identity;
        cameraPivot.localScale = Vector3.one;

        Camera desktopCamera = cameraPivot.GetComponentInChildren<Camera>(true);
        if (desktopCamera == null)
        {
            GameObject cameraObject = new GameObject("DesktopCamera");
            cameraObject.transform.SetParent(cameraPivot, false);
            desktopCamera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
        }

        desktopCamera.gameObject.tag = "MainCamera";
        desktopCamera.transform.localPosition = Vector3.zero;
        desktopCamera.transform.localRotation = Quaternion.identity;
        desktopCamera.transform.localScale = Vector3.one;
        desktopCamera.nearClipPlane = 0.05f;
        desktopCamera.farClipPlane = 500f;
        desktopCamera.enabled = true;

        AudioListener desktopListener = desktopCamera.GetComponent<AudioListener>();
        if (desktopListener == null)
        {
            desktopListener = desktopCamera.gameObject.AddComponent<AudioListener>();
        }
        desktopListener.enabled = true;

        ResetXrTransforms(xrOrigin);

        if (xrOrigin != null) xrOrigin.SetActive(false);
        if (xrSimulator != null) xrSimulator.SetActive(false);
        if (oldDesktopRig != null && oldDesktopRig != desktopRig) oldDesktopRig.SetActive(false);
        if (oldFloor != null && oldFloor != normalizedFloor) oldFloor.SetActive(false);

        normalizedFloor = ConfigureFloor(scene, normalizedFloor, environment, SpawnPosition);

        DisableOtherCameras(scene, desktopCamera);
        controller.enabled = true;

        Selection.activeGameObject = desktopRig;
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        EditorUtility.DisplayDialog(
            "NeuroPhobia",
            "Posicion corregida.\n\n" +
            "Jugador: X 0, Y 0.20, Z 12.22\n" +
            "La camara fue orientada automaticamente hacia el consultorio.\n" +
            "XR Origin, XR Device Simulator, DesktopTestRig y PisoCollider antiguos quedaron desactivados.\n\n" +
            "Ahora pulsa Play. Durante el juego, R regresa al punto inicial.",
            "Aceptar");
    }

    [MenuItem("Tools/NeuroPhobia/Preparar Nivel 1 para Quest")]
    public static void PrepareQuest()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog(
                "NeuroPhobia",
                "Deten el modo Play antes de ejecutar este comando.",
                "Aceptar");
            return;
        }

        Scene scene = SceneManager.GetActiveScene();
        GameObject desktopRig = FindObject(scene, "NP_DesktopPlayer_Normalizado", "DesktopPlayer_Normalizado");
        GameObject xrOrigin = FindObject(scene, "XR Origin (XR Rig)", "XR Origin");
        GameObject xrSimulator = FindObject(scene, "XR Device Simulator", "XR Device Simulator");

        ResetXrTransforms(xrOrigin);

        if (desktopRig != null) desktopRig.SetActive(false);
        if (xrOrigin != null) xrOrigin.SetActive(true);
        if (xrSimulator != null) xrSimulator.SetActive(false);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        EditorUtility.DisplayDialog(
            "NeuroPhobia",
            "Nivel 1 preparado para Quest.\nXR Origin activo y jugador de computador desactivado.",
            "Aceptar");
    }

    private static GameObject ConfigureFloor(
        Scene scene,
        GameObject floor,
        GameObject environment,
        Vector3 spawn)
    {
        if (floor == null)
        {
            floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "NP_PisoCollider_Normalizado";
            SceneManager.MoveGameObjectToScene(floor, scene);
        }

        floor.SetActive(true);
        RemoveRigidbody(floor);

        Bounds bounds;
        if (!TryGetBounds(environment, out bounds))
        {
            bounds = new Bounds(new Vector3(0f, 0f, 10f), new Vector3(8f, 0.2f, 10f));
        }

        float minX = Mathf.Min(bounds.min.x - 1f, spawn.x - 1f);
        float maxX = Mathf.Max(bounds.max.x + 1f, spawn.x + 1f);
        float minZ = Mathf.Min(bounds.min.z - 1f, spawn.z - 1f);
        float maxZ = Mathf.Max(bounds.max.z + 1f, spawn.z + 1f);

        float floorY = bounds.min.y - 0.10f;
        floor.transform.position = new Vector3(
            (minX + maxX) * 0.5f,
            floorY,
            (minZ + maxZ) * 0.5f);
        floor.transform.rotation = Quaternion.identity;
        floor.transform.localScale = new Vector3(
            Mathf.Max(8f, maxX - minX),
            0.20f,
            Mathf.Max(10f, maxZ - minZ));

        MeshRenderer renderer = floor.GetComponent<MeshRenderer>();
        if (renderer != null) renderer.enabled = false;

        BoxCollider collider = floor.GetComponent<BoxCollider>();
        if (collider == null) collider = floor.AddComponent<BoxCollider>();
        collider.enabled = true;
        collider.isTrigger = false;
        collider.center = Vector3.zero;
        collider.size = Vector3.one;

        return floor;
    }

    private static Quaternion FaceEnvironment(GameObject environment, Vector3 spawn)
    {
        Bounds bounds;
        if (TryGetBounds(environment, out bounds))
        {
            Vector3 direction = bounds.center - spawn;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.01f)
            {
                return Quaternion.LookRotation(direction.normalized, Vector3.up);
            }
        }

        return Quaternion.Euler(0f, 180f, 0f);
    }

    private static void ResetXrTransforms(GameObject xrOrigin)
    {
        if (xrOrigin == null) return;

        xrOrigin.transform.position = SpawnPosition;
        xrOrigin.transform.rotation = Quaternion.identity;
        xrOrigin.transform.localScale = Vector3.one;

        Transform cameraOffset = FindChild(xrOrigin.transform, "Camera Offset");
        if (cameraOffset != null)
        {
            cameraOffset.localPosition = Vector3.zero;
            cameraOffset.localRotation = Quaternion.identity;
            cameraOffset.localScale = Vector3.one;

            Camera xrCamera = cameraOffset.GetComponentInChildren<Camera>(true);
            if (xrCamera != null)
            {
                xrCamera.transform.localPosition = Vector3.zero;
                xrCamera.transform.localRotation = Quaternion.identity;
                xrCamera.transform.localScale = Vector3.one;
            }
        }
    }

    private static void DisableOtherCameras(Scene scene, Camera desktopCamera)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Camera cameraComponent in root.GetComponentsInChildren<Camera>(true))
            {
                bool isDesktop = cameraComponent == desktopCamera;
                cameraComponent.enabled = isDesktop;

                AudioListener listener = cameraComponent.GetComponent<AudioListener>();
                if (listener != null)
                {
                    listener.enabled = isDesktop;
                }
            }
        }
    }

    private static GameObject FindObject(Scene scene, string exactName, string containsName)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            GameObject result = FindRecursive(root.transform, exactName, containsName);
            if (result != null) return result;
        }
        return null;
    }

    private static GameObject FindRecursive(Transform current, string exactName, string containsName)
    {
        if (current.name == exactName ||
            current.name.IndexOf(containsName, System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return current.gameObject;
        }

        foreach (Transform child in current)
        {
            GameObject result = FindRecursive(child, exactName, containsName);
            if (result != null) return result;
        }

        return null;
    }

    private static Transform FindChild(Transform root, string name)
    {
        if (root == null) return null;
        if (root.name == name) return root;

        foreach (Transform child in root)
        {
            Transform result = FindChild(child, name);
            if (result != null) return result;
        }

        return null;
    }

    private static bool TryGetBounds(GameObject gameObject, out Bounds bounds)
    {
        bounds = new Bounds();
        if (gameObject == null) return false;

        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return false;

        bounds = renderers[0].bounds;
        for (int index = 1; index < renderers.Length; index++)
        {
            bounds.Encapsulate(renderers[index].bounds);
        }

        return true;
    }

    private static void RemoveRigidbody(GameObject gameObject)
    {
        if (gameObject == null) return;
        Rigidbody rigidbodyComponent = gameObject.GetComponent<Rigidbody>();
        if (rigidbodyComponent != null)
        {
            Object.DestroyImmediate(rigidbodyComponent);
        }
    }
}
#endif
