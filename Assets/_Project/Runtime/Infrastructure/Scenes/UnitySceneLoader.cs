using System;
using System.Threading.Tasks;
using FloatingIslandsRpg.Application.Scenes;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FloatingIslandsRpg.Infrastructure.Scenes
{
    public sealed class UnitySceneLoader : ISceneLoader
    {
        public Task LoadAsync(SceneId sceneId, SceneLoadMode loadMode)
        {
            var sceneName = SceneNameCatalog.GetName(sceneId);
            var unityLoadMode = loadMode == SceneLoadMode.Additive ? LoadSceneMode.Additive : LoadSceneMode.Single;
            var operation = SceneManager.LoadSceneAsync(sceneName, unityLoadMode);

            if (operation is null)
            {
                throw new InvalidOperationException($"SceneManager.LoadSceneAsync returned null for scene '{sceneName}'.");
            }

            return ToTask(operation);
        }

        public Task UnloadAsync(SceneId sceneId)
        {
            var sceneName = SceneNameCatalog.GetName(sceneId);
            var operation = SceneManager.UnloadSceneAsync(sceneName);

            if (operation is null)
            {
                throw new InvalidOperationException($"SceneManager.UnloadSceneAsync returned null for scene '{sceneName}'.");
            }

            return ToTask(operation);
        }

        private static Task ToTask(AsyncOperation operation)
        {
            if (operation.isDone)
            {
                return Task.CompletedTask;
            }

            var completionSource = new TaskCompletionSource<bool>();
            operation.completed += _ => completionSource.SetResult(true);
            return completionSource.Task;
        }
    }
}
