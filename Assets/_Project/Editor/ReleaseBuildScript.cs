using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FloatingIslandsRpg.Editor
{
    // T-029 (Windowsビルド作成・実機起動確認): Unity MCPが未接続のCLI専用環境で
    // Windows Standaloneビルドを作成するためのバッチモード呼び出し口。
    // Development Build / Script Debugging / Autoconnect Profilerはすべて無効(BuildOptions.None)。
    public static class ReleaseBuildScript
    {
        private const string OutputDirectory = "Builds/Windows/FloatingIslandsRpg";
        private const string OutputFileName = "FloatingIslandsRpg.exe";

        public static void BuildWindows64()
        {
            var scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            var outputPath = Path.Combine(OutputDirectory, OutputFileName);
            Directory.CreateDirectory(OutputDirectory);

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.StandaloneWindows64,
                targetGroup = BuildTargetGroup.Standalone,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;

            var resultText =
                $"[T029-BUILD] result={summary.result} " +
                $"totalErrors={summary.totalErrors} totalWarnings={summary.totalWarnings} " +
                $"totalSize={summary.totalSize} outputPath={summary.outputPath} " +
                $"scenesInBuild={scenes.Length}";

            Debug.Log(resultText);

            var outputEnvPath = Environment.GetEnvironmentVariable("T029_BUILD_OUTPUT");
            if (!string.IsNullOrEmpty(outputEnvPath))
            {
                File.WriteAllText(outputEnvPath, resultText + Environment.NewLine);
            }

            EditorApplication.Exit(summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded ? 0 : 1);
        }
    }
}
