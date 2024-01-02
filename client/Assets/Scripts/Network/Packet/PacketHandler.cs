using Core;
using System;
using UnityEngine;

public class PacketHandler //패킷의 생성 과정에 신경 쓸 필요 없이, 해당 패킷으로 할 동작만 여기서 구현해 주면 될 것.
                           //유니티에서는 여기 접근하는 스레드는 메인 스레드(Network Manager) 하나므로 쓰레드 안전 신경쓸 필요 없음.
                           //그래도 디버그로 중단점 만지다 보면 가끔 오류가 나는데 이유를 정확히 모르겠다.
{
    public static void S_BroadcastEnterGameHandler(PacketSession session, IPacket packet)
    {
        S_BroadcastEnterGame p = packet as S_BroadcastEnterGame;
        Room.Instance.RecvEnterGame(p);
    }

    public static void S_BroadcastLeaveGameHandler(PacketSession session, IPacket packet)
    {
        S_BroadcastLeaveGame p = packet as S_BroadcastLeaveGame;    
        Room.Instance.RecvLeaveGame(p);    
    }

    public static void S_RoomListHandler(PacketSession session, IPacket packet) //플레이어들 리스트 패킷 받으면...
    {
        S_RoomList p = packet as S_RoomList;
        Room.Instance.InitRoom(p);  
    }

    public static void S_BroadcastMoveHandler(PacketSession session, IPacket packet)
    {
        //패킷 값 출력
        S_BroadcastMove p = packet as S_BroadcastMove;
        Room.Instance.RecvMove(p);
    }

    public static void S_BroadcastEatFoodHandler(PacketSession session, IPacket packet)
    {
        S_BroadcastEatFood p = packet as S_BroadcastEatFood;
        Room.Instance.RecvEatFood(p);
    }

    internal static void S_BroadcastEatPlayerHandler(PacketSession session, IPacket packet)
    {
        S_BroadcastEatPlayer p = packet as S_BroadcastEatPlayer;    
        Room.Instance.RecvEatPlayer(p);  
    }
}
