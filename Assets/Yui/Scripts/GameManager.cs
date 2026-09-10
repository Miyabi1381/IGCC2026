using System;
using UnityEngine;
/*
 * プレイヤーなどのスクリプトで以下の例文のようなものを作り、
 *  HandlePauseChangedの中に何か特別にしたいことがあったら書く。
 *  特になければこの例文のようなものを入れておけばpauseの実装ができるはず

* Create a script like the example below in the player's script, etc.
* Write anything special you want to do inside HandlePauseChanged.
* If there's nothing special to do, just putting something like this example should implement pause.

 * private void OnEnable()
{
    if (GameManager.Instance != null)
        GameManager.Instance.OnPauseStateChanged += HandlePauseChanged;
}

private void OnDisable()
{
    if (GameManager.Instance != null)
        GameManager.Instance.OnPauseStateChanged -= HandlePauseChanged;
}

private void HandlePauseChanged(bool isPaused)
{
    enabled = !isPaused;
}

 */
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public event Action<bool> OnPauseStateChanged;

    public bool IsPaused { get; private set; } = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void SetPaused(bool paused)
    {
        if (IsPaused == paused)
        {
            return; 
        }

        IsPaused = paused;
        OnPauseStateChanged?.Invoke(IsPaused);
    }
}