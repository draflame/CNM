using UnityEngine;

/// <summary>
/// AI Decision System - Hệ thống ra quyết định thông minh cho enemy
/// Phân tích tình huống và chọn chiến thuật phù hợp
/// </summary>
public class AIDecisionMaker : MonoBehaviour
{
    [Header("AI Configuration")]
    [SerializeField] private float decisionInterval = 0.5f; // Cập nhật quyết định mỗi 0.5s
    [SerializeField] private bool showDebugInfo = true;
    
    // Thông tin về enemy
    private Enemy enemyScript;
    private float maxHealth;
    private float currentHealth;
    
    // Thông tin về player
    private knight player;
    private Transform playerTransform;
    
    // Decision timer
    private float decisionTimer = 0f;
    
    // Current tactical state
    public EnemyTactic CurrentTactic { get; private set; }
    public float ThreatLevel { get; private set; }
    
    // Tactical preferences
    [Header("Tactical Thresholds")]
    [SerializeField] private float lowHealthThreshold = 0.3f;      // 30% HP
    [SerializeField] private float aggressiveHealthThreshold = 0.7f; // 70% HP
    [SerializeField] private float closeDistance = 3f;
    [SerializeField] private float mediumDistance = 6f;
    [SerializeField] private float farDistance = 10f;

    public enum EnemyTactic
    {
        Patrol,        // Tuần tra bình thường
        Aggressive,    // Tấn công mạnh mẽ
        Defensive,     // Phòng thủ, giữ khoảng cách
        Flanking,      // Đi vòng, tìm góc tấn công
        Retreating,    // Rút lui, tìm hỗ trợ
        Ambushing      // Phục kích, chờ đợi
    }
    
    // Public method để các script khác lấy tactic hiện tại
    public EnemyTactic GetCurrentTactic()
    {
        return CurrentTactic;
    }

    private void Start()
    {
        enemyScript = GetComponent<Enemy>();
        player = FindFirstObjectByType<knight>();
        
        if (player != null)
            playerTransform = player.transform;
        
        CurrentTactic = EnemyTactic.Patrol;
    }

    private void Update()
    {
        if (player == null) return;
        
        decisionTimer += Time.deltaTime;
        
        if (decisionTimer >= decisionInterval)
        {
            decisionTimer = 0f;
            MakeDecision();
        }
    }

    /// <summary>
    /// Hàm chính ra quyết định - Phân tích tình huống và chọn tactics
    /// </summary>
    public void MakeDecision()
    {
        // Thu thập thông tin
        float healthPercent = GetHealthPercent();
        float playerHealthPercent = GetPlayerHealthPercent();
        float distance = GetDistanceToPlayer();
        bool playerIsAttacking = IsPlayerAttacking();
        int nearbyAllies = CountNearbyAllies();
        
        // Tính threat level (0-1)
        ThreatLevel = CalculateThreatLevel(healthPercent, playerHealthPercent, distance, playerIsAttacking);
        
        // DECISION TREE - Cây quyết định
        
        // 1. Nếu HP rất thấp và player còn khỏe → RÚT LUI
        if (healthPercent < lowHealthThreshold && playerHealthPercent > 0.5f)
        {
            CurrentTactic = EnemyTactic.Retreating;
        }
        // 2. Player yếu và mình ở gần → TẤN CÔNG MẠNH
        else if (playerHealthPercent < 0.4f && distance < closeDistance)
        {
            CurrentTactic = EnemyTactic.Aggressive;
        }
        // 3. Player đang tấn công và mình ở gần → PHÒNG THỦ
        else if (playerIsAttacking && distance < mediumDistance)
        {
            CurrentTactic = EnemyTactic.Defensive;
        }
        // 4. Có đồng minh gần → PHỐI HỢP ĐI VÒNG
        else if (nearbyAllies >= 2 && distance < mediumDistance)
        {
            CurrentTactic = EnemyTactic.Flanking;
        }
        // 5. Ở xa và HP cao → PHỤ KÍCH
        else if (distance > mediumDistance && healthPercent > aggressiveHealthThreshold)
        {
            CurrentTactic = EnemyTactic.Ambushing;
        }
        // 6. HP cao, player ở khoảng cách vừa → TẤN CÔNG
        else if (healthPercent > aggressiveHealthThreshold && distance < mediumDistance)
        {
            CurrentTactic = EnemyTactic.Aggressive;
        }
        // 7. Default → TUẦN TRA
        else
        {
            CurrentTactic = EnemyTactic.Patrol;
        }
        
        if (showDebugInfo)
        {
            DebugTactic();
        }
    }

    /// <summary>
    /// Tính toán mức độ nguy hiểm (0 = safe, 1 = very dangerous)
    /// </summary>
    private float CalculateThreatLevel(float myHP, float playerHP, float distance, bool playerAttacking)
    {
        float threat = 0f;
        
        // HP thấp → nguy hiểm cao
        threat += (1f - myHP) * 0.4f;
        
        // Player HP cao → nguy hiểm cao
        threat += playerHP * 0.2f;
        
        // Khoảng cách gần → nguy hiểm cao
        float distanceThreat = Mathf.Clamp01(1f - (distance / farDistance));
        threat += distanceThreat * 0.3f;
        
        // Player đang tấn công → nguy hiểm cao
        if (playerAttacking)
            threat += 0.1f;
        
        return Mathf.Clamp01(threat);
    }

    // ==================== HELPER FUNCTIONS ====================
    
    private float GetHealthPercent()
    {
        // Sẽ cần modify Enemy.cs để expose health
        // Tạm thời return 1f
        return 1f;
    }
    
    private float GetPlayerHealthPercent()
    {
        if (player == null) return 1f;
        // Cần access player.health / player.maxHealth
        return 1f;
    }
    
    private float GetDistanceToPlayer()
    {
        if (playerTransform == null) return 999f;
        return Vector2.Distance(transform.position, playerTransform.position);
    }
    
    private bool IsPlayerAttacking()
    {
        if (player == null) return false;
        // Cần check animator state hoặc attack flag
        return false;
    }
    
    private int CountNearbyAllies()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 8f);
        int count = 0;
        
        foreach (var col in colliders)
        {
            if (col.gameObject != gameObject && col.GetComponent<Enemy>() != null)
                count++;
        }
        
        return count;
    }

    /// <summary>
    /// Lấy vị trí dự đoán của player sau X giây
    /// </summary>
    public Vector2 PredictPlayerPosition(float timeAhead = 0.3f)
    {
        if (player == null) return Vector2.zero;
        
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb == null) return playerTransform.position;
        
        Vector2 currentPos = playerTransform.position;
        Vector2 velocity = playerRb.linearVelocity;
        
        // Dự đoán vị trí
        Vector2 predictedPos = currentPos + (velocity * timeAhead);
        
        return predictedPos;
    }

    /// <summary>
    /// Kiểm tra xem có nên tấn công không dựa trên tactics hiện tại
    /// </summary>
    public bool ShouldAttack()
    {
        return CurrentTactic == EnemyTactic.Aggressive || 
               CurrentTactic == EnemyTactic.Ambushing;
    }

    /// <summary>
    /// Kiểm tra xem có nên giữ khoảng cách không
    /// </summary>
    public bool ShouldKeepDistance()
    {
        return CurrentTactic == EnemyTactic.Defensive || 
               CurrentTactic == EnemyTactic.Retreating;
    }

    /// <summary>
    /// Lấy preferred distance dựa trên tactics
    /// </summary>
    public float GetPreferredDistance()
    {
        switch (CurrentTactic)
        {
            case EnemyTactic.Aggressive:
                return closeDistance;
            case EnemyTactic.Defensive:
            case EnemyTactic.Retreating:
                return mediumDistance;
            case EnemyTactic.Flanking:
                return closeDistance;
            case EnemyTactic.Ambushing:
                return mediumDistance;
            default:
                return mediumDistance;
        }
    }

    private void DebugTactic()
    {
        Debug.Log($"[{gameObject.name}] Tactic: {CurrentTactic} | Threat: {ThreatLevel:F2} | Dist: {GetDistanceToPlayer():F1}m");
    }

    // Visualize AI decisions trong Scene view
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        
        // Vẽ vòng tròn preferred distance
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, GetPreferredDistance());
        
        // Vẽ đường tới player
        if (playerTransform != null)
        {
            Gizmos.color = ThreatLevel > 0.5f ? Color.red : Color.green;
            Gizmos.DrawLine(transform.position, playerTransform.position);
            
            // Vẽ predicted position
            Vector2 predictedPos = PredictPlayerPosition(0.5f);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(predictedPos, 0.5f);
        }
    }
}
