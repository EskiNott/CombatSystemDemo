using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ComboTableElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ButtonText;
    [SerializeField] private TextMeshProUGUI ActionText;

    public void SetButtonText(string text)
    {
        ButtonText.text = text;
    }

    public void SetActionText(string text)
    {
        ActionText.text = text;
    }
}
