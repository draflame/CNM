using System;
using UnityEngine;

/// <summary>
/// Dữ liệu save của Chest
/// </summary>
[Serializable]
public class ChestSaveData
{
    public string chestID;
    public string sceneName;
    public float posX;
    public float posY;
    public bool isOpened;
    public bool hasDroppedItems; // ✅ Lưu trạng thái đã drop items

    public ChestSaveData(string id, string scene, Vector3 position, bool opened, bool dropped)
    {
        chestID = id;
        sceneName = scene;
        posX = position.x;
        posY = position.y;
        isOpened = opened;
        hasDroppedItems = dropped;
    }

    public ChestSaveData() { }
}
