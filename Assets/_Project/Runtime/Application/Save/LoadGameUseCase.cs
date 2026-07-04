using System;

namespace FloatingIslandsRpg.Application.Save
{
    public sealed class LoadGameUseCase
    {
        private readonly ISaveRepository _repository;

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
                var state = PlayerSessionStateMapper.FromSnapshot(snapshot);
                return LoadResult.Ok(state);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException)
            {
                return LoadResult.Failed(ex.Message);
            }
        }
    }
}
