using UnityEngine;

//싱글 플레이 시 음식, 플레이어 섭취를 담당하는 컴포넌트
public class OldSingleEater : OldEater
{
    private OldPlayer myPlayer;

    private void Awake()
    {
        myPlayer = GetComponent<OldPlayer>();
    }

    protected void OnTriggerEnter(Collider other)
    {
        Food food;
        // 충돌한 객체가 'Food'
        if (other.TryGetComponent(out food) == true)
        {
            Radius += 0.1f;
            transform.localScale = new Vector3(Radius * 2, Radius * 2, Radius * 2);
            OnEatFood(myPlayer.PlayerId, food.FoodId);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        OldPlayer prey;
        if (other.TryGetComponent(out prey) == true) //상대 플레이어랑 겹쳤다
        {
            if (prey.PlayerEater.Radius < Radius && Vector3.Distance(prey.transform.position, transform.position) < Radius)
            {
                Radius += (prey.PlayerEater.Radius / 2); //먹힌 플레이어 크기 반만큼 먹은 플레이어 크기 증가
                transform.localScale = new Vector3(Radius * 2, Radius * 2, Radius * 2);
                OnEatPlayer(myPlayer.PlayerId, prey.PlayerId);
                Destroy(prey.gameObject); //먹힌 플레이어 오브젝트 삭제
            }
        }
    }
}
