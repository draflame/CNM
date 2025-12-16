using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float attackDuration = 0.4f;

    [Header("Detection")]
    [SerializeField] private float range = 1f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private float colliderDistance = 0.1f;
    [SerializeField] private float staminaCost = 10f;

    private Animator animator;
    private float cooldownTimer = Mathf.Infinity;
    private bool isAttacking = false;
    private Player player;

    private void Start()
    {
        animator = GetComponent<Animator>();
        player = GetComponent<Player>();
    }

    private void Update()
    {
        cooldownTimer += Time.deltaTime;

        if (Input.GetKey(KeyCode.J) && cooldownTimer >= attackCooldown && !isAttacking)
        {
            Attack();
        }
    }

    private void Attack()
    {
        if (player.IsShieldActive())
        {
            return;
        }
        if (player.GetStamina() < staminaCost)
        {
            Debug.Log("Not enough stamina to attack");
            return;
        }
        isAttacking = true;
        player.SetStamina(player.GetStamina() - staminaCost);
        cooldownTimer = 0f;
        animator.SetBool("isAttacking", true);

        Invoke(nameof(PerformAttack), 0.1f);
        Invoke(nameof(ResetAttack), attackDuration);
    }

    private void PerformAttack()
    {
        Enemy enemy = GetEnemyInAttackZone();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }
    }

    private void ResetAttack()
    {
        isAttacking = false;
        animator.SetBool("isAttacking", false);
    }

    private Enemy GetEnemyInAttackZone()
    {
        // ✅ FIX: OverlapBox cần half-extents (size/2), không phải full size
        Vector2 attackCenter = boxCollider.bounds.center + transform.right * range * transform.localScale.x * colliderDistance;
        Vector2 attackHalfExtents = new Vector2((boxCollider.bounds.size.x + range) / 2f, boxCollider.bounds.size.y / 2f);

        Collider2D hit = Physics2D.OverlapBox(attackCenter, attackHalfExtents, 0, enemyLayer);

        if (hit != null)
        {
            Debug.Log($"✅ Hit enemy: {hit.gameObject.name}");
            return hit.GetComponent<Enemy>();
        }

        Debug.Log("❌ No enemy in attack zone");
        return null;
    }
    private void OnDrawGizmos()
    {
        if (boxCollider == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(
            boxCollider.bounds.center + transform.right * range * transform.localScale.x * colliderDistance,
            new Vector3(boxCollider.bounds.size.x + range, boxCollider.bounds.size.y, boxCollider.bounds.size.z)
        );
    }
}
