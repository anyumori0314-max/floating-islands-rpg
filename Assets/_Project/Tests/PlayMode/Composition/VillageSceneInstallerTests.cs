using System.Collections;
using FloatingIslandsRpg.Application.Scenes;
using FloatingIslandsRpg.Application.Session;
using FloatingIslandsRpg.Composition;
using FloatingIslandsRpg.Composition.Scenes;
using FloatingIslandsRpg.Domain.Characters.Stats;
using FloatingIslandsRpg.Domain.Quests;
using FloatingIslandsRpg.Presentation.Dialogue;
using FloatingIslandsRpg.Presentation.Scenes;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace FloatingIslandsRpg.Tests.PlayMode.Composition
{
    public sealed class VillageSceneInstallerTests
    {
        private GameObject _rootObject;
        private GameObject _npcObject;
        private GameObject _triggerObject;
        private GameObject _installerObject;
        private FakeSceneLoader _fakeSceneLoader;
        private GameServices _services;

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

            if (_triggerObject != null)
            {
                Object.DestroyImmediate(_triggerObject);
            }

            if (_rootObject != null)
            {
                Object.DestroyImmediate(_rootObject);
            }
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(target, value);
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

        private IEnumerator BuildSceneWithTrigger()
        {
            _rootObject = new GameObject("Root");
            var root = _rootObject.AddComponent<GameCompositionRoot>();
            _services = root.Services;
            _fakeSceneLoader = new FakeSceneLoader();
            _services.SceneTransitionUseCase = new SceneTransitionUseCase(_fakeSceneLoader);

            _triggerObject = new GameObject("FieldEntrance");
            var trigger = _triggerObject.AddComponent<SceneTransitionTrigger>();
            SetPrivateField(trigger, "_destinationSceneId", SceneId.Field);
            SetPrivateField(trigger, "_loadMode", SceneLoadMode.Single);

            _installerObject = new GameObject("VillageSceneInstaller");
            _installerObject.AddComponent<VillageSceneInstaller>();

            yield return null;
        }

        [UnityTest]
        public IEnumerator TransitionTriggerFires_RequestsConfiguredSceneTransition()
        {
            yield return BuildSceneWithTrigger();

            var trigger = _triggerObject.GetComponent<SceneTransitionTrigger>();
            InvokeTriggerEnterWithPlayer(trigger);
            yield return null;

            Assert.AreEqual(SceneId.Field, _fakeSceneLoader.LastLoadedSceneId);
            Assert.AreEqual(SceneLoadMode.Single, _fakeSceneLoader.LastLoadMode);
        }

        [UnityTest]
        public IEnumerator OnDestroy_UnsubscribesFromTransitionTrigger()
        {
            yield return BuildSceneWithTrigger();

            var trigger = _triggerObject.GetComponent<SceneTransitionTrigger>();
            Object.DestroyImmediate(_installerObject);
            _installerObject = null;

            InvokeTriggerEnterWithPlayer(trigger);
            yield return null;

            Assert.IsNull(_fakeSceneLoader.LastLoadedSceneId);
        }

        [UnityTest]
        public IEnumerator TransitionFails_AllowsTriggerRetry()
        {
            yield return BuildSceneWithTrigger();
            _fakeSceneLoader.FailWith = new System.Exception("Simulated transition failure");
            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("Simulated transition failure"));

            var trigger = _triggerObject.GetComponent<SceneTransitionTrigger>();
            InvokeTriggerEnterWithPlayer(trigger);
            yield return null;

            Assert.AreEqual(1, _fakeSceneLoader.LoadCallCount);

            _fakeSceneLoader.FailWith = null;
            InvokeTriggerEnterWithPlayer(trigger);
            yield return null;

            Assert.AreEqual(2, _fakeSceneLoader.LoadCallCount);
        }

        private static void InvokeTriggerEnterWithPlayer(SceneTransitionTrigger trigger)
        {
            // Kept inactive throughout: PlayerMovement.Awake() logs an error when its
            // InputActionReference is unset, and only component *presence* (via
            // GetComponent) matters for the trigger's player check, not an active/enabled
            // PlayerMovement.
            var playerObject = new GameObject("Player");
            playerObject.SetActive(false);
            var collider = playerObject.AddComponent<SphereCollider>();
            playerObject.AddComponent<UnityEngine.CharacterController>();
            playerObject.AddComponent<FloatingIslandsRpg.Presentation.Player.PlayerMovement>();

            var method = typeof(SceneTransitionTrigger).GetMethod("OnTriggerEnter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(trigger, new object[] { collider });

            Object.DestroyImmediate(playerObject);
        }
    }
}
