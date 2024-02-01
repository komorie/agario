using UnityEngine;


//네트워크를 통한 플레이어의 음식, 플레이어 섭취를 담당하는 컴포넌트
public class NetworkEater : Eater
{
    private NewRoom room;
    private NewPlayer player;
    private PacketReceiver packetReceiver;

    private void Awake()
    {
        room = NewRoom.Instance;
        player = GetComponent<NewPlayer>();
        packetReceiver = PacketReceiver.Instance;
    }

    private void OnEnable()
    {
        packetReceiver.OnBroadcastEatFood += RecvEatFood;
        packetReceiver.OnBroadcastEatPlayer += RecvEatPlayer;
    }

    private void OnDisable()
    {
        packetReceiver.OnBroadcastEatFood -= RecvEatFood;
        packetReceiver.OnBroadcastEatPlayer -= RecvEatPlayer;
    }

    public void RecvEatFood(S_BroadcastEatFood p) //음식 먹었다는 패킷 받음
    {
        if (p.playerId == player.PlayerId) //먹은 플레이어면 크기 올리기
        {
            transform.localScale += 0.1f * Vector3.one;
            Radius = transform.localScale.x * 0.5f;
            EatFoodId = p.foodId;
        }
    }

    //패킷에 크기 들어가게 하는것도
    public void RecvEatPlayer(S_BroadcastEatPlayer p) //플레이어 먹었다는 패킷 받음
    {
        NewPlayer prey;

        if (p.predatorId == player.PlayerId)
        {
            if (room.Players.TryGetValue(p.preyId, out prey))
            {
                player.transform.localScale += (prey.transform.localScale / 2); //먹힌 플레이어 크기 반만큼 먹은 플레이어 크기 증가
                Radius = transform.localScale.x * 0.5f;   //반지름 키우고
                Destroy(prey.gameObject); //먹힌 플레이어 오브젝트 삭제
                EatPreyId = p.preyId;
            }
        }
    }
}
