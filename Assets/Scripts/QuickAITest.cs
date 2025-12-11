using UnityEngine;

/// <summary>
/// Test script - Attach vào enemy để test AI nhanh
/// Tự động add AI components khi game start
/// </summary>
public class QuickAITest : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool autoAddAIComponents = true;
    [SerializeField] private bool showDebugLogs = true;
    
    private void Start()
    {
        if (autoAddAIComponents)
        {
            TestAddAIComponents();
        }
    }
    
    private void TestAddAIComponents()
    {
        // Test 1: Check if this is an Enemy
        Enemy enemy = GetComponent<Enemy>();
        if (enemy == null)
        {
            Debug.LogError("[QuickAITest] This GameObject is not an Enemy!");
            return;
        }
        
        if (showDebugLogs)
            Debug.Log($"[QuickAITest] Found Enemy component on {gameObject.name}");
        
        // Test 2: Add AIDecisionMaker if not exists
        AIDecisionMaker decision = GetComponent<AIDecisionMaker>();
        if (decision == null)
        {
            decision = gameObject.AddComponent<AIDecisionMaker>();
            if (showDebugLogs)
                Debug.Log($"[QuickAITest] ✓ Added AIDecisionMaker to {gameObject.name}");
        }
        else
        {
            if (showDebugLogs)
                Debug.Log($"[QuickAITest] AIDecisionMaker already exists");
        }
        
        // Test 3: Add SmartMovementAI if not exists
        SmartMovementAI movement = GetComponent<SmartMovementAI>();
        if (movement == null)
        {
            movement = gameObject.AddComponent<SmartMovementAI>();
            if (showDebugLogs)
                Debug.Log($"[QuickAITest] ✓ Added SmartMovementAI to {gameObject.name}");
        }
        else
        {
            if (showDebugLogs)
                Debug.Log($"[QuickAITest] SmartMovementAI already exists");
        }
        
        // Test 4: Add SmartAttackAI if not exists
        SmartAttackAI attack = GetComponent<SmartAttackAI>();
        if (attack == null)
        {
            attack = gameObject.AddComponent<SmartAttackAI>();
            if (showDebugLogs)
                Debug.Log($"[QuickAITest] ✓ Added SmartAttackAI to {gameObject.name}");
        }
        else
        {
            if (showDebugLogs)
                Debug.Log($"[QuickAITest] SmartAttackAI already exists");
        }
        
        // Test 5: Verify Enemy health methods
        float healthPercent = enemy.GetHealthPercent();
        if (showDebugLogs)
        {
            Debug.Log($"[QuickAITest] ✓ Enemy.GetHealthPercent() works! HP: {healthPercent * 100}%");
            Debug.Log($"[QuickAITest] Current HP: {enemy.GetCurrentHealth()} / {enemy.GetMaxHealth()}");
        }
        
        // Final summary
        if (showDebugLogs)
        {
            Debug.Log($"[QuickAITest] ════════════════════════════════");
            Debug.Log($"[QuickAITest] ✅ AI SETUP COMPLETE for {gameObject.name}!");
            Debug.Log($"[QuickAITest] Components: AIDecisionMaker ✓ SmartMovementAI ✓ SmartAttackAI ✓");
            Debug.Log($"[QuickAITest] Enemy HP system: WORKING ✓");
            Debug.Log($"[QuickAITest] ════════════════════════════════");
        }
    }
    
    private void Update()
    {
        // Test AI decision making every 2 seconds
        if (showDebugLogs && Time.frameCount % 120 == 0) // Every 2s at 60fps
        {
            AIDecisionMaker decision = GetComponent<AIDecisionMaker>();
            if (decision != null)
            {
                Debug.Log($"[QuickAITest] Current Tactic: {decision.CurrentTactic}, Threat: {decision.ThreatLevel:F2}");
            }
        }
    }
}
