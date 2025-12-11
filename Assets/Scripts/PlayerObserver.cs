using UnityEngine;

/// <summary>
/// Player Observer - Ghi nhận hành động của player để Boss AI học
/// Attach vào Player GameObject
/// </summary>
public class PlayerObserver : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private bool enableObservation = true;
    
    private knight playerScript;
    private Rigidbody2D rb;
    private Vector2 lastPosition;
    private float lastHealth;
    
    // Find all bosses with learning AI
    private BossLearningAI[] learningBosses;
    
    private void Start()
    {
        playerScript = GetComponent<knight>();
        rb = GetComponent<Rigidbody2D>();
        
        // Find all bosses that can learn
        learningBosses = FindObjectsByType<BossLearningAI>(FindObjectsSortMode.None);
        
        if (learningBosses.Length > 0)
        {
            Debug.Log($"[Player Observer] Found {learningBosses.Length} learning bosses");
        }
        
        if (playerScript != null)
        {
            lastHealth = GetCurrentHealth();
        }
        
        lastPosition = transform.position;
    }
    
    private void Update()
    {
        if (!enableObservation || playerScript == null) return;
        
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
        Vector2 currentPos = transform.position;
        Vector2 movement = currentPos - lastPosition;
        
        // Nếu di chuyển đột ngột trong 1 frame (dash/dodge)
        if (movement.magnitude > 3f * Time.deltaTime)
        {
            Vector2 dodgeDirection = movement.normalized;
            NotifyBosses_PlayerDodge(dodgeDirection);
        }
        
        lastPosition = currentPos;
    }
    
    /// <summary>
    /// Detect healing
    /// </summary>
    private void DetectHeal()
    {
        float currentHealth = GetCurrentHealth();
        
        // Health increased = healed
        if (currentHealth > lastHealth + 1f)
        {
            float healthPercent = GetHealthPercent();
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
        
        NotifyBosses_PlayerSkill(skillName);
    }
    
    /// <summary>
    /// Gọi khi player dodge (nếu có dodge system rõ ràng)
    /// </summary>
    public void OnDodge(Vector2 direction)
    {
        if (!enableObservation) return;
        
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
    /// Gọi khi player heal
    /// </summary>
    public void OnHeal()
    {
        if (!enableObservation) return;
        
        float healthPercent = GetHealthPercent();
        NotifyBosses_PlayerHeal(healthPercent);
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
        // TODO: Get actual player health
        // playerScript.health hoặc playerScript.GetCurrentHealth()
        return 100f;
    }
    
    private float GetHealthPercent()
    {
        // TODO: Get actual player health percent
        // playerScript.GetHealthPercent()
        return 1f;
    }
}
