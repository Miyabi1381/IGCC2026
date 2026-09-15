using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.SceneManagement;


public class StorySequencePlayer : MonoBehaviour
{
    [Header("Display")]
    [SerializeField] private TMP_Text displayText;

    [Header("Story Content")]
    [Tooltip("LocalizedTextTable??????key?????????????")]
    [SerializeField] private string[] storyKeys;

    [Header("Typewriter Effect")]
    [SerializeField] private bool useTypewriterEffect = true;
    [SerializeField] private float charactersPerSecond = 30f;

    [Header("Input")]
    [SerializeField] private InputActionReference advanceActionReference;

    [Header("Events")]
    [SerializeField] private UnityEvent onStoryComplete;

    private InputAction advanceAction;
    private int currentIndex = -1;
    private bool isTyping = false;
    private Coroutine typeRoutine;
    private string currentFullText = "";

    public string[] StoryKeys { get => storyKeys; set => storyKeys = value; }

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
        currentIndex++;

        if (currentIndex >= StoryKeys.Length)
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

        string key = StoryKeys[currentIndex];
        currentFullText = LanguageManager.Instance.GetText(key);

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
    }

    // 再生中に言語が切り替わった場合、今表示中の行を新しい言語で出し直す。
    private void HandleLanguageChanged(Language language)
    {
        if (currentIndex >= 0 && currentIndex < StoryKeys.Length)
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