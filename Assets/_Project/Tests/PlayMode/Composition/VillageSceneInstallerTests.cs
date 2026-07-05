using System.Collections;
using FloatingIslandsRpg.Application.Scenes;
using FloatingIslandsRpg.Application.Session;
using FloatingIslandsRpg.Composition;
using FloatingIslandsRpg.Composition.Scenes;
using FloatingIslandsRpg.Domain.Characters.Stats;
using FloatingIslandsRpg.Domain.Quests;
using FloatingIslandsRpg.Presentation.Dialogue;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace FloatingIslandsRpg.Tests.PlayMode.Composition
{
    public sealed class VillageSceneInstallerTests
    {
        private GameObject _rootObject;
        private GameObject _npcObject;
        private GameObject _installerObject;

        [TearDown]
        public void TearDown()
        {
            if (_installerObject != null)
            {
                Object.DestroyImmediate(_installerObject);
            }

            if (_npcObject != null)
            {
                Object.DestroyImmediate(_npcObject);
            }

            if (_rootObject != null)
            {
                Object.DestroyImmediate(_rootObject);
            }
        }

        [UnityTest]
        public IEnumerator Start_WithCurrentSession_LinksNpcToMainQuest()
        {
            _rootObject = new GameObject("Root");
            var root = _rootObject.AddComponent<GameCompositionRoot>();
            var mainQuest = new QuestProgress();
            var stats = new CharacterStats(1, 20, 5, 5, 2, 5, 0);
            root.Services.CurrentSession = new PlayerSessionState(
                SceneId.Village, stats, 0, 20, 5, mainQuest, new QuestProgress(), new QuestProgress());

            _npcObject = new GameObject("Npc");
            var npc = _npcObject.AddComponent<NpcInteractable>();

            _installerObject = new GameObject("VillageSceneInstaller");
            _installerObject.AddComponent<VillageSceneInstaller>();

            yield return null;

            Assert.AreSame(mainQuest, npc.LinkedQuest);
        }

        [UnityTest]
        public IEnumerator Start_WithoutCurrentSession_DoesNotThrow()
        {
            _rootObject = new GameObject("Root");
            _rootObject.AddComponent<GameCompositionRoot>();

            _npcObject = new GameObject("Npc");
            var npc = _npcObject.AddComponent<NpcInteractable>();

            _installerObject = new GameObject("VillageSceneInstaller");
            _installerObject.AddComponent<VillageSceneInstaller>();

            yield return null;

            Assert.IsNull(npc.LinkedQuest);
        }
    }
}
