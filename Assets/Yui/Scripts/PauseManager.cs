using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject volumePanel;
    [SerializeField] private Scrollbar bgmVolumeScrollbar;
    [SerializeField] private Scrollbar seVolumeScrollbar;

    [Header("Scene")]
    [SerializeField] private string titleSceneName = "TitleScene";

    [Header("Input")]
    [SerializeField] private InputActionReference pauseActionReference;

    [Header("SE Sample")]
    [SerializeField] private AudioClip seSampleClip;
    [SerializeField] private float seSampleDebounceSeconds = 0.15f;

    private InputAction pauseAction;
    private bool isPaused = false;
    private Coroutine seSampleRoutine;

    private void Awake()
    {
        if (pauseActionReference != null)
        {
            pauseAction = pauseActionReference.action;
        }
        else
        {
            pauseAction = new InputAction(name: "Pause", type: InputActionType.Button);
            pauseAction.AddBinding("<Keyboard>/escape");
            pauseAction.AddBinding("<Gamepad>/start");
        }

        pauseAction.performed += OnPausePerformed;
    }

    private void OnEnable()
    {
        pauseAction.Enable();
    }

    private void OnDisable()
    {
        pauseAction.Disable();
    }

    private void OnDestroy()
    {
        pauseAction.performed -= OnPausePerformed;
        if (pauseActionReference == null)
        {
            pauseAction.Dispose();
        }
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        TogglePause();
    }

    private void Start()
    {
        if (SoundManager.Instance == null)
        {
            Debug.LogWarning("PauseManager: SoundManager.Instance is null. Place a SoundManager in the first-loaded scene.");
        }
        else
        {
            if (bgmVolumeScrollbar != null)
            {
                bgmVolumeScrollbar.value = SoundManager.Instance.GetBgmVolume();
                bgmVolumeScrollbar.onValueChanged.AddListener(OnBgmScrollbarChanged);
            }
            if (seVolumeScrollbar != null)
            {
                seVolumeScrollbar.value = SoundManager.Instance.GetSeVolume();
                seVolumeScrollbar.onValueChanged.AddListener(OnSeScrollbarChanged);
            }
        }

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("PauseManager: GameManager.Instance is null. Place a GameManager in the first-loaded scene.");
        }

        pausePanel.SetActive(false);
        if (volumePanel != null) volumePanel.SetActive(false);
    }

    public void TogglePause()
    {
        if (isPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f; 

        if (GameManager.Instance != null)
        {
            Debug.Log("pause");
            GameManager.Instance.SetPaused(true);
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        if (volumePanel != null) volumePanel.SetActive(false);
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetPaused(false);
        }
    }

    public void OpenVolumeSettings()
    {
        Debug.Log("OpenVolumeSetting");
        if (volumePanel != null) volumePanel.SetActive(true);
        pausePanel.SetActive(false);
        Debug.Log("OpenVolumeSetting");
    }

    public void CloseVolumeSettings()
    {
        if (volumePanel != null) volumePanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    private void OnBgmScrollbarChanged(float linearValue)
    {
        if (SoundManager.Instance == null) return;
        SoundManager.Instance.SetBgmVolume(linearValue);
    }

    private void OnSeScrollbarChanged(float linearValue)
    {
        if (SoundManager.Instance == null) return;
        SoundManager.Instance.SetSeVolume(linearValue);

        // ドラッグ中に大量再生されないよう、操作が止まってから1回だけ鳴らす
        if (seSampleClip != null)
        {
            if (seSampleRoutine != null)
            {
                StopCoroutine(seSampleRoutine);
            }
            seSampleRoutine = StartCoroutine(PlaySeSampleDebounced());
        }
    }

    private System.Collections.IEnumerator PlaySeSampleDebounced()
    {
        yield return new WaitForSecondsRealtime(seSampleDebounceSeconds);
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySE(seSampleClip);
        }
        seSampleRoutine = null;
    }

    public void ReturnToTitle()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(titleSceneName);
    }
}