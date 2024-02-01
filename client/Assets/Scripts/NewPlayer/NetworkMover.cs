using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static S_RoomList;
using static UnityEditor.Experimental.GraphView.GraphView;

//타 플레이어가 네트워크를 통해 움직이는 물체의 이동 처리를 담당하는 컴포넌트
public class NetworkMover : Mover
{

    private NewPlayer player;
    private PacketReceiver packetReceiver;
    private bool isLerping = false;
    private Vector3 targetPosition;

    private void Awake()
    {
        player = GetComponent<NewPlayer>(); 
        packetReceiver = PacketReceiver.Instance;   
    }

    private void OnEnable()
    {
        packetReceiver.OnBroadcastMove += RecvBroadcastMove;
    }

    private void OnDisable()
    {
        packetReceiver.OnBroadcastMove -= RecvBroadcastMove;
    }
    public void Update()
    {
        if (isLerping) //일단 패킷이 와서 오차를 조정해야 하는 경우
        {

            float currentDistance = Vector3.Distance(transform.position, targetPosition); //상대 클라가 서버로 보낸 위치(+예측 들어간 위치)와 그전까지 내 클라가 추측해서 이동시킨 위치의 거리 비교

            Vector3 newDes = Vector3.Lerp(transform.position, targetPosition, 0.5f);   //상대의 현재 추정 위치로 이동하되, 선형 보간법을 사용해 현재 위치와 목표 위치의 중간 위치 지정

            transform.position = Vector3.MoveTowards(transform.position, newDes, (Speed + currentDistance) * Time.deltaTime); //거리 차이만큼의 가산점을 더해 최대 속도로 해서 부드럽게 동기화. 아직 더 정교하게는 못하겠다.

            targetPosition += new Vector3(MoveVector.x, MoveVector.y, 0) * Speed * Time.deltaTime; //일단 상대 클라의 다음 프레임 추정 위치 예측

            if (currentDistance < 0.5f) //거의 일치하는 시점에서 움직임 정지
            {
                isLerping = false;
            }
        }
        else //오차 조정 완료 후, 이동 벡터에 맞게 움직여줘야 함
        {
            transform.position += new Vector3(MoveVector.x, MoveVector.y, 0) * Speed * Time.deltaTime;
        }
    }

    private void RecvBroadcastMove(S_BroadcastMove p)
    {
        //온 패킷대로 위치 조정
        if (player.PlayerId == p.playerId)
        {
            DateTime now = DateTime.UtcNow;
            float currentSecond = now.Hour * 3600 + now.Minute * 60 + now.Second + now.Millisecond * 0.001f;
            float RTT = currentSecond - p.time;

            MoveVector = new Vector2(p.dirX, p.dirY); //다른 플레이어의 이동 방향
            targetPosition = new Vector3(p.posX + p.dirX * Speed * RTT, p.posY + p.dirY * Speed * RTT, p.posZ);
            //다른 플레이어의 실제 위치 예측
            //Myplayer는 프레임당 dir * Speed * Time.deltaTime 만큼 더한 위치로 이동하며 Time.deltaTime은 프레임당 흐른 시간초이다. 즉 dir * Speed는 1초당 이동한 속도가 된다. 즉 1초당 이동거리가 20
            //그럼 dir * Speed * RTT는 RTT초 만큼 이동한 위치이다. 서버에서 온 위치 + dir * Speed * RTT 가 상대의 현재 위치라고 가정하고, 내 클라에서 돌린 위치랑 보간을 실시
            isLerping = true;
        }
    }

}
