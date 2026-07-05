using System;
using System.Collections.Generic;
using FloatingIslandsRpg.Application.Save;
using FloatingIslandsRpg.Application.Scenes;
using FloatingIslandsRpg.Application.Session;
using FloatingIslandsRpg.Domain.Characters.Stats;
using FloatingIslandsRpg.Domain.MasterData;
using FloatingIslandsRpg.Domain.Progression;
using FloatingIslandsRpg.Domain.Quests;
using NUnit.Framework;

namespace FloatingIslandsRpg.Tests.EditMode.Save
{
    public class PlayerSessionStateMapperTests
    {
        private static PlayerSessionState CreateState(SceneId sceneId, int totalExperience, int currentHp, int currentMp)
        {
            var stats = new CharacterStats(level: 3, maxHp: 50, maxMp: 20, attack: 15, defense: 8, agility: 12, magic: 6);
            return new PlayerSessionState(
                sceneId, stats, totalExperience, currentHp, currentMp,
                new MainQuestProgress(), new QuestProgress(), new QuestProgress());
        }

        [Test]
        public void ToSnapshot_MapsAllFieldsCorrectly()
        {
            // Arrange
            var state = CreateState(SceneId.Dungeon, totalExperience: 250, currentHp: 30, currentMp: 10);

            // Act
            var snapshot = PlayerSessionStateMapper.ToSnapshot(state);

            // Assert
            Assert.AreEqual(SaveGameSnapshot.CurrentSaveVersion, snapshot.SaveVersion);
            Assert.AreEqual(SceneId.Dungeon, snapshot.CurrentSceneId);
            Assert.AreEqual(3, snapshot.Level);
            Assert.AreEqual(50, snapshot.MaxHp);
            Assert.AreEqual(20, snapshot.MaxMp);
            Assert.AreEqual(15, snapshot.Attack);
            Assert.AreEqual(8, snapshot.Defense);
            Assert.AreEqual(12, snapshot.Agility);
            Assert.AreEqual(6, snapshot.Magic);
            Assert.AreEqual(250, snapshot.TotalExperience);
            Assert.AreEqual(30, snapshot.CurrentHp);
            Assert.AreEqual(10, snapshot.CurrentMp);
            Assert.AreEqual(MainQuestStage.NotStarted, snapshot.MainQuestStage);
        }

        [Test]
        public void ToSnapshot_NullState_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => PlayerSessionStateMapper.ToSnapshot(null));
        }

        [Test]
        public void FromSnapshot_NullSnapshot_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => PlayerSessionStateMapper.FromSnapshot(null));
        }

        [Test]
        public void FromSnapshot_UnsupportedVersion_ThrowsNotSupportedException()
        {
            // Arrange
            var snapshot = new SaveGameSnapshot
            {
                SaveVersion = SaveGameSnapshot.CurrentSaveVersion + 1,
                CurrentSceneId = SceneId.Village,
                Level = 1,
                MaxHp = 10,
                MaxMp = 5
            };

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => PlayerSessionStateMapper.FromSnapshot(snapshot));
        }

        [Test]
        public void FromSnapshot_InvalidMainQuestStage_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var snapshot = new SaveGameSnapshot
            {
                SaveVersion = SaveGameSnapshot.CurrentSaveVersion,
                CurrentSceneId = SceneId.Village,
                Level = 1,
                MaxHp = 10,
                MaxMp = 5,
                MainQuestStage = (MainQuestStage)999
            };

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => PlayerSessionStateMapper.FromSnapshot(snapshot));
        }

        [Test]
        public void FromSnapshot_InvalidSubQuestState_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var snapshot = new SaveGameSnapshot
            {
                SaveVersion = SaveGameSnapshot.CurrentSaveVersion,
                CurrentSceneId = SceneId.Village,
                Level = 1,
                MaxHp = 10,
                MaxMp = 5,
                SubQuest1State = (QuestState)999
            };

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => PlayerSessionStateMapper.FromSnapshot(snapshot));
        }

        [Test]
        public void FromSnapshot_SaveVersion1_MigratesNotStartedLegacyMainQuestState()
        {
            // Arrange: a genuine v1 save predates MainQuestStage entirely.
            var snapshot = new SaveGameSnapshot
            {
                SaveVersion = 1,
                CurrentSceneId = SceneId.Village,
                Level = 1,
                MaxHp = 10,
                MaxMp = 5,
                MainQuestState = QuestState.NotStarted
            };

            // Act
            var restored = PlayerSessionStateMapper.FromSnapshot(snapshot);

            // Assert
            Assert.AreEqual(MainQuestStage.NotStarted, restored.MainQuest.CurrentStage);
        }

        [Test]
        public void FromSnapshot_SaveVersion1_MigratesInProgressLegacyMainQuestStateToExploreField()
        {
            // Arrange: v1's 3-value InProgress cannot say which stage the player had reached,
            // so it must migrate to the earliest safe in-progress stage.
            var snapshot = new SaveGameSnapshot
            {
                SaveVersion = 1,
                CurrentSceneId = SceneId.Village,
                Level = 1,
                MaxHp = 10,
                MaxMp = 5,
                MainQuestState = QuestState.InProgress
            };

            // Act
            var restored = PlayerSessionStateMapper.FromSnapshot(snapshot);

            // Assert
            Assert.AreEqual(MainQuestStage.ExploreField, restored.MainQuest.CurrentStage);
        }

        [Test]
        public void FromSnapshot_SaveVersion1_MigratesCompletedLegacyMainQuestState()
        {
            // Arrange
            var snapshot = new SaveGameSnapshot
            {
                SaveVersion = 1,
                CurrentSceneId = SceneId.Village,
                Level = 1,
                MaxHp = 10,
                MaxMp = 5,
                MainQuestState = QuestState.Completed
            };

            // Act
            var restored = PlayerSessionStateMapper.FromSnapshot(snapshot);

            // Assert
            Assert.AreEqual(MainQuestStage.Completed, restored.MainQuest.CurrentStage);
        }

        [Test]
        public void FromSnapshot_SaveVersion1_InvalidLegacyMainQuestState_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var snapshot = new SaveGameSnapshot
            {
                SaveVersion = 1,
                CurrentSceneId = SceneId.Village,
                Level = 1,
                MaxHp = 10,
                MaxMp = 5,
                MainQuestState = (QuestState)999
            };

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => PlayerSessionStateMapper.FromSnapshot(snapshot));
        }

        [Test]
        public void RoundTrip_PreservesFullMainQuestProgression()
        {
            // Arrange
            var original = CreateState(SceneId.Dungeon, totalExperience: 0, currentHp: 50, currentMp: 20);
            original.MainQuest.Start();
            original.MainQuest.AdvanceToEnterDungeon();
            original.MainQuest.AdvanceToDefeatBoss();
            original.MainQuest.Complete();

            // Act
            var snapshot = PlayerSessionStateMapper.ToSnapshot(original);
            var restored = PlayerSessionStateMapper.FromSnapshot(snapshot);

            // Assert
            Assert.AreEqual(MainQuestStage.Completed, restored.MainQuest.CurrentStage);
        }

        [Test]
        public void RoundTrip_PreservesAllValues()
        {
            // Arrange
            var original = CreateState(SceneId.Dungeon, totalExperience: 250, currentHp: 30, currentMp: 10);

            // Act
            var snapshot = PlayerSessionStateMapper.ToSnapshot(original);
            var restored = PlayerSessionStateMapper.FromSnapshot(snapshot);

            // Assert
            Assert.AreEqual(original.CurrentSceneId, restored.CurrentSceneId);
            Assert.AreEqual(original.Stats.Level, restored.Stats.Level);
            Assert.AreEqual(original.Stats.MaxHp, restored.Stats.MaxHp);
            Assert.AreEqual(original.Stats.MaxMp, restored.Stats.MaxMp);
            Assert.AreEqual(original.Stats.Attack, restored.Stats.Attack);
            Assert.AreEqual(original.Stats.Defense, restored.Stats.Defense);
            Assert.AreEqual(original.Stats.Agility, restored.Stats.Agility);
            Assert.AreEqual(original.Stats.Magic, restored.Stats.Magic);
            Assert.AreEqual(original.TotalExperience, restored.TotalExperience);
            Assert.AreEqual(original.CurrentHp, restored.CurrentHp);
            Assert.AreEqual(original.CurrentMp, restored.CurrentMp);
            Assert.AreEqual(original.MainQuest.CurrentStage, restored.MainQuest.CurrentStage);
            Assert.AreEqual(original.SubQuest1.CurrentState, restored.SubQuest1.CurrentState);
            Assert.AreEqual(original.SubQuest2.CurrentState, restored.SubQuest2.CurrentState);
        }

        [Test]
        public void RoundTrip_PreservesInProgressAndCompletedQuestStates()
        {
            // Arrange
            var original = CreateState(SceneId.Field, totalExperience: 0, currentHp: 50, currentMp: 20);
            original.MainQuest.Start();
            original.SubQuest1.Start();
            original.SubQuest1.Complete();

            // Act
            var snapshot = PlayerSessionStateMapper.ToSnapshot(original);
            var restored = PlayerSessionStateMapper.FromSnapshot(snapshot);

            // Assert
            Assert.AreEqual(MainQuestStage.ExploreField, restored.MainQuest.CurrentStage);
            Assert.AreEqual(QuestState.Completed, restored.SubQuest1.CurrentState);
            Assert.AreEqual(QuestState.NotStarted, restored.SubQuest2.CurrentState);
        }

        [Test]
        public void RoundTrip_PreservesInventoryAndEquipment()
        {
            var original = CreateState(SceneId.Village, totalExperience: 0, currentHp: 50, currentMp: 20);
            original.Inventory.Add("item_small_potion", 3);
            original.Inventory.Add("equip_rusty_sword", 1);
            original.Equipment.EquipWeapon("equip_rusty_sword");
            original.Equipment.EquipArmor("equip_traveler_armor");
            original.ClaimReward("field_pickup_1");

            var snapshot = PlayerSessionStateMapper.ToSnapshot(original);
            var restored = PlayerSessionStateMapper.FromSnapshot(snapshot);

            Assert.AreEqual(3, restored.Inventory.GetQuantity("item_small_potion"));
            Assert.AreEqual(1, restored.Inventory.GetQuantity("equip_rusty_sword"));
            Assert.AreEqual("equip_rusty_sword", restored.Equipment.EquippedWeaponId);
            Assert.AreEqual("equip_traveler_armor", restored.Equipment.EquippedArmorId);
            Assert.IsTrue(restored.HasClaimedReward("field_pickup_1"));
        }

        [Test]
        public void FromSnapshot_SaveVersion2_DefaultsToEmptyInventoryAndEquipment()
        {
            var snapshot = new SaveGameSnapshot
            {
                SaveVersion = 2,
                CurrentSceneId = SceneId.Village,
                Level = 1,
                MaxHp = 10,
                MaxMp = 5
            };

            var restored = PlayerSessionStateMapper.FromSnapshot(snapshot);

            Assert.AreEqual(0, restored.Inventory.GetQuantity("item_small_potion"));
            Assert.IsNull(restored.Equipment.EquippedWeaponId);
            Assert.IsNull(restored.Equipment.EquippedArmorId);
            Assert.AreEqual(0, restored.ClaimedRewardIds.Count);
        }

        [Test]
        public void FromSnapshot_SaveVersion1_DefaultsToEmptyInventoryAndEquipment()
        {
            var snapshot = new SaveGameSnapshot
            {
                SaveVersion = 1,
                CurrentSceneId = SceneId.Village,
                Level = 1,
                MaxHp = 10,
                MaxMp = 5
            };

            var restored = PlayerSessionStateMapper.FromSnapshot(snapshot);

            Assert.AreEqual(0, restored.Inventory.GetQuantity("item_small_potion"));
            Assert.IsNull(restored.Equipment.EquippedWeaponId);
        }

        [Test]
        public void FromSnapshot_DuplicateItemIdInInventoryEntries_LastEntryWins()
        {
            var snapshot = new SaveGameSnapshot
            {
                SaveVersion = SaveGameSnapshot.CurrentSaveVersion,
                CurrentSceneId = SceneId.Village,
                Level = 1,
                MaxHp = 10,
                MaxMp = 5,
                InventoryEntries = new[]
                {
                    new InventoryEntrySnapshot { ItemId = "item_small_potion", Quantity = 1 },
                    new InventoryEntrySnapshot { ItemId = "item_small_potion", Quantity = 5 }
                }
            };

            var restored = PlayerSessionStateMapper.FromSnapshot(snapshot);

            Assert.AreEqual(5, restored.Inventory.GetQuantity("item_small_potion"));
        }

        [Test]
        public void FromSnapshot_InvalidInventoryQuantity_ThrowsArgumentOutOfRangeException()
        {
            var snapshot = new SaveGameSnapshot
            {
                SaveVersion = SaveGameSnapshot.CurrentSaveVersion,
                CurrentSceneId = SceneId.Village,
                Level = 1,
                MaxHp = 10,
                MaxMp = 5,
                InventoryEntries = new[]
                {
                    new InventoryEntrySnapshot { ItemId = "item_small_potion", Quantity = 0 }
                }
            };

            Assert.Throws<ArgumentOutOfRangeException>(() => PlayerSessionStateMapper.FromSnapshot(snapshot));
        }

        [Test]
        public void FromSnapshot_NullEquippedIds_RestoresUnequippedLoadout()
        {
            var snapshot = new SaveGameSnapshot
            {
                SaveVersion = SaveGameSnapshot.CurrentSaveVersion,
                CurrentSceneId = SceneId.Village,
                Level = 1,
                MaxHp = 10,
                MaxMp = 5,
                EquippedWeaponId = null,
                EquippedArmorId = null
            };

            var restored = PlayerSessionStateMapper.FromSnapshot(snapshot);

            Assert.IsNull(restored.Equipment.EquippedWeaponId);
            Assert.IsNull(restored.Equipment.EquippedArmorId);
        }

        // --- Codex review Major 3: SaveVersion 3 integrity validation ---

        private static ExperienceTable CreateExperienceTable()
        {
            return new ExperienceTable(new[] { 0, 10, 25, 45, 70 });
        }

        private static IReadOnlyDictionary<string, EquipmentMasterData> CreateEquipmentCatalog()
        {
            return new Dictionary<string, EquipmentMasterData>(StringComparer.Ordinal)
            {
                ["equip_test_sword"] = new EquipmentMasterData("equip_test_sword", "Test Sword", EquipmentSlot.Weapon, 5, 0),
                ["equip_test_armor"] = new EquipmentMasterData("equip_test_armor", "Test Armor", EquipmentSlot.Armor, 0, 5)
            };
        }

        private static SaveGameSnapshot CreateSaveVersion3Snapshot(int level, int totalExperience)
        {
            return new SaveGameSnapshot
            {
                SaveVersion = SaveGameSnapshot.CurrentSaveVersion,
                CurrentSceneId = SceneId.Village,
                Level = level,
                MaxHp = 10,
                MaxMp = 5,
                TotalExperience = totalExperience
            };
        }

        [Test]
        public void FromSnapshot_SaveVersion3_ValidLevelAndExperience_Restores()
        {
            var snapshot = CreateSaveVersion3Snapshot(level: 1, totalExperience: 0);

            var restored = PlayerSessionStateMapper.FromSnapshot(snapshot, CreateExperienceTable());

            Assert.AreEqual(1, restored.Stats.Level);
        }

        [Test]
        public void FromSnapshot_SaveVersion3_Level1JustBelowNextThreshold_Restores()
        {
            // Level 2 requires 10 XP in the fixture table; 9 is the boundary just below it.
            var snapshot = CreateSaveVersion3Snapshot(level: 1, totalExperience: 9);

            var restored = PlayerSessionStateMapper.FromSnapshot(snapshot, CreateExperienceTable());

            Assert.AreEqual(1, restored.Stats.Level);
        }

        [Test]
        public void FromSnapshot_SaveVersion3_MaxLevelWithSurplusExperience_Restores()
        {
            // Beyond the last threshold (70 for level 5, the fixture table's MaxLevel): surplus
            // experience must not be rejected, nor push the level past MaxLevel.
            var snapshot = CreateSaveVersion3Snapshot(level: 5, totalExperience: 999);

            var restored = PlayerSessionStateMapper.FromSnapshot(snapshot, CreateExperienceTable());

            Assert.AreEqual(5, restored.Stats.Level);
        }

        [Test]
        public void FromSnapshot_SaveVersion3_LevelInconsistentWithExperience_ThrowsArgumentException()
        {
            var snapshot = CreateSaveVersion3Snapshot(level: 10, totalExperience: 0);

            Assert.Throws<ArgumentException>(() => PlayerSessionStateMapper.FromSnapshot(snapshot, CreateExperienceTable()));
        }

        [Test]
        public void FromSnapshot_SaveVersion3_NegativeTotalExperience_ThrowsArgumentException()
        {
            var snapshot = CreateSaveVersion3Snapshot(level: 1, totalExperience: -1);

            Assert.Throws<ArgumentException>(() => PlayerSessionStateMapper.FromSnapshot(snapshot, CreateExperienceTable()));
        }

        [Test]
        public void FromSnapshot_SaveVersion3_NoExperienceTableProvided_SkipsLevelExperienceValidation()
        {
            // Level/TotalExperience are inconsistent, but with no ExperienceTable supplied the
            // check must be skipped entirely (existing callers with no MasterData available).
            var snapshot = CreateSaveVersion3Snapshot(level: 10, totalExperience: 0);

            Assert.DoesNotThrow(() => PlayerSessionStateMapper.FromSnapshot(snapshot));
        }

        [Test]
        public void FromSnapshot_SaveVersion3_KnownWeaponId_Restores()
        {
            var snapshot = CreateSaveVersion3Snapshot(level: 1, totalExperience: 0);
            snapshot.EquippedWeaponId = "equip_test_sword";

            var restored = PlayerSessionStateMapper.FromSnapshot(snapshot, equipmentCatalog: CreateEquipmentCatalog());

            Assert.AreEqual("equip_test_sword", restored.Equipment.EquippedWeaponId);
        }

        [Test]
        public void FromSnapshot_SaveVersion3_KnownArmorId_Restores()
        {
            var snapshot = CreateSaveVersion3Snapshot(level: 1, totalExperience: 0);
            snapshot.EquippedArmorId = "equip_test_armor";

            var restored = PlayerSessionStateMapper.FromSnapshot(snapshot, equipmentCatalog: CreateEquipmentCatalog());

            Assert.AreEqual("equip_test_armor", restored.Equipment.EquippedArmorId);
        }

        [Test]
        public void FromSnapshot_SaveVersion3_UnknownWeaponId_ThrowsArgumentException()
        {
            var snapshot = CreateSaveVersion3Snapshot(level: 1, totalExperience: 0);
            snapshot.EquippedWeaponId = "equip_missing_sword";

            Assert.Throws<ArgumentException>(() => PlayerSessionStateMapper.FromSnapshot(snapshot, equipmentCatalog: CreateEquipmentCatalog()));
        }

        [Test]
        public void FromSnapshot_SaveVersion3_UnknownArmorId_ThrowsArgumentException()
        {
            var snapshot = CreateSaveVersion3Snapshot(level: 1, totalExperience: 0);
            snapshot.EquippedArmorId = "equip_missing_armor";

            Assert.Throws<ArgumentException>(() => PlayerSessionStateMapper.FromSnapshot(snapshot, equipmentCatalog: CreateEquipmentCatalog()));
        }

        [Test]
        public void FromSnapshot_SaveVersion3_WeaponSlotHasArmorCategoryId_ThrowsArgumentException()
        {
            var snapshot = CreateSaveVersion3Snapshot(level: 1, totalExperience: 0);
            snapshot.EquippedWeaponId = "equip_test_armor";

            Assert.Throws<ArgumentException>(() => PlayerSessionStateMapper.FromSnapshot(snapshot, equipmentCatalog: CreateEquipmentCatalog()));
        }

        [Test]
        public void FromSnapshot_SaveVersion3_ArmorSlotHasWeaponCategoryId_ThrowsArgumentException()
        {
            var snapshot = CreateSaveVersion3Snapshot(level: 1, totalExperience: 0);
            snapshot.EquippedArmorId = "equip_test_sword";

            Assert.Throws<ArgumentException>(() => PlayerSessionStateMapper.FromSnapshot(snapshot, equipmentCatalog: CreateEquipmentCatalog()));
        }

        [Test]
        public void FromSnapshot_SaveVersion3_NullOrEmptyEquippedIds_RestoreUnequipped_EvenWithCatalogProvided()
        {
            var snapshot = CreateSaveVersion3Snapshot(level: 1, totalExperience: 0);
            snapshot.EquippedWeaponId = null;
            snapshot.EquippedArmorId = string.Empty;

            var restored = PlayerSessionStateMapper.FromSnapshot(snapshot, equipmentCatalog: CreateEquipmentCatalog());

            Assert.IsNull(restored.Equipment.EquippedWeaponId);
            Assert.IsNull(restored.Equipment.EquippedArmorId);
        }

        [Test]
        public void FromSnapshot_SaveVersion3_NoEquipmentCatalogProvided_SkipsEquipmentValidation()
        {
            var snapshot = CreateSaveVersion3Snapshot(level: 1, totalExperience: 0);
            snapshot.EquippedWeaponId = "equip_missing_sword";

            Assert.DoesNotThrow(() => PlayerSessionStateMapper.FromSnapshot(snapshot));
        }

        [Test]
        public void FromSnapshot_SaveVersion1_IgnoresExperienceTableAndEquipmentCatalog()
        {
            // A v1 save has neither field written meaningfully; even if a real ExperienceTable/
            // equipment catalog is supplied, v1's own (pre-v3) restoration path must not run the
            // new checks against it (they predate SaveVersion 3's invariants entirely).
            var snapshot = new SaveGameSnapshot
            {
                SaveVersion = 1,
                CurrentSceneId = SceneId.Village,
                Level = 10,
                MaxHp = 10,
                MaxMp = 5,
                TotalExperience = 0,
                EquippedWeaponId = "equip_missing_sword"
            };

            Assert.DoesNotThrow(() => PlayerSessionStateMapper.FromSnapshot(snapshot, CreateExperienceTable(), CreateEquipmentCatalog()));
        }

        [Test]
        public void FromSnapshot_SaveVersion2_IgnoresExperienceTableAndEquipmentCatalog()
        {
            var snapshot = new SaveGameSnapshot
            {
                SaveVersion = 2,
                CurrentSceneId = SceneId.Village,
                Level = 10,
                MaxHp = 10,
                MaxMp = 5,
                TotalExperience = 0,
                EquippedWeaponId = "equip_missing_sword"
            };

            Assert.DoesNotThrow(() => PlayerSessionStateMapper.FromSnapshot(snapshot, CreateExperienceTable(), CreateEquipmentCatalog()));
        }

        [Test]
        public void FromSnapshot_SaveVersion3_ValidSnapshotWithBothCatalogsProvided_RestoresInventoryEquipmentAndClaimedRewards()
        {
            // Back-compat sanity check: providing both new validation catalogs must not change
            // restoration of the T-024 fields on an otherwise-valid v3 snapshot. Level 1 /
            // TotalExperience 0 is used here (rather than the CreateState() helper's fixed
            // Level 3) specifically so it is consistent with CreateExperienceTable() and does not
            // trip the new Level/TotalExperience check.
            var stats = new CharacterStats(level: 1, maxHp: 50, maxMp: 20, attack: 15, defense: 8, agility: 12, magic: 6);
            var original = new PlayerSessionState(
                SceneId.Village, stats, totalExperience: 0, currentHp: 50, currentMp: 20,
                new MainQuestProgress(), new QuestProgress(), new QuestProgress());
            original.Inventory.Add("equip_test_sword", 1);
            original.Equipment.EquipWeapon("equip_test_sword");
            original.ClaimReward("field_pickup_1");
            var snapshot = PlayerSessionStateMapper.ToSnapshot(original);

            var restored = PlayerSessionStateMapper.FromSnapshot(snapshot, CreateExperienceTable(), CreateEquipmentCatalog());

            Assert.AreEqual("equip_test_sword", restored.Equipment.EquippedWeaponId);
            Assert.AreEqual(1, restored.Inventory.GetQuantity("equip_test_sword"));
            Assert.IsTrue(restored.HasClaimedReward("field_pickup_1"));
        }
    }
}
