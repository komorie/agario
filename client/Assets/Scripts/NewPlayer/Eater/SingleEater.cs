using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//�̱� �÷��� �� ����, �÷��̾� ���븦 ����ϴ� ������Ʈ
public class SingleEater : Eater
{
    protected void OnTriggerEnter(Collider other)
    {
        Food food;
        // �浹�� ��ü�� 'Food'
        if (other.TryGetComponent(out food) == true)
        {
            Radius += 0.05f;
            transform.localScale = new Vector3(Radius * 2, Radius * 2, Radius * 2);
            OnEatFood(food.FoodId);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        NewPlayer prey;
        if (other.TryGetComponent(out prey) == true) //��� �÷��̾�� ���ƴ�
        {
            if (prey.PlayerEater.Radius < Radius && Vector3.Distance(prey.transform.position, transform.position) < Radius)
            {
                Radius += (prey.PlayerEater.Radius / 2); //���� �÷��̾� ũ�� �ݸ�ŭ ���� �÷��̾� ũ�� ����
                transform.localScale = new Vector3(Radius * 2, Radius * 2, Radius * 2);
                OnEatPlayer(prey.PlayerId);
                Destroy(prey.gameObject); //���� �÷��̾� ������Ʈ ����
            }
        }
    }
}
