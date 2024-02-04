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
        if (IsLerping) //�ϴ� ��Ŷ�� �ͼ� ������ �����ؾ� �ϴ� ���
        {

            float currentDistance = Vector3.Distance(transform.position, TargetPosition); //��� Ŭ�� ������ ���� ��ġ(+���� �� ��ġ)�� �������� �� Ŭ�� �����ؼ� �̵���Ų ��ġ�� �Ÿ� ��

            Vector3 newDes = Vector3.Lerp(transform.position, TargetPosition, 0.5f);   //����� ���� ���� ��ġ�� �̵��ϵ�, ���� �������� ����� ���� ��ġ�� ��ǥ ��ġ�� �߰� ��ġ ����

            transform.position = Vector3.MoveTowards(transform.position, newDes, (Speed + currentDistance) * Time.deltaTime ); //�Ÿ� ���̸�ŭ�� �������� ���� �ִ� �ӵ��� �ؼ� �ε巴�� ����ȭ. ���� �� �����ϰԴ� ���ϰڴ�.

            TargetPosition += new Vector3(MoveVector.x, MoveVector.y, 0) * Speed * Time.deltaTime; //�ϴ� ��� Ŭ���� ���� ������ ���� ��ġ ����

            if (currentDistance < 0.5f) //���� ��ġ�ϴ� �������� ������ ����
            {
                IsLerping = false;  
            }   
        }
        else //���� ���� �Ϸ� ��, �̵� ���Ϳ� �°� ��������� ��
        {
            transform.position += new Vector3(MoveVector.x, MoveVector.y, 0) * Speed * Time.deltaTime;
        }
    }


    //���� ���� ���¿��� �����϶�, �浹������ ����� �̵� ���͸� �������ִ� �Լ�
    //Ÿ �÷��̾� ĳ������ ���� ó�� ���� ���� ���� �ϸ� �� ��. ������ �ٸ� �÷��̾ �̵� ��Ŷ ������ ���� ����ϰ� �̵��ϱ� ������
    //�ڱ� �÷��̾� ĳ������ ���� ���� ���� ��, �̵� ������ �ٲ𶧸��� ȣ������� �ϰ�, ������ ��� ���� ���� �̵� �������� �ǵ������ ��

    protected void MoveAttachedOnWall()
    {
        foreach (Collider col in touchingColliders)
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
            }
        }
    }

}
