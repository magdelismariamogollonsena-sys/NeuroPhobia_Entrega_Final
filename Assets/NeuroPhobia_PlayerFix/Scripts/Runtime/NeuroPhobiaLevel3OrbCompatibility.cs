using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Compatibilidad de orbes para Nivel 3.
/// No depende del Tag Player ni de OnTriggerEnter.
/// Detecta proximidad tanto del jugador PC como del XR Origin.
/// </summary>
[DefaultExecutionOrder(-500)]
public sealed class NeuroPhobiaLevel3OrbCompatibility : MonoBehaviour
{
    public GameObject desktopPlayer;
    public GameObject xrOrigin;
    public float collectRadius = 1.35f;

    private readonly List<GameObject> orbs =
        new List<GameObject>();

    private int currentIndex;
    private bool completed;
    private NeuroPhobiaControlModeSelector selector;

    private void Start()
    {
        selector =
            FindFirstObjectByType<NeuroPhobiaControlModeSelector>();

        ResolvePlayers();
        DiscoverOrbs();
        PrepareOrbs();
    }

    private void Update()
    {
        if (completed)
        {
            return;
        }

        if (selector != null &&
            selector.CurrentMode ==
                NeuroPhobiaControlModeSelector.ControlMode.None)
        {
            return;
        }

        if (orbs.Count < 2)
        {
            DiscoverOrbs();
            PrepareOrbs();

            if (orbs.Count < 2)
            {
                return;
            }
        }

        Transform playerPoint = GetActivePlayerPoint();
        if (playerPoint == null)
        {
            return;
        }

        GameObject orb = orbs[currentIndex];
        if (orb == null)
        {
            return;
        }

        if (!orb.activeSelf)
        {
            orb.SetActive(true);
        }

        Vector3 a = playerPoint.position;
        Vector3 b = orb.transform.position;

        // La altura del visor/cámara no debe impedir recoger un orbe en el suelo.
        a.y = 0f;
        b.y = 0f;

        if (Vector3.Distance(a, b) <= collectRadius)
        {
            CollectCurrentOrb();
        }
    }

    private void ResolvePlayers()
    {
        Scene scene = gameObject.scene;

        if (desktopPlayer == null)
        {
            desktopPlayer =
                FindSceneObject(
                    scene,
                    "NP_DesktopPlayer_Normalizado");
        }

        if (xrOrigin == null)
        {
            xrOrigin =
                FindSceneObject(
                    scene,
                    "XR Origin (XR Rig)");

            if (xrOrigin == null)
            {
                xrOrigin =
                    FindSceneObject(scene, "XR Origin");
            }
        }
    }

    private void DiscoverOrbs()
    {
        orbs.Clear();

        List<GameObject> candidates =
            new List<GameObject>();

        Scene scene = gameObject.scene;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform[] transforms =
                root.GetComponentsInChildren<Transform>(true);

            foreach (Transform t in transforms)
            {
                if (!LooksLikeOrb(t.name))
                {
                    continue;
                }

                Transform candidate = GetTopOrbAncestor(t);
                GameObject go = candidate.gameObject;

                if (go == gameObject ||
                    go.name.IndexOf(
                        "Compatibility",
                        StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    continue;
                }

                if (!candidates.Contains(go))
                {
                    candidates.Add(go);
                }
            }
        }

        candidates =
            candidates
                .OrderByDescending(ScoreOrbCandidate)
                .Take(2)
                .OrderBy(SequenceScore)
                .ToList();

        orbs.AddRange(candidates);
    }

    private static bool LooksLikeOrb(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        string lower = name.ToLowerInvariant();

        if (lower.Contains("ui") ||
            lower.Contains("texto") ||
            lower.Contains("canvas") ||
            lower.Contains("guardia") ||
            lower.Contains("compat"))
        {
            return false;
        }

        return lower.Contains("orbe") ||
               lower.Contains("orb") ||
               lower.Contains("checkpoint");
    }

    private static Transform GetTopOrbAncestor(Transform t)
    {
        Transform result = t;

        while (result.parent != null &&
               LooksLikeOrb(result.parent.name))
        {
            result = result.parent;
        }

        return result;
    }

    private static int ScoreOrbCandidate(GameObject go)
    {
        string lower = go.name.ToLowerInvariant();
        int score = 0;

        if (lower.Contains("orbe") || lower.Contains("orb"))
        {
            score += 100;
        }

        if (go.GetComponentInChildren<Renderer>(true) != null)
        {
            score += 40;
        }

        if (go.GetComponentInChildren<Collider>(true) != null)
        {
            score += 20;
        }

        if (go.activeSelf)
        {
            score += 10;
        }

        return score;
    }

    private static int SequenceScore(GameObject go)
    {
        string lower = go.name.ToLowerInvariant();

        if (lower.Contains("azul") ||
            lower.Contains("_1") ||
            lower.Contains(" 1") ||
            lower.Contains("3m"))
        {
            return 0;
        }

        if (lower.Contains("naranja") ||
            lower.Contains("_2") ||
            lower.Contains(" 2") ||
            lower.Contains("1m"))
        {
            return 1;
        }

        return 2;
    }

    private void PrepareOrbs()
    {
        if (orbs.Count < 2)
        {
            return;
        }

        currentIndex = Mathf.Clamp(currentIndex, 0, 1);

        for (int i = 0; i < orbs.Count; i++)
        {
            GameObject orb = orbs[i];
            if (orb == null)
            {
                continue;
            }

            Collider[] colliders =
                orb.GetComponentsInChildren<Collider>(true);

            if (colliders.Length == 0)
            {
                SphereCollider sphere =
                    orb.AddComponent<SphereCollider>();

                sphere.radius = 0.65f;
                sphere.isTrigger = true;
            }
            else
            {
                foreach (Collider col in colliders)
                {
                    col.isTrigger = true;
                }
            }

            orb.SetActive(i == currentIndex);
        }
    }

    private void CollectCurrentOrb()
    {
        if (currentIndex < 0 ||
            currentIndex >= orbs.Count)
        {
            return;
        }

        GameObject current = orbs[currentIndex];

        if (current != null)
        {
            current.SetActive(false);
        }

        if (currentIndex == 0)
        {
            currentIndex = 1;

            if (orbs[1] != null)
            {
                orbs[1].SetActive(true);
            }

            TryReduceSpiderSpeed();
            return;
        }

        completed = true;
        StartCoroutine(CompleteLevel3());
    }

    private void TryReduceSpiderSpeed()
    {
        MonoBehaviour[] behaviours =
            FindObjectsByType<MonoBehaviour>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour == null)
            {
                continue;
            }

            var method =
                behaviour.GetType().GetMethod(
                    "SetReducedSpeed",
                    new Type[] { typeof(bool) });

            if (method != null)
            {
                try
                {
                    method.Invoke(
                        behaviour,
                        new object[] { true });
                }
                catch
                {
                }
            }
        }
    }

    private IEnumerator CompleteLevel3()
    {
        Time.timeScale = 0f;
        ShowCompletionUI();

        float end =
            Time.realtimeSinceStartup + 5f;

        while (Time.realtimeSinceStartup < end)
        {
            yield return null;
        }

        Time.timeScale = 1f;

        if (Application.CanStreamedLevelBeLoaded("Nivel4"))
        {
            SceneManager.LoadSceneAsync("Nivel4");
        }
        else
        {
            Debug.LogWarning(
                "NeuroPhobia: Nivel4 no está agregado al Build.");
        }
    }

    private void ShowCompletionUI()
    {
        GameObject root =
            new GameObject("NP_UI_Nivel3_Completado");

        Canvas canvas = root.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = 32760;

        Camera activeCamera = GetActiveCamera();

        if (Application.platform == RuntimePlatform.Android &&
            activeCamera != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = activeCamera;
            canvas.planeDistance = 0.45f;
        }
        else
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution =
            new Vector2(1280f, 720f);

        GameObject panel =
            new GameObject("Fondo", typeof(RectTransform));

        panel.transform.SetParent(root.transform, false);

        Image image = panel.AddComponent<Image>();
        image.color =
            new Color(0.025f, 0.035f, 0.05f, 0.98f);

        RectTransform rect =
            panel.GetComponent<RectTransform>();

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        GameObject textObject =
            new GameObject("Texto", typeof(RectTransform));

        textObject.transform.SetParent(panel.transform, false);

        TextMeshProUGUI text =
            textObject.AddComponent<TextMeshProUGUI>();

        text.text =
            "¡Felicitaciones!\n\n" +
            "Completaste el Nivel 3\n\n" +
            "El Nivel 4 comenzará en 5 segundos.";

        text.fontSize = 42f;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;

        RectTransform textRect =
            textObject.GetComponent<RectTransform>();

        textRect.anchorMin = new Vector2(0.12f, 0.20f);
        textRect.anchorMax = new Vector2(0.88f, 0.80f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }

    private Transform GetActivePlayerPoint()
    {
        ResolvePlayers();

        if (desktopPlayer != null &&
            desktopPlayer.activeInHierarchy)
        {
            Camera camera =
                desktopPlayer.GetComponentInChildren<Camera>(true);

            return camera != null
                ? camera.transform
                : desktopPlayer.transform;
        }

        if (xrOrigin != null &&
            xrOrigin.activeInHierarchy)
        {
            Camera camera =
                xrOrigin.GetComponentInChildren<Camera>(true);

            return camera != null
                ? camera.transform
                : xrOrigin.transform;
        }

        return null;
    }

    private Camera GetActiveCamera()
    {
        Transform point = GetActivePlayerPoint();

        if (point != null)
        {
            Camera camera = point.GetComponent<Camera>();
            if (camera != null)
            {
                return camera;
            }
        }

        return Camera.main;
    }

    private static GameObject FindSceneObject(
        Scene scene,
        string exactName)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform found =
                FindRecursive(root.transform, exactName);

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
        if (current.name == exactName)
        {
            return current;
        }

        foreach (Transform child in current)
        {
            Transform result =
                FindRecursive(child, exactName);

            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
}
