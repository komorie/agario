using Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class NetworkManager : GOSingleton<NetworkManager> 
{
    private ServerSession session;
    private IPEndPoint endPoint;
    private Connecter connector = new Connecter();
    public void Send(ArraySegment<byte> sendBuff)
    {
        session.Send(sendBuff);
    }

    public void Connect()
    {
        connector.Connect(endPoint, () => {
            session = new ServerSession();
            return session;
        }, 1); //서버에 연결 요청, 성공 시 Session 생성
    }

    public void Disconnect()
    {
        session.Disconnect();
    }

    private void Awake()
    {
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress iPAddr = ipHost.AddressList[0];
        endPoint = new IPEndPoint(iPAddr, 777); //현재 호스트의 IP주소와 포트 번호 가져오기
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
