using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static S_PlayerList;

public class Player : MonoBehaviour
{
    public int PlayerId { get; set; }
    public int Speed { get; set; }

    public bool IsMoving { get; set; } = false;
    public bool IsLerping { get; set; } = false;    
    public Vector2 MoveVector { get; set; }
    public Vector3 TargetPosition { get; set; }


    protected virtual void Awake()
    {
        Speed = 20;
        TargetPosition = transform.position;
/*        StartCoroutine(Distance1F());*/
    }

    protected virtual void Update()
    {
        if (IsLerping) //일단 패킷이 와서 오차를 조정해야 하는 경우
        {

            float currentDistance = Vector3.Distance(transform.position, TargetPosition); //상대 클라가 서버로 보낸 위치와 내 클라가 추측해서 이동시킨 위치의 거리 비교

            Debug.Log($"플레이어 {PlayerId} 현재 오차: {currentDistance}");

            if(currentDistance > 10f) //오차가 너무 크면 그냥 그 위치로 이동
            {
                transform.position = TargetPosition;
                IsLerping = false;
                return;
            }   

            TargetPosition += new Vector3(MoveVector.x, MoveVector.y, 0) * Speed * Time.deltaTime; //일단 상대 클라의 현재 추정 위치 예측

            Vector3 newDes = Vector3.Lerp(transform.position, TargetPosition, 0.5f);   //상대의 현재 추정 위치로 이동하되, 선형 보간법을 사용해 현재 위치와 목표 위치의 중간 위치 지정

            transform.position = Vector3.MoveTowards(transform.position, newDes, Speed * Time.deltaTime); //거기로 이동

            if (currentDistance < 0.01f) //거의 일치하는 시점에서 움직임 정지
            {
                IsLerping = false;  
            }   
        }
        else //오차 조정 완료 후, 이동 벡터에 맞게 움직여줘야 함
        {
            transform.position += new Vector3(MoveVector.x, MoveVector.y, 0) * Speed * Time.deltaTime;
        }
    }

/*    public virtual IEnumerator Distance1F()
    {
        //1초당 이동거리 체크
        while (true)
        {
            Vector3 oldPos = transform.position;
            yield return new WaitForSeconds(1.0f);
            Vector3 newPos = transform.position;
            float distance = Vector3.Distance(oldPos, newPos);
            Debug.Log($"플레이어 {PlayerId} 1초당 이동거리: {distance}");
        }
    }*/
}
