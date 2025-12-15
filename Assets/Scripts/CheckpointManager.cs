using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý hệ thống checkpoint và respawn
/// </summary>
public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    [Header("Default Spawn")]
    [SerializeField] private Vector3 defaultSpawnPosition = Vector3.zero;
    [SerializeField] private string defaultSpawnScene = "Map1";

    // Checkpoint hiện tại
    private string currentCheckpointID;
    private Vector3 currentCheckpointPosition;
    private string currentCheckpointScene;

    // List checkpoints đã kích hoạt (để tracking)
    private HashSet<string> activatedCheckpoints = new HashSet<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("✅ CheckpointManager initialized");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Kích hoạt checkpoint - Save full game + set làm respawn point
    /// </summary>
    public void ActivateCheckpoint(string checkpointID, Vector3 position)
    {
        currentCheckpointID = checkpointID;
        currentCheckpointPosition = position;
        currentCheckpointScene = SceneManager.GetActiveScene().name;

        // Đánh dấu checkpoint đã activate
        if (!activatedCheckpoints.Contains(checkpointID))
        {
            activatedCheckpoints.Add(checkpointID);
        }

        // 🎯 SAVE FULL GAME tại checkpoint
        if (SaveGameManager.Instance != null)
        {
            SaveGameManager.Instance.SaveGame();
            Debug.Log($"💾 Game saved at checkpoint: {checkpointID}");
        }
        else
        {
            Debug.LogWarning("⚠️ SaveGameManager not found!");
        }

        // 🩹 HỒI MÁU VÀ STAMINA ĐẦY
        knight player = FindObjectOfType<knight>();
        if (player != null)
        {
            player.RestoreHealthAndStamina();
            Debug.Log("💚 Player health and stamina restored");
        }

        // Visual feedback
        ShowCheckpointActivatedMessage(checkpointID);
    }

    /// <summary>
    /// Respawn player khi chết
    /// </summary>
    public void RespawnPlayer()
    {
        Debug.Log("💀 Player died - Respawning...");

        knight player = FindObjectOfType<knight>();
        if (player == null)
        {
            Debug.LogError("❌ Player not found for respawn!");
            return;
        }

        // Kiểm tra xem có checkpoint save không
        if (SaveGameManager.Instance != null && SaveGameManager.Instance.HasSaveData())
        {
            // 🔄 LOAD LẠI TỪ CHECKPOINT CUỐI
            Debug.Log("🔄 Loading from last checkpoint...");

            // Load game sẽ restore position, health, stamina, inventory, enemies
            if (LoadingManager.Instance != null)
            {
                GameData saveData = SaveGameManager.Instance.LoadGameData();
                if (saveData != null)
                {
                    LoadingManager.Instance.LoadMapFromSave(saveData, SceneManager.GetActiveScene().name);
                }
            }
            else
            {
                Debug.LogError("❌ LoadingManager not found!");
                SpawnAtDefault(player);
            }
        }
        else
        {
            // 📍 SPAWN TẠI DEFAULT NẾU CHƯA CÓ CHECKPOINT
            Debug.Log("📍 No checkpoint found - Spawning at default position");
            SpawnAtDefault(player);
        }
    }

    /// <summary>
    /// Spawn tại vị trí default (khi chưa có checkpoint)
    /// </summary>
    private void SpawnAtDefault(knight player)
    {
        if (player == null) return;

        // Teleport về default position
        player.transform.position = defaultSpawnPosition;

        // Reset health và stamina về mức cơ bản
        player.RestoreHealthAndStamina();

        // Reset scene (respawn enemies, reset chests...)
        if (LoadingManager.Instance != null)
        {
            LoadingManager.Instance.LoadMap(defaultSpawnScene, "default");
        }

        Debug.Log($"✅ Player respawned at default: {defaultSpawnPosition}");
    }

    /// <summary>
    /// Kiểm tra xem checkpoint đã được activate chưa
    /// </summary>
    public bool IsCheckpointActivated(string checkpointID)
    {
        return activatedCheckpoints.Contains(checkpointID);
    }

    /// <summary>
    /// Get current checkpoint info
    /// </summary>
    public string GetCurrentCheckpointID()
    {
        return currentCheckpointID;
    }

    public Vector3 GetCurrentCheckpointPosition()
    {
        return currentCheckpointPosition;
    }

    /// <summary>
    /// Clear all checkpoint data (khi start new game)
    /// </summary>
    public void ClearAllCheckpoints()
    {
        currentCheckpointID = null;
        currentCheckpointPosition = Vector3.zero;
        currentCheckpointScene = null;
        activatedCheckpoints.Clear();

        Debug.Log("🗑️ All checkpoints cleared");
    }

    /// <summary>
    /// Load checkpoint data từ save file
    /// </summary>
    public void LoadCheckpointData(string checkpointID, Vector3 position, string sceneName)
    {
        currentCheckpointID = checkpointID;
        currentCheckpointPosition = position;
        currentCheckpointScene = sceneName;

        if (!string.IsNullOrEmpty(checkpointID))
        {
            activatedCheckpoints.Add(checkpointID);
        }

        Debug.Log($"✅ Checkpoint data loaded: {checkpointID}");
    }

    /// <summary>
    /// Hiển thị message khi activate checkpoint
    /// </summary>
    private void ShowCheckpointActivatedMessage(string checkpointID)
    {
        // TODO: Hiển thị UI message "Checkpoint Activated"
        // Có thể tích hợp với UI system của bạn
        Debug.Log($"🔥 CHECKPOINT ACTIVATED: {checkpointID}");
    }

    /// <summary>
    /// Set default spawn position (gọi từ các map khác nhau)
    /// </summary>
    public void SetDefaultSpawn(Vector3 position, string sceneName)
    {
        defaultSpawnPosition = position;
        defaultSpawnScene = sceneName;
    }
}
