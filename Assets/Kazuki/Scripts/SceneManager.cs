using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public void ChangeTitletoPlay()
    {
        SceneManager.LoadScene("Play Scene");
    }
    public void ChangePlaytoTitle()
    {
        SceneManager.LoadScene("Title Scene");
    }
}
