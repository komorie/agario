using UnityEngine;

//네트워크를 통하지 않는 플레이어의 음식, 플레이어 섭취를 담당하는 컴포넌트
public class NormalEater : Eater
{
    protected void OnTriggerEnter(Collider other)
    {
        Food food;
        // 충돌한 객체가 'Food'
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
        if (other.TryGetComponent(out prey) == true) //상대 플레이어랑 겹쳤다
        {
            Debug.Log($"거리: {Vector3.Distance(prey.transform.position, transform.position)}");
            if (prey.Radius < Radius && Vector3.Distance(prey.transform.position, transform.position) < Radius)
            {
                transform.localScale += (prey.transform.localScale / 2); //먹힌 플레이어 크기 반만큼 먹은 플레이어 크기 증가
                Radius = transform.localScale.x * 0.5f;   //반지름 키우고
                Destroy(prey.gameObject); //먹힌 플레이어 오브젝트 삭제
                EatPreyId = prey.PlayerId;
            }
        }
    }
}
