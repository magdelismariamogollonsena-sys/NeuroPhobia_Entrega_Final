#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NeuroPhobia.Level45;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace NeuroPhobia.Level45.Editor
{
    public static class NeuroPhobiaLevel45PolishFix
    {
        private const string RootPath =
            "Assets/NeuroPhobia_Level45";

        private const string Scene4Path =
            "Assets/Scenes/Nivel4.unity";

        private const string Scene5Path =
            "Assets/Scenes/Nivel5.unity";

        private const string GeneratedPath =
            RootPath + "/Generated";

        private const string MaterialsPath =
            GeneratedPath + "/Materials";

        private const string TexturePath =
            RootPath + "/Art/Polish/Textures/";

        private const string FactoryModelPath =
            RootPath + "/Art/Factory/Models/";

        [MenuItem(
            "Tools/NeuroPhobia/Corregir visual movimiento e interfaz Niveles 4 y 5",
            priority = 420)]
        public static void FixBoth()
        {
            if (!CanEdit())
            {
                return;
            }

            try
            {
                FixLevel4Internal();
                FixLevel5Internal();

                EditorSceneManager.OpenScene(
                    Scene5Path,
                    OpenSceneMode.Single);

                EditorUtility.DisplayDialog(
                    "NeuroPhobia",
                    "Corrección aplicada a los niveles 4 y 5.\n\n" +
                    "Se corrigió:\n" +
                    "• Interfaz nítida y más grande.\n" +
                    "• Movimiento con W A S D y flechas.\n" +
                    "• Cámara correcta para prueba en computador.\n" +
                    "• Paredes con paneles y materiales.\n" +
                    "• Techo industrial y cielo oscuro.\n" +
                    "• Luces, vigas, ductos y señalización.\n\n" +
                    "Nivel 5 quedó abierto para probar.",
                    "Aceptar");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);

                EditorUtility.DisplayDialog(
                    "NeuroPhobia",
                    "La corrección se detuvo.\n\n" +
                    exception.Message +
                    "\n\nEnvía el primer error rojo completo.",
                    "Aceptar");
            }
        }

        [MenuItem(
            "Tools/NeuroPhobia/Corregir Nivel 4 visual y movimiento",
            priority = 421)]
        public static void FixLevel4()
        {
            if (!CanEdit())
            {
                return;
            }

            FixLevel4Internal();

            EditorUtility.DisplayDialog(
                "NeuroPhobia",
                "Nivel 4 corregido y guardado.",
                "Aceptar");
        }

        [MenuItem(
            "Tools/NeuroPhobia/Corregir Nivel 5 visual y movimiento",
            priority = 422)]
        public static void FixLevel5()
        {
            if (!CanEdit())
            {
                return;
            }

            FixLevel5Internal();

            EditorUtility.DisplayDialog(
                "NeuroPhobia",
                "Nivel 5 corregido y guardado.",
                "Aceptar");
        }

        private static bool CanEdit()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog(
                    "NeuroPhobia",
                    "Detén Play antes de ejecutar la corrección.",
                    "Aceptar");

                return false;
            }

            return true;
        }

        private static void FixLevel4Internal()
        {
            if (!File.Exists(Scene4Path))
            {
                throw new FileNotFoundException(
                    "No existe Assets/Scenes/Nivel4.unity. " +
                    "Primero configura el Nivel 4.");
            }

            Scene scene =
                EditorSceneManager.OpenScene(
                    Scene4Path,
                    OpenSceneMode.Single);

            GameObject levelRoot =
                FindSceneObject(
                    scene,
                    "NP_LEVEL4_FINAL");

            if (levelRoot == null)
            {
                throw new InvalidOperationException(
                    "No encontré NP_LEVEL4_FINAL en Nivel4. " +
                    "Primero ejecuta Configurar Nivel 4 FINAL.");
            }

            Camera playerCamera =
                FindPreferredCamera(scene);

            InstallRuntimeFix(
                scene,
                levelRoot,
                playerCamera,
                "Nivel 4");

            PolishHud(
                scene,
                "NP_Level4_HUD",
                playerCamera,
                false);

            BuildLevel4VisualPolish(
                levelRoot.transform);

            ConfigureLevel4Lighting(
                scene,
                playerCamera);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void FixLevel5Internal()
        {
            if (!File.Exists(Scene5Path))
            {
                throw new FileNotFoundException(
                    "No existe Assets/Scenes/Nivel5.unity. " +
                    "Primero configura el Nivel 5.");
            }

            Scene scene =
                EditorSceneManager.OpenScene(
                    Scene5Path,
                    OpenSceneMode.Single);

            GameObject levelRoot =
                FindSceneObject(
                    scene,
                    "NP_LEVEL5_FINAL");

            if (levelRoot == null)
            {
                throw new InvalidOperationException(
                    "No encontré NP_LEVEL5_FINAL en Nivel5. " +
                    "Primero ejecuta Configurar Nivel 5 FINAL.");
            }

            Camera playerCamera =
                FindPreferredCamera(scene);

            InstallRuntimeFix(
                scene,
                levelRoot,
                playerCamera,
                "Nivel 5");

            PolishHud(
                scene,
                "NP_Level5_HUD",
                playerCamera,
                true);

            BuildLevel5VisualPolish(
                levelRoot.transform);

            ConfigureLevel5Lighting(
                scene,
                playerCamera);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void InstallRuntimeFix(
            Scene scene,
            GameObject levelRoot,
            Camera playerCamera,
            string levelName)
        {
            Transform existing =
                FindRecursive(
                    levelRoot.transform,
                    "NP_RUNTIME_FIX_45");

            GameObject runtimeObject;

            if (existing == null)
            {
                runtimeObject =
                    new GameObject(
                        "NP_RUNTIME_FIX_45");

                runtimeObject.transform.SetParent(
                    levelRoot.transform,
                    false);
            }
            else
            {
                runtimeObject = existing.gameObject;
            }

            NPLevel45RuntimeFix runtimeFix =
                runtimeObject.GetComponent<
                    NPLevel45RuntimeFix>();

            if (runtimeFix == null)
            {
                runtimeFix =
                    runtimeObject.AddComponent<
                        NPLevel45RuntimeFix>();
            }

            runtimeFix.preferredCamera =
                playerCamera;

            runtimeFix.forceDesktopInsideEditor = true;
            runtimeFix.desktopMoveSpeed = 3.2f;
            runtimeFix.configuredLevel = levelName;

            EnsureEventSystem(scene);
            EditorUtility.SetDirty(runtimeFix);
        }

        private static void PolishHud(
            Scene scene,
            string canvasName,
            Camera playerCamera,
            bool levelFive)
        {
            GameObject canvasObject =
                FindSceneObject(
                    scene,
                    canvasName);

            if (canvasObject == null)
            {
                Debug.LogWarning(
                    "NeuroPhobia: no encontré " +
                    canvasName +
                    ". La corrección visual continuó sin modificar el HUD.");

                return;
            }

            Canvas canvas =
                canvasObject.GetComponent<Canvas>();

            if (canvas == null)
            {
                canvas =
                    canvasObject.AddComponent<Canvas>();
            }

            canvas.renderMode =
                RenderMode.ScreenSpaceOverlay;

            canvas.worldCamera = null;
            canvas.sortingOrder = 500;
            canvas.pixelPerfect = true;

            CanvasScaler scaler =
                canvasObject.GetComponent<
                    CanvasScaler>();

            if (scaler == null)
            {
                scaler =
                    canvasObject.AddComponent<
                        CanvasScaler>();
            }

            scaler.uiScaleMode =
                CanvasScaler.ScaleMode
                    .ScaleWithScreenSize;

            scaler.referenceResolution =
                new Vector2(960f, 540f);

            scaler.matchWidthOrHeight = 0.45f;

            if (canvasObject.GetComponent<
                    GraphicRaycaster>() == null)
            {
                canvasObject.AddComponent<
                    GraphicRaycaster>();
            }

            NPAdaptiveHUDCanvas adaptive =
                canvasObject.GetComponent<
                    NPAdaptiveHUDCanvas>();

            if (adaptive == null)
            {
                adaptive =
                    canvasObject.AddComponent<
                        NPAdaptiveHUDCanvas>();
            }

            adaptive.preferredCamera =
                playerCamera;

            adaptive.referenceResolution =
                new Vector2(960f, 540f);

            adaptive.matchWidthOrHeight = 0.45f;

            Transform panel =
                FindRecursive(
                    canvasObject.transform,
                    "HUDPanel");

            if (panel != null)
            {
                SetRect(
                    panel as RectTransform,
                    new Vector2(0f, 1f),
                    new Vector2(0f, 1f),
                    new Vector2(0f, 1f),
                    new Vector2(18f, -18f),
                    levelFive
                        ? new Vector2(500f, 220f)
                        : new Vector2(500f, 248f));

                Image panelImage =
                    panel.GetComponent<Image>();

                if (panelImage != null)
                {
                    panelImage.color =
                        new Color(
                            0.018f,
                            0.028f,
                            0.045f,
                            0.94f);

                    panelImage.raycastTarget = false;
                }

                Outline panelOutline =
                    panel.GetComponent<Outline>();

                if (panelOutline == null)
                {
                    panelOutline =
                        panel.gameObject
                            .AddComponent<Outline>();
                }

                panelOutline.effectColor =
                    new Color(
                        0.12f,
                        0.65f,
                        0.92f,
                        0.75f);

                panelOutline.effectDistance =
                    new Vector2(2f, -2f);

                ConfigureHudChildren(
                    panel,
                    levelFive);
            }

            Transform prompt =
                FindRecursive(
                    canvasObject.transform,
                    "CenterPrompt");

            if (prompt != null)
            {
                RectTransform promptRect =
                    prompt as RectTransform;

                SetRect(
                    promptRect,
                    new Vector2(0.5f, 0f),
                    new Vector2(0.5f, 0f),
                    new Vector2(0.5f, 0f),
                    new Vector2(0f, 38f),
                    new Vector2(650f, 58f));

                TMP_Text promptText =
                    prompt.GetComponent<TMP_Text>();

                ConfigureText(
                    promptText,
                    24f,
                    TextAlignmentOptions.Center,
                    FontStyles.Bold,
                    true);

                EnsurePromptBackground(
                    canvasObject.transform,
                    prompt);
            }

            PolishCompletionPanel(
                canvasObject.transform,
                levelFive);

            EditorUtility.SetDirty(canvasObject);
        }

        private static void ConfigureHudChildren(
            Transform panel,
            bool levelFive)
        {
            Transform title =
                FindRecursive(panel, "Title");

            Transform objective =
                FindRecursive(panel, "Objective");

            Transform timer =
                FindRecursive(panel, "Timer");

            Transform best =
                FindRecursive(panel, "Best");

            Transform progressBackground =
                FindRecursive(
                    panel,
                    "ProgressBackground");

            Transform hint =
                FindRecursive(panel, "Hint");

            SetTextRect(
                title,
                new Vector2(18f, -14f),
                new Vector2(464f, 34f),
                25f,
                FontStyles.Bold);

            SetTextRect(
                objective,
                new Vector2(18f, -58f),
                new Vector2(464f, 30f),
                19f,
                FontStyles.Normal);

            SetTextRect(
                timer,
                new Vector2(18f, -94f),
                new Vector2(464f, 30f),
                19f,
                FontStyles.Normal);

            if (!levelFive)
            {
                SetTextRect(
                    best,
                    new Vector2(18f, -130f),
                    new Vector2(464f, 28f),
                    17f,
                    FontStyles.Normal);
            }

            if (progressBackground != null)
            {
                SetRect(
                    progressBackground as RectTransform,
                    new Vector2(0f, 1f),
                    new Vector2(0f, 1f),
                    new Vector2(0f, 1f),
                    levelFive
                        ? new Vector2(18f, -144f)
                        : new Vector2(18f, -176f),
                    new Vector2(464f, 18f));
            }

            if (hint != null)
            {
                SetTextRect(
                    hint,
                    levelFive
                        ? new Vector2(18f, -172f)
                        : new Vector2(18f, -204f),
                    new Vector2(464f, 38f),
                    15f,
                    FontStyles.Italic);

                TMP_Text hintText =
                    hint.GetComponent<TMP_Text>();

                if (hintText != null)
                {
                    hintText.textWrappingMode =
                        TextWrappingModes.Normal;
                }
            }
        }

        private static void SetTextRect(
            Transform target,
            Vector2 position,
            Vector2 size,
            float fontSize,
            FontStyles style)
        {
            if (target == null)
            {
                return;
            }

            SetRect(
                target as RectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                position,
                size);

            TMP_Text text =
                target.GetComponent<TMP_Text>();

            ConfigureText(
                text,
                fontSize,
                TextAlignmentOptions.Left,
                style,
                false);
        }

        private static void ConfigureText(
            TMP_Text text,
            float fontSize,
            TextAlignmentOptions alignment,
            FontStyles style,
            bool centered)
        {
            if (text == null)
            {
                return;
            }

            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = Color.white;
            text.enableAutoSizing = false;
            text.textWrappingMode =
                TextWrappingModes.Normal;
            text.overflowMode =
                TextOverflowModes.Ellipsis;
            text.raycastTarget = false;
            text.outlineColor =
                new Color32(0, 0, 0, 230);
            text.outlineWidth = centered
                ? 0.22f
                : 0.15f;

            EditorUtility.SetDirty(text);
        }

        private static void EnsurePromptBackground(
            Transform canvas,
            Transform prompt)
        {
            Transform background =
                FindRecursive(
                    canvas,
                    "CenterPrompt_Background");

            GameObject backgroundObject;

            if (background == null)
            {
                backgroundObject =
                    new GameObject(
                        "CenterPrompt_Background",
                        typeof(RectTransform),
                        typeof(CanvasRenderer),
                        typeof(Image));

                backgroundObject.transform.SetParent(
                    canvas,
                    false);

                background =
                    backgroundObject.transform;
            }
            else
            {
                backgroundObject =
                    background.gameObject;
            }

            SetRect(
                background as RectTransform,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 40f),
                new Vector2(690f, 60f));

            Image image =
                backgroundObject.GetComponent<Image>();

            image.color =
                new Color(
                    0.015f,
                    0.025f,
                    0.04f,
                    0.78f);

            image.raycastTarget = false;

            background.SetSiblingIndex(
                Mathf.Max(
                    0,
                    prompt.GetSiblingIndex()));

            prompt.SetAsLastSibling();
        }

        private static void PolishCompletionPanel(
            Transform canvas,
            bool levelFive)
        {
            Transform completion =
                FindRecursive(
                    canvas,
                    "CompletionPanel");

            if (completion == null)
            {
                return;
            }

            RectTransform completionRect =
                completion as RectTransform;

            if (completionRect != null)
            {
                completionRect.anchorMin =
                    Vector2.zero;

                completionRect.anchorMax =
                    Vector2.one;

                completionRect.pivot =
                    new Vector2(0.5f, 0.5f);

                completionRect.offsetMin =
                    Vector2.zero;

                completionRect.offsetMax =
                    Vector2.zero;
            }

            Image overlay =
                completion.GetComponent<Image>();

            if (overlay != null)
            {
                overlay.color =
                    new Color(
                        0.008f,
                        0.016f,
                        0.030f,
                        0.975f);

                overlay.raycastTarget = true;
            }

            Transform title =
                FindRecursive(
                    completion,
                    "CompletionTitle");

            Transform subtitle =
                FindRecursive(
                    completion,
                    "CompletionSubtitle");

            Transform countdown =
                FindRecursive(
                    completion,
                    "Countdown");

            SetCenteredText(
                title,
                new Vector2(0f, 125f),
                new Vector2(800f, 76f),
                48f,
                FontStyles.Bold);

            SetCenteredText(
                subtitle,
                new Vector2(0f, 20f),
                new Vector2(760f, 125f),
                27f,
                FontStyles.Normal);

            SetCenteredText(
                countdown,
                new Vector2(0f, -100f),
                new Vector2(700f, 62f),
                22f,
                FontStyles.Normal);

            if (levelFive)
            {
                PolishButton(
                    FindRecursive(
                        completion,
                        "MenuButton"),
                    new Vector2(-145f, -130f));

                PolishButton(
                    FindRecursive(
                        completion,
                        "RestartButton"),
                    new Vector2(145f, -130f));
            }
        }

        private static void SetCenteredText(
            Transform target,
            Vector2 position,
            Vector2 size,
            float fontSize,
            FontStyles style)
        {
            if (target == null)
            {
                return;
            }

            SetRect(
                target as RectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                position,
                size);

            ConfigureText(
                target.GetComponent<TMP_Text>(),
                fontSize,
                TextAlignmentOptions.Center,
                style,
                true);
        }

        private static void PolishButton(
            Transform target,
            Vector2 position)
        {
            if (target == null)
            {
                return;
            }

            SetRect(
                target as RectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                position,
                new Vector2(250f, 66f));

            Image image =
                target.GetComponent<Image>();

            if (image != null)
            {
                image.color =
                    new Color(
                        0.08f,
                        0.42f,
                        0.62f,
                        1f);

                image.raycastTarget = true;
            }

            Button button =
                target.GetComponent<Button>();

            if (button != null)
            {
                button.interactable = true;
                button.enabled = true;
            }

            TMP_Text label =
                target.GetComponentInChildren<
                    TMP_Text>(true);

            ConfigureText(
                label,
                21f,
                TextAlignmentOptions.Center,
                FontStyles.Bold,
                true);
        }

        private static void BuildLevel5VisualPolish(
            Transform levelRoot)
        {
            RemoveOldPolish(levelRoot);

            Transform polish =
                CreateChild(
                    levelRoot,
                    "NP_LEVEL5_VISUAL_POLISH");

            Material wallMaterial =
                GetOrCreateMaterial(
                    "M_L45_FactoryWall_Polished",
                    TexturePath +
                    "factory_wall_panels.png",
                    new Color(
                        0.62f,
                        0.70f,
                        0.78f,
                        1f),
                    0.72f,
                    0.32f,
                    new Vector2(3.5f, 2.0f));

            Material floorMaterial =
                GetOrCreateMaterial(
                    "M_L45_FactoryFloor_Polished",
                    TexturePath +
                    "factory_floor_concrete.png",
                    new Color(
                        0.58f,
                        0.60f,
                        0.63f,
                        1f),
                    0.15f,
                    0.16f,
                    new Vector2(5.0f, 8.0f));

            Material ceilingMaterial =
                GetOrCreateMaterial(
                    "M_L45_FactoryCeiling_Polished",
                    TexturePath +
                    "factory_ceiling_panels.png",
                    new Color(
                        0.42f,
                        0.48f,
                        0.55f,
                        1f),
                    0.70f,
                    0.28f,
                    new Vector2(5.0f, 8.0f));

            Material beamMaterial =
                GetOrCreateMaterial(
                    "M_L45_FactoryBeams",
                    null,
                    new Color(
                        0.055f,
                        0.075f,
                        0.095f,
                        1f),
                    0.82f,
                    0.38f,
                    Vector2.one);

            Material lowerBandMaterial =
                GetOrCreateMaterial(
                    "M_L45_FactoryLowerBand",
                    null,
                    new Color(
                        0.06f,
                        0.13f,
                        0.18f,
                        1f),
                    0.65f,
                    0.27f,
                    Vector2.one);

            Material warningMaterial =
                GetOrCreateMaterial(
                    "M_L45_FactoryWarning_Polished",
                    null,
                    new Color(
                        1f,
                        0.38f,
                        0.045f,
                        1f),
                    0.35f,
                    0.42f,
                    Vector2.one,
                    new Color(
                        1f,
                        0.12f,
                        0.01f,
                        1f) * 1.5f);

            Material lightMaterial =
                GetOrCreateMaterial(
                    "M_L45_FactoryLightStrip",
                    null,
                    new Color(
                        0.68f,
                        0.89f,
                        1f,
                        1f),
                    0.05f,
                    0.18f,
                    Vector2.one,
                    new Color(
                        0.40f,
                        0.76f,
                        1f,
                        1f) * 4.0f);

            Material paletteMaterial =
                AssetDatabase.LoadAssetAtPath<
                    Material>(
                    MaterialsPath +
                    "/M_L45_FactoryPalette.mat");

            ApplyMaterialToNamedObject(
                levelRoot,
                "FactoryFloor",
                floorMaterial);

            string[] wallNames =
            {
                "Wall_Left",
                "Wall_Right",
                "Wall_Back",
                "Entry_Left",
                "Entry_Right"
            };

            foreach (string wallName in wallNames)
            {
                ApplyMaterialToNamedObject(
                    levelRoot,
                    wallName,
                    wallMaterial);
            }

            GameObject ceiling =
                CreateBox(
                    polish,
                    "Factory_Ceiling",
                    new Vector3(0f, 5.10f, 3.70f),
                    new Vector3(14.1f, 0.22f, 23.1f),
                    ceilingMaterial,
                    true);

            ceiling.isStatic = true;

            CreateBox(
                polish,
                "LowerBand_Left",
                new Vector3(-6.80f, 0.62f, 3.70f),
                new Vector3(0.10f, 1.24f, 22.7f),
                lowerBandMaterial,
                false);

            CreateBox(
                polish,
                "LowerBand_Right",
                new Vector3(6.80f, 0.62f, 3.70f),
                new Vector3(0.10f, 1.24f, 22.7f),
                lowerBandMaterial,
                false);

            CreateBox(
                polish,
                "LowerBand_Back",
                new Vector3(0f, 0.62f, 15.00f),
                new Vector3(13.6f, 1.24f, 0.10f),
                lowerBandMaterial,
                false);

            BuildFactoryWallRibs(
                polish,
                beamMaterial);

            BuildFactoryCeiling(
                polish,
                beamMaterial,
                lightMaterial);

            BuildFactorySafetyDetails(
                polish,
                warningMaterial,
                lightMaterial);

            BuildFactoryDucts(
                polish,
                paletteMaterial);

            Material backdropMaterial =
                GetOrCreateMaterial(
                    "M_L45_IndustrialNightSky",
                    TexturePath +
                    "night_industrial_sky.png",
                    Color.white,
                    0f,
                    0f,
                    Vector2.one);

            CreateBox(
                polish,
                "ExteriorNightBackdrop",
                new Vector3(0f, 3.0f, -12.0f),
                new Vector3(20f, 8f, 0.12f),
                backdropMaterial,
                false);

            EditorUtility.SetDirty(polish.gameObject);
        }

        private static void BuildFactoryWallRibs(
            Transform parent,
            Material beamMaterial)
        {
            for (float z = -6.7f;
                 z <= 14.6f;
                 z += 2.35f)
            {
                CreateBox(
                    parent,
                    "WallRib_Left_" +
                    z.ToString("0.0"),
                    new Vector3(-6.70f, 2.55f, z),
                    new Vector3(0.22f, 4.75f, 0.18f),
                    beamMaterial,
                    false);

                CreateBox(
                    parent,
                    "WallRib_Right_" +
                    z.ToString("0.0"),
                    new Vector3(6.70f, 2.55f, z),
                    new Vector3(0.22f, 4.75f, 0.18f),
                    beamMaterial,
                    false);
            }

            for (float x = -5.8f;
                 x <= 5.8f;
                 x += 2.32f)
            {
                CreateBox(
                    parent,
                    "WallRib_Back_" +
                    x.ToString("0.0"),
                    new Vector3(x, 2.55f, 14.88f),
                    new Vector3(0.18f, 4.75f, 0.22f),
                    beamMaterial,
                    false);
            }

            CreateBox(
                parent,
                "Rail_Left_Low",
                new Vector3(-6.68f, 1.35f, 3.7f),
                new Vector3(0.18f, 0.16f, 22.4f),
                beamMaterial,
                false);

            CreateBox(
                parent,
                "Rail_Right_Low",
                new Vector3(6.68f, 1.35f, 3.7f),
                new Vector3(0.18f, 0.16f, 22.4f),
                beamMaterial,
                false);

            CreateBox(
                parent,
                "Rail_Left_High",
                new Vector3(-6.68f, 4.10f, 3.7f),
                new Vector3(0.18f, 0.18f, 22.4f),
                beamMaterial,
                false);

            CreateBox(
                parent,
                "Rail_Right_High",
                new Vector3(6.68f, 4.10f, 3.7f),
                new Vector3(0.18f, 0.18f, 22.4f),
                beamMaterial,
                false);
        }

        private static void BuildFactoryCeiling(
            Transform parent,
            Material beamMaterial,
            Material lightMaterial)
        {
            float[] beamZ =
            {
                -6.4f,
                -3.1f,
                0.2f,
                3.5f,
                6.8f,
                10.1f,
                13.4f
            };

            for (int index = 0;
                 index < beamZ.Length;
                 index++)
            {
                CreateBox(
                    parent,
                    "CeilingBeam_" + index,
                    new Vector3(
                        0f,
                        4.82f,
                        beamZ[index]),
                    new Vector3(
                        13.7f,
                        0.26f,
                        0.24f),
                    beamMaterial,
                    false);

                CreateBox(
                    parent,
                    "CeilingLight_" + index,
                    new Vector3(
                        0f,
                        4.62f,
                        beamZ[index] + 0.35f),
                    new Vector3(
                        5.4f,
                        0.08f,
                        0.16f),
                    lightMaterial,
                    false);

                AddPointLight(
                    parent,
                    "CeilingLightPoint_" + index,
                    new Vector3(
                        0f,
                        4.35f,
                        beamZ[index] + 0.35f),
                    index % 2 == 0
                        ? new Color(
                            0.56f,
                            0.80f,
                            1f)
                        : new Color(
                            1f,
                            0.66f,
                            0.42f),
                    8.5f,
                    8f);
            }
        }

        private static void BuildFactorySafetyDetails(
            Transform parent,
            Material warningMaterial,
            Material lightMaterial)
        {
            float[] zPositions =
            {
                -5.4f,
                -1.4f,
                2.6f,
                6.6f,
                10.6f,
                14.0f
            };

            for (int index = 0;
                 index < zPositions.Length;
                 index++)
            {
                CreateBox(
                    parent,
                    "WarningPanel_Left_" + index,
                    new Vector3(
                        -6.56f,
                        2.55f,
                        zPositions[index]),
                    new Vector3(
                        0.08f,
                        0.52f,
                        0.72f),
                    warningMaterial,
                    false);

                CreateBox(
                    parent,
                    "WarningPanel_Right_" + index,
                    new Vector3(
                        6.56f,
                        2.55f,
                        zPositions[index] + 1.6f),
                    new Vector3(
                        0.08f,
                        0.52f,
                        0.72f),
                    warningMaterial,
                    false);
            }

            for (int index = 0;
                 index < 5;
                 index++)
            {
                float z = -4.0f + index * 4.2f;

                CreateBox(
                    parent,
                    "FloorWarning_Left_" + index,
                    new Vector3(
                        -2.0f,
                        0.015f,
                        z),
                    new Vector3(
                        2.0f,
                        0.025f,
                        0.18f),
                    warningMaterial,
                    false);

                CreateBox(
                    parent,
                    "FloorWarning_Right_" + index,
                    new Vector3(
                        2.0f,
                        0.015f,
                        z + 1.2f),
                    new Vector3(
                        2.0f,
                        0.025f,
                        0.18f),
                    warningMaterial,
                    false);
            }

            CreateBox(
                parent,
                "BackStatusLight",
                new Vector3(0f, 3.55f, 14.84f),
                new Vector3(4.2f, 0.16f, 0.10f),
                lightMaterial,
                false);
        }

        private static void BuildFactoryDucts(
            Transform parent,
            Material paletteMaterial)
        {
            CreateFactoryModel(
                parent,
                "OverheadPipe_A",
                "pipe-large-long.fbx",
                new Vector3(-4.8f, 4.15f, 1.0f),
                new Vector3(90f, 0f, 0f),
                new Vector3(1.15f, 1.15f, 1.15f),
                paletteMaterial);

            CreateFactoryModel(
                parent,
                "OverheadPipe_B",
                "pipe-large-long.fbx",
                new Vector3(4.8f, 4.15f, 7.0f),
                new Vector3(90f, 0f, 0f),
                new Vector3(1.15f, 1.15f, 1.15f),
                paletteMaterial);

            CreateFactoryModel(
                parent,
                "OverheadValve",
                "pipe-large-valve.fbx",
                new Vector3(4.75f, 3.65f, 10.4f),
                new Vector3(0f, 0f, 90f),
                Vector3.one,
                paletteMaterial);

            CreateFactoryModel(
                parent,
                "WallScanner",
                "scanner-high.fbx",
                new Vector3(-6.25f, 0f, 10.8f),
                new Vector3(0f, 90f, 0f),
                Vector3.one,
                paletteMaterial);
        }

        private static void BuildLevel4VisualPolish(
            Transform levelRoot)
        {
            RemoveOldPolish(levelRoot);

            Transform polish =
                CreateChild(
                    levelRoot,
                    "NP_LEVEL4_VISUAL_POLISH");

            Material wallMaterial =
                GetOrCreateMaterial(
                    "M_L45_HouseWall_Polished",
                    TexturePath +
                    "house_wall_plaster.png",
                    new Color(
                        0.95f,
                        0.91f,
                        0.83f,
                        1f),
                    0.02f,
                    0.18f,
                    new Vector2(3f, 2f));

            Material floorMaterial =
                GetOrCreateMaterial(
                    "M_L45_HouseFloor_Polished",
                    TexturePath +
                    "house_floor_wood.png",
                    new Color(
                        0.92f,
                        0.82f,
                        0.72f,
                        1f),
                    0.03f,
                    0.26f,
                    new Vector2(4f, 5f));

            Material ceilingMaterial =
                GetOrCreateMaterial(
                    "M_L45_HouseCeiling",
                    TexturePath +
                    "house_wall_plaster.png",
                    new Color(
                        1f,
                        0.97f,
                        0.91f,
                        1f),
                    0.01f,
                    0.15f,
                    new Vector2(4f, 4f));

            Material trimMaterial =
                GetOrCreateMaterial(
                    "M_L45_HouseTrim_Polished",
                    null,
                    new Color(
                        0.19f,
                        0.14f,
                        0.11f,
                        1f),
                    0.04f,
                    0.28f,
                    Vector2.one);

            Material warmLightMaterial =
                GetOrCreateMaterial(
                    "M_L45_HouseWarmLight",
                    null,
                    new Color(
                        1f,
                        0.82f,
                        0.54f,
                        1f),
                    0f,
                    0.12f,
                    Vector2.one,
                    new Color(
                        1f,
                        0.45f,
                        0.12f,
                        1f) * 3f);

            ApplyMaterialToNamedObject(
                levelRoot,
                "Floor",
                floorMaterial);

            string[] wallNames =
            {
                "Wall_Back",
                "Wall_Left",
                "Wall_Right",
                "Wall_Front_Left",
                "Wall_Front_Right",
                "Front_Header"
            };

            foreach (string wallName in wallNames)
            {
                ApplyMaterialToNamedObject(
                    levelRoot,
                    wallName,
                    wallMaterial);
            }

            CreateBox(
                polish,
                "House_Ceiling",
                new Vector3(0f, 3.02f, 0.6f),
                new Vector3(12.1f, 0.18f, 9.3f),
                ceilingMaterial,
                true);

            CreateBox(
                polish,
                "Crown_Back",
                new Vector3(0f, 2.82f, 4.92f),
                new Vector3(11.7f, 0.16f, 0.12f),
                trimMaterial,
                false);

            CreateBox(
                polish,
                "Crown_Left",
                new Vector3(-5.82f, 2.82f, 0.6f),
                new Vector3(0.12f, 0.16f, 8.7f),
                trimMaterial,
                false);

            CreateBox(
                polish,
                "Crown_Right",
                new Vector3(5.82f, 2.82f, 0.6f),
                new Vector3(0.12f, 0.16f, 8.7f),
                trimMaterial,
                false);

            for (int index = 0;
                 index < 5;
                 index++)
            {
                float x = -4.6f + index * 2.3f;

                CreateBox(
                    polish,
                    "BackWallPanel_" + index,
                    new Vector3(x, 1.35f, 4.84f),
                    new Vector3(1.85f, 1.55f, 0.08f),
                    wallMaterial,
                    false);

                CreateBox(
                    polish,
                    "BackWallPanelFrame_" + index,
                    new Vector3(x, 1.35f, 4.78f),
                    new Vector3(1.98f, 1.68f, 0.045f),
                    trimMaterial,
                    false);
            }

            CreateBox(
                polish,
                "CeilingLampStrip",
                new Vector3(0f, 2.84f, 0.4f),
                new Vector3(3.2f, 0.06f, 0.22f),
                warmLightMaterial,
                false);

            AddPointLight(
                polish,
                "HousePolishLight_A",
                new Vector3(0f, 2.58f, 0.4f),
                new Color(1f, 0.78f, 0.58f),
                8f,
                6f);

            AddPointLight(
                polish,
                "HousePolishLight_B",
                new Vector3(3.7f, 2.25f, 2.7f),
                new Color(1f, 0.65f, 0.40f),
                5f,
                4f);

            EditorUtility.SetDirty(polish.gameObject);
        }

        private static void ConfigureLevel5Lighting(
            Scene scene,
            Camera playerCamera)
        {
            RenderSettings.ambientMode =
                AmbientMode.Trilight;

            RenderSettings.ambientSkyColor =
                new Color(
                    0.20f,
                    0.28f,
                    0.38f);

            RenderSettings.ambientEquatorColor =
                new Color(
                    0.085f,
                    0.11f,
                    0.15f);

            RenderSettings.ambientGroundColor =
                new Color(
                    0.025f,
                    0.035f,
                    0.05f);

            RenderSettings.ambientIntensity = 0.82f;
            RenderSettings.reflectionIntensity = 0.55f;

            RenderSettings.fog = true;
            RenderSettings.fogMode =
                FogMode.Linear;
            RenderSettings.fogColor =
                new Color(
                    0.055f,
                    0.075f,
                    0.105f);
            RenderSettings.fogStartDistance = 18f;
            RenderSettings.fogEndDistance = 38f;

            RenderSettings.skybox =
                GetOrCreateSkybox(
                    "M_L45_FactorySkybox",
                    new Color(
                        0.10f,
                        0.18f,
                        0.30f),
                    new Color(
                        0.015f,
                        0.025f,
                        0.045f),
                    0.62f);

            EnsureDirectionalLight(
                scene,
                "NP_Level5_Directional",
                new Vector3(42f, -28f, 0f),
                new Color(
                    0.48f,
                    0.62f,
                    0.82f),
                0.38f);

            if (playerCamera != null)
            {
                playerCamera.clearFlags =
                    CameraClearFlags.Skybox;

                playerCamera.backgroundColor =
                    new Color(
                        0.025f,
                        0.035f,
                        0.055f);

                EditorUtility.SetDirty(
                    playerCamera);
            }
        }

        private static void ConfigureLevel4Lighting(
            Scene scene,
            Camera playerCamera)
        {
            RenderSettings.ambientMode =
                AmbientMode.Trilight;

            RenderSettings.ambientSkyColor =
                new Color(
                    0.72f,
                    0.67f,
                    0.58f);

            RenderSettings.ambientEquatorColor =
                new Color(
                    0.36f,
                    0.31f,
                    0.26f);

            RenderSettings.ambientGroundColor =
                new Color(
                    0.12f,
                    0.10f,
                    0.09f);

            RenderSettings.ambientIntensity = 0.75f;
            RenderSettings.reflectionIntensity = 0.50f;
            RenderSettings.fog = false;

            RenderSettings.skybox =
                GetOrCreateSkybox(
                    "M_L45_HouseSkybox",
                    new Color(
                        0.28f,
                        0.39f,
                        0.55f),
                    new Color(
                        0.08f,
                        0.07f,
                        0.08f),
                    0.75f);

            EnsureDirectionalLight(
                scene,
                "NP_Level4_Directional",
                new Vector3(48f, -32f, 0f),
                new Color(
                    1f,
                    0.84f,
                    0.68f),
                0.42f);

            if (playerCamera != null)
            {
                playerCamera.clearFlags =
                    CameraClearFlags.Skybox;

                playerCamera.backgroundColor =
                    new Color(
                        0.10f,
                        0.12f,
                        0.16f);

                EditorUtility.SetDirty(
                    playerCamera);
            }
        }

        private static void RemoveOldPolish(
            Transform levelRoot)
        {
            string[] names =
            {
                "NP_LEVEL4_VISUAL_POLISH",
                "NP_LEVEL5_VISUAL_POLISH"
            };

            foreach (string name in names)
            {
                Transform existing =
                    FindRecursive(
                        levelRoot,
                        name);

                if (existing != null)
                {
                    Object.DestroyImmediate(
                        existing.gameObject);
                }
            }
        }

        private static Material GetOrCreateMaterial(
            string name,
            string textureAssetPath,
            Color tint,
            float metallic,
            float smoothness,
            Vector2 tiling,
            Color? emission = null)
        {
            EnsureFolder(MaterialsPath);

            string materialPath =
                MaterialsPath + "/" +
                name + ".mat";

            Material material =
                AssetDatabase.LoadAssetAtPath<
                    Material>(materialPath);

            if (material == null)
            {
                Shader shader =
                    Shader.Find(
                        "Universal Render Pipeline/Lit") ??
                    Shader.Find("Standard") ??
                    Shader.Find("Diffuse");

                material =
                    new Material(shader);

                material.name = name;

                AssetDatabase.CreateAsset(
                    material,
                    materialPath);
            }

            Texture2D texture = null;

            if (!string.IsNullOrEmpty(
                    textureAssetPath))
            {
                texture =
                    AssetDatabase.LoadAssetAtPath<
                        Texture2D>(
                        textureAssetPath);
            }

            SetColorIfPresent(
                material,
                "_BaseColor",
                tint);

            SetColorIfPresent(
                material,
                "_Color",
                tint);

            if (texture != null)
            {
                if (material.HasProperty(
                        "_BaseMap"))
                {
                    material.SetTexture(
                        "_BaseMap",
                        texture);

                    material.SetTextureScale(
                        "_BaseMap",
                        tiling);
                }

                if (material.HasProperty(
                        "_MainTex"))
                {
                    material.SetTexture(
                        "_MainTex",
                        texture);

                    material.SetTextureScale(
                        "_MainTex",
                        tiling);
                }
            }

            if (material.HasProperty(
                    "_Metallic"))
            {
                material.SetFloat(
                    "_Metallic",
                    metallic);
            }

            if (material.HasProperty(
                    "_Smoothness"))
            {
                material.SetFloat(
                    "_Smoothness",
                    smoothness);
            }

            if (material.HasProperty(
                    "_Glossiness"))
            {
                material.SetFloat(
                    "_Glossiness",
                    smoothness);
            }

            if (emission.HasValue)
            {
                if (material.HasProperty(
                        "_EmissionColor"))
                {
                    material.SetColor(
                        "_EmissionColor",
                        emission.Value);

                    material.EnableKeyword(
                        "_EMISSION");
                }
            }
            else
            {
                material.DisableKeyword(
                    "_EMISSION");
            }

            material.enableInstancing = true;

            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();

            return material;
        }

        private static void SetColorIfPresent(
            Material material,
            string propertyName,
            Color value)
        {
            if (material.HasProperty(
                    propertyName))
            {
                material.SetColor(
                    propertyName,
                    value);
            }
        }

        private static Material GetOrCreateSkybox(
            string name,
            Color skyTint,
            Color groundColor,
            float exposure)
        {
            EnsureFolder(MaterialsPath);

            string path =
                MaterialsPath + "/" +
                name + ".mat";

            Material material =
                AssetDatabase.LoadAssetAtPath<
                    Material>(path);

            Shader shader =
                Shader.Find("Skybox/Procedural");

            if (shader == null)
            {
                return null;
            }

            if (material == null ||
                material.shader != shader)
            {
                if (material != null)
                {
                    AssetDatabase.DeleteAsset(path);
                }

                material =
                    new Material(shader);

                material.name = name;

                AssetDatabase.CreateAsset(
                    material,
                    path);
            }

            if (material.HasProperty(
                    "_SkyTint"))
            {
                material.SetColor(
                    "_SkyTint",
                    skyTint);
            }

            if (material.HasProperty(
                    "_GroundColor"))
            {
                material.SetColor(
                    "_GroundColor",
                    groundColor);
            }

            if (material.HasProperty(
                    "_Exposure"))
            {
                material.SetFloat(
                    "_Exposure",
                    exposure);
            }

            if (material.HasProperty(
                    "_AtmosphereThickness"))
            {
                material.SetFloat(
                    "_AtmosphereThickness",
                    0.55f);
            }

            if (material.HasProperty(
                    "_SunSize"))
            {
                material.SetFloat(
                    "_SunSize",
                    0.015f);
            }

            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();

            return material;
        }

        private static void ApplyMaterialToNamedObject(
            Transform root,
            string objectName,
            Material material)
        {
            Transform target =
                FindRecursive(
                    root,
                    objectName);

            if (target == null)
            {
                return;
            }

            foreach (Renderer rendererComponent in
                     target.GetComponentsInChildren<
                         Renderer>(true))
            {
                Material[] materials =
                    rendererComponent.sharedMaterials;

                if (materials == null ||
                    materials.Length == 0)
                {
                    rendererComponent.sharedMaterial =
                        material;

                    continue;
                }

                for (int index = 0;
                     index < materials.Length;
                     index++)
                {
                    materials[index] = material;
                }

                rendererComponent.sharedMaterials =
                    materials;

                EditorUtility.SetDirty(
                    rendererComponent);
            }
        }

        private static void CreateFactoryModel(
            Transform parent,
            string name,
            string fileName,
            Vector3 position,
            Vector3 eulerAngles,
            Vector3 scale,
            Material overrideMaterial)
        {
            string assetPath =
                FactoryModelPath + fileName;

            GameObject asset =
                AssetDatabase.LoadAssetAtPath<
                    GameObject>(assetPath);

            if (asset == null)
            {
                return;
            }

            GameObject wrapper =
                new GameObject(name);

            wrapper.transform.SetParent(
                parent,
                false);

            wrapper.transform.position =
                position;

            wrapper.transform.eulerAngles =
                eulerAngles;

            wrapper.transform.localScale =
                scale;

            GameObject instance =
                PrefabUtility.InstantiatePrefab(
                    asset) as GameObject;

            if (instance == null)
            {
                return;
            }

            instance.name = "Visual";
            instance.transform.SetParent(
                wrapper.transform,
                false);

            instance.transform.localPosition =
                Vector3.zero;

            instance.transform.localRotation =
                Quaternion.identity;

            if (overrideMaterial != null)
            {
                foreach (Renderer rendererComponent in
                         instance.GetComponentsInChildren<
                             Renderer>(true))
                {
                    Material[] materials =
                        rendererComponent.sharedMaterials;

                    for (int index = 0;
                         index < materials.Length;
                         index++)
                    {
                        materials[index] =
                            overrideMaterial;
                    }

                    rendererComponent.sharedMaterials =
                        materials;
                }
            }
        }

        private static GameObject CreateBox(
            Transform parent,
            string name,
            Vector3 position,
            Vector3 size,
            Material material,
            bool keepCollider)
        {
            GameObject box =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cube);

            box.name = name;
            box.transform.SetParent(
                parent,
                false);

            box.transform.position =
                position;

            box.transform.localScale =
                size;

            Renderer rendererComponent =
                box.GetComponent<Renderer>();

            if (rendererComponent != null &&
                material != null)
            {
                rendererComponent.sharedMaterial =
                    material;
            }

            if (!keepCollider)
            {
                Collider colliderComponent =
                    box.GetComponent<Collider>();

                if (colliderComponent != null)
                {
                    Object.DestroyImmediate(
                        colliderComponent);
                }
            }

            return box;
        }

        private static void AddPointLight(
            Transform parent,
            string name,
            Vector3 position,
            Color color,
            float range,
            float intensity)
        {
            GameObject lightObject =
                new GameObject(name);

            lightObject.transform.SetParent(
                parent,
                false);

            lightObject.transform.position =
                position;

            Light lightComponent =
                lightObject.AddComponent<Light>();

            lightComponent.type =
                LightType.Point;

            lightComponent.color = color;
            lightComponent.range = range;
            lightComponent.intensity = intensity;
            lightComponent.shadows =
                LightShadows.Soft;
        }

        private static void EnsureDirectionalLight(
            Scene scene,
            string name,
            Vector3 eulerAngles,
            Color color,
            float intensity)
        {
            GameObject existing =
                FindSceneObject(
                    scene,
                    name);

            Light lightComponent;

            if (existing == null)
            {
                existing =
                    new GameObject(name);

                SceneManager.MoveGameObjectToScene(
                    existing,
                    scene);

                lightComponent =
                    existing.AddComponent<Light>();
            }
            else
            {
                lightComponent =
                    existing.GetComponent<Light>();

                if (lightComponent == null)
                {
                    lightComponent =
                        existing.AddComponent<Light>();
                }
            }

            existing.transform.eulerAngles =
                eulerAngles;

            lightComponent.type =
                LightType.Directional;

            lightComponent.color = color;
            lightComponent.intensity = intensity;
            lightComponent.shadows =
                LightShadows.Soft;

            EditorUtility.SetDirty(existing);
        }

        private static void EnsureEventSystem(
            Scene scene)
        {
            EventSystem existing =
                scene.GetRootGameObjects()
                .SelectMany(root =>
                    root.GetComponentsInChildren<
                        EventSystem>(true))
                .FirstOrDefault();

            if (existing != null)
            {
                existing.gameObject.SetActive(true);
                existing.enabled = true;
                return;
            }

            GameObject eventObject =
                new GameObject("EventSystem");

            SceneManager.MoveGameObjectToScene(
                eventObject,
                scene);

            eventObject.AddComponent<
                EventSystem>();
        }

        private static Camera FindPreferredCamera(
            Scene scene)
        {
            Camera[] cameras =
                scene.GetRootGameObjects()
                .SelectMany(root =>
                    root.GetComponentsInChildren<
                        Camera>(true))
                .ToArray();

            Camera desktop =
                cameras.FirstOrDefault(
                    cameraComponent =>
                        HierarchyNameContains(
                            cameraComponent.transform,
                            "np_desktopplayer_normalizado"));

            if (desktop != null)
            {
                return desktop;
            }

            desktop =
                cameras.FirstOrDefault(
                    cameraComponent =>
                        HierarchyNameContains(
                            cameraComponent.transform,
                            "desktop"));

            if (desktop != null)
            {
                return desktop;
            }

            return cameras.FirstOrDefault(
                       cameraComponent =>
                           cameraComponent.enabled &&
                           cameraComponent.gameObject
                               .activeInHierarchy) ??
                   cameras.FirstOrDefault();
        }

        private static bool HierarchyNameContains(
            Transform target,
            string value)
        {
            Transform current = target;

            while (current != null)
            {
                if (current.name.IndexOf(
                        value,
                        StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }

        private static void SetRect(
            RectTransform rect,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 anchoredPosition,
            Vector2 size)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition =
                anchoredPosition;
            rect.sizeDelta = size;

            EditorUtility.SetDirty(rect);
        }

        private static Transform CreateChild(
            Transform parent,
            string name)
        {
            GameObject child =
                new GameObject(name);

            child.transform.SetParent(
                parent,
                false);

            return child.transform;
        }

        private static GameObject FindSceneObject(
            Scene scene,
            string exactName)
        {
            foreach (GameObject root in
                     scene.GetRootGameObjects())
            {
                Transform match =
                    FindRecursive(
                        root.transform,
                        exactName);

                if (match != null)
                {
                    return match.gameObject;
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
                    StringComparison.Ordinal))
            {
                return current;
            }

            foreach (Transform child in current)
            {
                Transform match =
                    FindRecursive(
                        child,
                        exactName);

                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private static void EnsureFolder(
            string assetFolder)
        {
            if (AssetDatabase.IsValidFolder(
                    assetFolder))
            {
                return;
            }

            string parent =
                Path.GetDirectoryName(
                    assetFolder)
                ?.Replace("\\", "/");

            string folderName =
                Path.GetFileName(
                    assetFolder);

            if (!string.IsNullOrEmpty(parent) &&
                !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            if (!string.IsNullOrEmpty(parent))
            {
                AssetDatabase.CreateFolder(
                    parent,
                    folderName);
            }
        }
    }
}
#endif
