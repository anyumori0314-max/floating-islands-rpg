using System.Collections;
using FloatingIslandsRpg.Application.Scenes;
using FloatingIslandsRpg.Application.Session;
using FloatingIslandsRpg.Composition;
using FloatingIslandsRpg.Composition.Scenes;
using FloatingIslandsRpg.Domain.Characters.Stats;
using FloatingIslandsRpg.Domain.MasterData;
using FloatingIslandsRpg.Domain.Quests;
using FloatingIslandsRpg.Infrastructure.MasterData;
using FloatingIslandsRpg.Presentation.Dialogue;
using FloatingIslandsRpg.Presentation.Items;
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

        private GameObject _inventoryPanelObject;
        private ItemDefinition _potionDefinition;
        private EquipmentDefinition _weaponDefinition;
        private EquipmentDefinition _armorDefinition;

        private IEnumerator BuildSceneWithInventoryPanel(PlayerSessionState session)
        {
            _rootObject = new GameObject("Root");
            var root = _rootObject.AddComponent<GameCompositionRoot>();
            root.Services.CurrentSession = session;

            _potionDefinition = ScriptableObject.CreateInstance<ItemDefinition>();
            SetPrivateField(_potionDefinition, "_id", "item_small_potion");
            SetPrivateField(_potionDefinition, "_displayName", "Small Potion");
            SetPrivateField(_potionDefinition, "_healAmount", 20);

            _weaponDefinition = ScriptableObject.CreateInstance<EquipmentDefinition>();
            SetPrivateField(_weaponDefinition, "_id", "equip_rusty_sword");
            SetPrivateField(_weaponDefinition, "_displayName", "Rusty Sword");
            SetPrivateField(_weaponDefinition, "_slot", EquipmentSlot.Weapon);
            SetPrivateField(_weaponDefinition, "_attackBonus", 3);

            _armorDefinition = ScriptableObject.CreateInstance<EquipmentDefinition>();
            SetPrivateField(_armorDefinition, "_id", "equip_traveler_armor");
            SetPrivateField(_armorDefinition, "_displayName", "Traveler Armor");
            SetPrivateField(_armorDefinition, "_slot", EquipmentSlot.Armor);
            SetPrivateField(_armorDefinition, "_defenseBonus", 3);

            _inventoryPanelObject = new GameObject("InventoryPanel");
            var statusText = new GameObject("StatusText").AddComponent<Text>();
            statusText.transform.SetParent(_inventoryPanelObject.transform);
            var panel = _inventoryPanelObject.AddComponent<InventoryPanelController>();
            SetPrivateField(panel, "_statusText", statusText);

            _installerObject = new GameObject("VillageSceneInstaller");
            var installer = _installerObject.AddComponent<VillageSceneInstaller>();
            SetPrivateField(installer, "_inventoryPanel", panel);
            SetPrivateField(installer, "_items", new[] { _potionDefinition });
            SetPrivateField(installer, "_weapons", new[] { _weaponDefinition });
            SetPrivateField(installer, "_armors", new[] { _armorDefinition });

            yield return null;
        }

        private static void InvokeUsePotionRequested(InventoryPanelController panel)
        {
            var field = typeof(InventoryPanelController).GetField("UsePotionRequested", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            ((System.Action)field.GetValue(panel))?.Invoke();
        }

        private static void InvokeEquipWeaponRequested(InventoryPanelController panel)
        {
            var field = typeof(InventoryPanelController).GetField("EquipWeaponRequested", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            ((System.Action)field.GetValue(panel))?.Invoke();
        }

        private static void InvokeEquipArmorRequested(InventoryPanelController panel)
        {
            var field = typeof(InventoryPanelController).GetField("EquipArmorRequested", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            ((System.Action)field.GetValue(panel))?.Invoke();
        }

        [UnityTest]
        public IEnumerator UsePotionRequested_OwnedPotion_ConsumesAndHeals()
        {
            var stats = new CharacterStats(2, 30, 10, 8, 4, 5, 2);
            var session = new PlayerSessionState(
                SceneId.Village, stats, 0, 5, 10,
                new MainQuestProgress(), new QuestProgress(), new QuestProgress());
            session.Inventory.Add("item_small_potion", 1);
            yield return BuildSceneWithInventoryPanel(session);

            InvokeUsePotionRequested(_inventoryPanelObject.GetComponent<InventoryPanelController>());

            Assert.AreEqual(0, session.Inventory.GetQuantity("item_small_potion"));
            Assert.AreEqual(25, session.CurrentHp);

            Object.DestroyImmediate(_inventoryPanelObject);
            Object.DestroyImmediate(_potionDefinition);
            Object.DestroyImmediate(_weaponDefinition);
            Object.DestroyImmediate(_armorDefinition);
        }

        [UnityTest]
        public IEnumerator UsePotionRequested_NoneOwned_DoesNothing()
        {
            var stats = new CharacterStats(2, 30, 10, 8, 4, 5, 2);
            var session = new PlayerSessionState(
                SceneId.Village, stats, 0, 5, 10,
                new MainQuestProgress(), new QuestProgress(), new QuestProgress());
            yield return BuildSceneWithInventoryPanel(session);

            InvokeUsePotionRequested(_inventoryPanelObject.GetComponent<InventoryPanelController>());

            Assert.AreEqual(5, session.CurrentHp);

            Object.DestroyImmediate(_inventoryPanelObject);
            Object.DestroyImmediate(_potionDefinition);
            Object.DestroyImmediate(_weaponDefinition);
            Object.DestroyImmediate(_armorDefinition);
        }

        [UnityTest]
        public IEnumerator EquipWeaponRequested_OwnedWeapon_Equips()
        {
            var stats = new CharacterStats(2, 30, 10, 8, 4, 5, 2);
            var session = new PlayerSessionState(
                SceneId.Village, stats, 0, 30, 10,
                new MainQuestProgress(), new QuestProgress(), new QuestProgress());
            session.Inventory.Add("equip_rusty_sword", 1);
            yield return BuildSceneWithInventoryPanel(session);

            InvokeEquipWeaponRequested(_inventoryPanelObject.GetComponent<InventoryPanelController>());

            Assert.AreEqual("equip_rusty_sword", session.Equipment.EquippedWeaponId);

            Object.DestroyImmediate(_inventoryPanelObject);
            Object.DestroyImmediate(_potionDefinition);
            Object.DestroyImmediate(_weaponDefinition);
            Object.DestroyImmediate(_armorDefinition);
        }

        [UnityTest]
        public IEnumerator EquipArmorRequested_OwnedArmor_Equips()
        {
            var stats = new CharacterStats(2, 30, 10, 8, 4, 5, 2);
            var session = new PlayerSessionState(
                SceneId.Village, stats, 0, 30, 10,
                new MainQuestProgress(), new QuestProgress(), new QuestProgress());
            session.Inventory.Add("equip_traveler_armor", 1);
            yield return BuildSceneWithInventoryPanel(session);

            InvokeEquipArmorRequested(_inventoryPanelObject.GetComponent<InventoryPanelController>());

            Assert.AreEqual("equip_traveler_armor", session.Equipment.EquippedArmorId);

            Object.DestroyImmediate(_inventoryPanelObject);
            Object.DestroyImmediate(_potionDefinition);
            Object.DestroyImmediate(_weaponDefinition);
            Object.DestroyImmediate(_armorDefinition);
        }

        [UnityTest]
        public IEnumerator EquipWeaponRequested_NotOwned_DoesNotEquip()
        {
            var stats = new CharacterStats(2, 30, 10, 8, 4, 5, 2);
            var session = new PlayerSessionState(
                SceneId.Village, stats, 0, 30, 10,
                new MainQuestProgress(), new QuestProgress(), new QuestProgress());
            yield return BuildSceneWithInventoryPanel(session);

            InvokeEquipWeaponRequested(_inventoryPanelObject.GetComponent<InventoryPanelController>());

            Assert.IsNull(session.Equipment.EquippedWeaponId);

            Object.DestroyImmediate(_inventoryPanelObject);
            Object.DestroyImmediate(_potionDefinition);
            Object.DestroyImmediate(_weaponDefinition);
            Object.DestroyImmediate(_armorDefinition);
        }

        [UnityTest]
        public IEnumerator Start_WithSessionAndPanel_RefreshesStatusTextWithCounts()
        {
            var stats = new CharacterStats(2, 30, 10, 8, 4, 5, 2);
            var session = new PlayerSessionState(
                SceneId.Village, stats, 0, 30, 10,
                new MainQuestProgress(), new QuestProgress(), new QuestProgress());
            session.Inventory.Add("item_small_potion", 2);
            yield return BuildSceneWithInventoryPanel(session);

            var statusText = _inventoryPanelObject.GetComponentInChildren<Text>();
            StringAssert.Contains("Small Potion x2", statusText.text);
            StringAssert.Contains("Weapon: None", statusText.text);

            Object.DestroyImmediate(_inventoryPanelObject);
            Object.DestroyImmediate(_potionDefinition);
            Object.DestroyImmediate(_weaponDefinition);
            Object.DestroyImmediate(_armorDefinition);
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
