using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using FloatingIslandsRpg.Application.Battle;
using FloatingIslandsRpg.Application.Scenes;
using FloatingIslandsRpg.Composition;
using FloatingIslandsRpg.Composition.Scenes;
using FloatingIslandsRpg.Domain.Quests;
using FloatingIslandsRpg.Infrastructure.MasterData;
using FloatingIslandsRpg.Presentation.Battle;
using FloatingIslandsRpg.Presentation.Dialogue;
using FloatingIslandsRpg.Presentation.Encounters;
using FloatingIslandsRpg.Presentation.Items;
using FloatingIslandsRpg.Presentation.Menu;
using FloatingIslandsRpg.Presentation.Player;
using FloatingIslandsRpg.Presentation.Results;
using FloatingIslandsRpg.Presentation.Scenes;
using FloatingIslandsRpg.Presentation.Title;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace FloatingIslandsRpg.Tests.PlayMode.EndToEnd
{
    // T-027 (通し結線・E2E確認): drives the REAL committed Scene assets (Title/Village/Field/
    // Dungeon/Battle/GameClear) through the full playthrough loop using only the same public
    // entry points a player's input would trigger (button onClick, NpcInteractable.RequestStart,
    // and -- for physics-driven triggers with no public API, exactly like every other PlayMode
    // test in this suite already does -- reflection into the trigger's own event). Every other
    // PlayMode test in this repo builds a synthetic scene via a BuildScene() fixture; this is the
    // one place that loads the actual .unity assets, so it is the only automated check that the
    // real committed wiring (Inspector references saved into Village.unity/Field.unity/etc.) is
    // correct end to end. It substitutes for an interactive Unity Editor Play Mode session
    // (Unity MCP/GUI automation was unavailable in this environment) and is deterministic and
    // repeatable where a manual session would not be.
    //
    // Battle outcomes are forced via BattleParticipantState.ApplyDamage (public) rather than
    // relying on real combat RNG to happen to produce a win or a loss: this keeps every other
    // step of battle resolution (ExecuteTurn, CombatCalculator, BattleEnded, reward grant, quest
    // advance, scene transition) 100% real production code, while making the test deterministic
    // regardless of the MasterData balance numbers configured in Battle.unity.
    [TestFixture]
    public class FullPlaythroughE2ETests
    {
        private string _saveDirectory;
        private GameServices _services;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            foreach (var leaked in UnityEngine.Object.FindObjectsByType<GameCompositionRoot>(FindObjectsSortMode.None))
            {
                UnityEngine.Object.DestroyImmediate(leaked.gameObject);
            }

            _saveDirectory = Path.Combine(Path.GetTempPath(), "fir-t027-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_saveDirectory);
            _services = new GameServices(_saveDirectory);

            // Created and DontDestroyOnLoad'd before Title.unity loads, so every installer's
            // Awake() -> GameCompositionRootLocator.EnsureRoot() finds this one (real
            // SceneTransitionUseCase/UnitySceneLoader, real JsonSaveRepository, but pointed at an
            // isolated temp directory instead of the real Application.persistentDataPath) rather
            // than creating its own default-path instance.
            var rootObject = new GameObject(nameof(GameCompositionRoot));
            rootObject.SetActive(false);
            var root = rootObject.AddComponent<GameCompositionRoot>();
            SetPrivateField(root, "<Services>k__BackingField", _services);
            rootObject.SetActive(true);
            yield return null;

            yield return SceneManager.LoadSceneAsync(SceneNameCatalog.GetName(SceneId.Title), LoadSceneMode.Single);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (var root in UnityEngine.Object.FindObjectsByType<GameCompositionRoot>(FindObjectsSortMode.None))
            {
                UnityEngine.Object.DestroyImmediate(root.gameObject);
            }

            var scratch = SceneManager.CreateScene("T027Scratch");
            SceneManager.SetActiveScene(scratch);

            for (var i = SceneManager.sceneCount - 1; i >= 0; i--)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene != scratch && scene.isLoaded)
                {
                    yield return SceneManager.UnloadSceneAsync(scene);
                }
            }

            if (_saveDirectory != null && Directory.Exists(_saveDirectory))
            {
                Directory.Delete(_saveDirectory, recursive: true);
            }
        }

        [UnityTest]
        public IEnumerator FullPlaythrough_TitleToContinue_CompletesWithoutErrors()
        {
            // 1. Title起動
            var titleController = UnityEngine.Object.FindFirstObjectByType<TitleScreenController>();
            Assert.IsNotNull(titleController, "TitleScreenController not found in Title.unity.");
            AssertNoPresentationDuplication();

            // 2. New Game
            GetPrivateField<Button>(titleController, "_newGameButton").onClick.Invoke();
            yield return WaitUntilSceneLoaded(SceneId.Village);
            Assert.IsNotNull(_services.CurrentSession);
            Assert.AreEqual(1, _services.CurrentSession.Stats.Level);
            AssertNoPresentationDuplication();

            // 3. Villageでメインクエスト開始
            var villageInstaller = UnityEngine.Object.FindFirstObjectByType<VillageSceneInstaller>();
            Assert.IsNotNull(villageInstaller);
            var mainQuestGiver = GetPrivateField<NpcInteractable>(villageInstaller, "_mainQuestGiver");
            Assert.IsTrue(mainQuestGiver.RequestStart());
            yield return null;
            Assert.AreEqual(MainQuestStage.ExploreField, _services.CurrentSession.MainQuest.CurrentStage);
            yield return CloseDialogue(mainQuestGiver);

            // 4. サブクエスト2件を独立して受注 (順序を問わず、互いにもMainQuestにも影響しない)
            var subQuest1Giver = GetPrivateField<NpcInteractable>(villageInstaller, "_subQuest1Giver");
            Assert.IsTrue(subQuest1Giver.RequestStart());
            yield return null;
            Assert.AreEqual(QuestState.InProgress, _services.CurrentSession.SubQuest1.CurrentState);
            Assert.AreEqual(QuestState.NotStarted, _services.CurrentSession.SubQuest2.CurrentState);
            yield return CloseDialogue(subQuest1Giver);

            var subQuest2Giver = GetPrivateField<NpcInteractable>(villageInstaller, "_subQuest2Giver");
            Assert.IsTrue(subQuest2Giver.RequestStart());
            yield return null;
            Assert.AreEqual(QuestState.InProgress, _services.CurrentSession.SubQuest2.CurrentState);
            Assert.AreEqual(QuestState.InProgress, _services.CurrentSession.SubQuest1.CurrentState);
            Assert.AreEqual(MainQuestStage.ExploreField, _services.CurrentSession.MainQuest.CurrentStage);
            yield return CloseDialogue(subQuest2Giver);

            // 10. メニュー開閉 (Village) -- MenuActivityGate pause/resume of NPC interactables here
            yield return OpenAndCloseMenu();

            // 5. Field到達
            yield return TransitionVia(SceneId.Field);
            Assert.AreEqual(MainQuestStage.EnterDungeon, _services.CurrentSession.MainQuest.CurrentStage);
            Assert.AreEqual(QuestState.Completed, _services.CurrentSession.SubQuest1.CurrentState);
            Assert.AreEqual(QuestState.InProgress, _services.CurrentSession.SubQuest2.CurrentState);
            AssertNoPresentationDuplication();

            var fieldInstaller = UnityEngine.Object.FindFirstObjectByType<FieldSceneInstaller>();
            var pickupEquipment = GetPrivateField<EquipmentDefinition>(fieldInstaller, "_pickupEquipment");
            var pickupWeaponId = pickupEquipment != null ? pickupEquipment.ToMasterData().Id : null;

            // 6. 通常戦闘 (Field) / 7. 経験値とレベルアップ
            var expBefore = _services.CurrentSession.TotalExperience;
            RaiseEvent(UnityEngine.Object.FindFirstObjectByType<FieldEncounterController>(), "EncounterTriggered");
            yield return ResolveBattle(forceVictory: true, isBoss: false);
            Assert.Greater(_services.CurrentSession.TotalExperience, expBefore);
            yield return AssertGateResumed();

            // 8. Item取得 (Field) -- weapon pickup, one-time only
            var pickup = UnityEngine.Object.FindFirstObjectByType<ItemPickupTrigger>();
            Assert.IsNotNull(pickup);
            RaiseEvent(pickup, "ItemPickupTriggered", pickup);
            yield return null;
            var qtyAfterFirstPickup = pickupWeaponId != null ? _services.CurrentSession.Inventory.GetQuantity(pickupWeaponId) : 0;
            RaiseEvent(pickup, "ItemPickupTriggered", pickup); // duplicate trigger must not double-grant
            yield return null;
            if (pickupWeaponId != null)
            {
                Assert.AreEqual(qtyAfterFirstPickup, _services.CurrentSession.Inventory.GetQuantity(pickupWeaponId));
            }

            // Back to Village to use the Menu (Potion/Equip) mid-playthrough
            yield return TransitionVia(SceneId.Village);
            yield return OpenAndCloseMenu();

            // 8 (続き). Potion使用 / 9. Weapon・Armorの装備
            yield return UsePotionAndEquipWeapon(pickupWeaponId);

            // Mid-game Save (T-027 note: no in-game Save button exists yet -- PROJECT.md T-024
            // "既知の問題" -- so, exactly like every prior manual QA session recorded in
            // PROJECT.md, Save is invoked directly via the real SaveGameUseCase here).
            var savedLevel = _services.CurrentSession.Stats.Level;
            var savedExperience = _services.CurrentSession.TotalExperience;
            var savedMainQuestStage = _services.CurrentSession.MainQuest.CurrentStage;
            var savedSubQuest1State = _services.CurrentSession.SubQuest1.CurrentState;
            var savedSubQuest2State = _services.CurrentSession.SubQuest2.CurrentState;
            var savedWeaponId = _services.CurrentSession.Equipment.EquippedWeaponId;
            var savedWeaponQty = pickupWeaponId != null ? _services.CurrentSession.Inventory.GetQuantity(pickupWeaponId) : 0;
            var saveResult = _services.SaveGameUseCase.Save(_services.CurrentSession);
            Assert.IsTrue(saveResult.Success);

            // 11. Dungeon到達 (Village -> Field -> Dungeon)
            yield return TransitionVia(SceneId.Field);
            yield return TransitionVia(SceneId.Dungeon);
            Assert.AreEqual(MainQuestStage.DefeatBoss, _services.CurrentSession.MainQuest.CurrentStage);
            Assert.AreEqual(QuestState.Completed, _services.CurrentSession.SubQuest2.CurrentState);
            AssertNoPresentationDuplication();

            // 12. 通常戦闘 (Dungeon)
            RaiseEvent(UnityEngine.Object.FindFirstObjectByType<FieldEncounterController>(), "EncounterTriggered");
            yield return ResolveBattle(forceVictory: true, isBoss: false);
            yield return AssertGateResumed();

            // 13. Boss戦 (最初は敗北させる) / 14. 敗北からRetry
            RaiseEvent(UnityEngine.Object.FindFirstObjectByType<BossEncounterTrigger>(), "BossEncounterTriggered");
            yield return ResolveBattle(forceVictory: false, isBoss: true);
            var resultController = UnityEngine.Object.FindFirstObjectByType<GameResultScreenController>();
            Assert.IsNotNull(resultController);
            Assert.AreEqual(BattleOutcome.PlayerDefeat, resultController.ShownOutcome);
            AssertNoPresentationDuplication();
            // MainQuest must still be at DefeatBoss (a loss must not complete it) and both
            // subquests must remain exactly as they were (a battle defeat/GameClear round trip
            // must not touch subquest state).
            Assert.AreEqual(MainQuestStage.DefeatBoss, _services.CurrentSession.MainQuest.CurrentStage);
            Assert.AreEqual(QuestState.Completed, _services.CurrentSession.SubQuest1.CurrentState);
            Assert.AreEqual(QuestState.Completed, _services.CurrentSession.SubQuest2.CurrentState);

            GetPrivateField<Button>(resultController, "_retryButton").onClick.Invoke();

            // 15. Boss再戦 (今度は勝利させる) / 16. MainQuest完了 / 17. GameClear
            yield return ResolveBattle(forceVictory: true, isBoss: true);
            resultController = UnityEngine.Object.FindFirstObjectByType<GameResultScreenController>();
            Assert.IsNotNull(resultController);
            Assert.AreEqual(BattleOutcome.PlayerVictory, resultController.ShownOutcome);
            Assert.AreEqual(MainQuestStage.Completed, _services.CurrentSession.MainQuest.CurrentStage);
            AssertNoPresentationDuplication();

            // 18. サブクエスト進行と完了状態の確認 / 19. Scene往復後の状態維持
            Assert.AreEqual(QuestState.Completed, _services.CurrentSession.SubQuest1.CurrentState);
            Assert.AreEqual(QuestState.Completed, _services.CurrentSession.SubQuest2.CurrentState);
            Assert.Greater(_services.CurrentSession.Stats.Level, 1);
            if (pickupWeaponId != null)
            {
                Assert.AreEqual(pickupWeaponId, _services.CurrentSession.Equipment.EquippedWeaponId);
            }

            // 20. Title復帰
            GetPrivateField<Button>(resultController, "_titleButton").onClick.Invoke();
            yield return WaitUntilSceneLoaded(SceneId.Title);
            AssertNoPresentationDuplication();

            // 21. Continue (loads the mid-playthrough save taken above, not the post-GameClear
            // in-memory state -- exercising the real disk round trip end to end)
            titleController = UnityEngine.Object.FindFirstObjectByType<TitleScreenController>();
            Assert.IsTrue(titleController.IsContinueAvailable, "Continue was not offered after a successful mid-game Save.");
            GetPrivateField<Button>(titleController, "_continueButton").onClick.Invoke();
            yield return WaitUntilSceneLoaded(SceneId.Village);

            // 22. Quest、Level、Experience、Inventory、Equipmentの復元
            Assert.AreEqual(savedLevel, _services.CurrentSession.Stats.Level);
            Assert.AreEqual(savedExperience, _services.CurrentSession.TotalExperience);
            Assert.AreEqual(savedMainQuestStage, _services.CurrentSession.MainQuest.CurrentStage);
            Assert.AreEqual(savedSubQuest1State, _services.CurrentSession.SubQuest1.CurrentState);
            Assert.AreEqual(savedSubQuest2State, _services.CurrentSession.SubQuest2.CurrentState);
            Assert.AreEqual(savedWeaponId, _services.CurrentSession.Equipment.EquippedWeaponId);
            if (pickupWeaponId != null)
            {
                Assert.AreEqual(savedWeaponQty, _services.CurrentSession.Inventory.GetQuantity(pickupWeaponId));
            }

            AssertNoPresentationDuplication();
        }

        private IEnumerator ResolveBattle(bool forceVictory, bool isBoss)
        {
            yield return WaitUntilSceneLoaded(SceneId.Battle);

            BattleUIController controller = null;
            var deadline = Time.realtimeSinceStartup + WaitTimeoutSeconds;
            while (Time.realtimeSinceStartup < deadline)
            {
                controller = UnityEngine.Object.FindFirstObjectByType<BattleUIController>();
                if (controller != null && GetPrivateField<object>(controller, "_session") != null)
                {
                    break;
                }

                yield return null;
            }

            Assert.IsNotNull(controller, "BattleUIController did not become ready in time.");

            var session = GetPrivateField<BattleSession>(controller, "_session");
            if (forceVictory)
            {
                session.Enemy.ApplyDamage(session.Enemy.CurrentHp - 1);
            }
            else
            {
                session.Player.ApplyDamage(session.Player.CurrentHp - 1);
            }

            var attackButton = GetPrivateField<Button>(controller, "_attackButton");
            var turns = 0;
            while (controller.CurrentOutcome == BattleOutcome.InProgress && turns < 30)
            {
                attackButton.onClick.Invoke();
                yield return null;
                turns++;
            }

            Assert.AreNotEqual(BattleOutcome.InProgress, controller.CurrentOutcome, "Battle did not resolve within the expected number of turns.");
            var expectedOutcome = forceVictory ? BattleOutcome.PlayerVictory : BattleOutcome.PlayerDefeat;
            Assert.AreEqual(expectedOutcome, controller.CurrentOutcome);

            if (forceVictory && !isBoss)
            {
                yield return WaitUntilSceneUnloaded(SceneId.Battle);
            }
            else
            {
                yield return WaitUntilSceneLoaded(SceneId.GameClear);
            }
        }

        private IEnumerator OpenAndCloseMenu()
        {
            var menu = UnityEngine.Object.FindFirstObjectByType<GameMenuController>();
            Assert.IsNotNull(menu, "GameMenuController not found in the current scene.");
            var playerMovement = UnityEngine.Object.FindFirstObjectByType<PlayerMovement>();

            GetPrivateField<Button>(menu, "_openButton").onClick.Invoke();
            yield return null;
            Assert.IsTrue(menu.IsOpen);
            if (playerMovement != null)
            {
                Assert.IsFalse(playerMovement.enabled, "PlayerMovement must be paused while the menu is open.");
            }

            GetPrivateField<Button>(menu, "_closeButton").onClick.Invoke();
            yield return null;
            Assert.IsFalse(menu.IsOpen);
            if (playerMovement != null)
            {
                Assert.IsTrue(playerMovement.enabled, "PlayerMovement must resume once the menu is closed.");
            }
        }

        private IEnumerator UsePotionAndEquipWeapon(string weaponId)
        {
            var menu = UnityEngine.Object.FindFirstObjectByType<GameMenuController>();
            var menuInstaller = UnityEngine.Object.FindFirstObjectByType<MenuInstaller>();
            Assert.IsNotNull(menu);
            Assert.IsNotNull(menuInstaller);

            GetPrivateField<Button>(menu, "_openButton").onClick.Invoke();
            yield return null;

            var items = GetPrivateField<ItemDefinition[]>(menuInstaller, "_items");
            var itemRows = GetPrivateField<ItemRowView[]>(menu, "_itemRows");
            var potionIndex = Array.FindIndex(items, i => i != null
                && i.ToMasterData().HealAmount > 0
                && _services.CurrentSession.Inventory.GetQuantity(i.ToMasterData().Id) > 0);

            if (potionIndex >= 0)
            {
                var hpBefore = _services.CurrentSession.CurrentHp;
                var qtyBefore = _services.CurrentSession.Inventory.GetQuantity(items[potionIndex].ToMasterData().Id);
                itemRows[potionIndex].UseButton.onClick.Invoke();
                yield return null;
                Assert.AreEqual(qtyBefore - 1, _services.CurrentSession.Inventory.GetQuantity(items[potionIndex].ToMasterData().Id));
                Assert.GreaterOrEqual(_services.CurrentSession.CurrentHp, hpBefore);
            }

            if (weaponId != null)
            {
                var weapons = GetPrivateField<EquipmentDefinition[]>(menuInstaller, "_weapons");
                var weaponRows = GetPrivateField<EquipmentRowView[]>(menu, "_weaponRows");
                var weaponIndex = Array.FindIndex(weapons, w => w != null && w.ToMasterData().Id == weaponId);

                if (weaponIndex >= 0)
                {
                    var atkBefore = _services.CurrentSession.Stats.Attack;
                    weaponRows[weaponIndex].EquipButton.onClick.Invoke();
                    yield return null;
                    Assert.AreEqual(weaponId, _services.CurrentSession.Equipment.EquippedWeaponId);
                }
            }

            GetPrivateField<Button>(menu, "_closeButton").onClick.Invoke();
            yield return null;
        }

        private IEnumerator TransitionVia(SceneId destination)
        {
            var triggers = UnityEngine.Object.FindObjectsByType<SceneTransitionTrigger>(FindObjectsSortMode.None);
            SceneTransitionTrigger match = null;
            var loadMode = SceneLoadMode.Single;

            foreach (var trigger in triggers)
            {
                var dest = GetPrivateField<SceneId>(trigger, "_destinationSceneId");
                if (dest == destination)
                {
                    match = trigger;
                    loadMode = GetPrivateField<SceneLoadMode>(trigger, "_loadMode");
                    break;
                }
            }

            Assert.IsNotNull(match, $"No SceneTransitionTrigger targeting {destination} found in the current scene.");
            RaiseEvent(match, "TransitionRequested", match, destination, loadMode);
            yield return WaitUntilSceneLoaded(destination);
        }

        private static IEnumerator CloseDialogue(NpcInteractable npc)
        {
            var view = GetPrivateField<DialogueBoxView>(npc, "_dialogueBoxView");
            var guard = 0;
            while (view != null && view.IsOpen && guard < 10)
            {
                view.Advance();
                guard++;
            }

            yield return null;
        }

        private static IEnumerator AssertGateResumed()
        {
            yield return null;
            var playerMovement = UnityEngine.Object.FindFirstObjectByType<PlayerMovement>();
            if (playerMovement != null)
            {
                Assert.IsTrue(playerMovement.enabled, "PlayerMovement must resume after returning from a regular battle.");
            }

            AssertNoPresentationDuplication();
        }

        // Wall-clock (not frame-count) budgeted: batch-mode frame throughput varies a lot with
        // how much else the Editor/test run is doing concurrently (e.g. this test running late
        // in a large full-suite pass vs. alone), so a fixed frame cap is not a reliable proxy for
        // "the async scene load had enough real time to finish".
        private const float WaitTimeoutSeconds = 30f;

        private static IEnumerator WaitUntilSceneLoaded(SceneId id)
        {
            var name = SceneNameCatalog.GetName(id);
            var deadline = Time.realtimeSinceStartup + WaitTimeoutSeconds;
            while (!SceneManager.GetSceneByName(name).isLoaded && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.IsTrue(SceneManager.GetSceneByName(name).isLoaded, $"Scene '{name}' did not finish loading in time.");
            yield return null;
        }

        private static IEnumerator WaitUntilSceneUnloaded(SceneId id)
        {
            var name = SceneNameCatalog.GetName(id);
            var deadline = Time.realtimeSinceStartup + WaitTimeoutSeconds;
            while (SceneManager.GetSceneByName(name).isLoaded && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.IsFalse(SceneManager.GetSceneByName(name).isLoaded, $"Scene '{name}' did not unload in time.");
        }

        // 重点確認: Canvas/EventSystem/Camera/GameMenuの重複禁止。Canvas自体は複数存在してよい
        // 設計(Dialogue/Menu/Battle UI等が別Canvasを持つ)ため、意味のある重複禁止対象である
        // Camera/AudioListener(有効な個数)、EventSystem(有効な個数)、GameMenuControllerの
        // インスタンス数のみを検証する。
        private static void AssertNoPresentationDuplication()
        {
            var enabledCameras = UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).Count(c => c.isActiveAndEnabled);
            Assert.AreEqual(1, enabledCameras, "Exactly one Camera should be enabled at a time.");

            var enabledListeners = UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None)
                .Count(l => l.enabled && l.gameObject.activeInHierarchy);
            Assert.LessOrEqual(enabledListeners, 1, "At most one AudioListener should be enabled at a time.");

            var activeEventSystems = UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Count(e => e.isActiveAndEnabled);
            Assert.AreEqual(1, activeEventSystems, "Exactly one EventSystem should be active at a time.");

            var menus = UnityEngine.Object.FindObjectsByType<GameMenuController>(FindObjectsSortMode.None);
            Assert.LessOrEqual(menus.Length, 1, "At most one GameMenuController should exist at a time.");
        }

        private static T GetPrivateField<T>(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"Field '{fieldName}' not found on {target.GetType().Name}.");
            return (T)field.GetValue(target);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"Field '{fieldName}' not found on {target.GetType().Name}.");
            field.SetValue(target, value);
        }

        private static void RaiseEvent(object target, string eventName, params object[] args)
        {
            Assert.IsNotNull(target, $"Cannot raise '{eventName}' on a null target.");
            var field = target.GetType().GetField(eventName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"Event '{eventName}' not found on {target.GetType().Name}.");
            var del = (Delegate)field.GetValue(target);
            del?.DynamicInvoke(args);
        }
    }
}
