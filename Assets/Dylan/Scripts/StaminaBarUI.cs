using UnityEngine;
using UnityEngine.UI;

public class StaminaBarUI : MonoBehaviour
{
    public PlayerController player;
    public CanvasGroup canvasGroup;
    public Image fillImage;
    public float fadeSpeed = 8f;

    public void SetPlayer(PlayerController newPlayer)
    {
        player = newPlayer;
    }

    void Update()
    {
        if (player == null)
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 0f, fadeSpeed * Time.deltaTime);
            return;
        }

        float targetAlpha = player.IsClimbing ? 1f : 0f;
        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
        fillImage.fillAmount = player.StaminaPercent;
    }
}