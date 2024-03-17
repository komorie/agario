using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

//네트워크 플레이 시 상황에 맞는 패킷 전송.
public class PacketSender : MonoBehaviour
{
    private NetworkManager network;

    private void Awake()
    {
        network = NetworkManager.Instance;
    }

    public void SendMovePacket(Vector2 moveVector)
    {
        C_Move movePacket = new C_Move();

        movePacket.time = CustomTimer.Instance.StopWatch.ElapsedSeconds;
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

    public void SendBeamStartPacket(Vector2 vec)
    {
        C_BeamStart beamStartPacket = new C_BeamStart();
        beamStartPacket.dirX = vec.x;
        beamStartPacket.dirY = vec.y;   
        network.Send(beamStartPacket.Write());      
    }

    public void SendBeamHitPacket(List<int> players)
    {
        C_BeamHit beamHitPacket = new C_BeamHit();  
        foreach (int id in players)
        {
            C_BeamHit.HitPlayer p = new C_BeamHit.HitPlayer();
            p.playerId = id;
            beamHitPacket.hitPlayers.Add(p);
        }
        network.Send(beamHitPacket.Write());    
    }

    public void SendStealthPacket(int userId)
    {
        C_Stealth stealth = new C_Stealth();
        stealth.userId = userId;
        network.Send(stealth.Write());
    }
}