using System;
using System.Collections.Generic;

namespace FloatingIslandsRpg.Application.Scenes
{
    public static class SceneNameCatalog
    {
        private static readonly Dictionary<SceneId, string> Names = new Dictionary<SceneId, string>
        {
            { SceneId.Sample, "SampleScene" },
            { SceneId.Bootstrap, "Bootstrap" },
            { SceneId.Title, "Title" },
            { SceneId.Field, "Field" },
            { SceneId.Battle, "Battle" },
            { SceneId.GameClear, "GameClear" },
        };

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
