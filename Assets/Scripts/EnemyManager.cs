using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý tracking tất cả enemies trong game, đặc biệt là enemies đã chết
/// để có thể save/load đúng trạng thái
/// </summary>
public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    // Dictionary lưu tất cả enemies theo scene
    private Dictionary<string, List<Enemy>> enemiesByScene = new Dictionary<string, List<Enemy>>();

    // List lưu thông tin enemies đã chết (để save game)
    private List<EnemySaveData> deadEnemies = new List<EnemySaveData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            Debug.Log("✅ EnemyManager initialized");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Khi scene mới load, tự động register tất cả enemies
        StartCoroutine(RegisterEnemiesInSceneDelayed(scene.name));
    }

    private System.Collections.IEnumerator RegisterEnemiesInSceneDelayed(string sceneName)
    {
        // Chờ 1 frame để enemies được khởi tạo
        yield return null;

        Enemy[] enemies = FindObjectsOfType<Enemy>();
        Debug.Log($"📋 EnemyManager found {enemies.Length} enemies in scene: {sceneName}");

        foreach (Enemy enemy in enemies)
        {
            RegisterEnemy(enemy);
        }
    }

    /// <summary>
    /// Đăng ký enemy vào manager
    /// Gọi hàm này từ Enemy.Start()
    /// </summary>
    public void RegisterEnemy(Enemy enemy)
    {
        if (enemy == null) return;

        string sceneName = SceneManager.GetActiveScene().name;

        if (!enemiesByScene.ContainsKey(sceneName))
        {
            enemiesByScene[sceneName] = new List<Enemy>();
        }

        if (!enemiesByScene[sceneName].Contains(enemy))
        {
            enemiesByScene[sceneName].Add(enemy);
            Debug.Log($"✅ Registered enemy: {enemy.GetEnemyID()} in scene {sceneName}");
        }
    }

    /// <summary>
    /// Gọi hàm này từ Enemy.Die() TRƯỚC KHI destroy
    /// </summary>
    public void ReportEnemyDeath(Enemy enemy)
    {
        if (enemy == null) return;

        // Lưu thông tin enemy đã chết
        EnemySaveData deathData = enemy.GetSaveData();
        if (deathData != null)
        {
            deathData.isDead = true; // Đảm bảo flag chết được set

            // Xóa entry cũ nếu có (tránh duplicate)
            deadEnemies.RemoveAll(e => e.enemyID == deathData.enemyID);

            // Thêm vào list dead enemies
            deadEnemies.Add(deathData);

            Debug.Log($"💀 Enemy death reported: {deathData.enemyID} ({deathData.enemyType})");
        }

        // Xóa khỏi list active enemies
        string sceneName = SceneManager.GetActiveScene().name;
        if (enemiesByScene.ContainsKey(sceneName))
        {
            enemiesByScene[sceneName].Remove(enemy);
        }
        //kiem tra enemy da chet co phai la BringerOfDeath khong
        if (enemy.GetEnemyType() == "BringerOfDeath")
        {
            //thong bao cho GameManager biet BringerOfDeath da chet
            GameManager.Instance.OnBringerOfDeathDefeated();
        }
    }

    /// <summary>
    /// Lấy tất cả enemies (sống + chết) trong scene hiện tại để save
    /// </summary>
    public List<EnemySaveData> GetAllEnemiesSaveData()
    {
        List<EnemySaveData> allEnemiesData = new List<EnemySaveData>();
        string currentScene = SceneManager.GetActiveScene().name;

        // 1. Lấy enemies còn sống trong scene hiện tại
        if (enemiesByScene.ContainsKey(currentScene))
        {
            foreach (Enemy enemy in enemiesByScene[currentScene])
            {
                if (enemy != null && !enemy.IsDead())
                {
                    EnemySaveData data = enemy.GetSaveData();
                    if (data != null)
                    {
                        allEnemiesData.Add(data);
                        Debug.Log($"💾 Saved alive enemy: {data.enemyID} - Health: {data.currentHealth}");
                    }
                }
            }
        }

        // 2. Thêm enemies đã chết trong scene hiện tại
        foreach (EnemySaveData deadEnemy in deadEnemies)
        {
            if (deadEnemy.sceneName == currentScene)
            {
                allEnemiesData.Add(deadEnemy);
                Debug.Log($"💀 Saved dead enemy: {deadEnemy.enemyID}");
            }
        }

        Debug.Log($"✅ EnemyManager: Total {allEnemiesData.Count} enemies saved for scene {currentScene}");
        return allEnemiesData;
    }

    /// <summary>
    /// Load trạng thái enemies từ save data
    /// </summary>
    public void LoadEnemiesState(List<EnemySaveData> savedEnemies)
    {
        if (savedEnemies == null || savedEnemies.Count == 0)
        {
            Debug.Log("No enemy save data to load");
            return;
        }

        string currentScene = SceneManager.GetActiveScene().name;

        // Reset dead enemies list cho scene hiện tại
        deadEnemies.RemoveAll(e => e.sceneName == currentScene);

        // Load lại dead enemies từ save
        foreach (EnemySaveData data in savedEnemies)
        {
            if (data.sceneName == currentScene && data.isDead)
            {
                deadEnemies.Add(data);
                Debug.Log($"💀 Loaded dead enemy info: {data.enemyID}");
            }
        }

        Debug.Log($"✅ EnemyManager loaded {deadEnemies.Count} dead enemies for scene {currentScene}");
    }

    /// <summary>
    /// Kiểm tra xem enemy có trong dead list không (dùng khi load game)
    /// </summary>
    public bool IsEnemyDead(string enemyID, string sceneName)
    {
        return deadEnemies.Exists(e => e.enemyID == enemyID && e.sceneName == sceneName);
    }

    /// <summary>
    /// Clear tất cả data (dùng khi start new game)
    /// </summary>
    public void ClearAllData()
    {
        enemiesByScene.Clear();
        deadEnemies.Clear();
        Debug.Log("🗑️ EnemyManager: All data cleared");
    }

    /// <summary>
    /// Debug: In thông tin tất cả enemies
    /// </summary>
    public void PrintDebugInfo()
    {
        Debug.Log("========== ENEMY MANAGER DEBUG ==========");

        string currentScene = SceneManager.GetActiveScene().name;

        if (enemiesByScene.ContainsKey(currentScene))
        {
            Debug.Log($"Alive enemies in {currentScene}: {enemiesByScene[currentScene].Count}");
            foreach (Enemy enemy in enemiesByScene[currentScene])
            {
                if (enemy != null)
                {
                    Debug.Log($"  - {enemy.GetEnemyID()} ({enemy.GetEnemyType()}) Health: {enemy.GetCurrentHealth()}");
                }
            }
        }

        Debug.Log($"Dead enemies in {currentScene}: {deadEnemies.FindAll(e => e.sceneName == currentScene).Count}");
        foreach (EnemySaveData dead in deadEnemies.FindAll(e => e.sceneName == currentScene))
        {
            Debug.Log($"  - {dead.enemyID} ({dead.enemyType}) [DEAD]");
        }

        Debug.Log("=========================================");
    }
}
