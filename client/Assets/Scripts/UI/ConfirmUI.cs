using System;
using TMPro;
using UnityEngine;

public class ConfirmUI : MonoBehaviour
{
    [SerializeField]
    TMP_Text message;
    [SerializeField]
    TMP_Text buttonMessage;
    [SerializeField]
    Action CloseAction;

    public void Init(string text, string buttonText, Action CloseCallback = null)
    {
        message.text = text;    
        buttonMessage.text = buttonText;
        CloseAction = CloseCallback;
    }

    public void ConfirmClick()
    {
        Destroy(this);
        CloseAction?.Invoke();
    }
}
