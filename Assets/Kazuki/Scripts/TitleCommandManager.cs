using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TitleCommandManager : MonoBehaviour
{
    public GameObject image;
    public GameObject[] Command = new GameObject[3];
    public GameObject[] Text = new GameObject[3];
    int currentCommand = 0;
    const int COMMAND_MIN = 0;
    const int COMMAND_MAX = 3;
    float timer = 0;
    float blinkInterval = 0.7f;
    bool textActive = true;
    void Start()
    {
        image.SetActive(true);
        for (int i = 0; i < COMMAND_MAX; i++)
        {
            if (i ==currentCommand )Command[i].SetActive(true);
        }
    }
    void Update()
    {
        timer += Time.deltaTime;
        if(timer>=blinkInterval)
        {
            textActive = !textActive;
            timer = 0.0f;
        }
        bool isChanged = false;
        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            currentCommand--;
            if (currentCommand < 0)
            {
                currentCommand = COMMAND_MAX - 1;
            }
            isChanged = true;
        }
        if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            currentCommand++;
            if (currentCommand >=COMMAND_MAX)
            {
                currentCommand = COMMAND_MIN;
            }
            isChanged = true;
        }
        if(isChanged)
        {
            timer = 0.0f;
            textActive = false;
        }
        for (int i = 0; i < COMMAND_MAX; i++)
        {
            if (i == currentCommand)
            {
                Command[i].SetActive(true);
                Text[i].SetActive(textActive);
            }
            else
            {
                Command[i].SetActive(false);
                Text[i].SetActive(true);
            }
        }
    }
    public int SetCurrentCommand()
    {
        return currentCommand;
    }
}

