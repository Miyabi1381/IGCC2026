using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    [SerializeField]FadeManager fadeManager;
    public void ChangeToPlay()
    {
        SceneManager.LoadScene("Play Scene");
        Debug.Log("ChangePlayScene");
    }
    public void ChangeToTitle()
    {
        SceneManager.LoadScene("Title Scene");
        Debug.Log("ChangeTitleScene");

    }
    public void ChangeToSetting()
    {
        SceneManager.LoadScene("Setting Scene");
        Debug.Log("ChangeSettingScene");

    }
}
