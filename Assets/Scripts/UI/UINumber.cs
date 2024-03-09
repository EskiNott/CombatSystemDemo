using TMPro;
using UnityEngine;
using DG.Tweening;

public class UINumber : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Text;
    public CanvasGroup Cg { get; private set; }

    private void OnEnable()
    {
        Cg = GetComponent<CanvasGroup>();
    }

    public void SetText(string text)
    {
        Text.text = text;
    }

}
