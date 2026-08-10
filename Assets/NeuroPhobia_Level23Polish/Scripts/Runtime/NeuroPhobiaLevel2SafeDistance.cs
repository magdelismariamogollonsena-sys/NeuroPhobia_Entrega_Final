using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Mantiene una distancia mínima entre el jugador y la araña del Nivel 2.
/// Funciona tanto con PC como con XR Origin.
/// </summary>
[DefaultExecutionOrder(-150)]
[DisallowMultipleComponent]
public sealed class NeuroPhobiaLevel2SafeDistance : MonoBehaviour
{
    [Header("Referencias")]
    public Transform spiderTarget;
    public GameObject desktopPlayer;
    public GameObject xrOrigin;

    [Header("Límite")]
    [Min(0.5f)]
    public float minimumDistance = 3.0f;

    [Tooltip("Margen extra para evitar vibración en el borde.")]
    [Min(0f)]
    public float boundaryPadding = 0.08f;

    private Canvas warningCanvas;
    private TMP_Text warningText;

    private void Start()
    {
        ResolveReferences();
        BuildWarningUI();
        SetWarning(false);
    }

    private void LateUpdate()
    {
        ResolveReferences();

        if (spiderTarget == null)
        {
            SetWarning(false);
            return;
        }

        Transform player = GetActivePlayer();

        if (player == null)
        {
            SetWarning(false);
            return;
        }

        Vector3 spiderPos = spiderTarget.position;
        Vector3 playerPos = player.position;

        Vector2 spiderXZ = new Vector2(spiderPos.x, spiderPos.z);
        Vector2 playerXZ = new Vector2(playerPos.x, playerPos.z);

        Vector2 delta = playerXZ - spiderXZ;
        float distance = delta.magnitude;

        if (distance >= minimumDistance)
        {
            SetWarning(false);
            return;
        }

        if (delta.sqrMagnitude < 0.0001f)
        {
            Vector3 fallback = -player.forward;
            delta = new Vector2(fallback.x, fallback.z);

            if (delta.sqrMagnitude < 0.0001f)
                delta = Vector2.up;
        }

        delta.Normalize();

        float safeRadius = minimumDistance + boundaryPadding;

        Vector3 corrected = playerPos;
        corrected.x = spiderPos.x + delta.x * safeRadius;
        corrected.z = spiderPos.z + delta.y * safeRadius;

        MovePlayerSafely(player.gameObject, corrected);
        SetWarning(true);
    }

    private void ResolveReferences()
    {
        if (desktopPlayer == null)
        {
            desktopPlayer = FindSceneObject("NP_DesktopPlayer_Normalizado");

            if (desktopPlayer == null)
                desktopPlayer = FindSceneObject("NP_DesktopPlayer_Niveles45");
        }

        if (xrOrigin == null)
        {
            xrOrigin = FindSceneObject("XR Origin (XR Rig)");

            if (xrOrigin == null)
                xrOrigin = FindSceneObject("XR Origin");
        }

        if (spiderTarget == null)
            spiderTarget = FindSpiderTarget();
    }

    private Transform GetActivePlayer()
    {
        if (desktopPlayer != null && desktopPlayer.activeInHierarchy)
            return desktopPlayer.transform;

        if (xrOrigin != null && xrOrigin.activeInHierarchy)
            return xrOrigin.transform;

        return null;
    }

    private void MovePlayerSafely(GameObject player, Vector3 corrected)
    {
        CharacterController controller = player.GetComponent<CharacterController>();

        if (controller != null)
        {
            bool wasEnabled = controller.enabled;
            controller.enabled = false;
            player.transform.position = corrected;
            controller.enabled = wasEnabled;
        }
        else
        {
            player.transform.position = corrected;
        }
    }

    private Transform FindSpiderTarget()
    {
        Renderer[] renderers =
            FindObjectsByType<Renderer>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

        Renderer best = null;
        int bestScore = int.MinValue;

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
                continue;

            string path = BuildHierarchyPath(renderer.transform).ToLowerInvariant();

            if (!path.Contains("spider") &&
                !path.Contains("wolf") &&
                !path.Contains("arana") &&
                !path.Contains("araña"))
            {
                continue;
            }

            int score = 0;

            if (path.Contains("nivel2")) score += 200;
            if (path.Contains("wolf")) score += 100;
            if (path.Contains("spider")) score += 80;
            if (renderer.gameObject.activeInHierarchy) score += 40;

            float size = renderer.bounds.size.magnitude;
            if (size > 0.02f && size < 20f) score += 20;

            if (score > bestScore)
            {
                bestScore = score;
                best = renderer;
            }
        }

        return best != null ? best.transform : null;
    }

    private static string BuildHierarchyPath(Transform current)
    {
        string path = current.name;
        Transform parent = current.parent;
        int guard = 0;

        while (parent != null && guard < 12)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
            guard++;
        }

        return path;
    }

    private void BuildWarningUI()
    {
        GameObject root = new GameObject("NP_Nivel2_Aviso_DistanciaSegura");
        root.transform.SetParent(transform, false);

        warningCanvas = root.AddComponent<Canvas>();
        warningCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        warningCanvas.overrideSorting = true;
        warningCanvas.sortingOrder = 32500;

        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);

        GameObject panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(root.transform, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.sizeDelta = new Vector2(610f, 72f);
        rect.anchoredPosition = new Vector2(0f, 35f);

        Image image = panel.GetComponent<Image>();
        image.color = new Color(0.16f, 0.035f, 0.045f, 0.94f);

        GameObject textObject = new GameObject("Texto", typeof(RectTransform));
        textObject.transform.SetParent(panel.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(18f, 8f);
        textRect.offsetMax = new Vector2(-18f, -8f);

        warningText = textObject.AddComponent<TextMeshProUGUI>();
        warningText.text = "LÍMITE DE DISTANCIA SEGURA · No te acerques más a la araña";
        warningText.fontSize = 22f;
        warningText.fontStyle = FontStyles.Bold;
        warningText.alignment = TextAlignmentOptions.Center;
        warningText.color = Color.white;
        warningText.textWrappingMode = TextWrappingModes.Normal;
        warningText.raycastTarget = false;
    }

    private void SetWarning(bool visible)
    {
        if (warningCanvas != null)
            warningCanvas.gameObject.SetActive(visible);
    }

    private static GameObject FindSceneObject(string exactName)
    {
        Scene scene = SceneManager.GetActiveScene();

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform found = FindRecursive(root.transform, exactName);

            if (found != null)
                return found.gameObject;
        }

        return null;
    }

    private static Transform FindRecursive(Transform current, string exactName)
    {
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
