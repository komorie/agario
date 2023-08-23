using Core;
using System;
using UnityEngine;

public class PacketHandler //패킷의 생성 과정에 신경 쓸 필요 없이, 해당 패킷으로 할 동작만 여기서 구현해 주면 될 것.
{
    public static void S_BroadcastEnterGameHandler(PacketSession session, IPacket packet)
    {
        S_BroadcastEnterGame p = packet as S_BroadcastEnterGame;
        PlayerManager.Instance.EnterGame(p);
    }

    public static void S_BroadcastLeaveGameHandler(PacketSession session, IPacket packet)
    {
        S_BroadcastLeaveGame p = packet as S_BroadcastLeaveGame;    
        PlayerManager.Instance.LeaveGame(p);    
    }

    public static void S_PlayerListHandler(PacketSession session, IPacket packet) //플레이어들 리스트 패킷 받으면...
    {
        S_PlayerList p = packet as S_PlayerList;
        PlayerManager.Instance.Add(p);  
    }

    public static void S_BroadcastMoveHandler(PacketSession session, IPacket packet)
    {
        //패킷 값 출력
        S_BroadcastMove p = packet as S_BroadcastMove;
        PlayerManager.Instance.Move(p);
    }
}
