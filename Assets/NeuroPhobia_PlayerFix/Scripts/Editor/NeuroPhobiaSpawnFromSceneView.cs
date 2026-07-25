#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class NeuroPhobiaSpawnFromSceneView
{
    private const string RigName = "NP_DesktopPlayer_Normalizado";

    [MenuItem("Tools/NeuroPhobia/Fijar inicio desde la vista de Scene")]
    public static void SetSpawnFromSceneView()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog(
                "NeuroPhobia",
                "Detén Play antes de ejecutar este comando.",
                "Aceptar");
            return;
        }

        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView == null || sceneView.camera == null)
        {
            EditorUtility.DisplayDialog(
                "NeuroPhobia",
                "Primero haz clic dentro de la pestaña Scene.",
                "Aceptar");
            return;
        }

        Scene scene = SceneManager.GetActiveScene();
        GameObject rig = FindInScene(scene, RigName);
        if (rig == null)
        {
            EditorUtility.DisplayDialog(
                "NeuroPhobia",
                "No encontré el objeto NP_DesktopPlayer_Normalizado.",
                "Aceptar");
            return;
        }

        Camera sceneCamera = sceneView.camera;
        Vector3 spawn = sceneCamera.transform.position - Vector3.up * 1.60f;

        RaycastHit hit;
        if (Physics.Raycast(sceneCamera.transform.position, Vector3.down, out hit, 100f))
        {
            spawn.y = hit.point.y + 0.02f;
        }

        float yaw = sceneCamera.transform.eulerAngles.y;
        Vector3 spawnRotation = new Vector3(0f, yaw, 0f);

        CharacterController character = rig.GetComponent<CharacterController>();
        if (character != null)
        {
            character.enabled = false;
        }

        rig.transform.SetPositionAndRotation(spawn, Quaternion.Euler(spawnRotation));
        rig.transform.localScale = Vector3.one;

        Transform cameraPivot = FindChild(rig.transform, "CameraPivot");
        if (cameraPivot != null)
        {
            cameraPivot.localPosition = new Vector3(0f, 1.60f, 0f);
            cameraPivot.localRotation = Quaternion.identity;
            cameraPivot.localScale = Vector3.one;
        }

        Camera desktopCamera = rig.GetComponentsInChildren<Camera>(true).FirstOrDefault();
        if (desktopCamera != null)
        {
            desktopCamera.transform.localPosition = Vector3.zero;
            desktopCamera.transform.localRotation = Quaternion.identity;
            desktopCamera.transform.localScale = Vector3.one;
        }

        MonoBehaviour controller = rig
            .GetComponents<MonoBehaviour>()
            .FirstOrDefault(component =>
                component != null &&
                component.GetType().Name == "NeuroPhobiaDesktopController");

        if (controller == null)
        {
            EditorUtility.DisplayDialog(
                "NeuroPhobia",
                "No encontré NeuroPhobiaDesktopController en el jugador.",
                "Aceptar");
            if (character != null) character.enabled = true;
            return;
        }

        SerializedObject serializedController = new SerializedObject(controller);
        SerializedProperty spawnPosition = serializedController.FindProperty("spawnPosition");
        SerializedProperty spawnRotationProperty = serializedController.FindProperty("spawnRotation");

        if (spawnPosition == null || spawnRotationProperty == null)
        {
            EditorUtility.DisplayDialog(
                "NeuroPhobia",
                "El controlador no contiene los campos spawnPosition y spawnRotation.",
                "Aceptar");
            if (character != null) character.enabled = true;
            return;
        }

        spawnPosition.vector3Value = spawn;
        spawnRotationProperty.vector3Value = spawnRotation;
        serializedController.ApplyModifiedPropertiesWithoutUndo();

        if (character != null)
        {
            character.enabled = true;
        }

        EditorUtility.SetDirty(rig);
        EditorUtility.SetDirty(controller);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Selection.activeGameObject = rig;
        SceneView.lastActiveSceneView.FrameSelected();

        EditorUtility.DisplayDialog(
            "NeuroPhobia",
            "Inicio actualizado desde la vista de Scene.\n\n" +
            $"Posición: X {spawn.x:F2}, Y {spawn.y:F2}, Z {spawn.z:F2}\n" +
            $"Rotación Y: {yaw:F1}\n\n" +
            "Ahora pulsa Play. La cámara aparecerá exactamente donde colocaste la vista.",
            "Aceptar");
    }

    private static GameObject FindInScene(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform result = FindChild(root.transform, name);
            if (result != null)
            {
                return result.gameObject;
            }
        }

        return null;
    }

    private static Transform FindChild(Transform root, string name)
    {
        if (root.name == name)
        {
            return root;
        }

        foreach (Transform child in root)
        {
            Transform result = FindChild(child, name);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
}
#endif
