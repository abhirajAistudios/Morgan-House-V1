public interface ISaveable
{
    void SaveState(ref SaveData data);
    void LoadState(SaveData data);
}
