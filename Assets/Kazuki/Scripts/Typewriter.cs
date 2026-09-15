using System.Collections;
using TMPro;
using UnityEngine;

public class Typewriter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] m_TextMeshProUGUI;
    TextMeshProUGUI textComponent;
    [SerializeField] private float Characterspacing = 0.05f; // 0.5秒は遅すぎるため0.05秒に修正

    private Coroutine typewriterCoroutine;
    private int count = 0;

    void Start()
    {
        if (m_TextMeshProUGUI == null) return;

        for (int i = 0; i < m_TextMeshProUGUI.Length; i++)
        {
            if (m_TextMeshProUGUI[i] != null)
                m_TextMeshProUGUI[i].enabled = false;
        }
    }
    public void OnStringChange()
    {
        if (m_TextMeshProUGUI == null || count >= m_TextMeshProUGUI.Length) return;

        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
        }

        for (int i = 0; i < m_TextMeshProUGUI.Length; i++)
        {
            if (m_TextMeshProUGUI[i] == null) continue;
            // 過去に表示したテキストと、今から表示するテキストを有効化
            m_TextMeshProUGUI[i].enabled = (i <= count);
        }

        for (int i = 0; i < count; i++)
        {
            if (m_TextMeshProUGUI[i] == null) continue;
            m_TextMeshProUGUI[i].ForceMeshUpdate();
            int totalCharacter = m_TextMeshProUGUI[i].textInfo.characterCount;
            m_TextMeshProUGUI[i].maxVisibleCharacters = totalCharacter;
        }

        typewriterCoroutine = StartCoroutine(DoTypewriter(count));
        count++;
    }


    private IEnumerator DoTypewriter(int currentCount)
    {
        textComponent = m_TextMeshProUGUI[currentCount];

        textComponent.ForceMeshUpdate();

        textComponent.maxVisibleCharacters = 0;
        int totalCharacter = textComponent.textInfo.characterCount;

        for (int i = 0; i <= totalCharacter; i++)
        {
            textComponent.maxVisibleCharacters = i;
            yield return new WaitForSeconds(Characterspacing);
        }
        textComponent.maxVisibleCharacters = totalCharacter;
        typewriterCoroutine = null;
        yield return new WaitForSeconds(0.1f);
        OnStringChange();
    }
}

