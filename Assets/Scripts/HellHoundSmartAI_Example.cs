/*
 * ══════════════════════════════════════════════════════════════════════════
 * FILE NÀY CHỈ LÀ VÍ DỤ THAM KHẢO - KHÔNG SỬ DỤNG TRỰC TIẾP
 * Đây là code mẫu để bạn tham khảo cách tích hợp AI vào HellHound.cs
 * Để sử dụng, copy code cần thiết vào file HellHound.cs thực tế của bạn
 * ══════════════════════════════════════════════════════════════════════════
 */

#if false  // Disabled - example code only

using UnityEngine;
using System.Collections;

/// <summary>
/// HellHound with Smart AI - Ví dụ tích hợp AI vào enemy
/// Copy code này vào HellHound.cs của bạn hoặc tham khảo
/// </summary>
public class HellHoundSmartAI : Enemy
{
    [Header("AI Components")]
    private AIDecisionMaker aiDecision;
    private SmartMovementAI aiMovement;
    private SmartAttackAI aiAttack;

    [Header("Patrol Settings")]
    [SerializeField] private float patrolRadius = 3f;
    [SerializeField] private float waitTime = 1f;
    private Vector2 spawnPoint;
    private Vector2 patrolTarget;
    private float waitTimer = 0f;

    [Header("Aggro Settings")]
    [SerializeField] private float aggroRange = 6f;
    [SerializeField] private float viewAngle = 90f;
    [SerializeField] private float lineOfSightDistance = 7f;
    [SerializeField] private LayerMask obstacleMask;

    [Header("Leap Attack Settings")]
    [SerializeField] private float leapForce = 12f;
    [SerializeField] private float leapCooldown = 3f;
    [SerializeField] private float leapRange = 4f;
    private bool canLeap = true;

    [Header("AI Toggle")]
    [SerializeField] private bool useSmartAI = true; // Bật/tắt AI

    private Animator anim;
    private bool hasTarget = false;

    protected override void Start()
    {
        base.Start();
        
        // Khởi tạo AI
        if (useSmartAI)
        {
            InitializeAI();
        }
        
        spawnPoint = transform.position;
        SetNewPatrolTarget();
        anim = GetComponent<Animator>();
    }

    /// <summary>
    /// Khởi tạo AI components
    /// </summary>
    private void InitializeAI()
    {
        // Add components nếu chưa có
        aiDecision = GetComponent<AIDecisionMaker>();
        if (aiDecision == null)
        {
            aiDecision = gameObject.AddComponent<AIDecisionMaker>();
            Debug.Log($"[{gameObject.name}] Added AIDecisionMaker");
        }

        aiMovement = GetComponent<SmartMovementAI>();
        if (aiMovement == null)
        {
            aiMovement = gameObject.AddComponent<SmartMovementAI>();
            Debug.Log($"[{gameObject.name}] Added SmartMovementAI");
        }

        aiAttack = GetComponent<SmartAttackAI>();
        if (aiAttack == null)
        {
            aiAttack = gameObject.AddComponent<SmartAttackAI>();
            Debug.Log($"[{gameObject.name}] Added SmartAttackAI");
        }
    }

    protected override void Update()
    {
        cooldownTimer += Time.deltaTime;
        UpdateAnimation();

        // ==================== SMART AI MODE ====================
        if (useSmartAI && aiDecision != null && aiMovement != null && aiAttack != null)
        {
            UpdateWithAI();
            return;
        }

        // ==================== ORIGINAL MODE ====================
        UpdateOriginal();
    }

    /// <summary>
    /// Update logic với AI
    /// </summary>
    private void UpdateWithAI()
    {
        if (player == null) return;

        // AI ra quyết định tactics
        aiDecision.MakeDecision();
        
        var tactic = aiDecision.CurrentTactic;
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

        // Xử lý theo tactic
        switch (tactic)
        {
            case AIDecisionMaker.EnemyTactic.Patrol:
                HandlePatrol();
                break;

            case AIDecisionMaker.EnemyTactic.Aggressive:
                HandleAggressive(distanceToPlayer);
                break;

            case AIDecisionMaker.EnemyTactic.Defensive:
                HandleDefensive(distanceToPlayer);
                break;

            case AIDecisionMaker.EnemyTactic.Retreating:
                HandleRetreating();
                break;

            case AIDecisionMaker.EnemyTactic.Flanking:
                HandleFlanking();
                break;

            case AIDecisionMaker.EnemyTactic.Ambushing:
                HandleAmbushing(distanceToPlayer);
                break;
        }
    }

    /// <summary>
    /// Xử lý Patrol tactic
    /// </summary>
    private void HandlePatrol()
    {
        anim.SetBool("isRunning", false);
        anim.SetBool("isMoving", true);

        if (Vector2.Distance(transform.position, patrolTarget) < 0.15f)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTime)
            {
                SetNewPatrolTarget();
                waitTimer = 0f;
            }
        }
    }

    /// <summary>
    /// Xử lý Aggressive tactic - Tấn công mạnh
    /// </summary>
    private void HandleAggressive(float distance)
    {
        anim.SetBool("isRunning", true);
        anim.SetBool("isMoving", true);

        // Smart movement về phía player
        aiMovement.SmartMove();

        // Leap attack nếu trong range
        if (distance <= leapRange && canLeap)
        {
            StartCoroutine(DoLeapAttack());
            return;
        }

        // Attack bình thường
        if (aiAttack.HasReadyAttack() && distance <= attackDuration)
        {
            var attack = aiAttack.DecideNextAttack();
            if (attack != null)
            {
                aiAttack.ExecuteAttack(attack);
                ApplyDamageToPlayer();
            }
        }
    }

    /// <summary>
    /// Xử lý Defensive tactic - Giữ khoảng cách
    /// </summary>
    private void HandleDefensive(float distance)
    {
        anim.SetBool("isRunning", false);
        anim.SetBool("isMoving", true);

        // AI tự động maintain distance
        aiMovement.SmartMove();

        // Chỉ tấn công từ xa hoặc khi an toàn
        if (distance > 3f && aiAttack.HasReadyAttack())
        {
            var attack = aiAttack.DecideNextAttack();
            if (attack != null)
            {
                aiAttack.ExecuteAttack(attack);
                ApplyDamageToPlayer();
            }
        }
    }

    /// <summary>
    /// Xử lý Retreating tactic - Rút lui
    /// </summary>
    private void HandleRetreating()
    {
        anim.SetBool("isRunning", true);
        anim.SetBool("isMoving", true);

        // AI tự động chạy về phía spawn hoặc allies
        aiMovement.SmartMove();

        // Có thể heal hoặc call backup ở đây
        if (Vector2.Distance(transform.position, spawnPoint) < 1f)
        {
            // Đã về spawn point, có thể heal
            // health = Mathf.Min(health + 0.1f * Time.deltaTime, maxHealth);
        }
    }

    /// <summary>
    /// Xử lý Flanking tactic - Đi vòng
    /// </summary>
    private void HandleFlanking()
    {
        anim.SetBool("isRunning", true);
        anim.SetBool("isMoving", true);

        // AI tự động flank
        aiMovement.SmartMove();

        // Attack từ góc bất ngờ
        float distance = Vector2.Distance(transform.position, player.transform.position);
        if (distance <= leapRange && canLeap)
        {
            StartCoroutine(DoLeapAttack());
        }
    }

    /// <summary>
    /// Xử lý Ambushing tactic - Phục kích
    /// </summary>
    private void HandleAmbushing(float distance)
    {
        anim.SetBool("isRunning", false);
        anim.SetBool("isMoving", false);

        // Đứng yên, nhìn về player
        Vector2 toPlayer = (Vector2)player.transform.position - (Vector2)transform.position;
        if (toPlayer.x > 0)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

        // Nếu player đến gần → leap attack bất ngờ!
        if (distance <= leapRange + 1f && canLeap)
        {
            StartCoroutine(DoLeapAttack());
        }
    }

    /// <summary>
    /// Leap attack với AI prediction
    /// </summary>
    private IEnumerator DoLeapAttack()
    {
        canLeap = false;
        anim.SetTrigger("Leap");

        yield return new WaitForSeconds(0.2f);

        // Dùng AI để dự đoán vị trí player
        Vector2 targetPos = aiDecision.PredictPlayerPosition(0.5f);
        Vector2 direction = (targetPos - (Vector2)transform.position).normalized;

        rb.AddForce(direction * leapForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(0.5f);

        // Damage khi leap hit
        if (PlayerInSight())
        {
            ApplyDamageToPlayer();
        }

        yield return new WaitForSeconds(leapCooldown - 0.7f);
        canLeap = true;
    }

    /// <summary>
    /// Update logic gốc (nếu tắt AI)
    /// </summary>
    private void UpdateOriginal()
    {
        if (PlayerVisible())
        {
            hasTarget = true;
            float dist = Vector2.Distance(transform.position, player.transform.position);

            if (dist <= leapRange && canLeap)
            {
                StartCoroutine(DoLeapAttackOriginal());
                return;
            }

            ChasePlayer();
        }
        else
        {
            hasTarget = false;
            Patrol();
        }
    }

    private void Patrol()
    {
        anim.SetBool("isRunning", false);
        anim.SetBool("isMoving", true);

        if (Vector2.Distance(transform.position, patrolTarget) < 0.15f)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTime)
            {
                SetNewPatrolTarget();
                waitTimer = 0f;
            }
        }
        else
        {
            Vector2 dir = (patrolTarget - (Vector2)transform.position).normalized;
            transform.position = Vector2.MoveTowards(transform.position, patrolTarget, speed * Time.deltaTime);

            if (dir.x > 0)
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            else
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    private void ChasePlayer()
    {
        anim.SetBool("isRunning", true);
        anim.SetBool("isMoving", true);

        Vector2 dir = ((Vector2)player.transform.position - (Vector2)transform.position).normalized;
        transform.position = Vector2.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);

        if (dir.x > 0)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    private IEnumerator DoLeapAttackOriginal()
    {
        canLeap = false;
        anim.SetTrigger("Leap");
        yield return new WaitForSeconds(0.2f);

        Vector2 dir = ((Vector2)player.transform.position - (Vector2)transform.position).normalized;
        rb.AddForce(dir * leapForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(0.5f);

        if (PlayerInSight())
            ApplyDamageToPlayer();

        yield return new WaitForSeconds(leapCooldown - 0.7f);
        canLeap = true;
    }

    private void SetNewPatrolTarget()
    {
        float randomX = Random.Range(-patrolRadius, patrolRadius);
        patrolTarget = new Vector2(spawnPoint.x + randomX, transform.position.y);
    }

    private bool PlayerVisible()
    {
        if (player == null) return false;

        float dist = Vector2.Distance(transform.position, player.transform.position);
        if (dist > aggroRange) return false;

        Vector2 dirToPlayer = ((Vector2)player.transform.position - (Vector2)transform.position).normalized;
        float angle = Vector2.Angle(transform.right * Mathf.Sign(transform.localScale.x), dirToPlayer);

        if (angle > viewAngle / 2f) return false;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToPlayer, lineOfSightDistance, obstacleMask | playerLayer);

        return hit.collider != null && hit.collider.GetComponent<knight>() != null;
    }

    protected override void UpdateAnimation()
    {
        base.UpdateAnimation();
    }

    protected override void Attack()
    {
        base.Attack();
        anim.SetTrigger("Attack");
    }

    protected override void ApplyDamageToPlayer()
    {
        base.ApplyDamageToPlayer();
    }
}

#endif // End of example code
