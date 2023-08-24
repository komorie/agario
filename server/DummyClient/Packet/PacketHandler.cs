using Core;
using DummyClient.Session;
using System;
using System.Collections.Generic;


internal class PacketHandler //패킷의 생성 과정에 신경 쓸 필요 없이, 해당 패킷으로 할 동작만 여기서 구현해 주면 될 것.
{
    public static void S_BroadcastEnterGameHandler(PacketSession session, IPacket packet)
    {

    }

    public static void S_BroadcastLeaveGameHandler(PacketSession session, IPacket packet)
    {
      
    }

    public static void S_PlayerListHandler(PacketSession session, IPacket packet)
    {
        S_PlayerList playerList = packet as S_PlayerList; 
        ServerSession s = session as ServerSession;

        //플레이어 리스트 순환하며 값 출력
        foreach (S_PlayerList.Player p in playerList.players)
        {
            if(p.isSelf) //자신인 경우 -> 세션 ID와 포지션 서버에서 받아온 걸로 지정
            {
                s.SessionId = p.playerId;
                s.PosX = p.posX;
                s.PosY = p.posY;
                s.PosZ = p.posZ;
                Console.WriteLine($"Player({p.playerId}): Pos({p.posX}, {p.posY}, {p.posZ})");
            }
        }       
 
    }

    public static void S_BroadcastMoveHandler(PacketSession session, IPacket packet)
    {
        ServerSession s = session as ServerSession;
        S_BroadcastMove p = packet as S_BroadcastMove;  

        if(s.SessionId == p.playerId) //나인 경우만 이동 패킷 보여주기
        {
            Console.WriteLine($"Player({p.playerId}): Pos({p.posX}, {p.posY}, {p.posZ})");
        }   
    }
}
