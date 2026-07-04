using FloatingIslandsRpg.Application.Scenes;
using UnityEngine.SceneManagement;

namespace FloatingIslandsRpg.Infrastructure.Scenes
{
    public sealed class UnitySceneLoader : ISceneLoader
    {
        public void Load(SceneId sceneId, SceneLoadMode loadMode)
        {
            var sceneName = SceneNameCatalog.GetName(sceneId);
            var unityLoadMode = loadMode == SceneLoadMode.Additive ? LoadSceneMode.Additive : LoadSceneMode.Single;
            SceneManager.LoadScene(sceneName, unityLoadMode);
        }

        public void Unload(SceneId sceneId)
        {
            var sceneName = SceneNameCatalog.GetName(sceneId);
            SceneManager.UnloadSceneAsync(sceneName);
        }
    }
}
