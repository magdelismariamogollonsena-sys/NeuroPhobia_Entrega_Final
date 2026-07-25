#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Level3SpiderDisappearanceFix
{
    private const string SpiderExactName =
        "NP_WolfSpider_Nivel3_Patrol";

    [MenuItem("Tools/NeuroPhobia/Corregir desaparicion arana Nivel 3")]
    public static void ApplyFix()
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

        GameObject spider = FindInScene(
            scene,
            SpiderExactName,
            "wolfspider_nivel3");

        if (spider == null)
        {
            EditorUtility.DisplayDialog(
                "NeuroPhobia",
                "No encontré la araña del Nivel 3.",
                "Aceptar");
            return;
        }

        spider.SetActive(true);
        Vector3 spiderPosition = spider.transform.position;
        Quaternion spiderRotation = spider.transform.rotation;

        Rigidbody body = spider.GetComponent<Rigidbody>();
        if (body != null)
        {
            body.isKinematic = true;
            body.useGravity = false;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }

        Animator animator =
            spider.GetComponentInChildren<Animator>(true);

        if (animator != null)
        {
            animator.applyRootMotion = false;
            animator.cullingMode =
                AnimatorCullingMode.AlwaysAnimate;
            animator.enabled = true;
        }

        foreach (Renderer rendererComponent in
                 spider.GetComponentsInChildren<Renderer>(true))
        {
            rendererComponent.enabled = true;
        }

        AranaController oldController =
            spider.GetComponent<AranaController>();

        if (oldController != null)
        {
            oldController.enabled = false;
        }

        Level3SpiderPatrolV2 patrol =
            spider.GetComponent<Level3SpiderPatrolV2>();

        if (patrol == null)
        {
            patrol =
                spider.AddComponent<Level3SpiderPatrolV2>();
        }

        Transform routeParent =
            spider.transform.parent != null
                ? spider.transform.parent
                : null;

        Transform pointA =
            FindOrCreatePoint(routeParent, "Patrol_A");

        Transform pointB =
            FindOrCreatePoint(routeParent, "Patrol_B");

        Vector3 routeAxis = spider.transform.right;
        routeAxis.y = 0f;

        if (routeAxis.sqrMagnitude < 0.001f)
        {
            routeAxis = Vector3.right;
        }

        routeAxis.Normalize();

        pointA.position =
            spiderPosition - routeAxis * 1.8f;

        pointB.position =
            spiderPosition + routeAxis * 1.8f;

        pointA.position = new Vector3(
            pointA.position.x,
            spiderPosition.y,
            pointA.position.z);

        pointB.position = new Vector3(
            pointB.position.x,
            spiderPosition.y,
            pointB.position.z);

        patrol.pointA = pointA;
        patrol.pointB = pointB;
        patrol.animator = animator;
        patrol.moveSpeed = 0.13f;
        patrol.animatorSpeed = 0.28f;
        patrol.turnSpeed = 4.5f;
        patrol.patrolHalfDistance = 1.8f;
        patrol.maxRouteDistanceFromSpawn = 6f;
        patrol.maxVerticalDeviation = 1.2f;
        patrol.enabled = true;

        GameObject guardObject = FindInScene(
            scene,
            "NP_Guardia_Arana_Nivel3",
            "Guardia_Arana_Nivel3");

        if (guardObject == null)
        {
            guardObject =
                new GameObject("NP_Guardia_Arana_Nivel3");

            SceneManager.MoveGameObjectToScene(
                guardObject,
                scene);
        }

        Level3SpiderVisibilityGuard guard =
            guardObject.GetComponent<
                Level3SpiderVisibilityGuard>();

        if (guard == null)
        {
            guard =
                guardObject.AddComponent<
                    Level3SpiderVisibilityGuard>();
        }

        guard.Configure(
            spider,
            spiderPosition,
            spiderRotation);

        EditorUtility.SetDirty(spider);
        EditorUtility.SetDirty(patrol);
        EditorUtility.SetDirty(guard);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Selection.activeGameObject = spider;

        EditorUtility.DisplayDialog(
            "NeuroPhobia",
            "Corrección aplicada.\n\n" +
            "La ruta quedó centrada alrededor de la posición actual de la araña.\n" +
            "El raycast del suelo ya ignora los colliders de la propia araña.\n" +
            "Root Motion quedó desactivado.\n" +
            "También se añadió una guardia externa para impedir que desaparezca.",
            "Aceptar");
    }

    private static Transform FindOrCreatePoint(
        Transform parent,
        string name)
    {
        Transform existing = parent != null
            ? parent.Find(name)
            : null;

        if (existing != null)
        {
            return existing;
        }

        GameObject pointObject = new GameObject(name);
        Transform point = pointObject.transform;

        if (parent != null)
        {
            point.SetParent(parent, true);
        }
        else
        {
            SceneManager.MoveGameObjectToScene(
                pointObject,
                SceneManager.GetActiveScene());
        }

        return point;
    }

    private static GameObject FindInScene(
        Scene scene,
        string exactName,
        string containsName)
    {
        foreach (GameObject root in
                 scene.GetRootGameObjects())
        {
            Transform result = FindRecursive(
                root.transform,
                exactName,
                containsName);

            if (result != null)
            {
                return result.gameObject;
            }
        }

        return null;
    }

    private static Transform FindRecursive(
        Transform current,
        string exactName,
        string containsName)
    {
        if (string.Equals(
                current.name,
                exactName,
                StringComparison.Ordinal) ||
            current.name.IndexOf(
                containsName,
                StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return current;
        }

        foreach (Transform child in current)
        {
            Transform result = FindRecursive(
                child,
                exactName,
                containsName);

            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
}
#endif
