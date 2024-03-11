using UnityEngine;


//네트워크 플레이 시 타 플레이어의 음식, 플레이어 섭취를 담당하는 컴포넌트
public class NetOtherEater : Eater
{
    private Room room;
    private Player myPlayer;
    private PacketReceiver packetReceiver;

    private void Awake()
    {
        room = Room.Instance;
        myPlayer = GetComponent<Player>();
        packetReceiver = PacketReceiver.Instance;
    }

    private void OnEnable()
    {
        packetReceiver.OnBroadcastEatFood += RecvEatFood; //음식 먹었다는 패킷 받을 시 실행
        packetReceiver.OnBroadcastEatPlayer += RecvEatPlayer; //플레이어 먹었다는 패킷 받을 시 실행
    }

    private void OnDisable()
    {
        packetReceiver.OnBroadcastEatFood -= RecvEatFood;
        packetReceiver.OnBroadcastEatPlayer -= RecvEatPlayer;
    }

    public void RecvEatFood(S_BroadcastEatFood p) //음식 섭취 처리
    {
        if(p.playerId == myPlayer.PlayerId)
        {
            Radius += 0.05f;
            transform.localScale = new Vector3(Radius * 2, Radius * 2, Radius * 2);
            OnEatFood(myPlayer.PlayerId, p.foodId, p);
        }
    }

    public void RecvEatPlayer(S_BroadcastEatPlayer p) //플레이어 포식 처리
    {
        Player prey;
        if (p.predatorId == myPlayer.PlayerId)
        {
            if (room.Players.TryGetValue(p.preyId, out prey))
            { 
                Radius += (prey.PlayerEater.Radius / 2); //먹힌 플레이어 크기 반만큼 먹은 플레이어 크기 증가
                transform.localScale = new Vector3(Radius * 2, Radius * 2, Radius * 2);
                OnEatPlayer(myPlayer.PlayerId, p.preyId, p);
                Destroy(prey.gameObject); //먹힌 플레이어 오브젝트 삭제
            }
        }
    }
}
