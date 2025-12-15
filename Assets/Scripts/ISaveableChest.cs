using UnityEngine;

/// <summary>
/// Interface cho các Chest có thể save/load được
/// </summary>
public interface ISaveableChest
{
    string GetChestID();
    bool IsOpened();
    ChestSaveData GetSaveData();
    void LoadFromSaveData(ChestSaveData data);
}
