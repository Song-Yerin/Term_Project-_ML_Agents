using UnityEngine;
using UnityEngine.UI;

using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image filledImage;
    private float currentHP = 100f;
    private float maxHP = 100f;

    public void TakeDamage(float damage)
    {
        currentHP = Mathf.Clamp(currentHP - damage, 0, maxHP);
        UpdateUI();
    }

    void UpdateUI()
    {
        if (filledImage != null)
        {
            filledImage.fillAmount = currentHP / maxHP;
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
