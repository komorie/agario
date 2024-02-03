using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//��� ��ü���� �������� �̵� ó��(��) �� ����ϴ� ������Ʈ
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

    //���� ���� ���¿��� �����϶�, �浹������ ����� �̵� ���͸� �������ִ� �Լ�
    protected void MoveAttachedOnWall()
    {
        foreach (Collider col in TouchingColliders)
        {
            //�浹 �������� ���� ������Ʈ�� ���͸� �� -> �浹 ������ ���� ������ �Ǵ� ���� ���͸� �����ɴϴ�.
            Vector3 wallNormal = transform.position - col.ClosestPoint(transform.position);
            wallNormal.Normalize();
            Vector2 wallNormal2D = new Vector2(wallNormal.x, wallNormal.y);

            // MoveVector�� �������Ϳ� ����
            float dot = Vector2.Dot(wallNormal, MoveVector);

            // ���� ���� �÷����� -> �ڻ��� ��Ÿ�� �÷��� -> ���� ���� -> �����κ��� �־�����
            // ���� ���� ���̳ʽ��� -> �� ���Ͱ� �а��� �̷�� -> �� MoveVector�� ���� ���� ���ٴ� ��

            if (dot < 0)
            {
                MoveVector -= wallNormal2D * dot; //�̵����Ϳ��� ���翵�� ũ�⸸�� �������͸�ŭ �߰��� �̵� -> ���� ���� ���� ���� ����
                OnMoveVectorChanged(MoveVector);
            }
        }
    }
}
