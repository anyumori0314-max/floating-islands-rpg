namespace FloatingIslandsRpg.Application.Save
{
    public interface ISaveRepository
    {
        void Save(SaveGameSnapshot snapshot);

        bool TryLoad(out SaveGameSnapshot snapshot);
    }
}
