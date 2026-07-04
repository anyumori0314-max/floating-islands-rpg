using System;
using System.Collections.Generic;

namespace FloatingIslandsRpg.Application.Scenes
{
    public static class SceneNameCatalog
    {
        private static readonly Dictionary<SceneId, string> Names = new Dictionary<SceneId, string>
        {
            { SceneId.Title, "Title" },
            { SceneId.Village, "Village" },
            { SceneId.Field, "Field" },
            { SceneId.Dungeon, "Dungeon" },
            { SceneId.Battle, "Battle" },
            { SceneId.GameClear, "GameClear" },
        };

        public static int RegisteredCount => Names.Count;

        public static string GetName(SceneId sceneId)
        {
            if (!Names.TryGetValue(sceneId, out var name))
            {
                throw new ArgumentOutOfRangeException(nameof(sceneId), sceneId, "Unknown SceneId.");
            }

            return name;
        }
    }
}
