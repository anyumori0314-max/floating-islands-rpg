using System.Reflection;
using FloatingIslandsRpg.Domain.Quests;
using FloatingIslandsRpg.Presentation.Dialogue;
using FloatingIslandsRpg.Presentation.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace FloatingIslandsRpg.Tests.PlayMode.Dialogue
{
    public sealed class NpcInteractableTests
    {
        private InputActionAsset _moveActionAsset;
        private InputActionReference _moveActionReference;
        private GameObject _playerObject;
        private PlayerMovement _playerMovement;

        private GameObject _dialogueRootObject;
        private Text _dialogueText;
        private GameObject _dialogueViewObject;
        private DialogueBoxView _dialogueView;

        [SetUp]
        public void SetUp()
        {
            _moveActionAsset = ScriptableObject.CreateInstance<InputActionAsset>();
            var actionMap = _moveActionAsset.AddActionMap("Player");
            var moveAction = actionMap.AddAction("Move", InputActionType.Value, expectedControlLayout: "Vector2");
            _moveActionReference = InputActionReference.Create(moveAction);

            _playerObject = new GameObject("TestPlayer");
            _playerObject.SetActive(false);
            _playerObject.AddComponent<CharacterController>();
            _playerMovement = _playerObject.AddComponent<PlayerMovement>();
            SetPrivateField(_playerMovement, "_moveAction", _moveActionReference);
            _playerObject.SetActive(true);

            _dialogueRootObject = new GameObject("DialogueRoot");
            var textObject = new GameObject("Text");
            textObject.transform.SetParent(_dialogueRootObject.transform);
            _dialogueText = textObject.AddComponent<Text>();

            _dialogueViewObject = new GameObject("DialogueBoxView");
            _dialogueViewObject.SetActive(false);
            _dialogueView = _dialogueViewObject.AddComponent<DialogueBoxView>();
            SetPrivateField(_dialogueView, "_root", _dialogueRootObject);
            SetPrivateField(_dialogueView, "_lineText", _dialogueText);
            _dialogueViewObject.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_dialogueViewObject);
            Object.DestroyImmediate(_dialogueRootObject);
            Object.DestroyImmediate(_playerObject);

            if (_moveActionReference != null)
            {
                Object.DestroyImmediate(_moveActionReference);
            }

            if (_moveActionAsset != null)
            {
                Object.DestroyImmediate(_moveActionAsset);
            }
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(target, value);
        }

        private NpcInteractable CreateNpc(string name, string[] lines, out GameObject npcObject)
        {
            npcObject = new GameObject(name);
            var npc = npcObject.AddComponent<NpcInteractable>();
            SetPrivateField(npc, "_dialogueLines", lines);
            SetPrivateField(npc, "_dialogueBoxView", _dialogueView);
            SetPrivateField(npc, "_playerMovement", _playerMovement);
            return npc;
        }

        [Test]
        public void RequestStart_ValidConfiguration_OpensDialogueAndDisablesPlayerMovement()
        {
            var npc = CreateNpc("Npc", new[] { "Hi" }, out var npcObject);

            var started = npc.RequestStart();

            Assert.IsTrue(started);
            Assert.IsTrue(_dialogueView.IsOpen);
            Assert.IsFalse(_playerMovement.enabled);

            Object.DestroyImmediate(npcObject);
        }

        [Test]
        public void RequestStart_WhenDialogueAlreadyOpen_ReturnsFalseAndDoesNotReopen()
        {
            var npcA = CreateNpc("NpcA", new[] { "A1" }, out var npcAObject);
            var npcB = CreateNpc("NpcB", new[] { "B1" }, out var npcBObject);

            npcA.RequestStart();
            var startedB = npcB.RequestStart();

            Assert.IsFalse(startedB);
            Assert.AreEqual("A1", _dialogueText.text);

            Object.DestroyImmediate(npcAObject);
            Object.DestroyImmediate(npcBObject);
        }

        [Test]
        public void DialogueClosed_ReEnablesPlayerMovement()
        {
            var npc = CreateNpc("Npc", new[] { "OnlyLine" }, out var npcObject);

            npc.RequestStart();
            Assert.IsFalse(_playerMovement.enabled);

            _dialogueView.Advance();

            Assert.IsFalse(_dialogueView.IsOpen);
            Assert.IsTrue(_playerMovement.enabled);

            Object.DestroyImmediate(npcObject);
        }

        [Test]
        public void RequestStart_NoDialogueLines_ReturnsFalseAndDoesNotOpen()
        {
            var npc = CreateNpc("Npc", new string[0], out var npcObject);

            var started = npc.RequestStart();

            Assert.IsFalse(started);
            Assert.IsFalse(_dialogueView.IsOpen);
            LogAssert.NoUnexpectedReceived();

            Object.DestroyImmediate(npcObject);
        }

        [Test]
        public void RequestStart_LinkedQuestNotStarted_StartsQuest()
        {
            var npc = CreateNpc("Npc", new[] { "QuestLine" }, out var npcObject);
            var quest = new QuestProgress();
            npc.LinkedQuest = quest;

            npc.RequestStart();

            Assert.AreEqual(QuestState.InProgress, quest.CurrentState);

            Object.DestroyImmediate(npcObject);
        }

        [Test]
        public void RequestStart_LinkedQuestAlreadyInProgress_DoesNotThrowAndKeepsState()
        {
            var npc = CreateNpc("Npc", new[] { "QuestLine" }, out var npcObject);
            var quest = new QuestProgress();
            quest.Start();
            npc.LinkedQuest = quest;

            npc.RequestStart();

            Assert.AreEqual(QuestState.InProgress, quest.CurrentState);

            Object.DestroyImmediate(npcObject);
        }

        [Test]
        public void TwoInstances_IndependentDialogueState()
        {
            var npcA = CreateNpc("NpcA", new[] { "A1", "A2" }, out var npcAObject);
            var npcB = CreateNpc("NpcB", new[] { "B1" }, out var npcBObject);
            var questA = new QuestProgress();
            npcA.LinkedQuest = questA;

            npcA.RequestStart();
            _dialogueView.Advance();
            _dialogueView.Advance();

            Assert.IsFalse(_dialogueView.IsOpen);
            Assert.AreEqual(QuestState.InProgress, questA.CurrentState);
            Assert.IsTrue(_playerMovement.enabled);

            var startedB = npcB.RequestStart();

            Assert.IsTrue(startedB);
            Assert.AreEqual("B1", _dialogueText.text);
            Assert.IsFalse(_playerMovement.enabled);
            Assert.AreEqual(QuestState.InProgress, questA.CurrentState);

            Object.DestroyImmediate(npcAObject);
            Object.DestroyImmediate(npcBObject);
        }

        [Test]
        public void OnDestroy_WhileDialogueOpen_UnsubscribesWithoutError()
        {
            var npc = CreateNpc("Npc", new[] { "Line" }, out var npcObject);
            npc.RequestStart();

            Object.DestroyImmediate(npcObject);

            Assert.DoesNotThrow(() => _dialogueView.Advance());
        }
    }
}
