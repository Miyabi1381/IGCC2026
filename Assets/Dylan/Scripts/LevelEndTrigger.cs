using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEndTrigger : MonoBehaviour
{
    [Header("--- ENDING THRESHOLD ---")]
    public int RequiredOfferings = 5;

    [Header("--- SCENES ---")]
    public string GoodEndingSceneName;
    public string BadEndingSceneName;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (DeathManager.Instance == null) return;

        bool metThreshold = DeathManager.Instance.CurrentOfferings >= RequiredOfferings;
        string sceneToLoad = metThreshold ? GoodEndingSceneName : BadEndingSceneName;

        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogWarning("LevelEndTrigger: Scene name not set for this outcome.");
            return;
        }

        SceneManager.LoadScene(sceneToLoad);
    }
}