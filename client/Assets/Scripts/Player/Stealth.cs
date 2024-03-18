using System;
using System.Collections;
using UnityEngine;

public class Stealth : MonoBehaviour
{
    private Player player;
    private Color origin;
    private Color stealthColor;
    private WaitForSeconds waitFor1sec = new WaitForSeconds(1.0f); 
    private int coolTime = 5;
    public Action<bool, int> StateChanged; //UI 갱신용

    private bool pIsStealth = false;
    private int pCurrentCoolTime = -1;

    private bool IsStealth { get { return pIsStealth; } set { pIsStealth = value; StateChanged?.Invoke(pIsStealth, pCurrentCoolTime); } }
    private int CurrentCoolTime { get { return pCurrentCoolTime; } set { pCurrentCoolTime = value; StateChanged?.Invoke(pIsStealth, pCurrentCoolTime); } }


    private void Awake()
    {
        player = GetComponent<Player>();
        origin = GetComponent<Renderer>().material.color;
        stealthColor = new Color(origin.r, origin.g, origin.b, 0.5f);
    }

    public void StealthStart()
    {
        if (IsStealth || CurrentCoolTime != -1) return;
        IsStealth = true;

        ColorChanger.ChangeMaterialColor(gameObject, stealthColor);
        gameObject.layer = LayerMask.NameToLayer("Transparent"); // 투명화

        StartCoroutine(DoStealth());
    }
    
    public IEnumerator DoStealth()
    {
        yield return new WaitForSeconds(1.0f);
        IsStealth = false;
        StealthEnd();
    }

    public void StealthEnd()
    {
        ColorChanger.ChangeMaterialColor(gameObject, origin);
        gameObject.layer = LayerMask.NameToLayer("Player"); //투명화 해제
        if(player is InputPlayer)
        {
            CurrentCoolTime = coolTime;
            StartCoroutine(Cooldown());
        }
    }

    private IEnumerator Cooldown()
    {
        for (CurrentCoolTime = coolTime; CurrentCoolTime >= 0; CurrentCoolTime--)
        {
            if (CurrentCoolTime > 0) yield return waitFor1sec;
        }
    }
}
