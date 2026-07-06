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
    public sealed class FieldSceneInstallerTests
    {
        private GameObject _rootObject;
        private GameObject _encounterObject;
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

            _gateObject = new GameObject("FieldActivityGate");
            _gateObject.AddComponent<FieldActivityGate>();

            _triggerObject = new GameObject("VillageEntrance");
            var trigger = _triggerObject.AddComponent<SceneTransitionTrigger>();
            SetPrivateField(trigger, "_destinationSceneId", SceneId.Village);
            SetPrivateField(trigger, "_loadMode", SceneLoadMode.Single);

            _installerObject = new GameObject("FieldSceneInstaller");
            _installerObject.AddComponent<FieldSceneInstaller>();

            yield return null;
        }

        [UnityTest]
        public IEnumerator EncounterTriggered_SetsPendingBattleAndLoadsBattleAdditively()
        {
            yield return BuildScene();

            var encounterController = _encounterObject.GetComponent<FieldEncounterController>();
            InvokeEncounterTriggered(encounterController);
            yield return null;

            Assert.IsNotNull(_services.PendingBattle);
            Assert.AreEqual(SceneId.Field, _services.PendingBattle.ReturnSceneId);
            Assert.IsFalse(_services.PendingBattle.IsBossEncounter);
            Assert.AreEqual(SceneId.Battle, _fakeSceneLoader.LastLoadedSceneId);
            Assert.AreEqual(SceneLoadMode.Additive, _fakeSceneLoader.LastLoadMode);
        }

        [UnityTest]
        public IEnumerator EncounterTriggered_PausesActivityGate()
        {
            yield return BuildScene();
            var gate = _gateObject.GetComponent<FieldActivityGate>();
            var cameraObject = new GameObject("Camera");
            var camera = cameraObject.AddComponent<Camera>();
            SetPrivateField(gate, "_fieldCamera", camera);

            var encounterController = _encounterObject.GetComponent<FieldEncounterController>();
            InvokeEncounterTriggered(encounterController);
            yield return null;

            Assert.IsFalse(camera.enabled);

            Object.DestroyImmediate(cameraObject);
        }

        [UnityTest]
        public IEnumerator EncounterTriggered_TransitionFails_ClearsPendingBattleAndResumesGate()
        {
            yield return BuildScene();
            _fakeSceneLoader.FailWith = new System.Exception("Simulated additive load failure");
            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("Simulated additive load failure"));

            var gate = _gateObject.GetComponent<FieldActivityGate>();
            var cameraObject = new GameObject("Camera");
            var camera = cameraObject.AddComponent<Camera>();
            SetPrivateField(gate, "_fieldCamera", camera);
            camera.enabled = false;

            var encounterController = _encounterObject.GetComponent<FieldEncounterController>();
            InvokeEncounterTriggered(encounterController);
            yield return null;

            Assert.IsNull(_services.PendingBattle);
            Assert.IsTrue(camera.enabled);

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

            Assert.AreEqual(SceneId.Village, _fakeSceneLoader.LastLoadedSceneId);
            Assert.AreEqual(SceneLoadMode.Single, _fakeSceneLoader.LastLoadMode);

            Object.DestroyImmediate(playerObject);
        }

        [UnityTest]
        public IEnumerator OnDestroy_UnsubscribesFromEncounterController()
        {
            yield return BuildScene();

            var encounterController = _encounterObject.GetComponent<FieldEncounterController>();
            Object.DestroyImmediate(_installerObject);
            _installerObject = null;

            InvokeEncounterTriggered(encounterController);
            yield return null;

            Assert.IsNull(_services.PendingBattle);
            Assert.IsNull(_fakeSceneLoader.LastLoadedSceneId);
        }

        private static void InvokeEncounterTriggered(FieldEncounterController controller)
        {
            var field = typeof(FieldEncounterController).GetField("EncounterTriggered", BindingFlags.NonPublic | BindingFlags.Instance);
            var handler = (System.Action)field.GetValue(controller);
            handler?.Invoke();
        }

        private static PlayerSessionState CreateSessionAtStage(MainQuestStage stage)
        {
            var mainQuest = new MainQuestProgress();
            if (stage >= MainQuestStage.ExploreField)
            {
                mainQuest.Start();
            }

            var stats = new CharacterStats(1, 20, 5, 5, 3, 5, 2);
            return new PlayerSessionState(
                SceneId.Field, stats, 0, 20, 5, mainQuest, new QuestProgress(), new QuestProgress());
        }

        [UnityTest]
        public IEnumerator Start_MainQuestAtExploreField_AdvancesToEnterDungeon()
        {
            var session = CreateSessionAtStage(MainQuestStage.ExploreField);
            yield return BuildScene(session);

            Assert.AreEqual(MainQuestStage.EnterDungeon, session.MainQuest.CurrentStage);
        }

        [UnityTest]
        public IEnumerator Start_MainQuestNotStarted_DoesNotAdvance()
        {
            var session = CreateSessionAtStage(MainQuestStage.NotStarted);
            yield return BuildScene(session);

            Assert.AreEqual(MainQuestStage.NotStarted, session.MainQuest.CurrentStage);
        }

        private static PlayerSessionState CreateSessionWithSubQuest1(QuestState subQuest1State)
        {
            var subQuest1 = new QuestProgress();
            if (subQuest1State == QuestState.InProgress || subQuest1State == QuestState.Completed)
            {
                subQuest1.Start();
            }

            if (subQuest1State == QuestState.Completed)
            {
                subQuest1.Complete();
            }

            var stats = new CharacterStats(1, 20, 5, 5, 3, 5, 2);
            return new PlayerSessionState(
                SceneId.Field, stats, 0, 20, 5, new MainQuestProgress(), subQuest1, new QuestProgress());
        }

        [UnityTest]
        public IEnumerator Start_SubQuest1InProgress_CompletesSubQuest1()
        {
            var session = CreateSessionWithSubQuest1(QuestState.InProgress);
            yield return BuildScene(session);

            Assert.AreEqual(QuestState.Completed, session.SubQuest1.CurrentState);
        }

        [UnityTest]
        public IEnumerator Start_SubQuest1NotStarted_DoesNotComplete()
        {
            var session = CreateSessionWithSubQuest1(QuestState.NotStarted);
            yield return BuildScene(session);

            Assert.AreEqual(QuestState.NotStarted, session.SubQuest1.CurrentState);
        }

        [UnityTest]
        public IEnumerator Start_SubQuest1AlreadyCompleted_StaysCompletedAndDoesNotThrow()
        {
            var session = CreateSessionWithSubQuest1(QuestState.Completed);

            yield return BuildScene(session);

            Assert.AreEqual(QuestState.Completed, session.SubQuest1.CurrentState);
        }

        [UnityTest]
        public IEnumerator Start_SubQuest1InProgress_DoesNotAffectMainQuestOrSubQuest2()
        {
            var mainQuest = new MainQuestProgress();
            var subQuest1 = new QuestProgress();
            subQuest1.Start();
            var subQuest2 = new QuestProgress();
            var stats = new CharacterStats(1, 20, 5, 5, 3, 5, 2);
            var session = new PlayerSessionState(SceneId.Field, stats, 0, 20, 5, mainQuest, subQuest1, subQuest2);
            yield return BuildScene(session);

            Assert.AreEqual(QuestState.Completed, session.SubQuest1.CurrentState);
            Assert.AreEqual(QuestState.NotStarted, session.SubQuest2.CurrentState);
            Assert.AreEqual(MainQuestStage.NotStarted, session.MainQuest.CurrentStage);
        }

        [UnityTest]
        public IEnumerator Start_WithoutCurrentSession_DoesNotThrowAndSceneStillFunctions()
        {
            yield return BuildScene(null);

            var encounterController = _encounterObject.GetComponent<FieldEncounterController>();
            InvokeEncounterTriggered(encounterController);
            yield return null;

            Assert.IsNotNull(_services.PendingBattle);
        }
    }
}
