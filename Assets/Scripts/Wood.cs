using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class Wood : MonoBehaviour
{
    [SerializeField] private Transform groundCheck;
    private BoxCollider2D boxCollider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        boxCollider= GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //handleCheckPlayer();
    }

    private void handleCheckPlayer()
    {
        float woodTop = boxCollider.bounds.max.y;
        float playerY = groundCheck.position.y;
        if (playerY < woodTop)
        {
            if (!boxCollider.isTrigger)
                boxCollider.isTrigger = true;
        }
        else
        {
            if (boxCollider.isTrigger)
                boxCollider.isTrigger = false;
        }
    }
}
