using System.Collections;
using System.Collections.Generic;
using EskiNottToolKit;
using TMPro;
using UnityEngine;

public class DebugConsole : MonoSingleton<DebugConsole>
{
    [SerializeField] TextMeshProUGUI consoleText;
    CanvasGroup canvasGroup;
    bool consoleShow;

    private void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }
    private void Update()
    {
        ConsoleShowControl();
    }

    private void ConsoleShowControl()
    {
        canvasGroup.alpha = consoleShow ? 1 : 0;
    }

    public void SwitchConsoleShow()
    {
        consoleShow = !consoleShow;
    }

    public void SetConsoleLog(string s)
    {
        consoleText.text = s;
    }
}
