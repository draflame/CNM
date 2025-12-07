using UnityEngine;

public class Spike : MonoBehaviour
{
    [SerializeField] private float damage = 10f;
    [SerializeField] private float attackDuration = 0.5f;
    [SerializeField] private float stayDamage = 5f;
    [SerializeField] private float stayDamageInterval = 1f;
    private float stayDamageTimer = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            knight player = collision.GetComponent<knight>();
            if (player != null)
            {
                player.TakeDamage(damage);
                stayDamageTimer = 0; 

            }
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            knight player = collision.GetComponent<knight>();
            if (player != null)
            {
                stayDamageTimer += Time.deltaTime;
                if (stayDamageTimer >= stayDamageInterval)
                {
                    player.TakeDamage(stayDamage);
                    stayDamageTimer = 0f; 
                }
            }
        }
    }
    //private void OnTriggerExit2D(Collider2D collision)
    //{
    //    if (collision.CompareTag("Player"))
    //    {
    //        Player player = collision.GetComponent<Player>();
    //        if (player != null)
    //        {
    //            player.StopStayDamage();
    //        }
    //    }
    //}

}
