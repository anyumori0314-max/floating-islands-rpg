using System;
using System.Collections.Generic;
using System.Linq;
using FloatingIslandsRpg.Application.Scenes;
using NUnit.Framework;

namespace FloatingIslandsRpg.Tests.EditMode.Scenes
{
    public class SceneNameCatalogTests
    {
        private static IEnumerable<SceneId> AllSceneIds =>
            Enum.GetValues(typeof(SceneId)).Cast<SceneId>();

        [Test]
        public void GetName_AllDefinedSceneIds_ReturnNonEmptyNames()
        {
            foreach (var sceneId in AllSceneIds)
            {
                var name = SceneNameCatalog.GetName(sceneId);

                Assert.IsFalse(string.IsNullOrEmpty(name), $"{sceneId} returned a null or empty name.");
            }
        }

        [Test]
        public void GetName_AllDefinedSceneIds_ReturnUniqueNames()
        {
            var names = AllSceneIds.Select(SceneNameCatalog.GetName).ToList();

            Assert.AreEqual(names.Count, names.Distinct(StringComparer.Ordinal).Count());
        }

        [Test]
        public void GetName_UndefinedSceneId_ThrowsArgumentOutOfRangeException()
        {
            var undefinedSceneId = (SceneId)(-1);

            Assert.Throws<ArgumentOutOfRangeException>(() => SceneNameCatalog.GetName(undefinedSceneId));
        }

        [Test]
        public void GetName_Sample_MatchesExistingSampleSceneAsset()
        {
            Assert.AreEqual("SampleScene", SceneNameCatalog.GetName(SceneId.Sample));
        }
    }
}
