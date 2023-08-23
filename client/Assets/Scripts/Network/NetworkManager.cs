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

    protected override void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress iPAddr = ipHost.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(iPAddr, 777); //���� ȣ��Ʈ�� IP�ּҿ� ��Ʈ ��ȣ ��������

        Connecter connector = new Connecter();

        connector.Connect(endPoint, () => { return session; }, 1); //������ ���� ��û, ���� �� Session ����, 10ȸ �õ�(���� ������ 10�� ����)

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
}