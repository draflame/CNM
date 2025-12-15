using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý tracking tất cả chests trong game
/// </summary>
public class ChestManager : MonoBehaviour
{
    public static ChestManager Instance { get; private set; }

    private Dictionary<string, List<Chest>> chestsByScene = new Dictionary<string, List<Chest>>();
    private List<ChestSaveData> openedChests = new List<ChestSaveData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            Debug.Log("✅ ChestManager initialized");
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
        StartCoroutine(RegisterChestsInSceneDelayed(scene.name));
    }

    private System.Collections.IEnumerator RegisterChestsInSceneDelayed(string sceneName)
    {
        yield return new WaitForSeconds(0.1f);

        Chest[] chests = FindObjectsOfType<Chest>();
        foreach (Chest chest in chests)
        {
            RegisterChest(chest, sceneName);
        }

        Debug.Log($"📦 Registered {chests.Length} chests in scene: {sceneName}");
    }

    public void RegisterChest(Chest chest, string sceneName = null)
    {
        if (chest == null) return;

        if (string.IsNullOrEmpty(sceneName))
            sceneName = SceneManager.GetActiveScene().name;

        if (!chestsByScene.ContainsKey(sceneName))
            chestsByScene[sceneName] = new List<Chest>();

        if (!chestsByScene[sceneName].Contains(chest))
        {
            chestsByScene[sceneName].Add(chest);
        }
    }

    public void OnChestOpened(Chest chest)
    {
        if (chest == null) return;

        ChestSaveData data = chest.GetSaveData();

        // Xóa data cũ nếu có
        openedChests.RemoveAll(c => c.chestID == data.chestID);

        // Thêm data mới
        openedChests.Add(data);

        Debug.Log($"📦 Chest opened and saved: {data.chestID}");
    }

    public List<ChestSaveData> GetAllChestsSaveData()
    {
        List<ChestSaveData> allChestsData = new List<ChestSaveData>();

        // Lấy tất cả chests còn trong scene
        Chest[] chests = FindObjectsOfType<Chest>();
        foreach (Chest chest in chests)
        {
            if (chest != null)
            {
                ChestSaveData data = chest.GetSaveData();
                allChestsData.Add(data);
            }
        }

        // Merge với opened chests đã lưu (nếu chưa có trong scene)
        foreach (ChestSaveData openedData in openedChests)
        {
            bool found = false;
            foreach (ChestSaveData data in allChestsData)
            {
                if (data.chestID == openedData.chestID)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                allChestsData.Add(openedData);
            }
        }

        Debug.Log($"💾 Collected {allChestsData.Count} chest save data");
        return allChestsData;
    }

    public void ClearAllData()
    {
        chestsByScene.Clear();
        openedChests.Clear();
        Debug.Log("🗑️ ChestManager data cleared");
    }
}
