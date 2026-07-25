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
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class NeuroPhobiaLevels23PolishInstaller
{
    private const string WolfModelPath = "Assets/NeuroPhobia_Levels123/External/Nivel2/WolfSpider/source/Wolf_spider.fbx";
    private const string GeneratedFolder = "Assets/NeuroPhobia_Level23Polish/Generated";

    [MenuItem("Tools/NeuroPhobia/Configurar Nivel 2 FINAL")]
    public static void ConfigureLevel2()
    {
        if (!CanRun()) return;
        OpenScene("Assets/Scenes/Nivel2.unity");
        Scene scene = SceneManager.GetActiveScene();
        GameObject spider = FindByExactOrContains(scene, "NP_WolfSpider_Nivel2_Static", "wolfspider");
        if (spider == null) { Fail("No encontré la araña del Nivel 2."); return; }

        DisableComponentsByType(scene, "Level2GazeTimer", "Level2HUD", "Level2ExclusionZone", "Level2SafeDistanceController");
        SetObjectActive(scene, "CanvasHUD_Nivel2", false);
        SetObjectActive(scene, "ZonaExclusion_8m", false);

        AnimatorController idleController = CreateAnimationController(true);
        Animator animator = PrepareAnimator(spider, idleController);
        Transform visual = FindChildContaining(spider.transform, "Visual_WolfSpider") ?? (spider.transform.childCount > 0 ? spider.transform.GetChild(0) : spider.transform);
        Level2SpiderIdleLife life = spider.GetComponent<Level2SpiderIdleLife>() ?? spider.AddComponent<Level2SpiderIdleLife>();
        SerializedObject lifeSO = new SerializedObject(life);
        lifeSO.FindProperty("visualRoot").objectReferenceValue = visual;
        lifeSO.FindProperty("animator").objectReferenceValue = animator;
        lifeSO.FindProperty("useAnimator").boolValue = idleController != null;
        lifeSO.ApplyModifiedPropertiesWithoutUndo();

        GameObject setup = FindRoot(scene, "NP_NIVEL2_FINAL") ?? NewRoot(scene, "NP_NIVEL2_FINAL");
        Level2ScreenGazeFlow flow = setup.GetComponent<Level2ScreenGazeFlow>() ?? setup.AddComponent<Level2ScreenGazeFlow>();
        Canvas canvas = RecreateCanvas(scene, "NP_UI_NIVEL2_FIJA", 2100);
        TMP_Text timer = CreateTopPanel(canvas.transform, "Panel_Temporizador_N2", "NIVEL 2 · DISTANCIA SEGURA", out Image fill, out TMP_Text warning);
        GameObject completion = CreateCompletionPanel(canvas.transform, "Panel_Completado_N2", "¡Felicitaciones!\n\nCompletaste el Nivel 2", out TMP_Text completionText);

        SerializedObject flowSO = new SerializedObject(flow);
        flowSO.FindProperty("playerCamera").objectReferenceValue = FindActiveCamera(scene);
        flowSO.FindProperty("spiderTarget").objectReferenceValue = spider.transform;
        flowSO.FindProperty("playerRoot").objectReferenceValue = FindPlayer(scene)?.transform;
        flowSO.FindProperty("timerText").objectReferenceValue = timer;
        flowSO.FindProperty("warningText").objectReferenceValue = warning;
        flowSO.FindProperty("progressFill").objectReferenceValue = fill;
        flowSO.FindProperty("completionPanel").objectReferenceValue = completion;
        flowSO.FindProperty("completionText").objectReferenceValue = completionText;
        flowSO.FindProperty("requiredSeconds").floatValue = 30f;
        flowSO.FindProperty("safeDistance").floatValue = 8f;
        flowSO.FindProperty("completionDelay").floatValue = 5f;
        flowSO.FindProperty("nextSceneName").stringValue = "Nivel3";
        flowSO.ApplyModifiedPropertiesWithoutUndo();

        completion.SetActive(false);
        Save(scene);
        EditorUtility.DisplayDialog("NeuroPhobia", "Nivel 2 configurado.\n\nHUD fijo en pantalla, mejor intento, araña con movimiento sutil, felicitaciones y paso automático al Nivel 3.", "Aceptar");
    }

    [MenuItem("Tools/NeuroPhobia/Configurar Nivel 3 FINAL")]
    public static void ConfigureLevel3()
    {
        if (!CanRun()) return;
        OpenScene("Assets/Scenes/Nivel3.unity");
        Scene scene = SceneManager.GetActiveScene();
        RemoveOldLevel3Placeholders(scene);

        GameObject environment = FindByExactOrContains(scene, "NP_ENV_NIVEL3_SOTANO", "nivel3_sotano");
        GameObject spider = FindByExactOrContains(scene, "NP_WolfSpider_Nivel3_Patrol", "wolfspider_nivel3");
        if (environment == null || spider == null) { Fail("No encontré el sótano nuevo o la araña del Nivel 3."); return; }

        SetObjectActive(scene, "CanvasHUD_Nivel3", false);
        DisableComponentsByType(scene, "Level3ProgressControllerV2", "Level3CheckpointV2", "Level3ProgressController", "Level3Checkpoint");
        DestroyNamed(scene, "CP1_3m", "CP2_1m", "Checkpoint_3m", "Checkpoint_1m");

        AnimatorController locomotion = CreateAnimationController(false);
        Animator animator = PrepareAnimator(spider, locomotion);
        Transform visual = FindChildContaining(spider.transform, "Visual_WolfSpider") ?? (spider.transform.childCount > 0 ? spider.transform.GetChild(0) : spider.transform);

        Level3SpiderPatrolV2 patrol = spider.GetComponent<Level3SpiderPatrolV2>() ?? spider.AddComponent<Level3SpiderPatrolV2>();
        Transform pointA = FindOrCreatePoint(environment.transform, "Patrol_A", new Vector3(-1.8f, 0.18f, 5.4f));
        Transform pointB = FindOrCreatePoint(environment.transform, "Patrol_B", new Vector3(1.8f, 0.18f, 5.4f));
        patrol.pointA = pointA; patrol.pointB = pointB; patrol.animator = animator; patrol.visualRoot = visual;
        patrol.moveSpeed = 0.13f; patrol.animatorSpeed = 0.28f; patrol.turnSpeed = 4.5f;
        AranaController arana = spider.GetComponent<AranaController>(); if (arana != null) arana.enabled = false;

        AddLevel3Decoration(scene, environment.transform);

        GameObject orbRoot = FindRoot(scene, "NP_ORBES_NIVEL3") ?? NewRoot(scene, "NP_ORBES_NIVEL3");
        orbRoot.transform.SetParent(environment.transform, false);
        ClearChildren(orbRoot.transform);
        Vector3 playerPosition = FindPlayer(scene)?.transform.position ?? Vector3.zero;
        Vector3 spiderPosition = spider.transform.position;
        Vector3 direction = playerPosition - spiderPosition; direction.y = 0f;
        if (direction.sqrMagnitude < 0.01f) direction = Vector3.back;
        direction.Normalize();
        GameObject orb1 = CreateOrb(orbRoot.transform, "Orbe_1_3m", spiderPosition + direction * 3f + Vector3.up * 0.75f, new Color(0.20f, 0.75f, 1f));
        GameObject orb2 = CreateOrb(orbRoot.transform, "Orbe_2_1m", spiderPosition + direction * 1f + Vector3.up * 0.75f, new Color(1f, 0.58f, 0.18f));

        GameObject setup = FindRoot(scene, "NP_NIVEL3_FINAL") ?? NewRoot(scene, "NP_NIVEL3_FINAL");
        Level3OrbSequenceFlow flow = setup.GetComponent<Level3OrbSequenceFlow>() ?? setup.AddComponent<Level3OrbSequenceFlow>();
        Canvas canvas = RecreateCanvas(scene, "NP_UI_NIVEL3_FIJA", 2100);
        TMP_Text progress = CreateTopPanel(canvas.transform, "Panel_Progreso_N3", "NIVEL 3 · APROXIMACIÓN PROGRESIVA", out Image fill, out TMP_Text message);
        GameObject completion = CreateCompletionPanel(canvas.transform, "Panel_Completado_N3", "¡Felicitaciones!\n\nCompletaste el Nivel 3", out TMP_Text completionText);

        SerializedObject flowSO = new SerializedObject(flow);
        flowSO.FindProperty("playerRoot").objectReferenceValue = FindPlayer(scene)?.transform;
        SetObjectArray(flowSO.FindProperty("orbs"), new List<GameObject>{orb1, orb2});
        flowSO.FindProperty("progressText").objectReferenceValue = progress;
        flowSO.FindProperty("messageText").objectReferenceValue = message;
        flowSO.FindProperty("progressFill").objectReferenceValue = fill;
        flowSO.FindProperty("completionPanel").objectReferenceValue = completion;
        flowSO.FindProperty("completionText").objectReferenceValue = completionText;
        flowSO.FindProperty("spiderPatrol").objectReferenceValue = patrol;
        flowSO.FindProperty("completionDelay").floatValue = 5f;
        flowSO.FindProperty("nextSceneName").stringValue = "Nivel4";
        flowSO.ApplyModifiedPropertiesWithoutUndo();
        completion.SetActive(false);

        Save(scene);
        EditorUtility.DisplayDialog("NeuroPhobia", "Nivel 3 configurado.\n\nSe eliminaron las paredes antiguas, se mejoró la patrulla, se añadió decoración y se crearon dos orbes visibles.", "Aceptar");
    }

    private static bool CanRun()
    {
        if (EditorApplication.isPlaying) { Fail("Detén Play antes de ejecutar el comando."); return false; }
        return true;
    }

    private static void OpenScene(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException(path);
        if (SceneManager.GetActiveScene().path != path) EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
    }

    private static void RemoveOldLevel3Placeholders(Scene scene)
    {
        string[] prefixes = { "Sotano_Wall_", "Sotano_Floor", "Sotano_Ceiling", "Sotano_Techo" };
        foreach (Transform t in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToArray())
            if (prefixes.Any(p => t.name.StartsWith(p, StringComparison.OrdinalIgnoreCase))) UnityEngine.Object.DestroyImmediate(t.gameObject);
    }

    private static void AddLevel3Decoration(Scene scene, Transform environment)
    {
        GameObject old = FindByExactOrContains(scene, "NP_DECORACION_EXTRA_NIVEL3", "decoracion_extra_nivel3");
        if (old != null) UnityEngine.Object.DestroyImmediate(old);
        GameObject root = new GameObject("NP_DECORACION_EXTRA_NIVEL3"); root.transform.SetParent(environment, false);
        Material metal = FindMaterial("M_RustyMetal"); Material concrete = FindMaterial("M_ConcreteWall"); Material barrelMat = FindMaterial("M_Barrel"); Material ductMat = FindMaterial("M_Duct"); Material lampMat = FindMaterial("M_Lamp");

        string barrelPath = "Assets/NeuroPhobia_Levels123/External/Nivel3/Props/Barrel/Barrel.fbx";
        Vector3[] barrelPositions = { new Vector3(-4.15f,0f,2f), new Vector3(-4.1f,0f,3f), new Vector3(4.1f,0f,7.8f), new Vector3(3.5f,0f,9.1f) };
        for (int i=0;i<barrelPositions.Length;i++)
        {
            GameObject b = InstantiateModel(barrelPath, root.transform, "Barril_Extra_"+(i+1));
            if (b != null) { FitAndPlace(b,0.9f,barrelPositions[i],Quaternion.Euler(0,i*31f,0)); AssignMaterial(b,barrelMat); AddLocalBoundsCollider(b); }
        }

        string ductPath = "Assets/NeuroPhobia_Levels123/External/Nivel3/Props/Duct/Duct.fbx";
        GameObject duct1=InstantiateModel(ductPath,root.transform,"Ducto_Lateral_1"); if(duct1!=null){FitAndPlace(duct1,3.8f,new Vector3(-3.6f,2.65f,4f),Quaternion.Euler(0,0,90));AssignMaterial(duct1,ductMat);}
        GameObject duct2=InstantiateModel(ductPath,root.transform,"Ducto_Lateral_2"); if(duct2!=null){FitAndPlace(duct2,3.2f,new Vector3(3.7f,2.55f,8f),Quaternion.Euler(0,180,90));AssignMaterial(duct2,ductMat);}

        string lampPath = "Assets/NeuroPhobia_Levels123/External/Nivel3/Props/Lamp/Lamp.fbx";
        Vector3[] lampPositions={new Vector3(-4.75f,2.1f,3.5f),new Vector3(4.75f,2.1f,7.3f)};
        for(int i=0;i<lampPositions.Length;i++)
        {
            GameObject l=InstantiateModel(lampPath,root.transform,"Lampara_Lateral_"+(i+1)); if(l!=null){FitAndPlace(l,0.65f,lampPositions[i],Quaternion.Euler(0,i==0?90:-90,0));AssignMaterial(l,lampMat);}
            GameObject lightGO=new GameObject("Luz_Lateral_"+(i+1));lightGO.transform.SetParent(root.transform);lightGO.transform.position=lampPositions[i]+new Vector3(i==0?0.35f:-0.35f,-0.1f,0f);Light light=lightGO.AddComponent<Light>();light.type=LightType.Point;light.range=5f;light.intensity=0.65f;light.color=new Color(1f,0.42f,0.22f);light.shadows=LightShadows.Soft;
        }

        CreateShelf(root.transform,new Vector3(-4.45f,1.05f,8.4f),metal);
        CreateCrate(root.transform,"Caja_1",new Vector3(3.9f,0.35f,2.2f),new Vector3(1.2f,0.7f,1f),concrete);
        CreateCrate(root.transform,"Caja_2",new Vector3(3.6f,0.30f,3.25f),new Vector3(0.9f,0.6f,0.9f),metal);
        CreateCrate(root.transform,"Caja_3",new Vector3(-3.6f,0.25f,9.5f),new Vector3(0.8f,0.5f,0.8f),metal);
        CreatePipe(root.transform,"Tuberia_Izquierda",new Vector3(-4.72f,1.65f,6f),new Vector3(0.16f,3.1f,0.16f),metal);
        CreatePipe(root.transform,"Tuberia_Derecha",new Vector3(4.72f,1.45f,4f),new Vector3(0.14f,2.7f,0.14f),metal);
    }

    private static void CreateShelf(Transform parent, Vector3 position, Material mat)
    {
        GameObject root=new GameObject("Estanteria_Metalica");root.transform.SetParent(parent);root.transform.position=position;
        for(int y=0;y<3;y++)CreatePrimitive(PrimitiveType.Cube,"Bandeja_"+y,root.transform,new Vector3(0,y*0.75f-0.75f,0),new Vector3(1.4f,0.08f,0.55f),mat,true);
        float[] xs={-0.66f,0.66f};foreach(float x in xs)CreatePrimitive(PrimitiveType.Cube,"Poste",root.transform,new Vector3(x,0,0),new Vector3(0.08f,2.35f,0.08f),mat,true);
    }
    private static void CreateCrate(Transform p,string n,Vector3 pos,Vector3 scale,Material mat)=>CreatePrimitive(PrimitiveType.Cube,n,p,pos,scale,mat,true);
    private static void CreatePipe(Transform p,string n,Vector3 pos,Vector3 scale,Material mat)=>CreatePrimitive(PrimitiveType.Cylinder,n,p,pos,scale,mat,true);

    private static GameObject CreateOrb(Transform parent,string name,Vector3 position,Color color)
    {
        GameObject orb=GameObject.CreatePrimitive(PrimitiveType.Sphere);orb.name=name;orb.transform.SetParent(parent);orb.transform.position=position;orb.transform.localScale=Vector3.one*0.42f;
        Collider c=orb.GetComponent<Collider>();if(c!=null)UnityEngine.Object.DestroyImmediate(c);
        Material mat=CreateOrbMaterial(name+"_Mat",color);orb.GetComponent<Renderer>().sharedMaterial=mat;orb.AddComponent<Level3OrbVisual>();
        Light light=orb.AddComponent<Light>();light.type=LightType.Point;light.range=2.5f;light.intensity=0.8f;light.color=color;
        return orb;
    }

    private static Material CreateOrbMaterial(string name,Color color)
    {
        EnsureFolder(GeneratedFolder+"/Materials");string path=GeneratedFolder+"/Materials/"+name+".mat";Material mat=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(mat==null){Shader shader=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");mat=new Material(shader);AssetDatabase.CreateAsset(mat,path);} 
        if(mat.HasProperty("_BaseColor"))mat.SetColor("_BaseColor",color);else if(mat.HasProperty("_Color"))mat.color=color;
        if(mat.HasProperty("_EmissionColor")){mat.EnableKeyword("_EMISSION");mat.SetColor("_EmissionColor",color*2.5f);}EditorUtility.SetDirty(mat);return mat;
    }

    private static AnimatorController CreateAnimationController(bool idleOnly)
    {
        if (!(AssetImporter.GetAtPath(WolfModelPath) is ModelImporter importer)) return null;
        importer.importAnimation=true;importer.animationType=ModelImporterAnimationType.Generic;
        ModelImporterClipAnimation[] clips=importer.clipAnimations;
        if(clips==null||clips.Length==0)clips=importer.defaultClipAnimations;
        foreach(var c in clips){c.loopTime=true;c.loopPose=true;}
        if(clips.Length>0)importer.clipAnimations=clips;
        importer.SaveAndReimport();
        AnimationClip[] imported=AssetDatabase.LoadAllAssetsAtPath(WolfModelPath).OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__preview__",StringComparison.OrdinalIgnoreCase)).ToArray();
        AnimationClip chosen=idleOnly?imported.FirstOrDefault(c=>ContainsAny(c.name,"idle","rest","breath")):imported.OrderByDescending(c=>AnimationScore(c.name)).FirstOrDefault();
        if(chosen==null)return null;
        EnsureFolder(GeneratedFolder+"/Animation");string path=GeneratedFolder+"/Animation/"+(idleOnly?"WolfSpider_Idle.controller":"WolfSpider_Locomotion.controller");AssetDatabase.DeleteAsset(path);
        AnimatorController controller=AnimatorController.CreateAnimatorControllerAtPath(path);AnimatorState state=controller.layers[0].stateMachine.AddState(idleOnly?"Idle":"Walk");state.motion=chosen;controller.layers[0].stateMachine.defaultState=state;AssetDatabase.SaveAssets();return controller;
    }

    private static int AnimationScore(string name){string n=name.ToLowerInvariant();if(n.Contains("walk"))return 100;if(n.Contains("crawl"))return 95;if(n.Contains("run"))return 85;if(n.Contains("free"))return 80;if(n.Contains("move"))return 70;return 10;}
    private static bool ContainsAny(string value,params string[] terms){string n=value.ToLowerInvariant();return terms.Any(n.Contains);}
    private static Animator PrepareAnimator(GameObject spider,AnimatorController controller){Transform visual=FindChildContaining(spider.transform,"Visual_WolfSpider")??spider.transform;Animator animator=spider.GetComponentInChildren<Animator>(true);if(animator==null)animator=visual.gameObject.AddComponent<Animator>();animator.applyRootMotion=false;if(controller!=null){animator.runtimeAnimatorController=controller;animator.enabled=true;}return animator;}

    private static TMP_Text CreateTopPanel(Transform canvas,string panelName,string initial,out Image fill,out TMP_Text warning)
    {
        GameObject panel=CreateUI(panelName,canvas);RectTransform pr=panel.GetComponent<RectTransform>();pr.anchorMin=new Vector2(0.5f,1f);pr.anchorMax=new Vector2(0.5f,1f);pr.pivot=new Vector2(0.5f,1f);pr.anchoredPosition=new Vector2(0,-25);pr.sizeDelta=new Vector2(720,205);Image bg=panel.AddComponent<Image>();bg.color=new Color(0.025f,0.045f,0.075f,0.92f);
        TMP_Text text=CreateText("Texto_Principal",panel.transform,new Vector2(0,35),new Vector2(680,125),31);text.text=initial;text.alignment=TextAlignmentOptions.Center;
        GameObject bar=CreateUI("Barra_Fondo",panel.transform);RectTransform br=bar.GetComponent<RectTransform>();br.anchorMin=new Vector2(0.5f,0);br.anchorMax=new Vector2(0.5f,0);br.pivot=new Vector2(0.5f,0);br.anchoredPosition=new Vector2(0,22);br.sizeDelta=new Vector2(650,22);bar.AddComponent<Image>().color=new Color(1,1,1,0.2f);
        GameObject f=CreateUI("Barra_Progreso",bar.transform);RectTransform fr=f.GetComponent<RectTransform>();fr.anchorMin=Vector2.zero;fr.anchorMax=Vector2.one;fr.offsetMin=fr.offsetMax=Vector2.zero;fill=f.AddComponent<Image>();fill.color=new Color(0.22f,0.82f,0.64f,1);fill.type=Image.Type.Filled;fill.fillMethod=Image.FillMethod.Horizontal;fill.fillAmount=0;
        warning=CreateText("Texto_Aviso",canvas,new Vector2(0,-170),new Vector2(800,65),28);warning.color=new Color(1f,0.72f,0.24f);warning.alignment=TextAlignmentOptions.Center;warning.gameObject.SetActive(false);return text;
    }

    private static GameObject CreateCompletionPanel(Transform canvas,string name,string title,out TMP_Text text)
    {
        GameObject panel=CreateUI(name,canvas);RectTransform r=panel.GetComponent<RectTransform>();r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;panel.AddComponent<Image>().color=new Color(0.015f,0.03f,0.06f,0.98f);
        GameObject card=CreateUI("Tarjeta",panel.transform);RectTransform cr=card.GetComponent<RectTransform>();cr.anchorMin=cr.anchorMax=new Vector2(0.5f,0.5f);cr.sizeDelta=new Vector2(980,520);card.AddComponent<Image>().color=new Color(0.07f,0.13f,0.22f,1);
        text=CreateText("Texto_Final",card.transform,Vector2.zero,new Vector2(870,410),54);text.text=title;text.alignment=TextAlignmentOptions.Center;return panel;
    }

    private static Canvas RecreateCanvas(Scene scene,string name,int order){GameObject old=FindByExactOrContains(scene,name,name);if(old!=null)UnityEngine.Object.DestroyImmediate(old);GameObject go=new GameObject(name,typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));SceneManager.MoveGameObjectToScene(go,scene);Canvas c=go.GetComponent<Canvas>();c.renderMode=RenderMode.ScreenSpaceOverlay;c.sortingOrder=order;CanvasScaler s=go.GetComponent<CanvasScaler>();s.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;s.referenceResolution=new Vector2(1920,1080);s.matchWidthOrHeight=0.5f;return c;}
    private static GameObject CreateUI(string name,Transform parent){GameObject go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);return go;}
    private static TMP_Text CreateText(string name,Transform parent,Vector2 pos,Vector2 size,float font){GameObject go=CreateUI(name,parent);RectTransform r=go.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=new Vector2(0.5f,0.5f);r.anchoredPosition=pos;r.sizeDelta=size;TextMeshProUGUI t=go.AddComponent<TextMeshProUGUI>();t.fontSize=font;t.color=Color.white;t.alignment=TextAlignmentOptions.Center;if(TMP_Settings.defaultFontAsset!=null)t.font=TMP_Settings.defaultFontAsset;return t;}

    private static void DisableComponentsByType(Scene scene,params string[] typeNames){HashSet<string> names=new HashSet<string>(typeNames);foreach(MonoBehaviour b in scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<MonoBehaviour>(true)))if(b!=null&&names.Contains(b.GetType().Name)){b.enabled=false;EditorUtility.SetDirty(b);}}
    private static void SetObjectActive(Scene scene,string name,bool active){GameObject go=FindByExactOrContains(scene,name,name);if(go!=null)go.SetActive(active);}
    private static void DestroyNamed(Scene scene,params string[] names){HashSet<string> set=new HashSet<string>(names);foreach(Transform t in scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray())if(set.Contains(t.name))UnityEngine.Object.DestroyImmediate(t.gameObject);}
    private static GameObject FindPlayer(Scene scene){string[] names={"NP_DesktopPlayer_Normalizado","DesktopTestRig","XR Origin (XR Rig)"};foreach(string n in names){GameObject go=FindByExactOrContains(scene,n,n);if(go!=null&&go.activeInHierarchy)return go;}return scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<CharacterController>(true)).Select(c=>c.gameObject).FirstOrDefault();}
    private static Camera FindActiveCamera(Scene scene)=>scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Camera>(true)).FirstOrDefault(c=>c.isActiveAndEnabled)??Camera.main;
    private static GameObject FindRoot(Scene scene,string name)=>scene.GetRootGameObjects().FirstOrDefault(r=>r.name==name);
    private static GameObject NewRoot(Scene scene,string name){GameObject go=new GameObject(name);SceneManager.MoveGameObjectToScene(go,scene);return go;}
    private static GameObject FindByExactOrContains(Scene scene,string exact,string contains){foreach(GameObject root in scene.GetRootGameObjects()){Transform t=FindRecursive(root.transform,exact,contains);if(t!=null)return t.gameObject;}return null;}
    private static Transform FindRecursive(Transform t,string exact,string contains){if(t.name==exact||t.name.IndexOf(contains,StringComparison.OrdinalIgnoreCase)>=0)return t;foreach(Transform c in t){Transform f=FindRecursive(c,exact,contains);if(f!=null)return f;}return null;}
    private static Transform FindChildContaining(Transform t,string contains){if(t.name.IndexOf(contains,StringComparison.OrdinalIgnoreCase)>=0)return t;foreach(Transform c in t){Transform f=FindChildContaining(c,contains);if(f!=null)return f;}return null;}
    private static Transform FindOrCreatePoint(Transform parent,string name,Vector3 pos){Transform t=parent.Find(name);if(t==null){GameObject go=new GameObject(name);go.transform.SetParent(parent);t=go.transform;}t.position=pos;return t;}
    private static void ClearChildren(Transform parent){for(int i=parent.childCount-1;i>=0;i--)UnityEngine.Object.DestroyImmediate(parent.GetChild(i).gameObject);}
    private static void SetObjectArray(SerializedProperty p,List<GameObject> values){p.arraySize=values.Count;for(int i=0;i<values.Count;i++)p.GetArrayElementAtIndex(i).objectReferenceValue=values[i];}
    private static Material FindMaterial(string name){string guid=AssetDatabase.FindAssets(name+" t:Material").FirstOrDefault();return string.IsNullOrEmpty(guid)?null:AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));}
    private static GameObject InstantiateModel(string path,Transform parent,string name){GameObject asset=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(asset==null)return null;GameObject go=(GameObject)PrefabUtility.InstantiatePrefab(asset);go.name=name;go.transform.SetParent(parent,false);return go;}
    private static void FitAndPlace(GameObject go,float targetMax,Vector3 position,Quaternion rotation){go.transform.rotation=rotation;Bounds b=GetBounds(go);float max=Mathf.Max(b.size.x,b.size.y,b.size.z);if(max>0.0001f)go.transform.localScale*=targetMax/max;b=GetBounds(go);go.transform.position+=position-new Vector3(b.center.x,b.min.y,b.center.z);}
    private static Bounds GetBounds(GameObject go){Renderer[] rs=go.GetComponentsInChildren<Renderer>(true);if(rs.Length==0)return new Bounds(go.transform.position,Vector3.one);Bounds b=rs[0].bounds;for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);return b;}
    private static void AssignMaterial(GameObject go,Material mat){if(mat==null)return;foreach(Renderer r in go.GetComponentsInChildren<Renderer>(true))r.sharedMaterial=mat;}
    private static void AddLocalBoundsCollider(GameObject go){Bounds world=GetBounds(go);Vector3[] corners=new Vector3[8];Vector3 min=world.min,max=world.max;int k=0;for(int x=0;x<2;x++)for(int y=0;y<2;y++)for(int z=0;z<2;z++)corners[k++]=new Vector3(x==0?min.x:max.x,y==0?min.y:max.y,z==0?min.z:max.z);Bounds local=new Bounds(go.transform.InverseTransformPoint(corners[0]),Vector3.zero);for(int i=1;i<corners.Length;i++)local.Encapsulate(go.transform.InverseTransformPoint(corners[i]));BoxCollider c=go.GetComponent<BoxCollider>();if(c==null)c=go.AddComponent<BoxCollider>();c.center=local.center;c.size=local.size;c.isTrigger=false;}
    private static GameObject CreatePrimitive(PrimitiveType type,string name,Transform parent,Vector3 pos,Vector3 scale,Material mat,bool collider){GameObject go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(parent);go.transform.position=pos;go.transform.localScale=scale;if(mat!=null)go.GetComponent<Renderer>().sharedMaterial=mat;if(!collider){Collider c=go.GetComponent<Collider>();if(c!=null)UnityEngine.Object.DestroyImmediate(c);}return go;}
    private static void EnsureFolder(string path){string[] parts=path.Split('/');string current=parts[0];for(int i=1;i<parts.Length;i++){string next=current+"/"+parts[i];if(!AssetDatabase.IsValidFolder(next))AssetDatabase.CreateFolder(current,parts[i]);current=next;}}
    private static void Save(Scene scene){EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();}
    private static void Fail(string message)=>EditorUtility.DisplayDialog("NeuroPhobia",message,"Aceptar");
}
#endif
