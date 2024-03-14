using UnityEngine;

//네트워크 플레이 시 자신 플레이어의 음식, 플레이어 섭취를 담당하는 컴포넌트
public class OldNetMyEater : OldEater
{
    Food eatenFood;
    OldPlayer myPlayer; 
    OldPlayer eatenPlayer;
    private PlayerPacketSender packetSender;
    private PacketReceiver packetReceiver;

    private void Awake()
    {
        packetReceiver = PacketReceiver.Instance;
        packetSender = GetComponent<PlayerPacketSender>();
        myPlayer = GetComponent<OldPlayer>();
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

    protected void OnTriggerEnter(Collider other)
    {
        // 충돌한 객체가 'Food'
        if (other.TryGetComponent(out eatenFood) == true)
        {
            packetSender.SendEatPacket(eatenFood.FoodId);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (eatenPlayer != null) return;

        if (other.TryGetComponent(out eatenPlayer) == true) //상대 플레이어랑 거리 내에서 겹쳤는지 확인
        {
            if (eatenPlayer.PlayerEater.Radius < Radius && Vector3.Distance(eatenPlayer.transform.position, transform.position) < Radius)
            {
                packetSender.SendEatPlayerPacket(myPlayer.PlayerId, eatenPlayer.PlayerId);
            }
            else
            {
                eatenPlayer = null;
            }
        }
    }

    private void RecvEatFood(S_BroadcastEatFood p)
    {
        if(eatenFood != null && p.playerId == myPlayer.PlayerId)
        {
            Radius += 0.05f;
            transform.localScale = new Vector3(Radius * 2, Radius * 2, Radius * 2);
            OnEatFood(myPlayer.PlayerId, eatenFood.FoodId, p);
            eatenFood = null;
        }
    }
    private void RecvEatPlayer(S_BroadcastEatPlayer p)
    {
        if (eatenPlayer != null && p.predatorId == myPlayer.PlayerId)
        {
            Radius += (eatenPlayer.PlayerEater.Radius / 2); //먹힌 플레이어 크기 반만큼 먹은 플레이어 크기 증가
            transform.localScale = new Vector3(Radius * 2, Radius * 2, Radius * 2);
            OnEatPlayer(myPlayer.PlayerId, eatenPlayer.PlayerId, p);
            Destroy(eatenPlayer.gameObject);
            eatenPlayer = null;
        }
    }
}
