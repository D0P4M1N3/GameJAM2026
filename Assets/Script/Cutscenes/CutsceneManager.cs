using UnityEngine;
using UnityEngine.Playables;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance;

    [SerializeField] private PlayableDirector director;

    private void Awake()
    {
        Instance = this;
    }

    public void Play(PlayableAsset timeline)
    {
        if (timeline == null) return;

        director.Stop(); // ensure clean state
        director.playableAsset = timeline;
        director.time = 0;
        director.Evaluate(); // optional but avoids 1-frame delay
        director.Play();
    }
}