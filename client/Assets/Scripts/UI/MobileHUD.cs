using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MobileHUD : MonoBehaviour
{
    [SerializeField]    
    TMP_Text connectAddress;

    [SerializeField]
    GameObject joyStick;

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
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) //����Ͽ����� ��ƽ �ѱ�
        {
            joyStick.SetActive(true);
        }
        else
        {
            joyStick.SetActive(false);
        }
    }

    private void OnDisable()
    {
        myPlayer.beamAttack.StateChanged -= OnBeamStateChanged;
        myPlayer.stealth.StateChanged -= OnStealthStateChanged;
    }

    public void Init(InputPlayer player, string address)
    {
        myPlayer = player;
        myPlayer.beamAttack.StateChanged += OnBeamStateChanged;
        myPlayer.stealth.StateChanged += OnStealthStateChanged;
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

    public void OnStealthStateChanged(bool isStealth, int currentCoolTime)
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
