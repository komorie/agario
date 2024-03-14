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

    //���� ���� ���¿��� �����϶�, �浹������ ����� �̵� ���͸� �������ִ� �Լ�
    public Vector2 WallCalculate(Vector2 vec)
    {
        foreach (Collider col in touchingColliders)
        {
            //�浹 �������� ���� ������Ʈ�� ���͸� �� -> �浹 ������ ���� ������ �Ǵ� ���� ���͸� �����ɴϴ�.
            Vector3 wallNormal = transform.position - col.ClosestPoint(transform.position);
            wallNormal.Normalize();
            Vector2 wallNormal2D = new Vector2(wallNormal.x, wallNormal.y);

            // MoveVector�� �������Ϳ� ����
            float dot = Vector2.Dot(wallNormal, vec);

            // ���� ���� �÷����� -> �ڻ��� ��Ÿ�� �÷��� -> ���� ���� -> �����κ��� �־�����
            // ���� ���� ���̳ʽ��� -> �� ���Ͱ� �а��� �̷�� -> �� MoveVector�� ���� ���� ���ٴ� ��

            if (dot < 0)
            {
                vec -= wallNormal2D * dot; //�̵����Ϳ��� ���翵�� ũ�⸸�� �������͸�ŭ �߰��� �̵� -> ���� ���� ���� ���� ����
            }
        }

        return vec;
    }

    public Vector2 Move(Vector2 vec) //�̵�
    {
        if (isLerping)
        {
            vec = Vector2.zero;
        }

        transform.position += new Vector3(vec.x, vec.y, 0) * speed * Time.deltaTime; //���� ���Ͱ��� �ӵ��� ���� �̵�

        isDirChanged = (moveVector != vec); //�̵� ������ �ٲ������
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

    public IEnumerator Lerp(Vector2 vec, Vector3 lastPos, float lastTime) //���� ����� Ÿ �÷��̾��� ��ġ�� �������� Ÿ �÷��̾��� ��ġ�� �ٸ� ��� �̵� ���� ����
    {
        DateTime now = DateTime.UtcNow;
        float currentSecond = now.Hour * 3600 + now.Minute * 60 + now.Second + now.Millisecond * 0.001f;
        float RTT = currentSecond - lastTime; //Ÿ �÷��̾ �̵��� ������ �ð��� ���� �ð��� ����

        Vector3 target = lastPos + new Vector3(vec.x, vec.y, 0) * speed * RTT;  //���������� �� ��ġ���� �ҿ� �ð��� ���� �̵� ���͸� ���� ����� �ֽ� ���� ��ġ
        Vector3 current = transform.position += new Vector3(vec.x, vec.y, 0) * speed * RTT; //���� ������ ���� ��ġ���� �̵���ų ��ġ

        while (isLerping)
        {
            float currentDistance = Vector3.Distance(current, target); //�ֽ� ���� ��ġ�� ���� �÷��̾��� ��ġ�� �Ÿ� ����

            if (currentDistance > 0.1f)
            {
                target += new Vector3(vec.x, vec.y, 0) * speed * Time.deltaTime; //���� �������� �ֽ� ���� ��ġ
                current = transform.position += new Vector3(vec.x, vec.y, 0) * speed * Time.deltaTime; //���� �������� ���� ������ ��ġ
                Vector3 newDes = Vector3.Lerp(current, target, 0.5f); //�ֽ� ���� ��ġ�� �̵��ϵ�, ���� ������ ����� �� ���� ��ġ�� �ֽ� ���� ��ġ�� �߰� ��ġ ����
                transform.position = Vector3.MoveTowards(transform.position, newDes, speed * Time.deltaTime * 3); //�߰� ��ġ�� �̵�
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
