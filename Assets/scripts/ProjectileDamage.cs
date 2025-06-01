using UnityEngine;

public class ProjectileDamage : MonoBehaviour
{
    public float damage = 15f;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Glad"))
        {
            HealthBar hp = other.GetComponentInChildren<HealthBar>();
            if (hp != null)
            {
                hp.TakeDamage(damage);
                Debug.Log("투사체로 데미지!");
            }

            Destroy(gameObject); // 충돌하면 투사체 제거
        }
    }
}
