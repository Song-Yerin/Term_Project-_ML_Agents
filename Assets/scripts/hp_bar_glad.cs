using UnityEngine;
using UnityEngine.UI;

public class HealthBarGlad : MonoBehaviour
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
        if (filledImage == null)
        {
            Debug.LogWarning("filledImage가 연결되지 않았습니다! 체력바가 안 줄어듭니다.");
            return;
        }

        filledImage.fillAmount = currentHP / maxHP;
    }

}