
using System;
using UnityEngine;

// シングルトン。
public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance { get; private set; }

    [SerializeField] private LocalizedTextTable textTable;
    [SerializeField] private Language defaultLanguage = Language.Japanese;

    // 言語が切り替わった時に発火するイベント。
    // 各LocalizedTextはこれを購読してテキストを更新する。
    public event Action<Language> OnLanguageChanged;

    public Language CurrentLanguage { get; private set; }

    private const string LANGUAGE_PREF_KEY = "SelectedLanguage";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSavedLanguage();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void LoadSavedLanguage()
    {
        int savedValue = PlayerPrefs.GetInt(LANGUAGE_PREF_KEY, (int)defaultLanguage);
        CurrentLanguage = (Language)savedValue;
    }

    public void SetLanguage(Language language)
    {
        if (CurrentLanguage == language)
        {
            return; // 変化がない場合はイベントを発火しない
        }

        CurrentLanguage = language;
        PlayerPrefs.SetInt(LANGUAGE_PREF_KEY, (int)language);
        PlayerPrefs.Save();

        OnLanguageChanged?.Invoke(CurrentLanguage);
    }

    // keyに対応するテキストが見つからない場合は、キー自体を目立つ形で返す。
    // これにより、翻訳漏れがゲーム画面上ですぐ気づけるようになる。
    public string GetText(string key)
    {
        if (textTable == null)
        {
            Debug.LogWarning("LanguageManager: LocalizedTextTable is not assigned.");
            return "!" + key + "!";
        }

        if (textTable.TryGetText(key, CurrentLanguage, out string text))
        {
            return text;
        }

        Debug.LogWarning("LanguageManager: key not found: " + key);
        return "!" + key + "!";
    }
}