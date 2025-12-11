using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Smart Attack Pattern AI - Hệ thống tấn công thông minh
/// Tránh spam, vary attacks, predict player, combo attacks
/// </summary>
public class SmartAttackAI : MonoBehaviour
{
    [Header("References")]
    private AIDecisionMaker decisionMaker;
    private Animator animator;
    
    [Header("Attack Pattern Settings")]
    [SerializeField] private bool useSmartPatterns = true;
    [SerializeField] private int patternLength = 3; // Số attacks trong 1 combo
    [SerializeField] private float comboDelay = 0.3f;
    
    [Header("Attack Types")]
    [SerializeField] private List<AttackData> availableAttacks = new List<AttackData>();
    
    private Queue<AttackData> currentPattern = new Queue<AttackData>();
    private AttackData lastUsedAttack;
    private float lastAttackTime = 0f;
    
    // Track player behavior
    private Dictionary<string, int> playerBehaviorCount = new Dictionary<string, int>
    {
        {"dodgeLeft", 0},
        {"dodgeRight", 0},
        {"block", 0},
        {"jump", 0}
    };

    [System.Serializable]
    public class AttackData
    {
        public string attackName;
        public string animationTrigger;
        public float damage;
        public float range;
        public float cooldown;
        public AttackType type;
        
        [HideInInspector] public float lastUsedTime = 0f;
    }

    public enum AttackType
    {
        Quick,      // Nhanh, damage thấp
        Heavy,      // Chậm, damage cao
        Ranged,     // Tấn công từ xa
        Area,       // AOE
        Grab        // Counter block
    }

    private void Start()
    {
        decisionMaker = GetComponent<AIDecisionMaker>();
        animator = GetComponent<Animator>();
        
        // Tạo default attacks nếu chưa có
        if (availableAttacks.Count == 0)
        {
            availableAttacks.Add(new AttackData 
            { 
                attackName = "QuickSlash", 
                animationTrigger = "Attack", 
                damage = 10f, 
                range = 2f, 
                cooldown = 1f,
                type = AttackType.Quick
            });
            
            availableAttacks.Add(new AttackData 
            { 
                attackName = "HeavyStrike", 
                animationTrigger = "HeavyAttack", 
                damage = 25f, 
                range = 2.5f, 
                cooldown = 2f,
                type = AttackType.Heavy
            });
        }
    }

    /// <summary>
    /// Quyết định attack tiếp theo dựa trên context
    /// </summary>
    public AttackData DecideNextAttack()
    {
        if (!useSmartPatterns)
        {
            return GetRandomAvailableAttack();
        }
        
        // Filter attacks sẵn dùng (không còn cooldown)
        List<AttackData> readyAttacks = GetReadyAttacks();
        if (readyAttacks.Count == 0)
            return null;
        
        // Analyze context
        knight player = FindFirstObjectByType<knight>();
        if (player == null)
            return readyAttacks[Random.Range(0, readyAttacks.Count)];
        
        float distance = Vector2.Distance(transform.position, player.transform.position);
        bool playerBlocking = IsPlayerBlocking();
        bool playerLowHealth = GetPlayerHealthPercent() < 0.3f;
        
        // SMART SELECTION based on context
        
        // 1. Player đang block → dùng Grab attack
        if (playerBlocking)
        {
            var grabAttack = readyAttacks.Find(a => a.type == AttackType.Grab);
            if (grabAttack != null)
                return grabAttack;
        }
        
        // 2. Player HP thấp và ở gần → dùng Heavy để kết liễu
        if (playerLowHealth && distance < 3f)
        {
            var heavyAttack = readyAttacks.Find(a => a.type == AttackType.Heavy);
            if (heavyAttack != null)
                return heavyAttack;
        }
        
        // 3. Ở xa → dùng Ranged
        if (distance > 5f)
        {
            var rangedAttack = readyAttacks.Find(a => a.type == AttackType.Ranged);
            if (rangedAttack != null)
                return rangedAttack;
        }
        
        // 4. TRÁNH SPAM - không dùng attack vừa dùng
        if (lastUsedAttack != null)
        {
            readyAttacks.Remove(lastUsedAttack);
            if (readyAttacks.Count == 0)
                return null;
        }
        
        // 5. Weighted random dựa trên tactic
        return SelectWeightedAttack(readyAttacks);
    }

    /// <summary>
    /// Chọn attack với weight dựa trên tactic hiện tại
    /// </summary>
    private AttackData SelectWeightedAttack(List<AttackData> attacks)
    {
        var tactic = decisionMaker.CurrentTactic;
        
        switch (tactic)
        {
            case AIDecisionMaker.EnemyTactic.Aggressive:
                // Ưu tiên Heavy và Quick
                return PreferAttackTypes(attacks, AttackType.Heavy, AttackType.Quick);
                
            case AIDecisionMaker.EnemyTactic.Defensive:
                // Ưu tiên Ranged và Area
                return PreferAttackTypes(attacks, AttackType.Ranged, AttackType.Area);
                
            default:
                return attacks[Random.Range(0, attacks.Count)];
        }
    }

    private AttackData PreferAttackTypes(List<AttackData> attacks, params AttackType[] preferredTypes)
    {
        // 70% chọn preferred, 30% random
        if (Random.value < 0.7f)
        {
            foreach (var type in preferredTypes)
            {
                var attack = attacks.Find(a => a.type == type);
                if (attack != null)
                    return attack;
            }
        }
        
        return attacks[Random.Range(0, attacks.Count)];
    }

    /// <summary>
    /// Generate attack combo pattern
    /// </summary>
    public void GenerateAttackCombo()
    {
        currentPattern.Clear();
        
        for (int i = 0; i < patternLength; i++)
        {
            AttackData attack = DecideNextAttack();
            if (attack != null)
            {
                currentPattern.Enqueue(attack);
            }
        }
    }

    /// <summary>
    /// Execute attack với animation
    /// </summary>
    public void ExecuteAttack(AttackData attack)
    {
        if (attack == null || animator == null)
            return;
        
        // Trigger animation
        animator.SetTrigger(attack.animationTrigger);
        
        // Update tracking
        attack.lastUsedTime = Time.time;
        lastUsedAttack = attack;
        lastAttackTime = Time.time;
        
        Debug.Log($"[{gameObject.name}] Executing: {attack.attackName} (Type: {attack.type})");
    }

    /// <summary>
    /// Execute next attack trong combo
    /// </summary>
    public AttackData GetNextComboAttack()
    {
        if (currentPattern.Count > 0)
            return currentPattern.Dequeue();
        
        return null;
    }

    /// <summary>
    /// Kiểm tra có attack nào ready không
    /// </summary>
    public bool HasReadyAttack()
    {
        return GetReadyAttacks().Count > 0;
    }

    /// <summary>
    /// Lấy danh sách attacks đã sẵn sàng (hết cooldown)
    /// </summary>
    private List<AttackData> GetReadyAttacks()
    {
        List<AttackData> ready = new List<AttackData>();
        
        foreach (var attack in availableAttacks)
        {
            if (Time.time - attack.lastUsedTime >= attack.cooldown)
            {
                ready.Add(attack);
            }
        }
        
        return ready;
    }

    private AttackData GetRandomAvailableAttack()
    {
        var ready = GetReadyAttacks();
        if (ready.Count == 0)
            return null;
        
        return ready[Random.Range(0, ready.Count)];
    }

    // ==================== PLAYER BEHAVIOR TRACKING ====================
    
    /// <summary>
    /// Track player dodge pattern để dự đoán
    /// </summary>
    public void OnPlayerDodge(bool isLeft)
    {
        if (isLeft)
            playerBehaviorCount["dodgeLeft"]++;
        else
            playerBehaviorCount["dodgeRight"]++;
    }

    /// <summary>
    /// Dự đoán hướng player sẽ dodge
    /// </summary>
    public Vector2 PredictPlayerDodgeDirection()
    {
        int leftCount = playerBehaviorCount["dodgeLeft"];
        int rightCount = playerBehaviorCount["dodgeRight"];
        
        // Player hay dodge left → aim right
        if (leftCount > rightCount + 2)
            return Vector2.right;
        else if (rightCount > leftCount + 2)
            return Vector2.left;
        
        return Vector2.zero; // Không rõ pattern
    }

    // ==================== HELPER FUNCTIONS ====================
    
    private bool IsPlayerBlocking()
    {
        // Cần implement check player shield/block state
        return false;
    }
    
    private float GetPlayerHealthPercent()
    {
        knight player = FindFirstObjectByType<knight>();
        if (player == null) return 1f;
        
        // Cần access player health
        return 1f;
    }

    public float GetLastAttackTime() => lastAttackTime;
    public AttackData GetLastAttack() => lastUsedAttack;

    // ==================== GIZMOS ====================
    
    private void OnDrawGizmosSelected()
    {
        if (availableAttacks.Count == 0) return;
        
        // Vẽ attack ranges
        foreach (var attack in availableAttacks)
        {
            Color color = attack.type switch
            {
                AttackType.Quick => Color.yellow,
                AttackType.Heavy => Color.red,
                AttackType.Ranged => Color.blue,
                AttackType.Area => Color.magenta,
                AttackType.Grab => Color.green,
                _ => Color.white
            };
            
            Gizmos.color = color;
            Gizmos.DrawWireSphere(transform.position, attack.range);
        }
    }
}
