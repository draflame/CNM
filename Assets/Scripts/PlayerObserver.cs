using UnityEngine;

/// <summary>
/// Player Observer - Ghi nhận hành động của player để Boss AI học
/// Attach vào Player GameObject
/// </summary>
public class PlayerObserver : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private bool enableObservation = true;
    [SerializeField] private bool showDebugLogs = true;
    
    private knight playerScript;
    private Rigidbody2D rb;
    private Vector2 lastPosition;
    private float lastHealth;
    private float nextScanTime = 0f;
    
    // Find all bosses with learning AI
    private BossLearningAI[] learningBosses;
    
    private void Start()
    {
        playerScript = GetComponent<knight>();
        rb = GetComponent<Rigidbody2D>();
        
        if (playerScript != null)
        {
            lastHealth = GetCurrentHealth();
        }
        
        lastPosition = transform.position;
        
        // Scan lần đầu
        ScanForLearningBosses();
        
        if (showDebugLogs)
            Debug.Log($"✅ [Player Observer] Initialized - Monitoring player actions for AI learning");
    }
    
    private void ScanForLearningBosses()
    {
        // Find all bosses that can learn
        learningBosses = FindObjectsByType<BossLearningAI>(FindObjectsSortMode.None);
        
        if (learningBosses.Length > 0)
        {
            if (showDebugLogs)
                Debug.Log($"🔍 [Player Observer] Found {learningBosses.Length} learning boss(es)");
        }
        else
        {
            if (showDebugLogs)
                Debug.LogWarning("⚠️ [Player Observer] No learning bosses found yet. Will retry...");
        }
    }
    
    private void Update()
    {
        if (!enableObservation || playerScript == null) return;
        
        // Re-scan nếu chưa tìm thấy boss
        if (learningBosses == null || learningBosses.Length == 0)
        {
            if (Time.time >= nextScanTime)
            {
                nextScanTime = Time.time + 2f; // Scan mỗi 2 giây
                ScanForLearningBosses();
            }
            return;
        }
        
        // Detect dodge by movement pattern
        DetectDodge();
        
        // Detect heal
        DetectHeal();
    }
    
    /// <summary>
    /// Detect dodge bằng movement spike
    /// </summary>
    private void DetectDodge()
    {
        if (rb == null) return;
        
        Vector2 velocity = rb.linearVelocity;
        
        // Phát hiện dodge qua velocity spike (tốc độ đột ngột cao)
        if (velocity.magnitude > 8f) // Threshold cho dash/dodge
        {
            Vector2 dodgeDirection = velocity.normalized;
            
            if (showDebugLogs)
                Debug.Log($"🏃 [Player Observer] Dodge detected! Direction: {dodgeDirection}");
            
            NotifyBosses_PlayerDodge(dodgeDirection);
        }
    }
    
    /// <summary>
    /// Detect healing
    /// </summary>
    private void DetectHeal()
    {
        float currentHealth = GetCurrentHealth();
        
        // Health increased = healed (threshold 0.5 HP thay vì 1)
        if (currentHealth > lastHealth + 0.5f)
        {
            float healthPercent = lastHealth / GetMaxHealth(); // % trước khi heal
            
            if (showDebugLogs)
                Debug.Log($"💚 [Player Observer] Heal detected! HP before: {healthPercent * 100:F0}% (from {lastHealth} to {currentHealth})");
            
            NotifyBosses_PlayerHeal(healthPercent);
        }
        
        lastHealth = currentHealth;
    }
    
    // ========================================
    // PUBLIC METHODS - Gọi từ Player scripts
    // ========================================
    
    /// <summary>
    /// Gọi khi player dùng skill
    /// VD: Trong SkillBase.cs → playerObserver.OnSkillUsed(skillName)
    /// </summary>
    public void OnSkillUsed(string skillName)
    {
        if (!enableObservation) return;
        
        if (showDebugLogs)
            Debug.Log($"⚔️ [Player Observer] Skill used: {skillName}");
        
        NotifyBosses_PlayerSkill(skillName);
    }
    
    /// <summary>
    /// Gọi khi player dodge (nếu có dodge system rõ ràng)
    /// </summary>
    public void OnDodge(Vector2 direction)
    {
        if (!enableObservation) return;
        
        if (showDebugLogs)
            Debug.Log($"🏃 [Player Observer] Dodge called: {direction}");
        
        NotifyBosses_PlayerDodge(direction);
    }
    
    /// <summary>
    /// Gọi khi player block/shield
    /// </summary>
    public void OnBlock()
    {
        if (!enableObservation) return;
        
        NotifyBosses_PlayerBlock();
    }
    
    /// <summary>
    /// Gọi khi player heal (với healthPercent trước khi heal)
    /// </summary>
    public void OnHeal(float healthPercentBeforeHeal)
    {
        if (!enableObservation) return;
        
        NotifyBosses_PlayerHeal(healthPercentBeforeHeal);
    }
    
    /// <summary>
    /// Gọi khi player attack
    /// </summary>
    public void OnAttack(string attackType)
    {
        if (!enableObservation) return;
        
        // NotifyBosses... (có thể thêm nếu cần)
    }
    
    // ========================================
    // NOTIFY BOSSES
    // ========================================
    
    private void NotifyBosses_PlayerDodge(Vector2 direction)
    {
        foreach (var boss in learningBosses)
        {
            if (boss != null && boss.enabled)
            {
                boss.OnPlayerDodge(direction);
            }
        }
    }
    
    private void NotifyBosses_PlayerSkill(string skillName)
    {
        foreach (var boss in learningBosses)
        {
            if (boss != null && boss.enabled)
            {
                boss.OnPlayerUseSkill(skillName);
            }
        }
    }
    
    private void NotifyBosses_PlayerHeal(float healthPercent)
    {
        foreach (var boss in learningBosses)
        {
            if (boss != null && boss.enabled)
            {
                boss.OnPlayerHeal(healthPercent);
            }
        }
    }
    
    private void NotifyBosses_PlayerBlock()
    {
        foreach (var boss in learningBosses)
        {
            if (boss != null && boss.enabled)
            {
                boss.OnPlayerBlock();
            }
        }
    }
    
    // ========================================
    // HELPER METHODS
    // ========================================
    
    private float GetCurrentHealth()
    {
        if (playerScript != null)
            return playerScript.GetCurrentHealth();
        return 100f;
    }
    
    private float GetMaxHealth()
    {
        if (playerScript != null)
            return playerScript.GetMaxHealth();
        return 100f;
    }
}
