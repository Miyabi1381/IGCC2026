using System.Collections.Generic;
using UnityEngine;

// キー(id)ごとに、日本語/英語のテキストを1組ずつ登録しておくデータアセット。
// Assets右クリック → Create → Localization → Localized Text Table で作成する。
[CreateAssetMenu(fileName = "LocalizedTextTable", menuName = "Localization/Localized Text Table")]
public class LocalizedTextTable : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public string key;
        [TextArea(1, 3)] public string japanese;
        [TextArea(1, 3)] public string english;
    }

    [SerializeField] private List<Entry> entries = new List<Entry>();

    private Dictionary<string, Entry> lookup;

    private void OnEnable()
    {
        BuildLookup();
    }

    private void BuildLookup()
    {
        lookup = new Dictionary<string, Entry>();
        foreach (Entry entry in entries)
        {
            if (entry == null || string.IsNullOrEmpty(entry.key))
            {
                continue;
            }

            if (lookup.ContainsKey(entry.key))
            {
                Debug.LogWarning("LocalizedTextTable: key is duplicated. Skipped: " + entry.key);
                continue;
            }

            lookup.Add(entry.key, entry);
        }
    }

    public bool TryGetText(string key, Language language, out string text)
    {
        if (lookup == null)
        {
            BuildLookup();
        }

        if (!lookup.TryGetValue(key, out Entry entry))
        {
            text = null;
            return false;
        }

        text = language == Language.Japanese ? entry.japanese : entry.english;
        return true;
    }
}