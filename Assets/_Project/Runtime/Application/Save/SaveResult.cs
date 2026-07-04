namespace FloatingIslandsRpg.Application.Save
{
    public sealed class SaveResult
    {
        public bool Success { get; }
        public string ErrorMessage { get; }

        private SaveResult(bool success, string errorMessage)
        {
            Success = success;
            ErrorMessage = errorMessage;
        }

        public static SaveResult Ok()
        {
            return new SaveResult(true, null);
        }

        public static SaveResult Failed(string errorMessage)
        {
            return new SaveResult(false, errorMessage);
        }
    }
}
