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

        //플레이어 리스트 순환하며 값 출력
        foreach (S_PlayerList.Player p in playerList.players)
        {
            Console.WriteLine($"Player({p.playerId}): Pos({p.posX}, {p.posY}, {p.posZ})");
        }       
 
    }

    public static void S_BroadcastMoveHandler(PacketSession session, IPacket packet)
    {

    }
}
