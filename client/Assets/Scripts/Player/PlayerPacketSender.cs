using System;
using UnityEngine;

//네트워크 플레이 시 상황에 맞는 패킷 전송.
public class PlayerPacketSender : MonoBehaviour
{
    private NetworkManager network;

    private void Awake()
    {
        network = NetworkManager.Instance;
    }

    public void SendMovePacket(Vector2 moveVector)
    {
        C_Move movePacket = new C_Move();

        DateTime now = DateTime.UtcNow;
        float sendTime = now.Hour * 3600 + now.Minute * 60 + now.Second + now.Millisecond * 0.001f;

        movePacket.time = sendTime;
        movePacket.dirX = moveVector.x;
        movePacket.dirY = moveVector.y;
        movePacket.posX = transform.position.x;
        movePacket.posY = transform.position.y;
        movePacket.posZ = 0;

        network.Send(movePacket.Write());
    }

    public void SendEatPacket(int foodId)
    {
        C_EatFood eatPacket = new C_EatFood();
        eatPacket.foodId = foodId;
        network.Send(eatPacket.Write());
    }

    public void SendEatPlayerPacket(int predatorId, int preyId)
    {
        C_EatPlayer eatPlayerPacket = new C_EatPlayer();
        eatPlayerPacket.predatorId = predatorId;
        eatPlayerPacket.preyId = preyId;
        network.Send(eatPlayerPacket.Write());
    }
}