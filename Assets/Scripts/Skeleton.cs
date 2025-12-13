using UnityEngine;

public class Skeleton : Enemy
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }
    protected override void Attack()
    {
        base.Attack();
        animator.SetTrigger("SkeletonAttack");
        // Add specific attack logic for Skeleton if needed
    }
    protected override void ApplyDamageToPlayer()
    {
        base.ApplyDamageToPlayer();
    }
    protected override void UpdateAnimation()
    {
        float velocityX = (transform.position.x - lastPosition.x) / Time.deltaTime;
        bool isRunning = Mathf.Abs(velocityX) > 0.01f;
        animator.SetBool("isMoving", isRunning);
        base.UpdateAnimation();
        
    }
}
