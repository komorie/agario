using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MobileHUD : MonoBehaviour
{
    [SerializeField]    
    TMP_Text connectAddress;

    [SerializeField]
    Button atkButton;

    [SerializeField]
    Button stlButton;

    [SerializeField]
    TMP_Text atkBtnText;

    [SerializeField]
    TMP_Text stlBtnText;

    private string atkText = "����";
    private string stlText = "����";
    private InputPlayer myPlayer;


    private void Awake()
    {
        myPlayer = FindObjectOfType<InputPlayer>();
    }

    private void OnEnable()
    {
        myPlayer.beamAttack.StateChanged += OnBeamStateChanged;
        myPlayer.stealth.StateChanged += OnStealthStateChanged;
    }

    private void OnDisable()
    {
        myPlayer.beamAttack.StateChanged -= OnBeamStateChanged;
        myPlayer.stealth.StateChanged -= OnStealthStateChanged;
    }


    public void UpdateUI(string address)
    {
        connectAddress.text = address;
    }

    public void OnBeamStateChanged(bool canUse, bool isFlying, int currentCoolTime)
    {
        if (!canUse || isFlying)
        {
            atkButton.enabled = false;  //��Ȱ��ȭ
        }
        else
        {
            if (currentCoolTime <= 0)
            {
                atkButton.enabled = true;
                atkBtnText.text = atkText; //�⺻ ���·�
            }
            else
            {
                atkBtnText.text = currentCoolTime.ToString(); //���� ��Ÿ�� ǥ��
                atkButton.enabled = false;  //��Ȱ��ȭ
            }
        }
    }

    public void OnStealthStateChanged(bool isStealth, float currentCoolTime)
    {
        if(isStealth)
        {
            stlButton.enabled = false;
        }
        else
        {
            Debug.Log(currentCoolTime);
            if(currentCoolTime <= 0)
            {
                stlButton.enabled = true;
                stlBtnText.text = stlText; //�⺻ ���·�
            }
            else
            {
                stlButton.enabled = false;
                stlBtnText.text = currentCoolTime.ToString(); //���� ��Ÿ�� ǥ��

            }
        }
    }
}
