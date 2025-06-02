using UnityEngine;

public class AttackHit : MonoBehaviour
{
    public float damage = 10f;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy")) // 상대 캐릭터에 Enemy 태그가 있어야 함
        {
            Debug.Log("적 피격!");
            HealthBar hp = other.GetComponent<HealthBar>();
            if (hp != null)
            {
                hp.TakeDamage(damage);
            }
        }
    }
} 