using Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class NetworkManager : GOSingleton<NetworkManager> 
{
    private ServerSession session = new ServerSession();
    public void Send(ArraySegment<byte> sendBuff)
    {
        session.Send(sendBuff);
    }   

    private void Awake()
    {
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress iPAddr = IPAddress.Parse("fe80::e090:d9bc:4ae7:9a9d%16");
        IPEndPoint endPoint = new IPEndPoint(iPAddr, 777); //현재 호스트의 IP주소와 포트 번호 가져오기

        Connecter connector = new Connecter();

        connector.Connect(endPoint, () => { return session; }, 1); //서버에 연결 요청, 성공 시 Session 생성, 10회 시도(연결 세션이 10개 생성)

    }

    private void Update()
    {
        List<IPacket> packetList = PacketQueue.Instance.PopAll();
        if(packetList != null)
        {
            foreach (IPacket p in packetList)
            {
                PacketManager.Instance.HandlerPacket(session, p);
            }
        }   

    }

    private void OnApplicationQuit()
    {
        session.Disconnect();
    }

    // 일정 주기마다 지속적으로 호출

}
