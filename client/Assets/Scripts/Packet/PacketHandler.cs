using Core;
using System;
using UnityEngine;

public class PacketHandler //패킷의 생성 과정에 신경 쓸 필요 없이, 해당 패킷으로 할 동작만 여기서 구현해 주면 될 것.
{
    public static void S_PlayerInfoResHandler(PacketSession session, IPacket packet)
    {
        S_PlayerInfoRes p = packet as S_PlayerInfoRes;

        Console.WriteLine($"PlayerInfoReq: {p.playerId}, {p.name}");

        foreach (S_PlayerInfoRes.Skill skill in p.skills)
        {
            Console.WriteLine($"Skill: {skill.id}, {skill.level}, {skill.duration}");
        }
    }

    public static void S_ChatHandler(PacketSession session, IPacket packet)
    {
        S_Chat p = packet as S_Chat;
        ServerSession serverSession = session as ServerSession;
        Debug.Log($"{p.playerId}: {p.chat}");

    }   

}
