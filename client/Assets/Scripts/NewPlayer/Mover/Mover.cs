using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//모든 물체들의 공통적인 이동 처리(벽) 를 담당하는 컴포넌트
public abstract class Mover : MonoBehaviour
{
    public int Speed { get; set; } = 20;
    public Vector2 MoveVector { get; set; }

    public event Action<Vector2> MoveVectorChanged;

    public HashSet<Collider> TouchingColliders { get; set; } = new HashSet<Collider>();

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wall"))
        {
            TouchingColliders.Add(other);
            MoveAttachedOnWall();
        }
     }
    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Wall"))
        {
            TouchingColliders.Remove(other); 
        }
    }

    protected virtual void OnMoveVectorChanged(Vector2 moveVector) => MoveVectorChanged?.Invoke(moveVector);

    //벽에 닿은 상태에서 움직일때, 충돌지점을 계산해 이동 벡터를 조정해주는 함수
    protected void MoveAttachedOnWall()
    {
        foreach (Collider col in TouchingColliders)
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
                OnMoveVectorChanged(MoveVector);
            }
        }
    }
}
