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

            GameData saveData = SaveGameManager.Instance.LoadGameData();
            if (saveData != null)
            {
                string currentScene = SceneManager.GetActiveScene().name;

                // ✅ DEBUG: Log để kiểm tra
                Debug.Log($"🔍 Current Scene: '{currentScene}'");
                Debug.Log($"🔍 Save Scene: '{saveData.lastSceneName}'");
                Debug.Log($"🔍 Are they equal? {saveData.lastSceneName == currentScene}");

                // ✅ FIX: So sánh chặt chẽ và trim whitespace
                if (!string.IsNullOrEmpty(saveData.lastSceneName) &&
                    saveData.lastSceneName.Trim() != currentScene.Trim())
                {
                    // Nếu checkpoint ở scene khác -> load scene đó
                    Debug.Log($"🌍 Respawn requires scene change: {currentScene} → {saveData.lastSceneName}");

                    if (LoadingManager.Instance != null)
                    {
                        LoadingManager.Instance.LoadMapFromSave(saveData, currentScene);
                    }
                }
                else
                {
                    // ✅ Nếu cùng scene -> CHỈ RESET PLAYER, KHÔNG RELOAD SCENE
                    Debug.Log("♻️ Respawning in same scene - Resetting player state WITHOUT reloading scene");

                    // Reset player position
                    player.transform.position = new Vector3(saveData.playerPosX, saveData.playerPosY, player.transform.position.z);

                    // Apply saved data (health, stamina, inventory, enemies)
                    if (SaveGameManager.Instance != null)
                    {
                        SaveGameManager.Instance.ApplyLoadedData(player, saveData);
                    }

                    // ✅ FIX: Enable movement sau khi respawn
                    player.EnableMovement();

                    Debug.Log("✅ Player respawned at checkpoint in same scene");
                }
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

        // ✅ Enable movement
        player.EnableMovement();

        // ✅ FIX: KHÔNG reload scene nếu đã ở default scene
        string currentScene = SceneManager.GetActiveScene().name;

        Debug.Log($"🔍 SpawnAtDefault - Current: '{currentScene}', Default: '{defaultSpawnScene}'");

        if (!string.IsNullOrEmpty(defaultSpawnScene) &&
            defaultSpawnScene.Trim() != currentScene.Trim())
        {
            // Chỉ load scene khác nếu cần thiết
            Debug.Log($"🌍 Need to load default scene: {currentScene} → {defaultSpawnScene}");

            if (LoadingManager.Instance != null)
            {
                LoadingManager.Instance.LoadMap(defaultSpawnScene, "default");
            }
        }
        else
        {
            Debug.Log("✅ Already in default scene - No need to reload");

            // ✅ TODO: Reset scene state nếu cần (hiện tại không cần vì sẽ load lại từ save)
            // Reset scene state (respawn enemies, reset chests) WITHOUT reloading
            // if (EnemyManager.Instance != null)
            // {
            //     EnemyManager.Instance.ResetAllEnemies();
            // }

            // if (ChestManager.Instance != null)
            // {
            //     ChestManager.Instance.ResetAllChests();
            // }
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
    public void ResetCheckpoints()
    {
        currentCheckpointID = null;
        currentCheckpointPosition = Vector3.zero;
        currentCheckpointScene = null;
        activatedCheckpoints.Clear();

        Debug.Log("🗑️ All checkpoints cleared");
    }

    /// <summary>
    /// [DEPRECATED] Dùng ResetCheckpoints() thay thế
    /// </summary>
    public void ClearAllCheckpoints()
    {
        ResetCheckpoints();
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
