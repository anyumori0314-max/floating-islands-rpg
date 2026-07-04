namespace FloatingIslandsRpg.Application.Scenes
{
    public interface ISceneLoader
    {
        void Load(SceneId sceneId, SceneLoadMode loadMode);

        void Unload(SceneId sceneId);
    }
}
