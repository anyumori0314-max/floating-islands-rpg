using FloatingIslandsRpg.Application.Scenes;

namespace FloatingIslandsRpg.Composition
{
    // Bridges the Additive Battle-scene boundary: set by a Field/Dungeon scene installer
    // right before it loads Battle additively, consumed by BattleSceneInstaller to decide
    // where a victory should lead (unload back to the field/dungeon vs. proceed to
    // GameClear for a boss). See PROJECT.md section 4, "Scene composition" (Additive load
    // requirement). Stores temporary battle context between scenes.
    public sealed class PendingBattleContext
    {
        public SceneId ReturnSceneId { get; }
        public bool IsBossEncounter { get; }

        public PendingBattleContext(SceneId returnSceneId, bool isBossEncounter)
        {
            ReturnSceneId = returnSceneId;
            IsBossEncounter = isBossEncounter;
        }
    }
}
