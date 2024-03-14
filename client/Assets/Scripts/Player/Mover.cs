using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Mover : MonoBehaviour
{
    private int speed = 20;
    private bool isDirChanged = false;
    private bool isLerping = false;
    private Vector2 moveVector = Vector2.zero;
    private HashSet<Collider> touchingColliders = new HashSet<Collider>();
    private Coroutine currentLerp = null;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wall"))
        {
            touchingColliders.Add(other);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Wall"))
        {
            touchingColliders.Remove(other);
        }
    }

    //벽에 닿은 상태에서 움직일때, 충돌지점을 계산해 이동 벡터를 조정해주는 함수
    public Vector2 WallCalculate(Vector2 vec)
    {
        foreach (Collider col in touchingColliders)
        {
            //충돌 지점에서 현재 오브젝트의 벡터를 뺀 -> 충돌 지점의 벽과 수직이 되는 방향 벡터를 가져옵니다.
            Vector3 wallNormal = transform.position - col.ClosestPoint(transform.position);
            wallNormal.Normalize();
            Vector2 wallNormal2D = new Vector2(wallNormal.x, wallNormal.y);

            // MoveVector를 법선벡터와 내적
            float dot = Vector2.Dot(wallNormal, vec);

            // 내적 값이 플러스다 -> 코사인 세타가 플러스 -> 같은 방향 -> 벽으로부터 멀어진다
            // 내적 값이 마이너스다 -> 두 벡터가 둔각을 이룬다 -> 즉 MoveVector가 벽을 향해 간다는 것

            if (dot < 0)
            {
                vec -= wallNormal2D * dot; //이동벡터에서 정사영의 크기만한 법선벡터만큼 추가로 이동 -> 벽을 향해 가는 성분 제거
            }
        }

        return vec;
    }

    public Vector2 Move(Vector2 vec) //이동
    {
        if (isLerping)
        {
            vec = Vector2.zero;
        }

        transform.position += new Vector3(vec.x, vec.y, 0) * speed * Time.deltaTime; //계산된 벡터값과 속도에 따라 이동

        isDirChanged = (moveVector != vec); //이동 방향이 바뀌었는지
        moveVector = vec;

        return vec;

    }

    public void StartLerp(Vector2 vec, Vector3 lastPos, float lastTime)
    {
        if(currentLerp != null)
        {
            StopCoroutine(currentLerp);
        }

        isLerping = true;
        currentLerp = StartCoroutine(Lerp(vec, lastPos, lastTime));
    }

    public IEnumerator Lerp(Vector2 vec, Vector3 lastPos, float lastTime) //내가 계산한 타 플레이어의 위치와 서버상의 타 플레이어의 위치가 다른 경우 이동 벡터 조절
    {
        DateTime now = DateTime.UtcNow;
        float currentSecond = now.Hour * 3600 + now.Minute * 60 + now.Second + now.Millisecond * 0.001f;
        float RTT = currentSecond - lastTime; //타 플레이어가 이동을 시작한 시간과 현재 시간의 차이

        Vector3 target = lastPos + new Vector3(vec.x, vec.y, 0) * speed * RTT;  //서버상으로 온 위치에서 소요 시간에 따른 이동 벡터를 더해 계산한 최신 예측 위치
        Vector3 current = transform.position += new Vector3(vec.x, vec.y, 0) * speed * RTT; //내가 추측한 현재 위치에서 이동시킬 위치

        while (isLerping)
        {
            float currentDistance = Vector3.Distance(current, target); //최신 예측 위치와 현재 플레이어의 위치의 거리 차이

            if (currentDistance > 0.1f)
            {
                target += new Vector3(vec.x, vec.y, 0) * speed * Time.deltaTime; //다음 프레임의 최신 예측 위치
                current = transform.position += new Vector3(vec.x, vec.y, 0) * speed * Time.deltaTime; //다음 프레임의 내가 추측한 위치
                Vector3 newDes = Vector3.Lerp(current, target, 0.5f); //최신 예측 위치로 이동하되, 선형 보간을 사용해 내 추측 위치와 최신 추측 위치의 중간 위치 지정
                transform.position = Vector3.MoveTowards(transform.position, newDes, speed * Time.deltaTime * 3); //중간 위치로 이동
            }
            else
            {
                isLerping = false;
                currentLerp = null;
            }

/*            Debug.Log($"{isLerping} {vec.x} {vec.y} {currentDistance}");*/

            yield return null;
        }
    }

    public bool IsDirChanged()
    {
        return isDirChanged;
    }

}
