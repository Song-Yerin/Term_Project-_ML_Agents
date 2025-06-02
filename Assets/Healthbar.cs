using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image fillImage;           // ✅ 초록색 체력 바 이미지
    public float maxHealth = 100f;
    private float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateBar();
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        UpdateBar();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void UpdateBar()
    {
        fillImage.fillAmount = currentHealth / maxHealth;
    }

    void Die()
    {
        Debug.Log("사망!");
        gameObject.SetActive(false); // 또는 애니메이션, 제거 등 처리
    }
}
