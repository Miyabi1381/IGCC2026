using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
public enum CommandState
{
    START=0,
    SETTING=1,
    EXITGAME=2,
}
public class Command : MonoBehaviour
{
    [SerializeField]private FadeManager fadeManager;
    public ChangeScene changeScene;
    public TitleCommandManager titleCommandManager;
    public int currentState;
    public bool active;
    void Start()
    {
        currentState = (int)CommandState.START;
    }

    void Update()
    {
        currentState = titleCommandManager.SetCurrentCommand();
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            active = true;
        }
        if (active)
        {
            switch (currentState)
            {
            case 0:
                //Start
                StartCoroutine(fadeManager.FadeOutAndLoadScene(0));
                break;
            case 1:
                //Setting
                StartCoroutine(fadeManager.FadeOutAndLoadScene(1));
                break;
            case 2:
                //Exit Game
                Application.Quit();
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#endif
                break;
            }
            active = false;
        }
    }
}
