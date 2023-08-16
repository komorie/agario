using Core;
using System;
using System.Collections.Generic;


internal class PacketHandler //패킷의 생성 과정에 신경 쓸 필요 없이, 해당 패킷으로 할 동작만 여기서 구현해 주면 될 것.
{
    public static void C_PlayerInfoReqHandler(PacketSession session, IPacket packet)
    {
        C_PlayerInfoReq p = packet as C_PlayerInfoReq;

        Console.WriteLine($"PlayerInfoReq: {p.playerId}, {p.name}");

        foreach (C_PlayerInfoReq.Skill skill in p.skills)
        {
            Console.WriteLine($"Skill: {skill.id}, {skill.level}, {skill.duration}");
        }

    }
}
