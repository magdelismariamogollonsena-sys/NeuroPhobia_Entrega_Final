#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class NeuroPhobiaNivel4CasaEnormeFinal
{
    private const string ScenePath = "Assets/Scenes/Nivel4.unity";
    private const string BackupPath = "Assets/Scenes/Nivel4_BACKUP_Antes_CasaEnorme.unity";
    private const string AssetRoot = "Assets/NeuroPhobia_Level4_Rebuild";
    private const string GeneratedRoot = AssetRoot + "/Generated";
    private const string MaterialsRoot = GeneratedRoot + "/Materials";
    private const string NewRootName = "NP_ENV_NIVEL4_CASA_ENORME_FINAL";

    private static readonly Dictionary<Material, Material> Converted =
        new Dictionary<Material, Material>();

    [MenuItem(
        "Tools/NeuroPhobia/Nivel 4/RECONSTRUIR CASA ENORME FINAL",
        priority = 220)]
    public static void Build()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog(
                "NeuroPhobia",
                "Detén Play antes de reconstruir el Nivel 4.",
                "Aceptar");
            return;
        }

        if (!File.Exists(ScenePath))
        {
            EditorUtility.DisplayDialog(
                "NeuroPhobia",
                "No encontré Assets/Scenes/Nivel4.unity",
                "Aceptar");
            return;
        }

        EnsureFolder(MaterialsRoot);

        // Copia de seguridad automática, sin sobrescribir una ya existente.
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(BackupPath) == null)
        {
            AssetDatabase.CopyAsset(ScenePath, BackupPath);
            AssetDatabase.SaveAssets();
        }

        Scene scene =
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        // Eliminar únicamente entornos visuales que nosotros conocemos.
        RemoveOldGeneratedVisuals(scene);

        GameObject level4Root =
            FindSceneObject(scene, "NP_LEVEL4_FINAL");

        Transform visualParent = null;

        // Si existe el nivel 4 original, quitamos SOLO sus dos secciones visuales.
        // Gameplay, SpiderRoutes, Controller, UI y exposición se conservan.
        if (level4Root != null)
        {
            DestroyDirectChild(level4Root.transform, "Environment");
            DestroyDirectChild(level4Root.transform, "Furniture");
            visualParent = level4Root.transform;
        }

        GameObject house = new GameObject(NewRootName);
        SceneManager.MoveGameObjectToScene(house, scene);

        if (visualParent != null)
            house.transform.SetParent(visualParent, false);

        house.transform.localPosition = Vector3.zero;
        house.transform.localRotation = Quaternion.identity;
        house.transform.localScale = Vector3.one;

        Transform architecture = Group("01_ARQUITECTURA", house.transform);
        Transform living = Group("02_SALA", house.transform);
        Transform dining = Group("03_COMEDOR", house.transform);
        Transform library = Group("04_LECTURA_LIBRERO", house.transform);
        Transform stairs = Group("05_ESCALERA_DECORATIVA", house.transform);
        Transform decoration = Group("06_DECORACION", house.transform);
        Transform lighting = Group("07_ILUMINACION", house.transform);
        Transform windows = Group("08_VENTANAS_Y_CORTINAS", house.transform);

        // Materiales.
        Material floorMat = CreateTexturedMaterial(
            "M_L4_Parquet",
            AssetRoot + "/Textures/diagonal_parquet_diff_1k.jpg",
            new Color(0.95f, 0.88f, 0.78f, 1f),
            new Vector2(8f, 6f),
            0.18f);

        Material wallMat = CreateTexturedMaterial(
            "M_L4_Pared_Crema",
            AssetRoot + "/Textures/painted_plaster_wall_diff_1k.jpg",
            new Color(0.93f, 0.85f, 0.72f, 1f),
            new Vector2(6f, 3f),
            0.08f);

        Material accentMat = CreateTexturedMaterial(
            "M_L4_Pared_Accent",
            AssetRoot + "/Textures/plaster_grey_04_diff_1k.jpg",
            new Color(0.63f, 0.68f, 0.70f, 1f),
            new Vector2(4f, 3f),
            0.06f);

        Material woodMat = CreateTexturedMaterial(
            "M_L4_Madera_Trim",
            AssetRoot + "/Textures/wood_floor_diff_1k.jpg",
            new Color(0.55f, 0.34f, 0.20f, 1f),
            new Vector2(3f, 1f),
            0.14f);

        Material ceilingMat = CreateColorMaterial(
            "M_L4_Techo_Calido",
            new Color(0.88f, 0.82f, 0.72f, 1f),
            0.05f);

        Material trimMat = CreateColorMaterial(
            "M_L4_Moldura_Oscura",
            new Color(0.22f, 0.14f, 0.09f, 1f),
            0.12f);

        Material rugMat = CreateColorMaterial(
            "M_L4_Alfombra_AzulPetroleo",
            new Color(0.20f, 0.31f, 0.34f, 1f),
            0.12f);

        Material art1 = CreateColorMaterial(
            "M_L4_Arte_Azul",
            new Color(0.20f, 0.45f, 0.62f, 1f),
            0.08f);

        Material art2 = CreateColorMaterial(
            "M_L4_Arte_Ocre",
            new Color(0.78f, 0.49f, 0.20f, 1f),
            0.08f);

        Material art3 = CreateColorMaterial(
            "M_L4_Arte_Verde",
            new Color(0.27f, 0.49f, 0.34f, 1f),
            0.08f);

        // ==============================================================
        // CASA ENORME: 32 x 24 m, exactamente 4x el área del anterior 16x12.
        // ==============================================================

        // Suelo sólido: top del piso exactamente Y=0.
        CreateBox(
            "PISO_PARQUET_SEGURO_32x24",
            architecture,
            new Vector3(0f, -0.10f, 0f),
            new Vector3(32f, 0.20f, 24f),
            floorMat,
            true);

        // Techo completo, cálido.
        CreateBox(
            "TECHO_CALIDO",
            architecture,
            new Vector3(0f, 4.30f, 0f),
            new Vector3(32f, 0.18f, 24f),
            ceilingMat,
            true);

        BuildOuterWalls(architecture, wallMat);
        BuildBackWindowWall(architecture, wallMat);
        BuildFrontEntryWall(architecture, wallMat);

        // Zócalos y molduras para que no parezca una caja vacía.
        BuildBaseboards(architecture, woodMat);
        BuildCrownMolding(architecture, trimMat);
        BuildCeilingBeams(architecture, woodMat);

        // Zona de pared de lectura con textura distinta.
        CreateBox(
            "PARED_ACCENT_LECTURA",
            architecture,
            new Vector3(15.82f, 2.05f, 6.25f),
            new Vector3(0.06f, 3.40f, 8.80f),
            accentMat,
            false);

        // Dos columnas que enmarcan visualmente la zona del fondo.
        CreateBox(
            "Columna_Transicion_Izq",
            architecture,
            new Vector3(-5.7f, 2.10f, 5.5f),
            new Vector3(0.55f, 4.20f, 0.55f),
            wallMat,
            true);

        CreateBox(
            "Columna_Transicion_Der",
            architecture,
            new Vector3(5.7f, 2.10f, 5.5f),
            new Vector3(0.55f, 4.20f, 0.55f),
            wallMat,
            true);

        CreateBox(
            "Viga_Transicion",
            architecture,
            new Vector3(0f, 3.82f, 5.5f),
            new Vector3(11.95f, 0.38f, 0.55f),
            woodMat,
            true);

        // ==============================================================
        // SALA CENTRAL. Dejamos el área x +/-4 y z -2..3 despejada para
        // la araña y los tres puntos de exposición existentes.
        // ==============================================================

        CreateBox(
            "Alfombra_Sala",
            living,
            new Vector3(0f, 0.026f, 5.5f),
            new Vector3(10.5f, 0.052f, 7.2f),
            rugMat,
            false);

        Place(
            "House/Couch_Large1.fbx",
            "Sofa_Principal",
            living,
            new Vector3(0f, 0f, 8.7f),
            180f,
            5.4f,
            true);

        Place(
            "House/Chair_3.fbx",
            "Sillon_Izquierdo",
            living,
            new Vector3(-6.1f, 0f, 5.8f),
            70f,
            1.7f,
            true);

        Place(
            "House/Chair_3.fbx",
            "Sillon_Derecho",
            living,
            new Vector3(6.1f, 0f, 5.8f),
            -70f,
            1.7f,
            true);

        Place(
            "Kenney/tableCoffeeGlass.fbx",
            "Mesa_Centro",
            living,
            new Vector3(0f, 0f, 5.25f),
            0f,
            2.5f,
            true);

        Place(
            "House/Drawer_3.fbx",
            "Consola_Sala",
            living,
            new Vector3(-10.6f, 0f, 3.4f),
            90f,
            3.2f,
            true);

        Place(
            "House/Fireplace.fbx",
            "Chimenea_Decorativa",
            living,
            new Vector3(-15.30f, 0f, 4.2f),
            90f,
            3.1f,
            true);

        Place(
            "House/Light_Floor1.fbx",
            "Lampara_Sala",
            living,
            new Vector3(-6.7f, 0f, 8.1f),
            0f,
            2.0f,
            false);

        Place(
            "House/Houseplant_5.fbx",
            "Planta_Sala",
            living,
            new Vector3(7.0f, 0f, 8.7f),
            0f,
            1.8f,
            false);

        // ==============================================================
        // COMEDOR GRANDE - esquina posterior izquierda.
        // ==============================================================

        Place(
            "House/Table_RoundLarge.fbx",
            "Mesa_Comedor",
            dining,
            new Vector3(-10.3f, 0f, 8.0f),
            0f,
            3.5f,
            true);

        for (int i = 0; i < 6; i++)
        {
            float angle = i * 60f;
            float rad = angle * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(
                -10.3f + Mathf.Sin(rad) * 2.85f,
                0f,
                8.0f + Mathf.Cos(rad) * 2.85f);

            Place(
                "Kenney/chairModernCushion.fbx",
                "Silla_Comedor_" + (i + 1),
                dining,
                pos,
                angle + 180f,
                1.25f,
                true);
        }

        Place(
            "House/Light_Chandelier.fbx",
            "Lampara_Comedor",
            dining,
            new Vector3(-10.3f, 3.55f, 8.0f),
            0f,
            2.25f,
            false,
            false);

        Place(
            "House/Houseplant_1.fbx",
            "Planta_Comedor",
            dining,
            new Vector3(-14.1f, 0f, 9.6f),
            0f,
            1.7f,
            false);

        // ==============================================================
        // LECTURA / LIBRERO - posterior derecha.
        // ==============================================================

        Place(
            "Extra/Bookcase_Books.fbx",
            "Librero_Con_Libros",
            library,
            new Vector3(14.7f, 0f, 7.5f),
            -90f,
            3.8f,
            true);

        Place(
            "House/Shelf_Large.fbx",
            "Estanteria_Secundaria",
            library,
            new Vector3(14.8f, 0f, 3.7f),
            -90f,
            3.0f,
            true);

        Place(
            "Extra/Sofa_individual.fbx",
            "Butaca_Lectura_1",
            library,
            new Vector3(9.9f, 0f, 7.2f),
            -30f,
            1.9f,
            true);

        Place(
            "Extra/Sofa_individual.fbx",
            "Butaca_Lectura_2",
            library,
            new Vector3(9.0f, 0f, 4.8f),
            35f,
            1.9f,
            true);

        Place(
            "House/Table_RoundSmall.fbx",
            "Mesa_Lectura",
            library,
            new Vector3(10.5f, 0f, 5.7f),
            0f,
            1.45f,
            true);

        Place(
            "Kenney/lampRoundFloor.fbx",
            "Lampara_Lectura",
            library,
            new Vector3(12.7f, 0f, 5.0f),
            0f,
            2.0f,
            false);

        Place(
            "Kenney/pottedPlant.fbx",
            "Planta_Lectura",
            library,
            new Vector3(13.4f, 0f, 10.1f),
            0f,
            1.6f,
            false);

        // ==============================================================
        // ESCALERA DECORATIVA - derecha, lejos del gameplay.
        // ==============================================================

        Place(
            "Kenney/stairsOpen.fbx",
            "Escalera_Decorativa",
            stairs,
            new Vector3(11.2f, 0f, -6.6f),
            0f,
            6.2f,
            true);

        CreateBox(
            "Descanso_Escalera",
            stairs,
            new Vector3(11.2f, 2.55f, -10.0f),
            new Vector3(7.0f, 0.28f, 3.6f),
            woodMat,
            true);

        CreateBox(
            "Muro_Escalera_Derecha",
            stairs,
            new Vector3(15.35f, 1.8f, -6.5f),
            new Vector3(0.40f, 3.6f, 10.0f),
            accentMat,
            true);

        // ==============================================================
        // VENTANAS GRANDES + CORTINAS. El HDR se ve a través de las tres.
        // ==============================================================

        float[] windowX = { -9f, 0f, 9f };

        foreach (float x in windowX)
        {
            GameObject w = Place(
                "House/Window_Large1.fbx",
                "Ventana_Grande_" + x.ToString("0"),
                windows,
                new Vector3(x, 0.72f, 11.88f),
                180f,
                5.2f,
                false);

            if (w != null)
                FitToApproxBox(w, 5.1f, 2.65f, 0.35f);

            GameObject c = Place(
                "House/Curtains_Double.fbx",
                "Cortinas_" + x.ToString("0"),
                windows,
                new Vector3(x, 0f, 11.55f),
                180f,
                5.7f,
                false);

            if (c != null)
                FitToApproxBox(c, 6.0f, 3.15f, 0.85f);
        }

        // ==============================================================
        // DECORACIÓN: cuadros y paneles, sin descargar nada adicional.
        // ==============================================================

        CreateFramedArt(
            decoration,
            "Cuadro_1",
            new Vector3(-15.70f, 2.35f, -4.9f),
            new Vector3(0f, 90f, 0f),
            2.3f,
            1.5f,
            trimMat,
            art1);

        CreateFramedArt(
            decoration,
            "Cuadro_2",
            new Vector3(-15.70f, 2.35f, -1.8f),
            new Vector3(0f, 90f, 0f),
            2.3f,
            1.5f,
            trimMat,
            art2);

        CreateFramedArt(
            decoration,
            "Cuadro_3",
            new Vector3(-15.70f, 2.35f, 1.3f),
            new Vector3(0f, 90f, 0f),
            2.3f,
            1.5f,
            trimMat,
            art3);

        // Paneles verticales hogareños a la derecha.
        for (int i = 0; i < 5; i++)
        {
            CreateBox(
                "Panel_Madera_" + (i + 1),
                decoration,
                new Vector3(15.66f, 1.95f, -9.2f + i * 1.05f),
                new Vector3(0.12f, 3.2f, 0.52f),
                woodMat,
                false);
        }

        // Ventilador decorativo de techo.
        Place(
            "Kenney/ceilingFan.fbx",
            "Ventilador_Techo",
            decoration,
            new Vector3(0f, 3.88f, 0.4f),
            0f,
            2.9f,
            false,
            false);

        // ==============================================================
        // ILUMINACIÓN CÁLIDA.
        // ==============================================================

        SetupDirectionalLight(scene);

        AddPointLight(
            lighting,
            "Luz_Sala_Principal",
            new Vector3(0f, 3.55f, 4.2f),
            new Color(1f, 0.78f, 0.58f),
            3.0f,
            12f);

        AddPointLight(
            lighting,
            "Luz_Comedor",
            new Vector3(-10.3f, 3.25f, 8.0f),
            new Color(1f, 0.72f, 0.50f),
            2.4f,
            9f);

        AddPointLight(
            lighting,
            "Luz_Lectura",
            new Vector3(10.4f, 2.65f, 6.1f),
            new Color(1f, 0.76f, 0.56f),
            2.0f,
            8f);

        AddPointLight(
            lighting,
            "Luz_Entrada",
            new Vector3(0f, 3.45f, -6.5f),
            new Color(1f, 0.82f, 0.65f),
            2.1f,
            10f);

        AddPointLight(
            lighting,
            "Luz_Escalera",
            new Vector3(11.0f, 3.1f, -6.0f),
            new Color(1f, 0.72f, 0.52f),
            1.7f,
            7f);

        SetupSkybox();

        // Recupera ambiente sonoro existente del proyecto, si está disponible.
        AudioClip ambience = AssetDatabase.LoadAssetAtPath<AudioClip>(
            "Assets/NeuroPhobia_Level45/Audio/Level4/house_ambience.mp3");

        if (ambience != null)
        {
            GameObject audio = new GameObject("Ambiente_Casa_Nivel4");
            audio.transform.SetParent(lighting, false);

            AudioSource source = audio.AddComponent<AudioSource>();
            source.clip = ambience;
            source.loop = true;
            source.playOnAwake = true;
            source.spatialBlend = 0f;
            source.volume = 0.18f;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.SetDirty(house);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Selection.activeGameObject = house;

        if (SceneView.lastActiveSceneView != null)
            SceneView.lastActiveSceneView.FrameSelected();

        EditorUtility.DisplayDialog(
            "NeuroPhobia · Nivel 4",
            "CASA ENORME creada y guardada.\n\n" +
            "Tamaño: 32 x 24 metros (4 veces el área de la versión 16 x 12).\n\n" +
            "Incluye:\n" +
            "• sala grande\n" +
            "• comedor de 6 puestos\n" +
            "• librero y rincón de lectura\n" +
            "• chimenea\n" +
            "• escalera decorativa\n" +
            "• 3 ventanas grandes con cortinas\n" +
            "• fondo HDR exterior\n" +
            "• parquet real\n" +
            "• paredes de yeso\n" +
            "• techo cálido con vigas\n" +
            "• cuadros\n" +
            "• plantas y lámparas\n" +
            "• piso y paredes con collider\n\n" +
            "NO se eliminaron Gameplay, SpiderRoutes, Controller, UI ni selector PC/RV.\n\n" +
            "Respaldo: " + BackupPath,
            "Aceptar");
    }

    [MenuItem(
        "Tools/NeuroPhobia/Nivel 4/Seleccionar CASA ENORME",
        priority = 221)]
    public static void SelectHouse()
    {
        GameObject go =
            FindSceneObject(SceneManager.GetActiveScene(), NewRootName);

        if (go == null)
        {
            EditorUtility.DisplayDialog(
                "NeuroPhobia",
                "Todavía no existe la casa enorme.",
                "Aceptar");
            return;
        }

        Selection.activeGameObject = go;

        if (SceneView.lastActiveSceneView != null)
            SceneView.lastActiveSceneView.FrameSelected();
    }

    private static void RemoveOldGeneratedVisuals(Scene scene)
    {
        string[] names =
        {
            NewRootName,
            "NP_ENV_NIVEL4_HOGAR_ORGANIZADO"
        };

        foreach (string n in names)
        {
            GameObject go = FindSceneObject(scene, n);

            if (go != null)
                UnityEngine.Object.DestroyImmediate(go);
        }
    }

    private static void DestroyDirectChild(
        Transform parent,
        string childName)
    {
        if (parent == null)
            return;

        Transform child = parent.Find(childName);

        if (child != null)
            UnityEngine.Object.DestroyImmediate(child.gameObject);
    }

    private static void BuildOuterWalls(
        Transform parent,
        Material wall)
    {
        // Laterales completos.
        CreateBox(
            "Pared_Izquierda",
            parent,
            new Vector3(-15.9f, 2.10f, 0f),
            new Vector3(0.22f, 4.20f, 24f),
            wall,
            true);

        CreateBox(
            "Pared_Derecha",
            parent,
            new Vector3(15.9f, 2.10f, 0f),
            new Vector3(0.22f, 4.20f, 24f),
            wall,
            true);
    }

    private static void BuildBackWindowWall(
        Transform parent,
        Material wall)
    {
        // Zócalo bajo ventanas y dintel superior.
        CreateBox(
            "Pared_Fondo_Inferior",
            parent,
            new Vector3(0f, 0.35f, 11.9f),
            new Vector3(32f, 0.70f, 0.22f),
            wall,
            true);

        CreateBox(
            "Pared_Fondo_Superior",
            parent,
            new Vector3(0f, 3.78f, 11.9f),
            new Vector3(32f, 0.84f, 0.22f),
            wall,
            true);

        // Cuatro bloques separan tres ventanas grandes.
        CreateBox(
            "Pared_Fondo_Bloque_1",
            parent,
            new Vector3(-13.75f, 2.05f, 11.9f),
            new Vector3(4.50f, 2.70f, 0.22f),
            wall,
            true);

        CreateBox(
            "Pared_Fondo_Bloque_2",
            parent,
            new Vector3(-4.50f, 2.05f, 11.9f),
            new Vector3(4.00f, 2.70f, 0.22f),
            wall,
            true);

        CreateBox(
            "Pared_Fondo_Bloque_3",
            parent,
            new Vector3(4.50f, 2.05f, 11.9f),
            new Vector3(4.00f, 2.70f, 0.22f),
            wall,
            true);

        CreateBox(
            "Pared_Fondo_Bloque_4",
            parent,
            new Vector3(13.75f, 2.05f, 11.9f),
            new Vector3(4.50f, 2.70f, 0.22f),
            wall,
            true);
    }

    private static void BuildFrontEntryWall(
        Transform parent,
        Material wall)
    {
        // Entrada central enorme de 6 m.
        CreateBox(
            "Pared_Frente_Izquierda",
            parent,
            new Vector3(-9.5f, 2.10f, -11.9f),
            new Vector3(13.0f, 4.20f, 0.22f),
            wall,
            true);

        CreateBox(
            "Pared_Frente_Derecha",
            parent,
            new Vector3(9.5f, 2.10f, -11.9f),
            new Vector3(13.0f, 4.20f, 0.22f),
            wall,
            true);

        CreateBox(
            "Pared_Frente_Sobre_Entrada",
            parent,
            new Vector3(0f, 3.65f, -11.9f),
            new Vector3(6.0f, 1.10f, 0.22f),
            wall,
            true);
    }

    private static void BuildBaseboards(
        Transform parent,
        Material mat)
    {
        CreateBox(
            "Zocalo_Izq",
            parent,
            new Vector3(-15.68f, 0.26f, 0f),
            new Vector3(0.12f, 0.50f, 23.6f),
            mat,
            false);

        CreateBox(
            "Zocalo_Der",
            parent,
            new Vector3(15.68f, 0.26f, 0f),
            new Vector3(0.12f, 0.50f, 23.6f),
            mat,
            false);

        CreateBox(
            "Zocalo_Fondo",
            parent,
            new Vector3(0f, 0.26f, 11.68f),
            new Vector3(31.4f, 0.50f, 0.12f),
            mat,
            false);
    }

    private static void BuildCrownMolding(
        Transform parent,
        Material mat)
    {
        CreateBox(
            "Moldura_Superior_Izq",
            parent,
            new Vector3(-15.66f, 4.03f, 0f),
            new Vector3(0.16f, 0.18f, 23.6f),
            mat,
            false);

        CreateBox(
            "Moldura_Superior_Der",
            parent,
            new Vector3(15.66f, 4.03f, 0f),
            new Vector3(0.16f, 0.18f, 23.6f),
            mat,
            false);

        CreateBox(
            "Moldura_Superior_Fondo",
            parent,
            new Vector3(0f, 4.03f, 11.66f),
            new Vector3(31.4f, 0.18f, 0.16f),
            mat,
            false);
    }

    private static void BuildCeilingBeams(
        Transform parent,
        Material mat)
    {
        float[] xs = { -10.5f, -5.25f, 0f, 5.25f, 10.5f };

        foreach (float x in xs)
        {
            CreateBox(
                "Viga_Techo_" + x.ToString("0.00"),
                parent,
                new Vector3(x, 4.10f, 0f),
                new Vector3(0.18f, 0.20f, 23.4f),
                mat,
                false);
        }
    }

    private static void CreateFramedArt(
        Transform parent,
        string name,
        Vector3 position,
        Vector3 euler,
        float width,
        float height,
        Material frame,
        Material art)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = position;
        root.transform.eulerAngles = euler;

        CreateBox(
            "Marco",
            root.transform,
            Vector3.zero,
            new Vector3(width, height, 0.11f),
            frame,
            false);

        CreateBox(
            "Arte",
            root.transform,
            new Vector3(0f, 0f, -0.07f),
            new Vector3(width - 0.24f, height - 0.24f, 0.055f),
            art,
            false);
    }

    private static GameObject Place(
        string relativePath,
        string name,
        Transform parent,
        Vector3 position,
        float yaw,
        float targetHorizontalSize,
        bool collider,
        bool snapToFloor = true)
    {
        string path = AssetRoot + "/Models/" + relativePath;

        GameObject prefab =
            AssetDatabase.LoadAssetAtPath<GameObject>(path);

        if (prefab == null)
        {
            Debug.LogWarning(
                "[NeuroPhobia Nivel4] No encontré modelo: " + path);
            return null;
        }

        GameObject go =
            PrefabUtility.InstantiatePrefab(prefab) as GameObject;

        if (go == null)
            return null;

        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.position = position;
        go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        go.transform.localScale = Vector3.one;

        ConvertRendererMaterials(go);

        Bounds b;

        if (TryGetBounds(go, out b))
        {
            float horizontal =
                Mathf.Max(b.size.x, b.size.z);

            if (horizontal > 0.0001f)
            {
                float factor =
                    targetHorizontalSize / horizontal;

                go.transform.localScale *= factor;
            }
        }

        if (snapToFloor)
            SnapToFloor(go, 0f);

        if (collider)
            AddBoundsCollider(go);

        return go;
    }

    private static void FitToApproxBox(
        GameObject go,
        float width,
        float height,
        float depth)
    {
        if (go == null)
            return;

        Bounds b;

        if (!TryGetBounds(go, out b))
            return;

        Vector3 s = go.transform.localScale;

        if (b.size.x > 0.001f)
            s.x *= width / b.size.x;

        if (b.size.y > 0.001f)
            s.y *= height / b.size.y;

        if (b.size.z > 0.001f)
            s.z *= depth / b.size.z;

        go.transform.localScale = s;
    }

    private static void SnapToFloor(
        GameObject go,
        float floorY)
    {
        Bounds b;

        if (!TryGetBounds(go, out b))
            return;

        go.transform.position +=
            Vector3.up * (floorY - b.min.y);
    }

    private static void AddBoundsCollider(GameObject go)
    {
        if (go == null)
            return;

        // Si el modelo ya tiene colliders, no duplicarlos.
        Collider[] existing =
            go.GetComponentsInChildren<Collider>(true);

        if (existing != null && existing.Length > 0)
            return;

        Bounds b;

        if (!TryGetBounds(go, out b))
            return;

        BoxCollider box = go.AddComponent<BoxCollider>();

        box.center =
            go.transform.InverseTransformPoint(b.center);

        Vector3 scale = go.transform.lossyScale;

        float sx = Mathf.Max(0.0001f, Mathf.Abs(scale.x));
        float sy = Mathf.Max(0.0001f, Mathf.Abs(scale.y));
        float sz = Mathf.Max(0.0001f, Mathf.Abs(scale.z));

        box.size = new Vector3(
            b.size.x / sx,
            b.size.y / sy,
            b.size.z / sz);

        box.isTrigger = false;
    }

    private static bool TryGetBounds(
        GameObject go,
        out Bounds bounds)
    {
        Renderer[] renderers =
            go.GetComponentsInChildren<Renderer>(true);

        if (renderers == null || renderers.Length == 0)
        {
            bounds = new Bounds(go.transform.position, Vector3.zero);
            return false;
        }

        bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        return true;
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
            GameObject.CreatePrimitive(PrimitiveType.Cube);

        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = localScale;

        Renderer renderer = go.GetComponent<Renderer>();

        if (renderer != null && material != null)
            renderer.sharedMaterial = material;

        Collider c = go.GetComponent<Collider>();

        if (c != null)
            c.enabled = collider;

        return go;
    }

    private static Transform Group(
        string name,
        Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    private static void AddPointLight(
        Transform parent,
        string name,
        Vector3 position,
        Color color,
        float intensity,
        float range)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = position;

        Light light = go.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.shadows = LightShadows.Soft;
    }

    private static void SetupDirectionalLight(Scene scene)
    {
        Light directional = null;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Light[] lights =
                root.GetComponentsInChildren<Light>(true);

            foreach (Light l in lights)
            {
                if (l != null && l.type == LightType.Directional)
                {
                    directional = l;
                    break;
                }
            }

            if (directional != null)
                break;
        }

        if (directional == null)
        {
            GameObject go = new GameObject("Directional Light");
            SceneManager.MoveGameObjectToScene(go, scene);
            directional = go.AddComponent<Light>();
            directional.type = LightType.Directional;
        }

        directional.gameObject.SetActive(true);
        directional.enabled = true;
        directional.color = new Color(1f, 0.91f, 0.78f);
        directional.intensity = 0.75f;
        directional.transform.rotation =
            Quaternion.Euler(48f, -35f, 0f);

        EditorUtility.SetDirty(directional);
    }

    private static void SetupSkybox()
    {
        Texture2D hdr =
            AssetDatabase.LoadAssetAtPath<Texture2D>(
                AssetRoot + "/HDR/suburban_garden_2k.hdr");

        if (hdr == null)
        {
            Debug.LogWarning(
                "[NeuroPhobia Nivel4] No encontré el HDR exterior.");
            return;
        }

        string matPath =
            MaterialsRoot + "/M_L4_Exterior_Skybox.mat";

        Material sky =
            AssetDatabase.LoadAssetAtPath<Material>(matPath);

        Shader shader = Shader.Find("Skybox/Panoramic");

        if (shader == null)
            return;

        if (sky == null)
        {
            sky = new Material(shader);
            AssetDatabase.CreateAsset(sky, matPath);
        }
        else
        {
            sky.shader = shader;
        }

        if (sky.HasProperty("_MainTex"))
            sky.SetTexture("_MainTex", hdr);

        if (sky.HasProperty("_Exposure"))
            sky.SetFloat("_Exposure", 0.75f);

        if (sky.HasProperty("_Rotation"))
            sky.SetFloat("_Rotation", 110f);

        EditorUtility.SetDirty(sky);
        RenderSettings.skybox = sky;
        RenderSettings.ambientMode =
            UnityEngine.Rendering.AmbientMode.Skybox;
        RenderSettings.ambientIntensity = 0.75f;

        DynamicGI.UpdateEnvironment();
    }

    private static Material CreateTexturedMaterial(
        string name,
        string texturePath,
        Color tint,
        Vector2 tiling,
        float smoothness)
    {
        string path = MaterialsRoot + "/" + name + ".mat";

        Material mat =
            AssetDatabase.LoadAssetAtPath<Material>(path);

        Shader shader =
            Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
            shader = Shader.Find("Standard");

        if (mat == null)
        {
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }
        else
        {
            mat.shader = shader;
        }

        Texture2D tex =
            AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);

        if (mat.HasProperty("_BaseMap"))
        {
            mat.SetTexture("_BaseMap", tex);
            mat.SetColor("_BaseColor", tint);
            mat.SetTextureScale("_BaseMap", tiling);
        }
        else
        {
            mat.mainTexture = tex;
            mat.color = tint;
            mat.mainTextureScale = tiling;
        }

        if (mat.HasProperty("_Smoothness"))
            mat.SetFloat("_Smoothness", smoothness);

        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static Material CreateColorMaterial(
        string name,
        Color color,
        float smoothness)
    {
        string path = MaterialsRoot + "/" + name + ".mat";

        Material mat =
            AssetDatabase.LoadAssetAtPath<Material>(path);

        Shader shader =
            Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
            shader = Shader.Find("Standard");

        if (mat == null)
        {
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }
        else
        {
            mat.shader = shader;
        }

        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color);
        else
            mat.color = color;

        if (mat.HasProperty("_Smoothness"))
            mat.SetFloat("_Smoothness", smoothness);

        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static void ConvertRendererMaterials(GameObject root)
    {
        Shader urp =
            Shader.Find("Universal Render Pipeline/Lit");

        if (urp == null)
            return;

        Renderer[] renderers =
            root.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer in renderers)
        {
            Material[] source = renderer.sharedMaterials;
            Material[] result = new Material[source.Length];

            for (int i = 0; i < source.Length; i++)
            {
                Material src = source[i];

                if (src == null)
                {
                    result[i] = CreateColorMaterial(
                        "M_L4_Modelo_Default",
                        new Color(0.70f, 0.66f, 0.59f, 1f),
                        0.10f);
                    continue;
                }

                if (src.shader != null &&
                    src.shader.name.Contains(
                        "Universal Render Pipeline"))
                {
                    result[i] = src;
                    continue;
                }

                Material converted;

                if (Converted.TryGetValue(src, out converted))
                {
                    result[i] = converted;
                    continue;
                }

                string safe = Sanitize(src.name);

                string path = AssetDatabase.GenerateUniqueAssetPath(
                    MaterialsRoot + "/Imported_" + safe + ".mat");

                converted = new Material(urp);

                Color c = Color.white;

                if (src.HasProperty("_Color"))
                    c = src.color;

                if (converted.HasProperty("_BaseColor"))
                    converted.SetColor("_BaseColor", c);

                Texture main = src.mainTexture;

                if (main != null &&
                    converted.HasProperty("_BaseMap"))
                {
                    converted.SetTexture("_BaseMap", main);
                }

                if (src.HasProperty("_Glossiness") &&
                    converted.HasProperty("_Smoothness"))
                {
                    converted.SetFloat(
                        "_Smoothness",
                        src.GetFloat("_Glossiness"));
                }

                AssetDatabase.CreateAsset(converted, path);
                Converted[src] = converted;
                result[i] = converted;
            }

            renderer.sharedMaterials = result;
        }
    }

    private static string Sanitize(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "Material";

        foreach (char c in Path.GetInvalidFileNameChars())
            value = value.Replace(c, '_');

        value = value.Replace('/', '_').Replace('\\', '_');

        return value;
    }

    private static void EnsureFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder))
            return;

        string[] parts = folder.Split('/');
        string current = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];

            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);

            current = next;
        }
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
                return found.gameObject;
        }

        return null;
    }

    private static Transform FindRecursive(
        Transform current,
        string exactName)
    {
        if (current == null)
            return null;

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
                FindRecursive(child, exactName);

            if (found != null)
                return found;
        }

        return null;
    }
}
#endif
