using UnityEngine;

public class GroundAoEDamage : MonoBehaviour
{
    public float damage = 15f;
    public float duration = 2f;

    void Start()
    {
        // 일정 시간 후 장판 자동 제거
        Destroy(gameObject, duration);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            HealthBar hp = other.GetComponentInChildren<HealthBar>();
            if (hp != null)
            {
                hp.TakeDamage(damage);
                Debug.Log("장판 데미지 적용됨!");
            }
        }
    }
}
