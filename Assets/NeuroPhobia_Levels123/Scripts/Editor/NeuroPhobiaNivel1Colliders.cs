#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class NeuroPhobiaNivel1Colliders
{
    private const string EnvironmentRootName = "NP_ENV_NIVEL1_CONSULTORIO";
    private const string GeneratedRootName = "NP_COLISIONES_NIVEL1_GENERADAS";

    [MenuItem("Tools/NeuroPhobia/Agregar colisiones al consultorio Nivel 1")]
    public static void AddConsultingRoomColliders()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog(
                "NeuroPhobia",
                "Detén el modo Play antes de ejecutar este comando.",
                "Aceptar");
            return;
        }

        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            EditorUtility.DisplayDialog(
                "NeuroPhobia",
                "No hay una escena abierta.",
                "Aceptar");
            return;
        }

        GameObject environment = FindInScene(scene, EnvironmentRootName);
        if (environment == null)
        {
            environment = FindContaining(scene, "CONSULTORIO");
        }

        if (environment == null)
        {
            EditorUtility.DisplayDialog(
                "NeuroPhobia",
                "No encontré NP_ENV_NIVEL1_CONSULTORIO.\nAbre la escena Nivel1 e inténtalo otra vez.",
                "Aceptar");
            return;
        }

        GameObject oldGenerated = FindInScene(scene, GeneratedRootName);
        if (oldGenerated != null)
        {
            Undo.DestroyObjectImmediate(oldGenerated);
        }

        GameObject generatedRoot = new GameObject(GeneratedRootName);
        Undo.RegisterCreatedObjectUndo(generatedRoot, "Crear colisiones Nivel 1");
        SceneManager.MoveGameObjectToScene(generatedRoot, scene);

        int added = 0;
        int existing = 0;
        int skipped = 0;

        Renderer[] renderers = environment.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer rendererComponent in renderers)
        {
            if (rendererComponent == null)
            {
                continue;
            }

            GameObject target = rendererComponent.gameObject;

            if (ShouldSkip(target))
            {
                skipped++;
                continue;
            }

            Collider currentCollider = target.GetComponent<Collider>();
            if (currentCollider != null)
            {
                currentCollider.enabled = true;
                currentCollider.isTrigger = false;
                existing++;
                continue;
            }

            BoxCollider box = Undo.AddComponent<BoxCollider>(target);

            MeshFilter meshFilter = target.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                Bounds meshBounds = meshFilter.sharedMesh.bounds;
                box.center = meshBounds.center;
                box.size = EnsureMinimumThickness(meshBounds.size);
                box.isTrigger = false;
                added++;
                continue;
            }

            SkinnedMeshRenderer skinned = target.GetComponent<SkinnedMeshRenderer>();
            if (skinned != null)
            {
                Bounds localBounds = skinned.localBounds;
                box.center = localBounds.center;
                box.size = EnsureMinimumThickness(localBounds.size);
                box.isTrigger = false;
                added++;
                continue;
            }

            UnityEngine.Object.DestroyImmediate(box);
            skipped++;
        }

        GameObject roomModel = FindChildContaining(environment.transform, "Consultorio_Moderno")?.gameObject
                               ?? environment;

        Bounds roomBounds;
        if (TryGetBounds(roomModel, out roomBounds))
        {
            CreateBoundaryWalls(generatedRoot.transform, roomBounds);
        }

        GameObject normalizedFloor = FindInScene(scene, "NP_PisoCollider_Normalizado");
        if (normalizedFloor != null)
        {
            normalizedFloor.SetActive(true);

            BoxCollider floorCollider = normalizedFloor.GetComponent<BoxCollider>();
            if (floorCollider == null)
            {
                floorCollider = Undo.AddComponent<BoxCollider>(normalizedFloor);
            }

            floorCollider.enabled = true;
            floorCollider.isTrigger = false;

            MeshRenderer floorRenderer = normalizedFloor.GetComponent<MeshRenderer>();
            if (floorRenderer != null)
            {
                floorRenderer.enabled = false;
            }
        }

        GameObject player = FindInScene(scene, "NP_DesktopPlayer_Normalizado");
        if (player != null)
        {
            CharacterController character = player.GetComponent<CharacterController>();
            if (character != null)
            {
                character.detectCollisions = true;
                character.enableOverlapRecovery = true;
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject = generatedRoot;

        EditorUtility.DisplayDialog(
            "NeuroPhobia",
            "Colisiones agregadas correctamente.\n\n" +
            $"Nuevas: {added}\n" +
            $"Ya existentes: {existing}\n" +
            $"Omitidas: {skipped}\n\n" +
            "También se crearon cuatro límites invisibles alrededor del consultorio para impedir que el jugador salga por los lados.",
            "Aceptar");
    }

    private static bool ShouldSkip(GameObject target)
    {
        string name = target.name.ToLowerInvariant();

        string[] excludedTerms =
        {
            "piso",
            "floor",
            "ceiling",
            "techo",
            "light",
            "luz",
            "audio",
            "respiracion",
            "breathing",
            "canvas",
            "text",
            "texto",
            "card",
            "spider2d",
            "camera",
            "camara",
            "particle",
            "reflection"
        };

        if (excludedTerms.Any(name.Contains))
        {
            return true;
        }

        Renderer rendererComponent = target.GetComponent<Renderer>();
        if (rendererComponent == null)
        {
            return true;
        }

        Bounds bounds = rendererComponent.bounds;
        return bounds.size.sqrMagnitude < 0.0004f;
    }

    private static Vector3 EnsureMinimumThickness(Vector3 size)
    {
        const float minimum = 0.04f;
        return new Vector3(
            Mathf.Max(minimum, Mathf.Abs(size.x)),
            Mathf.Max(minimum, Mathf.Abs(size.y)),
            Mathf.Max(minimum, Mathf.Abs(size.z)));
    }

    private static void CreateBoundaryWalls(Transform parent, Bounds bounds)
    {
        const float thickness = 0.20f;
        float height = Mathf.Max(2.8f, bounds.size.y);
        float centerY = bounds.min.y + height * 0.5f;

        CreateInvisibleWall(
            "Limite_Izquierdo",
            parent,
            new Vector3(bounds.min.x - thickness * 0.5f, centerY, bounds.center.z),
            new Vector3(thickness, height, bounds.size.z + thickness * 2f));

        CreateInvisibleWall(
            "Limite_Derecho",
            parent,
            new Vector3(bounds.max.x + thickness * 0.5f, centerY, bounds.center.z),
            new Vector3(thickness, height, bounds.size.z + thickness * 2f));

        CreateInvisibleWall(
            "Limite_Frontal",
            parent,
            new Vector3(bounds.center.x, centerY, bounds.min.z - thickness * 0.5f),
            new Vector3(bounds.size.x + thickness * 2f, height, thickness));

        CreateInvisibleWall(
            "Limite_Trasero",
            parent,
            new Vector3(bounds.center.x, centerY, bounds.max.z + thickness * 0.5f),
            new Vector3(bounds.size.x + thickness * 2f, height, thickness));
    }

    private static void CreateInvisibleWall(
        string name,
        Transform parent,
        Vector3 worldPosition,
        Vector3 worldSize)
    {
        GameObject wall = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(wall, "Crear límite del consultorio");
        wall.transform.SetParent(parent, false);
        wall.transform.position = worldPosition;
        wall.transform.rotation = Quaternion.identity;
        wall.transform.localScale = Vector3.one;

        BoxCollider collider = wall.AddComponent<BoxCollider>();
        collider.center = Vector3.zero;
        collider.size = worldSize;
        collider.isTrigger = false;
    }

    private static bool TryGetBounds(GameObject root, out Bounds bounds)
    {
        bounds = new Bounds();

        if (root == null)
        {
            return false;
        }

        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true)
            .Where(rendererComponent => rendererComponent != null && !ShouldSkip(rendererComponent.gameObject))
            .ToArray();

        if (renderers.Length == 0)
        {
            return false;
        }

        bounds = renderers[0].bounds;
        for (int index = 1; index < renderers.Length; index++)
        {
            bounds.Encapsulate(renderers[index].bounds);
        }

        return true;
    }

    private static GameObject FindInScene(Scene scene, string exactName)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform result = FindExact(root.transform, exactName);
            if (result != null)
            {
                return result.gameObject;
            }
        }

        return null;
    }

    private static GameObject FindContaining(Scene scene, string containsText)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform result = FindChildContaining(root.transform, containsText);
            if (result != null)
            {
                return result.gameObject;
            }
        }

        return null;
    }

    private static Transform FindExact(Transform current, string exactName)
    {
        if (string.Equals(current.name, exactName, StringComparison.Ordinal))
        {
            return current;
        }

        foreach (Transform child in current)
        {
            Transform result = FindExact(child, exactName);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private static Transform FindChildContaining(Transform current, string containsText)
    {
        if (current.name.IndexOf(containsText, StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return current;
        }

        foreach (Transform child in current)
        {
            Transform result = FindChildContaining(child, containsText);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
}
#endif
