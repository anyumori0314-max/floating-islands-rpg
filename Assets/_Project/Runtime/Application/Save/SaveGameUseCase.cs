using System;
using FloatingIslandsRpg.Application.Session;

namespace FloatingIslandsRpg.Application.Save
{
    public sealed class SaveGameUseCase
    {
        private readonly ISaveRepository _repository;

        public SaveGameUseCase(ISaveRepository repository)
        {
            if (repository is null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            _repository = repository;
        }

        public SaveResult Save(PlayerSessionState state)
        {
            if (state is null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            var snapshot = PlayerSessionStateMapper.ToSnapshot(state);
            _repository.Save(snapshot);
            return SaveResult.Ok();
        }
    }
}
