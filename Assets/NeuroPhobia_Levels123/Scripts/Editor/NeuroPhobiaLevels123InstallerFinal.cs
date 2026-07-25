#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class NeuroPhobiaLevels123InstallerFinal
{
    private const string Root = "Assets/NeuroPhobia_Levels123";
    private const string Generated = Root + "/Generated";
    private const string Materials = Generated + "/Materials";
    private const string Controllers = Generated + "/AnimatorControllers";
    private static int spiderLayer;

    [MenuItem("Tools/NeuroPhobia/Reconfigurar Niveles 1-3 FINAL")]
    public static void Install()
    {
        Debug.Log("[NeuroPhobia FINAL 2026-07-23] Instalador definitivo iniciado. La roca no usa BoxCollider.");
        if (!ValidateRequiredAssets()) return;
        if (!EditorUtility.DisplayDialog("NeuroPhobia", "Se crearán copias de seguridad y se reconfigurarán Nivel1, Nivel2 y Nivel3. No se modificarán Assets/XR, Assets/XRI ni Assets/Settings.", "Continuar", "Cancelar")) return;

        try
        {
            Directory.CreateDirectory(Materials);
            Directory.CreateDirectory(Controllers);
            AssetDatabase.Refresh();
            spiderLayer = EnsureLayer("Spider");
            ConfigureImporters();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            CreateMaterials();
            AnimatorController walkController = CreateWalkController();
            string backupFolder = CreateBackupFolder();
            ReconfigureLevel1(backupFolder);
            ReconfigureLevel2(backupFolder);
            ReconfigureLevel3(backupFolder, walkController);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("NeuroPhobia", "FINAL: Niveles 1, 2 y 3 reconfigurados. Roca_Realista quedó sin BoxCollider. Las escenas originales quedaron respaldadas en Assets/Scenes/Backups_Niveles123.", "Aceptar");
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            EditorUtility.DisplayDialog("NeuroPhobia", "La instalación se detuvo. Revisa la consola de Unity. No borres las copias de seguridad.", "Aceptar");
        }
        finally { EditorUtility.ClearProgressBar(); }
    }

    [MenuItem("Tools/NeuroPhobia/Validar archivos Niveles 1-3 FINAL")]
    public static void ValidateMenu() => ValidateRequiredAssets(true);

    private static bool ValidateRequiredAssets(bool showSuccess = false)
    {
        string[] required = {
            Root + "/External/Nivel1/ModernOffice/Modern Office.fbx",
            Root + "/External/Nivel1/Materials/Plaster/Plaster_Color.jpg",
            Root + "/External/Nivel1/Materials/Planks/Planks_Color.jpg",
            Root + "/Audio/Nivel1/Respiracion_Inhala_Exhala.mp3",
            Root + "/Audio/Nivel1/Ambiente_Consultorio.mp3",
            Root + "/External/Nivel2/WolfSpider/source/Wolf_spider.fbx",
            Root + "/External/Nivel2/Garden/Sky/SunnyRoseGarden_2K.hdr",
            Root + "/External/Nivel2/Garden/Grass/Grass_Diffuse.jpg",
            Root + "/External/Nivel2/Garden/Rock/Rock07.fbx",
            Root + "/External/Nivel2/Garden/Shrub/Shrub03.fbx",
            Root + "/External/Nivel2/Garden/Fern/Fern02.fbx",
            Root + "/Audio/Nivel2/Ambiente_Jardin.mp3",
            Root + "/External/Nivel3/Materials/ConcreteWall/ConcreteWall_Diffuse.jpg",
            Root + "/External/Nivel3/Materials/ConcreteFloor/ConcreteFloor_Diffuse.jpg",
            Root + "/External/Nivel3/Materials/RustyMetal/RustyMetal_Diffuse.jpg",
            Root + "/External/Nivel3/Props/Duct/Duct.fbx",
            Root + "/External/Nivel3/Props/Lamp/Lamp.fbx",
            Root + "/External/Nivel3/Props/Barrel/Barrel.fbx",
            Root + "/Audio/Nivel3/Ambiente_Sotano.mp3",
            "Assets/Scenes/Nivel1.unity", "Assets/Scenes/Nivel2.unity", "Assets/Scenes/Nivel3.unity",
            "Assets/Images/Nivel1/spider_card_01.png", "Assets/Images/Nivel1/spider_card_02.png", "Assets/Images/Nivel1/spider_card_03.png", "Assets/Images/Nivel1/spider_card_04.png"
        };
        List<string> missing = required.Where(p => AssetDatabase.LoadMainAssetAtPath(p) == null && !File.Exists(p)).ToList();
        if (missing.Count > 0)
        {
            EditorUtility.DisplayDialog("Faltan archivos", string.Join("\n", missing), "Aceptar");
            return false;
        }
        if (showSuccess) EditorUtility.DisplayDialog("Validación", "Están presentes todos los assets externos necesarios para reconfigurar los tres niveles. La narración grabada del Nivel 1 sigue siendo un insumo de la autora y no está incluida.", "Aceptar");
        return true;
    }

    private static void ConfigureImporters()
    {
        string[] normals = AssetDatabase.FindAssets("", new[] { Root }).Select(AssetDatabase.GUIDToAssetPath).Where(p => p.Contains("_Normal.")).ToArray();
        foreach (string path in normals) ConfigureTexture(path, true, false, 2048);
        string[] masks = AssetDatabase.FindAssets("", new[] { Root }).Select(AssetDatabase.GUIDToAssetPath).Where(p => p.Contains("MetallicSmoothness") || p.Contains("_AO.") || p.Contains("_Roughness.") || p.Contains("_Alpha.")).ToArray();
        foreach (string path in masks) ConfigureTexture(path, false, false, 1024);
        string[] colors = AssetDatabase.FindAssets("", new[] { Root }).Select(AssetDatabase.GUIDToAssetPath).Where(p => p.EndsWith(".jpg") || p.EndsWith(".png")).ToArray();
        foreach (string path in colors) if (!normals.Contains(path) && !masks.Contains(path)) ConfigureTexture(path, false, true, path.Contains("Modern Office.fbm") ? 1024 : 2048);

        ModelImporter spider = AssetImporter.GetAtPath(Root + "/External/Nivel2/WolfSpider/source/Wolf_spider.fbx") as ModelImporter;
        if (spider != null)
        {
            spider.importAnimation = true;
            spider.animationType = ModelImporterAnimationType.Generic;
            spider.importCameras = false;
            spider.importLights = false;
            spider.SaveAndReimport();
        }
    }

    private static void ConfigureTexture(string path, bool normal, bool srgb, int maxSize)
    {
        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null) return;
        ti.textureType = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
        ti.sRGBTexture = normal ? false : srgb;
        ti.maxTextureSize = maxSize;
        ti.textureCompression = TextureImporterCompression.CompressedHQ;
        ti.alphaIsTransparency = path.Contains("DiffuseAlpha") || path.Contains("GlassDiffuse");
        ti.SaveAndReimport();
    }

    private static void CreateMaterials()
    {
        CreateLit("M_Plaster", L1("Materials/Plaster/Plaster_Color.jpg"), L1("Materials/Plaster/Plaster_Normal.png"), L1("Materials/Plaster/Plaster_MetallicSmoothness.png"), null, false);
        CreateLit("M_Planks", L1("Materials/Planks/Planks_Color.jpg"), L1("Materials/Planks/Planks_Normal.png"), L1("Materials/Planks/Planks_MetallicSmoothness.png"), L1("Materials/Planks/Planks_AO.jpg"), false);
        CreateLit("M_WolfSpider", Root + "/External/Nivel2/WolfSpider/textures/Wolf_Spider_Diffuse.png", Root + "/External/Nivel2/WolfSpider/textures/Wolf_Spider_Normal.png", Root + "/External/Nivel2/WolfSpider/textures/Wolf_Spider_MetallicSmoothness.png", null, false);
        CreateLit("M_Grass", L2("Garden/Grass/Grass_Diffuse.jpg"), L2("Garden/Grass/Grass_Normal.png"), L2("Garden/Grass/Grass_MetallicSmoothness.png"), L2("Garden/Grass/Grass_AO.jpg"), false);
        CreateLit("M_Rock", L2("Garden/Rock/Rock_Diffuse.jpg"), L2("Garden/Rock/Rock_Normal.png"), L2("Garden/Rock/Rock_MetallicSmoothness.png"), null, false);
        CreateLit("M_Shrub", L2("Garden/Shrub/Shrub_DiffuseAlpha.png"), L2("Garden/Shrub/Shrub_Normal.png"), L2("Garden/Shrub/Shrub_MetallicSmoothness.png"), null, true);
        CreateLit("M_Fern", L2("Garden/Fern/Fern_DiffuseAlpha.png"), L2("Garden/Fern/Fern_Normal.png"), L2("Garden/Fern/Fern_MetallicSmoothness.png"), null, true);
        CreateLit("M_ConcreteWall", L3("Materials/ConcreteWall/ConcreteWall_Diffuse.jpg"), L3("Materials/ConcreteWall/ConcreteWall_Normal.png"), L3("Materials/ConcreteWall/ConcreteWall_MetallicSmoothness.png"), L3("Materials/ConcreteWall/ConcreteWall_AO.jpg"), false);
        CreateLit("M_ConcreteFloor", L3("Materials/ConcreteFloor/ConcreteFloor_Diffuse.jpg"), L3("Materials/ConcreteFloor/ConcreteFloor_Normal.png"), L3("Materials/ConcreteFloor/ConcreteFloor_MetallicSmoothness.png"), L3("Materials/ConcreteFloor/ConcreteFloor_AO.jpg"), false);
        CreateLit("M_RustyMetal", L3("Materials/RustyMetal/RustyMetal_Diffuse.jpg"), L3("Materials/RustyMetal/RustyMetal_Normal.png"), L3("Materials/RustyMetal/RustyMetal_MetallicSmoothness.png"), L3("Materials/RustyMetal/RustyMetal_AO.jpg"), false);
        CreateLit("M_Duct", L3("Props/Duct/Duct_Diffuse.jpg"), L3("Props/Duct/Duct_Normal.exr"), L3("Props/Duct/Duct_MetallicSmoothness.png"), null, false);
        CreateLit("M_Lamp", L3("Props/Lamp/Lamp_Diffuse.jpg"), L3("Props/Lamp/Lamp_Normal.png"), L3("Props/Lamp/Lamp_MetallicSmoothness.png"), null, false, L3("Props/Lamp/Lamp_Emission.png"));
        CreateLit("M_Barrel", L3("Props/Barrel/Barrel_Diffuse.jpg"), L3("Props/Barrel/Barrel_Normal.exr"), L3("Props/Barrel/Barrel_MetallicSmoothness.png"), null, false);
        CreateSkybox();
    }

    private static Material CreateLit(string name, string color, string normal, string metallicSmoothness, string ao, bool cutout, string emission = null)
    {
        string path = Materials + "/" + name + ".mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (mat == null) { mat = new Material(shader); AssetDatabase.CreateAsset(mat, path); }
        else mat.shader = shader;
        mat.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(color));
        Texture2D normalTex = AssetDatabase.LoadAssetAtPath<Texture2D>(normal);
        if (normalTex != null) { mat.SetTexture("_BumpMap", normalTex); mat.EnableKeyword("_NORMALMAP"); }
        Texture2D metal = AssetDatabase.LoadAssetAtPath<Texture2D>(metallicSmoothness);
        if (metal != null) { mat.SetTexture("_MetallicGlossMap", metal); mat.EnableKeyword("_METALLICSPECGLOSSMAP"); }
        Texture2D occ = string.IsNullOrEmpty(ao) ? null : AssetDatabase.LoadAssetAtPath<Texture2D>(ao);
        if (occ != null) { mat.SetTexture("_OcclusionMap", occ); mat.EnableKeyword("_OCCLUSIONMAP"); }
        if (!string.IsNullOrEmpty(emission))
        {
            Texture2D em = AssetDatabase.LoadAssetAtPath<Texture2D>(emission);
            if (em != null) { mat.SetTexture("_EmissionMap", em); mat.SetColor("_EmissionColor", Color.white * 1.2f); mat.EnableKeyword("_EMISSION"); }
        }
        mat.SetFloat("_Smoothness", 1f);
        if (cutout)
        {
            mat.SetFloat("_AlphaClip", 1f); mat.SetFloat("_Cutoff", 0.35f);
            mat.EnableKeyword("_ALPHATEST_ON");
            mat.SetFloat("_Cull", 0f);
        }
        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static void CreateSkybox()
    {
        string path = Materials + "/M_Sky_SunnyRoseGarden.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        Shader shader = Shader.Find("Skybox/Panoramic");
        if (mat == null) { mat = new Material(shader); AssetDatabase.CreateAsset(mat, path); }
        else mat.shader = shader;
        mat.SetTexture("_MainTex", AssetDatabase.LoadAssetAtPath<Texture>(L2("Garden/Sky/SunnyRoseGarden_2K.hdr")));
        mat.SetFloat("_Exposure", 0.9f);
        EditorUtility.SetDirty(mat);
    }

    private static AnimatorController CreateWalkController()
    {
        string path = Controllers + "/WolfSpider_Walk.controller";
        AssetDatabase.DeleteAsset(path);
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        AnimationClip[] clips = AssetDatabase.LoadAllAssetsAtPath(Root + "/External/Nivel2/WolfSpider/source/Wolf_spider.fbx")
            .OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview__")).ToArray();
        AnimationClip walk = clips.FirstOrDefault(c => c.name.ToLowerInvariant().Contains("spider walking") && !c.name.ToLowerInvariant().Contains("fast") && !c.name.ToLowerInvariant().Contains("back")) ?? clips.FirstOrDefault();
        if (walk != null)
        {
            AnimatorState state = controller.AddMotion(walk);
            state.name = "Slow Walk";
            state.speed = 0.45f;
            controller.layers[0].stateMachine.defaultState = state;
        }
        return controller;
    }

    private static void ReconfigureLevel1(string backupFolder)
    {
        string path = "Assets/Scenes/Nivel1.unity";
        Backup(path, backupFolder);
        Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        RemoveOldEnvironment(scene, 1);
        GameObject env = NewRoot("NP_ENV_NIVEL1_CONSULTORIO");
        GameObject office = InstantiateModel(Root + "/External/Nivel1/ModernOffice/Modern Office.fbx", env.transform, "Consultorio_Moderno");
        FitAndPlace(office, 8f, new Vector3(0f, 0f, 3.3f), Quaternion.Euler(0f, 180f, 0f));
        DisableChildrenContaining(office, new[] { "Table Decoration", "Table", "Chair" });
        UpgradeOfficeMaterials(office);

        Material floorMat = Mat("M_Planks");
        CreateCube("Piso_Clinico", env.transform, new Vector3(0, -0.08f, 3), new Vector3(8, 0.15f, 8), floorMat, true);
        AddExistingPrefab("Assets/UnityTechnologies/Basic Asset Pack Interior/Prefabs/Furniture/SofaArmChair.prefab", env.transform, new Vector3(-1.1f, 0f, 2.2f), Quaternion.Euler(0, 25, 0), 1f);
        AddExistingPrefab("Assets/UnityTechnologies/Basic Asset Pack Interior/Prefabs/Furniture/SofaArmChair.prefab", env.transform, new Vector3(1.1f, 0f, 2.2f), Quaternion.Euler(0, -25, 0), 1f);
        AddExistingPrefab("Assets/UnityTechnologies/Basic Asset Pack Interior/Prefabs/Furniture/TableRectangleShort.prefab", env.transform, new Vector3(0, 0f, 2.8f), Quaternion.identity, 0.8f);
        AddExistingPrefab("Assets/UnityTechnologies/Basic Asset Pack Interior/Prefabs/Props/PlantPotRoundMedium.prefab", env.transform, new Vector3(-2.6f, 0f, 4.8f), Quaternion.identity, 1f);
        SetupLight(scene, new Color(1f, 0.91f, 0.78f), 1.05f, Quaternion.Euler(45, -30, 0));
        RenderSettings.ambientMode = AmbientMode.Flat; RenderSettings.ambientLight = new Color(0.55f, 0.55f, 0.58f);
        AddLoopAudio(env, Root + "/Audio/Nivel1/Ambiente_Consultorio.mp3", 0.08f);
        AddOneShotAudio(env, Root + "/Audio/Nivel1/Respiracion_Inhala_Exhala.mp3", "Respiracion_Audio", 0.25f);
        SetupDesktopRig(scene, new Vector3(0, 0, -0.8f));
        SetupLevel1Logic(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void SetupLevel1Logic(Scene scene)
    {
        GameObject setup = FindOrCreate(scene, "Nivel1_AutoSetup");
        Level1ExposureController ctrl = setup.GetComponent<Level1ExposureController>() ?? setup.AddComponent<Level1ExposureController>();
        ctrl.breathingGuide = UnityEngine.Object.FindFirstObjectByType<BreathingSphereGuide>(FindObjectsInactive.Include);
        ctrl.continueButton = FindByName<Button>("Button_Continuar_Nivel2");
        ctrl.statusText = FindByName<TMP_Text>("Text (TMP)") ?? UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault();
        ctrl.spiderCards.Clear();
        for (int i = 1; i <= 4; i++)
        {
            GameObject card = GameObject.Find("Spider2D_Card_" + i);
            if (card != null)
            {
                card.transform.position = new Vector3(0f, 1.65f, 2.4f);
                card.transform.rotation = Quaternion.identity;
                ctrl.spiderCards.Add(card);
            }
        }
        if (ctrl.continueButton != null) ctrl.continueButton.gameObject.SetActive(false);
    }

    private static void ReconfigureLevel2(string backupFolder)
    {
        string path = "Assets/Scenes/Nivel2.unity";
        Backup(path, backupFolder);
        Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        RemoveOldEnvironment(scene, 2);
        GameObject env = NewRoot("NP_ENV_NIVEL2_JARDIN_REALISTA");
        CreateCube("Terreno_Cesped", env.transform, new Vector3(0, -0.1f, 8), new Vector3(32, 0.2f, 32), Mat("M_Grass"), true);
        CreateCube("Sendero", env.transform, new Vector3(0, 0.015f, 6), new Vector3(2.2f, 0.03f, 16), Mat("M_ConcreteFloor"), true);

        GameObject rock = InstantiateModel(L2("Garden/Rock/Rock07.fbx"), env.transform, "Roca_Realista");
        FitAndPlace(rock, 2.1f, new Vector3(0, 0, 10f), Quaternion.Euler(0, 25, 0));
        // La roca es decorativa y está detrás de la zona de exclusión.
        // No se crea ni se consulta ningún BoxCollider en Roca_Realista.
        foreach (Collider existingCollider in rock.GetComponents<Collider>())
            UnityEngine.Object.DestroyImmediate(existingCollider);
        Bounds rockBounds = GetBounds(rock);

        GameObject spider = CreateSpider(env.transform, "NP_WolfSpider_Nivel2_Static", new Vector3(0, rockBounds.max.y + 0.03f, 10f), 0.55f, null, false);
        AranaController arana = spider.GetComponent<AranaController>() ?? spider.AddComponent<AranaController>();
        arana.defaultPattern = MovementPattern.IDLE;
        Animator animator = spider.GetComponentInChildren<Animator>(); if (animator != null) animator.enabled = false;

        PlaceGardenProps(env.transform);
        SetupLight(scene, new Color(1f, 0.96f, 0.84f), 1.2f, Quaternion.Euler(48, -28, 0));
        RenderSettings.skybox = Mat("M_Sky_SunnyRoseGarden");
        RenderSettings.ambientMode = AmbientMode.Skybox;
        DynamicGI.UpdateEnvironment();
        AddLoopAudio(env, Root + "/Audio/Nivel2/Ambiente_Jardin.mp3", 0.18f);
        SetupDesktopRig(scene, Vector3.zero);
        SetupLevel2Logic(scene, spider.transform);
        EditorSceneManager.SaveScene(scene);
    }

    private static void SetupLevel2Logic(Scene scene, Transform spider)
    {
        GameObject setup = FindOrCreate(scene, "Nivel2_AutoSetup");
        RemoveComponentByName(setup, "Level2ExclusionZone");
        Level2SafeDistanceController safe = setup.GetComponent<Level2SafeDistanceController>() ?? setup.AddComponent<Level2SafeDistanceController>();
        safe.spiderTarget = spider; safe.safeDistance = 8f; safe.hud = UnityEngine.Object.FindFirstObjectByType<Level2HUD>(FindObjectsInactive.Include);
        Level2GazeTimer gaze = setup.GetComponent<Level2GazeTimer>() ?? setup.AddComponent<Level2GazeTimer>();
        gaze.spiderTarget = spider; gaze.requiredGazeSeconds = 30f; gaze.gazeAngleThreshold = 10f; gaze.requireLineOfSight = true;
        MetricsCollector metrics = UnityEngine.Object.FindFirstObjectByType<MetricsCollector>(FindObjectsInactive.Include);
        if (metrics != null) { metrics.level = 2; metrics.spider = spider; }
    }

    private static void PlaceGardenProps(Transform parent)
    {
        Vector3[] shrubPos = { new Vector3(-5,0,5), new Vector3(5,0,5), new Vector3(-6,0,10), new Vector3(6,0,11), new Vector3(-4,0,15), new Vector3(4,0,15) };
        for (int i=0;i<shrubPos.Length;i++)
        {
            GameObject g=InstantiateModel(L2("Garden/Shrub/Shrub03.fbx"),parent,"Arbusto_"+(i+1));
            FitAndPlace(g, UnityEngine.Random.Range(1.2f,1.8f), shrubPos[i], Quaternion.Euler(0,i*47,0)); AssignMaterial(g,Mat("M_Shrub"));
        }
        Vector3[] fernPos = { new Vector3(-2.8f,0,7), new Vector3(2.7f,0,7.5f), new Vector3(-3,0,11), new Vector3(3.4f,0,12), new Vector3(-2.2f,0,14), new Vector3(2.1f,0,15) };
        for (int i=0;i<fernPos.Length;i++)
        {
            GameObject g=InstantiateModel(L2("Garden/Fern/Fern02.fbx"),parent,"Helecho_"+(i+1));
            FitAndPlace(g, UnityEngine.Random.Range(0.7f,1.1f), fernPos[i], Quaternion.Euler(0,i*61,0)); AssignMaterial(g,Mat("M_Fern"));
        }
    }

    private static void ReconfigureLevel3(string backupFolder, AnimatorController walkController)
    {
        string path = "Assets/Scenes/Nivel3.unity";
        Backup(path, backupFolder);
        Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        RemoveOldEnvironment(scene, 3);
        GameObject env = NewRoot("NP_ENV_NIVEL3_SOTANO");
        Material wall = Mat("M_ConcreteWall"), floor = Mat("M_ConcreteFloor");
        CreateCube("Piso_Sotano", env.transform, new Vector3(0,-0.1f,5), new Vector3(10,0.2f,12), floor, true);
        CreateCube("Techo_Sotano", env.transform, new Vector3(0,3.2f,5), new Vector3(10,0.2f,12), wall, true);
        CreateCube("Pared_Izquierda", env.transform, new Vector3(-5,1.55f,5), new Vector3(0.2f,3.3f,12), wall, true);
        CreateCube("Pared_Derecha", env.transform, new Vector3(5,1.55f,5), new Vector3(0.2f,3.3f,12), wall, true);
        CreateCube("Pared_Fondo", env.transform, new Vector3(0,1.55f,11), new Vector3(10,3.3f,0.2f), wall, true);

        GameObject duct=InstantiateModel(L3("Props/Duct/Duct.fbx"),env.transform,"Ductos_Industriales"); FitAndPlace(duct,5f,new Vector3(0,2.65f,7),Quaternion.Euler(0,90,0)); AssignMaterial(duct,Mat("M_Duct"));
        GameObject lamp=InstantiateModel(L3("Props/Lamp/Lamp.fbx"),env.transform,"Lampara_Emergencia"); FitAndPlace(lamp,0.8f,new Vector3(0,2.35f,10.7f),Quaternion.Euler(0,180,0)); AssignMaterial(lamp,Mat("M_Lamp"));
        Light emergency = new GameObject("Luz_Emergencia").AddComponent<Light>(); emergency.transform.SetParent(env.transform); emergency.transform.position=new Vector3(0,2.3f,8.5f); emergency.type=LightType.Point; emergency.range=8f; emergency.intensity=1.2f; emergency.color=new Color(1f,0.45f,0.25f); emergency.shadows=LightShadows.Soft;
        for(int i=0;i<2;i++){ GameObject b=InstantiateModel(L3("Props/Barrel/Barrel.fbx"),env.transform,"Barril_"+(i+1)); FitAndPlace(b,0.9f,new Vector3(i==0?-3.8f:3.8f,0,8+i),Quaternion.Euler(0,i*35,0)); AssignMaterial(b,Mat("M_Barrel")); AddBoundsCollider(b); }

        Transform a=new GameObject("Patrol_A").transform; a.SetParent(env.transform); a.position=new Vector3(-1.5f,0.18f,5f);
        Transform bpt=new GameObject("Patrol_B").transform; bpt.SetParent(env.transform); bpt.position=new Vector3(1.5f,0.18f,5f);
        GameObject spider=CreateSpider(env.transform,"NP_WolfSpider_Nivel3_Patrol",a.position,0.62f,walkController,true);
        Level3SpiderPatrolV2 patrol=spider.AddComponent<Level3SpiderPatrolV2>(); patrol.pointA=a; patrol.pointB=bpt; patrol.animator=spider.GetComponentInChildren<Animator>(); patrol.moveSpeed=0.16f; patrol.animatorSpeed=0.35f;
        AranaController arana=spider.GetComponent<AranaController>()??spider.AddComponent<AranaController>(); arana.defaultPattern=MovementPattern.IDLE;

        SetupLight(scene,new Color(0.42f,0.48f,0.62f),0.25f,Quaternion.Euler(60,-20,0));
        RenderSettings.ambientMode=AmbientMode.Flat; RenderSettings.ambientLight=new Color(0.045f,0.05f,0.07f);
        AddLoopAudio(env,Root+"/Audio/Nivel3/Ambiente_Sotano.mp3",0.16f);
        SetupDesktopRig(scene,Vector3.zero);
        SetupLevel3Logic(scene,spider.transform,patrol,env.transform);
        EditorSceneManager.SaveScene(scene);
    }

    private static void SetupLevel3Logic(Scene scene, Transform spider, Level3SpiderPatrolV2 patrol, Transform env)
    {
        GameObject setup=FindOrCreate(scene,"Nivel3_AutoSetup");
        RemoveComponentByName(setup,"Level3ProgressController"); RemoveComponentByName(setup,"Level3SpiderMovement"); RemoveComponentByName(setup,"Level3AvoidanceMonitor");
        foreach(GameObject old in scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).Where(t=>t.name.Contains("CP1_")||t.name.Contains("CP2_")||t.name.Contains("Checkpoint_3m")||t.name.Contains("Checkpoint_1m")).Select(t=>t.gameObject).Distinct().ToArray()) UnityEngine.Object.DestroyImmediate(old);
        Level3HUD hud=UnityEngine.Object.FindFirstObjectByType<Level3HUD>(FindObjectsInactive.Include);
        Level3ProgressControllerV2 progress=setup.GetComponent<Level3ProgressControllerV2>()??setup.AddComponent<Level3ProgressControllerV2>(); progress.hud=hud; progress.spiderPatrol=patrol; progress.metricsCollector=UnityEngine.Object.FindFirstObjectByType<MetricsCollector>(FindObjectsInactive.Include);
        CreateCheckpoint("CP1_3m",env,new Vector3(0,1f,2f),1,progress);
        CreateCheckpoint("CP2_1m",env,new Vector3(0,1f,4f),2,progress);
        GameObject overlay=CreateCalmOverlay();
        Level3AdaptiveCalmV2 calm=setup.GetComponent<Level3AdaptiveCalmV2>()??setup.AddComponent<Level3AdaptiveCalmV2>(); calm.spiderTarget=spider; calm.spiderPatrol=patrol; calm.hud=hud; calm.breathingOverlay=overlay; calm.metricsCollector=progress.metricsCollector; calm.calmAudio=overlay.GetComponent<AudioSource>();
        if(progress.metricsCollector!=null){progress.metricsCollector.level=3;progress.metricsCollector.spider=spider;}
    }

    private static void CreateCheckpoint(string name, Transform parent, Vector3 pos, int order, Level3ProgressControllerV2 progress)
    {
        GameObject cp=GameObject.CreatePrimitive(PrimitiveType.Cylinder); cp.name=name; cp.transform.SetParent(parent); cp.transform.position=pos; cp.transform.localScale=new Vector3(0.8f,1f,0.8f); cp.GetComponent<Collider>().isTrigger=true; cp.GetComponent<Renderer>().enabled=false;
        Level3CheckpointV2 c=cp.AddComponent<Level3CheckpointV2>(); c.checkpointOrder=order; c.progressController=progress;
    }

    private static GameObject CreateCalmOverlay()
    {
        GameObject canvasGO = new GameObject("Overlay_Respiracion_Nivel3", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(AudioSource));
        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        AudioSource audio = canvasGO.GetComponent<AudioSource>();
        audio.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(Root + "/Audio/Nivel1/Respiracion_Inhala_Exhala.mp3");
        audio.playOnAwake = false;
        audio.loop = true;
        audio.volume = 0.25f;

        GameObject panel = new GameObject("Panel", typeof(RectTransform), typeof(Image), typeof(CalmPulseUI));
        panel.transform.SetParent(canvasGO.transform, false);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.25f, 0.25f);
        rt.anchorMax = new Vector2(0.75f, 0.75f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0.04f, 0.08f, 0.12f, 0.9f);

        GameObject pulseGO = new GameObject("Pulso", typeof(RectTransform), typeof(Image));
        pulseGO.transform.SetParent(panel.transform, false);
        RectTransform pulseRect = pulseGO.GetComponent<RectTransform>();
        pulseRect.anchorMin = pulseRect.anchorMax = new Vector2(0.5f, 0.58f);
        pulseRect.sizeDelta = new Vector2(150f, 150f);
        Image pulseImage = pulseGO.GetComponent<Image>();
        pulseImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        pulseImage.color = new Color(0.3f, 0.65f, 1f, 0.9f);

        GameObject textGO = new GameObject("Mensaje", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGO.transform.SetParent(panel.transform, false);
        RectTransform tr = textGO.GetComponent<RectTransform>();
        tr.anchorMin = new Vector2(0f, 0f);
        tr.anchorMax = new Vector2(1f, 0.42f);
        tr.offsetMin = new Vector2(20, 20);
        tr.offsetMax = new Vector2(-20, -10);
        TextMeshProUGUI text = textGO.GetComponent<TextMeshProUGUI>();
        text.text = "PAUSA DE CALMA\nINHALA\n4 · 7 · 8";
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 34;
        text.color = Color.white;

        CalmPulseUI pulse = panel.GetComponent<CalmPulseUI>();
        pulse.pulse = pulseRect;
        pulse.instruction = text;
        canvasGO.SetActive(false);
        return canvasGO;
    }

    private static GameObject CreateSpider(Transform parent,string name,Vector3 pos,float targetSize,RuntimeAnimatorController controller,bool animate)
    {
        GameObject root=new GameObject(name);root.transform.SetParent(parent);root.transform.position=pos;SetLayerRecursively(root,spiderLayer);
        GameObject visual=InstantiateModel(Root+"/External/Nivel2/WolfSpider/source/Wolf_spider.fbx",root.transform,"Visual_WolfSpider"); FitChildToSize(visual,targetSize); Bounds visualBounds=GetBounds(visual); visual.transform.position += new Vector3(root.transform.position.x-visualBounds.center.x, root.transform.position.y-visualBounds.min.y, root.transform.position.z-visualBounds.center.z); AssignMaterial(visual,Mat("M_WolfSpider")); SetLayerRecursively(visual,spiderLayer);
        Animator animator=visual.GetComponentInChildren<Animator>(); if(animator!=null){animator.runtimeAnimatorController=controller;animator.enabled=animate;animator.applyRootMotion=false;}
        AddBoundsCollider(root); return root;
    }

    private static void SetupDesktopRig(Scene scene,Vector3 position)
    {
        GameObject old=GameObject.Find("DesktopTestRig"); if(old!=null) UnityEngine.Object.DestroyImmediate(old);
        GameObject rig=new GameObject("DesktopTestRig",typeof(CharacterController),typeof(NeuroPhobiaDesktopRig)); rig.transform.position=position;
        CharacterController cc=rig.GetComponent<CharacterController>();cc.height=1.75f;cc.radius=0.28f;cc.center=new Vector3(0,0.88f,0);cc.stepOffset=0.3f;
        GameObject cameraGO=new GameObject("DesktopCamera",typeof(Camera),typeof(AudioListener));cameraGO.transform.SetParent(rig.transform,false);cameraGO.transform.localPosition=new Vector3(0,1.65f,0);cameraGO.tag="MainCamera";Camera cam=cameraGO.GetComponent<Camera>();cam.nearClipPlane=0.05f;cam.farClipPlane=100f;rig.GetComponent<NeuroPhobiaDesktopRig>().desktopCamera=cam;
        SceneManager.MoveGameObjectToScene(rig,scene);
    }

    private static void RemoveOldEnvironment(Scene scene,int level)
    {
        string[] exact={"Old House scene","Door","Grass 02","3","Chair_01","Table_01","frasco","Roca_DistanciaSegura","CaveSpider_Nivel2_Static","CaveSpider_Nivel3_SlowPatrol","ZonaExclusion_8m"};
        foreach(GameObject root in scene.GetRootGameObjects())
        {
            if(root.name.StartsWith("NP_ENV_")||exact.Contains(root.name)||root.name.StartsWith("bathroomFloor")) UnityEngine.Object.DestroyImmediate(root);
        }
        foreach(GameObject root in scene.GetRootGameObjects())
        {
            if(root.name=="Directional Light") continue;
            if(level==3 && (root.name.StartsWith("Sotano_Wall_")||root.name.StartsWith("Checkpoint_")||root.name.StartsWith("CP1_")||root.name.StartsWith("CP2_"))) UnityEngine.Object.DestroyImmediate(root);
        }
    }

    private static void UpgradeOfficeMaterials(GameObject office)
    {
        Dictionary<string,string> map = new Dictionary<string,string>() {
            {"Structure","Structure_c.png"},{"Table","Table_c.png"},{"Carpet","Carpet_c.png"},{"Painting","Paintings_c.png"},{"Lights","Lights_c.png"},{"Table Decoration","Table Decoration_c.png"},{"Books","Books_c.png"},{"Plant","Plant_c.png"},{"Chair","Chair_c.png"}
        };
        foreach(Renderer r in office.GetComponentsInChildren<Renderer>(true))
        {
            string key=map.Keys.FirstOrDefault(k=>r.name.Contains(k,StringComparison.OrdinalIgnoreCase)||(r.sharedMaterial!=null&&r.sharedMaterial.name.Contains(k,StringComparison.OrdinalIgnoreCase)));
            if(key==null) continue;
            string matPath=Materials+"/M_Office_"+key.Replace(" ","")+".mat";
            Material mat=AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if(mat==null){Shader sh=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");mat=new Material(sh);mat.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/External/Nivel1/ModernOffice/Modern Office.fbm/"+map[key]));AssetDatabase.CreateAsset(mat,matPath);}
            r.sharedMaterial=mat;
        }
    }

    private static void DisableChildrenContaining(GameObject root,string[] terms)
    {
        foreach(Transform t in root.GetComponentsInChildren<Transform>(true)) if(t!=root.transform&&terms.Any(term => t.name.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)) t.gameObject.SetActive(false);
    }

    private static void SetupLight(Scene scene,Color colorValue,float intensity,Quaternion rotation)
    {
        Light light=UnityEngine.Object.FindObjectsByType<Light>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault(l=>l.type==LightType.Directional);
        if(light==null){GameObject go=new GameObject("Directional Light");SceneManager.MoveGameObjectToScene(go,scene);light=go.AddComponent<Light>();light.type=LightType.Directional;}
        light.color=colorValue;light.intensity=intensity;light.shadows=LightShadows.Soft;light.transform.rotation=rotation;
    }

    private static void AddLoopAudio(GameObject parent,string clipPath,float volume)
    {
        AudioSource source=parent.AddComponent<AudioSource>();source.clip=AssetDatabase.LoadAssetAtPath<AudioClip>(clipPath);source.loop=true;source.playOnAwake=true;source.volume=volume;source.spatialBlend=0f;
    }
    private static void AddOneShotAudio(GameObject parent,string clipPath,string name,float volume)
    {
        GameObject go=new GameObject(name);go.transform.SetParent(parent.transform);AudioSource source=go.AddComponent<AudioSource>();source.clip=AssetDatabase.LoadAssetAtPath<AudioClip>(clipPath);source.playOnAwake=false;source.volume=volume;source.spatialBlend=0f;
    }

    private static GameObject CreateCube(string name,Transform parent,Vector3 position,Vector3 scale,Material material,bool collider)
    {
        GameObject go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent);go.transform.position=position;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=material;if(!collider)UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());return go;
    }

    private static GameObject InstantiateModel(string path,Transform parent,string name)
    {
        GameObject asset=AssetDatabase.LoadAssetAtPath<GameObject>(path); if(asset==null)throw new FileNotFoundException(path);
        GameObject go=(GameObject)PrefabUtility.InstantiatePrefab(asset);go.name=name;go.transform.SetParent(parent,false);return go;
    }

    private static GameObject AddExistingPrefab(string path,Transform parent,Vector3 position,Quaternion rotation,float scale)
    {
        GameObject asset=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(asset==null)return null;GameObject go=(GameObject)PrefabUtility.InstantiatePrefab(asset);go.transform.SetParent(parent);go.transform.position=position;go.transform.rotation=rotation;go.transform.localScale*=scale;return go;
    }

    private static void FitAndPlace(GameObject go,float targetMax,Vector3 position,Quaternion rotation)
    {
        go.transform.rotation=rotation;FitChildToSize(go,targetMax);Bounds b=GetBounds(go);go.transform.position+=position-new Vector3(b.center.x,b.min.y,b.center.z);
    }
    private static void FitChildToSize(GameObject go,float targetMax)
    {
        Bounds b=GetBounds(go);float max=Mathf.Max(b.size.x,b.size.y,b.size.z);if(max>0.0001f)go.transform.localScale*=targetMax/max;
    }
    private static Bounds GetBounds(GameObject go)
    {
        Renderer[] rs=go.GetComponentsInChildren<Renderer>(true);if(rs.Length==0)return new Bounds(go.transform.position,Vector3.one);Bounds b=rs[0].bounds;foreach(Renderer r in rs.Skip(1))b.Encapsulate(r.bounds);return b;
    }
    private static void AddBoundsCollider(GameObject go)
    {
        if (go == null) return;

        Bounds b = GetBounds(go);
        BoxCollider c = go.GetComponent<BoxCollider>();

        // No usar ?? con UnityEngine.Object. Un componente destruido puede parecer no nulo
        // para C#, aunque Unity lo considere nulo (fake null).
        if (c == null)
            c = go.AddComponent<BoxCollider>();

        c.center = go.transform.InverseTransformPoint(b.center);
        Vector3 localSize = go.transform.InverseTransformVector(b.size);
        c.size = new Vector3(
            Mathf.Abs(localSize.x),
            Mathf.Abs(localSize.y),
            Mathf.Abs(localSize.z));
    }
    private static void AssignMaterial(GameObject go,Material mat){foreach(Renderer r in go.GetComponentsInChildren<Renderer>(true))r.sharedMaterial=mat;}
    private static Material Mat(string name)=>AssetDatabase.LoadAssetAtPath<Material>(Materials+"/"+name+".mat");
    private static string L1(string suffix)=>Root+"/External/Nivel1/"+suffix;
    private static string L2(string suffix)=>Root+"/External/Nivel2/"+suffix;
    private static string L3(string suffix)=>Root+"/External/Nivel3/"+suffix;
    private static GameObject NewRoot(string name){GameObject go=new GameObject(name);SceneManager.MoveGameObjectToScene(go,SceneManager.GetActiveScene());return go;}
    private static GameObject FindOrCreate(Scene scene,string name){GameObject go=scene.GetRootGameObjects().FirstOrDefault(x=>x.name==name);if(go==null){go=new GameObject(name);SceneManager.MoveGameObjectToScene(go,scene);}return go;}
    private static T FindByName<T>(string name) where T:Component{GameObject go=GameObject.Find(name);return go==null?null:go.GetComponent<T>();}
    private static void RemoveComponentByName(GameObject root,string typeName){foreach(MonoBehaviour mb in root.GetComponents<MonoBehaviour>().Where(x=>x!=null&&x.GetType().Name==typeName).ToArray())UnityEngine.Object.DestroyImmediate(mb);}
    private static void SetLayerRecursively(GameObject go,int layer){go.layer=layer;foreach(Transform t in go.transform)SetLayerRecursively(t.gameObject,layer);}

    private static int EnsureLayer(string layerName)
    {
        int existing=LayerMask.NameToLayer(layerName);if(existing>=0)return existing;
        SerializedObject tagManager=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);SerializedProperty layers=tagManager.FindProperty("layers");
        for(int i=8;i<32;i++){SerializedProperty p=layers.GetArrayElementAtIndex(i);if(string.IsNullOrEmpty(p.stringValue)){p.stringValue=layerName;tagManager.ApplyModifiedProperties();return i;}}
        throw new InvalidOperationException("No hay una capa libre para Spider.");
    }

    private static string CreateBackupFolder()
    {
        string folder="Assets/Scenes/Backups_Niveles123/"+DateTime.Now.ToString("yyyyMMdd_HHmmss");Directory.CreateDirectory(folder);AssetDatabase.Refresh();return folder;
    }
    private static void Backup(string scenePath,string folder){AssetDatabase.CopyAsset(scenePath,folder+"/"+Path.GetFileName(scenePath));}
}
#endif
