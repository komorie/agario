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
        if (IsLerping) //�ϴ� ��Ŷ�� �ͼ� ������ �����ؾ� �ϴ� ���
        {

            float currentDistance = Vector3.Distance(transform.position, TargetPosition); //��� Ŭ�� ������ ���� ��ġ�� �� Ŭ�� �����ؼ� �̵���Ų ��ġ�� �Ÿ� ��

            Debug.Log($"�÷��̾� {PlayerId} ���� ����: {currentDistance}");

            if(currentDistance > 10f) //������ �ʹ� ũ�� �׳� �� ��ġ�� �̵�
            {
                transform.position = TargetPosition;
                IsLerping = false;
                return;
            }   

            TargetPosition += new Vector3(MoveVector.x, MoveVector.y, 0) * Speed * Time.deltaTime; //�ϴ� ��� Ŭ���� ���� ���� ��ġ ����

            Vector3 newDes = Vector3.Lerp(transform.position, TargetPosition, 0.5f);   //����� ���� ���� ��ġ�� �̵��ϵ�, ���� �������� ����� ���� ��ġ�� ��ǥ ��ġ�� �߰� ��ġ ����

            transform.position = Vector3.MoveTowards(transform.position, newDes, Speed * Time.deltaTime); //�ű�� �̵�

            if (currentDistance < 0.01f) //���� ��ġ�ϴ� �������� ������ ����
            {
                IsLerping = false;  
            }   
        }
        else //���� ���� �Ϸ� ��, �̵� ���Ϳ� �°� ��������� ��
        {
            transform.position += new Vector3(MoveVector.x, MoveVector.y, 0) * Speed * Time.deltaTime;
        }
    }

/*    public virtual IEnumerator Distance1F()
    {
        //1�ʴ� �̵��Ÿ� üũ
        while (true)
        {
            Vector3 oldPos = transform.position;
            yield return new WaitForSeconds(1.0f);
            Vector3 newPos = transform.position;
            float distance = Vector3.Distance(oldPos, newPos);
            Debug.Log($"�÷��̾� {PlayerId} 1�ʴ� �̵��Ÿ�: {distance}");
        }
    }*/
}
