using System.Collections.Generic;
using FloatingIslandsRpg.Application.Inventory;
using FloatingIslandsRpg.Application.Progression;
using FloatingIslandsRpg.Application.Quests;
using FloatingIslandsRpg.Application.Save;
using FloatingIslandsRpg.Application.Scenes;
using FloatingIslandsRpg.Application.Session;
using FloatingIslandsRpg.Domain.Characters.Stats;
using FloatingIslandsRpg.Domain.Inventory;
using FloatingIslandsRpg.Domain.MasterData;
using FloatingIslandsRpg.Domain.Progression;
using FloatingIslandsRpg.Domain.Quests;
using NUnit.Framework;

namespace FloatingIslandsRpg.Tests.EditMode.EndToEnd
{
    // T-027 (通し結線・E2E確認): cross-cutting Application-layer checks that no single T-016〜T-026
    // unit test suite exercises together -- the full Main+Sub quest chain running side by side,
    // one-time-reward gating exactly as Field/Battle installers use it, and a full SaveVersion 3
    // round trip of every piece of state the checklist calls out (quest/level/experience/
    // inventory/equipment). Pure Domain/Application: no MonoBehaviour, no Scene, no MasterData
    // ScriptableObject assets.
    public class FullFlowIntegrationTests
    {
        private static StatGrowthProfile CreateGrowthProfile()
        {
            return new StatGrowthProfile(
                minLevel: 1, maxLevel: 10,
                baseMaxHp: 20, baseMaxMp: 5, baseAttack: 5, baseDefense: 3, baseAgility: 5, baseMagic: 2,
                growthMaxHp: 5, growthMaxMp: 1, growthAttack: 2, growthDefense: 1, growthAgility: 1, growthMagic: 1);
        }

        private static ExperienceTable CreateExperienceTable()
        {
            return new ExperienceTable(new[] { 0, 10, 25, 45, 70, 100, 140, 190, 250, 320 });
        }

        private static PlayerSessionState CreateNewGameSession()
        {
            var stats = CharacterStatsCalculator.Calculate(CreateGrowthProfile(), level: 1);
            return new PlayerSessionState(
                SceneId.Village, stats, totalExperience: 0, currentHp: stats.MaxHp, currentMp: stats.MaxMp,
                mainQuest: new MainQuestProgress(), subQuest1: new QuestProgress(), subQuest2: new QuestProgress());
        }

        [Test]
        public void FullQuestChain_MainAndSubQuestsAdvance_WithoutCrossContamination()
        {
            var session = CreateNewGameSession();
            var startMainQuest = new StartMainQuestUseCase();
            var advanceMainQuest = new AdvanceMainQuestUseCase();
            var startSubQuest = new StartSubQuestUseCase();
            var completeSubQuest = new CompleteSubQuestUseCase();

            Assert.AreEqual(MainQuestAdvanceResult.Advanced, startMainQuest.Execute(session.MainQuest));
            Assert.AreEqual(MainQuestStage.ExploreField, session.MainQuest.CurrentStage);
            Assert.AreEqual(QuestState.NotStarted, session.SubQuest1.CurrentState);
            Assert.AreEqual(QuestState.NotStarted, session.SubQuest2.CurrentState);

            Assert.AreEqual(SubQuestAdvanceResult.Advanced, startSubQuest.Execute(session.SubQuest1));
            Assert.AreEqual(QuestState.InProgress, session.SubQuest1.CurrentState);
            Assert.AreEqual(MainQuestStage.ExploreField, session.MainQuest.CurrentStage);
            Assert.AreEqual(QuestState.NotStarted, session.SubQuest2.CurrentState);

            Assert.AreEqual(SubQuestAdvanceResult.Advanced, startSubQuest.Execute(session.SubQuest2));
            Assert.AreEqual(QuestState.InProgress, session.SubQuest2.CurrentState);
            Assert.AreEqual(MainQuestStage.ExploreField, session.MainQuest.CurrentStage);
            Assert.AreEqual(QuestState.InProgress, session.SubQuest1.CurrentState);

            // Field reached: SubQuest1 completes, MainQuest advances. Neither touches SubQuest2.
            Assert.AreEqual(SubQuestAdvanceResult.Advanced, completeSubQuest.Execute(session.SubQuest1));
            Assert.AreEqual(MainQuestAdvanceResult.Advanced, advanceMainQuest.Execute(session.MainQuest, MainQuestEvent.FieldReached));
            Assert.AreEqual(QuestState.Completed, session.SubQuest1.CurrentState);
            Assert.AreEqual(MainQuestStage.EnterDungeon, session.MainQuest.CurrentStage);
            Assert.AreEqual(QuestState.InProgress, session.SubQuest2.CurrentState);

            // Dungeon reached: SubQuest2 completes, MainQuest advances. SubQuest1 stays Completed.
            Assert.AreEqual(SubQuestAdvanceResult.Advanced, completeSubQuest.Execute(session.SubQuest2));
            Assert.AreEqual(MainQuestAdvanceResult.Advanced, advanceMainQuest.Execute(session.MainQuest, MainQuestEvent.DungeonReached));
            Assert.AreEqual(QuestState.Completed, session.SubQuest2.CurrentState);
            Assert.AreEqual(MainQuestStage.DefeatBoss, session.MainQuest.CurrentStage);
            Assert.AreEqual(QuestState.Completed, session.SubQuest1.CurrentState);

            // Boss defeated: only MainQuest changes.
            Assert.AreEqual(MainQuestAdvanceResult.Advanced, advanceMainQuest.Execute(session.MainQuest, MainQuestEvent.BossDefeated));
            Assert.AreEqual(MainQuestStage.Completed, session.MainQuest.CurrentStage);
            Assert.AreEqual(QuestState.Completed, session.SubQuest1.CurrentState);
            Assert.AreEqual(QuestState.Completed, session.SubQuest2.CurrentState);
        }

        [Test]
        public void SubQuestsCanReachCompleted_WhileMainQuestNeverStarted()
        {
            // PROJECT.md T-025: subquests must be startable/completable fully independent of
            // whether the main quest has been touched at all.
            var session = CreateNewGameSession();
            var startSubQuest = new StartSubQuestUseCase();
            var completeSubQuest = new CompleteSubQuestUseCase();

            startSubQuest.Execute(session.SubQuest1);
            completeSubQuest.Execute(session.SubQuest1);
            startSubQuest.Execute(session.SubQuest2);
            completeSubQuest.Execute(session.SubQuest2);

            Assert.AreEqual(QuestState.Completed, session.SubQuest1.CurrentState);
            Assert.AreEqual(QuestState.Completed, session.SubQuest2.CurrentState);
            Assert.AreEqual(MainQuestStage.NotStarted, session.MainQuest.CurrentStage);
        }

        [Test]
        public void RepeatedQuestEvents_DoNotAdvanceOrRegressTwice()
        {
            var session = CreateNewGameSession();
            var advanceMainQuest = new AdvanceMainQuestUseCase();
            var startSubQuest = new StartSubQuestUseCase();
            var completeSubQuest = new CompleteSubQuestUseCase();

            new StartMainQuestUseCase().Execute(session.MainQuest);
            startSubQuest.Execute(session.SubQuest1);
            completeSubQuest.Execute(session.SubQuest1);

            // Re-completing an already-Completed subquest, re-starting an already-InProgress one,
            // and re-firing a MainQuestEvent the quest has already moved past must all be safe
            // no-ops (Rejected), matching FieldSceneInstaller/DungeonSceneInstaller being called
            // on every revisit regardless of quest state.
            Assert.AreEqual(SubQuestAdvanceResult.Rejected, completeSubQuest.Execute(session.SubQuest1));
            Assert.AreEqual(SubQuestAdvanceResult.Rejected, startSubQuest.Execute(session.SubQuest1));
            Assert.AreEqual(QuestState.Completed, session.SubQuest1.CurrentState);

            Assert.AreEqual(MainQuestAdvanceResult.Advanced, advanceMainQuest.Execute(session.MainQuest, MainQuestEvent.FieldReached));
            Assert.AreEqual(MainQuestAdvanceResult.Rejected, advanceMainQuest.Execute(session.MainQuest, MainQuestEvent.FieldReached));
            Assert.AreEqual(MainQuestStage.EnterDungeon, session.MainQuest.CurrentStage);
        }

        [Test]
        public void OneTimeRewardGate_PreventsDuplicateGrant_WhenTriggeredTwice()
        {
            // Mirrors the exact ClaimReward-then-AddItemUseCase pattern FieldSceneInstaller and
            // BattleSceneInstaller use to prevent a duplicate pickup/battle-victory reward
            // (PROJECT.md T-024/T-023), exercised here directly against Domain/Application only.
            var session = CreateNewGameSession();
            var addItem = new AddItemUseCase();
            var knownIds = new[] { "rusty_sword" };

            void GrantOnce(string rewardId)
            {
                if (session.ClaimReward(rewardId))
                {
                    addItem.Execute(session.Inventory, knownIds, "rusty_sword", 1);
                }
            }

            GrantOnce("field-pickup-1");
            GrantOnce("field-pickup-1");
            GrantOnce("field-pickup-1");

            Assert.AreEqual(1, session.Inventory.GetQuantity("rusty_sword"));
        }

        [Test]
        public void BattleRewardChain_LevelsUpAndFullyHealsExactlyOnce()
        {
            var session = CreateNewGameSession();
            var grantReward = new GrantBattleRewardUseCase();
            session.SetCurrentHp(1);

            var result = grantReward.Execute(session, CreateExperienceTable(), CreateGrowthProfile(), rewardExperience: 50);

            Assert.IsTrue(result.LeveledUp);
            Assert.Greater(result.NewLevel, 1);
            Assert.AreEqual(result.NewLevel, session.Stats.Level);
            Assert.AreEqual(50, session.TotalExperience);
            // ApplyStatGrowth's full-heal-on-level-up policy (PROJECT.md T-023) must have fired.
            Assert.AreEqual(session.Stats.MaxHp, session.CurrentHp);
            Assert.AreEqual(session.Stats.MaxMp, session.CurrentMp);
        }

        [Test]
        public void SaveLoadRoundTrip_FullyProgressedState_RestoresEveryField()
        {
            var session = CreateNewGameSession();
            var experienceTable = CreateExperienceTable();
            var growthProfile = CreateGrowthProfile();

            new StartMainQuestUseCase().Execute(session.MainQuest);
            new AdvanceMainQuestUseCase().Execute(session.MainQuest, MainQuestEvent.FieldReached);
            new StartSubQuestUseCase().Execute(session.SubQuest1);
            new CompleteSubQuestUseCase().Execute(session.SubQuest1);
            new StartSubQuestUseCase().Execute(session.SubQuest2);

            new GrantBattleRewardUseCase().Execute(session, experienceTable, growthProfile, rewardExperience: 25);

            var weapon = new EquipmentMasterData("rusty_sword", "Rusty Sword", EquipmentSlot.Weapon, attackBonus: 3, defenseBonus: 0);
            var armor = new EquipmentMasterData("traveler_armor", "Traveler Armor", EquipmentSlot.Armor, attackBonus: 0, defenseBonus: 2);
            var equipmentCatalog = new Dictionary<string, EquipmentMasterData>
            {
                [weapon.Id] = weapon,
                [armor.Id] = armor
            };

            var addItem = new AddItemUseCase();
            addItem.Execute(session.Inventory, new[] { weapon.Id, armor.Id }, weapon.Id, 1);
            addItem.Execute(session.Inventory, new[] { weapon.Id, armor.Id }, armor.Id, 1);

            var smallPotion = new ItemMasterData("small_potion", "Small Potion", healAmount: 20);
            var itemCatalog = new Dictionary<string, ItemMasterData> { [smallPotion.Id] = smallPotion };
            addItem.Execute(session.Inventory, new[] { smallPotion.Id }, smallPotion.Id, 3);

            new EquipItemUseCase().Execute(session.Equipment, session.Inventory, equipmentCatalog, weapon.Id, EquipmentSlot.Weapon);
            new EquipItemUseCase().Execute(session.Equipment, session.Inventory, equipmentCatalog, armor.Id, EquipmentSlot.Armor);

            session.SetCurrentHp(1);
            new ConsumeItemUseCase().Execute(session.Inventory, session, itemCatalog, smallPotion.Id);

            session.ClaimReward("field-pickup-1");
            session.ClaimReward("battle-victory-1");

            var snapshot = PlayerSessionStateMapper.ToSnapshot(session);
            var restored = PlayerSessionStateMapper.FromSnapshot(snapshot, experienceTable, equipmentCatalog);

            Assert.AreEqual(session.Stats.Level, restored.Stats.Level);
            Assert.AreEqual(session.TotalExperience, restored.TotalExperience);
            Assert.AreEqual(session.CurrentHp, restored.CurrentHp);
            Assert.AreEqual(session.CurrentMp, restored.CurrentMp);
            Assert.AreEqual(session.MainQuest.CurrentStage, restored.MainQuest.CurrentStage);
            Assert.AreEqual(session.SubQuest1.CurrentState, restored.SubQuest1.CurrentState);
            Assert.AreEqual(session.SubQuest2.CurrentState, restored.SubQuest2.CurrentState);
            Assert.AreEqual(session.Inventory.GetQuantity(weapon.Id), restored.Inventory.GetQuantity(weapon.Id));
            Assert.AreEqual(session.Inventory.GetQuantity(armor.Id), restored.Inventory.GetQuantity(armor.Id));
            Assert.AreEqual(session.Inventory.GetQuantity(smallPotion.Id), restored.Inventory.GetQuantity(smallPotion.Id));
            Assert.AreEqual(2, restored.Inventory.GetQuantity(smallPotion.Id));
            Assert.AreEqual(session.Equipment.EquippedWeaponId, restored.Equipment.EquippedWeaponId);
            Assert.AreEqual(session.Equipment.EquippedArmorId, restored.Equipment.EquippedArmorId);
            CollectionAssert.AreEquivalent(session.ClaimedRewardIds, restored.ClaimedRewardIds);
        }
    }
}
