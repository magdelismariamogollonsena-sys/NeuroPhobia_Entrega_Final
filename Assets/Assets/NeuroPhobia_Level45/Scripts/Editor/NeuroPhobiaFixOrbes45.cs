#if UNITY_EDITOR
using System.IO;
using NeuroPhobia.Level45;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeuroPhobia.Level45.Editor
{
    public static class NeuroPhobiaFixOrbes45
    {
        [MenuItem(
            "Tools/NeuroPhobia/Niveles 4 y 5/CORREGIR DISTANCIA ORBES PC Y RV",
            priority = 450)]
        public static void Apply()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog(
                    "NeuroPhobia",
                    "Detén Play antes de ejecutar el arreglo.",
                    "Aceptar");
                return;
            }

            FixLevel4();
            FixLevel5();

            EditorUtility.DisplayDialog(
                "NeuroPhobia",
                "Arreglo aplicado.\n\n" +
                "Nivel 4: radio del punto = 2.25 m.\n" +
                "Nivel 5: radio de orbes = 2.25 m.\n" +
                "Zona final = 2.25 m.\n\n" +
                "La distancia ahora se calcula en el suelo (X/Z), " +
                "sin contar la altura de la cámara.\n\n" +
                "Prueba Nivel 4 y Nivel 5 en Play.",
                "Aceptar");
        }

        private static void FixLevel4()
        {
            const string scenePath = "Assets/Scenes/Nivel4.unity";
            const string backupPath =
                "Assets/Scenes/Nivel4_BACKUP_Antes_FIX_Orbes.unity";

            if (!File.Exists(scenePath))
                return;

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(backupPath) == null)
            {
                AssetDatabase.CopyAsset(scenePath, backupPath);
                AssetDatabase.SaveAssets();
            }

            Scene scene =
                EditorSceneManager.OpenScene(
                    scenePath,
                    OpenSceneMode.Single);

            NPLevel4Controller controller =
                FindInScene<NPLevel4Controller>(scene);

            if (controller != null)
            {
                controller.activationRadius = 2.25f;
                EditorUtility.SetDirty(controller);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
        }

        private static void FixLevel5()
        {
            const string scenePath = "Assets/Scenes/Nivel5.unity";
            const string backupPath =
                "Assets/Scenes/Nivel5_BACKUP_Antes_FIX_Orbes.unity";

            if (!File.Exists(scenePath))
                return;

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(backupPath) == null)
            {
                AssetDatabase.CopyAsset(scenePath, backupPath);
                AssetDatabase.SaveAssets();
            }

            Scene scene =
                EditorSceneManager.OpenScene(
                    scenePath,
                    OpenSceneMode.Single);

            NPLevel5Controller controller =
                FindInScene<NPLevel5Controller>(scene);

            if (controller != null)
            {
                controller.orbCollectionRadius = 2.25f;
                controller.finalZoneRadius = 2.25f;
                EditorUtility.SetDirty(controller);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
        }

        private static T FindInScene<T>(Scene scene)
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
    }
}
#endif
