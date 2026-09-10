using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject volumePanel;
    [SerializeField] private Scrollbar bgmVolumeScrollbar;
    [SerializeField] private Scrollbar seVolumeScrollbar;

    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string exposedBgmVolumeParam = "BGM_Volume";
    [SerializeField] private string exposedSeVolumeParam = "SE_Volume";
    [SerializeField] private AudioSource seSampleSource; // SE調整時に試し鳴らしする場合に使用(任意)
    [SerializeField] private AudioClip seSampleClip;      // 試し鳴らし用クリップ(任意)

    [Header("Scene")]
    [SerializeField] private string titleSceneName = "TitleScene";

    [Header("Input")]
    [SerializeField] private InputActionReference pauseActionReference;

    private InputAction pauseAction;
    private bool isPaused = false;
    private const string BGM_VOLUME_PREF_KEY = "BGM_Volume";
    private const string SE_VOLUME_PREF_KEY = "SE_Volume";

    [Header("SE Sample Debounce")]
    [SerializeField] private float seSampleDebounceSeconds = 0.15f; // 操作が止まってから再生までの待ち時間
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
        // 保存済みのBGM/SE音量をそれぞれ復元
        float savedBgmVolume = PlayerPrefs.GetFloat(BGM_VOLUME_PREF_KEY, 0.75f);
        float savedSeVolume = PlayerPrefs.GetFloat(SE_VOLUME_PREF_KEY, 0.75f);

        if (bgmVolumeScrollbar != null)
        {
            bgmVolumeScrollbar.value = savedBgmVolume;
            bgmVolumeScrollbar.onValueChanged.AddListener(SetBgmVolume);
        }
        if (seVolumeScrollbar != null)
        {
            seVolumeScrollbar.value = savedSeVolume;
            seVolumeScrollbar.onValueChanged.AddListener(SetSeVolume);
        }

        SetBgmVolume(savedBgmVolume);
        SetSeVolume(savedSeVolume);

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
        Time.timeScale = 0f;          // ゲーム内時間を停止
        AudioListener.pause = false;  // UI操作音などは鳴らしたい場合はfalseのまま
    }

    public void ResumeGame()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        if (volumePanel != null) volumePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    // 音量設定パネルを開く(ボタンのOnClickに割り当てる)
    public void OpenVolumeSettings()
    {
        if (volumePanel != null) volumePanel.SetActive(true);
        pausePanel.SetActive(false);
    }

    public void CloseVolumeSettings()
    {
        if (volumePanel != null) volumePanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    // Scrollbarの値(0.0-1.0)をAudioMixerのdB値に変換して適用(BGM用)
    public void SetBgmVolume(float linearValue)
    {
        float dB = linearValue > 0.0001f ? Mathf.Log10(linearValue) * 20f : -80f;
        if (audioMixer != null)
        {
            audioMixer.SetFloat(exposedBgmVolumeParam, dB);
        }
        PlayerPrefs.SetFloat(BGM_VOLUME_PREF_KEY, linearValue);
        PlayerPrefs.Save();
    }

    // Scrollbarの値(0.0-1.0)をAudioMixerのdB値に変換して適用(SE用)
    public void SetSeVolume(float linearValue)
    {
        float dB = linearValue > 0.0001f ? Mathf.Log10(linearValue) * 20f : -80f;
        if (audioMixer != null)
        {
            audioMixer.SetFloat(exposedSeVolumeParam, dB);
        }
        PlayerPrefs.SetFloat(SE_VOLUME_PREF_KEY, linearValue);
        PlayerPrefs.Save();

        // ドラッグ中に大量再生されないよう、操作が止まってから1回だけ試し鳴らしする
        if (seSampleSource != null && seSampleClip != null)
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
        // Time.timeScale = 0中(ポーズ中)でも待てるようUnscaledDeltaTimeで待機
        yield return new WaitForSecondsRealtime(seSampleDebounceSeconds);
        seSampleSource.PlayOneShot(seSampleClip);
        seSampleRoutine = null;
    }

    // タイトルシーンへ戻る(ボタンのOnClickに割り当てる)
    public void ReturnToTitle()
    {
        Time.timeScale = 1f; // シーン遷移前に必ず時間を戻す
        SceneManager.LoadScene(titleSceneName);
    }
}