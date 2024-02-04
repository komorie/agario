using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static S_RoomList;

public class OldPlayer : MonoBehaviour
{
    public int PlayerId { get; set; }
    public int Speed { get; set; } = 20;
    public float Radius { get; set; }
    public bool IsLerping { get; set; } = false;    
    public Vector2 MoveVector { get; set; }
    public Vector3 TargetPosition { get; set; }

    protected HashSet<Collider> touchingColliders = new HashSet<Collider>();


    protected virtual void Awake()
    {  
        TargetPosition = transform.position;
    }


    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wall"))
        {
            touchingColliders.Add(other);
            MoveAttachedOnWall();
        }
    }


    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Wall"))
        {
            touchingColliders.Remove(other);
        }
    }

    protected virtual void Update()
    {
        if (IsLerping) //일단 패킷이 와서 오차를 조정해야 하는 경우
        {

            float currentDistance = Vector3.Distance(transform.position, TargetPosition); //상대 클라가 서버로 보낸 위치(+예측 들어간 위치)와 그전까지 내 클라가 추측해서 이동시킨 위치의 거리 비교

            Vector3 newDes = Vector3.Lerp(transform.position, TargetPosition, 0.5f);   //상대의 현재 추정 위치로 이동하되, 선형 보간법을 사용해 현재 위치와 목표 위치의 중간 위치 지정

            transform.position = Vector3.MoveTowards(transform.position, newDes, (Speed + currentDistance) * Time.deltaTime ); //거리 차이만큼의 가산점을 더해 최대 속도로 해서 부드럽게 동기화. 아직 더 정교하게는 못하겠다.

            TargetPosition += new Vector3(MoveVector.x, MoveVector.y, 0) * Speed * Time.deltaTime; //일단 상대 클라의 다음 프레임 추정 위치 예측

            if (currentDistance < 0.5f) //거의 일치하는 시점에서 움직임 정지
            {
                IsLerping = false;  
            }   
        }
        else //오차 조정 완료 후, 이동 벡터에 맞게 움직여줘야 함
        {
            transform.position += new Vector3(MoveVector.x, MoveVector.y, 0) * Speed * Time.deltaTime;
        }
    }


    //벽에 닿은 상태에서 움직일때, 충돌지점을 계산해 이동 벡터를 조정해주는 함수
    //타 플레이어 캐릭터의 경우는 처음 벽에 닿을 때만 하면 될 듯. 어차피 다른 플레이어도 이동 패킷 보내기 전에 계산하고 이동하기 때문에
    //자기 플레이어 캐릭터인 경우는 벽에 닿을 때, 이동 방향이 바뀔때마다 호출해줘야 하고, 벽에서 벗어날 때는 원래 이동 방향으로 되돌려줘야 함

    protected void MoveAttachedOnWall()
    {
        foreach (Collider col in touchingColliders)
        {
            //충돌 지점에서 현재 오브젝트의 벡터를 뺀 -> 충돌 지점의 벽과 수직이 되는 방향 벡터를 가져옵니다.
            Vector3 wallNormal = transform.position - col.ClosestPoint(transform.position);
            wallNormal.Normalize();
            Vector2 wallNormal2D = new Vector2(wallNormal.x, wallNormal.y);

            // MoveVector를 법선벡터와 내적
            float dot = Vector2.Dot(wallNormal, MoveVector);

            // 내적 값이 플러스다 -> 코사인 세타가 플러스 -> 같은 방향 -> 벽으로부터 멀어진다
            // 내적 값이 마이너스다 -> 두 벡터가 둔각을 이룬다 -> 즉 MoveVector가 벽을 향해 간다는 것

            if (dot < 0)
            {
                MoveVector -= wallNormal2D * dot; //이동벡터에서 정사영의 크기만한 법선벡터만큼 추가로 이동 -> 벽을 향해 가는 성분 제거
            }
        }
    }

}
