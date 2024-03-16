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
    
    private float fillTime = 3.0f; //충전 시간
    private float fadeOutTime = 1.0f; //사라지는 시간

    private bool canUse = true;
    private bool isFlying = false;


    void Awake()
    {
        beamPrefab = Resources.Load<GameObject>("Prefabs/Beam");
        packetSender = GetComponent<PacketSender>();
        inputPlayer = GetComponent<InputPlayer>();
        room = Room.Instance;
    }

    //빔 충전
    public IEnumerator BeamCharge(Vector2 vec, float radius)
    {
        if (!canUse) yield break;
        canUse = false;

        if (GameScene.isMulti && packetSender != null) packetSender.SendBeamStartPacket(vec); //차지 시작 패킷

        beam = Instantiate(beamPrefab, transform.position, Quaternion.identity, transform).GetComponent<Beam>(); //빔 생성

        //빔의 크기, 위치, 회전 조정
        //지금까지 2차원 (x, y) 이동벡터를 그대로 써먹을 수 있게 오브젝트를 배치했기 때문에 카메라 기준 위쪽이 (0, 0, -1) 이 되어버렸다.
        Vector3 dir = new Vector3(vec.x, vec.y, 0);
        beam.transform.SetPositionAndRotation(
            transform.position + new Vector3(vec.x, vec.y, 0) * radius * 10 + new Vector3(vec.x * radius * 2, vec.y * radius * 2, 0),  
            Quaternion.LookRotation(dir, Vector3.back)
        );

        //빔 차지 애니메이션
        yield return LerpBeamColor(fillTime, bStartCol, bEndCol);

        //싱글 플레이 시 바로 Hit, 멀티면 패킷 전송
        if (!GameScene.isMulti)
        {
            BeamHit(beam.HitPlayers);
        }

        if (GameScene.isMulti && packetSender != null) packetSender.SendBeamHitPacket(beam.HitPlayers); //포지션, 방향 벡터 추가(나중에)

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

    public IEnumerator BeamDamaged() //본인이 맞았을 때 색깔 변화시키고 에어본 효과 처리
    {
        if (isFlying) yield break;
        isFlying = true;

        Material pm = GetComponent<Renderer>().material;
        Color originColor = pm.color;

        ChangeColorWithMaterialPropertyBlock(gameObject, Color.yellow);
        
        if(inputPlayer != null) inputPlayer.ToggleInputActions(); //이동 봉인

        //1초 동안 점-프
        Vector3 originPos = transform.position; // 원래 위치 저장
        float peak = 10.0f; // 최대 높이
        float duration = 1.0f; // 전체 에어본 지속 시간

        Debug.Log(originPos.z);

        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            //중간에 오브젝트가 파괴되었는지 확인
            if (this == null || gameObject == null)
            {
                yield break; // 코루틴 종료
            }

            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / duration; // 0에서 1 사이의 값
            float height = 4 * peak * normalizedTime * (1 - normalizedTime); 
            height = height < originPos.z ? 0 : height; 
            transform.position = new Vector3(originPos.x, originPos.y, originPos.z - height);
            yield return null;
        }

        Debug.Log(originPos.z);
        transform.position = originPos;

        ChangeColorWithMaterialPropertyBlock(gameObject, originColor);
        if (inputPlayer != null) inputPlayer.ToggleInputActions(); //봉인 해제

        isFlying = false;
    }

    public IEnumerator LerpBeamColor(float time, Color start, Color end)
    {
        float elapsed = 0; //빔 충전 애니메이션
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
