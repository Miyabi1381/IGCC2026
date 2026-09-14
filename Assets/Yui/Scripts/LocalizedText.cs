using UnityEngine;
using UnityEngine.UI;
using TMPro;

// テキストを表示しているGameObject(TextMeshProUGUI、または通常のText)に
// このコンポーネントを追加し、Inspectorでkeyを指定する。
// LocalizedTextTableに登録したkeyと一致させること。
public class LocalizedText : MonoBehaviour
{
    [SerializeField] private string key;

    private TMP_Text tmpText;
    private Text uiText;

    private void Awake()
    {
        // TMP_TextとTextのどちらが付いているか自動判定する。
        // どちらも付いていない場合はWarningを出す。
        tmpText = GetComponent<TMP_Text>();
        uiText = GetComponent<Text>();

        if (tmpText == null && uiText == null)
        {
            Debug.LogWarning("LocalizedText: no TMP_Text or Text component found on " + gameObject.name);
        }
    }

    private void OnEnable()
    {
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged += HandleLanguageChanged;
            ApplyText();
        }
    }

    private void OnDisable()
    {
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged -= HandleLanguageChanged;
        }
    }
    void Start() 
    {
        ApplyText();
    }

    private void HandleLanguageChanged(Language language)
    {
        ApplyText();
    }

    private void ApplyText()
    {

        if (LanguageManager.Instance == null || string.IsNullOrEmpty(key))
        {
            return;
        }

        string text = LanguageManager.Instance.GetText(key);
        if (tmpText != null)
        {
            tmpText.text = text;
        }
        else if (uiText != null)
        {
            uiText.text = text;
        }
    }
}