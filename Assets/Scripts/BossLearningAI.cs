using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Boss Learning AI - Boss học pattern của player và thích nghi
/// Ghi nhớ behavior của player và đổi tactics để counter
/// </summary>
public class BossLearningAI : MonoBehaviour
{
    [Header("Learning Settings")]
    [SerializeField] private bool enableLearning = true;
    [SerializeField] private float learningSpeed = 1f; // 0.5 = slow, 2 = fast
    [SerializeField] private int minObservationsToLearn = 3; // Cần observe bao nhiêu lần
    [SerializeField] private bool showLearningDebug = true;
    
    [Header("Adaptation Settings")]
    [SerializeField] private float adaptationDelay = 2f; // Delay trước khi adapt (tránh quá nhanh)
    
    // Pattern tracking
    private Dictionary<string, int> playerPatterns = new Dictionary<string, int>();
    private Dictionary<string, float> playerTimings = new Dictionary<string, float>();
    private Queue<string> recentActions = new Queue<string>(); // Last 10 actions
    
    // Learning state
    private bool hasLearnedDodgePattern = false;
    private bool hasLearnedComboPattern = false;
    private bool hasLearnedHealPattern = false;
    private bool hasLearnedSkillSpam = false;
    
    // Predicted behaviors
    private Vector2 predictedDodgeDirection = Vector2.zero;
    private string mostUsedSkill = "";
    private float playerHealThreshold = 0.3f; // Default 30%
    private string predictedNextMove = "";
    
    // Adaptation modifiers
    private float currentAggressiveness = 1f;
    private float currentDistance = 5f;
    
    // References
    private knight player;
    private AIDecisionMaker aiDecision;
    private SmartAttackAI attackAI;
    
    private void Start()
    {
        player = FindFirstObjectByType<knight>();
        aiDecision = GetComponent<AIDecisionMaker>();
        attackAI = GetComponent<SmartAttackAI>();
        
        // Initialize pattern dictionary
        InitializePatternTracking();
    }
    
    private void InitializePatternTracking()
    {
        // Dodge patterns
        playerPatterns["dodgeLeft"] = 0;
        playerPatterns["dodgeRight"] = 0;
        playerPatterns["dodgeBackward"] = 0;
        
        // Attack patterns
        playerPatterns["lightAttack"] = 0;
        playerPatterns["heavyAttack"] = 0;
        playerPatterns["comboFinisher"] = 0;
        
        // Skill usage
        playerPatterns["tornadoSlash"] = 0;
        playerPatterns["heavySlash"] = 0;
        playerPatterns["rampageSlash"] = 0;
        playerPatterns["infernoSkill"] = 0;
        
        // Defensive patterns
        playerPatterns["shield"] = 0;
        playerPatterns["heal"] = 0;
        playerPatterns["retreat"] = 0;
        
        // Timing patterns
        playerTimings["attackAfterDodge"] = 0f;
        playerTimings["dodgeAfterAttack"] = 0f;
        playerTimings["averageComboLength"] = 1f;
    }
    
    private void Update()
    {
        if (!enableLearning || player == null) return;
        
        // Observe player behavior
        ObservePlayerBehavior();
        
        // Analyze patterns periodically
        if (Time.frameCount % 120 == 0) // Every 2 seconds
        {
            AnalyzePatternsAndAdapt();
        }
    }
    
    // ========================================
    // OBSERVATION - Ghi nhận hành động player
    // ========================================
    
    private void ObservePlayerBehavior()
    {
        // Được gọi từ các events hoặc polling
        // Trong thực tế, bạn cần hook vào player actions
    }
    
    /// <summary>
    /// Gọi từ bên ngoài khi player dodge
    /// </summary>
    public void OnPlayerDodge(Vector2 direction)
    {
        if (!enableLearning) return;
        
        // Record dodge direction
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            if (direction.x < 0)
            {
                IncrementPattern("dodgeLeft");
                RecordAction("dodgeLeft");
            }
            else
            {
                IncrementPattern("dodgeRight");
                RecordAction("dodgeRight");
            }
        }
        else
        {
            IncrementPattern("dodgeBackward");
            RecordAction("dodgeBackward");
        }
    }
    
    /// <summary>
    /// Gọi khi player dùng skill
    /// </summary>
    public void OnPlayerUseSkill(string skillName)
    {
        if (!enableLearning) return;
        
        string patternKey = skillName.ToLower().Replace(" ", "");
        IncrementPattern(patternKey);
        RecordAction(patternKey);
        
        if (showLearningDebug)
            Debug.Log($"📝 [Boss Learning] Player used: {skillName} → Pattern key: {patternKey} (Count: {GetPatternCount(patternKey)})");
    }
    
    /// <summary>
    /// Gọi khi player heal
    /// </summary>
    public void OnPlayerHeal(float healthPercent)
    {
        if (!enableLearning) return;
        
        IncrementPattern("heal");
        RecordAction("heal");
        
        // Learn heal threshold
        if (healthPercent < playerHealThreshold)
        {
            playerHealThreshold = (playerHealThreshold + healthPercent) / 2f;
            
            if (showLearningDebug)
                Debug.Log($"[Boss Learning] Player heals at {playerHealThreshold * 100:F0}% HP");
        }
    }
    
    /// <summary>
    /// Gọi khi player block/shield
    /// </summary>
    public void OnPlayerBlock()
    {
        if (!enableLearning) return;
        
        IncrementPattern("shield");
        RecordAction("shield");
    }
    
    private void IncrementPattern(string patternKey)
    {
        if (!playerPatterns.ContainsKey(patternKey))
            playerPatterns[patternKey] = 0;
        
        playerPatterns[patternKey]++;
    }
    
    private void RecordAction(string action)
    {
        recentActions.Enqueue(action);
        
        // Keep only last 10 actions
        if (recentActions.Count > 10)
            recentActions.Dequeue();
    }
    
    // ========================================
    // ANALYSIS - Phân tích patterns
    // ========================================
    
    private void AnalyzePatternsAndAdapt()
    {
        // 1. Analyze dodge patterns
        AnalyzeDodgePatterns();
        
        // 2. Analyze skill usage
        AnalyzeSkillUsage();
        
        // 3. Analyze combo patterns
        AnalyzeComboPatterns();
        
        // 4. Analyze defensive behavior
        AnalyzeDefensiveBehavior();
        
        // 5. Apply adaptations
        ApplyAdaptations();
    }
    
    private void AnalyzeDodgePatterns()
    {
        int leftDodges = GetPatternCount("dodgeLeft");
        int rightDodges = GetPatternCount("dodgeRight");
        int backDodges = GetPatternCount("dodgeBackward");
        
        int totalDodges = leftDodges + rightDodges + backDodges;
        
        if (totalDodges >= minObservationsToLearn)
        {
            // Player có tendency dodge về hướng nào?
            if (leftDodges > rightDodges + 3 && leftDodges > backDodges)
            {
                predictedDodgeDirection = Vector2.right; // Aim right to catch player
                hasLearnedDodgePattern = true;
                
                if (showLearningDebug)
                    Debug.Log($"🧠 [Boss Learned] Player favors LEFT dodge → Aim RIGHT!");
            }
            else if (rightDodges > leftDodges + 3 && rightDodges > backDodges)
            {
                predictedDodgeDirection = Vector2.left;
                hasLearnedDodgePattern = true;
                
                if (showLearningDebug)
                    Debug.Log($"🧠 [Boss Learned] Player favors RIGHT dodge → Aim LEFT!");
            }
            else if (backDodges > leftDodges + 2 && backDodges > rightDodges + 2)
            {
                // Player hay lùi → Boss advance faster
                currentAggressiveness = 1.3f;
                
                if (showLearningDebug)
                    Debug.Log($"🧠 [Boss Learned] Player retreats often → ADVANCE!");
            }
        }
    }
    
    private void AnalyzeSkillUsage()
    {
        // Tìm skill được dùng nhiều nhất
        int maxUsage = 0;
        string mostUsed = "";
        
        // Chữ thường, không dấu cách (khớp với ToLower().Replace(" ", ""))
        string[] skills = { "tornadoslash", "heavyslash", "rampageslash", "infernoskill" };
        
        foreach (string skill in skills)
        {
            int usage = GetPatternCount(skill);
            if (usage > maxUsage)
            {
                maxUsage = usage;
                mostUsed = skill;
            }
        }
        
        if (showLearningDebug)
            Debug.Log($"🔍 [Boss Analysis] Checking skill usage... Max: {maxUsage} ({mostUsed})");
        
        if (maxUsage >= minObservationsToLearn)
        {
            mostUsedSkill = mostUsed;
            hasLearnedSkillSpam = true;
            
            // Counter strategies
            if (mostUsed == "tornadoslash")
            {
                // Tornado có range → Keep distance
                currentDistance = 8f;
                
                if (showLearningDebug)
                    Debug.Log($"🧠 [Boss Learned] Player spams Tornado → KEEP DISTANCE!");
            }
            else if (mostUsed == "heavyslash")
            {
                // Heavy attack chậm → Interrupt
                currentAggressiveness = 1.4f;
                
                if (showLearningDebug)
                    Debug.Log($"🧠 [Boss Learned] Player uses Heavy → INTERRUPT!");
            }
            else if (mostUsed == "rampageslash")
            {
                // Rampage = close range → Use ranged
                currentDistance = 6f;
                
                if (showLearningDebug)
                    Debug.Log($"🧠 [Boss Learned] Player uses Rampage → RANGED ATTACK!");
            }
        }
    }
    
    private void AnalyzeComboPatterns()
    {
        // Phân tích combo length và patterns
        // TODO: Implement combo detection
        
        int lightAttacks = GetPatternCount("lightAttack");
        int heavyAttacks = GetPatternCount("heavyAttack");
        
        if (lightAttacks > heavyAttacks * 3)
        {
            // Player spam light attacks → Wait for opening
            hasLearnedComboPattern = true;
            
            if (showLearningDebug)
                Debug.Log($"🧠 [Boss Learned] Player spams light attacks → WAIT & COUNTER!");
        }
    }
    
    private void AnalyzeDefensiveBehavior()
    {
        int shieldUsage = GetPatternCount("shield");
        int retreats = GetPatternCount("retreat");
        int heals = GetPatternCount("heal");
        
        // Player defensive → Be more aggressive
        if (shieldUsage > 5 || retreats > 5)
        {
            currentAggressiveness = 1.5f;
            
            if (showLearningDebug)
                Debug.Log($"🧠 [Boss Learned] Player plays defensive → AGGRESSIVE MODE!");
        }
        
        // Player heals at threshold
        if (heals >= 2)
        {
            hasLearnedHealPattern = true;
            
            if (showLearningDebug)
                Debug.Log($"🧠 [Boss Learned] Player heals at {playerHealThreshold * 100:F0}% → PRESSURE BEFORE HEAL!");
        }
    }
    
    // ========================================
    // ADAPTATION - Apply learned behaviors
    // ========================================
    
    private void ApplyAdaptations()
    {
        // Adjust AI decision maker
        if (aiDecision != null)
        {
            // Adjust preferred distance based on learning
            // aiDecision.SetPreferredDistance(currentDistance);
        }
        
        // Adjust attack AI
        if (attackAI != null && hasLearnedDodgePattern)
        {
            // Tell attack AI về predicted dodge direction
            // attackAI.SetPredictedDodgeDirection(predictedDodgeDirection);
        }
    }
    
    /// <summary>
    /// Boss thay đổi tactics dựa trên learning
    /// </summary>
    public AIDecisionMaker.EnemyTactic GetAdaptedTactic(AIDecisionMaker.EnemyTactic originalTactic)
    {
        if (!enableLearning) return originalTactic;
        
        // Override tactics based on learning
        
        // Nếu player hay heal và HP thấp → Rush để prevent heal
        if (hasLearnedHealPattern && player != null)
        {
            float playerHP = GetPlayerHealthPercent();
            
            if (playerHP <= playerHealThreshold + 0.1f && playerHP > playerHealThreshold - 0.05f)
            {
                // Player sắp heal → Aggressive!
                if (showLearningDebug)
                    Debug.Log($"🧠 [Boss Adapting] Player near heal threshold → RUSH!");
                
                return AIDecisionMaker.EnemyTactic.Aggressive;
            }
        }
        
        // Nếu player spam skills → Counter tactic
        if (hasLearnedSkillSpam && mostUsedSkill == "tornadoSlash")
        {
            float distance = GetDistanceToPlayer();
            
            if (distance < 6f)
            {
                // Too close to tornado spammer → Back off
                return AIDecisionMaker.EnemyTactic.Defensive;
            }
        }
        
        return originalTactic;
    }
    
    /// <summary>
    /// Boss aim vào vị trí dự đoán thay vì vị trí hiện tại
    /// </summary>
    public Vector2 GetPredictedPlayerPosition()
    {
        if (!hasLearnedDodgePattern || player == null)
            return player.transform.position;
        
        // Dự đoán player sẽ dodge về đâu
        Vector2 currentPos = player.transform.position;
        Vector2 predictedPos = currentPos + predictedDodgeDirection * 2f; // 2m offset
        
        return predictedPos;
    }
    
    /// <summary>
    /// Boss có nên interrupt player không?
    /// </summary>
    public bool ShouldInterruptPlayer()
    {
        if (!hasLearnedSkillSpam) return false;
        
        // Nếu player hay dùng heavy/slow skills → Interrupt!
        if (mostUsedSkill == "heavySlash" || mostUsedSkill == "rampageSlash")
        {
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Get aggressiveness modifier based on learning
    /// </summary>
    public float GetAggressivenessModifier()
    {
        return currentAggressiveness;
    }
    
    /// <summary>
    /// Get preferred distance based on learning
    /// </summary>
    public float GetPreferredDistance()
    {
        return currentDistance;
    }
    
    // ========================================
    // HELPER FUNCTIONS
    // ========================================
    
    private int GetPatternCount(string patternKey)
    {
        return playerPatterns.ContainsKey(patternKey) ? playerPatterns[patternKey] : 0;
    }
    
    private float GetPlayerHealthPercent()
    {
        if (player == null) return 1f;
        // TODO: Get actual player health percent
        return 1f;
    }
    
    private float GetDistanceToPlayer()
    {
        if (player == null) return 999f;
        return Vector2.Distance(transform.position, player.transform.position);
    }
    
    /// <summary>
    /// Reset learning (cho boss phases mới hoặc restart)
    /// </summary>
    public void ResetLearning()
    {
        List<string> keys = new List<string>(playerPatterns.Keys);
        foreach (var key in keys)
        {
            playerPatterns[key] = 0;
        }
        
        recentActions.Clear();
        
        hasLearnedDodgePattern = false;
        hasLearnedComboPattern = false;
        hasLearnedHealPattern = false;
        hasLearnedSkillSpam = false;
        
        currentAggressiveness = 1f;
        currentDistance = 5f;
        
        if (showLearningDebug)
            Debug.Log("🔄 [Boss Learning] Reset!");
    }
    
    /// <summary>
    /// Get learning summary for debug
    /// </summary>
    public string GetLearningSummary()
    {
        string summary = "=== BOSS LEARNING SUMMARY ===\n";
        
        summary += $"Learned Dodge Pattern: {hasLearnedDodgePattern}\n";
        if (hasLearnedDodgePattern)
            summary += $"  → Predicted Direction: {predictedDodgeDirection}\n";
        
        summary += $"Learned Skill Spam: {hasLearnedSkillSpam}\n";
        if (hasLearnedSkillSpam)
            summary += $"  → Most Used Skill: {mostUsedSkill}\n";
        
        summary += $"Learned Heal Pattern: {hasLearnedHealPattern}\n";
        if (hasLearnedHealPattern)
            summary += $"  → Heal Threshold: {playerHealThreshold * 100:F0}%\n";
        
        summary += $"\nAdaptations:\n";
        summary += $"  Aggressiveness: {currentAggressiveness:F2}x\n";
        summary += $"  Preferred Distance: {currentDistance:F1}m\n";
        
        return summary;
    }
    
    // Visualize learning in Scene view
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || player == null) return;
        
        // Draw predicted player position
        if (hasLearnedDodgePattern)
        {
            Vector2 predictedPos = GetPredictedPlayerPosition();
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(predictedPos, 0.8f);
            Gizmos.DrawLine(player.transform.position, predictedPos);
        }
        
        // Draw preferred distance
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, currentDistance);
    }
}
