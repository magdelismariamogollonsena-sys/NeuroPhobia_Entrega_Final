#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>Audita y convierte solo materiales asignados a instancias del apartamento de Nivel5.</summary>
public static class NeuroPhobiaNivel5MaterialFix
{
    private const string ScenePath = "Assets/Scenes/Nivel5.unity";
    private const string ConvertedFolder = "Assets/NeuroPhobia_Level5/Materials/Converted";
    private const string RootName = "NP_L5_APARTAMENTO_VISUAL";

    [MenuItem("Tools/NeuroPhobia/Nivel 5/Diagnosticar materiales del apartamento", priority = 451)]
    public static void Diagnose()
    {
        Scene scene = OpenLevel5();
        Transform root = FindRoot(scene);
        string path = Path.Combine(Path.GetTempPath(), "NP_L5_material_audit.txt");
        File.WriteAllText(path, BuildAudit(root), Encoding.UTF8);
        Debug.Log("Diagnóstico de materiales Nivel5: " + path);
    }

    [MenuItem("Tools/NeuroPhobia/Nivel 5/Convertir materiales del apartamento a URP", priority = 452)]
    public static void Convert()
    {
        Scene scene = OpenLevel5();
        Transform root = FindRoot(scene);
        Shader lit = Shader.Find("Universal Render Pipeline/Lit");
        if (lit == null) throw new InvalidOperationException("Shader Universal Render Pipeline/Lit no disponible.");
        EnsureFolder();

        var copies = new Dictionary<Material, Material>();
        Material missingFallback = null;
        int slotsChanged = 0;
        int missingFound = 0;
        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            Material[] slots = renderer.sharedMaterials;
            bool changed = false;
            for (int i = 0; i < slots.Length; i++)
            {
                Material source = slots[i];
                if (source == null)
                {
                    missingFound++;
                    if (missingFallback == null) missingFallback = Fallback(lit);
                    slots[i] = missingFallback;
                }
                else if (NeedsConversion(source))
                {
                    if (!copies.TryGetValue(source, out Material converted))
                    {
                        converted = CopyToUrp(source, lit);
                        copies.Add(source, converted);
                    }
                    slots[i] = converted;
                }
                else continue;
                slotsChanged++;
                changed = true;
            }
            if (!changed) continue;
            renderer.sharedMaterials = slots;
            PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
            EditorUtility.SetDirty(renderer);
        }
        if (slotsChanged > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("No se pudo guardar Nivel5.");
            AssetDatabase.SaveAssets();
        }

        int remaining = 0;
        int missing = 0;
        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            foreach (Material material in renderer.sharedMaterials)
            {
                if (material == null) missing++;
                else if (NeedsConversion(material)) remaining++;
            }
        string result = "Nivel5 materiales: copias URP=" + copies.Count +
            ", slots reasignados=" + slotsChanged + ", missing originales=" + missingFound +
            ", incompatibles restantes=" + remaining + ", missing restantes=" + missing;
        Debug.Log(result);
        if (remaining != 0 || missing != 0) throw new InvalidOperationException(result);
        File.WriteAllText(Path.Combine(Path.GetTempPath(), "NP_L5_material_after.txt"), BuildAudit(root), Encoding.UTF8);
    }

    private static Scene OpenLevel5()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Detén Play Mode.");
        Scene active = SceneManager.GetActiveScene();
        if (active.isDirty && active.path != ScenePath)
            throw new InvalidOperationException("Hay otra escena con cambios sin guardar.");
        return active.path == ScenePath ? active : EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
    }

    private static Transform FindRoot(Scene scene)
    {
        foreach (GameObject go in scene.GetRootGameObjects())
            if (go.name == RootName) return go.transform;
        throw new InvalidOperationException("No se encontró " + RootName + " en Nivel5.");
    }

    private static bool NeedsConversion(Material material)
    {
        if (material.shader == null) return true;
        string name = material.shader.name;
        return !name.StartsWith("Universal Render Pipeline/", StringComparison.Ordinal) &&
               !name.StartsWith("Shader Graphs/", StringComparison.Ordinal);
    }

    private static string BuildAudit(Transform root)
    {
        var uses = new Dictionary<Material, HashSet<string>>();
        var missing = new List<string>();
        int renderers = 0;
        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            renderers++;
            for (int i = 0; i < renderer.sharedMaterials.Length; i++)
            {
                Material material = renderer.sharedMaterials[i];
                if (material == null) { missing.Add(PathOf(renderer.transform, root) + " [slot " + i + "]"); continue; }
                if (!uses.TryGetValue(material, out HashSet<string> list))
                {
                    list = new HashSet<string>();
                    uses.Add(material, list);
                }
                list.Add(PathOf(renderer.transform, root));
            }
        }
        var text = new StringBuilder();
        text.AppendLine("RENDERERS=" + renderers + " UNIQUE_MATERIALS=" + uses.Count + " MISSING_SLOTS=" + missing.Count);
        foreach (var pair in uses.OrderBy(p => p.Key.name))
        {
            Material material = pair.Key;
            text.AppendLine("MATERIAL=" + material.name);
            text.AppendLine("SHADER=" + (material.shader != null ? material.shader.name : "<missing>") + " PROBLEM=" + NeedsConversion(material));
            text.AppendLine("SOURCE=" + AssetDatabase.GetAssetPath(material));
            text.AppendLine("BASE=" + TextureInfo(material, "_BaseMap", "_MainTex") + " COLOR=" + ColorInfo(material, "_BaseColor", "_Color"));
            text.AppendLine("NORMAL=" + TextureInfo(material, "_BumpMap"));
            text.AppendLine("METALLIC_MAP=" + TextureInfo(material, "_MetallicGlossMap") +
                " METALLIC=" + FloatInfo(material, "_Metallic") +
                " SMOOTHNESS=" + FloatInfo(material, "_Smoothness", "_Glossiness"));
            text.AppendLine("EMISSION=" + TextureInfo(material, "_EmissionMap") + " EMISSION_COLOR=" + ColorInfo(material, "_EmissionColor"));
            text.AppendLine("MODE=" + FloatInfo(material, "_Mode", "_Surface") + " CUTOUT=" + FloatInfo(material, "_Cutoff", "_AlphaClip"));
            text.AppendLine("USERS=" + string.Join(" | ", pair.Value.OrderBy(x => x)));
        }
        foreach (string item in missing) text.AppendLine("MISSING=" + item);
        return text.ToString();
    }

    private static string PathOf(Transform item, Transform root)
    {
        var parts = new List<string>();
        while (item != null && item != root) { parts.Add(item.name); item = item.parent; }
        parts.Reverse();
        return string.Join("/", parts);
    }

    private static string TextureInfo(Material material, params string[] keys)
    {
        Texture texture = ReadTexture(material, keys);
        return texture == null ? "<none>" : AssetDatabase.GetAssetPath(texture) + " (" + texture.name + ")";
    }

    private static string FloatInfo(Material material, params string[] keys)
    {
        foreach (string key in keys) if (material.HasProperty(key)) return material.GetFloat(key).ToString("0.###");
        return "<none>";
    }

    private static string ColorInfo(Material material, params string[] keys)
    {
        foreach (string key in keys) if (material.HasProperty(key)) return material.GetColor(key).ToString();
        return "<none>";
    }

    private static Texture ReadTexture(Material material, params string[] keys)
    {
        foreach (string key in keys)
            if (material.HasProperty(key) && material.GetTexture(key) != null) return material.GetTexture(key);
        // Conserva texturas serializadas aun si el shader original está ausente.
        SerializedObject serialized = new SerializedObject(material);
        SerializedProperty envs = serialized.FindProperty("m_SavedProperties.m_TexEnvs");
        if (envs != null)
            for (int i = 0; i < envs.arraySize; i++)
            {
                SerializedProperty entry = envs.GetArrayElementAtIndex(i);
                SerializedProperty first = entry.FindPropertyRelative("first");
                SerializedProperty value = entry.FindPropertyRelative("second.m_Texture");
                if (first == null || value == null || !keys.Contains(first.stringValue)) continue;
                if (value.objectReferenceValue is Texture texture) return texture;
            }
        return null;
    }

    private static Material CopyToUrp(Material source, Shader lit)
    {
        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(source, out string guid, out long localId);
        string safeName = new string(source.name.Select(c => char.IsLetterOrDigit(c) || c == '_' ? c : '_').ToArray());
        string file = safeName + "_" + (string.IsNullOrEmpty(guid) ? "scene" : guid.Substring(0, 8)) + "_" + localId + "_L5_URP.mat";
        string path = ConvertedFolder + "/" + file;
        Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null) return existing;

        Material target = new Material(lit) { name = source.name + "_L5_URP" };
        Texture baseMap = ReadTexture(source, "_BaseMap", "_MainTex");
        if (baseMap != null) target.SetTexture("_BaseMap", baseMap);
        target.SetColor("_BaseColor", ReadColor(source, Color.white, "_BaseColor", "_Color"));
        CopyScaleOffset(source, target, "_BaseMap", "_BaseMap", "_MainTex");

        Texture normal = ReadTexture(source, "_BumpMap");
        if (normal != null)
        {
            target.SetTexture("_BumpMap", normal);
            target.SetFloat("_BumpScale", ReadFloat(source, 1f, "_BumpScale"));
            target.EnableKeyword("_NORMALMAP");
        }
        Texture metallicMap = ReadTexture(source, "_MetallicGlossMap");
        if (metallicMap != null)
        {
            target.SetTexture("_MetallicGlossMap", metallicMap);
            target.EnableKeyword("_METALLICSPECGLOSSMAP");
        }
        target.SetFloat("_Metallic", ReadFloat(source, 0f, "_Metallic"));
        target.SetFloat("_Smoothness", ReadFloat(source, .5f, "_Smoothness", "_Glossiness"));

        Texture emissionMap = ReadTexture(source, "_EmissionMap");
        Color emissionColor = ReadColor(source, Color.black, "_EmissionColor");
        if (emissionMap != null) target.SetTexture("_EmissionMap", emissionMap);
        if (emissionMap != null || emissionColor.maxColorComponent > .001f)
        {
            target.SetColor("_EmissionColor", emissionColor);
            target.EnableKeyword("_EMISSION");
            target.globalIlluminationFlags = source.globalIlluminationFlags;
        }

        float standardMode = ReadFloat(source, 0f, "_Mode");
        float surface = ReadFloat(source, standardMode >= 2f ? 1f : 0f, "_Surface");
        float cutoff = ReadFloat(source, .5f, "_Cutoff");
        bool alphaClip = standardMode == 1f || ReadFloat(source, 0f, "_AlphaClip") > .5f;
        target.SetFloat("_Cutoff", cutoff);
        if (alphaClip)
        {
            target.SetFloat("_AlphaClip", 1f);
            target.EnableKeyword("_ALPHATEST_ON");
            target.renderQueue = (int)RenderQueue.AlphaTest;
        }
        else if (surface > .5f || source.renderQueue >= (int)RenderQueue.Transparent)
        {
            target.SetFloat("_Surface", 1f);
            target.SetFloat("_Blend", 0f);
            target.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            target.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            target.SetFloat("_ZWrite", 0f);
            target.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            target.SetOverrideTag("RenderType", "Transparent");
            target.renderQueue = (int)RenderQueue.Transparent;
        }
        AssetDatabase.CreateAsset(target, path);
        return target;
    }

    private static float ReadFloat(Material source, float fallback, params string[] keys)
    {
        foreach (string key in keys) if (source.HasProperty(key)) return source.GetFloat(key);
        return fallback;
    }

    private static Color ReadColor(Material source, Color fallback, params string[] keys)
    {
        foreach (string key in keys) if (source.HasProperty(key)) return source.GetColor(key);
        return fallback;
    }

    private static void CopyScaleOffset(Material source, Material target, string targetKey, params string[] sourceKeys)
    {
        foreach (string key in sourceKeys)
        {
            if (!source.HasProperty(key)) continue;
            target.SetTextureScale(targetKey, source.GetTextureScale(key));
            target.SetTextureOffset(targetKey, source.GetTextureOffset(key));
            return;
        }
    }

    private static Material Fallback(Shader lit)
    {
        string path = ConvertedFolder + "/MissingMaterial_L5_URP.mat";
        Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null) return existing;
        Material material = new Material(lit) { name = "MissingMaterial_L5_URP" };
        material.SetColor("_BaseColor", Color.white);
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static void EnsureFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/NeuroPhobia_Level5")) AssetDatabase.CreateFolder("Assets", "NeuroPhobia_Level5");
        if (!AssetDatabase.IsValidFolder("Assets/NeuroPhobia_Level5/Materials")) AssetDatabase.CreateFolder("Assets/NeuroPhobia_Level5", "Materials");
        if (!AssetDatabase.IsValidFolder(ConvertedFolder)) AssetDatabase.CreateFolder("Assets/NeuroPhobia_Level5/Materials", "Converted");
    }
}
#endif
