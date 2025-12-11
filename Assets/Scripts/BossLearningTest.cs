using UnityEngine;

/// <summary>
/// Test Helper - Simulate player actions để test Boss Learning
/// Attach vào Player hoặc empty GameObject
/// </summary>
public class BossLearningTest : MonoBehaviour
{
    [Header("Test Controls")]
    [SerializeField] private KeyCode testDodgeLeftKey = KeyCode.Alpha1;
    [SerializeField] private KeyCode testDodgeRightKey = KeyCode.Alpha2;
    [SerializeField] private KeyCode testSkillKey = KeyCode.Alpha3;
    [SerializeField] private KeyCode testHealKey = KeyCode.Alpha4;
    [SerializeField] private KeyCode testResetKey = KeyCode.R;
    [SerializeField] private KeyCode testSummaryKey = KeyCode.L;
    
    [Header("References")]
    private BossLearningAI[] learningBosses;
    private bool hasScanned = false;
    private float nextScanTime = 0f;
    
    void Start()
    {
        Debug.Log("✅ BossLearningTest START - Script is running!");
        ScanForBosses();
    }
    
    void ScanForBosses()
    {
        learningBosses = FindObjectsByType<BossLearningAI>(FindObjectsSortMode.None);
        
        Debug.Log($"🔍 Found {learningBosses.Length} BossLearningAI component(s)");
        
        if (learningBosses.Length > 0)
        {
            hasScanned = true;
            Debug.Log("=== BOSS LEARNING TEST HELPER ===");
            Debug.Log($"Found {learningBosses.Length} learning boss(es)");
            Debug.Log("\nTest Controls:");
            Debug.Log($"  1 = Dodge LEFT");
            Debug.Log($"  2 = Dodge RIGHT");
            Debug.Log($"  3 = Use Skill (Tornado)");
            Debug.Log($"  4 = Heal");
            Debug.Log($"  R = Reset Learning");
            Debug.Log($"  L = Show Learning Summary");
            Debug.Log("\nPress keys to simulate player actions!");
        }
        else
        {
            Debug.LogWarning("⚠️ [Test] No learning bosses found yet. Will retry...");
        }
    }
    
    void Update()
    {
        // Auto re-scan nếu chưa tìm thấy bosses (chờ AutoAddBossLearning chạy)
        if (!hasScanned && Time.time >= nextScanTime)
        {
            nextScanTime = Time.time + 1f; // Scan mỗi 1 giây
            ScanForBosses();
        }
        
        if (learningBosses == null || learningBosses.Length == 0) 
        {
            return; // Không spam warning nữa
        }
        
        // Test dodge left
        if (Input.GetKeyDown(testDodgeLeftKey))
        {
            Debug.Log("🎮 [TEST] KEY PRESSED: 1 - Simulating Dodge LEFT");
            SimulateDodge(Vector2.left);
        }
        
        // Test dodge right
        if (Input.GetKeyDown(testDodgeRightKey))
        {
            Debug.Log("🎮 [TEST] KEY PRESSED: 2 - Simulating Dodge RIGHT");
            SimulateDodge(Vector2.right);
        }
        
        // Test skill
        if (Input.GetKeyDown(testSkillKey))
        {
            Debug.Log("🎮 [TEST] KEY PRESSED: 3 - Simulating Tornado Slash");
            SimulateSkill("Tornado Slash");
        }
        
        // Test heal
        if (Input.GetKeyDown(testHealKey))
        {
            Debug.Log("🎮 [TEST] KEY PRESSED: 4 - Simulating Heal at 32%");
            SimulateHeal(0.32f); // 32% HP
        }
        
        // Reset learning
        if (Input.GetKeyDown(testResetKey))
        {
            foreach (var boss in learningBosses)
            {
                boss.ResetLearning();
            }
            Debug.Log("🔄 [TEST] Reset all learning!");
        }
        
        // Show summary
        if (Input.GetKeyDown(testSummaryKey))
        {
            foreach (var boss in learningBosses)
            {
                Debug.Log(boss.GetLearningSummary());
            }
        }
    }
    
    void SimulateDodge(Vector2 direction)
    {
        foreach (var boss in learningBosses)
        {
            boss.OnPlayerDodge(direction);
        }
    }
    
    void SimulateSkill(string skillName)
    {
        foreach (var boss in learningBosses)
        {
            boss.OnPlayerUseSkill(skillName);
        }
    }
    
    void SimulateHeal(float healthPercent)
    {
        foreach (var boss in learningBosses)
        {
            boss.OnPlayerHeal(healthPercent);
        }
    }
    
    void OnGUI()
    {
        if (learningBosses.Length == 0) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Box("=== BOSS LEARNING TEST ===");
        
        GUILayout.Label("Press to simulate:");
        if (GUILayout.Button("1 = Dodge LEFT"))
            SimulateDodge(Vector2.left);
        
        if (GUILayout.Button("2 = Dodge RIGHT"))
            SimulateDodge(Vector2.right);
        
        if (GUILayout.Button("3 = Tornado Slash"))
            SimulateSkill("Tornado Slash");
        
        if (GUILayout.Button("4 = Heal"))
            SimulateHeal(0.32f);
        
        if (GUILayout.Button("R = Reset"))
        {
            foreach (var boss in learningBosses)
                boss.ResetLearning();
        }
        
        if (GUILayout.Button("L = Show Summary"))
        {
            foreach (var boss in learningBosses)
                Debug.Log(boss.GetLearningSummary());
        }
        
        GUILayout.EndArea();
    }
}
