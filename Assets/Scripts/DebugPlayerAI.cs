using UnityEngine;

/// <summary>
/// Debug helper - Kiểm tra PlayerObserver có hoạt động không
/// Attach vào empty GameObject để test
/// </summary>
public class DebugPlayerAI : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("=== DEBUG PLAYER AI INTEGRATION ===");
        
        // Tìm Player
        knight player = FindFirstObjectByType<knight>();
        if (player == null)
        {
            Debug.LogError("❌ Player (knight) not found!");
            return;
        }
        Debug.Log($"✅ Player found: {player.gameObject.name}");
        
        // Kiểm tra PlayerObserver
        PlayerObserver observer = player.GetComponent<PlayerObserver>();
        if (observer == null)
        {
            Debug.LogError("❌ PlayerObserver NOT found on player!");
            Debug.Log("→ Adding PlayerObserver manually...");
            observer = player.gameObject.AddComponent<PlayerObserver>();
        }
        else
        {
            Debug.Log($"✅ PlayerObserver found on player");
        }
        
        // Kiểm tra Boss
        BossLearningAI[] bosses = FindObjectsByType<BossLearningAI>(FindObjectsSortMode.None);
        if (bosses.Length == 0)
        {
            Debug.LogError("❌ No BossLearningAI found!");
        }
        else
        {
            Debug.Log($"✅ Found {bosses.Length} boss(es) with BossLearningAI");
            foreach (var boss in bosses)
            {
                Debug.Log($"  → {boss.gameObject.name}");
            }
        }
        
        Debug.Log("\n📝 Integration Status:");
        Debug.Log($"  Player: {(player != null ? "✅" : "❌")}");
        Debug.Log($"  PlayerObserver: {(observer != null ? "✅" : "❌")}");
        Debug.Log($"  Learning Bosses: {bosses.Length}");
        
        if (player != null && observer != null && bosses.Length > 0)
        {
            Debug.Log("\n🎉 Everything is set up correctly!");
            Debug.Log("Now play the game and use skills/dodge/heal");
            Debug.Log("You should see logs like:");
            Debug.Log("  ⚔️ [Player Observer] Skill used: ...");
            Debug.Log("  🏃 [Player Observer] Dodge called: ...");
            Debug.Log("  💚 [Player Observer] Heal detected: ...");
        }
        
        // Self destroy
        Destroy(gameObject, 2f);
    }
    
    private void Update()
    {
        // Test manual skill notification
        if (Input.GetKeyDown(KeyCode.T))
        {
            knight player = FindFirstObjectByType<knight>();
            if (player != null)
            {
                PlayerObserver observer = player.GetComponent<PlayerObserver>();
                if (observer != null)
                {
                    Debug.Log("🧪 [DEBUG] Manually triggering skill notification...");
                    observer.OnSkillUsed("Test Skill");
                }
            }
        }
    }
}
