using System.Collections;
using System.Reflection;
using FloatingIslandsRpg.Application.Scenes;
using FloatingIslandsRpg.Application.Session;
using FloatingIslandsRpg.Composition;
using FloatingIslandsRpg.Composition.Scenes;
using FloatingIslandsRpg.Domain.Characters.Stats;
using FloatingIslandsRpg.Domain.Quests;
using FloatingIslandsRpg.Presentation.Encounters;
using FloatingIslandsRpg.Presentation.Scenes;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace FloatingIslandsRpg.Tests.PlayMode.Composition
{
    public sealed class DungeonSceneInstallerTests
    {
        private GameObject _rootObject;
        private GameObject _encounterObject;
        private GameObject _bossTriggerObject;
        private GameObject _gateObject;
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

            if (_encounterObject != null)
            {
                Object.DestroyImmediate(_encounterObject);
            }

            if (_bossTriggerObject != null)
            {
                Object.DestroyImmediate(_bossTriggerObject);
            }

            if (_gateObject != null)
            {
                Object.DestroyImmediate(_gateObject);
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
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(target, value);
        }

        private IEnumerator BuildScene()
        {
            yield return BuildScene(null);
        }

        private IEnumerator BuildScene(PlayerSessionState currentSession)
        {
            _rootObject = new GameObject("Root");
            var root = _rootObject.AddComponent<GameCompositionRoot>();
            _services = root.Services;
            _services.CurrentSession = currentSession;
            _fakeSceneLoader = new FakeSceneLoader();
            _services.SceneTransitionUseCase = new SceneTransitionUseCase(_fakeSceneLoader);

            _encounterObject = new GameObject("FieldEncounterController");
            _encounterObject.AddComponent<FieldEncounterController>();

            _bossTriggerObject = new GameObject("BossEncounterTrigger");
            _bossTriggerObject.AddComponent<BossEncounterTrigger>();

            _gateObject = new GameObject("FieldActivityGate");
            _gateObject.AddComponent<FieldActivityGate>();

            _triggerObject = new GameObject("FieldExit");
            var trigger = _triggerObject.AddComponent<SceneTransitionTrigger>();
            SetPrivateField(trigger, "_destinationSceneId", SceneId.Field);
            SetPrivateField(trigger, "_loadMode", SceneLoadMode.Single);

            _installerObject = new GameObject("DungeonSceneInstaller");
            _installerObject.AddComponent<DungeonSceneInstaller>();

            yield return null;
        }

        private static void InvokeEvent(object target, string eventFieldName)
        {
            var field = target.GetType().GetField(eventFieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            var handler = (System.Action)field.GetValue(target);
            handler?.Invoke();
        }

        [UnityTest]
        public IEnumerator RegularEncounterTriggered_SetsNonBossPendingBattleAndLoadsBattleAdditively()
        {
            yield return BuildScene();

            var encounterController = _encounterObject.GetComponent<FieldEncounterController>();
            InvokeEvent(encounterController, "EncounterTriggered");
            yield return null;

            Assert.IsNotNull(_services.PendingBattle);
            Assert.AreEqual(SceneId.Dungeon, _services.PendingBattle.ReturnSceneId);
            Assert.IsFalse(_services.PendingBattle.IsBossEncounter);
            Assert.AreEqual(SceneId.Battle, _fakeSceneLoader.LastLoadedSceneId);
            Assert.AreEqual(SceneLoadMode.Additive, _fakeSceneLoader.LastLoadMode);
        }

        [UnityTest]
        public IEnumerator BossEncounterTriggered_SetsBossPendingBattleAndLoadsBattleAdditively()
        {
            yield return BuildScene();

            var bossTrigger = _bossTriggerObject.GetComponent<BossEncounterTrigger>();
            InvokeEvent(bossTrigger, "BossEncounterTriggered");
            yield return null;

            Assert.IsNotNull(_services.PendingBattle);
            Assert.AreEqual(SceneId.Dungeon, _services.PendingBattle.ReturnSceneId);
            Assert.IsTrue(_services.PendingBattle.IsBossEncounter);
            Assert.AreEqual(SceneId.Battle, _fakeSceneLoader.LastLoadedSceneId);
            Assert.AreEqual(SceneLoadMode.Additive, _fakeSceneLoader.LastLoadMode);
        }

        [UnityTest]
        public IEnumerator BossEncounterTriggered_PausesActivityGate()
        {
            yield return BuildScene();
            var gate = _gateObject.GetComponent<FieldActivityGate>();
            var cameraObject = new GameObject("Camera");
            var camera = cameraObject.AddComponent<Camera>();
            SetPrivateField(gate, "_fieldCamera", camera);

            var bossTrigger = _bossTriggerObject.GetComponent<BossEncounterTrigger>();
            InvokeEvent(bossTrigger, "BossEncounterTriggered");
            yield return null;

            Assert.IsFalse(camera.enabled);

            Object.DestroyImmediate(cameraObject);
        }

        [UnityTest]
        public IEnumerator BossEncounterTriggered_TransitionFails_ClearsPendingBattleResumesGateAndAllowsRetry()
        {
            yield return BuildScene();
            _fakeSceneLoader.FailWith = new System.Exception("Simulated boss load failure");
            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("Simulated boss load failure"));

            var gate = _gateObject.GetComponent<FieldActivityGate>();
            var cameraObject = new GameObject("Camera");
            var camera = cameraObject.AddComponent<Camera>();
            SetPrivateField(gate, "_fieldCamera", camera);
            camera.enabled = false;

            var bossTrigger = _bossTriggerObject.GetComponent<BossEncounterTrigger>();
            InvokeEvent(bossTrigger, "BossEncounterTriggered");
            yield return null;

            Assert.IsNull(_services.PendingBattle);
            Assert.IsTrue(camera.enabled);

            _fakeSceneLoader.FailWith = null;
            InvokeEvent(bossTrigger, "BossEncounterTriggered");
            yield return null;

            Assert.AreEqual(2, _fakeSceneLoader.LoadCallCount);

            Object.DestroyImmediate(cameraObject);
        }

        [UnityTest]
        public IEnumerator TransitionTriggerFires_RequestsConfiguredSceneTransition()
        {
            yield return BuildScene();

            var trigger = _triggerObject.GetComponent<SceneTransitionTrigger>();
            var method = typeof(SceneTransitionTrigger).GetMethod("OnTriggerEnter", BindingFlags.NonPublic | BindingFlags.Instance);

            var playerObject = new GameObject("Player");
            playerObject.SetActive(false);
            var collider = playerObject.AddComponent<SphereCollider>();
            playerObject.AddComponent<CharacterController>();
            playerObject.AddComponent<FloatingIslandsRpg.Presentation.Player.PlayerMovement>();

            method.Invoke(trigger, new object[] { collider });
            yield return null;

            Assert.AreEqual(SceneId.Field, _fakeSceneLoader.LastLoadedSceneId);
            Assert.AreEqual(SceneLoadMode.Single, _fakeSceneLoader.LastLoadMode);

            Object.DestroyImmediate(playerObject);
        }

        [UnityTest]
        public IEnumerator OnDestroy_UnsubscribesFromBossEncounterTrigger()
        {
            yield return BuildScene();

            var bossTrigger = _bossTriggerObject.GetComponent<BossEncounterTrigger>();
            Object.DestroyImmediate(_installerObject);
            _installerObject = null;

            InvokeEvent(bossTrigger, "BossEncounterTriggered");
            yield return null;

            Assert.IsNull(_services.PendingBattle);
            Assert.IsNull(_fakeSceneLoader.LastLoadedSceneId);
        }

        private static PlayerSessionState CreateSessionAtStage(MainQuestStage stage)
        {
            var mainQuest = new MainQuestProgress();
            if (stage >= MainQuestStage.ExploreField)
            {
                mainQuest.Start();
            }

            if (stage >= MainQuestStage.EnterDungeon)
            {
                mainQuest.AdvanceToEnterDungeon();
            }

            var stats = new CharacterStats(1, 20, 5, 5, 3, 5, 2);
            return new PlayerSessionState(
                SceneId.Dungeon, stats, 0, 20, 5, mainQuest, new QuestProgress(), new QuestProgress());
        }

        [UnityTest]
        public IEnumerator Start_MainQuestAtEnterDungeon_AdvancesToDefeatBoss()
        {
            var session = CreateSessionAtStage(MainQuestStage.EnterDungeon);
            yield return BuildScene(session);

            Assert.AreEqual(MainQuestStage.DefeatBoss, session.MainQuest.CurrentStage);
        }

        [UnityTest]
        public IEnumerator Start_MainQuestAtExploreField_DoesNotSkipToDefeatBoss()
        {
            var session = CreateSessionAtStage(MainQuestStage.ExploreField);
            yield return BuildScene(session);

            Assert.AreEqual(MainQuestStage.ExploreField, session.MainQuest.CurrentStage);
        }

        [UnityTest]
        public IEnumerator Start_WithoutCurrentSession_DoesNotThrowAndSceneStillFunctions()
        {
            yield return BuildScene(null);

            var encounterController = _encounterObject.GetComponent<FieldEncounterController>();
            InvokeEvent(encounterController, "EncounterTriggered");
            yield return null;

            Assert.IsNotNull(_services.PendingBattle);
        }
    }
}
