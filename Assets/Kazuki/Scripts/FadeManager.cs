using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class FadeManager : MonoBehaviour
{
    [SerializeField]private ChangeScene changeScene;
    [SerializeField]private Command command;
    public Image fadePanel;
    public float fadeDuration = 1.0f;

    public IEnumerator FadeOutAndLoadScene(int state)
    {
        fadePanel.enabled = true;
        float elapsedTime = 0.0f;
        Color startColor = fadePanel.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 1.0f);

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime*2;
            float t = Easing.OutQuint(elapsedTime);
            fadePanel.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }
        fadePanel.color = endColor;
        switch (state)
        {
        case 0:
            changeScene.ChangeToPlay();
            break;
        case 1:
            changeScene.ChangeToSetting();
            break;
        }
    }
}
