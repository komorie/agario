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
    private float fillTime = 1.5f; //충전 시간
    private float fadeOutTime = 1.0f; //사라지는 시간
    private int coolTime = 5;
    private WaitForSeconds waitFor1sec = new WaitForSeconds(1.0f);
    public Action<bool, bool, int> StateChanged; //UI 갱신

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

    //빔 충전
    public IEnumerator BeamCharge(Vector2 vec, float radius)
    {
        if (!CanUse || CurrentCoolTime != -1) yield break;
        CanUse = false;

        if (GameScene.isMulti && player is InputPlayer) packetSender.SendBeamStartPacket(vec); //차지 시작 패킷

        beam = Instantiate(beamPrefab, transform.position, Quaternion.identity, transform).GetComponent<Beam>(); //빔 생성

        //빔의 크기, 위치, 회전 조정
        //지금까지 2차원 (x, y) 이동벡터를 그대로 써먹을 수 있게 오브젝트를 배치했기 때문에 카메라 기준 위쪽이 (0, 0, -1) 이 되어버렸다.
        Vector3 dir = new Vector3(vec.x, vec.y, 0);
        beam.transform.SetPositionAndRotation(
            transform.position + new Vector3(vec.x, vec.y, 0) * radius * 10 + new Vector3(vec.x * radius * 2, vec.y * radius * 2, 0),  
            Quaternion.LookRotation(dir, Vector3.back)
        );

        //빔 차지 애니메이션
        yield return ColorChanger.LerpMaterialColor(beam.gameObject, fillTime, bStartCol, bEndCol);

        //싱글 플레이 시 바로 Hit, 멀티면 패킷 전송
        if (!GameScene.isMulti)
        {
            BeamHit(beam.HitPlayers);
        }

        if (GameScene.isMulti && player is InputPlayer) packetSender.SendBeamHitPacket(beam.HitPlayers); //포지션, 방향 벡터 추가(나중에)

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

    public void BeamHit(List<int> hitTargets) //맞은 사람들 처리
    {
        foreach (int targetId in hitTargets)
        {
            Player p = room.Players[targetId];
            StartCoroutine(p.beamAttack.BeamDamaged());
        }
    }

    public IEnumerator BeamDamaged() //본인이 맞았을 때 색깔 변화시키고 에어본 효과 처리
    {
        if (gameObject.layer == LayerMask.NameToLayer("Transparent"))
        {
            yield break; //투명 상태면 안맞음
        }

        if (IsFlying) yield break; //이미 에어본 상태면 안맞음
        IsFlying = true;

        ColorChanger.ChangeMaterialColor(gameObject, Color.yellow);
        
        InputPlayer inputPlayer = player as InputPlayer;
        if(inputPlayer != null) inputPlayer.ToggleInputActions(); //이동 봉인

        //1초 동안 점-프
        Vector3 originPos = transform.position; // 원래 위치 저장
        float peak = 10.0f; // 최대 높이
        float duration = 1.0f; // 전체 에어본 지속 시간

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
            transform.position = new Vector3(originPos.x, originPos.y, originPos.z - height); //수정 필요(0으로 딱 떨어지지 않는다)
            yield return null;
        }

        transform.position = originPos;

        ColorChanger.ChangeMaterialColor(gameObject, originColor);
        if (inputPlayer != null) inputPlayer.ToggleInputActions(); //봉인 해제

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
