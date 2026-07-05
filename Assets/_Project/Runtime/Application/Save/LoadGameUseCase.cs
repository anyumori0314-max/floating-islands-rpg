using System;
using System.Collections.Generic;
using FloatingIslandsRpg.Domain.MasterData;
using FloatingIslandsRpg.Domain.Progression;

namespace FloatingIslandsRpg.Application.Save
{
    public sealed class LoadGameUseCase
    {
        private readonly ISaveRepository _repository;

        // Optional (Codex review Major 3): when left unset, PlayerSessionStateMapper.FromSnapshot
        // skips its SaveVersion 3 integrity checks, exactly as before this property existed.
        // Composition assigns both from real MasterData once it is available (see
        // TitleSceneInstaller), without needing to change this class's constructor or any
        // existing call site.
        public ExperienceTable ExperienceTable { get; set; }
        public IReadOnlyDictionary<string, EquipmentMasterData> EquipmentCatalog { get; set; }

        public LoadGameUseCase(ISaveRepository repository)
        {
            if (repository is null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            _repository = repository;
        }

        public LoadResult Load()
        {
            if (!_repository.TryLoad(out var snapshot))
            {
                return LoadResult.Failed("No valid save data available.");
            }

            try
            {
                var state = PlayerSessionStateMapper.FromSnapshot(snapshot, ExperienceTable, EquipmentCatalog);
                return LoadResult.Ok(state);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException)
            {
                return LoadResult.Failed(ex.Message);
            }
        }
    }
}
