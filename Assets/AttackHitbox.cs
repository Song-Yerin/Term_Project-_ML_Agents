using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    public float damage = 10f;

    private void OnTriggerEnter(Collider other)
    {
        // 자기 자신과의 충돌은 무시
        if (other.gameObject == this.gameObject) return;

        Debug.Log($"[AttackHitbox] 충돌 대상: {other.name}");

        if (other.CompareTag("Enemy"))
        {
            Debug.Log("💥 적 피격 감지!");

            HealthBar hp = other.GetComponentInChildren<HealthBar>();
            if (hp != null)
            {
                hp.TakeDamage(damage);
                Debug.Log($"✅ 체력 감소 적용: -{damage}");
            }
        }
    }
}
