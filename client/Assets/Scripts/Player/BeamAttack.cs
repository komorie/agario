using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeamAttack : MonoBehaviour
{
    private GameObject beamPrefab;
    private Beam beam;
    private Player player;
    private PacketSender packetSender;
    private Room room;
    private Color bStartCol = new Color(1f, 0f, 0f, 0f);
    private Color bEndCol = new Color(1f, 0f, 0f, 1f);
    private Color fStartCol = new Color(1f, 1f, 0f, 1f);
    private Color fEndCol = new Color(1f, 1f, 0f, 0f);
    private Color originColor;
    private float fillTime = 1.5f; //���� �ð�
    private float fadeOutTime = 1.0f; //������� �ð�
    private int coolTime = 5;
    private WaitForSeconds waitFor1sec = new WaitForSeconds(1.0f);
    public Action<bool, bool, int> StateChanged; //UI ����

    private bool pCanUse = true;
    private bool pIsFlying = false;
    private int pCurrentCoolTime = -1;

    private bool CanUse { get { return pCanUse; } set { pCanUse = value; StateChanged?.Invoke(pCanUse, pIsFlying, pCurrentCoolTime); } }
    private bool IsFlying { get { return pIsFlying; } set { pIsFlying = value; StateChanged?.Invoke(pCanUse, pIsFlying, pCurrentCoolTime); } }
    private int CurrentCoolTime { get { return pCurrentCoolTime; } set { pCurrentCoolTime = value; StateChanged?.Invoke(pCanUse, pIsFlying, pCurrentCoolTime); } }


    void Awake()
    {
        beamPrefab = Resources.Load<GameObject>("Prefabs/Beam");
        packetSender = GetComponent<PacketSender>();
        player = GetComponent<Player>();
        originColor = GetComponent<Renderer>().material.color;
        room = Room.Instance;
    }

    //�� ����
    public IEnumerator BeamCharge(Vector2 vec, float radius)
    {
        if (!CanUse || CurrentCoolTime != -1) yield break;
        CanUse = false;

        if (GameScene.isMulti && player is InputPlayer) packetSender.SendBeamStartPacket(vec); //���� ���� ��Ŷ

        beam = Instantiate(beamPrefab, transform.position, Quaternion.identity, transform).GetComponent<Beam>(); //�� ����

        //���� ũ��, ��ġ, ȸ�� ����
        //���ݱ��� 2���� (x, y) �̵����͸� �״�� ����� �� �ְ� ������Ʈ�� ��ġ�߱� ������ ī�޶� ���� ������ (0, 0, -1) �� �Ǿ���ȴ�.
        Vector3 dir = new Vector3(vec.x, vec.y, 0);
        beam.transform.SetPositionAndRotation(
            transform.position + new Vector3(vec.x, vec.y, 0) * radius * 10 + new Vector3(vec.x * radius * 2, vec.y * radius * 2, 0),  
            Quaternion.LookRotation(dir, Vector3.back)
        );

        //�� ���� �ִϸ��̼�
        yield return ColorChanger.LerpMaterialColor(beam.gameObject, fillTime, bStartCol, bEndCol);

        //�̱� �÷��� �� �ٷ� Hit, ��Ƽ�� ��Ŷ ����
        if (!GameScene.isMulti)
        {
            BeamHit(beam.HitPlayers);
        }

        if (GameScene.isMulti && player is InputPlayer) packetSender.SendBeamHitPacket(beam.HitPlayers); //������, ���� ���� �߰�(���߿�)

        beam.transform.parent = null;
        yield return ColorChanger.LerpMaterialColor(beam.gameObject, fadeOutTime, fStartCol, fEndCol);
        Destroy(beam.gameObject);
        CanUse = true;

        if (player is InputPlayer)
        {
            CurrentCoolTime = coolTime;
            StartCoroutine(Cooldown());
        }
    }

    public void BeamHit(List<int> hitTargets) //���� ����� ó��
    {
        foreach (int targetId in hitTargets)
        {
            Player p = room.Players[targetId];
            StartCoroutine(p.beamAttack.BeamDamaged());
        }
    }

    public IEnumerator BeamDamaged() //������ �¾��� �� ���� ��ȭ��Ű�� ��� ȿ�� ó��
    {
        if (gameObject.layer == LayerMask.NameToLayer("Transparent"))
        {
            yield break; //���� ���¸� �ȸ���
        }

        if (IsFlying) yield break; //�̹� ��� ���¸� �ȸ���
        IsFlying = true;

        ColorChanger.ChangeMaterialColor(gameObject, Color.yellow);
        
        InputPlayer inputPlayer = player as InputPlayer;
        if(inputPlayer != null) inputPlayer.ToggleInputActions(); //�̵� ����

        //1�� ���� ��-��
        Vector3 originPos = transform.position; // ���� ��ġ ����
        float peak = 10.0f; // �ִ� ����
        float duration = 1.0f; // ��ü ��� ���� �ð�

        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            //�߰��� ������Ʈ�� �ı��Ǿ����� Ȯ��
            if (this == null || gameObject == null)
            {
                yield break; // �ڷ�ƾ ����
            }

            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / duration; // 0���� 1 ������ ��
            float height = 4 * peak * normalizedTime * (1 - normalizedTime); 
            height = height < originPos.z ? 0 : height; 
            transform.position = new Vector3(originPos.x, originPos.y, originPos.z - height); //���� �ʿ�(0���� �� �������� �ʴ´�)
            yield return null;
        }

        transform.position = originPos;

        ColorChanger.ChangeMaterialColor(gameObject, originColor);
        if (inputPlayer != null) inputPlayer.ToggleInputActions(); //���� ����

        IsFlying = false;
    }

    private IEnumerator Cooldown()
    {
        for (CurrentCoolTime = coolTime; CurrentCoolTime >= 0; CurrentCoolTime--)
        {
            if (CurrentCoolTime > 0) yield return waitFor1sec;
        }
    }
}
