using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Management;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Pregunta al entrar a cada nivel si se jugará con computador o RV.
/// No recuerda la respuesta: pregunta en cada carga de escena.
/// </summary>
[DefaultExecutionOrder(-20000)]
[DisallowMultipleComponent]
public sealed class NeuroPhobiaControlModeSelector : MonoBehaviour
{
    public enum ControlMode
    {
        None,
        Computer,
        VirtualReality
    }

    [Header("Referencias")]
    public GameObject desktopPlayer;
    public GameObject xrOrigin;
    public Camera desktopCamera;

    [Header("Configuración")]
    public bool askEveryTimeSceneStarts = true;
    public float inputGuardSeconds = 0.45f;

    public ControlMode CurrentMode { get; private set; }

    private GameObject selectorRoot;
    private Canvas selectorCanvas;
    private RectTransform computerButtonRect;
    private RectTransform vrButtonRect;
    private TMP_Text statusText;
    private float previousTimeScale = 1f;
    private float shownAtRealtime;
    private bool selectionMade;
    private bool started;
    private bool previousXRPressed;
    private Camera activeGameplayCamera;
    private bool xrInitializationInProgress;
    private bool xrSubsystemsStarted;
    private bool computerRequestedDuringXRInitialization;

    private static readonly string[] DesktopNames =
    {
        "NP_DesktopPlayer_Normalizado",
        "NP_DesktopPlayer_Niveles45",
        "DesktopTestRig"
    };

    private static readonly string[] XRNames =
    {
        "XR Origin (XR Rig)",
        "XR Origin",
        "XROrigin"
    };

    private void Awake()
    {
        ResolveSceneObjects();
        xrSubsystemsStarted = AreXRSubsystemsRunning();
    }

    private void Start()
    {
        started = true;

        if (askEveryTimeSceneStarts)
        {
            ShowSelector();
        }
        else
        {
            SelectComputer();
        }
    }

    private void OnEnable()
    {
        if (Application.isPlaying && started && askEveryTimeSceneStarts)
        {
            ShowSelector();
        }
    }

    private void Update()
    {
        if (!selectionMade)
        {
            HandleSelectionInput();
            return;
        }

        if (ReadF1Pressed())
        {
            ShowSelector();
        }
    }

    public void ShowSelector()
    {
        ResolveSceneObjects();
        PrepareNeutralState();

        selectionMade = false;
        CurrentMode = ControlMode.None;
        shownAtRealtime = Time.realtimeSinceStartup;
        previousXRPressed = ReadXRSelectionPressed();

        if (selectorRoot == null)
        {
            BuildSelectorUI();
        }

        selectorRoot.SetActive(true);

        if (statusText != null)
        {
            statusText.text = "Selecciona el modo para comenzar.";
        }
    }

    public void SelectComputer()
    {
        if (selectionMade)
        {
            return;
        }

        if (xrInitializationInProgress)
        {
            computerRequestedDuringXRInitialization = true;
            SetStatus(
                "Cancelando la inicialización de realidad virtual...");
            return;
        }

        ResolveSceneObjects();
        StopAndDeinitializeXR();

        if (desktopPlayer == null)
        {
            SetStatus("No se encontró el jugador de computador.");
            return;
        }

        if (xrOrigin != null)
        {
            xrOrigin.SetActive(false);
        }

        DisableXRDeviceSimulator();

        desktopPlayer.SetActive(true);
        SetDesktopControllersEnabled(desktopPlayer, true);

        Camera camera =
            desktopPlayer.GetComponentInChildren<Camera>(true);

        if (camera == null)
        {
            SetStatus("El jugador de computador no tiene cámara.");
            return;
        }

        desktopCamera = camera;
        DisableCameras(desktopPlayer);
        camera.gameObject.SetActive(true);
        camera.enabled = true;
        EnsureMainCameraTag(camera);
        SetAudioListenerState(desktopPlayer, camera, true);

        activeGameplayCamera = camera;
        CurrentMode = ControlMode.Computer;
        FinishSelection();

        NeuroPhobiaDesktopController controller =
            desktopPlayer.GetComponent<NeuroPhobiaDesktopController>();

        if (controller != null)
        {
            controller.LockCursor();
        }
    }

    public void SelectVirtualReality()
    {
        if (selectionMade || xrInitializationInProgress)
        {
            return;
        }

        ResolveSceneObjects();

        if (xrOrigin == null)
        {
            SetStatus("No se encontró XR Origin en esta escena.");
            return;
        }

        StartCoroutine(InitializeAndSelectVirtualReality());
    }

    private IEnumerator InitializeAndSelectVirtualReality()
    {
        XRGeneralSettings settings = XRGeneralSettings.Instance;
        XRManagerSettings manager =
            settings != null ? settings.Manager : null;

        if (manager == null)
        {
            SetStatus(
                "No se encontró la configuración de realidad virtual. " +
                "Puedes continuar en modo COMPUTADOR.");
            yield break;
        }

        xrInitializationInProgress = true;
        computerRequestedDuringXRInitialization = false;
        SetStatus("Inicializando realidad virtual...");

        if (manager.activeLoader == null)
            yield return manager.InitializeLoader();

        xrInitializationInProgress = false;

        if (computerRequestedDuringXRInitialization)
        {
            computerRequestedDuringXRInitialization = false;
            SelectComputer();
            yield break;
        }

        if (manager.activeLoader == null)
        {
            SetStatus(
                "No se detectó un runtime o dispositivo de realidad " +
                "virtual. Puedes elegir COMPUTADOR.");
            yield break;
        }

        if (!xrSubsystemsStarted && !AreXRSubsystemsRunning())
        {
            manager.StartSubsystems();
            xrSubsystemsStarted = true;
        }
        else
        {
            xrSubsystemsStarted = true;
        }

        ResolveSceneObjects();
        Camera xrCamera =
            xrOrigin.GetComponentInChildren<Camera>(true);

        if (xrCamera == null)
        {
            StopAndDeinitializeXR();
            xrOrigin.SetActive(false);
            SetStatus(
                "XR Origin no contiene una cámara. " +
                "Puedes elegir COMPUTADOR.");
            yield break;
        }

        if (desktopPlayer != null)
        {
            SetDesktopControllersEnabled(desktopPlayer, false);
            SetAudioListenerState(desktopPlayer, null, false);
            DisableCameras(desktopPlayer);
        }

        DisableCameras(xrOrigin);
        xrOrigin.SetActive(true);
        xrCamera.gameObject.SetActive(true);
        xrCamera.enabled = true;
        EnsureMainCameraTag(xrCamera);
        SetAudioListenerState(xrOrigin, xrCamera, true);

        activeGameplayCamera = xrCamera;
        CurrentMode = ControlMode.VirtualReality;

        if (desktopPlayer != null)
            desktopPlayer.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;
        FinishSelection();
    }

    private void StopAndDeinitializeXR()
    {
        if (xrInitializationInProgress)
            return;

        XRGeneralSettings settings = XRGeneralSettings.Instance;
        XRManagerSettings manager =
            settings != null ? settings.Manager : null;

        if (manager == null || manager.activeLoader == null)
        {
            xrSubsystemsStarted = false;
            return;
        }

        manager.StopSubsystems();
        xrSubsystemsStarted = false;
        manager.DeinitializeLoader();
    }

    private static void DisableCameras(GameObject root)
    {
        if (root == null)
            return;

        foreach (Camera cameraComponent in
                 root.GetComponentsInChildren<Camera>(true))
        {
            cameraComponent.enabled = false;
        }
    }

    private static bool AreXRSubsystemsRunning()
    {
        List<XRDisplaySubsystem> displays =
            new List<XRDisplaySubsystem>();
        SubsystemManager.GetSubsystems(displays);

        foreach (XRDisplaySubsystem display in displays)
        {
            if (display != null && display.running)
                return true;
        }

        return false;
    }

    private void ResolveSceneObjects()
    {
        Scene scene = gameObject.scene;

        if (desktopPlayer == null)
        {
            desktopPlayer = FindBestDesktopPlayer(scene);
        }

        if (xrOrigin == null)
        {
            xrOrigin = FindBestXRRoot(scene);
        }

        if (desktopCamera == null && desktopPlayer != null)
        {
            desktopCamera =
                desktopPlayer.GetComponentInChildren<Camera>(true);
        }
    }

    private void PrepareNeutralState()
    {
        previousTimeScale =
            Time.timeScale > 0f ? Time.timeScale : 1f;

        Time.timeScale = 0f;

        if (desktopPlayer != null)
        {
            desktopPlayer.SetActive(true);
            SetDesktopControllersEnabled(desktopPlayer, false);

            Camera camera =
                desktopPlayer.GetComponentInChildren<Camera>(true);

            if (camera != null)
            {
                desktopCamera = camera;
                camera.gameObject.SetActive(true);
                camera.enabled = true;
                EnsureMainCameraTag(camera);
                SetAudioListenerState(desktopPlayer, camera, true);
            }
        }

        if (xrOrigin != null)
        {
            xrOrigin.SetActive(false);
        }

        DisableXRDeviceSimulator();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void FinishSelection()
    {
        selectionMade = true;

        if (selectorRoot != null)
        {
            selectorRoot.SetActive(false);
        }

        Time.timeScale =
            previousTimeScale > 0f ? previousTimeScale : 1f;
    }

    private void HandleSelectionInput()
    {
        if (Time.realtimeSinceStartup - shownAtRealtime <
            inputGuardSeconds)
        {
            previousXRPressed = ReadXRSelectionPressed();
            return;
        }

        if (ReadComputerShortcutPressed())
        {
            SelectComputer();
            return;
        }

        if (ReadVRShortcutPressed())
        {
            SelectVirtualReality();
            return;
        }

        bool xrPressed = ReadXRSelectionPressed();
        if (xrPressed && !previousXRPressed)
        {
            SelectVirtualReality();
            previousXRPressed = true;
            return;
        }

        previousXRPressed = xrPressed;

        Vector2 pointer;
        if (!ReadPointerClick(out pointer))
        {
            return;
        }

        Camera uiCamera =
            selectorCanvas != null &&
            selectorCanvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? selectorCanvas.worldCamera
                : null;

        if (computerButtonRect != null &&
            RectTransformUtility.RectangleContainsScreenPoint(
                computerButtonRect,
                pointer,
                uiCamera))
        {
            SelectComputer();
            return;
        }

        if (vrButtonRect != null &&
            RectTransformUtility.RectangleContainsScreenPoint(
                vrButtonRect,
                pointer,
                uiCamera))
        {
            SelectVirtualReality();
        }
    }

    private void BuildSelectorUI()
    {
        selectorRoot =
            new GameObject("NP_UI_Seleccion_PC_RV");

        selectorRoot.transform.SetParent(transform, false);

        selectorCanvas = selectorRoot.AddComponent<Canvas>();
        selectorCanvas.overrideSorting = true;
        selectorCanvas.sortingOrder = 32760;

        // En PC el Overlay es la forma más estable de garantizar que se vea.
        // En Android/Quest se usa la cámara activa para que el Canvas llegue al visor.
        if (Application.platform == RuntimePlatform.Android &&
            desktopCamera != null)
        {
            selectorCanvas.renderMode =
                RenderMode.ScreenSpaceCamera;
            selectorCanvas.worldCamera = desktopCamera;
            selectorCanvas.planeDistance = 0.45f;
        }
        else
        {
            selectorCanvas.renderMode =
                RenderMode.ScreenSpaceOverlay;
        }

        CanvasScaler scaler =
            selectorRoot.AddComponent<CanvasScaler>();

        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution =
            new Vector2(1280f, 720f);
        scaler.screenMatchMode =
            CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        selectorRoot.AddComponent<GraphicRaycaster>();

        GameObject background =
            CreateUIObject(selectorRoot.transform, "Fondo");

        Image backgroundImage =
            background.AddComponent<Image>();

        backgroundImage.color =
            new Color(0.025f, 0.035f, 0.05f, 0.97f);

        StretchFull(
            background.GetComponent<RectTransform>());

        GameObject card =
            CreateUIObject(background.transform, "Tarjeta");

        RectTransform cardRect =
            card.GetComponent<RectTransform>();

        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);
        cardRect.sizeDelta = new Vector2(820f, 520f);
        cardRect.anchoredPosition = Vector2.zero;

        Image cardImage = card.AddComponent<Image>();
        cardImage.color =
            new Color(0.08f, 0.10f, 0.14f, 0.99f);

        Outline outline = card.AddComponent<Outline>();
        outline.effectColor =
            new Color(0.22f, 0.72f, 0.85f, 0.8f);
        outline.effectDistance = new Vector2(2f, -2f);

        TMP_Text title =
            CreateText(
                card.transform,
                "Titulo",
                42f,
                FontStyles.Bold,
                TextAlignmentOptions.Center);

        title.text =
            GetLevelTitle() +
            "\n¿Cómo quieres jugar?";

        SetRect(
            title.rectTransform,
            new Vector2(0.08f, 0.67f),
            new Vector2(0.92f, 0.94f));

        TMP_Text subtitle =
            CreateText(
                card.transform,
                "Subtitulo",
                22f,
                FontStyles.Normal,
                TextAlignmentOptions.Center);

        subtitle.text =
            "Selecciona el modo de control antes de comenzar.";

        SetRect(
            subtitle.rectTransform,
            new Vector2(0.08f, 0.58f),
            new Vector2(0.92f, 0.69f));

        computerButtonRect =
            CreateChoiceButton(
                card.transform,
                "BotonComputador",
                "USAR COMPUTADOR",
                new Vector2(0.12f, 0.35f),
                new Vector2(0.88f, 0.52f),
                new Color(0.10f, 0.42f, 0.62f, 1f),
                SelectComputer);

        vrButtonRect =
            CreateChoiceButton(
                card.transform,
                "BotonRV",
                "USAR REALIDAD VIRTUAL",
                new Vector2(0.12f, 0.16f),
                new Vector2(0.88f, 0.33f),
                new Color(0.32f, 0.20f, 0.58f, 1f),
                SelectVirtualReality);

        statusText =
            CreateText(
                card.transform,
                "Estado",
                18f,
                FontStyles.Normal,
                TextAlignmentOptions.Center);

        statusText.text =
            "PC: clic o C   |   RV: clic, V o gatillo";

        SetRect(
            statusText.rectTransform,
            new Vector2(0.05f, 0.03f),
            new Vector2(0.95f, 0.14f));
    }

    private RectTransform CreateChoiceButton(
        Transform parent,
        string name,
        string label,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Color color,
        UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject =
            CreateUIObject(parent, name);

        RectTransform rect =
            buttonObject.GetComponent<RectTransform>();

        SetRect(rect, anchorMin, anchorMax);

        Image image = buttonObject.AddComponent<Image>();
        image.color = color;

        Button button = buttonObject.AddComponent<Button>();
        button.onClick.AddListener(action);

        TMP_Text text =
            CreateText(
                buttonObject.transform,
                "Texto",
                27f,
                FontStyles.Bold,
                TextAlignmentOptions.Center);

        text.text = label;
        StretchFull(text.rectTransform);

        return rect;
    }

    private static GameObject CreateUIObject(
        Transform parent,
        string name)
    {
        GameObject go =
            new GameObject(name, typeof(RectTransform));

        go.transform.SetParent(parent, false);
        return go;
    }

    private static TMP_Text CreateText(
        Transform parent,
        string name,
        float fontSize,
        FontStyles style,
        TextAlignmentOptions alignment)
    {
        GameObject go =
            CreateUIObject(parent, name);

        TextMeshProUGUI text =
            go.AddComponent<TextMeshProUGUI>();

        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = Color.white;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;

        return text;
    }

    private static void StretchFull(RectTransform rect)
    {
        SetRect(rect, Vector2.zero, Vector2.one);
    }

    private static void SetRect(
        RectTransform rect,
        Vector2 anchorMin,
        Vector2 anchorMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private string GetLevelTitle()
    {
        string sceneName =
            SceneManager.GetActiveScene().name;

        if (sceneName.StartsWith(
            "Nivel",
            StringComparison.OrdinalIgnoreCase))
        {
            return "NIVEL " + sceneName.Substring(5);
        }

        return sceneName.ToUpperInvariant();
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }

        Debug.LogWarning("NeuroPhobia PC/RV: " + message);
    }

    private static GameObject FindBestDesktopPlayer(Scene scene)
    {
        foreach (string exactName in DesktopNames)
        {
            GameObject exact = FindSceneObject(scene, exactName);
            if (exact != null && exactName != "DesktopTestRig")
            {
                return exact;
            }
        }

        return null;
    }

    private static GameObject FindBestXRRoot(Scene scene)
    {
        foreach (string exactName in XRNames)
        {
            GameObject exact = FindSceneObject(scene, exactName);
            if (exact != null)
            {
                return exact;
            }
        }

        return null;
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

    private static void DisableXRDeviceSimulator()
    {
        Scene scene = SceneManager.GetActiveScene();
        GameObject simulator =
            FindSceneObject(scene, "XR Device Simulator");

        if (simulator != null)
        {
            simulator.SetActive(false);
        }
    }

    private static void SetDesktopControllersEnabled(
        GameObject player,
        bool enabledState)
    {
        if (player == null)
        {
            return;
        }

        NeuroPhobiaDesktopController unified =
            player.GetComponent<NeuroPhobiaDesktopController>();

        if (unified != null)
        {
            unified.enabled = enabledState;
        }

        MonoBehaviour[] behaviours =
            player.GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour == null || behaviour == unified)
            {
                continue;
            }

            string typeName = behaviour.GetType().Name;

            if (typeName == "NPDesktopFallbackPlayer" ||
                typeName == "DesktopPlayerController" ||
                typeName == "DesktopController")
            {
                behaviour.enabled = false;
            }
        }
    }

    private static void SetAudioListenerState(
        GameObject root,
        Camera selectedCamera,
        bool enableSelected)
    {
        if (root == null)
        {
            return;
        }

        AudioListener[] listeners =
            root.GetComponentsInChildren<AudioListener>(true);

        foreach (AudioListener listener in listeners)
        {
            bool selected =
                selectedCamera != null &&
                listener.gameObject == selectedCamera.gameObject;

            listener.enabled = enableSelected && selected;
        }

        if (enableSelected &&
            selectedCamera != null &&
            selectedCamera.GetComponent<AudioListener>() == null)
        {
            selectedCamera.gameObject.AddComponent<AudioListener>();
        }
    }

    private static void EnsureMainCameraTag(Camera camera)
    {
        if (camera == null)
        {
            return;
        }

        try
        {
            camera.gameObject.tag = "MainCamera";
        }
        catch
        {
        }
    }

    private static bool ReadPointerClick(out Vector2 position)
    {
        position = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            position = Mouse.current.position.ReadValue();
            return true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetMouseButtonDown(0))
        {
            position = Input.mousePosition;
            return true;
        }
#endif

        return false;
    }

    private static bool ReadComputerShortcutPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null &&
            Keyboard.current.cKey.wasPressedThisFrame)
        {
            return true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.C);
#else
        return false;
#endif
    }

    private static bool ReadVRShortcutPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null &&
            Keyboard.current.vKey.wasPressedThisFrame)
        {
            return true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.V);
#else
        return false;
#endif
    }

    private static bool ReadF1Pressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null &&
            Keyboard.current.f1Key.wasPressedThisFrame)
        {
            return true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.F1);
#else
        return false;
#endif
    }

    private static bool ReadXRSelectionPressed()
    {
        List<UnityEngine.XR.InputDevice> devices =
            new List<UnityEngine.XR.InputDevice>();

        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);

        if (AnyXRSelectPressed(devices))
        {
            return true;
        }

        devices.Clear();
        InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, devices);
        return AnyXRSelectPressed(devices);
    }

    private static bool AnyXRSelectPressed(
        List<UnityEngine.XR.InputDevice> devices)
    {
        foreach (UnityEngine.XR.InputDevice device in devices)
        {
            bool pressed;

            if (device.TryGetFeatureValue(
                    UnityEngine.XR.CommonUsages.triggerButton,
                    out pressed) &&
                pressed)
            {
                return true;
            }

            if (device.TryGetFeatureValue(
                    UnityEngine.XR.CommonUsages.primaryButton,
                    out pressed) &&
                pressed)
            {
                return true;
            }
        }

        return false;
    }
}
