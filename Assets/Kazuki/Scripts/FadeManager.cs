using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class FadeManager : MonoBehaviour
{
    [SerializeField]private ChangeScene changeScene;
    public Image fadePanel;
    public float fadeDuration = 1.0f;

    public IEnumerator FadeOutAndLoadScene()
    {
        fadePanel.enabled = true;
        float elapsedTime = 0.0f;
        Color startColor = fadePanel.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 1.0f);

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadePanel.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }
        fadePanel.color = endColor;
        changeScene.ChangeToPlay();
    }
}
