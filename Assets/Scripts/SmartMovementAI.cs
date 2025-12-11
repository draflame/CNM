using UnityEngine;
using System.Collections;

/// <summary>
/// Smart Movement AI - Di chuyển thông minh với predictive positioning
/// Tránh spam, tìm góc tấn công tốt, phản ứng với player movement
/// </summary>
public class SmartMovementAI : MonoBehaviour
{
    [Header("References")]
    private AIDecisionMaker decisionMaker;
    private Rigidbody2D rb;
    private Transform playerTransform;
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float stoppingDistance = 2f;
    
    [Header("Flanking Settings")]
    [SerializeField] private float flankingRadius = 4f;
    [SerializeField] private float flankingSpeed = 4f;
    [SerializeField] private bool preferRightFlank = true;
    
    [Header("Evasion Settings")]
    [SerializeField] private float dodgeDistance = 2f;
    [SerializeField] private float dodgeCooldown = 2f;
    [SerializeField] private float dodgeSpeed = 8f;
    private float lastDodgeTime = 0f;
    
    [Header("Predictive Movement")]
    [SerializeField] private bool usePredictiveMovement = true;
    [SerializeField] private float predictionTime = 0.3f;
    
    private Vector2 targetPosition;
    private bool isMoving = false;
    private bool isDodging = false;

    private void Start()
    {
        decisionMaker = GetComponent<AIDecisionMaker>();
        rb = GetComponent<Rigidbody2D>();
        
        knight player = FindFirstObjectByType<knight>();
        if (player != null)
            playerTransform = player.transform;
    }

    /// <summary>
    /// Di chuyển thông minh dựa trên tactic hiện tại
    /// </summary>
    public void SmartMove()
    {
        if (playerTransform == null || isDodging) return;
        
        var tactic = decisionMaker.CurrentTactic;
        
        switch (tactic)
        {
            case AIDecisionMaker.EnemyTactic.Aggressive:
                MoveTowardsPlayer();
                break;
                
            case AIDecisionMaker.EnemyTactic.Defensive:
                MaintainDistance();
                break;
                
            case AIDecisionMaker.EnemyTactic.Retreating:
                RetreatFromPlayer();
                break;
                
            case AIDecisionMaker.EnemyTactic.Flanking:
                FlankPlayer();
                break;
                
            case AIDecisionMaker.EnemyTactic.Ambushing:
                HoldPosition();
                break;
        }
    }

    /// <summary>
    /// Di chuyển về phía player (aggressive)
    /// </summary>
    private void MoveTowardsPlayer()
    {
        Vector2 targetPos = usePredictiveMovement 
            ? decisionMaker.PredictPlayerPosition(predictionTime)
            : (Vector2)playerTransform.position;
        
        float distance = Vector2.Distance(transform.position, targetPos);
        
        if (distance > stoppingDistance)
        {
            Vector2 direction = (targetPos - (Vector2)transform.position).normalized;
            MoveInDirection(direction, moveSpeed);
            FlipTowardsDirection(direction);
        }
    }

    /// <summary>
    /// Giữ khoảng cách an toàn (defensive)
    /// </summary>
    private void MaintainDistance()
    {
        float preferredDistance = decisionMaker.GetPreferredDistance();
        float currentDistance = Vector2.Distance(transform.position, playerTransform.position);
        
        if (currentDistance < preferredDistance)
        {
            // Quá gần → lùi lại
            Vector2 direction = ((Vector2)transform.position - (Vector2)playerTransform.position).normalized;
            MoveInDirection(direction, moveSpeed * 0.8f);
            FlipTowardsDirection(-direction); // Vẫn nhìn về player
        }
        else if (currentDistance > preferredDistance + 2f)
        {
            // Quá xa → tiến lại
            Vector2 direction = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
            MoveInDirection(direction, moveSpeed * 0.5f);
            FlipTowardsDirection(direction);
        }
    }

    /// <summary>
    /// Rút lui khỏi player (retreat)
    /// </summary>
    private void RetreatFromPlayer()
    {
        Vector2 retreatDirection = ((Vector2)transform.position - (Vector2)playerTransform.position).normalized;
        MoveInDirection(retreatDirection, moveSpeed * 1.2f);
        
        // Tìm cover/allies
        Vector2 nearestAllyPos = FindNearestAlly();
        if (nearestAllyPos != Vector2.zero)
        {
            Vector2 toAlly = (nearestAllyPos - (Vector2)transform.position).normalized;
            Vector2 combinedDirection = (retreatDirection + toAlly * 0.5f).normalized;
            MoveInDirection(combinedDirection, moveSpeed);
        }
    }

    /// <summary>
    /// Di chuyển vòng quanh player (flanking)
    /// </summary>
    private void FlankPlayer()
    {
        Vector2 toPlayer = (Vector2)playerTransform.position - (Vector2)transform.position;
        float angle = preferRightFlank ? -90f : 90f;
        
        // Tính vị trí bên hông player
        Vector2 flankDirection = Quaternion.Euler(0, 0, angle) * toPlayer.normalized;
        Vector2 flankTarget = (Vector2)playerTransform.position + flankDirection * flankingRadius;
        
        // Di chuyển tới vị trí flank
        Vector2 moveDir = (flankTarget - (Vector2)transform.position).normalized;
        MoveInDirection(moveDir, flankingSpeed);
        FlipTowardsDirection(toPlayer.normalized);
        
        // Đổi hướng flank mỗi 3 giây
        if (Time.time % 6f > 3f)
            preferRightFlank = !preferRightFlank;
    }

    /// <summary>
    /// Đứng im chờ player (ambush)
    /// </summary>
    private void HoldPosition()
    {
        // Chỉ quay mặt về player
        Vector2 toPlayer = (Vector2)playerTransform.position - (Vector2)transform.position;
        FlipTowardsDirection(toPlayer.normalized);
    }

    /// <summary>
    /// Dodge/tránh tấn công của player
    /// </summary>
    public void TryDodge(Vector2 dangerDirection)
    {
        if (Time.time - lastDodgeTime < dodgeCooldown || isDodging)
            return;
        
        StartCoroutine(PerformDodge(dangerDirection));
    }

    private IEnumerator PerformDodge(Vector2 dangerDirection)
    {
        isDodging = true;
        lastDodgeTime = Time.time;
        
        // Dodge vuông góc với hướng nguy hiểm
        Vector2 dodgeDirection = Vector2.Perpendicular(dangerDirection).normalized;
        
        // Random left hoặc right
        if (Random.value > 0.5f)
            dodgeDirection = -dodgeDirection;
        
        float dodgeDuration = 0.2f;
        float timer = 0f;
        
        while (timer < dodgeDuration)
        {
            MoveInDirection(dodgeDirection, dodgeSpeed);
            timer += Time.deltaTime;
            yield return null;
        }
        
        isDodging = false;
    }

    /// <summary>
    /// Di chuyển theo hướng với tốc độ cho trước
    /// </summary>
    private void MoveInDirection(Vector2 direction, float speed)
    {
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
            isMoving = true;
        }
        else
        {
            transform.Translate(direction * speed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Flip sprite theo hướng di chuyển
    /// </summary>
    private void FlipTowardsDirection(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > 0.1f)
        {
            Vector3 scale = transform.localScale;
            scale.x = direction.x > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    /// <summary>
    /// Tìm đồng minh gần nhất
    /// </summary>
    private Vector2 FindNearestAlly()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 10f);
        float nearestDistance = Mathf.Infinity;
        Vector2 nearestPos = Vector2.zero;
        
        foreach (var col in colliders)
        {
            if (col.gameObject != gameObject && col.GetComponent<Enemy>() != null)
            {
                float dist = Vector2.Distance(transform.position, col.transform.position);
                if (dist < nearestDistance)
                {
                    nearestDistance = dist;
                    nearestPos = col.transform.position;
                }
            }
        }
        
        return nearestPos;
    }

    /// <summary>
    /// Dừng di chuyển
    /// </summary>
    public void StopMovement()
    {
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
        isMoving = false;
    }

    // Getters
    public bool IsMoving() => isMoving;
    public bool IsDodging() => isDodging;

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || playerTransform == null) return;
        
        // Vẽ flanking positions
        Vector2 toPlayer = (Vector2)playerTransform.position - (Vector2)transform.position;
        Vector2 rightFlank = Quaternion.Euler(0, 0, -90f) * toPlayer.normalized;
        Vector2 leftFlank = Quaternion.Euler(0, 0, 90f) * toPlayer.normalized;
        
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere((Vector2)playerTransform.position + rightFlank * flankingRadius, 0.5f);
        Gizmos.DrawWireSphere((Vector2)playerTransform.position + leftFlank * flankingRadius, 0.5f);
    }
}
