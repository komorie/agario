using Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;

public class NetworkManager : GOSingleton<NetworkManager> 
{

    public static string connectingAddress = null;

    private ServerSession session;
    private IPEndPoint endPoint;
    private Connecter connector = new Connecter();
    public void Send(ArraySegment<byte> sendBuff)
    {
        session.Send(sendBuff);
    }

    public void Connect(string address = null)
    {
        if(address != null && address != "")
        {
            IPAddress ipAdd = IPAddress.Parse(address);
            endPoint = new IPEndPoint(ipAdd, 777); //주소를 입력하면 해당 주소로 접속
        }
        else
        {
            connectingAddress = endPoint.Address.ToString();
        }

        connector.Connect(endPoint, () => {
            session = new ServerSession();
            return session;
        }, 1); //서버에 연결 요청, 성공 시 Session 생성
    }

    public void Disconnect()
    {
        session?.Disconnect();
    }

    private void Awake()
    {
        DontDestroyOnLoad(this);

        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress iPAddr = ipHost.AddressList[1];
        endPoint = new IPEndPoint(iPAddr, 777); //현재 호스트의 IP주소와 포트 번호 가져오기(기본 설정)
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
        Disconnect();
    }

    // 일정 주기마다 지속적으로 호출

}
