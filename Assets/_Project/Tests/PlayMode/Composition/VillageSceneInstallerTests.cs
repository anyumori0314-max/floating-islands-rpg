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
using UnityEngine.UI;

namespace FloatingIslandsRpg.Tests.PlayMode.Composition
{
    public sealed class VillageSceneInstallerTests
    {
        private GameObject _rootObject;
        private GameObject _npcObject;
        private GameObject _npcObject2;
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

            if (_npcObject2 != null)
            {
                Object.DestroyImmediate(_npcObject2);
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

        private IEnumerator BuildSceneWithMainQuestGiver(PlayerSessionState currentSession)
        {
            _rootObject = new GameObject("Root");
            var root = _rootObject.AddComponent<GameCompositionRoot>();
            root.Services.CurrentSession = currentSession;

            _npcObject = new GameObject("Npc");
            var npc = _npcObject.AddComponent<NpcInteractable>();
            SetPrivateField(npc, "_dialogueLines", new[] { "Hello" });

            var dialogueViewObject = new GameObject("DialogueBoxView");
            dialogueViewObject.SetActive(false);
            var dialogueRoot = new GameObject("DialogueRoot");
            var textObject = new GameObject("Text");
            textObject.transform.SetParent(dialogueRoot.transform);
            var dialogueText = textObject.AddComponent<UnityEngine.UI.Text>();
            var dialogueView = dialogueViewObject.AddComponent<FloatingIslandsRpg.Presentation.Dialogue.DialogueBoxView>();
            SetPrivateField(dialogueView, "_root", dialogueRoot);
            SetPrivateField(dialogueView, "_lineText", dialogueText);
            dialogueViewObject.SetActive(true);
            SetPrivateField(npc, "_dialogueBoxView", dialogueView);

            _installerObject = new GameObject("VillageSceneInstaller");
            var installer = _installerObject.AddComponent<VillageSceneInstaller>();
            SetPrivateField(installer, "_mainQuestGiver", npc);

            yield return null;
        }

        [UnityTest]
        public IEnumerator MainQuestGiverDialogueStarted_WithCurrentSession_StartsMainQuest()
        {
            var mainQuest = new MainQuestProgress();
            var stats = new CharacterStats(1, 20, 5, 5, 2, 5, 0);
            var session = new PlayerSessionState(
                SceneId.Village, stats, 0, 20, 5, mainQuest, new QuestProgress(), new QuestProgress());
            yield return BuildSceneWithMainQuestGiver(session);

            _npcObject.GetComponent<NpcInteractable>().RequestStart();

            Assert.AreEqual(MainQuestStage.ExploreField, mainQuest.CurrentStage);
        }

        [UnityTest]
        public IEnumerator MainQuestGiverDialogueStarted_WithoutCurrentSession_DoesNotThrow()
        {
            yield return BuildSceneWithMainQuestGiver(null);

            Assert.DoesNotThrow(() => _npcObject.GetComponent<NpcInteractable>().RequestStart());
        }

        [UnityTest]
        public IEnumerator MainQuestGiverDialogueStarted_QuestAlreadyStarted_DoesNotThrowOrRegress()
        {
            var mainQuest = new MainQuestProgress();
            mainQuest.Start();
            mainQuest.AdvanceToEnterDungeon();
            var stats = new CharacterStats(1, 20, 5, 5, 2, 5, 0);
            var session = new PlayerSessionState(
                SceneId.Village, stats, 0, 20, 5, mainQuest, new QuestProgress(), new QuestProgress());
            yield return BuildSceneWithMainQuestGiver(session);

            _npcObject.GetComponent<NpcInteractable>().RequestStart();

            Assert.AreEqual(MainQuestStage.EnterDungeon, mainQuest.CurrentStage);
        }

        [UnityTest]
        public IEnumerator OnDestroy_UnsubscribesFromMainQuestGiver()
        {
            var mainQuest = new MainQuestProgress();
            var stats = new CharacterStats(1, 20, 5, 5, 2, 5, 0);
            var session = new PlayerSessionState(
                SceneId.Village, stats, 0, 20, 5, mainQuest, new QuestProgress(), new QuestProgress());
            yield return BuildSceneWithMainQuestGiver(session);

            var npc = _npcObject.GetComponent<NpcInteractable>();
            Object.DestroyImmediate(_installerObject);
            _installerObject = null;

            npc.RequestStart();

            Assert.AreEqual(MainQuestStage.NotStarted, mainQuest.CurrentStage);
        }

        // Mirrors BuildSceneWithMainQuestGiver's NPC+DialogueBoxView construction, but wires both
        // subquest givers (PROJECT.md T-025) onto a single installer/root, since the two
        // subquests' independence from each other is itself part of what must be verified.
        private static GameObject CreateInteractableNpc(string name)
        {
            var npcObject = new GameObject(name);
            var npc = npcObject.AddComponent<NpcInteractable>();
            SetPrivateField(npc, "_dialogueLines", new[] { "Hello" });

            var dialogueViewObject = new GameObject(name + "DialogueBoxView");
            dialogueViewObject.SetActive(false);
            var dialogueRoot = new GameObject(name + "DialogueRoot");
            var textObject = new GameObject(name + "Text");
            textObject.transform.SetParent(dialogueRoot.transform);
            var dialogueText = textObject.AddComponent<UnityEngine.UI.Text>();
            var dialogueView = dialogueViewObject.AddComponent<FloatingIslandsRpg.Presentation.Dialogue.DialogueBoxView>();
            SetPrivateField(dialogueView, "_root", dialogueRoot);
            SetPrivateField(dialogueView, "_lineText", dialogueText);
            dialogueViewObject.SetActive(true);
            SetPrivateField(npc, "_dialogueBoxView", dialogueView);

            return npcObject;
        }

        private IEnumerator BuildSceneWithSubQuestGivers(PlayerSessionState currentSession)
        {
            _rootObject = new GameObject("Root");
            var root = _rootObject.AddComponent<GameCompositionRoot>();
            root.Services.CurrentSession = currentSession;

            _npcObject = CreateInteractableNpc("SubQuest1Npc");
            _npcObject2 = CreateInteractableNpc("SubQuest2Npc");

            _installerObject = new GameObject("VillageSceneInstaller");
            var installer = _installerObject.AddComponent<VillageSceneInstaller>();
            SetPrivateField(installer, "_subQuest1Giver", _npcObject.GetComponent<NpcInteractable>());
            SetPrivateField(installer, "_subQuest2Giver", _npcObject2.GetComponent<NpcInteractable>());

            yield return null;
        }

        [UnityTest]
        public IEnumerator SubQuest1GiverDialogueStarted_WithCurrentSession_StartsSubQuest1()
        {
            var mainQuest = new MainQuestProgress();
            var subQuest1 = new QuestProgress();
            var stats = new CharacterStats(1, 20, 5, 5, 2, 5, 0);
            var session = new PlayerSessionState(
                SceneId.Village, stats, 0, 20, 5, mainQuest, subQuest1, new QuestProgress());
            yield return BuildSceneWithSubQuestGivers(session);

            _npcObject.GetComponent<NpcInteractable>().RequestStart();

            Assert.AreEqual(QuestState.InProgress, subQuest1.CurrentState);
        }

        [UnityTest]
        public IEnumerator SubQuest2GiverDialogueStarted_WithCurrentSession_StartsSubQuest2()
        {
            var mainQuest = new MainQuestProgress();
            var subQuest2 = new QuestProgress();
            var stats = new CharacterStats(1, 20, 5, 5, 2, 5, 0);
            var session = new PlayerSessionState(
                SceneId.Village, stats, 0, 20, 5, mainQuest, new QuestProgress(), subQuest2);
            yield return BuildSceneWithSubQuestGivers(session);

            _npcObject2.GetComponent<NpcInteractable>().RequestStart();

            Assert.AreEqual(QuestState.InProgress, subQuest2.CurrentState);
        }

        [UnityTest]
        public IEnumerator SubQuestGiversDialogueStarted_AreIndependentOfEachOther()
        {
            var mainQuest = new MainQuestProgress();
            var subQuest1 = new QuestProgress();
            var subQuest2 = new QuestProgress();
            var stats = new CharacterStats(1, 20, 5, 5, 2, 5, 0);
            var session = new PlayerSessionState(
                SceneId.Village, stats, 0, 20, 5, mainQuest, subQuest1, subQuest2);
            yield return BuildSceneWithSubQuestGivers(session);

            _npcObject.GetComponent<NpcInteractable>().RequestStart();

            Assert.AreEqual(QuestState.InProgress, subQuest1.CurrentState);
            Assert.AreEqual(QuestState.NotStarted, subQuest2.CurrentState);
            // Independent of MainQuest too: never touched by either subquest giver.
            Assert.AreEqual(MainQuestStage.NotStarted, mainQuest.CurrentStage);
        }

        [UnityTest]
        public IEnumerator SubQuest1GiverDialogueStarted_WithoutCurrentSession_DoesNotThrow()
        {
            yield return BuildSceneWithSubQuestGivers(null);

            Assert.DoesNotThrow(() => _npcObject.GetComponent<NpcInteractable>().RequestStart());
        }

        [UnityTest]
        public IEnumerator SubQuest1GiverDialogueStarted_AlreadyCompleted_DoesNotThrowOrRegress()
        {
            var subQuest1 = new QuestProgress();
            subQuest1.Start();
            subQuest1.Complete();
            var stats = new CharacterStats(1, 20, 5, 5, 2, 5, 0);
            var session = new PlayerSessionState(
                SceneId.Village, stats, 0, 20, 5, new MainQuestProgress(), subQuest1, new QuestProgress());
            yield return BuildSceneWithSubQuestGivers(session);

            _npcObject.GetComponent<NpcInteractable>().RequestStart();

            Assert.AreEqual(QuestState.Completed, subQuest1.CurrentState);
        }

        [UnityTest]
        public IEnumerator OnDestroy_UnsubscribesFromSubQuestGivers()
        {
            var subQuest1 = new QuestProgress();
            var stats = new CharacterStats(1, 20, 5, 5, 2, 5, 0);
            var session = new PlayerSessionState(
                SceneId.Village, stats, 0, 20, 5, new MainQuestProgress(), subQuest1, new QuestProgress());
            yield return BuildSceneWithSubQuestGivers(session);

            var npc = _npcObject.GetComponent<NpcInteractable>();
            Object.DestroyImmediate(_installerObject);
            _installerObject = null;

            npc.RequestStart();

            Assert.AreEqual(QuestState.NotStarted, subQuest1.CurrentState);
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
