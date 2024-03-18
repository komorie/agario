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

    private string atkText = "공격";
    private string stlText = "은신";
    private InputPlayer myPlayer;

    private void Awake()
    {
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) //모바일에서만 스틱 켜기
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
            atkButton.enabled = false;  //비활성화
        }
        else
        {
            if (currentCoolTime <= 0)
            {
                atkButton.enabled = true;
                atkBtnText.text = atkText; //기본 상태로
            }
            else
            {
                atkBtnText.text = currentCoolTime.ToString(); //현재 쿨타임 표시
                atkButton.enabled = false;  //비활성화
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
                stlBtnText.text = stlText; //기본 상태로
            }
            else
            {
                stlButton.enabled = false;
                stlBtnText.text = currentCoolTime.ToString(); //현재 쿨타임 표시

            }
        }
    }
}
