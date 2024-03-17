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
    private WaitForSeconds waitFor1sec = new WaitForSeconds(1.0f);

    private bool canUse = true;
    private bool isFlying = false;
    private int coolTime = 5;
    private int currentCoolTime = -1;

    public Action<bool, bool, int> StateChanged; //UI ����

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
        if (!canUse || currentCoolTime != -1) yield break;
        canUse = false;

        if (GameScene.isMulti && packetSender != null) packetSender.SendBeamStartPacket(vec); //���� ���� ��Ŷ
        StateChanged?.Invoke(canUse, isFlying, currentCoolTime);

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

        if (GameScene.isMulti && packetSender != null) packetSender.SendBeamHitPacket(beam.HitPlayers); //������, ���� ���� �߰�(���߿�)

        beam.transform.parent = null;
        yield return ColorChanger.LerpMaterialColor(beam.gameObject, fadeOutTime, fStartCol, fEndCol);
        Destroy(beam.gameObject);
        canUse = true;

        if (player is InputPlayer)
        {
            currentCoolTime = coolTime;
            StartCoroutine(Cooldown());
        }
    }

    public void BeamHit(List<int> hitTargets) //���� ����� ó��
    {
        foreach (int targetId in hitTargets)
        {
            Player player = room.Players[targetId];
            StartCoroutine(player.beamAttack.BeamDamaged());
        }
    }

    public IEnumerator BeamDamaged() //������ �¾��� �� ���� ��ȭ��Ű�� ��� ȿ�� ó��
    {

        if (gameObject.layer == LayerMask.NameToLayer("Transparent"))
        {
            yield break; //���� ���¸� �ȸ���
        }


        if (isFlying) yield break; //�̹� ��� ���¸� �ȸ���
        isFlying = true;
        StateChanged?.Invoke(canUse, isFlying, currentCoolTime);

        ColorChanger.ChangeMaterialColor(gameObject, Color.yellow);
        
        InputPlayer inputPlayer = player as InputPlayer;
        if(inputPlayer != null) inputPlayer.ToggleInputActions(); //�̵� ����

        //1�� ���� ��-��
        Vector3 originPos = transform.position; // ���� ��ġ ����
        float peak = 10.0f; // �ִ� ����
        float duration = 1.0f; // ��ü ��� ���� �ð�

        Debug.Log(originPos.z);

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

        Debug.Log(originPos.z);
        transform.position = originPos;

        ColorChanger.ChangeMaterialColor(gameObject, originColor);
        if (inputPlayer != null) inputPlayer.ToggleInputActions(); //���� ����

        isFlying = false;
        StateChanged?.Invoke(canUse, isFlying, currentCoolTime);
    }

    private IEnumerator Cooldown()
    {
        for (currentCoolTime = coolTime; currentCoolTime >= 0; currentCoolTime--)
        {
            StateChanged?.Invoke(canUse, isFlying, currentCoolTime);
            if (currentCoolTime > 0) yield return waitFor1sec;
        }
    }
}
