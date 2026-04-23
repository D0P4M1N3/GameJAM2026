using UnityEngine;
using UnityEngine.Playables;
using System.Collections.Generic;

public class CutsceneRoster : MonoBehaviour
{
    [System.Serializable]
    public class CutsceneEntry
    {
        public string key;
        public PlayableAsset timeline;
    }






    public static CutsceneRoster Instance;

    [SerializeField] private List<CutsceneEntry> entries;

    private Dictionary<string, PlayableAsset> lookup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        BuildLookup();
    }

    private void BuildLookup()
    {
        lookup = new Dictionary<string, PlayableAsset>();

        foreach (var entry in entries)
        {
            if (entry.timeline == null) continue;

            if (!lookup.ContainsKey(entry.key))
                lookup.Add(entry.key, entry.timeline);
            else
                Debug.LogWarning($"Duplicate cutscene key: {entry.key}");
        }
    }

    public void Play(string key)
    {
        if (!lookup.TryGetValue(key, out var timeline))
        {
            Debug.LogWarning($"Cutscene not found: {key}");
            return;
        }

        CutsceneManager.Instance.Play(timeline);
    }
}