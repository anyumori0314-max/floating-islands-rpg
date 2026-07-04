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
        public void GetName_AllDefinedSceneIds_ReturnNonWhiteSpaceNames()
        {
            foreach (var sceneId in AllSceneIds)
            {
                var name = SceneNameCatalog.GetName(sceneId);

                Assert.IsFalse(string.IsNullOrWhiteSpace(name), $"{sceneId} returned a null, empty, or whitespace-only name.");
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
        public void GetName_Title_ReturnsTitle()
        {
            Assert.AreEqual("Title", SceneNameCatalog.GetName(SceneId.Title));
        }

        [Test]
        public void GetName_Village_ReturnsVillage()
        {
            Assert.AreEqual("Village", SceneNameCatalog.GetName(SceneId.Village));
        }

        [Test]
        public void GetName_Field_ReturnsField()
        {
            Assert.AreEqual("Field", SceneNameCatalog.GetName(SceneId.Field));
        }

        [Test]
        public void GetName_Dungeon_ReturnsDungeon()
        {
            Assert.AreEqual("Dungeon", SceneNameCatalog.GetName(SceneId.Dungeon));
        }

        [Test]
        public void GetName_Battle_ReturnsBattle()
        {
            Assert.AreEqual("Battle", SceneNameCatalog.GetName(SceneId.Battle));
        }

        [Test]
        public void GetName_GameClear_ReturnsGameClear()
        {
            Assert.AreEqual("GameClear", SceneNameCatalog.GetName(SceneId.GameClear));
        }

        [Test]
        public void SceneNameCatalog_AllEnumValues_AreRegisteredInCatalog()
        {
            foreach (var sceneId in AllSceneIds)
            {
                Assert.DoesNotThrow(() => SceneNameCatalog.GetName(sceneId), $"{sceneId} is not registered in SceneNameCatalog.");
            }
        }

        [Test]
        public void SceneNameCatalog_HasNoEntriesBeyondDefinedSceneIds()
        {
            var definedCount = AllSceneIds.Count();

            Assert.AreEqual(definedCount, SceneNameCatalog.RegisteredCount);
        }
    }
}
