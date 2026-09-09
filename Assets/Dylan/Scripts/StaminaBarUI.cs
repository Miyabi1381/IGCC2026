using UnityEngine;
using UnityEngine.UI;

public class StaminaBarUI : MonoBehaviour
{
    public PlayerController player;
    public CanvasGroup canvasGroup;
    public Image fillImage;
    public float fadeSpeed = 8f;

    void Update()
    {
        float targetAlpha = player.IsClimbing ? 1f : 0f;
        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
        fillImage.fillAmount = player.StaminaPercent;
    }
}