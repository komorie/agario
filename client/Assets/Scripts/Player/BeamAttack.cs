using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeamAttack : MonoBehaviour
{
    private GameObject beamPrefab;
    private Beam beam;
    private InputPlayer inputPlayer;
    private PacketSender packetSender;
    private Room room;
    
    private Color bStartCol = new Color(1f, 0f, 0f, 0f);
    private Color bEndCol = new Color(1f, 0f, 0f, 1f);
    private Color fStartCol = new Color(1f, 1f, 0f, 1f);
    private Color fEndCol = new Color(1f, 1f, 0f, 0f);
    
    private float fillTime = 3.0f; //���� �ð�
    private float fadeOutTime = 1.0f; //������� �ð�

    private bool canUse = true;
    private bool isFlying = false;


    void Awake()
    {
        beamPrefab = Resources.Load<GameObject>("Prefabs/Beam");
        packetSender = GetComponent<PacketSender>();
        inputPlayer = GetComponent<InputPlayer>();
        room = Room.Instance;
    }

    //�� ����
    public IEnumerator BeamCharge(Vector2 vec, float radius)
    {
        if (!canUse) yield break;
        canUse = false;

        if (GameScene.isMulti && packetSender != null) packetSender.SendBeamStartPacket(vec); //���� ���� ��Ŷ

        beam = Instantiate(beamPrefab, transform.position, Quaternion.identity, transform).GetComponent<Beam>(); //�� ����

        //���� ũ��, ��ġ, ȸ�� ����
        //���ݱ��� 2���� (x, y) �̵����͸� �״�� ����� �� �ְ� ������Ʈ�� ��ġ�߱� ������ ī�޶� ���� ������ (0, 0, -1) �� �Ǿ���ȴ�.
        Vector3 dir = new Vector3(vec.x, vec.y, 0);
        beam.transform.SetPositionAndRotation(
            transform.position + new Vector3(vec.x, vec.y, 0) * radius * 10 + new Vector3(vec.x * radius * 2, vec.y * radius * 2, 0),  
            Quaternion.LookRotation(dir, Vector3.back)
        );

        //�� ���� �ִϸ��̼�
        yield return LerpBeamColor(fillTime, bStartCol, bEndCol);

        //�̱� �÷��� �� �ٷ� Hit, ��Ƽ�� ��Ŷ ����
        if (!GameScene.isMulti)
        {
            BeamHit(beam.HitPlayers);
        }

        if (GameScene.isMulti && packetSender != null) packetSender.SendBeamHitPacket(beam.HitPlayers); //������, ���� ���� �߰�(���߿�)

        beam.transform.parent = null;
        yield return LerpBeamColor(fadeOutTime, fStartCol, fEndCol);
        Destroy(beam.gameObject);
        canUse = true;
    }

    public void BeamHit(List<int> hitTargets)
    {
        foreach (int targetId in hitTargets)
        {
            Player player = room.Players[targetId];
            StartCoroutine(player.beamAttack.BeamDamaged());
        }
    }

    public IEnumerator BeamDamaged() //������ �¾��� �� ���� ��ȭ��Ű�� ��� ȿ�� ó��
    {
        if (isFlying) yield break;
        isFlying = true;

        Material pm = GetComponent<Renderer>().material;
        Color originColor = pm.color;

        ChangeColorWithMaterialPropertyBlock(gameObject, Color.yellow);
        
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
            transform.position = new Vector3(originPos.x, originPos.y, originPos.z - height);
            yield return null;
        }

        Debug.Log(originPos.z);
        transform.position = originPos;

        ChangeColorWithMaterialPropertyBlock(gameObject, originColor);
        if (inputPlayer != null) inputPlayer.ToggleInputActions(); //���� ����

        isFlying = false;
    }

    public IEnumerator LerpBeamColor(float time, Color start, Color end)
    {
        float elapsed = 0; //�� ���� �ִϸ��̼�
        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / fillTime;
            ChangeColorWithMaterialPropertyBlock(beam.gameObject, Color.Lerp(start, end, normalizedTime));
            yield return null;
        }
    }

    public void ChangeColorWithMaterialPropertyBlock(GameObject gameObject, Color color)
    {
        Renderer renderer = gameObject.GetComponent<Renderer>();
        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();

        renderer.GetPropertyBlock(propBlock);

        propBlock.SetColor("_Color", color);

        renderer.SetPropertyBlock(propBlock);
    }
}
