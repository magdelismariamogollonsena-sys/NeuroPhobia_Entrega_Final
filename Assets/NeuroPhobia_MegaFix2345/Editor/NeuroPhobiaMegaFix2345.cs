#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NeuroPhobia.Level45;
using NeuroPhobia.Level45.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class NeuroPhobiaMegaFix2345
{
    private const string Scene2Path = "Assets/Scenes/Nivel2.unity";
    private const string Scene3Path = "Assets/Scenes/Nivel3.unity";
    private const string Scene4Path = "Assets/Scenes/Nivel4.unity";
    private const string Scene5Path = "Assets/Scenes/Nivel5.unity";

    private const string N2RootName = "NP_ENV_NIVEL2_PARQUE_GRANDE_FINAL";
    private const string N3RootName = "NP_ENV_NIVEL3_SOTANO_ENORME_FINAL";

    private const string N2Assets = "Assets/NeuroPhobia_Level2_ParkFinal";
    private const string N3Assets = "Assets/NeuroPhobia_Level3_BasementFinal";
    private const string L45Root = "Assets/NeuroPhobia_Level45";
    private const string FactoryModels = L45Root + "/Art/Factory/Models/";
    private const string FactoryPolish = L45Root + "/Art/Polish/Textures/";

    [MenuItem(
        "Tools/NeuroPhobia/MEGA ARREGLO NIVELES 2-5/1. APLICAR TODO 2-5",
        priority = 600)]
    public static void ApplyAll()
    {
        if (!CanEdit()) return;

        try
        {
            ApplyLevel2();
            ApplyLevel3();
            ApplyLevel4();
            ApplyLevel5();
            EnsureBuildScenes();

            EditorUtility.DisplayDialog(
                "NeuroPhobia",
                "MEGA ARREGLO COMPLETADO.\n\n" +
                "Nivel 2: parque sin agua y con mucha más vegetación.\n" +
                "Nivel 3: sótano pequeño desactivado y sótano grande mucho más decorado.\n" +
                "Nivel 4: casa enorme organizada con exposición repartida.\n" +
                "Nivel 5: fábrica enorme con recorrido largo, 4 orbes y 3 arañas.\n\n" +
                "Se crearon respaldos antes de modificar las escenas.\n\n" +
                "Prueba cada nivel por separado empezando por Nivel2.",
                "Aceptar");
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            EditorUtility.DisplayDialog(
                "NeuroPhobia",
                "El mega arreglo se detuvo por un error.\n\n" +
                ex.Message +
                "\n\nAbre Console y envía el PRIMER error rojo CS... si aparece.",
                "Aceptar");
        }
    }

    [MenuItem(
        "Tools/NeuroPhobia/MEGA ARREGLO NIVELES 2-5/2. Nivel 2 - quitar agua y poner MUCHOS arboles",
        priority = 601)]
    public static void MenuLevel2()
    {
        if (!CanEdit()) return;
        ApplyLevel2();
        EditorUtility.DisplayDialog(
            "NeuroPhobia",
            "Nivel 2 corregido: agua visual eliminada y vegetación reforzada.",
            "Aceptar");
    }

    [MenuItem(
        "Tools/NeuroPhobia/MEGA ARREGLO NIVELES 2-5/3. Nivel 3 - eliminar sotano pequeno y decorar ENORME",
        priority = 602)]
    public static void MenuLevel3()
    {
        if (!CanEdit()) return;
        ApplyLevel3();
        EditorUtility.DisplayDialog(
            "NeuroPhobia",
            "Nivel 3 corregido: el sótano pequeño quedó fuera y el grande fue llenado con más decoración.",
            "Aceptar");
    }

    [MenuItem(
        "Tools/NeuroPhobia/MEGA ARREGLO NIVELES 2-5/4. Nivel 4 - CASA ENORME FINAL",
        priority = 603)]
    public static void MenuLevel4()
    {
        if (!CanEdit()) return;
        ApplyLevel4();
        EditorUtility.DisplayDialog(
            "NeuroPhobia",
            "Nivel 4 actualizado: casa enorme y recorrido de exposición repartido.",
            "Aceptar");
    }

    [MenuItem(
        "Tools/NeuroPhobia/MEGA ARREGLO NIVELES 2-5/5. Nivel 5 - FABRICA ENORME FINAL",
        priority = 604)]
    public static void MenuLevel5()
    {
        if (!CanEdit()) return;
        ApplyLevel5();
        EditorUtility.DisplayDialog(
            "NeuroPhobia",
            "Nivel 5 actualizado: fábrica enorme, recorrido largo, cuatro orbes y tres arañas.",
            "Aceptar");
    }

    private static bool CanEdit()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog(
                "NeuroPhobia",
                "Detén Play antes de ejecutar este comando.",
                "Aceptar");
            return false;
        }

        return true;
    }

    // ============================================================
    // NIVEL 2
    // ============================================================

    private static void ApplyLevel2()
    {
        BackupScene(Scene2Path, "Assets/Scenes/Nivel2_BACKUP_MegaFix.unity");

        Scene scene = EditorSceneManager.OpenScene(
            Scene2Path,
            OpenSceneMode.Single);

        GameObject park = FindSceneObject(scene, N2RootName);

        if (park == null)
        {
            NeuroPhobiaNivel2ParqueGrandeFinal.Build();
            scene = EditorSceneManager.OpenScene(
                Scene2Path,
                OpenSceneMode.Single);
            park = FindSceneObject(scene, N2RootName);
        }

        if (park == null)
            throw new Exception("No pude crear/encontrar el parque grande del Nivel 2.");

        DisableWaterLikeObjects(scene);

        Transform trees = FindOrCreateChild(
            park.transform,
            "02_ARBOLES");

        Transform bushes = FindOrCreateChild(
            park.transform,
            "03_ARBUSTOS_FLORES");

        Transform extraTrees = ReplaceChild(
            trees,
            "EXTRA_ARBOLES_MEGA");

        Transform extraBushes = ReplaceChild(
            bushes,
            "EXTRA_VEGETACION_MEGA");

        string[] treeAssets =
        {
            N2Assets + "/Models/Nature/tree_oak.fbx",
            N2Assets + "/Models/Nature/tree_detailed.fbx",
            N2Assets + "/Models/Nature/tree_default.fbx",
            N2Assets + "/Models/Nature/tree_tall.fbx",
            N2Assets + "/Models/Nature/tree_pineTallA.fbx",
            N2Assets + "/Models/Nature/tree_pineRoundC.fbx",
            N2Assets + "/Models/Nature/tree_fat.fbx",
            N2Assets + "/Models/Nature/tree_small.fbx"
        };

        // 48 árboles adicionales en dos anillos, dejando el centro despejado.
        for (int i = 0; i < 48; i++)
        {
            float angle = i * (360f / 48f);
            float radius =
                i % 2 == 0
                    ? 25.5f
                    : 21.0f;

            float radians = angle * Mathf.Deg2Rad;

            Vector3 position = new Vector3(
                Mathf.Sin(radians) * radius,
                0f,
                Mathf.Cos(radians) * radius * 0.82f);

            position += new Vector3(
                Mathf.Sin(i * 2.31f) * 1.4f,
                0f,
                Mathf.Cos(i * 1.73f) * 1.2f);

            PlaceModel(
                treeAssets[i % treeAssets.Length],
                "Arbol_Extra_" + (i + 1),
                extraTrees,
                position,
                new Vector3(0f, (i * 47f) % 360f, 0f),
                3.5f + (i % 6) * 0.55f,
                true,
                null);
        }

        string[] bushAssets =
        {
            N2Assets + "/Models/Nature/plant_bush.fbx",
            N2Assets + "/Models/Nature/plant_bushDetailed.fbx",
            N2Assets + "/Models/Nature/plant_bushLarge.fbx"
        };

        for (int i = 0; i < 28; i++)
        {
            float angle = i * (360f / 28f) + 6f;
            float radians = angle * Mathf.Deg2Rad;
            float radius = 17.2f + (i % 3) * 1.2f;

            Vector3 position = new Vector3(
                Mathf.Sin(radians) * radius,
                0f,
                Mathf.Cos(radians) * radius * 0.84f);

            PlaceModel(
                bushAssets[i % bushAssets.Length],
                "Arbusto_Extra_" + (i + 1),
                extraBushes,
                position,
                new Vector3(0f, (i * 61f) % 360f, 0f),
                1.3f + (i % 4) * 0.25f,
                false,
                null);
        }

        EditorUtility.SetDirty(park);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
    }

    private static void DisableWaterLikeObjects(Scene scene)
    {
        string[] waterWords =
        {
            "water", "agua", "river", "rio", "río",
            "lake", "laguna", "pond", "pool", "ocean"
        };

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Renderer[] renderers =
                root.GetComponentsInChildren<Renderer>(true);

            foreach (Renderer renderer in renderers)
            {
                if (renderer == null) continue;

                string combined =
                    BuildPath(renderer.transform).ToLowerInvariant();

                foreach (Material material in renderer.sharedMaterials)
                {
                    if (material != null)
                        combined += " " + material.name.ToLowerInvariant();
                }

                bool looksLikeWater =
                    waterWords.Any(word => combined.Contains(word));

                if (!looksLikeWater)
                    continue;

                if (IsProtectedGameplay(renderer.transform))
                    continue;

                renderer.gameObject.SetActive(false);
                EditorUtility.SetDirty(renderer.gameObject);
            }
        }
    }

    // ============================================================
    // NIVEL 3
    // ============================================================

    private static void ApplyLevel3()
    {
        BackupScene(Scene3Path, "Assets/Scenes/Nivel3_BACKUP_MegaFix.unity");

        Scene scene = EditorSceneManager.OpenScene(
            Scene3Path,
            OpenSceneMode.Single);

        GameObject basement = FindSceneObject(scene, N3RootName);

        if (basement == null)
        {
            NeuroPhobiaNivel3SotanoEnormeFinal.Build();
            scene = EditorSceneManager.OpenScene(
                Scene3Path,
                OpenSceneMode.Single);
            basement = FindSceneObject(scene, N3RootName);
        }

        if (basement == null)
            throw new Exception("No pude crear/encontrar el sótano enorme del Nivel 3.");

        DisableOldSmallBasement(scene, basement);

        Transform mega = ReplaceChild(
            basement.transform,
            "07_DECORACION_MEGA_EXTRA");

        Transform storage = NewGroup("ALMACEN", mega);
        Transform machines = NewGroup("MAQUINAS", mega);
        Transform pipes = NewGroup("TUBERIAS", mega);
        Transform details = NewGroup("DETALLES", mega);
        Transform lights = NewGroup("LUCES_EXTRA", mega);

        Material factoryMaterial = GetFactoryPaletteMaterial();
        Material metalDark = MakeMaterial(
            "Assets/NeuroPhobia_MegaFix2345/Generated/M_N3_MetalDark.mat",
            new Color(0.10f, 0.12f, 0.13f, 1f),
            0.20f,
            null,
            Vector2.one);

        Material floorMark = MakeMaterial(
            "Assets/NeuroPhobia_MegaFix2345/Generated/M_N3_Warning.mat",
            new Color(0.78f, 0.28f, 0.06f, 1f),
            0.10f,
            null,
            Vector2.one);

        // Cajas por todo el perímetro.
        string[] boxes =
        {
            FactoryModels + "box-large.fbx",
            FactoryModels + "box-small.fbx",
            FactoryModels + "box-wide.fbx"
        };

        Vector3[] boxClusters =
        {
            new Vector3(-27,0,-18), new Vector3(-25,0,-14),
            new Vector3(-27,0,-5), new Vector3(-25,0,2),
            new Vector3(-27,0,11), new Vector3(-24,0,18),
            new Vector3(26,0,-18), new Vector3(24,0,-13),
            new Vector3(27,0,-5), new Vector3(25,0,4),
            new Vector3(27,0,12), new Vector3(24,0,19)
        };

        for (int i = 0; i < boxClusters.Length; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                PlaceModel(
                    boxes[(i + j) % boxes.Length],
                    "CajaMega_" + i + "_" + j,
                    storage,
                    boxClusters[i] +
                        new Vector3(j * 1.1f, 0f, (j % 2) * 0.9f),
                    new Vector3(0f, (i * 31f + j * 57f) % 360f, 0f),
                    1.7f + j * 0.35f,
                    true,
                    factoryMaterial);
            }
        }

        // Estanterías y muebles industriales.
        for (int i = 0; i < 8; i++)
        {
            float z = -18f + i * 5.0f;

            PlaceModel(
                N3Assets + "/Models/Props/bookcaseClosedWide.fbx",
                "EstanteriaMega_Izq_" + (i + 1),
                storage,
                new Vector3(-29.2f, 0f, z),
                new Vector3(0f, 90f, 0f),
                3.0f,
                true,
                null);
        }

        string[] machineAssets =
        {
            FactoryModels + "machine.fbx",
            FactoryModels + "machine-window.fbx",
            FactoryModels + "machine-fortified.fbx",
            FactoryModels + "scanner-high.fbx",
            FactoryModels + "screen-wide.fbx"
        };

        Vector3[] machinePositions =
        {
            new Vector3(20,0,-18), new Vector3(21,0,-10),
            new Vector3(20,0,-1), new Vector3(21,0,8),
            new Vector3(20,0,17),
            new Vector3(-18,0,-19), new Vector3(-18,0,19)
        };

        for (int i = 0; i < machinePositions.Length; i++)
        {
            PlaceModel(
                machineAssets[i % machineAssets.Length],
                "MaquinaMega_" + (i + 1),
                machines,
                machinePositions[i],
                new Vector3(0f, i % 2 == 0 ? 180f : 0f, 0f),
                3.7f,
                true,
                factoryMaterial);
        }

        // Tuberías visibles por techo y paredes.
        for (int i = 0; i < 10; i++)
        {
            float z = -20f + i * 4.4f;

            GameObject pipe =
                PlaceModel(
                    FactoryModels + "pipe-large-long.fbx",
                    "TuberiaMega_Derecha_" + (i + 1),
                    pipes,
                    new Vector3(29.2f, 3.25f, z),
                    new Vector3(0f, 90f, 0f),
                    4.3f,
                    false,
                    factoryMaterial);

            if (pipe != null)
                pipe.transform.localScale *= 1.25f;
        }

        for (int i = 0; i < 8; i++)
        {
            float x = -24f + i * 7f;

            PlaceModel(
                FactoryModels + "pipe-large-bend.fbx",
                "TuberiaTecho_" + (i + 1),
                pipes,
                new Vector3(x, 3.8f, 20f),
                new Vector3(0f, i * 45f, 90f),
                3.5f,
                false,
                factoryMaterial);
        }

        // Señalización, barreras y piezas pequeñas.
        for (int i = 0; i < 14; i++)
        {
            float x = -23f + (i % 7) * 7.5f;
            float z = i < 7 ? -12f : 13f;

            PlaceModel(
                FactoryModels + "warning-traffic.fbx",
                "AdvertenciaMega_" + (i + 1),
                details,
                new Vector3(x, 0f, z),
                new Vector3(0f, (i % 2) * 180f, 0f),
                1.4f,
                true,
                factoryMaterial);
        }

        // Marcas de piso para romper áreas grandes vacías.
        for (int i = 0; i < 12; i++)
        {
            CreateBox(
                "MarcaPiso_" + (i + 1),
                details,
                new Vector3(-17f + i * 3.1f, 0.026f, -7.5f),
                new Vector3(1.6f, 0.045f, 0.35f),
                floorMark,
                false);
        }

        // Más iluminación en todo el espacio.
        Vector3[] lightPositions =
        {
            new Vector3(-24,3.7f,-17), new Vector3(-12,3.7f,-17),
            new Vector3(0,3.7f,-17), new Vector3(12,3.7f,-17),
            new Vector3(24,3.7f,-17),
            new Vector3(-24,3.7f,0), new Vector3(-12,3.7f,0),
            new Vector3(0,3.7f,0), new Vector3(12,3.7f,0),
            new Vector3(24,3.7f,0),
            new Vector3(-24,3.7f,17), new Vector3(-12,3.7f,17),
            new Vector3(0,3.7f,17), new Vector3(12,3.7f,17),
            new Vector3(24,3.7f,17)
        };

        for (int i = 0; i < lightPositions.Length; i++)
        {
            AddPointLight(
                lights,
                "LuzIndustrialExtra_" + (i + 1),
                lightPositions[i],
                i % 5 == 0
                    ? new Color(1f, 0.20f, 0.10f)
                    : new Color(0.52f, 0.66f, 0.82f),
                i % 5 == 0 ? 1.1f : 0.75f,
                8.0f);
        }

        EditorUtility.SetDirty(basement);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
    }

    private static void DisableOldSmallBasement(
        Scene scene,
        GameObject keep)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root == keep) continue;

            string lower = root.name.ToLowerInvariant();

            bool oldBasement =
                lower == "np_env_nivel3_sotano" ||
                lower.Contains("np_env_nivel3_sotano_pequeno") ||
                (lower.Contains("nivel3") &&
                 lower.Contains("sotano") &&
                 !lower.Contains("enorme"));

            if (!oldBasement)
                continue;

            RescueProtectedTopChildren(root.transform, scene);

            root.name =
                "NP_OLD_N3_SOTANO_PEQUENO_DESACTIVADO";

            root.SetActive(false);
            EditorUtility.SetDirty(root);
        }
    }

    private static void RescueProtectedTopChildren(
        Transform oldRoot,
        Scene scene)
    {
        List<Transform> move =
            new List<Transform>();

        foreach (Transform child in oldRoot)
        {
            if (ContainsProtectedGameplay(child))
                move.Add(child);
        }

        foreach (Transform child in move)
        {
            Undo.SetTransformParent(
                child,
                null,
                "Rescatar gameplay Nivel3");

            SceneManager.MoveGameObjectToScene(
                child.gameObject,
                scene);
        }
    }

    // ============================================================
    // NIVEL 4
    // ============================================================

    private static void ApplyLevel4()
    {
        BackupScene(Scene4Path, "Assets/Scenes/Nivel4_BACKUP_MegaFix.unity");

        Scene scene = EditorSceneManager.OpenScene(
            Scene4Path,
            OpenSceneMode.Single);

        NPLevel4Controller controller =
            FindComponentInScene<NPLevel4Controller>(scene);

        if (controller == null)
        {
            NeuroPhobiaLevel45Installer.ConfigureLevel4();

            scene = EditorSceneManager.OpenScene(
                Scene4Path,
                OpenSceneMode.Single);
        }

        // Reemplaza SOLO el entorno visual; conserva gameplay y controlador.
        NeuroPhobiaNivel4CasaEnormeFinal.Build();

        scene = EditorSceneManager.OpenScene(
            Scene4Path,
            OpenSceneMode.Single);

        controller =
            FindComponentInScene<NPLevel4Controller>(scene);

        if (controller == null)
            throw new Exception("No encontré NPLevel4Controller después de configurar Nivel 4.");

        GameObject p1 = FindSceneObject(scene, "L4_Exposure_1");
        GameObject p2 = FindSceneObject(scene, "L4_Exposure_2");
        GameObject p3 = FindSceneObject(scene, "L4_Exposure_3");

        if (p1 != null)
            p1.transform.position = new Vector3(-8.5f, 0.42f, -5.0f);

        if (p2 != null)
            p2.transform.position = new Vector3(-9.7f, 0.42f, 7.3f);

        if (p3 != null)
            p3.transform.position = new Vector3(9.6f, 0.42f, 6.3f);

        Vector3[] route =
        {
            new Vector3(2.7f, 0.06f, 2.2f),
            new Vector3(4.5f, 0.06f, 4.8f),
            new Vector3(0.8f, 0.06f, 6.0f),
            new Vector3(-2.2f, 0.06f, 3.5f)
        };

        for (int i = 0; i < route.Length; i++)
        {
            GameObject waypoint =
                FindSceneObject(
                    scene,
                    "L4_Route_" + (i + 1));

            if (waypoint != null)
                waypoint.transform.position = route[i];
        }

        GameObject spider =
            FindSceneObject(scene, "NP_Spider_Level4");

        if (spider != null)
            spider.transform.position = route[0];

        controller.secondsPerPoint = 10f;
        controller.activationRadius = 1.75f;
        controller.nextSceneName = "Nivel5";
        controller.transitionDelay = 5f;

        EnsureSelectorActive(scene);

        EditorUtility.SetDirty(controller);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
    }

    // ============================================================
    // NIVEL 5
    // ============================================================

    private static void ApplyLevel5()
    {
        BackupScene(Scene5Path, "Assets/Scenes/Nivel5_BACKUP_MegaFix.unity");

        Scene scene = EditorSceneManager.OpenScene(
            Scene5Path,
            OpenSceneMode.Single);

        NPLevel5Controller controller =
            FindComponentInScene<NPLevel5Controller>(scene);

        if (controller == null)
        {
            NeuroPhobiaLevel45Installer.ConfigureLevel5();

            scene = EditorSceneManager.OpenScene(
                Scene5Path,
                OpenSceneMode.Single);

            controller =
                FindComponentInScene<NPLevel5Controller>(scene);
        }

        if (controller == null)
            throw new Exception("No encontré NPLevel5Controller en Nivel 5.");

        GameObject root =
            FindSceneObject(scene, "NP_LEVEL5_FINAL");

        if (root == null)
            throw new Exception("No encontré NP_LEVEL5_FINAL.");

        DestroyDirectChild(root.transform, "Environment");
        DestroyDirectChild(root.transform, "FactoryProps");

        Transform env = FindOrCreateChild(
            root.transform,
            "Environment");

        Transform props = FindOrCreateChild(
            root.transform,
            "FactoryProps");

        Transform zones = NewGroup(
            "MEGA_FACTORY_ZONES",
            props);

        Transform machines = NewGroup(
            "MAQUINAS",
            zones);

        Transform conveyors = NewGroup(
            "TRANSPORTADORES",
            zones);

        Transform catwalks = NewGroup(
            "PASARELAS",
            zones);

        Transform pipes = NewGroup(
            "TUBERIAS",
            zones);

        Transform storage = NewGroup(
            "ALMACEN",
            zones);

        Transform lights = NewGroup(
            "LUCES",
            env);

        Material floorMat = MakeMaterial(
            "Assets/NeuroPhobia_MegaFix2345/Generated/M_L5_Floor.mat",
            new Color(0.40f, 0.42f, 0.43f, 1f),
            0.06f,
            FactoryPolish + "factory_floor_concrete.png",
            new Vector2(12f, 8f));

        Material wallMat = MakeMaterial(
            "Assets/NeuroPhobia_MegaFix2345/Generated/M_L5_Wall.mat",
            new Color(0.45f, 0.48f, 0.52f, 1f),
            0.05f,
            FactoryPolish + "factory_wall_panels.png",
            new Vector2(10f, 4f));

        Material ceilingMat = MakeMaterial(
            "Assets/NeuroPhobia_MegaFix2345/Generated/M_L5_Ceiling.mat",
            new Color(0.28f, 0.30f, 0.32f, 1f),
            0.04f,
            FactoryPolish + "factory_ceiling_panels.png",
            new Vector2(10f, 6f));

        Material warningMat = MakeMaterial(
            "Assets/NeuroPhobia_MegaFix2345/Generated/M_L5_Warning.mat",
            new Color(0.92f, 0.34f, 0.06f, 1f),
            0.12f,
            null,
            Vector2.one);

        Material factoryMaterial = GetFactoryPaletteMaterial();

        // Fábrica de 60 x 44 metros.
        CreateBox(
            "PISO_FABRICA_60x44",
            env,
            new Vector3(0f, -0.10f, 0f),
            new Vector3(60f, 0.20f, 44f),
            floorMat,
            true);

        CreateBox(
            "TECHO_FABRICA",
            env,
            new Vector3(0f, 6.2f, 0f),
            new Vector3(60f, 0.20f, 44f),
            ceilingMat,
            true);

        CreateBox(
            "Pared_Izquierda",
            env,
            new Vector3(-29.9f, 3.0f, 0f),
            new Vector3(0.25f, 6.0f, 44f),
            wallMat,
            true);

        CreateBox(
            "Pared_Derecha",
            env,
            new Vector3(29.9f, 3.0f, 0f),
            new Vector3(0.25f, 6.0f, 44f),
            wallMat,
            true);

        CreateBox(
            "Pared_Fondo",
            env,
            new Vector3(0f, 3.0f, 21.9f),
            new Vector3(60f, 6.0f, 0.25f),
            wallMat,
            true);

        CreateBox(
            "Pared_Frente_Izq",
            env,
            new Vector3(-18f, 3.0f, -21.9f),
            new Vector3(24f, 6.0f, 0.25f),
            wallMat,
            true);

        CreateBox(
            "Pared_Frente_Der",
            env,
            new Vector3(18f, 3.0f, -21.9f),
            new Vector3(24f, 6.0f, 0.25f),
            wallMat,
            true);

        // Columnas estructurales.
        for (int x = -24; x <= 24; x += 12)
        {
            for (int z = -15; z <= 15; z += 15)
            {
                PlaceModel(
                    FactoryModels + "structure-high.fbx",
                    "Estructura_" + x + "_" + z,
                    env,
                    new Vector3(x, 0f, z),
                    Vector3.zero,
                    5.0f,
                    true,
                    factoryMaterial);
            }
        }

        // Línea de transportadores a la izquierda.
        for (int i = 0; i < 7; i++)
        {
            PlaceModel(
                FactoryModels + "conveyor-long-sides.fbx",
                "Conveyor_Izq_" + (i + 1),
                conveyors,
                new Vector3(-17f, 0f, -15f + i * 5.0f),
                Vector3.zero,
                5.0f,
                true,
                factoryMaterial);
        }

        // Segunda línea horizontal.
        for (int i = 0; i < 6; i++)
        {
            PlaceModel(
                FactoryModels + "conveyor-long.fbx",
                "Conveyor_Fondo_" + (i + 1),
                conveyors,
                new Vector3(-10f + i * 4.5f, 0f, 13f),
                new Vector3(0f, 90f, 0f),
                4.5f,
                true,
                factoryMaterial);
        }

        string[] machineAssets =
        {
            FactoryModels + "machine.fbx",
            FactoryModels + "machine-window.fbx",
            FactoryModels + "machine-fortified.fbx",
            FactoryModels + "scanner-high.fbx",
            FactoryModels + "robot-arm-a.fbx",
            FactoryModels + "robot-arm-b.fbx"
        };

        Vector3[] machinePos =
        {
            new Vector3(14,0,-15), new Vector3(20,0,-14),
            new Vector3(14,0,-7), new Vector3(21,0,-5),
            new Vector3(15,0,4), new Vector3(22,0,5),
            new Vector3(14,0,13), new Vector3(22,0,14)
        };

        for (int i = 0; i < machinePos.Length; i++)
        {
            PlaceModel(
                machineAssets[i % machineAssets.Length],
                "Maquina_Fabrica_" + (i + 1),
                machines,
                machinePos[i],
                new Vector3(0f, i % 2 == 0 ? 180f : 0f, 0f),
                4.2f,
                true,
                factoryMaterial);
        }

        // Almacén de cajas.
        string[] boxAssets =
        {
            FactoryModels + "box-large.fbx",
            FactoryModels + "box-small.fbx",
            FactoryModels + "box-wide.fbx"
        };

        for (int i = 0; i < 20; i++)
        {
            float x = -26f + (i % 4) * 2.0f;
            float z = -16f + (i / 4) * 7.5f;

            PlaceModel(
                boxAssets[i % 3],
                "Caja_Fabrica_" + (i + 1),
                storage,
                new Vector3(x, 0f, z),
                new Vector3(0f, (i * 37f) % 360f, 0f),
                1.8f + (i % 3) * 0.35f,
                true,
                factoryMaterial);
        }

        // Pasarela central elevada.
        for (int i = 0; i < 6; i++)
        {
            PlaceModel(
                FactoryModels + "catwalk-straight.fbx",
                "Pasarela_" + (i + 1),
                catwalks,
                new Vector3(-10f + i * 4f, 3.0f, 0f),
                new Vector3(0f, 90f, 0f),
                4.0f,
                false,
                factoryMaterial);
        }

        PlaceModel(
            FactoryModels + "catwalk-stairs.fbx",
            "Escalera_Pasarela_1",
            catwalks,
            new Vector3(-13f, 0f, 0f),
            new Vector3(0f, 90f, 0f),
            5.5f,
            true,
            factoryMaterial);

        PlaceModel(
            FactoryModels + "catwalk-stairs.fbx",
            "Escalera_Pasarela_2",
            catwalks,
            new Vector3(13f, 0f, 0f),
            new Vector3(0f, -90f, 0f),
            5.5f,
            true,
            factoryMaterial);

        PlaceModel(
            FactoryModels + "crane.fbx",
            "Grua_Industrial",
            props,
            new Vector3(0f, 0f, 17f),
            Vector3.zero,
            10f,
            true,
            factoryMaterial);

        // Tuberías en pared y techo.
        for (int i = 0; i < 10; i++)
        {
            float z = -18f + i * 4f;

            PlaceModel(
                FactoryModels + "pipe-large-long.fbx",
                "Pipe_Derecha_" + (i + 1),
                pipes,
                new Vector3(28.8f, 4.4f, z),
                new Vector3(0f, 90f, 0f),
                4.0f,
                false,
                factoryMaterial);
        }

        for (int i = 0; i < 8; i++)
        {
            float x = -24f + i * 7f;

            PlaceModel(
                FactoryModels + "pipe-large-bend.fbx",
                "Pipe_Techo_" + (i + 1),
                pipes,
                new Vector3(x, 5.25f, -11f),
                new Vector3(0f, i * 45f, 90f),
                3.2f,
                false,
                factoryMaterial);
        }

        // Señalización de ruta.
        for (int i = 0; i < 10; i++)
        {
            CreateBox(
                "FranjaSeguridad_" + (i + 1),
                props,
                new Vector3(-12.5f + i * 2.8f, 0.026f, -3.5f),
                new Vector3(1.2f, 0.045f, 0.35f),
                warningMat,
                false);
        }

        // Iluminación industrial.
        Vector3[] lightPos =
        {
            new Vector3(-22,5.3f,-15), new Vector3(-8,5.3f,-15),
            new Vector3(8,5.3f,-15), new Vector3(22,5.3f,-15),
            new Vector3(-22,5.3f,0), new Vector3(-8,5.3f,0),
            new Vector3(8,5.3f,0), new Vector3(22,5.3f,0),
            new Vector3(-22,5.3f,15), new Vector3(-8,5.3f,15),
            new Vector3(8,5.3f,15), new Vector3(22,5.3f,15)
        };

        for (int i = 0; i < lightPos.Length; i++)
        {
            Color color =
                i % 4 == 0
                    ? new Color(0.25f, 0.55f, 1f)
                    : i % 4 == 2
                        ? new Color(1f, 0.34f, 0.10f)
                        : new Color(0.72f, 0.82f, 0.92f);

            AddPointLight(
                lights,
                "LuzFabrica_" + (i + 1),
                lightPos[i],
                color,
                1.55f,
                11f);
        }

        // Recorrido de orbes mucho más largo.
        SetPosition(scene, "L5_Orb_1", new Vector3(-21f, 0.48f, -12f));
        SetPosition(scene, "L5_Orb_2", new Vector3(-18f, 0.48f, 12f));
        SetPosition(scene, "L5_Orb_3", new Vector3(10f, 0.48f, 15f));
        SetPosition(scene, "L5_Orb_4", new Vector3(22f, 0.48f, 6f));
        SetPosition(scene, "L5_FinalZone", new Vector3(0f, 0.035f, 18.5f));

        Vector3[] routeA =
        {
            new Vector3(-24f,0.06f,-10f),
            new Vector3(-24f,0.06f,9f),
            new Vector3(-14f,0.06f,9f),
            new Vector3(-14f,0.06f,-10f)
        };

        Vector3[] routeB =
        {
            new Vector3(14f,0.06f,-9f),
            new Vector3(23f,0.06f,-9f),
            new Vector3(23f,0.06f,11f),
            new Vector3(14f,0.06f,11f)
        };

        Vector3[] routeC =
        {
            new Vector3(-4f,0.06f,15f),
            new Vector3(4f,0.06f,15f),
            new Vector3(4f,0.06f,20f),
            new Vector3(-4f,0.06f,20f)
        };

        SetRoute(scene, "L5_Route_A", routeA);
        SetRoute(scene, "L5_Route_B", routeB);
        SetRoute(scene, "L5_Route_C", routeC);

        SetPosition(scene, "NP_Spider_Level5_A", routeA[0]);
        SetPosition(scene, "NP_Spider_Level5_B", routeB[0]);
        SetPosition(scene, "NP_Spider_Level5_Central", routeC[0]);

        controller.orbCollectionRadius = 1.55f;
        controller.finalZoneRadius = 1.75f;
        controller.finalLookSeconds = 15f;

        EnsureSelectorActive(scene);

        EditorUtility.SetDirty(controller);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
    }

    // ============================================================
    // COMMON
    // ============================================================

    private static void BackupScene(
        string source,
        string backup)
    {
        if (!File.Exists(source))
            throw new FileNotFoundException(source);

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(backup) == null)
        {
            AssetDatabase.CopyAsset(source, backup);
            AssetDatabase.SaveAssets();
        }
    }

    private static void EnsureBuildScenes()
    {
        string[] desired =
        {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/Nivel1.unity",
            "Assets/Scenes/Nivel2.unity",
            "Assets/Scenes/Nivel3.unity",
            "Assets/Scenes/Nivel4.unity",
            "Assets/Scenes/Nivel5.unity"
        };

        List<EditorBuildSettingsScene> list =
            EditorBuildSettings.scenes.ToList();

        foreach (string path in desired)
        {
            if (!File.Exists(path))
                continue;

            EditorBuildSettingsScene existing =
                list.FirstOrDefault(
                    item =>
                        string.Equals(
                            item.path,
                            path,
                            StringComparison.OrdinalIgnoreCase));

            if (existing == null)
                list.Add(new EditorBuildSettingsScene(path, true));
            else
                existing.enabled = true;
        }

        EditorBuildSettings.scenes = list.ToArray();
    }

    private static void EnsureSelectorActive(Scene scene)
    {
        GameObject selector =
            FindSceneObject(
                scene,
                "NP_Seleccion_Modo_Control");

        if (selector != null)
        {
            selector.SetActive(true);
            EditorUtility.SetDirty(selector);
        }
    }

    private static void SetRoute(
        Scene scene,
        string prefix,
        Vector3[] positions)
    {
        for (int i = 0; i < positions.Length; i++)
        {
            GameObject point =
                FindSceneObject(
                    scene,
                    prefix + "_" + (i + 1));

            if (point != null)
            {
                point.transform.position = positions[i];
                EditorUtility.SetDirty(point);
            }
        }
    }

    private static void SetPosition(
        Scene scene,
        string objectName,
        Vector3 position)
    {
        GameObject go =
            FindSceneObject(scene, objectName);

        if (go != null)
        {
            go.transform.position = position;
            EditorUtility.SetDirty(go);
        }
    }

    private static T FindComponentInScene<T>(
        Scene scene)
        where T : Component
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            T found =
                root.GetComponentInChildren<T>(true);

            if (found != null)
                return found;
        }

        return null;
    }

    private static Transform FindOrCreateChild(
        Transform parent,
        string name)
    {
        Transform child = parent.Find(name);

        if (child != null)
            return child;

        return NewGroup(name, parent);
    }

    private static Transform ReplaceChild(
        Transform parent,
        string name)
    {
        Transform old = parent.Find(name);

        if (old != null)
            UnityEngine.Object.DestroyImmediate(
                old.gameObject);

        return NewGroup(name, parent);
    }

    private static Transform NewGroup(
        string name,
        Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    private static void DestroyDirectChild(
        Transform parent,
        string name)
    {
        Transform child = parent.Find(name);

        if (child != null)
            UnityEngine.Object.DestroyImmediate(
                child.gameObject);
    }

    private static GameObject CreateBox(
        string name,
        Transform parent,
        Vector3 localPosition,
        Vector3 localScale,
        Material material,
        bool collider)
    {
        GameObject go =
            GameObject.CreatePrimitive(
                PrimitiveType.Cube);

        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = localScale;

        Renderer renderer =
            go.GetComponent<Renderer>();

        if (renderer != null &&
            material != null)
        {
            renderer.sharedMaterial = material;
        }

        Collider col = go.GetComponent<Collider>();

        if (col != null)
            col.enabled = collider;

        return go;
    }

    private static GameObject PlaceModel(
        string assetPath,
        string objectName,
        Transform parent,
        Vector3 localPosition,
        Vector3 localEuler,
        float targetHorizontalSize,
        bool addCollider,
        Material overrideMaterial)
    {
        GameObject prefab =
            AssetDatabase.LoadAssetAtPath<GameObject>(
                assetPath);

        if (prefab == null)
        {
            Debug.LogWarning(
                "[MegaFix2345] No encontré modelo: " +
                assetPath);

            return null;
        }

        GameObject go =
            PrefabUtility.InstantiatePrefab(
                prefab) as GameObject;

        if (go == null)
            return null;

        go.name = objectName;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        go.transform.localRotation =
            Quaternion.Euler(localEuler);
        go.transform.localScale = Vector3.one;

        if (overrideMaterial != null)
        {
            foreach (Renderer renderer in
                     go.GetComponentsInChildren<Renderer>(true))
            {
                Material[] materials =
                    renderer.sharedMaterials;

                for (int i = 0;
                     i < materials.Length;
                     i++)
                {
                    materials[i] = overrideMaterial;
                }

                renderer.sharedMaterials = materials;
            }
        }

        Bounds bounds;

        if (TryGetBounds(go, out bounds))
        {
            float horizontal =
                Mathf.Max(
                    bounds.size.x,
                    bounds.size.z);

            if (horizontal > 0.0001f)
            {
                go.transform.localScale *=
                    targetHorizontalSize / horizontal;
            }
        }

        SnapToFloor(
            go,
            parent.position.y);

        if (addCollider)
            AddBoundsCollider(go);

        return go;
    }

    private static void SnapToFloor(
        GameObject go,
        float floorY)
    {
        Bounds bounds;

        if (!TryGetBounds(go, out bounds))
            return;

        go.transform.position +=
            Vector3.up *
            (floorY - bounds.min.y);
    }

    private static void AddBoundsCollider(
        GameObject go)
    {
        Collider[] existing =
            go.GetComponentsInChildren<Collider>(true);

        if (existing != null &&
            existing.Length > 0)
        {
            return;
        }

        Bounds bounds;

        if (!TryGetBounds(go, out bounds))
            return;

        BoxCollider box =
            go.AddComponent<BoxCollider>();

        box.center =
            go.transform.InverseTransformPoint(
                bounds.center);

        Vector3 scale =
            go.transform.lossyScale;

        box.size = new Vector3(
            bounds.size.x /
                Mathf.Max(
                    0.0001f,
                    Mathf.Abs(scale.x)),
            bounds.size.y /
                Mathf.Max(
                    0.0001f,
                    Mathf.Abs(scale.y)),
            bounds.size.z /
                Mathf.Max(
                    0.0001f,
                    Mathf.Abs(scale.z)));

        box.isTrigger = false;
    }

    private static bool TryGetBounds(
        GameObject go,
        out Bounds bounds)
    {
        Renderer[] renderers =
            go.GetComponentsInChildren<Renderer>(
                true);

        if (renderers == null ||
            renderers.Length == 0)
        {
            bounds =
                new Bounds(
                    go.transform.position,
                    Vector3.zero);

            return false;
        }

        bounds = renderers[0].bounds;

        for (int i = 1;
             i < renderers.Length;
             i++)
        {
            bounds.Encapsulate(
                renderers[i].bounds);
        }

        return true;
    }

    private static Material GetFactoryPaletteMaterial()
    {
        string path =
            "Assets/NeuroPhobia_MegaFix2345/Generated/M_FactoryPalette.mat";

        Texture2D texture =
            AssetDatabase.LoadAssetAtPath<Texture2D>(
                L45Root +
                "/Art/Factory/Textures/colormap.png");

        return MakeMaterial(
            path,
            Color.white,
            0.08f,
            texture != null
                ? AssetDatabase.GetAssetPath(texture)
                : null,
            Vector2.one);
    }

    private static Material MakeMaterial(
        string path,
        Color color,
        float smoothness,
        string texturePath,
        Vector2 tiling)
    {
        EnsureAssetFolder(
            Path.GetDirectoryName(path)
                .Replace("\\", "/"));

        Material material =
            AssetDatabase.LoadAssetAtPath<Material>(
                path);

        Shader shader =
            Shader.Find(
                "Universal Render Pipeline/Lit");

        if (shader == null)
            shader = Shader.Find("Standard");

        if (material == null)
        {
            material =
                new Material(shader);

            AssetDatabase.CreateAsset(
                material,
                path);
        }
        else
        {
            material.shader = shader;
        }

        Texture2D texture =
            !string.IsNullOrEmpty(texturePath)
                ? AssetDatabase.LoadAssetAtPath<Texture2D>(
                    texturePath)
                : null;

        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", color);
        else
            material.color = color;

        if (texture != null)
        {
            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture(
                    "_BaseMap",
                    texture);

                material.SetTextureScale(
                    "_BaseMap",
                    tiling);
            }
            else
            {
                material.mainTexture = texture;
                material.mainTextureScale = tiling;
            }
        }

        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat(
                "_Smoothness",
                smoothness);
        }

        EditorUtility.SetDirty(material);
        return material;
    }

    private static void AddPointLight(
        Transform parent,
        string name,
        Vector3 localPosition,
        Color color,
        float intensity,
        float range)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition =
            localPosition;

        Light light =
            go.AddComponent<Light>();

        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.shadows = LightShadows.Soft;
    }

    private static bool IsProtectedGameplay(
        Transform transform)
    {
        string path =
            BuildPath(transform)
                .ToLowerInvariant();

        string[] protectedWords =
        {
            "spider", "arana", "araña",
            "player", "desktop", "xr",
            "camera", "hud", "ui",
            "controller", "manager",
            "selector", "gaze", "mirada",
            "timer", "orbe", "orb",
            "eventsystem", "limite",
            "limit"
        };

        return protectedWords.Any(
            path.Contains);
    }

    private static bool ContainsProtectedGameplay(
        Transform transform)
    {
        if (IsProtectedGameplay(transform))
            return true;

        foreach (Transform child in transform)
        {
            if (ContainsProtectedGameplay(child))
                return true;
        }

        return false;
    }

    private static string BuildPath(
        Transform transform)
    {
        string path = transform.name;
        Transform parent = transform.parent;

        while (parent != null)
        {
            path =
                parent.name + "/" + path;

            parent = parent.parent;
        }

        return path;
    }

    private static GameObject FindSceneObject(
        Scene scene,
        string exactName)
    {
        foreach (GameObject root in
                 scene.GetRootGameObjects())
        {
            Transform found =
                FindRecursive(
                    root.transform,
                    exactName);

            if (found != null)
                return found.gameObject;
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
                StringComparison.OrdinalIgnoreCase))
        {
            return current;
        }

        foreach (Transform child in current)
        {
            Transform found =
                FindRecursive(
                    child,
                    exactName);

            if (found != null)
                return found;
        }

        return null;
    }

    private static void EnsureAssetFolder(
        string folder)
    {
        if (string.IsNullOrEmpty(folder) ||
            AssetDatabase.IsValidFolder(folder))
        {
            return;
        }

        string[] parts =
            folder.Split('/');

        string current = parts[0];

        for (int i = 1;
             i < parts.Length;
             i++)
        {
            string next =
                current + "/" + parts[i];

            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(
                    current,
                    parts[i]);
            }

            current = next;
        }
    }
}
#endif
