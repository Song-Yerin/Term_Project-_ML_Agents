using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 30f;
    public GameManager gameManager;
    public bool ownerIs1p = true;
    public int damage = 15;

    void Update()
    {
        transform.position += speed * Time.deltaTime * transform.forward;

        float attackRange = 0.5f;
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange);

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag(ownerIs1p ? "2P" : "1P"))
            {
                if (hit.CompareTag(ownerIs1p ? "2P" : "1P"))
                    gameManager.GetComponent<GameManager>().Damage(ownerIs1p, damage);
                Destroy(gameObject);
                break;
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(ownerIs1p ? "2P-Projectile" : "1P-Projectile"))
            Destroy(gameObject);
    }
}
