using UnityEngine;
using UnityEngine.UI;

// 言語切り替えボタン用のヘルパー。
// 「日本語」ボタンと「English」ボタン、2つを両方このスクリプトに割り当てて使う。
// 現在選択されている方のボタンを自動でハイライトする。
public class LanguageToggleButtons : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button japaneseButton;
    [SerializeField] private Button englishButton;

    [Header("Highlight Colors")]
    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private Color unselectedColor = new Color(0.6f, 0.6f, 0.6f, 1f);

    private void OnEnable()
    {
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged += HandleLanguageChanged;
        }

        RefreshHighlight();
    }

    private void OnDisable()
    {
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged -= HandleLanguageChanged;
        }
    }

    public void SetJapanese()
    {
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.SetLanguage(Language.Japanese);
        }
    }

    public void SetEnglish()
    {
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.SetLanguage(Language.English);
        }
    }

    private void HandleLanguageChanged(Language language)
    {
        RefreshHighlight();
    }

    private void RefreshHighlight()
    {
        if (LanguageManager.Instance == null)
        {
            return;
        }

        bool isJapanese = LanguageManager.Instance.CurrentLanguage == Language.Japanese;

        SetButtonHighlight(japaneseButton, isJapanese);
        SetButtonHighlight(englishButton, !isJapanese);
    }

    private void SetButtonHighlight(Button button, bool isSelected)
    {
        if (button == null)
        {
            return;
        }

        Image image = button.image;
        if (image != null)
        {
            image.color = isSelected ? selectedColor : unselectedColor;
        }
    }
}