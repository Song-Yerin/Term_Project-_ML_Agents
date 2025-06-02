using UnityEngine;
using UnityEngine.UI;

public class HealthBarcontrol : MonoBehaviour
{
    public Image fillImage;           // 초록색 체력바 이미지
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
    }

    void UpdateBar()
    {
        fillImage.fillAmount = currentHealth / maxHealth;
    }
}
