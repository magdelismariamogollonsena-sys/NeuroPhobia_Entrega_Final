using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Build headless para Meta Quest 3 (Android/ARM64/IL2CPP).
/// Uso: Unity.exe -batchmode -quiet -projectPath "..." -buildTarget Android
///      -executeMethod BuildQuest3.Build -logFile build.log
/// </summary>
public static class BuildQuest3
{
    public static void Build()
    {
        PlayerSettings.SetScriptingBackend(
            BuildTargetGroup.Android,
            ScriptingImplementation.IL2CPP);

        PlayerSettings.Android.targetArchitectures =
            AndroidArchitecture.ARM64;

        string[] scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        string outputPath = "Builds/Quest3/NeuroPhobia.apk";
        System.IO.Directory.CreateDirectory("Builds/Quest3");

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = BuildTarget.Android,
            targetGroup = BuildTargetGroup.Android,
            options = BuildOptions.None
        };

        UnityEditor.Build.Reporting.BuildReport report =
            BuildPipeline.BuildPlayer(options);

        Debug.Log(
            "NeuroPhobia Quest3 build result: " +
            report.summary.result +
            " | size: " + report.summary.totalSize +
            " bytes | errors: " + report.summary.totalErrors);

        if (report.summary.result !=
            UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            EditorApplication.Exit(1);
        }
    }
}
