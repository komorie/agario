﻿using Core;
using Server;
using Server.Session;
using System;
using System.Collections.Generic;


internal class PacketHandler //패킷의 생성 과정에 신경 쓸 필요 없이, 해당 패킷으로 할 동작만 여기서 구현해 주면 될 것.
{
    public static void C_LeaveGameHandler(PacketSession session, IPacket packet) //클라에게서 나간다 패킷이 왔을 때
    {
        ClientSession clientSession = session as ClientSession;
        if (clientSession.Room == null) return;

        GameRoom room = clientSession.Room;
        room.Push(() => { room.Leave(clientSession); });    


    }

    public static void C_MoveHandler(PacketSession session, IPacket packet) //클라에게서 이동 패킷이 왔을 때
    {
        C_Move movePacket = packet as C_Move;
        ClientSession clientSession = session as ClientSession;
        if (clientSession.Room == null) return;

        GameRoom room = clientSession.Room;
        room.Push(() => { room.Move(clientSession, movePacket); }); //클라가 보낸 패킷을 이용해 이동 처리  
        
    }
}
