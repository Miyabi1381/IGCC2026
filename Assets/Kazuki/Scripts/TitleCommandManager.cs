using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TitleCommandManager : MonoBehaviour
{
    public GameObject image;
    public GameObject[] Command = new GameObject[3];
    public TextMeshProUGUI[] CommandText = new TextMeshProUGUI[3];
    int currentCommand = 0;
    const int COMMAND_MIN = 0;
    const int COMMAND_MAX = 3;
    float timer = 0;
    float blinkInterval = 1f;
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
            textActive = true;
        }
        float progress = timer / blinkInterval;
        float easeValue = Easing.OutExpo(progress);
        float currentAlpha = textActive ? (1.0f - easeValue) : easeValue;
        for (int i = 0; i < COMMAND_MAX; i++)
        {
            if (i == currentCommand)
            {
                Command[i].SetActive(true);
                Color c = CommandText[i].color;
                c.a = currentAlpha;
                CommandText[i].color = c;
            }
            else
            {
                Command[i].SetActive(false);
                Color c = CommandText[i].color;
                c.a = 1.0f; // 必要に応じて 0.0f（非表示）にしてください
                CommandText[i].color = c;
            }
        }
    }
    public int SetCurrentCommand()
    {
        return currentCommand;
    }
}

