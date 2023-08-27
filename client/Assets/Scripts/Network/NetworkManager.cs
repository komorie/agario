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
        }, 1); //������ ���� ��û, ���� �� Session ����
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
        endPoint = new IPEndPoint(iPAddr, 777); //���� ȣ��Ʈ�� IP�ּҿ� ��Ʈ ��ȣ ��������
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

    // ���� �ֱ⸶�� ���������� ȣ��

}
