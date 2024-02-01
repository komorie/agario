using UnityEngine;

//��Ʈ��ũ�� ������ �ʴ� �÷��̾��� ����, �÷��̾� ���븦 ����ϴ� ������Ʈ
public class NormalEater : Eater
{
    protected void OnTriggerEnter(Collider other)
    {
        Food food;
        // �浹�� ��ü�� 'Food'
        if (other.TryGetComponent(out food) == true)
        {
            transform.localScale += 0.1f * Vector3.one;
            Radius = transform.localScale.x * 0.5f;
            EatFoodId = food.FoodId;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        Player prey;
        if (other.TryGetComponent(out prey) == true) //��� �÷��̾�� ���ƴ�
        {
            Debug.Log($"�Ÿ�: {Vector3.Distance(prey.transform.position, transform.position)}");
            if (prey.Radius < Radius && Vector3.Distance(prey.transform.position, transform.position) < Radius)
            {
                transform.localScale += (prey.transform.localScale / 2); //���� �÷��̾� ũ�� �ݸ�ŭ ���� �÷��̾� ũ�� ����
                Radius = transform.localScale.x * 0.5f;   //������ Ű���
                Destroy(prey.gameObject); //���� �÷��̾� ������Ʈ ����
                EatPreyId = prey.PlayerId;
            }
        }
    }
}
