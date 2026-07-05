using System;
using System.Collections.Generic;
using FloatingIslandsRpg.Application.Save;
using FloatingIslandsRpg.Domain.MasterData;
using FloatingIslandsRpg.Domain.Progression;
using UnityEngine;

namespace FloatingIslandsRpg.Infrastructure.Save
{
    public sealed class JsonSaveRepository : ISaveRepository
    {
        private readonly FileSystemSaveStorage _storage;

        // Optional (Codex review Major 3), mirroring LoadGameUseCase: when unset, IsRestorable()
        // below validates exactly as it did before SaveVersion 3 integrity checks existed. Without
        // these, a primary save that is well-formed JSON but fails the new Level/TotalExperience or
        // equipment-id checks would never be recognized as unrestorable here, so TryLoad would
        // never fall back to a valid backup for that specific failure mode.
        public ExperienceTable ExperienceTable { get; set; }
        public IReadOnlyDictionary<string, EquipmentMasterData> EquipmentCatalog { get; set; }

        public JsonSaveRepository(FileSystemSaveStorage storage)
        {
            if (storage is null)
            {
                throw new ArgumentNullException(nameof(storage));
            }

            _storage = storage;
        }

        public void Save(SaveGameSnapshot snapshot)
        {
            if (snapshot is null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            var json = JsonUtility.ToJson(snapshot);
            _storage.Write(json);
        }

        public bool TryLoad(out SaveGameSnapshot snapshot)
        {
            if (_storage.TryReadPrimary(out var primaryJson) && TryParseAndValidate(primaryJson, out snapshot))
            {
                return true;
            }

            if (_storage.TryReadBackup(out var backupJson) && TryParseAndValidate(backupJson, out snapshot))
            {
                return true;
            }

            snapshot = null;
            return false;
        }

        // A candidate is accepted only once it both parses as JSON and is confirmed restorable as
        // valid game state (PlayerSessionStateMapper.FromSnapshot succeeds). A syntactically valid
        // but semantically invalid snapshot (e.g. MaxHp = 0, CurrentHp > MaxHp) must not be treated
        // as usable, or a corrupted-but-parsable primary would shadow a valid backup.
        private bool TryParseAndValidate(string json, out SaveGameSnapshot snapshot)
        {
            if (!TryParse(json, out var parsed) || !IsRestorable(parsed))
            {
                snapshot = null;
                return false;
            }

            snapshot = parsed;
            return true;
        }

        // A parse failure means the file is corrupted; treated as an expected failure mode
        // (triggers fallback to the backup, or a safe initial state) rather than an unexpected error.
        private static bool TryParse(string json, out SaveGameSnapshot snapshot)
        {
            try
            {
                snapshot = JsonUtility.FromJson<SaveGameSnapshot>(json);
                return snapshot != null && snapshot.SaveVersion != 0;
            }
            catch (ArgumentException)
            {
                snapshot = null;
                return false;
            }
        }

        private bool IsRestorable(SaveGameSnapshot snapshot)
        {
            try
            {
                PlayerSessionStateMapper.FromSnapshot(snapshot, ExperienceTable, EquipmentCatalog);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (NotSupportedException)
            {
                return false;
            }
        }
    }
}
