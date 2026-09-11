using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "SoundLibrary", menuName = "Audio/Sound Library")]
public class SoundLibrary : ScriptableObject
{
    [System.Serializable]
    public class SoundEntry
    {
        public string id;
        public AudioClip clip;
        [Range(0f, 1f)] public float volumeScale = 1f;
    }

    [SerializeField] private List<SoundEntry> entries = new List<SoundEntry>();

    private Dictionary<string, SoundEntry> lookup;

    private void OnEnable()
    {
        BuildLookup();
    }

    private void BuildLookup()
    {
        lookup = new Dictionary<string, SoundEntry>();
        foreach (SoundEntry entry in entries)
        {
            if (entry == null || string.IsNullOrEmpty(entry.id))
            {
                continue;
            }

            if (lookup.ContainsKey(entry.id))
            {
                Debug.LogWarning("SoundLibrary: id is duplicated. Skipped: " + entry.id);
                continue;
            }

            lookup.Add(entry.id, entry);
        }
    }

    public bool TryGet(string id, out AudioClip clip, out float volumeScale)
    {
        if (lookup == null)
        {
            BuildLookup();
        }

        if (lookup.TryGetValue(id, out SoundEntry entry))
        {
            clip = entry.clip;
            volumeScale = entry.volumeScale;
            return true;
        }

        clip = null;
        volumeScale = 1f;
        return false;
    }
}