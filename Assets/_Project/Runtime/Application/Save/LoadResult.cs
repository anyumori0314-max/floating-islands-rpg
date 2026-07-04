using FloatingIslandsRpg.Application.Session;

namespace FloatingIslandsRpg.Application.Save
{
    public sealed class LoadResult
    {
        public bool Success { get; }
        public PlayerSessionState State { get; }
        public string ErrorMessage { get; }

        private LoadResult(bool success, PlayerSessionState state, string errorMessage)
        {
            Success = success;
            State = state;
            ErrorMessage = errorMessage;
        }

        public static LoadResult Ok(PlayerSessionState state)
        {
            return new LoadResult(true, state, null);
        }

        public static LoadResult Failed(string errorMessage)
        {
            return new LoadResult(false, null, errorMessage);
        }
    }
}
