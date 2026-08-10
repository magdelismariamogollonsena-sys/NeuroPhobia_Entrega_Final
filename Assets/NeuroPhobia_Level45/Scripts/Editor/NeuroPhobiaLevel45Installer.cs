#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NeuroPhobia.Level45;
using TMPro;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace NeuroPhobia.Level45.Editor
{
    public static class NeuroPhobiaLevel45Installer
    {
        private const string RootPath = "Assets/NeuroPhobia_Level45";
        private const string Scene4Path = "Assets/Scenes/Nivel4.unity";
        private const string Scene5Path = "Assets/Scenes/Nivel5.unity";
        private const string Scene3Path = "Assets/Scenes/Nivel3.unity";
        private const string GeneratedPath =
            RootPath + "/Generated";
        private const string MaterialsPath =
            GeneratedPath + "/Materials";
        private const string ControllersPath =
            GeneratedPath + "/Animators";

        private static readonly string HouseModelPath =
            RootPath + "/Art/House/Models/";
        private static readonly string FactoryModelPath =
            RootPath + "/Art/Factory/Models/";
        private static readonly string FactoryTexturePath =
            RootPath + "/Art/Factory/Textures/colormap.png";

        [MenuItem(
            "Tools/NeuroPhobia/Configurar Niveles 4 y 5 COMPLETOS",
            priority = 400)]
        public static void ConfigureBoth()
        {
            if (!CanEdit())
            {
                return;
            }

            try
            {
                PrepareAssetImporters();
                ConfigureLevel4Internal();
                ConfigureLevel5Internal();
                AddBuildScenesInternal();

                EditorUtility.DisplayDialog(
                    "NeuroPhobia",
                    "Los niveles 4 y 5 quedaron configurados.\n\n" +
                    "También se agregaron al Build en el orden correcto.\n\n" +
                    "Abre Nivel4 y pruébalo primero.",
                    "Aceptar");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);

                EditorUtility.DisplayDialog(
                    "NeuroPhobia",
                    "La configuración se detuvo por un error.\n\n" +
                    exception.Message +
                    "\n\nRevisa la Console y envía el primer error rojo.",
                    "Aceptar");
            }
        }

        [MenuItem(
            "Tools/NeuroPhobia/Configurar Nivel 4 FINAL",
            priority = 401)]
        public static void ConfigureLevel4()
        {
            if (!CanEdit())
            {
                return;
            }

            PrepareAssetImporters();
            ConfigureLevel4Internal();

            EditorUtility.DisplayDialog(
                "NeuroPhobia",
                "Nivel 4 configurado y guardado.",
                "Aceptar");
        }

        [MenuItem(
            "Tools/NeuroPhobia/Configurar Nivel 5 FINAL",
            priority = 402)]
        public static void ConfigureLevel5()
        {
            if (!CanEdit())
            {
                return;
            }

            PrepareAssetImporters();
            ConfigureLevel5Internal();

            EditorUtility.DisplayDialog(
                "NeuroPhobia",
                "Nivel 5 configurado y guardado.",
                "Aceptar");
        }

        [MenuItem(
            "Tools/NeuroPhobia/Agregar Niveles 4 y 5 al Build",
            priority = 403)]
        public static void AddBuildScenes()
        {
            if (!CanEdit())
            {
                return;
            }

            AddBuildScenesInternal();

            EditorUtility.DisplayDialog(
                "NeuroPhobia",
                "Nivel4 y Nivel5 están incluidos en el Build.",
                "Aceptar");
        }

        [MenuItem(
            "Tools/NeuroPhobia/Verificar Niveles 4 y 5",
            priority = 404)]
        public static void VerifyLevels()
        {
            List<string> problems = new List<string>();

            if (!File.Exists(Scene4Path))
            {
                problems.Add("Falta Assets/Scenes/Nivel4.unity");
            }

            if (!File.Exists(Scene5Path))
            {
                problems.Add("Falta Assets/Scenes/Nivel5.unity");
            }

            if (!File.Exists(
                    RootPath +
                    "/Audio/Level4/house_ambience.mp3"))
            {
                problems.Add("Falta el audio ambiental del Nivel 4");
            }

            if (!File.Exists(
                    RootPath +
                    "/Audio/Level5/industrial_ambience.mp3"))
            {
                problems.Add("Falta el audio industrial del Nivel 5");
            }

            string spiderAsset = FindSpiderAssetPath();
            if (string.IsNullOrEmpty(spiderAsset))
            {
                problems.Add(
                    "No encontré el modelo Wolf Spider. " +
                    "Se utilizará la araña procedural de respaldo.");
            }

            string message = problems.Count == 0
                ? "Verificación correcta.\n\n" +
                  "Escenas, audios y araña encontrados."
                : "Revisión terminada:\n\n• " +
                  string.Join("\n• ", problems);

            EditorUtility.DisplayDialog(
                "NeuroPhobia",
                message,
                "Aceptar");
        }

        private static bool CanEdit()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog(
                    "NeuroPhobia",
                    "Detén Play antes de ejecutar el comando.",
                    "Aceptar");
                return false;
            }

            return true;
        }

        private static void ConfigureLevel4Internal()
        {
            Scene scene = OpenOrCreateScene(
                Scene4Path,
                out bool copiedFromLevel3);

            Camera playerCamera = PrepareSceneBase(
                scene,
                copiedFromLevel3,
                new Vector3(0f, 1.65f, -3.7f));

            GameObject previous =
                FindSceneObject(scene, "NP_LEVEL4_FINAL");

            if (previous != null)
            {
                Object.DestroyImmediate(previous);
            }

            GameObject root = new GameObject("NP_LEVEL4_FINAL");
            SceneManager.MoveGameObjectToScene(root, scene);

            Transform environment = CreateChild(
                root.transform,
                "Environment");
            Transform furniture = CreateChild(
                root.transform,
                "Furniture");
            Transform routes = CreateChild(
                root.transform,
                "SpiderRoutes");
            Transform gameplay = CreateChild(
                root.transform,
                "Gameplay");

            Material floorMaterial = GetMaterial(
                "M_L45_FloorWarm",
                new Color(0.34f, 0.27f, 0.22f),
                0.05f);

            Material wallMaterial = GetMaterial(
                "M_L45_WallWarm",
                new Color(0.76f, 0.72f, 0.65f),
                0.02f);

            Material trimMaterial = GetMaterial(
                "M_L45_Trim",
                new Color(0.18f, 0.20f, 0.22f),
                0.02f);

            Material woodMaterial = GetMaterial(
                "M_L45_Wood",
                new Color(0.30f, 0.17f, 0.10f),
                0.03f);

            Material fabricMaterial = GetMaterial(
                "M_L45_Fabric",
                new Color(0.22f, 0.34f, 0.42f),
                0.02f);

            Material greenMaterial = GetMaterial(
                "M_L45_Plant",
                new Color(0.20f, 0.45f, 0.24f),
                0.02f);

            BuildLevel4Room(
                environment,
                floorMaterial,
                wallMaterial,
                trimMaterial);

            BuildLevel4Furniture(
                furniture,
                woodMaterial,
                fabricMaterial,
                greenMaterial);

            AddPointLight(
                environment,
                "WarmLight_Center",
                new Vector3(0f, 2.35f, 0.4f),
                new Color(1f, 0.78f, 0.58f),
                7.5f,
                22f);

            AddPointLight(
                environment,
                "WarmLight_Bed",
                new Vector3(-3.5f, 1.7f, 2.0f),
                new Color(1f, 0.66f, 0.45f),
                4.5f,
                12f);

            AudioSource ambience = AddAudio(
                environment,
                "House_Ambience",
                RootPath +
                "/Audio/Level4/house_ambience.mp3",
                true,
                0.22f,
                0f);

            Vector3[] spiderRoute =
            {
                new Vector3(2.7f, 0.06f, 2.4f),
                new Vector3(3.7f, 0.06f, 0.9f),
                new Vector3(2.1f, 0.06f, 0.2f),
                new Vector3(1.4f, 0.06f, 1.8f)
            };

            Transform[] routePoints =
                CreateRoutePoints(
                    routes,
                    "L4_Route",
                    spiderRoute);

            NPNaturalSpiderPatrol spider =
                CreateSpider(
                    gameplay,
                    "NP_Spider_Level4",
                    spiderRoute[0],
                    0.68f,
                    routePoints,
                    0.16f);

            Transform[] exposurePoints =
            {
                CreateOrb(
                    gameplay,
                    "L4_Exposure_1",
                    new Vector3(0f, 0.42f, -1.7f),
                    new Color(0.20f, 0.72f, 1f)),
                CreateOrb(
                    gameplay,
                    "L4_Exposure_2",
                    new Vector3(-2.25f, 0.42f, 0.65f),
                    new Color(0.25f, 0.90f, 0.66f)),
                CreateOrb(
                    gameplay,
                    "L4_Exposure_3",
                    new Vector3(1.75f, 0.42f, 2.35f),
                    new Color(1f, 0.64f, 0.20f))
            };

            Level4UI ui = CreateLevel4UI(
                root.transform,
                playerCamera);

            GameObject controllerObject = new GameObject(
                "NP_Level4_Controller");

            controllerObject.transform.SetParent(
                root.transform,
                false);

            NPLevel4Controller controller =
                controllerObject.AddComponent<
                    NPLevel4Controller>();

            controller.spiderTarget =
                spider != null ? spider.transform : null;
            controller.spiderPatrol = spider;
            controller.exposurePoints = exposurePoints;
            controller.secondsPerPoint = 10f;
            controller.activationRadius = 1.45f;
            controller.nextSceneName = "Nivel5";
            controller.transitionDelay = 5f;

            controller.titleText = ui.title;
            controller.objectiveText = ui.objective;
            controller.timerText = ui.timer;
            controller.bestText = ui.best;
            controller.promptText = ui.prompt;
            controller.progressFill = ui.progressFill;

            controller.completionPanel = ui.completionPanel;
            controller.completionTitle = ui.completionTitle;
            controller.completionSubtitle =
                ui.completionSubtitle;
            controller.countdownText = ui.countdown;

            controller.uiAudio = AddAudio(
                gameplay,
                "UI_Audio_Level4",
                null,
                false,
                0.8f,
                0f);

            controller.checkpointClip = LoadAudio(
                RootPath + "/Audio/UI/checkpoint.ogg");

            controller.completionClip = LoadAudio(
                RootPath + "/Audio/UI/completion.ogg");

            EnsurePlayerPosition(
                playerCamera,
                new Vector3(0f, 1.65f, -3.7f));

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Selection.activeGameObject = root;
        }

        private static void ConfigureLevel5Internal()
        {
            Scene scene = OpenOrCreateScene(
                Scene5Path,
                out bool copiedFromLevel3);

            Camera playerCamera = PrepareSceneBase(
                scene,
                copiedFromLevel3,
                new Vector3(0f, 1.65f, -7.8f));

            GameObject previous =
                FindSceneObject(scene, "NP_LEVEL5_FINAL");

            if (previous != null)
            {
                Object.DestroyImmediate(previous);
            }

            GameObject root = new GameObject("NP_LEVEL5_FINAL");
            SceneManager.MoveGameObjectToScene(root, scene);

            Transform environment = CreateChild(
                root.transform,
                "Environment");
            Transform props = CreateChild(
                root.transform,
                "FactoryProps");
            Transform routes = CreateChild(
                root.transform,
                "SpiderRoutes");
            Transform gameplay = CreateChild(
                root.transform,
                "Gameplay");

            Material floorMaterial = GetMaterial(
                "M_L45_FactoryFloor",
                new Color(0.12f, 0.14f, 0.16f),
                0.05f);

            Material wallMaterial = GetMaterial(
                "M_L45_FactoryWall",
                new Color(0.20f, 0.23f, 0.27f),
                0.02f);

            Material warningMaterial = GetMaterial(
                "M_L45_Warning",
                new Color(0.95f, 0.48f, 0.08f),
                0.35f);

            Material factoryMaterial =
                GetFactoryPaletteMaterial();

            BuildLevel5Factory(
                environment,
                props,
                floorMaterial,
                wallMaterial,
                warningMaterial,
                factoryMaterial);

            AddPointLight(
                environment,
                "FactoryLight_Blue",
                new Vector3(-4.5f, 3.6f, 1.5f),
                new Color(0.28f, 0.55f, 1f),
                9f,
                28f);

            AddPointLight(
                environment,
                "FactoryLight_Orange",
                new Vector3(4.8f, 3.4f, 5.8f),
                new Color(1f, 0.38f, 0.12f),
                9f,
                28f);

            AddPointLight(
                environment,
                "FactoryLight_Final",
                new Vector3(0f, 3.2f, 10.8f),
                new Color(0.35f, 1f, 0.72f),
                8f,
                22f);

            AddAudio(
                environment,
                "Industrial_Ambience",
                RootPath +
                "/Audio/Level5/industrial_ambience.mp3",
                true,
                0.22f,
                0f);

            AudioSource fanAudio = AddAudio(
                environment,
                "Industrial_Fan_3D",
                RootPath +
                "/Audio/Level5/industrial_fan.mp3",
                true,
                0.36f,
                1f);

            fanAudio.transform.position =
                new Vector3(5.4f, 1.8f, 6.6f);
            fanAudio.minDistance = 1.5f;
            fanAudio.maxDistance = 11f;
            fanAudio.rolloffMode =
                AudioRolloffMode.Logarithmic;

            Transform[] routeA = CreateRoutePoints(
                routes,
                "L5_Route_A",
                new[]
                {
                    new Vector3(-4.8f, 0.06f, -1.2f),
                    new Vector3(-4.8f, 0.06f, 4.8f),
                    new Vector3(-2.4f, 0.06f, 4.8f),
                    new Vector3(-2.4f, 0.06f, -1.2f)
                });

            Transform[] routeB = CreateRoutePoints(
                routes,
                "L5_Route_B",
                new[]
                {
                    new Vector3(3.0f, 0.06f, 1.5f),
                    new Vector3(5.2f, 0.06f, 2.4f),
                    new Vector3(4.6f, 0.06f, 7.0f),
                    new Vector3(2.4f, 0.06f, 6.0f)
                });

            Transform[] routeC = CreateRoutePoints(
                routes,
                "L5_Route_C",
                new[]
                {
                    new Vector3(-1.3f, 0.06f, 9.2f),
                    new Vector3(1.5f, 0.06f, 9.2f),
                    new Vector3(1.5f, 0.06f, 12.0f),
                    new Vector3(-1.3f, 0.06f, 12.0f)
                });

            NPNaturalSpiderPatrol spiderA =
                CreateSpider(
                    gameplay,
                    "NP_Spider_Level5_A",
                    routeA[0].position,
                    0.58f,
                    routeA,
                    0.17f);

            NPNaturalSpiderPatrol spiderB =
                CreateSpider(
                    gameplay,
                    "NP_Spider_Level5_B",
                    routeB[0].position,
                    0.62f,
                    routeB,
                    0.15f);

            NPNaturalSpiderPatrol spiderC =
                CreateSpider(
                    gameplay,
                    "NP_Spider_Level5_Central",
                    routeC[0].position,
                    0.72f,
                    routeC,
                    0.13f);

            Transform[] orbs =
            {
                CreateOrb(
                    gameplay,
                    "L5_Orb_1",
                    new Vector3(-4.3f, 0.48f, -0.8f),
                    new Color(0.18f, 0.72f, 1f)),
                CreateOrb(
                    gameplay,
                    "L5_Orb_2",
                    new Vector3(-3.7f, 0.48f, 5.2f),
                    new Color(0.32f, 1f, 0.60f)),
                CreateOrb(
                    gameplay,
                    "L5_Orb_3",
                    new Vector3(4.3f, 0.48f, 5.8f),
                    new Color(1f, 0.60f, 0.18f)),
                CreateOrb(
                    gameplay,
                    "L5_Orb_4",
                    new Vector3(0f, 0.48f, 9.4f),
                    new Color(0.82f, 0.36f, 1f))
            };

            Transform finalZone = CreateFinalZone(
                gameplay,
                new Vector3(0f, 0.035f, 11.0f));

            Level5UI ui = CreateLevel5UI(
                root.transform,
                playerCamera);

            GameObject controllerObject = new GameObject(
                "NP_Level5_Controller");

            controllerObject.transform.SetParent(
                root.transform,
                false);

            NPLevel5Controller controller =
                controllerObject.AddComponent<
                    NPLevel5Controller>();

            controller.orbs = orbs;
            controller.finalZone = finalZone;
            controller.centralSpider =
                spiderC != null ? spiderC.transform : null;
            controller.spiderPatrols =
                new[] { spiderA, spiderB, spiderC }
                    .Where(item => item != null)
                    .ToArray();

            controller.orbCollectionRadius = 1.35f;
            controller.finalZoneRadius = 1.55f;
            controller.finalLookSeconds = 15f;

            controller.titleText = ui.title;
            controller.objectiveText = ui.objective;
            controller.timerText = ui.timer;
            controller.promptText = ui.prompt;
            controller.progressFill = ui.progressFill;

            controller.completionPanel =
                ui.completionPanel;
            controller.completionTitle =
                ui.completionTitle;
            controller.completionSubtitle =
                ui.completionSubtitle;
            controller.menuButton = ui.menuButton;
            controller.restartButton = ui.restartButton;
            controller.menuSceneName =
                DetectMenuSceneName();

            controller.uiAudio = AddAudio(
                gameplay,
                "UI_Audio_Level5",
                null,
                false,
                0.8f,
                0f);

            controller.orbClip = LoadAudio(
                RootPath + "/Audio/UI/select.ogg");

            controller.completionClip = LoadAudio(
                RootPath + "/Audio/UI/completion.ogg");

            EnsureEventSystem(scene);

            EnsurePlayerPosition(
                playerCamera,
                new Vector3(0f, 1.65f, -7.8f));

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Selection.activeGameObject = root;
        }

        private static Scene OpenOrCreateScene(
            string targetPath,
            out bool copiedFromLevel3)
        {
            copiedFromLevel3 = false;

            string sceneDirectory =
                Path.GetDirectoryName(targetPath);

            if (!Directory.Exists(sceneDirectory))
            {
                Directory.CreateDirectory(sceneDirectory);
            }

            if (!File.Exists(targetPath))
            {
                if (File.Exists(Scene3Path))
                {
                    AssetDatabase.CopyAsset(
                        Scene3Path,
                        targetPath);

                    AssetDatabase.Refresh();
                    copiedFromLevel3 = true;
                }
                else
                {
                    Scene created =
                        EditorSceneManager.NewScene(
                            NewSceneSetup.EmptyScene,
                            NewSceneMode.Single);

                    EditorSceneManager.SaveScene(
                        created,
                        targetPath);

                    return created;
                }
            }

            return EditorSceneManager.OpenScene(
                targetPath,
                OpenSceneMode.Single);
        }

        private static Camera PrepareSceneBase(
            Scene scene,
            bool copiedFromLevel3,
            Vector3 desiredCameraPosition)
        {
            Camera cameraComponent =
                FindBestCamera(scene);

            DeactivateCopiedLevelContent(
                scene,
                cameraComponent);

            if (cameraComponent == null)
            {
                cameraComponent = CreateFallbackPlayer(
                    scene,
                    desiredCameraPosition);
            }

            EnsurePlayerPosition(
                cameraComponent,
                desiredCameraPosition);

            return cameraComponent;
        }

        private static Camera FindBestCamera(Scene scene)
        {
            Camera[] cameras = scene
                .GetRootGameObjects()
                .SelectMany(rootObject =>
                    rootObject.GetComponentsInChildren<
                        Camera>(true))
                .ToArray();

            Camera active = cameras.FirstOrDefault(
                cameraComponent =>
                    cameraComponent != null &&
                    cameraComponent.enabled &&
                    cameraComponent.gameObject.activeInHierarchy);

            if (active != null)
            {
                return active;
            }

            return cameras.FirstOrDefault();
        }

        private static void DeactivateCopiedLevelContent(
            Scene scene,
            Camera cameraComponent)
        {
            foreach (GameObject rootObject in
                     scene.GetRootGameObjects())
            {
                if (ShouldPreserveRoot(
                        rootObject,
                        cameraComponent))
                {
                    DisableKnownLevel3Children(
                        rootObject.transform,
                        cameraComponent != null
                            ? cameraComponent.transform
                            : null);

                    continue;
                }

                rootObject.SetActive(false);
                EditorUtility.SetDirty(rootObject);
            }
        }

        private static bool ShouldPreserveRoot(
            GameObject rootObject,
            Camera cameraComponent)
        {
            if (cameraComponent != null &&
                cameraComponent.transform.IsChildOf(
                    rootObject.transform))
            {
                return true;
            }

            string name =
                rootObject.name.ToLowerInvariant();

            string[] keywords =
            {
                "player", "desktop", "xr origin",
                "xrorigin", "rig", "input",
                "eventsystem", "game state",
                "gamestate", "pause", "breathing",
                "simulator", "ovr"
            };

            if (keywords.Any(name.Contains))
            {
                return true;
            }

            return rootObject.GetComponentInChildren<
                       EventSystem>(true) != null;
        }

        private static void DisableKnownLevel3Children(
            Transform current,
            Transform cameraTransform)
        {
            foreach (Transform child in current)
            {
                if (cameraTransform != null &&
                    (child == cameraTransform ||
                     cameraTransform.IsChildOf(child)))
                {
                    DisableKnownLevel3Children(
                        child,
                        cameraTransform);
                    continue;
                }

                string name =
                    child.name.ToLowerInvariant();

                bool oldLevelContent =
                    name.Contains("nivel3") ||
                    name.Contains("nivel4") ||
                    name.Contains("nivel5") ||
                    name.Contains("sotano") ||
                    name.Contains("factory") ||
                    name.Contains("fabrica") ||
                    name.Contains("house") ||
                    name.Contains("room") ||
                    name.Contains("environment") ||
                    name.Contains("entorno") ||
                    name.Contains("spider") ||
                    name.Contains("arana") ||
                    name.Contains("orbe") ||
                    name.Contains("orb") ||
                    name.Contains("checkpoint") ||
                    name.Contains("patrol") ||
                    name.Contains("np_env");

                if (oldLevelContent)
                {
                    child.gameObject.SetActive(false);
                    EditorUtility.SetDirty(child.gameObject);
                    continue;
                }

                DisableKnownLevel3Children(
                    child,
                    cameraTransform);
            }
        }

        private static Camera CreateFallbackPlayer(
            Scene scene,
            Vector3 cameraPosition)
        {
            GameObject player = new GameObject(
                "NP_DesktopPlayer_Niveles45");

            SceneManager.MoveGameObjectToScene(
                player,
                scene);

            player.transform.position =
                new Vector3(
                    cameraPosition.x,
                    0f,
                    cameraPosition.z);

            CharacterController controller =
                player.AddComponent<CharacterController>();

            controller.height = 1.75f;
            controller.radius = 0.28f;
            controller.center = new Vector3(0f, 0.88f, 0f);

            GameObject cameraObject =
                new GameObject("Main Camera");

            cameraObject.transform.SetParent(
                player.transform,
                false);

            cameraObject.transform.localPosition =
                new Vector3(0f, cameraPosition.y, 0f);

            Camera cameraComponent =
                cameraObject.AddComponent<Camera>();

            cameraObject.AddComponent<AudioListener>();
            cameraObject.tag = "MainCamera";

            NPDesktopFallbackPlayer movement =
                player.AddComponent<
                    NPDesktopFallbackPlayer>();

            movement.playerCamera = cameraComponent;

            return cameraComponent;
        }

        private static void EnsurePlayerPosition(
            Camera cameraComponent,
            Vector3 desiredCameraPosition)
        {
            if (cameraComponent == null)
            {
                return;
            }

            Transform root =
                cameraComponent.transform.root;

            Vector3 delta =
                desiredCameraPosition -
                cameraComponent.transform.position;

            root.position += delta;
            EditorUtility.SetDirty(root.gameObject);
        }

        private static void BuildLevel4Room(
            Transform parent,
            Material floorMaterial,
            Material wallMaterial,
            Material trimMaterial)
        {
            CreateBox(
                parent,
                "Floor",
                new Vector3(0f, -0.10f, 0.6f),
                new Vector3(12f, 0.20f, 9.2f),
                floorMaterial);

            CreateBox(
                parent,
                "Wall_Back",
                new Vector3(0f, 1.45f, 5.1f),
                new Vector3(12f, 2.9f, 0.18f),
                wallMaterial);

            CreateBox(
                parent,
                "Wall_Left",
                new Vector3(-6f, 1.45f, 0.6f),
                new Vector3(0.18f, 2.9f, 9.2f),
                wallMaterial);

            CreateBox(
                parent,
                "Wall_Right",
                new Vector3(6f, 1.45f, 0.6f),
                new Vector3(0.18f, 2.9f, 9.2f),
                wallMaterial);

            CreateBox(
                parent,
                "Wall_Front_Left",
                new Vector3(-3.8f, 1.45f, -4f),
                new Vector3(4.4f, 2.9f, 0.18f),
                wallMaterial);

            CreateBox(
                parent,
                "Wall_Front_Right",
                new Vector3(3.8f, 1.45f, -4f),
                new Vector3(4.4f, 2.9f, 0.18f),
                wallMaterial);

            CreateBox(
                parent,
                "Front_Header",
                new Vector3(0f, 2.55f, -4f),
                new Vector3(3.2f, 0.70f, 0.18f),
                wallMaterial);

            CreateBox(
                parent,
                "Baseboard_Back",
                new Vector3(0f, 0.10f, 4.96f),
                new Vector3(11.8f, 0.20f, 0.10f),
                trimMaterial);

            CreateBox(
                parent,
                "Baseboard_Left",
                new Vector3(-5.86f, 0.10f, 0.6f),
                new Vector3(0.10f, 0.20f, 8.8f),
                trimMaterial);

            CreateBox(
                parent,
                "Baseboard_Right",
                new Vector3(5.86f, 0.10f, 0.6f),
                new Vector3(0.10f, 0.20f, 8.8f),
                trimMaterial);
        }

        private static void BuildLevel4Furniture(
            Transform parent,
            Material woodMaterial,
            Material fabricMaterial,
            Material greenMaterial)
        {
            CreateModel(
                HouseModelPath + "Bed_King.fbx",
                "Bed",
                parent,
                new Vector3(-3.55f, 0f, 2.55f),
                new Vector3(0f, 90f, 0f),
                Vector3.one,
                fabricMaterial,
                true);

            CreateModel(
                HouseModelPath + "NightStand_1.fbx",
                "NightStand",
                parent,
                new Vector3(-5.0f, 0f, 2.6f),
                Vector3.zero,
                Vector3.one,
                woodMaterial,
                true);

            CreateModel(
                HouseModelPath + "Light_Desk.fbx",
                "BedLamp",
                parent,
                new Vector3(-5.0f, 0.78f, 2.6f),
                Vector3.zero,
                Vector3.one * 0.85f,
                null,
                false);

            CreateModel(
                HouseModelPath + "Couch_Medium1.fbx",
                "Couch",
                parent,
                new Vector3(3.85f, 0f, -0.9f),
                new Vector3(0f, -90f, 0f),
                Vector3.one,
                fabricMaterial,
                true);

            CreateModel(
                HouseModelPath + "Table_RoundSmall.fbx",
                "SideTable",
                parent,
                new Vector3(2.6f, 0f, -1.7f),
                Vector3.zero,
                Vector3.one,
                woodMaterial,
                true);

            CreateModel(
                HouseModelPath + "Bookshelf.fbx",
                "Bookshelf",
                parent,
                new Vector3(-4.8f, 0f, -1.1f),
                new Vector3(0f, 90f, 0f),
                Vector3.one,
                woodMaterial,
                true);

            CreateModel(
                HouseModelPath + "Drawer_2.fbx",
                "Drawer",
                parent,
                new Vector3(-4.75f, 0f, 0.8f),
                new Vector3(0f, 90f, 0f),
                Vector3.one,
                woodMaterial,
                true);

            CreateModel(
                HouseModelPath + "Carpet_1.fbx",
                "Carpet",
                parent,
                new Vector3(0f, 0.015f, 0.6f),
                Vector3.zero,
                new Vector3(1.35f, 1f, 1.35f),
                fabricMaterial,
                false);

            CreateModel(
                HouseModelPath + "Houseplant_6.fbx",
                "Plant_Left",
                parent,
                new Vector3(-5.0f, 0f, 4.0f),
                Vector3.zero,
                Vector3.one,
                greenMaterial,
                true);

            CreateModel(
                HouseModelPath + "Houseplant_1.fbx",
                "Plant_Right",
                parent,
                new Vector3(5.0f, 0f, 3.9f),
                Vector3.zero,
                Vector3.one,
                greenMaterial,
                true);

            CreateModel(
                HouseModelPath + "Window_Large1.fbx",
                "Window",
                parent,
                new Vector3(0f, 1.45f, 4.96f),
                new Vector3(0f, 180f, 0f),
                Vector3.one,
                null,
                false);

            CreateModel(
                HouseModelPath + "Curtains_Double.fbx",
                "Curtains",
                parent,
                new Vector3(0f, 1.55f, 4.84f),
                new Vector3(0f, 180f, 0f),
                Vector3.one,
                fabricMaterial,
                false);

            CreateModel(
                HouseModelPath + "Door_1.fbx",
                "Door",
                parent,
                new Vector3(0f, 0f, -3.93f),
                Vector3.zero,
                Vector3.one,
                woodMaterial,
                true);

            CreateModel(
                HouseModelPath + "Light_Floor1.fbx",
                "FloorLamp",
                parent,
                new Vector3(4.9f, 0f, -2.7f),
                Vector3.zero,
                Vector3.one,
                null,
                true);
        }

        private static void BuildLevel5Factory(
            Transform environment,
            Transform props,
            Material floorMaterial,
            Material wallMaterial,
            Material warningMaterial,
            Material factoryMaterial)
        {
            CreateBox(
                environment,
                "FactoryFloor",
                new Vector3(0f, -0.12f, 3.7f),
                new Vector3(14f, 0.24f, 23f),
                floorMaterial);

            CreateBox(
                environment,
                "Wall_Left",
                new Vector3(-7f, 2.5f, 3.7f),
                new Vector3(0.22f, 5f, 23f),
                wallMaterial);

            CreateBox(
                environment,
                "Wall_Right",
                new Vector3(7f, 2.5f, 3.7f),
                new Vector3(0.22f, 5f, 23f),
                wallMaterial);

            CreateBox(
                environment,
                "Wall_Back",
                new Vector3(0f, 2.5f, 15.2f),
                new Vector3(14f, 5f, 0.22f),
                wallMaterial);

            CreateBox(
                environment,
                "Entry_Left",
                new Vector3(-4.5f, 2.5f, -7.8f),
                new Vector3(5f, 5f, 0.22f),
                wallMaterial);

            CreateBox(
                environment,
                "Entry_Right",
                new Vector3(4.5f, 2.5f, -7.8f),
                new Vector3(5f, 5f, 0.22f),
                wallMaterial);

            CreateBox(
                environment,
                "WarningLine_Left",
                new Vector3(-0.75f, 0.01f, 3.7f),
                new Vector3(0.18f, 0.02f, 22f),
                warningMaterial);

            CreateBox(
                environment,
                "WarningLine_Right",
                new Vector3(0.75f, 0.01f, 3.7f),
                new Vector3(0.18f, 0.02f, 22f),
                warningMaterial);

            CreateModel(
                FactoryModelPath + "machine-fortified.fbx",
                "Machine_Left",
                props,
                new Vector3(-5.3f, 0f, 1.2f),
                new Vector3(0f, 90f, 0f),
                Vector3.one * 1.15f,
                factoryMaterial,
                true);

            CreateModel(
                FactoryModelPath + "machine.fbx",
                "Machine_Right",
                props,
                new Vector3(5.1f, 0f, 5.8f),
                new Vector3(0f, -90f, 0f),
                Vector3.one * 1.15f,
                factoryMaterial,
                true);

            CreateModel(
                FactoryModelPath + "robot-arm-a.fbx",
                "RobotArm",
                props,
                new Vector3(4.6f, 0f, 0.2f),
                new Vector3(0f, -90f, 0f),
                Vector3.one,
                factoryMaterial,
                true);

            for (int index = 0; index < 4; index++)
            {
                CreateModel(
                    FactoryModelPath + "conveyor-long-sides.fbx",
                    "Conveyor_" + index,
                    props,
                    new Vector3(
                        2.4f,
                        0f,
                        1.4f + index * 2.0f),
                    Vector3.zero,
                    Vector3.one,
                    factoryMaterial,
                    true);
            }

            CreateModel(
                FactoryModelPath + "catwalk-straight.fbx",
                "Catwalk_Left",
                props,
                new Vector3(-5.0f, 2.0f, 8.0f),
                new Vector3(0f, 90f, 0f),
                Vector3.one,
                factoryMaterial,
                true);

            CreateModel(
                FactoryModelPath + "catwalk-stairs.fbx",
                "Catwalk_Stairs",
                props,
                new Vector3(-5.0f, 0f, 6.0f),
                new Vector3(0f, 90f, 0f),
                Vector3.one,
                factoryMaterial,
                true);

            CreateModel(
                FactoryModelPath + "pipe-large-long.fbx",
                "Pipe_Left",
                props,
                new Vector3(-6.55f, 2.5f, 3.0f),
                new Vector3(90f, 0f, 0f),
                Vector3.one,
                factoryMaterial,
                false);

            CreateModel(
                FactoryModelPath + "pipe-large-valve.fbx",
                "Pipe_Valve",
                props,
                new Vector3(-6.45f, 1.7f, 7.2f),
                new Vector3(0f, 0f, 90f),
                Vector3.one,
                factoryMaterial,
                false);

            CreateModel(
                FactoryModelPath + "crane.fbx",
                "Crane",
                props,
                new Vector3(4.9f, 0f, 11.4f),
                new Vector3(0f, -90f, 0f),
                Vector3.one,
                factoryMaterial,
                true);

            CreateModel(
                FactoryModelPath + "hopper-high-round.fbx",
                "Hopper",
                props,
                new Vector3(-4.8f, 0f, 12.0f),
                Vector3.zero,
                Vector3.one,
                factoryMaterial,
                true);

            CreateModel(
                FactoryModelPath + "screen-wide.fbx",
                "ControlScreen",
                props,
                new Vector3(5.9f, 1.3f, 8.7f),
                new Vector3(0f, -90f, 0f),
                Vector3.one,
                factoryMaterial,
                false);

            Vector3[] boxPositions =
            {
                new Vector3(-3.7f, 0f, -3.8f),
                new Vector3(-4.5f, 0f, -3.2f),
                new Vector3(4.7f, 0f, -2.3f),
                new Vector3(5.2f, 0f, -1.5f),
                new Vector3(-3.8f, 0f, 7.1f),
                new Vector3(3.7f, 0f, 10.2f)
            };

            for (int index = 0;
                 index < boxPositions.Length;
                 index++)
            {
                CreateModel(
                    FactoryModelPath +
                    (index % 2 == 0
                        ? "box-large.fbx"
                        : "box-small.fbx"),
                    "FactoryBox_" + index,
                    props,
                    boxPositions[index],
                    new Vector3(
                        0f,
                        index * 23f,
                        0f),
                    Vector3.one,
                    factoryMaterial,
                    true);
            }
        }

        private static NPNaturalSpiderPatrol CreateSpider(
            Transform parent,
            string name,
            Vector3 position,
            float desiredSize,
            Transform[] routePoints,
            float speed)
        {
            GameObject spiderRoot = new GameObject(name);
            spiderRoot.transform.SetParent(parent, false);
            spiderRoot.transform.position = position;

            string spiderAssetPath = FindSpiderAssetPath();
            GameObject visual = null;

            if (!string.IsNullOrEmpty(spiderAssetPath))
            {
                GameObject asset =
                    AssetDatabase.LoadAssetAtPath<GameObject>(
                        spiderAssetPath);

                if (asset != null)
                {
                    visual =
                        PrefabUtility.InstantiatePrefab(asset)
                        as GameObject;

                    if (visual != null)
                    {
                        visual.name = "Visual_WolfSpider";
                        visual.transform.SetParent(
                            spiderRoot.transform,
                            false);

                        foreach (MonoBehaviour component in
                                 visual.GetComponentsInChildren<
                                     MonoBehaviour>(true))
                        {
                            Object.DestroyImmediate(component);
                        }

                        NormalizeVisual(
                            visual,
                            spiderRoot.transform.position,
                            desiredSize);

                        PrepareSpiderAnimator(
                            visual,
                            spiderAssetPath,
                            name);
                    }
                }
            }

            if (visual == null)
            {
                visual = CreateProceduralSpiderVisual(
                    spiderRoot.transform,
                    desiredSize);
            }

            AddBoundsCollider(spiderRoot);

            NPNaturalSpiderPatrol patrol =
                spiderRoot.AddComponent<
                    NPNaturalSpiderPatrol>();

            patrol.waypoints = routePoints;
            patrol.moveSpeed = speed;
            patrol.turnSpeed = 4.5f;
            patrol.pauseAtPoint = 0.8f;
            patrol.visualRoot = visual.transform;
            patrol.animator =
                visual.GetComponentInChildren<Animator>(true);
            patrol.animationSpeed = 0.32f;
            patrol.rayHeight = 1.2f;
            patrol.rayDistance = 3.5f;
            patrol.maximumDistanceFromRoute = 14f;

            return patrol;
        }

        private static string FindSpiderAssetPath()
        {
            string[] guids =
                AssetDatabase.FindAssets(
                    "spider t:GameObject");

            IEnumerable<string> paths = guids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path =>
                    !path.StartsWith(
                        RootPath,
                        StringComparison.OrdinalIgnoreCase))
                .Where(path =>
                    path.EndsWith(
                        ".fbx",
                        StringComparison.OrdinalIgnoreCase) ||
                    path.EndsWith(
                        ".prefab",
                        StringComparison.OrdinalIgnoreCase));

            string preferred = paths.FirstOrDefault(
                path =>
                    path.IndexOf(
                        "wolf",
                        StringComparison.OrdinalIgnoreCase) >= 0);

            if (!string.IsNullOrEmpty(preferred))
            {
                return preferred;
            }

            string first = paths.FirstOrDefault();
            if (!string.IsNullOrEmpty(first))
            {
                return first;
            }

            string[] wolfGuids =
                AssetDatabase.FindAssets(
                    "wolf t:GameObject");

            return wolfGuids
                .Select(AssetDatabase.GUIDToAssetPath)
                .FirstOrDefault(path =>
                    path.EndsWith(
                        ".fbx",
                        StringComparison.OrdinalIgnoreCase) ||
                    path.EndsWith(
                        ".prefab",
                        StringComparison.OrdinalIgnoreCase));
        }

        private static void NormalizeVisual(
            GameObject visual,
            Vector3 groundPosition,
            float desiredSize)
        {
            Renderer[] renderers =
                visual.GetComponentsInChildren<
                    Renderer>(true);

            if (renderers.Length == 0)
            {
                return;
            }

            Bounds bounds = renderers[0].bounds;
            for (int index = 1;
                 index < renderers.Length;
                 index++)
            {
                bounds.Encapsulate(renderers[index].bounds);
            }

            float largest = Mathf.Max(
                bounds.size.x,
                bounds.size.y,
                bounds.size.z);

            if (largest > 0.0001f)
            {
                float factor = desiredSize / largest;
                visual.transform.localScale *= factor;
            }

            renderers =
                visual.GetComponentsInChildren<
                    Renderer>(true);

            bounds = renderers[0].bounds;
            for (int index = 1;
                 index < renderers.Length;
                 index++)
            {
                bounds.Encapsulate(renderers[index].bounds);
            }

            visual.transform.position += Vector3.up *
                (groundPosition.y - bounds.min.y + 0.015f);
        }

        private static void PrepareSpiderAnimator(
            GameObject visual,
            string assetPath,
            string controllerName)
        {
            Animator animator =
                visual.GetComponentInChildren<
                    Animator>(true);

            if (animator == null)
            {
                animator = visual.AddComponent<Animator>();
            }

            animator.applyRootMotion = false;
            animator.cullingMode =
                AnimatorCullingMode.AlwaysAnimate;

            AnimationClip[] clips =
                AssetDatabase.LoadAllAssetsAtPath(assetPath)
                .OfType<AnimationClip>()
                .Where(clip =>
                    clip != null &&
                    !clip.name.StartsWith(
                        "__preview__",
                        StringComparison.OrdinalIgnoreCase))
                .ToArray();

            AnimationClip preferred =
                clips.FirstOrDefault(clip =>
                    clip.name.IndexOf(
                        "walk",
                        StringComparison.OrdinalIgnoreCase) >= 0) ??
                clips.FirstOrDefault(clip =>
                    clip.name.IndexOf(
                        "idle",
                        StringComparison.OrdinalIgnoreCase) >= 0) ??
                clips.FirstOrDefault();

            if (preferred == null)
            {
                animator.enabled = false;
                return;
            }

            EnsureFolder(GeneratedPath);
            EnsureFolder(ControllersPath);

            string safeName = string.Concat(
                controllerName.Select(character =>
                    char.IsLetterOrDigit(character)
                        ? character
                        : '_'));

            string controllerPath =
                ControllersPath + "/" +
                safeName + ".controller";

            AnimatorController controller =
                AssetDatabase.LoadAssetAtPath<
                    AnimatorController>(
                    controllerPath);

            if (controller == null)
            {
                controller =
                    AnimatorController
                    .CreateAnimatorControllerAtPathWithClip(
                        controllerPath,
                        preferred);
            }

            animator.runtimeAnimatorController = controller;
            animator.enabled = true;
        }

        private static GameObject CreateProceduralSpiderVisual(
            Transform parent,
            float desiredSize)
        {
            GameObject visual =
                new GameObject("Visual_ProceduralSpider");

            visual.transform.SetParent(parent, false);

            Material bodyMaterial = GetMaterial(
                "M_L45_Spider",
                new Color(0.07f, 0.045f, 0.03f),
                0.02f);

            GameObject abdomen = CreatePrimitive(
                PrimitiveType.Sphere,
                visual.transform,
                "Abdomen",
                new Vector3(0f, 0.12f, -0.10f),
                new Vector3(
                    desiredSize * 0.48f,
                    desiredSize * 0.30f,
                    desiredSize * 0.58f),
                bodyMaterial);

            GameObject head = CreatePrimitive(
                PrimitiveType.Sphere,
                visual.transform,
                "Head",
                new Vector3(0f, 0.11f, 0.20f),
                new Vector3(
                    desiredSize * 0.34f,
                    desiredSize * 0.26f,
                    desiredSize * 0.34f),
                bodyMaterial);

            Object.DestroyImmediate(
                abdomen.GetComponent<Collider>());

            Object.DestroyImmediate(
                head.GetComponent<Collider>());

            for (int side = -1; side <= 1; side += 2)
            {
                for (int leg = 0; leg < 4; leg++)
                {
                    float z =
                        -0.18f + leg * 0.13f;

                    Vector3 start =
                        new Vector3(
                            side * desiredSize * 0.16f,
                            0.10f,
                            z);

                    Vector3 knee =
                        new Vector3(
                            side * desiredSize *
                            (0.38f + leg * 0.025f),
                            0.07f,
                            z + (leg - 1.5f) * 0.04f);

                    Vector3 end =
                        new Vector3(
                            side * desiredSize *
                            (0.60f + leg * 0.025f),
                            0.015f,
                            z + (leg - 1.5f) * 0.08f);

                    CreateCylinderBetween(
                        visual.transform,
                        "Leg_A",
                        start,
                        knee,
                        desiredSize * 0.025f,
                        bodyMaterial);

                    CreateCylinderBetween(
                        visual.transform,
                        "Leg_B",
                        knee,
                        end,
                        desiredSize * 0.022f,
                        bodyMaterial);
                }
            }

            return visual;
        }

        private static void CreateCylinderBetween(
            Transform parent,
            string name,
            Vector3 start,
            Vector3 end,
            float radius,
            Material material)
        {
            Vector3 midpoint = (start + end) * 0.5f;
            Vector3 direction = end - start;

            GameObject cylinder = CreatePrimitive(
                PrimitiveType.Cylinder,
                parent,
                name,
                midpoint,
                new Vector3(
                    radius,
                    direction.magnitude * 0.5f,
                    radius),
                material);

            cylinder.transform.up =
                direction.normalized;

            Object.DestroyImmediate(
                cylinder.GetComponent<Collider>());
        }

        private static Transform[] CreateRoutePoints(
            Transform parent,
            string prefix,
            Vector3[] positions)
        {
            Transform[] points =
                new Transform[positions.Length];

            for (int index = 0;
                 index < positions.Length;
                 index++)
            {
                GameObject pointObject =
                    new GameObject(
                        prefix + "_" + (index + 1));

                pointObject.transform.SetParent(
                    parent,
                    false);

                pointObject.transform.position =
                    positions[index];

                points[index] =
                    pointObject.transform;
            }

            return points;
        }

        private static Transform CreateOrb(
            Transform parent,
            string name,
            Vector3 position,
            Color color)
        {
            Material orbMaterial = GetMaterial(
                "M_" + name,
                color,
                2.8f);

            GameObject root =
                new GameObject(name);

            root.transform.SetParent(parent, false);
            root.transform.position = position;

            GameObject sphere = CreatePrimitive(
                PrimitiveType.Sphere,
                root.transform,
                "OrbVisual",
                Vector3.zero,
                Vector3.one * 0.34f,
                orbMaterial);

            Collider sphereCollider =
                sphere.GetComponent<Collider>();

            if (sphereCollider != null)
            {
                Object.DestroyImmediate(sphereCollider);
            }

            Light lightComponent =
                root.AddComponent<Light>();

            lightComponent.type = LightType.Point;
            lightComponent.color = color;
            lightComponent.range = 2.8f;
            lightComponent.intensity = 2.4f;

            root.AddComponent<NPOrbPulse>();
            return root.transform;
        }

        private static Transform CreateFinalZone(
            Transform parent,
            Vector3 position)
        {
            Material zoneMaterial = GetMaterial(
                "M_L45_FinalZone",
                new Color(0.22f, 1f, 0.65f),
                1.5f);

            GameObject zone = CreatePrimitive(
                PrimitiveType.Cylinder,
                parent,
                "L5_FinalZone",
                position,
                new Vector3(1.35f, 0.025f, 1.35f),
                zoneMaterial);

            Collider colliderComponent =
                zone.GetComponent<Collider>();

            if (colliderComponent != null)
            {
                Object.DestroyImmediate(colliderComponent);
            }

            zone.AddComponent<NPOrbPulse>();
            return zone.transform;
        }

        private static GameObject CreateModel(
            string assetPath,
            string name,
            Transform parent,
            Vector3 position,
            Vector3 eulerAngles,
            Vector3 scale,
            Material overrideMaterial,
            bool addCollider)
        {
            GameObject wrapper = new GameObject(name);
            wrapper.transform.SetParent(parent, false);
            wrapper.transform.position = position;
            wrapper.transform.eulerAngles = eulerAngles;
            wrapper.transform.localScale = scale;

            GameObject asset =
                AssetDatabase.LoadAssetAtPath<
                    GameObject>(assetPath);

            if (asset == null)
            {
                Debug.LogWarning(
                    "NeuroPhobia: no se encontró " +
                    assetPath);

                return wrapper;
            }

            GameObject instance =
                PrefabUtility.InstantiatePrefab(asset)
                as GameObject;

            if (instance == null)
            {
                return wrapper;
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

            if (addCollider)
            {
                AddBoundsCollider(wrapper);
            }

            return wrapper;
        }

        private static void AddBoundsCollider(
            GameObject rootObject)
        {
            Renderer[] renderers =
                rootObject.GetComponentsInChildren<
                    Renderer>(true);

            if (renderers.Length == 0)
            {
                return;
            }

            List<Vector3> localPoints =
                new List<Vector3>();

            foreach (Renderer rendererComponent in
                     renderers)
            {
                Bounds bounds = rendererComponent.bounds;
                Vector3 center = bounds.center;
                Vector3 extents = bounds.extents;

                for (int x = -1; x <= 1; x += 2)
                {
                    for (int y = -1; y <= 1; y += 2)
                    {
                        for (int z = -1; z <= 1; z += 2)
                        {
                            Vector3 worldPoint =
                                center +
                                Vector3.Scale(
                                    extents,
                                    new Vector3(x, y, z));

                            localPoints.Add(
                                rootObject.transform
                                    .InverseTransformPoint(
                                        worldPoint));
                        }
                    }
                }
            }

            if (localPoints.Count == 0)
            {
                return;
            }

            Bounds localBounds =
                new Bounds(
                    localPoints[0],
                    Vector3.zero);

            foreach (Vector3 point in localPoints)
            {
                localBounds.Encapsulate(point);
            }

            BoxCollider boxCollider =
                rootObject.GetComponent<BoxCollider>();

            if (boxCollider == null)
            {
                boxCollider =
                    rootObject.AddComponent<BoxCollider>();
            }

            boxCollider.center = localBounds.center;
            boxCollider.size = localBounds.size;
        }

        private static GameObject CreateBox(
            Transform parent,
            string name,
            Vector3 position,
            Vector3 size,
            Material material)
        {
            return CreatePrimitive(
                PrimitiveType.Cube,
                parent,
                name,
                position,
                size,
                material);
        }

        private static GameObject CreatePrimitive(
            PrimitiveType primitiveType,
            Transform parent,
            string name,
            Vector3 position,
            Vector3 scale,
            Material material)
        {
            GameObject result =
                GameObject.CreatePrimitive(
                    primitiveType);

            result.name = name;
            result.transform.SetParent(parent, false);
            result.transform.position = position;
            result.transform.localScale = scale;

            Renderer rendererComponent =
                result.GetComponent<Renderer>();

            if (rendererComponent != null &&
                material != null)
            {
                rendererComponent.sharedMaterial =
                    material;
            }

            return result;
        }

        private static Material GetFactoryPaletteMaterial()
        {
            Material material = GetMaterial(
                "M_L45_FactoryPalette",
                Color.white,
                0.05f);

            Texture2D texture =
                AssetDatabase.LoadAssetAtPath<Texture2D>(
                    FactoryTexturePath);

            if (texture != null)
            {
                if (material.HasProperty("_BaseMap"))
                {
                    material.SetTexture(
                        "_BaseMap",
                        texture);
                }

                if (material.HasProperty("_MainTex"))
                {
                    material.SetTexture(
                        "_MainTex",
                        texture);
                }

                EditorUtility.SetDirty(material);
            }

            return material;
        }

        private static Material GetMaterial(
            string name,
            Color color,
            float emissionStrength)
        {
            EnsureFolder(GeneratedPath);
            EnsureFolder(MaterialsPath);

            string path =
                MaterialsPath + "/" + name + ".mat";

            Material material =
                AssetDatabase.LoadAssetAtPath<Material>(
                    path);

            if (material == null)
            {
                Shader shader =
                    Shader.Find(
                        "Universal Render Pipeline/Lit") ??
                    Shader.Find("Standard") ??
                    Shader.Find("Diffuse");

                material = new Material(shader);
                material.name = name;

                AssetDatabase.CreateAsset(
                    material,
                    path);
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            if (emissionStrength > 0.08f)
            {
                Color emissionColor =
                    color * emissionStrength;

                if (material.HasProperty(
                        "_EmissionColor"))
                {
                    material.SetColor(
                        "_EmissionColor",
                        emissionColor);

                    material.EnableKeyword("_EMISSION");
                }
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat(
                    "_Smoothness",
                    0.24f);
            }

            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
            return material;
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

            lightObject.transform.position = position;

            Light lightComponent =
                lightObject.AddComponent<Light>();

            lightComponent.type = LightType.Point;
            lightComponent.color = color;
            lightComponent.range = range;
            lightComponent.intensity = intensity;
            lightComponent.shadows =
                LightShadows.Soft;
        }

        private static AudioSource AddAudio(
            Transform parent,
            string name,
            string clipPath,
            bool loop,
            float volume,
            float spatialBlend)
        {
            GameObject audioObject =
                new GameObject(name);

            audioObject.transform.SetParent(
                parent,
                false);

            AudioSource audioSource =
                audioObject.AddComponent<AudioSource>();

            audioSource.clip =
                string.IsNullOrEmpty(clipPath)
                    ? null
                    : LoadAudio(clipPath);

            audioSource.loop = loop;
            audioSource.playOnAwake =
                audioSource.clip != null;
            audioSource.volume = volume;
            audioSource.spatialBlend = spatialBlend;

            return audioSource;
        }

        private static AudioClip LoadAudio(
            string assetPath)
        {
            return AssetDatabase.LoadAssetAtPath<
                AudioClip>(assetPath);
        }

        private static Level4UI CreateLevel4UI(
            Transform parent,
            Camera cameraComponent)
        {
            Canvas canvas = CreateCanvas(
                parent,
                "NP_Level4_HUD",
                cameraComponent);

            Image panel = CreatePanel(
                canvas.transform,
                "HUDPanel",
                new Vector2(24f, -24f),
                new Vector2(530f, 280f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Color(0.04f, 0.07f, 0.10f, 0.90f));

            TMP_Text title = CreateText(
                panel.transform,
                "Title",
                "NIVEL 4 · EXPOSICIÓN CERCANA",
                30f,
                FontStyles.Bold,
                TextAlignmentOptions.Left,
                new Vector2(22f, -20f),
                new Vector2(486f, 42f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f));

            TMP_Text objective = CreateText(
                panel.transform,
                "Objective",
                "Punto de exposición: 1 / 3",
                24f,
                FontStyles.Normal,
                TextAlignmentOptions.Left,
                new Vector2(22f, -76f),
                new Vector2(486f, 34f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f));

            TMP_Text timer = CreateText(
                panel.transform,
                "Timer",
                "Mirada continua: 0.0 / 10 s",
                24f,
                FontStyles.Normal,
                TextAlignmentOptions.Left,
                new Vector2(22f, -118f),
                new Vector2(486f, 34f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f));

            TMP_Text best = CreateText(
                panel.transform,
                "Best",
                "Mejor intento: 0.0 s",
                22f,
                FontStyles.Normal,
                TextAlignmentOptions.Left,
                new Vector2(22f, -158f),
                new Vector2(486f, 32f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f));

            Image progressFill = CreateProgressBar(
                panel.transform,
                new Vector2(22f, -214f),
                new Vector2(486f, 24f));

            TMP_Text hint = CreateText(
                panel.transform,
                "Hint",
                "Acércate al orbe y mira a la araña.",
                18f,
                FontStyles.Italic,
                TextAlignmentOptions.Left,
                new Vector2(22f, -246f),
                new Vector2(486f, 26f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f));

            TMP_Text prompt = CreateText(
                canvas.transform,
                "CenterPrompt",
                "Acércate al orbe luminoso",
                30f,
                FontStyles.Bold,
                TextAlignmentOptions.Center,
                new Vector2(0f, 110f),
                new Vector2(880f, 110f),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f));

            CompletionUI completion =
                CreateCompletionUI(
                    canvas.transform,
                    false);

            return new Level4UI
            {
                title = title,
                objective = objective,
                timer = timer,
                best = best,
                prompt = prompt,
                progressFill = progressFill,
                completionPanel =
                    completion.panel.gameObject,
                completionTitle = completion.title,
                completionSubtitle =
                    completion.subtitle,
                countdown = completion.countdown
            };
        }

        private static Level5UI CreateLevel5UI(
            Transform parent,
            Camera cameraComponent)
        {
            Canvas canvas = CreateCanvas(
                parent,
                "NP_Level5_HUD",
                cameraComponent);

            Image panel = CreatePanel(
                canvas.transform,
                "HUDPanel",
                new Vector2(24f, -24f),
                new Vector2(550f, 250f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Color(0.035f, 0.055f, 0.075f, 0.92f));

            TMP_Text title = CreateText(
                panel.transform,
                "Title",
                "NIVEL 5 · DESAFÍO FINAL",
                30f,
                FontStyles.Bold,
                TextAlignmentOptions.Left,
                new Vector2(22f, -20f),
                new Vector2(506f, 42f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f));

            TMP_Text objective = CreateText(
                panel.transform,
                "Objective",
                "Orbes recogidos: 0 / 4",
                24f,
                FontStyles.Normal,
                TextAlignmentOptions.Left,
                new Vector2(22f, -78f),
                new Vector2(506f, 34f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f));

            TMP_Text timer = CreateText(
                panel.transform,
                "Timer",
                "Busca el siguiente orbe",
                24f,
                FontStyles.Normal,
                TextAlignmentOptions.Left,
                new Vector2(22f, -122f),
                new Vector2(506f, 34f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f));

            Image progressFill = CreateProgressBar(
                panel.transform,
                new Vector2(22f, -178f),
                new Vector2(506f, 24f));

            TMP_Text hint = CreateText(
                panel.transform,
                "Hint",
                "Las arañas siguen rutas fijas y no persiguen al jugador.",
                18f,
                FontStyles.Italic,
                TextAlignmentOptions.Left,
                new Vector2(22f, -210f),
                new Vector2(506f, 30f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f));

            TMP_Text prompt = CreateText(
                canvas.transform,
                "CenterPrompt",
                "Sigue el orbe luminoso",
                30f,
                FontStyles.Bold,
                TextAlignmentOptions.Center,
                new Vector2(0f, 110f),
                new Vector2(900f, 110f),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f));

            CompletionUI completion =
                CreateCompletionUI(
                    canvas.transform,
                    true);

            return new Level5UI
            {
                title = title,
                objective = objective,
                timer = timer,
                prompt = prompt,
                progressFill = progressFill,
                completionPanel =
                    completion.panel.gameObject,
                completionTitle = completion.title,
                completionSubtitle =
                    completion.subtitle,
                menuButton = completion.menuButton,
                restartButton =
                    completion.restartButton
            };
        }

        private static Canvas CreateCanvas(
            Transform parent,
            string name,
            Camera cameraComponent)
        {
            GameObject canvasObject =
                new GameObject(name);

            canvasObject.transform.SetParent(
                parent,
                false);

            Canvas canvas =
                canvasObject.AddComponent<Canvas>();

            canvas.renderMode =
                RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cameraComponent;
            canvas.planeDistance = 0.65f;
            canvas.sortingOrder = 100;

            CanvasScaler scaler =
                canvasObject.AddComponent<
                    CanvasScaler>();

            scaler.uiScaleMode =
                CanvasScaler.ScaleMode
                    .ScaleWithScreenSize;

            scaler.referenceResolution =
                new Vector2(1920f, 1080f);

            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<
                GraphicRaycaster>();

            Type trackedRaycaster =
                Type.GetType(
                    "UnityEngine.XR.Interaction.Toolkit.UI." +
                    "TrackedDeviceGraphicRaycaster, " +
                    "Unity.XR.Interaction.Toolkit");

            if (trackedRaycaster != null)
            {
                canvasObject.AddComponent(
                    trackedRaycaster);
            }

            canvasObject.AddComponent<
                NPFixedCanvasCameraBinder>();

            return canvas;
        }

        private static Image CreatePanel(
            Transform parent,
            string name,
            Vector2 anchoredPosition,
            Vector2 size,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Color color)
        {
            GameObject panelObject =
                new GameObject(
                    name,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image));

            panelObject.transform.SetParent(
                parent,
                false);

            RectTransform rect =
                panelObject.GetComponent<
                    RectTransform>();

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = anchorMin;
            rect.anchoredPosition =
                anchoredPosition;
            rect.sizeDelta = size;

            Image image =
                panelObject.GetComponent<Image>();

            image.color = color;
            image.raycastTarget = false;

            return image;
        }

        private static TMP_Text CreateText(
            Transform parent,
            string name,
            string text,
            float fontSize,
            FontStyles style,
            TextAlignmentOptions alignment,
            Vector2 anchoredPosition,
            Vector2 size,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            GameObject textObject =
                new GameObject(
                    name,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(TextMeshProUGUI));

            textObject.transform.SetParent(
                parent,
                false);

            RectTransform rect =
                textObject.GetComponent<
                    RectTransform>();

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = anchorMin;
            rect.anchoredPosition =
                anchoredPosition;
            rect.sizeDelta = size;

            TextMeshProUGUI textComponent =
                textObject.GetComponent<
                    TextMeshProUGUI>();

            textComponent.text = text;
            textComponent.fontSize = fontSize;
            textComponent.fontStyle = style;
            textComponent.alignment = alignment;
            textComponent.color = Color.white;
            textComponent.textWrappingMode =
                TextWrappingModes.Normal;
            textComponent.raycastTarget = false;

            return textComponent;
        }

        private static Image CreateProgressBar(
            Transform parent,
            Vector2 anchoredPosition,
            Vector2 size)
        {
            Image background = CreatePanel(
                parent,
                "ProgressBackground",
                anchoredPosition,
                size,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Color(1f, 1f, 1f, 0.18f));

            GameObject fillObject =
                new GameObject(
                    "ProgressFill",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image));

            fillObject.transform.SetParent(
                background.transform,
                false);

            RectTransform fillRect =
                fillObject.GetComponent<
                    RectTransform>();

            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(3f, 3f);
            fillRect.offsetMax = new Vector2(-3f, -3f);

            Image fill =
                fillObject.GetComponent<Image>();

            fill.color =
                new Color(0.20f, 0.78f, 1f, 1f);
            fill.type = Image.Type.Filled;
            fill.fillMethod =
                Image.FillMethod.Horizontal;
            fill.fillOrigin = 0;
            fill.fillAmount = 0f;
            fill.raycastTarget = false;

            return fill;
        }

        private static CompletionUI CreateCompletionUI(
            Transform parent,
            bool createButtons)
        {
            Image overlay = CreatePanel(
                parent,
                "CompletionPanel",
                Vector2.zero,
                Vector2.zero,
                Vector2.zero,
                Vector2.one,
                new Color(0.015f, 0.025f, 0.045f, 0.97f));

            RectTransform overlayRect =
                overlay.GetComponent<RectTransform>();

            overlayRect.pivot =
                new Vector2(0.5f, 0.5f);
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;

            TMP_Text title = CreateText(
                overlay.transform,
                "CompletionTitle",
                "¡Felicitaciones!",
                68f,
                FontStyles.Bold,
                TextAlignmentOptions.Center,
                new Vector2(0f, 170f),
                new Vector2(1300f, 100f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f));

            title.color =
                new Color(0.42f, 1f, 0.72f);

            TMP_Text subtitle = CreateText(
                overlay.transform,
                "CompletionSubtitle",
                "Completaste el nivel",
                38f,
                FontStyles.Normal,
                TextAlignmentOptions.Center,
                new Vector2(0f, 40f),
                new Vector2(1200f, 170f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f));

            TMP_Text countdown = null;
            Button menuButton = null;
            Button restartButton = null;

            if (createButtons)
            {
                menuButton = CreateButton(
                    overlay.transform,
                    "MenuButton",
                    "Volver al menú",
                    new Vector2(-190f, -155f));

                restartButton = CreateButton(
                    overlay.transform,
                    "RestartButton",
                    "Repetir Nivel 5",
                    new Vector2(190f, -155f));
            }
            else
            {
                countdown = CreateText(
                    overlay.transform,
                    "Countdown",
                    "El Nivel 5 comenzará en 5 segundos",
                    30f,
                    FontStyles.Normal,
                    TextAlignmentOptions.Center,
                    new Vector2(0f, -125f),
                    new Vector2(1000f, 70f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f));
            }

            overlay.gameObject.SetActive(false);

            return new CompletionUI
            {
                panel = overlay,
                title = title,
                subtitle = subtitle,
                countdown = countdown,
                menuButton = menuButton,
                restartButton = restartButton
            };
        }

        private static Button CreateButton(
            Transform parent,
            string name,
            string label,
            Vector2 anchoredPosition)
        {
            GameObject buttonObject =
                new GameObject(
                    name,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image),
                    typeof(Button));

            buttonObject.transform.SetParent(
                parent,
                false);

            RectTransform rect =
                buttonObject.GetComponent<
                    RectTransform>();

            rect.anchorMin =
                new Vector2(0.5f, 0.5f);
            rect.anchorMax =
                new Vector2(0.5f, 0.5f);
            rect.pivot =
                new Vector2(0.5f, 0.5f);
            rect.anchoredPosition =
                anchoredPosition;
            rect.sizeDelta =
                new Vector2(330f, 86f);

            Image image =
                buttonObject.GetComponent<Image>();

            image.color =
                new Color(0.12f, 0.42f, 0.58f, 1f);

            Sprite sprite =
                AssetDatabase.LoadAssetAtPath<Sprite>(
                    RootPath +
                    "/UI/Sprites/button_blue.png");

            if (sprite != null)
            {
                image.sprite = sprite;
                image.type = Image.Type.Sliced;
            }

            Button button =
                buttonObject.GetComponent<Button>();

            button.targetGraphic = image;

            TMP_Text buttonText = CreateText(
                buttonObject.transform,
                "Label",
                label,
                27f,
                FontStyles.Bold,
                TextAlignmentOptions.Center,
                Vector2.zero,
                Vector2.zero,
                Vector2.zero,
                Vector2.one);

            RectTransform labelRect =
                buttonText.GetComponent<
                    RectTransform>();

            labelRect.pivot =
                new Vector2(0.5f, 0.5f);
            labelRect.offsetMin =
                new Vector2(16f, 8f);
            labelRect.offsetMax =
                new Vector2(-16f, -8f);

            buttonText.raycastTarget = false;
            return button;
        }

        private static void PrepareAssetImporters()
        {
            EnsureFolder(GeneratedPath);
            EnsureFolder(MaterialsPath);
            EnsureFolder(ControllersPath);

            string spritesPath =
                RootPath + "/UI/Sprites";

            if (Directory.Exists(spritesPath))
            {
                foreach (string file in
                         Directory.GetFiles(
                             spritesPath,
                             "*.png",
                             SearchOption.TopDirectoryOnly))
                {
                    string normalized =
                        file.Replace("\\", "/");

                    TextureImporter importer =
                        AssetImporter.GetAtPath(
                            normalized)
                        as TextureImporter;

                    if (importer == null)
                    {
                        continue;
                    }

                    bool changed = false;

                    if (importer.textureType !=
                        TextureImporterType.Sprite)
                    {
                        importer.textureType =
                            TextureImporterType.Sprite;
                        changed = true;
                    }

                    if (importer.spritePixelsPerUnit != 100f)
                    {
                        importer.spritePixelsPerUnit = 100f;
                        changed = true;
                    }

                    if (changed)
                    {
                        importer.SaveAndReimport();
                    }
                }
            }

            AssetDatabase.Refresh();
        }

        private static void AddBuildScenesInternal()
        {
            List<EditorBuildSettingsScene> scenes =
                EditorBuildSettings.scenes.ToList();

            AddOrEnableScene(
                scenes,
                Scene4Path,
                afterSceneName: "Nivel3");

            AddOrEnableScene(
                scenes,
                Scene5Path,
                afterSceneName: "Nivel4");

            EditorBuildSettings.scenes =
                scenes.ToArray();
        }

        private static void AddOrEnableScene(
            List<EditorBuildSettingsScene> scenes,
            string scenePath,
            string afterSceneName)
        {
            int existingIndex =
                scenes.FindIndex(scene =>
                    string.Equals(
                        scene.path,
                        scenePath,
                        StringComparison.OrdinalIgnoreCase));

            if (existingIndex >= 0)
            {
                EditorBuildSettingsScene existing =
                    scenes[existingIndex];

                scenes[existingIndex] =
                    new EditorBuildSettingsScene(
                        existing.path,
                        true);

                return;
            }

            EditorBuildSettingsScene newScene =
                new EditorBuildSettingsScene(
                    scenePath,
                    true);

            int afterIndex =
                scenes.FindIndex(scene =>
                    string.Equals(
                        Path.GetFileNameWithoutExtension(
                            scene.path),
                        afterSceneName,
                        StringComparison.OrdinalIgnoreCase));

            if (afterIndex >= 0)
            {
                scenes.Insert(
                    afterIndex + 1,
                    newScene);
            }
            else
            {
                scenes.Add(newScene);
            }
        }

        private static string DetectMenuSceneName()
        {
            string menu = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene =>
                    Path.GetFileNameWithoutExtension(
                        scene.path))
                .FirstOrDefault(name =>
                    name.IndexOf(
                        "menu",
                        StringComparison.OrdinalIgnoreCase) >= 0);

            return !string.IsNullOrEmpty(menu)
                ? menu
                : "Nivel1";
        }

        private static void EnsureEventSystem(
            Scene scene)
        {
            EventSystem existing = scene
                .GetRootGameObjects()
                .SelectMany(rootObject =>
                    rootObject.GetComponentsInChildren<
                        EventSystem>(true))
                .FirstOrDefault();

            if (existing != null)
            {
                return;
            }

            GameObject eventObject =
                new GameObject("EventSystem");

            SceneManager.MoveGameObjectToScene(
                eventObject,
                scene);

            eventObject.AddComponent<EventSystem>();

            Type xrUiModule =
                Type.GetType(
                    "UnityEngine.XR.Interaction.Toolkit.UI." +
                    "XRUIInputModule, " +
                    "Unity.XR.Interaction.Toolkit");

            Type inputSystemModule =
                Type.GetType(
                    "UnityEngine.InputSystem.UI." +
                    "InputSystemUIInputModule, " +
                    "Unity.InputSystem");

            if (xrUiModule != null)
            {
                eventObject.AddComponent(
                    xrUiModule);
            }
            else if (inputSystemModule != null)
            {
                eventObject.AddComponent(
                    inputSystemModule);
            }
            else
            {
                eventObject.AddComponent<
                    StandaloneInputModule>();
            }
        }

        private static GameObject FindSceneObject(
            Scene scene,
            string exactName)
        {
            foreach (GameObject rootObject in
                     scene.GetRootGameObjects())
            {
                Transform match =
                    FindRecursive(
                        rootObject.transform,
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

        private static Transform CreateChild(
            Transform parent,
            string name)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent, false);
            return child.transform;
        }

        private static void EnsureFolder(
            string assetFolder)
        {
            if (AssetDatabase.IsValidFolder(assetFolder))
            {
                return;
            }

            string parent =
                Path.GetDirectoryName(assetFolder)
                    ?.Replace("\\", "/");

            string folderName =
                Path.GetFileName(assetFolder);

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

        private sealed class Level4UI
        {
            public TMP_Text title;
            public TMP_Text objective;
            public TMP_Text timer;
            public TMP_Text best;
            public TMP_Text prompt;
            public Image progressFill;
            public GameObject completionPanel;
            public TMP_Text completionTitle;
            public TMP_Text completionSubtitle;
            public TMP_Text countdown;
        }

        private sealed class Level5UI
        {
            public TMP_Text title;
            public TMP_Text objective;
            public TMP_Text timer;
            public TMP_Text prompt;
            public Image progressFill;
            public GameObject completionPanel;
            public TMP_Text completionTitle;
            public TMP_Text completionSubtitle;
            public Button menuButton;
            public Button restartButton;
        }

        private sealed class CompletionUI
        {
            public Image panel;
            public TMP_Text title;
            public TMP_Text subtitle;
            public TMP_Text countdown;
            public Button menuButton;
            public Button restartButton;
        }
    }
}
#endif
