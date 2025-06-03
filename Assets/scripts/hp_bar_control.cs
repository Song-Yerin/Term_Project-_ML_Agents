using UnityEngine;
using UnityEngine.UI;

public class HealthBarcontrol : MonoBehaviour
{
    public GameManager gameManager;
    public Image fillImage;           // 초록색 체력바 이미지
    public int maxHealth = 100;
    public bool amI1p = true;

    void Update()
    {
        int currentHealth = amI1p ? gameManager.p1HP : gameManager.p2HP;
        fillImage.fillAmount = (float)currentHealth / (float)maxHealth;
    }
}
