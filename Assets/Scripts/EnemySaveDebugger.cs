using UnityEngine;
using System.IO;

/// <summary>
/// Script debug để kiểm tra enemy save/load
/// Attach vào một GameObject trong scene để xem logs chi tiết
/// </summary>
public class EnemySaveDebugger : MonoBehaviour
{
    [Header("Debug Options")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private KeyCode debugKey = KeyCode.F9;

    private void Update()
    {
        if (Input.GetKeyDown(debugKey))
        {
            PrintEnemyDebugInfo();
        }
    }

    private void PrintEnemyDebugInfo()
    {
        Debug.Log("==================== ENEMY DEBUG INFO ====================");

        // 🎯 Sử dụng EnemyManager nếu có
        if (EnemyManager.Instance != null)
        {
            Debug.Log("📋 Using EnemyManager for debug info:");
            EnemyManager.Instance.PrintDebugInfo();
        }
        else
        {
            Debug.LogWarning("⚠️ EnemyManager not found! Using fallback method.");
        }

        // 1. Tìm tất cả enemies trong scene
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        Debug.Log($"📊 Total alive enemies found by FindObjectsOfType: {enemies.Length}");

        if (enemies.Length == 0)
        {
            Debug.LogWarning("⚠️ No alive enemies found in current scene!");
        }
        else
        {
            // 2. In thông tin từng enemy
            for (int i = 0; i < enemies.Length; i++)
            {
                Enemy enemy = enemies[i];
                if (enemy == null) continue;

                Debug.Log($"\n--- Alive Enemy {i + 1} ---");
                Debug.Log($"  Name: {enemy.gameObject.name}");
                Debug.Log($"  ID: {enemy.GetEnemyID()}");
                Debug.Log($"  Type: {enemy.GetEnemyType()}");
                Debug.Log($"  Health: {enemy.GetCurrentHealth()}/{enemy.GetMaxHealth()}");
                Debug.Log($"  Position: {enemy.transform.position}");
                Debug.Log($"  Is Dead: {enemy.IsDead()}");
            }
        }

        // 3. Kiểm tra save file
        CheckSaveFile();

        Debug.Log("========================================================");
    }

    private void CheckSaveFile()
    {
        string saveFilePath = Path.Combine(Application.persistentDataPath, "savegame.json");

        Debug.Log($"\n💾 Save file path: {saveFilePath}");

        if (File.Exists(saveFilePath))
        {
            try
            {
                string json = File.ReadAllText(saveFilePath);
                GameData data = JsonUtility.FromJson<GameData>(json);

                if (data != null && data.enemies != null)
                {
                    Debug.Log($"✅ Save file found with {data.enemies.Count} enemies saved");

                    foreach (var enemyData in data.enemies)
                    {
                        Debug.Log($"  - {enemyData.enemyID} ({enemyData.enemyType}) " +
                                  $"[Health: {enemyData.currentHealth}, Dead: {enemyData.isDead}]");
                    }
                }
                else
                {
                    Debug.LogWarning("⚠️ Save file exists but has no enemy data");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error reading save file: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ No save file found");
        }
    }

    private void OnGUI()
    {
        if (!showDebugInfo) return;

        GUI.Box(new Rect(10, 10, 300, 80), "Enemy Save Debugger");

        Enemy[] enemies = FindObjectsOfType<Enemy>();
        GUI.Label(new Rect(20, 35, 280, 20), $"Enemies in scene: {enemies.Length}");
        GUI.Label(new Rect(20, 55, 280, 20), $"Press {debugKey} for detailed info");

        if (GUI.Button(new Rect(20, 70, 100, 20), "Debug Info"))
        {
            PrintEnemyDebugInfo();
        }
    }
}
