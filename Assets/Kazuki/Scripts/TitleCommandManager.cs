using UnityEngine;
using UnityEngine.InputSystem;

public class TitleCommandManager : MonoBehaviour
{
    public GameObject image;
    public GameObject[] Command = new GameObject[3];

    int n = 0;
    int currentCommand = 0;
    const int COMMAND_MIN = 0;
    const int COMMAND_MAX = 3;
    void Start()
    {
        image.SetActive(true);
        for (int i = 0; i < COMMAND_MAX; i++)
        {
            if (i != n)
            {
                Command[i].SetActive(false);
            }
        }
    }
    void Update()
    {
        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            currentCommand--;
            if (currentCommand < 0)
            {
                currentCommand = COMMAND_MAX - 1;
            }
        }
        if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            currentCommand++;
            if (currentCommand >=COMMAND_MAX)
            {
                currentCommand = COMMAND_MIN;
            }
        }
        for (int i = 0; i < COMMAND_MAX; i++)
        {
            if (i != currentCommand)
            {
                Command[i].SetActive(false);
            }
            else
            {
                Command[i].SetActive(true);
            }
        }
    }
    public int SetCurrentCommand()
    {
        return currentCommand;
    }
}

