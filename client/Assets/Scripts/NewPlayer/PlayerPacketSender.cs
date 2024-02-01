using System;
using UnityEngine;

//네트워크 플레이 시, 플레이어가 움직이고 섭취할 때마다 해당하는 패킷 전송.
public class PlayerPacketSender : MonoBehaviour
{
    private NetworkManager network;
    private NewPlayer player;
    private Mover myMover;
    private Eater myEater;

    private Vector2 moveVector = Vector2.zero;
    private int eatFoodId = -1;
    private int eatPreyId = -1;

    private void Awake()
    {
        network = NetworkManager.Instance;
        player = GetComponent<NewPlayer>();
        myMover = GetComponent<Mover>();
        myEater = GetComponent<Eater>();
    }

    public void Update()
    {
        if (moveVector != myMover.MoveVector) //이동 벡터가 직전과 달라지면 이동 패킷 전송
        {
            SendMovePacket();
            moveVector = myMover.MoveVector;
        }
        if (eatFoodId != myEater.EatFoodId) //음식 먹은거 확인되면 패킷 전송
        {
            SendEatPacket(myEater.EatFoodId);
            eatFoodId = myEater.EatFoodId;    
        }
        if (eatPreyId != myEater.EatPreyId) //음식 먹은거 확인되면 패킷 전송
        {
            SendEatPlayerPacket(myEater.EatFoodId);
            eatPreyId = myEater.EatPreyId;    
        }
    }

    private void SendMovePacket()
    {
        C_Move movePacket = new C_Move();

        DateTime now = DateTime.UtcNow;
        float sendTime = now.Hour * 3600 + now.Minute * 60 + now.Second + now.Millisecond * 0.001f;

        movePacket.time = sendTime;
        movePacket.dirX = myMover.MoveVector.x;
        movePacket.dirY = myMover.MoveVector.y;
        movePacket.posX = transform.position.x;
        movePacket.posY = transform.position.y;
        movePacket.posZ = 0;

        network.Send(movePacket.Write());
    }

    private void SendEatPacket(int foodId)
    {
        C_EatFood eatPacket = new C_EatFood();
        eatPacket.foodId = foodId;
        network.Send(eatPacket.Write());
    }

    private void SendEatPlayerPacket(int preyId)
    {
        C_EatPlayer eatPlayerPacket = new C_EatPlayer();
        eatPlayerPacket.predatorId = player.PlayerId;
        eatPlayerPacket.preyId = preyId;
        network.Send(eatPlayerPacket.Write());
    }
}