using UnityEngine;

/// <summary>
/// Interface cho các Enemy có thể save/load được
/// Bất kỳ enemy nào muốn được lưu trạng thái phải implement interface này
/// </summary>
public interface ISaveableEnemy
{
    /// <summary>
    /// ID duy nhất của enemy này
    /// Cần được set trong Inspector hoặc tự động generate khi spawn
    /// </summary>
    string GetEnemyID();

    /// <summary>
    /// Loại enemy (ví dụ: "Goblin", "BringerOfDeath", "DeathClaw")
    /// Dùng để xác định loại enemy khi load
    /// </summary>
    string GetEnemyType();

    /// <summary>
    /// Lấy dữ liệu save của enemy này
    /// </summary>
    EnemySaveData GetSaveData();

    /// <summary>
    /// Load và áp dụng trạng thái từ save data
    /// </summary>
    void LoadFromSaveData(EnemySaveData data);

    /// <summary>
    /// Kiểm tra xem enemy đã chết chưa
    /// </summary>
    bool IsDead();
}
