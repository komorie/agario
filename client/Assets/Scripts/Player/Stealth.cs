using System;
using System.Collections;
using UnityEngine;

public class Stealth : MonoBehaviour
{
    private Player player;
    private PacketSender packetSender;
    private Color origin;
    private Color stealthColor;

    private bool isStealth = false;
    private float coolTime = 5;
    private float currentCoolTime = -1;
    private WaitForSeconds waitFor1sec = new WaitForSeconds(1.0f);

    public Action<bool, float> StateChanged; //UI 갱신용

    private void Awake()
    {
        player = GetComponent<Player>();
        packetSender = GetComponent<PacketSender>();
        origin = GetComponent<Renderer>().material.color;
        stealthColor = new Color(origin.r, origin.g, origin.b, 0.5f);
    }

    public void StealthStart()
    {
        if (isStealth || currentCoolTime != -1) return;
        isStealth = true;

        StateChanged?.Invoke(isStealth, currentCoolTime);

        ColorChanger.ChangeMaterialColor(gameObject, stealthColor);
        gameObject.layer = LayerMask.NameToLayer("Transparent"); // 투명화

        StartCoroutine(DoStealth());
    }
    
    public IEnumerator DoStealth()
    {
        yield return new WaitForSeconds(1.0f);
        isStealth = false;
        StateChanged?.Invoke(isStealth, currentCoolTime);
        StealthEnd();
    }

    public void StealthEnd()
    {
        ColorChanger.ChangeMaterialColor(gameObject, origin);
        gameObject.layer = LayerMask.NameToLayer("Player"); //투명화 해제
        if(player is InputPlayer)
        {
            currentCoolTime = coolTime;
            StartCoroutine(Cooldown());
        }
    }

    private IEnumerator Cooldown()
    {
        for (currentCoolTime = coolTime; currentCoolTime >= 0; currentCoolTime--)
        {
            StateChanged?.Invoke(isStealth, currentCoolTime);
            if (currentCoolTime > 0) yield return waitFor1sec;
        }
    }
}
