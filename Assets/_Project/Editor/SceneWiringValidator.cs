using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FloatingIslandsRpg.Editor
{
    // T-027 (通し結線・E2E確認): a minimal batchmode-callable substitute for Unity MCP's
    // `manage_scene validate` (unavailable in a CLI-only environment). Opens each official Scene
    // and reports Missing Script / Missing Reference / Broken Prefab counts so T-027's scene
    // validation checklist item can be verified without an interactive Editor/MCP session.
    public static class SceneWiringValidator
    {
        private static readonly string[] OfficialSceneNames =
        {
            "Title", "Village", "Field", "Dungeon", "Battle", "GameClear"
        };

        public static void ValidateOfficialScenes()
        {
            var report = new StringBuilder();
            var totalMissingScripts = 0;
            var totalMissingReferences = 0;
            var totalBrokenPrefabs = 0;

            foreach (var sceneName in OfficialSceneNames)
            {
                var scenePath = FindScenePath(sceneName);
                if (scenePath == null)
                {
                    report.AppendLine($"[T027-SCENE-VALIDATE] {sceneName}: SCENE FILE NOT FOUND");
                    continue;
                }

                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                var missingScripts = 0;
                var missingReferences = 0;
                var brokenPrefabs = 0;

                foreach (var root in scene.GetRootGameObjects())
                {
                    foreach (var t in root.GetComponentsInChildren<Transform>(true))
                    {
                        var go = t.gameObject;

                        if (PrefabUtility.IsPartOfPrefabInstance(go)
                            && PrefabUtility.GetPrefabInstanceStatus(go) == PrefabInstanceStatus.MissingAsset)
                        {
                            brokenPrefabs++;
                            report.AppendLine($"[T027-SCENE-VALIDATE] {sceneName}: BROKEN PREFAB at {GetHierarchyPath(go)}");
                        }

                        var components = go.GetComponents<Component>();
                        foreach (var component in components)
                        {
                            if (component == null)
                            {
                                missingScripts++;
                                report.AppendLine($"[T027-SCENE-VALIDATE] {sceneName}: MISSING SCRIPT at {GetHierarchyPath(go)}");
                                continue;
                            }

                            var serializedObject = new SerializedObject(component);
                            var property = serializedObject.GetIterator();
                            var enterChildren = true;
                            while (property.NextVisible(enterChildren))
                            {
                                enterChildren = false;
                                if (property.name == "m_Script")
                                {
                                    continue;
                                }

                                if (property.propertyType == SerializedPropertyType.ObjectReference
                                    && property.objectReferenceValue == null
                                    && property.objectReferenceInstanceIDValue != 0)
                                {
                                    missingReferences++;
                                    report.AppendLine(
                                        $"[T027-SCENE-VALIDATE] {sceneName}: MISSING REFERENCE at {GetHierarchyPath(go)} ({component.GetType().Name}.{property.propertyPath})");
                                }
                            }
                        }
                    }
                }

                report.AppendLine($"[T027-SCENE-VALIDATE] {sceneName}: MissingScript={missingScripts} MissingReference={missingReferences} BrokenPrefab={brokenPrefabs}");
                totalMissingScripts += missingScripts;
                totalMissingReferences += missingReferences;
                totalBrokenPrefabs += brokenPrefabs;
            }

            report.AppendLine($"[T027-SCENE-VALIDATE] TOTAL: MissingScript={totalMissingScripts} MissingReference={totalMissingReferences} BrokenPrefab={totalBrokenPrefabs}");

            var summary = report.ToString();
            Debug.Log(summary);

            var outputPath = Environment.GetEnvironmentVariable("T027_SCENE_VALIDATE_OUTPUT");
            if (!string.IsNullOrEmpty(outputPath))
            {
                File.WriteAllText(outputPath, summary);
            }
        }

        private static string FindScenePath(string sceneName)
        {
            foreach (var guid in AssetDatabase.FindAssets($"t:Scene {sceneName}"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.Equals(Path.GetFileNameWithoutExtension(path), sceneName, StringComparison.Ordinal))
                {
                    return path;
                }
            }

            return null;
        }

        private static string GetHierarchyPath(GameObject go)
        {
            var path = go.name;
            var current = go.transform.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
        }
    }
}
