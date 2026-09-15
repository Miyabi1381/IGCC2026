using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class StorySequencePlayer : MonoBehaviour
{
    [System.Serializable]
    public class StoryLine
    {
        [Tooltip("LocalizedTextTableに登録済みのkey")]
        public string Key;

        [Tooltip("leave empty to keep showing whatever image was displayed on the previous line")]
        public Sprite Image;

        [Tooltip("Seconds to wait on this line before auto-advancing. Leave at -1 to use the default delay below.")]
        public float AutoAdvanceDelay = -1f;
    }

    [Header("Display")]
    [SerializeField] private TMP_Text displayText;
    [SerializeField] private Image storyImageDisplay; // the illustration/cutscene image shown alongside the text

    [Header("Story Content")]
    [Tooltip("Each entry is a LocalizedTextTable key, with an optional image to switch to when that line is shown")]
    [SerializeField] private StoryLine[] storyLines;

    [Header("Typewriter Effect")]
    [SerializeField] private bool useTypewriterEffect = true;
    [SerializeField] private float charactersPerSecond = 30f;

    [Header("Auto Advance")]
    [Tooltip("If enabled, lines advance automatically after their delay instead of waiting for player input. The player can still press advance to skip ahead early.")]
    [SerializeField] private bool useAutoAdvance = false;
    [SerializeField] private float defaultAutoAdvanceDelay = 2f;

    [Header("Input")]
    [SerializeField] private InputActionReference advanceActionReference;

    [Header("Events")]
    [SerializeField] private UnityEvent onStoryComplete;

    private InputAction advanceAction;
    private int currentIndex = -1;
    private bool isTyping = false;
    private Coroutine typeRoutine;
    private Coroutine autoAdvanceRoutine;
    private string currentFullText = "";

    public StoryLine[] StoryLines { get => storyLines; set => storyLines = value; }

    private void Awake()
    {
        if (advanceActionReference != null)
        {
            advanceAction = advanceActionReference.action;
        }
        else
        {
            advanceAction = new InputAction(name: "AdvanceStory", type: InputActionType.Button);
            advanceAction.AddBinding("<Keyboard>/space");
            advanceAction.AddBinding("<Mouse>/leftButton");
            advanceAction.AddBinding("<Touchscreen>/primaryTouch/tap");
        }

        advanceAction.performed += OnAdvancePerformed;
    }

    private void OnEnable()
    {
        advanceAction.Enable();

        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged += HandleLanguageChanged;
        }
    }

    private void OnDisable()
    {
        advanceAction.Disable();

        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged -= HandleLanguageChanged;
        }
    }

    private void OnDestroy()
    {
        advanceAction.performed -= OnAdvancePerformed;
        if (advanceActionReference == null)
        {
            advanceAction.Dispose();
        }
    }

    private void Start()
    {
        currentIndex = -1;
        ShowNextLine();
    }

    private void OnAdvancePerformed(InputAction.CallbackContext context)
    {
        if (isTyping)
        {
            // タイプ中に押されたら、演出を飛ばして全文を即表示する
            SkipTypewriter();
        }
        else
        {
            ShowNextLine();
        }
    }

    private void ShowNextLine()
    {
        CancelAutoAdvance(); // stop any pending timer for the line we're leaving, manual or otherwise

        currentIndex++;

        if (currentIndex >= StoryLines.Length)
        {
            onStoryComplete?.Invoke();
            return;
        }

        DisplayCurrentLine();
    }

    private void DisplayCurrentLine()
    {
        if (LanguageManager.Instance == null)
        {
            Debug.LogWarning("StorySequencePlayer: LanguageManager.Instance is null.");
            return;
        }

        StoryLine line = StoryLines[currentIndex];
        currentFullText = LanguageManager.Instance.GetText(line.Key);

        // Only swap the image if this line has one assigned — leaving it null lets several
        // consecutive lines share the same illustration before the next explicit image change.
        if (storyImageDisplay != null && line.Image != null)
        {
            storyImageDisplay.sprite = line.Image;
        }

        if (typeRoutine != null)
        {
            StopCoroutine(typeRoutine);
        }

        if (useTypewriterEffect)
        {
            typeRoutine = StartCoroutine(TypewriterRoutine(currentFullText));
        }
        else
        {
            displayText.text = currentFullText;
            TryStartAutoAdvance(); // text shown instantly, so schedule the auto-advance timer right away
        }
    }

    private IEnumerator TypewriterRoutine(string fullText)
    {
        isTyping = true;
        displayText.text = "";

        float secondsPerCharacter = 1f / Mathf.Max(charactersPerSecond, 1f);

        for (int i = 0; i < fullText.Length; i++)
        {
            displayText.text = fullText.Substring(0, i + 1);
            yield return new WaitForSecondsRealtime(secondsPerCharacter);
        }

        isTyping = false;
        typeRoutine = null;

        TryStartAutoAdvance(); // typewriter finished naturally — start the auto-advance clock now
    }

    private void SkipTypewriter()
    {
        if (typeRoutine != null)
        {
            StopCoroutine(typeRoutine);
            typeRoutine = null;
        }

        displayText.text = currentFullText;
        isTyping = false;

        TryStartAutoAdvance(); // player skipped ahead to full text — auto-advance timer starts from here instead
    }

    // Starts the auto-advance countdown for whatever line is currently displayed, if enabled.
    private void TryStartAutoAdvance()
    {
        if (!useAutoAdvance) return;

        CancelAutoAdvance(); // just in case one was already pending, shouldn't normally happen
        float delay = GetCurrentLineDelay();
        autoAdvanceRoutine = StartCoroutine(AutoAdvanceRoutine(delay));
    }

    private float GetCurrentLineDelay()
    {
        if (currentIndex >= 0 && currentIndex < StoryLines.Length)
        {
            float lineDelay = StoryLines[currentIndex].AutoAdvanceDelay;
            if (lineDelay >= 0f) return lineDelay; // per-line override takes priority
        }

        return defaultAutoAdvanceDelay;
    }

    private IEnumerator AutoAdvanceRoutine(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        autoAdvanceRoutine = null;
        ShowNextLine();
    }

    private void CancelAutoAdvance()
    {
        if (autoAdvanceRoutine != null)
        {
            StopCoroutine(autoAdvanceRoutine);
            autoAdvanceRoutine = null;
        }
    }

    // 再生中に言語が切り替わった場合、今表示中の行を新しい言語で出し直す。
    private void HandleLanguageChanged(Language language)
    {
        if (currentIndex >= 0 && currentIndex < StoryLines.Length)
        {
            DisplayCurrentLine();
        }
    }

    public void TransitionToMainGame()
    {
        SceneManager.LoadScene("MainGame");
    }

    public void TransitionToTitle()
    {
        SceneManager.LoadScene("Title Scene");
    }
}