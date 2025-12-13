using System.Collections;
using UnityEngine;

public class Heart : MonoBehaviour
{
    [Header("Heart Settings")]
    //[SerializeField] private GameObject heartPrefab;
    private Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //public void Addheart(Transform transform)
    //{
    //    GameObject heart = Instantiate(heartPrefab, transform.position, transform.rotation);
    //    heart.transform.SetParent(transform);
    //}
    public IEnumerator Broken()
    {
        animator.SetTrigger("Broken");
        yield return new WaitForSeconds(0.3f);
        yield return new WaitForEndOfFrame();
        Destroy(gameObject);

    }

}
