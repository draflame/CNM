using UnityEngine;

/// <summary>
/// Helper: Tự động add BossLearningAI vào tất cả boss trong scene
/// Chỉ chạy 1 lần trong Editor
/// </summary>
public class AutoAddBossLearning : MonoBehaviour
{
    [Header("Auto Setup")]
    [Tooltip("Nhấn Play để tự động add BossLearningAI vào tất cả boss")]
    [SerializeField] private bool autoAddOnStart = true;
    
    void Start()
    {
        if (!autoAddOnStart) return;
        
        Debug.Log("🔍 Searching for bosses to add BossLearningAI...");
        
        // Tìm tất cả boss (BringerOfDeath)
        BringerOfDeath[] bosses = FindObjectsByType<BringerOfDeath>(FindObjectsSortMode.None);
        
        int addedCount = 0;
        foreach (var boss in bosses)
        {
            // Kiểm tra đã có BossLearningAI chưa
            if (boss.GetComponent<BossLearningAI>() == null)
            {
                // Add component
                boss.gameObject.AddComponent<BossLearningAI>();
                Debug.Log($"✅ Added BossLearningAI to: {boss.gameObject.name}");
                addedCount++;
            }
            else
            {
                Debug.Log($"⚠️ {boss.gameObject.name} already has BossLearningAI");
            }
        }
        
        if (addedCount > 0)
        {
            Debug.Log($"🎉 Successfully added BossLearningAI to {addedCount} boss(es)!");
        }
        else if (bosses.Length == 0)
        {
            Debug.LogWarning("⚠️ No BringerOfDeath found in scene!");
        }
        
        // Tự destroy sau khi setup xong
        Destroy(this.gameObject, 0.5f);
    }
}
