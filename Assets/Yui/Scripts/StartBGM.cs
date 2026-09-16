using UnityEngine;

public class StartBGM : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SoundManager a = SoundManager.Instance;
        if (!a.IsBgmPlaying)
        {
            SoundManager.Instance.PlayBGM("TitleBGM");
        }
        else 
        {
            SoundManager.Instance.StopBGM();
            SoundManager.Instance.PlayBGM("TitleBGM");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
