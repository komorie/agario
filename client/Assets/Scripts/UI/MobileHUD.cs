using System;
using TMPro;
using UnityEngine;

public class MobileHUD : MonoBehaviour
{
    [SerializeField]    
    TMP_Text connectAddress;

    [SerializeField]
    TMP_Text elapsedTime;

    public void UpdateUI(string address)
    {
        connectAddress.text = address;
    }
}
