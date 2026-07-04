using System.Threading.Tasks;

namespace FloatingIslandsRpg.Application.Scenes
{
    public interface ISceneLoader
    {
        Task LoadAsync(SceneId sceneId, SceneLoadMode loadMode);

        Task UnloadAsync(SceneId sceneId);
    }
}
