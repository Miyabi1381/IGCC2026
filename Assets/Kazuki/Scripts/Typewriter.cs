using System.Collections;
using UnityEngine;
using TMPro;

public class Typewriter : MonoBehaviour
{
    // ① 1つのTextMeshProコンポーネントを使い回す
    [SerializeField] private TextMeshProUGUI m_TextMeshProUGUI;

    // ② 表示したい文章のリスト（インスペクターから複数入力）
    [SerializeField] private string[] m_TextLines;

    [SerializeField] private float Characterspacing = 0.05f;

    private Coroutine typewriterCoroutine;
    private int count = 0;

    void Start()
    {
        if (m_TextMeshProUGUI != null)
        {
            m_TextMeshProUGUI.text = "";
        }
    }

    public void OnStringChange()
    {
        if (m_TextMeshProUGUI == null || m_TextLines == null || count >= m_TextLines.Length) return;

        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
        }

        // 開始
        typewriterCoroutine = StartCoroutine(DoTypewriter(m_TextLines[count]));
        count++;
    }

    private IEnumerator DoTypewriter(string targetText)
    {
        // 初期化
        m_TextMeshProUGUI.text = targetText;
        m_TextMeshProUGUI.ForceMeshUpdate();
        m_TextMeshProUGUI.maxVisibleCharacters = 0;

        int totalCharacter = m_TextMeshProUGUI.textInfo.characterCount;

        for (int i = 0; i <= totalCharacter; i++)
        {
            m_TextMeshProUGUI.maxVisibleCharacters = i;
            yield return new WaitForSeconds(Characterspacing);
        }

        m_TextMeshProUGUI.maxVisibleCharacters = totalCharacter;
        typewriterCoroutine = null;

        // 待機
        yield return new WaitForSeconds(0.1f);
        OnStringChange();
    }
}
