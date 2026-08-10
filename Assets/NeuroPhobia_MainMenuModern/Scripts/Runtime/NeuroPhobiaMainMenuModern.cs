using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Menú principal moderno de NeuroPhobia.
/// Se construye por código para mantener una apariencia consistente
/// tanto en computador como en Android/Meta Quest.
/// </summary>
[DisallowMultipleComponent]
public sealed class NeuroPhobiaMainMenuModern : MonoBehaviour
{
    private const string UiRootName = "NP_UI_Menu_Moderno";

    private GameObject uiRoot;
    private GameObject mainPanel;
    private GameObject levelsPanel;
    private GameObject instructionsPanel;
    private TMP_Text feedbackText;

    private static readonly Color Background =
        new Color(0.025f, 0.035f, 0.055f, 1f);

    private static readonly Color Panel =
        new Color(0.055f, 0.075f, 0.105f, 0.96f);

    private static readonly Color PanelSoft =
        new Color(0.075f, 0.10f, 0.135f, 0.96f);

    private static readonly Color Accent =
        new Color(0.11f, 0.78f, 0.72f, 1f);

    private static readonly Color AccentBlue =
        new Color(0.18f, 0.48f, 0.95f, 1f);

    private static readonly Color Muted =
        new Color(0.68f, 0.75f, 0.82f, 1f);

    private void Start()
    {
        RebuildMenu();
    }

    public void RebuildMenu()
    {
        RemovePreviousUi();
        EnsureEventSystem();

        uiRoot = new GameObject(UiRootName, typeof(RectTransform));
        uiRoot.transform.SetParent(transform, false);

        Canvas canvas = uiRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 30000;

        CanvasScaler scaler = uiRoot.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1440f, 900f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        uiRoot.AddComponent<GraphicRaycaster>();

        BuildBackground(uiRoot.transform);
        BuildMainPanel(uiRoot.transform);
        BuildLevelsPanel(uiRoot.transform);
        BuildInstructionsPanel(uiRoot.transform);

        ShowMain();
    }

    private void BuildBackground(Transform parent)
    {
        GameObject bg = CreateUIObject(parent, "Fondo");
        Stretch(bg.GetComponent<RectTransform>());

        Image image = bg.AddComponent<Image>();
        image.color = Background;

        CreateDecorativeBlock(
            bg.transform,
            "GlowSuperior",
            new Vector2(0.00f, 0.77f),
            new Vector2(0.48f, 1.00f),
            new Color(0.06f, 0.35f, 0.42f, 0.22f));

        CreateDecorativeBlock(
            bg.transform,
            "GlowDerecho",
            new Vector2(0.72f, 0.00f),
            new Vector2(1.00f, 0.52f),
            new Color(0.20f, 0.20f, 0.55f, 0.18f));

        CreateDecorativeBlock(
            bg.transform,
            "LineaSuperior",
            new Vector2(0.00f, 0.985f),
            new Vector2(1.00f, 1.00f),
            Accent);

        TMP_Text smallBrand = CreateText(
            bg.transform,
            "MarcaSuperior",
            "NEUROPHOBIA · EXPERIENCIA TERAPÉUTICA INMERSIVA",
            17f,
            FontStyles.Bold,
            TextAlignmentOptions.Left,
            Muted);

        SetAnchors(
            smallBrand.rectTransform,
            new Vector2(0.055f, 0.91f),
            new Vector2(0.70f, 0.965f));

        TMP_Text footer = CreateText(
            bg.transform,
            "Pie",
            "5 niveles de exposición gradual   •   Computador y Realidad Virtual",
            16f,
            FontStyles.Normal,
            TextAlignmentOptions.Left,
            new Color(0.52f, 0.60f, 0.69f, 1f));

        SetAnchors(
            footer.rectTransform,
            new Vector2(0.055f, 0.025f),
            new Vector2(0.75f, 0.075f));
    }

    private void BuildMainPanel(Transform parent)
    {
        mainPanel = CreateUIObject(parent, "PanelPrincipal");
        Stretch(mainPanel.GetComponent<RectTransform>());

        GameObject left = CreateUIObject(mainPanel.transform, "Presentacion");
        RectTransform leftRect = left.GetComponent<RectTransform>();
        SetAnchors(
            leftRect,
            new Vector2(0.055f, 0.13f),
            new Vector2(0.59f, 0.86f));

        TMP_Text title = CreateText(
            left.transform,
            "Titulo",
            "NEURO\nPHOBIA",
            78f,
            FontStyles.Bold,
            TextAlignmentOptions.Left,
            Color.white);

        SetAnchors(
            title.rectTransform,
            new Vector2(0.00f, 0.55f),
            new Vector2(0.90f, 0.93f));

        TMP_Text accent = CreateText(
            left.transform,
            "Acento",
            "ENTRENAMIENTO GRADUAL CONTRA LA ARACNOFOBIA",
            19f,
            FontStyles.Bold,
            TextAlignmentOptions.Left,
            Accent);

        SetAnchors(
            accent.rectTransform,
            new Vector2(0.00f, 0.48f),
            new Vector2(0.94f, 0.57f));

        TMP_Text description = CreateText(
            left.transform,
            "Descripcion",
            "Avanza a tu ritmo mediante una experiencia progresiva que combina observación, distancia segura, aproximación y contacto virtual controlado.",
            25f,
            FontStyles.Normal,
            TextAlignmentOptions.Left,
            new Color(0.84f, 0.88f, 0.92f, 1f));

        description.textWrappingMode = TextWrappingModes.Normal;

        SetAnchors(
            description.rectTransform,
            new Vector2(0.00f, 0.26f),
            new Vector2(0.92f, 0.48f));

        GameObject routeCard = CreateCard(
            left.transform,
            "Ruta",
            new Vector2(0.00f, 0.00f),
            new Vector2(0.92f, 0.22f),
            PanelSoft);

        TMP_Text routeTitle = CreateText(
            routeCard.transform,
            "RutaTitulo",
            "TU RECORRIDO",
            16f,
            FontStyles.Bold,
            TextAlignmentOptions.Left,
            Accent);

        SetAnchors(
            routeTitle.rectTransform,
            new Vector2(0.05f, 0.68f),
            new Vector2(0.95f, 0.92f));

        TMP_Text route = CreateText(
            routeCard.transform,
            "RutaTexto",
            "01  Psicoeducación     02  Distancia segura     03  Aproximación\n04  Contacto virtual     05  Desafío final",
            17f,
            FontStyles.Normal,
            TextAlignmentOptions.Left,
            Color.white);

        SetAnchors(
            route.rectTransform,
            new Vector2(0.05f, 0.13f),
            new Vector2(0.95f, 0.68f));

        GameObject actionCard = CreateCard(
            mainPanel.transform,
            "Acciones",
            new Vector2(0.64f, 0.16f),
            new Vector2(0.945f, 0.84f),
            Panel);

        TMP_Text ready = CreateText(
            actionCard.transform,
            "Listo",
            "¿LISTO PARA COMENZAR?",
            26f,
            FontStyles.Bold,
            TextAlignmentOptions.Center,
            Color.white);

        SetAnchors(
            ready.rectTransform,
            new Vector2(0.08f, 0.82f),
            new Vector2(0.92f, 0.95f));

        TMP_Text helper = CreateText(
            actionCard.transform,
            "Ayuda",
            "Puedes iniciar desde el Nivel 1 o elegir un nivel específico.",
            17f,
            FontStyles.Normal,
            TextAlignmentOptions.Center,
            Muted);

        helper.textWrappingMode = TextWrappingModes.Normal;

        SetAnchors(
            helper.rectTransform,
            new Vector2(0.10f, 0.70f),
            new Vector2(0.90f, 0.82f));

        CreateMenuButton(
            actionCard.transform,
            "Iniciar",
            "INICIAR EXPERIENCIA",
            new Vector2(0.10f, 0.54f),
            new Vector2(0.90f, 0.67f),
            Accent,
            Color.black,
            StartExperience);

        CreateMenuButton(
            actionCard.transform,
            "ElegirNivel",
            "ELEGIR NIVEL",
            new Vector2(0.10f, 0.38f),
            new Vector2(0.90f, 0.51f),
            AccentBlue,
            Color.white,
            ShowLevels);

        CreateMenuButton(
            actionCard.transform,
            "Instrucciones",
            "CONTROLES E INSTRUCCIONES",
            new Vector2(0.10f, 0.22f),
            new Vector2(0.90f, 0.35f),
            new Color(0.13f, 0.16f, 0.21f, 1f),
            Color.white,
            ShowInstructions);

        CreateMenuButton(
            actionCard.transform,
            "Salir",
            "SALIR",
            new Vector2(0.10f, 0.08f),
            new Vector2(0.90f, 0.19f),
            new Color(0.17f, 0.10f, 0.13f, 1f),
            new Color(0.95f, 0.70f, 0.74f, 1f),
            QuitGame);

        feedbackText = CreateText(
            actionCard.transform,
            "Estado",
            "",
            15f,
            FontStyles.Normal,
            TextAlignmentOptions.Center,
            new Color(0.95f, 0.72f, 0.40f, 1f));

        SetAnchors(
            feedbackText.rectTransform,
            new Vector2(0.08f, 0.005f),
            new Vector2(0.92f, 0.075f));
    }

    private void BuildLevelsPanel(Transform parent)
    {
        levelsPanel = CreateUIObject(parent, "PanelNiveles");
        Stretch(levelsPanel.GetComponent<RectTransform>());

        GameObject shade = CreateUIObject(levelsPanel.transform, "Sombra");
        Stretch(shade.GetComponent<RectTransform>());
        Image shadeImage = shade.AddComponent<Image>();
        shadeImage.color = new Color(0f, 0f, 0f, 0.80f);

        GameObject card = CreateCard(
            shade.transform,
            "TarjetaNiveles",
            new Vector2(0.18f, 0.13f),
            new Vector2(0.82f, 0.87f),
            new Color(0.045f, 0.06f, 0.085f, 0.995f));

        TMP_Text title = CreateText(
            card.transform,
            "Titulo",
            "SELECCIONA UN NIVEL",
            36f,
            FontStyles.Bold,
            TextAlignmentOptions.Center,
            Color.white);

        SetAnchors(
            title.rectTransform,
            new Vector2(0.08f, 0.84f),
            new Vector2(0.92f, 0.95f));

        TMP_Text subtitle = CreateText(
            card.transform,
            "Subtitulo",
            "Cada nivel aumenta gradualmente el grado de exposición.",
            18f,
            FontStyles.Normal,
            TextAlignmentOptions.Center,
            Muted);

        SetAnchors(
            subtitle.rectTransform,
            new Vector2(0.08f, 0.77f),
            new Vector2(0.92f, 0.85f));

        CreateLevelButton(
            card.transform,
            1,
            "PSICOEDUCACIÓN",
            "Respiración y exposición mediante imágenes.",
            0.62f,
            () => LoadLevel("Nivel1"));

        CreateLevelButton(
            card.transform,
            2,
            "DISTANCIA SEGURA",
            "Observación continua de una araña a distancia.",
            0.49f,
            () => LoadLevel("Nivel2"));

        CreateLevelButton(
            card.transform,
            3,
            "APROXIMACIÓN PROGRESIVA",
            "Acércate mediante puntos de exposición controlados.",
            0.36f,
            () => LoadLevel("Nivel3"));

        CreateLevelButton(
            card.transform,
            4,
            "CONTACTO VIRTUAL",
            "Interacción cercana en un entorno doméstico.",
            0.23f,
            () => LoadLevel("Nivel4"));

        CreateLevelButton(
            card.transform,
            5,
            "DESAFÍO FINAL",
            "Integra lo aprendido en un escenario más complejo.",
            0.10f,
            () => LoadLevel("Nivel5"));

        CreateMenuButton(
            card.transform,
            "Volver",
            "VOLVER",
            new Vector2(0.34f, 0.015f),
            new Vector2(0.66f, 0.085f),
            new Color(0.12f, 0.15f, 0.20f, 1f),
            Color.white,
            ShowMain);
    }

    private void BuildInstructionsPanel(Transform parent)
    {
        instructionsPanel = CreateUIObject(parent, "PanelInstrucciones");
        Stretch(instructionsPanel.GetComponent<RectTransform>());

        GameObject shade = CreateUIObject(instructionsPanel.transform, "Sombra");
        Stretch(shade.GetComponent<RectTransform>());
        Image shadeImage = shade.AddComponent<Image>();
        shadeImage.color = new Color(0f, 0f, 0f, 0.80f);

        GameObject card = CreateCard(
            shade.transform,
            "TarjetaInstrucciones",
            new Vector2(0.20f, 0.14f),
            new Vector2(0.80f, 0.86f),
            new Color(0.045f, 0.06f, 0.085f, 0.995f));

        TMP_Text title = CreateText(
            card.transform,
            "Titulo",
            "CONTROLES E INSTRUCCIONES",
            34f,
            FontStyles.Bold,
            TextAlignmentOptions.Center,
            Color.white);

        SetAnchors(
            title.rectTransform,
            new Vector2(0.06f, 0.84f),
            new Vector2(0.94f, 0.95f));

        GameObject pcCard = CreateCard(
            card.transform,
            "Computador",
            new Vector2(0.07f, 0.42f),
            new Vector2(0.48f, 0.78f),
            PanelSoft);

        TMP_Text pcTitle = CreateText(
            pcCard.transform,
            "TituloPC",
            "COMPUTADOR",
            23f,
            FontStyles.Bold,
            TextAlignmentOptions.Left,
            Accent);

        SetAnchors(
            pcTitle.rectTransform,
            new Vector2(0.07f, 0.74f),
            new Vector2(0.93f, 0.92f));

        TMP_Text pcText = CreateText(
            pcCard.transform,
            "TextoPC",
            "W A S D  ·  Movimiento\nMouse  ·  Mirar\nShift  ·  Correr\nEscape  ·  Liberar cursor",
            20f,
            FontStyles.Normal,
            TextAlignmentOptions.Left,
            Color.white);

        SetAnchors(
            pcText.rectTransform,
            new Vector2(0.07f, 0.12f),
            new Vector2(0.93f, 0.72f));

        GameObject vrCard = CreateCard(
            card.transform,
            "RV",
            new Vector2(0.52f, 0.42f),
            new Vector2(0.93f, 0.78f),
            PanelSoft);

        TMP_Text vrTitle = CreateText(
            vrCard.transform,
            "TituloRV",
            "REALIDAD VIRTUAL",
            23f,
            FontStyles.Bold,
            TextAlignmentOptions.Left,
            AccentBlue);

        SetAnchors(
            vrTitle.rectTransform,
            new Vector2(0.07f, 0.74f),
            new Vector2(0.93f, 0.92f));

        TMP_Text vrText = CreateText(
            vrCard.transform,
            "TextoRV",
            "Usa el visor y los controladores.\nAntes de cada nivel podrás escoger entre Computador y RV.\nMantén movimientos cómodos y controlados.",
            19f,
            FontStyles.Normal,
            TextAlignmentOptions.Left,
            Color.white);

        vrText.textWrappingMode = TextWrappingModes.Normal;

        SetAnchors(
            vrText.rectTransform,
            new Vector2(0.07f, 0.12f),
            new Vector2(0.93f, 0.72f));

        TMP_Text note = CreateText(
            card.transform,
            "Nota",
            "La exposición es gradual. Puedes detenerte cuando lo necesites y continuar cuando estés preparado.",
            19f,
            FontStyles.Normal,
            TextAlignmentOptions.Center,
            Muted);

        note.textWrappingMode = TextWrappingModes.Normal;

        SetAnchors(
            note.rectTransform,
            new Vector2(0.10f, 0.20f),
            new Vector2(0.90f, 0.37f));

        CreateMenuButton(
            card.transform,
            "Volver",
            "VOLVER AL MENÚ",
            new Vector2(0.32f, 0.055f),
            new Vector2(0.68f, 0.15f),
            Accent,
            Color.black,
            ShowMain);
    }

    private void CreateLevelButton(
        Transform parent,
        int number,
        string title,
        string description,
        float anchorY,
        UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = CreateUIObject(
            parent,
            "Nivel_" + number);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        SetAnchors(
            rect,
            new Vector2(0.08f, anchorY),
            new Vector2(0.92f, anchorY + 0.105f));

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.075f, 0.10f, 0.135f, 1f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);

        ColorBlock colors = button.colors;
        colors.normalColor = image.color;
        colors.highlightedColor = new Color(0.10f, 0.18f, 0.22f, 1f);
        colors.pressedColor = new Color(0.07f, 0.45f, 0.42f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;

        TMP_Text numberText = CreateText(
            buttonObject.transform,
            "Numero",
            number.ToString("00"),
            25f,
            FontStyles.Bold,
            TextAlignmentOptions.Center,
            Accent);

        SetAnchors(
            numberText.rectTransform,
            new Vector2(0.02f, 0.10f),
            new Vector2(0.13f, 0.90f));

        TMP_Text titleText = CreateText(
            buttonObject.transform,
            "Titulo",
            title,
            18f,
            FontStyles.Bold,
            TextAlignmentOptions.Left,
            Color.white);

        SetAnchors(
            titleText.rectTransform,
            new Vector2(0.15f, 0.48f),
            new Vector2(0.95f, 0.88f));

        TMP_Text descriptionText = CreateText(
            buttonObject.transform,
            "Descripcion",
            description,
            14f,
            FontStyles.Normal,
            TextAlignmentOptions.Left,
            Muted);

        SetAnchors(
            descriptionText.rectTransform,
            new Vector2(0.15f, 0.10f),
            new Vector2(0.95f, 0.50f));
    }

    private GameObject CreateCard(
        Transform parent,
        string name,
        Vector2 min,
        Vector2 max,
        Color color)
    {
        GameObject card = CreateUIObject(parent, name);

        RectTransform rect = card.GetComponent<RectTransform>();
        SetAnchors(rect, min, max);

        Image image = card.AddComponent<Image>();
        image.color = color;

        Outline outline = card.AddComponent<Outline>();
        outline.effectColor = new Color(0.20f, 0.36f, 0.44f, 0.35f);
        outline.effectDistance = new Vector2(1f, -1f);

        return card;
    }

    private void CreateMenuButton(
        Transform parent,
        string name,
        string label,
        Vector2 min,
        Vector2 max,
        Color background,
        Color foreground,
        UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = CreateUIObject(parent, name);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        SetAnchors(rect, min, max);

        Image image = buttonObject.AddComponent<Image>();
        image.color = background;

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);

        ColorBlock colors = button.colors;
        colors.normalColor = background;
        colors.highlightedColor = Color.Lerp(background, Color.white, 0.12f);
        colors.pressedColor = Color.Lerp(background, Color.black, 0.18f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;

        TMP_Text text = CreateText(
            buttonObject.transform,
            "Texto",
            label,
            19f,
            FontStyles.Bold,
            TextAlignmentOptions.Center,
            foreground);

        Stretch(text.rectTransform);
    }

    private void CreateDecorativeBlock(
        Transform parent,
        string name,
        Vector2 min,
        Vector2 max,
        Color color)
    {
        GameObject block = CreateUIObject(parent, name);
        SetAnchors(block.GetComponent<RectTransform>(), min, max);

        Image image = block.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
    }

    private static TMP_Text CreateText(
        Transform parent,
        string name,
        string value,
        float fontSize,
        FontStyles style,
        TextAlignmentOptions alignment,
        Color color)
    {
        GameObject go = CreateUIObject(parent, name);

        TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = color;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;

        return text;
    }

    private static GameObject CreateUIObject(
        Transform parent,
        string name)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void Stretch(RectTransform rect)
    {
        SetAnchors(rect, Vector2.zero, Vector2.one);
    }

    private static void SetAnchors(
        RectTransform rect,
        Vector2 min,
        Vector2 max)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void RemovePreviousUi()
    {
        Transform old = transform.Find(UiRootName);

        if (old == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(old.gameObject);
        }
        else
        {
            DestroyImmediate(old.gameObject);
        }
    }

    private static void EnsureEventSystem()
    {
        EventSystem current = FindFirstObjectByType<EventSystem>();

        if (current != null)
        {
            current.gameObject.SetActive(true);
            return;
        }

        GameObject eventObject = new GameObject(
            "EventSystem",
            typeof(EventSystem),
            typeof(StandaloneInputModule));

        eventObject.transform.position = Vector3.zero;
    }

    public void ShowMain()
    {
        if (mainPanel != null)
        {
            mainPanel.SetActive(true);
        }

        if (levelsPanel != null)
        {
            levelsPanel.SetActive(false);
        }

        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(false);
        }

        if (feedbackText != null)
        {
            feedbackText.text = "";
        }
    }

    public void ShowLevels()
    {
        if (mainPanel != null)
        {
            mainPanel.SetActive(false);
        }

        if (levelsPanel != null)
        {
            levelsPanel.SetActive(true);
        }

        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(false);
        }
    }

    public void ShowInstructions()
    {
        if (mainPanel != null)
        {
            mainPanel.SetActive(false);
        }

        if (levelsPanel != null)
        {
            levelsPanel.SetActive(false);
        }

        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(true);
        }
    }

    public void StartExperience()
    {
        LoadLevel("Nivel1");
    }

    public void LoadLevel(string sceneName)
    {
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            if (feedbackText != null)
            {
                feedbackText.text =
                    "No se puede cargar " +
                    sceneName +
                    ". Verifica Build Settings.";
            }

            Debug.LogError(
                "NeuroPhobia MainMenu: la escena " +
                sceneName +
                " no está disponible en Build Settings.");

            return;
        }

        SceneManager.LoadSceneAsync(sceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        if (feedbackText != null)
        {
            feedbackText.text =
                "En una compilación, este botón cerrará la aplicación.";
        }
#else
        Application.Quit();
#endif
    }
}
