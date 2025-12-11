using UnityEngine;

/// <summary>
/// Auto setup AI components cho boss
/// Chạy 1 lần rồi tự destroy
/// </summary>
public class AutoSetupBossAI : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("🔧 [Auto Setup] Setting up Boss AI components...");
        
        // Tìm tất cả boss
        BringerOfDeath[] bosses = FindObjectsByType<BringerOfDeath>(FindObjectsSortMode.None);
        
        foreach (var boss in bosses)
        {
            Debug.Log($"📋 [Setup] Checking {boss.gameObject.name}...");
            
            // 1. Check BossLearningAI
            BossLearningAI learningAI = boss.GetComponent<BossLearningAI>();
            if (learningAI == null)
            {
                learningAI = boss.gameObject.AddComponent<BossLearningAI>();
                Debug.Log($"  ✅ Added BossLearningAI");
            }
            else
            {
                Debug.Log($"  ✓ BossLearningAI already exists");
            }
            
            // 2. Check AIDecisionMaker
            AIDecisionMaker aiDecision = boss.GetComponent<AIDecisionMaker>();
            if (aiDecision == null)
            {
                aiDecision = boss.gameObject.AddComponent<AIDecisionMaker>();
                Debug.Log($"  ✅ Added AIDecisionMaker");
            }
            else
            {
                Debug.Log($"  ✓ AIDecisionMaker already exists");
            }
        }
        
        if (bosses.Length > 0)
        {
            Debug.Log($"🎉 [Setup] Complete! {bosses.Length} boss(es) configured");
        }
        else
        {
            Debug.LogWarning("⚠️ [Setup] No BringerOfDeath found in scene");
        }
        
        // Self destroy
        Destroy(gameObject, 1f);
    }
}
