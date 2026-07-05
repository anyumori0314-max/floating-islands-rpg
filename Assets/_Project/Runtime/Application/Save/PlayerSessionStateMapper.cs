using System;
using System.Collections.Generic;
using FloatingIslandsRpg.Application.Session;
using FloatingIslandsRpg.Domain.Characters.Stats;
using FloatingIslandsRpg.Domain.MasterData;
using FloatingIslandsRpg.Domain.Progression;
using FloatingIslandsRpg.Domain.Quests;

namespace FloatingIslandsRpg.Application.Save
{
    public static class PlayerSessionStateMapper
    {
        public static SaveGameSnapshot ToSnapshot(PlayerSessionState state)
        {
            if (state is null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            return new SaveGameSnapshot
            {
                SaveVersion = SaveGameSnapshot.CurrentSaveVersion,
                CurrentSceneId = state.CurrentSceneId,
                Level = state.Stats.Level,
                MaxHp = state.Stats.MaxHp,
                MaxMp = state.Stats.MaxMp,
                Attack = state.Stats.Attack,
                Defense = state.Stats.Defense,
                Agility = state.Stats.Agility,
                Magic = state.Stats.Magic,
                TotalExperience = state.TotalExperience,
                CurrentHp = state.CurrentHp,
                CurrentMp = state.CurrentMp,
                MainQuestStage = state.MainQuest.CurrentStage,
                SubQuest1State = state.SubQuest1.CurrentState,
                SubQuest2State = state.SubQuest2.CurrentState,
                InventoryEntries = ToInventoryEntries(state.Inventory),
                EquippedWeaponId = state.Equipment.EquippedWeaponId,
                EquippedArmorId = state.Equipment.EquippedArmorId,
                ClaimedRewardIds = new List<string>(state.ClaimedRewardIds).ToArray()
            };
        }

        private static InventoryEntrySnapshot[] ToInventoryEntries(Domain.Inventory.Inventory inventory)
        {
            var quantities = inventory.Quantities;
            var entries = new InventoryEntrySnapshot[quantities.Count];
            var index = 0;

            foreach (var entry in quantities)
            {
                entries[index] = new InventoryEntrySnapshot { ItemId = entry.Key, Quantity = entry.Value };
                index++;
            }

            return entries;
        }

        // experienceTable/equipmentCatalog are optional (Codex review Major 3): when omitted, the
        // SaveVersion 3 integrity checks below are skipped entirely, so every pre-existing caller
        // that restores a snapshot with no MasterData available keeps its exact prior behavior.
        // Composition supplies both from real MasterData for the one production load path
        // (TitleSceneInstaller); v1/v2 saves never run these checks regardless (see below), since
        // they predate SaveVersion 3's Inventory/Equipment/Level-vs-Experience invariants.
        public static PlayerSessionState FromSnapshot(
            SaveGameSnapshot snapshot,
            ExperienceTable experienceTable = null,
            IReadOnlyDictionary<string, EquipmentMasterData> equipmentCatalog = null)
        {
            if (snapshot is null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            if (snapshot.SaveVersion < 1 || snapshot.SaveVersion > SaveGameSnapshot.CurrentSaveVersion)
            {
                throw new NotSupportedException($"Unsupported save version: {snapshot.SaveVersion}.");
            }

            if (snapshot.SaveVersion >= 3)
            {
                ValidateSaveVersion3Integrity(snapshot, experienceTable, equipmentCatalog);
            }

            var stats = new CharacterStats(
                snapshot.Level,
                snapshot.MaxHp,
                snapshot.MaxMp,
                snapshot.Attack,
                snapshot.Defense,
                snapshot.Agility,
                snapshot.Magic);

            // SaveVersion 1 predates MainQuestStage; migrate its 3-value MainQuestState into the
            // nearest safe equivalent 5-value stage instead of losing progress outright.
            var mainQuest = snapshot.SaveVersion == 1
                ? RestoreMainQuestFromLegacyState(snapshot.MainQuestState)
                : RestoreMainQuest(snapshot.MainQuestStage);

            var subQuest1 = RestoreQuest(snapshot.SubQuest1State);
            var subQuest2 = RestoreQuest(snapshot.SubQuest2State);

            // SaveVersion < 3 predates Inventory/Equipment/ClaimedRewardIds entirely; those
            // saves restore to the safe initial values (empty inventory, nothing equipped, no
            // rewards claimed) rather than attempting to read fields that were never written.
            var inventory = snapshot.SaveVersion >= 3
                ? RestoreInventory(snapshot.InventoryEntries)
                : new Domain.Inventory.Inventory();

            var equipment = snapshot.SaveVersion >= 3
                ? new Domain.Inventory.EquipmentLoadout(snapshot.EquippedWeaponId, snapshot.EquippedArmorId)
                : new Domain.Inventory.EquipmentLoadout();

            var claimedRewardIds = snapshot.SaveVersion >= 3 && snapshot.ClaimedRewardIds != null
                ? snapshot.ClaimedRewardIds
                : Array.Empty<string>();

            return new PlayerSessionState(
                snapshot.CurrentSceneId,
                stats,
                snapshot.TotalExperience,
                snapshot.CurrentHp,
                snapshot.CurrentMp,
                mainQuest,
                subQuest1,
                subQuest2,
                inventory,
                equipment,
                claimedRewardIds);
        }

        // Rejects a SaveVersion 3+ snapshot whose Level/TotalExperience or equipped ids could not
        // have arisen from legitimate play (Codex review Major 3), e.g. a hand-edited save file.
        // Throwing ArgumentException here is deliberate: LoadGameUseCase.Load() already treats
        // ArgumentException as an expected "this save is unusable" outcome (see its catch clause),
        // and JsonSaveRepository.IsRestorable() already routes the same exception into falling
        // back to the backup save -- both paths keep working unchanged with no further wiring.
        private static void ValidateSaveVersion3Integrity(
            SaveGameSnapshot snapshot,
            ExperienceTable experienceTable,
            IReadOnlyDictionary<string, EquipmentMasterData> equipmentCatalog)
        {
            if (experienceTable != null)
            {
                if (snapshot.TotalExperience < 0)
                {
                    throw new ArgumentException(
                        $"Save TotalExperience {snapshot.TotalExperience} must be 0 or greater.", nameof(snapshot));
                }

                var expectedLevel = LevelUpCalculator.CalculateLevel(experienceTable, snapshot.TotalExperience);
                if (snapshot.Level != expectedLevel)
                {
                    throw new ArgumentException(
                        $"Save Level {snapshot.Level} is inconsistent with TotalExperience {snapshot.TotalExperience} (expected Level {expectedLevel}).",
                        nameof(snapshot));
                }
            }

            if (equipmentCatalog != null)
            {
                ValidateEquippedId(snapshot.EquippedWeaponId, EquipmentSlot.Weapon, equipmentCatalog, nameof(snapshot.EquippedWeaponId));
                ValidateEquippedId(snapshot.EquippedArmorId, EquipmentSlot.Armor, equipmentCatalog, nameof(snapshot.EquippedArmorId));
            }
        }

        // A null/empty equipped id always means "unequipped" and is always valid, regardless of
        // catalog contents (PROJECT.md T-024 EquipmentLoadout convention, preserved here).
        private static void ValidateEquippedId(
            string equippedId,
            EquipmentSlot expectedSlot,
            IReadOnlyDictionary<string, EquipmentMasterData> equipmentCatalog,
            string fieldName)
        {
            if (string.IsNullOrWhiteSpace(equippedId))
            {
                return;
            }

            if (!equipmentCatalog.TryGetValue(equippedId, out var equipment))
            {
                throw new ArgumentException($"Save {fieldName} '{equippedId}' does not exist in the equipment catalog.", fieldName);
            }

            if (equipment.Slot != expectedSlot)
            {
                throw new ArgumentException(
                    $"Save {fieldName} '{equippedId}' is a {equipment.Slot} item and cannot be equipped in the {expectedSlot} slot.",
                    fieldName);
            }
        }

        // Duplicate ItemIds in a corrupted/hand-edited save are resolved "last entry wins"
        // (PROJECT.md T-024: "重複ItemIdの扱いを明確にする") rather than throwing, since the
        // array itself has no uniqueness guarantee the way a Dictionary would.
        private static Domain.Inventory.Inventory RestoreInventory(InventoryEntrySnapshot[] entries)
        {
            if (entries == null || entries.Length == 0)
            {
                return new Domain.Inventory.Inventory();
            }

            var quantities = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var entry in entries)
            {
                quantities[entry.ItemId] = entry.Quantity;
            }

            return new Domain.Inventory.Inventory(quantities);
        }

        private static MainQuestProgress RestoreMainQuest(MainQuestStage targetStage)
        {
            if (!Enum.IsDefined(typeof(MainQuestStage), targetStage))
            {
                throw new ArgumentOutOfRangeException(nameof(targetStage), targetStage, "Unknown MainQuestStage.");
            }

            var quest = new MainQuestProgress();

            if (targetStage >= MainQuestStage.ExploreField)
            {
                quest.Start();
            }

            if (targetStage >= MainQuestStage.EnterDungeon)
            {
                quest.AdvanceToEnterDungeon();
            }

            if (targetStage >= MainQuestStage.DefeatBoss)
            {
                quest.AdvanceToDefeatBoss();
            }

            if (targetStage == MainQuestStage.Completed)
            {
                quest.Complete();
            }

            return quest;
        }

        private static MainQuestProgress RestoreMainQuestFromLegacyState(QuestState legacyState)
        {
            if (!Enum.IsDefined(typeof(QuestState), legacyState))
            {
                throw new ArgumentOutOfRangeException(nameof(legacyState), legacyState, "Unknown QuestState.");
            }

            // A v1 "InProgress" save cannot say which of Field/Dungeon/Boss the player had
            // reached, so it is restored to the earliest in-progress stage (a safe initial
            // value) rather than guessed further along.
            var mappedStage = legacyState switch
            {
                QuestState.NotStarted => MainQuestStage.NotStarted,
                QuestState.InProgress => MainQuestStage.ExploreField,
                QuestState.Completed => MainQuestStage.Completed,
                _ => MainQuestStage.NotStarted
            };

            return RestoreMainQuest(mappedStage);
        }

        private static QuestProgress RestoreQuest(QuestState state)
        {
            if (!Enum.IsDefined(typeof(QuestState), state))
            {
                throw new ArgumentOutOfRangeException(nameof(state), state, "Unknown QuestState.");
            }

            var quest = new QuestProgress();

            if (state == QuestState.InProgress || state == QuestState.Completed)
            {
                quest.Start();
            }

            if (state == QuestState.Completed)
            {
                quest.Complete();
            }

            return quest;
        }
    }
}
