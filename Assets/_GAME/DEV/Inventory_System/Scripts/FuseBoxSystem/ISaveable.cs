public interface ISaveable
{
    void SaveState(ref AutoSaveManager.SaveData data);
    void LoadState(AutoSaveManager.SaveData data);
}
