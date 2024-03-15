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
            endPoint = new IPEndPoint(ipAdd, 777); //�ּҸ� �Է��ϸ� �ش� �ּҷ� ����
        }
        else
        {
            connectingAddress = endPoint.Address.ToString();
        }

        connector.Connect(endPoint, () => {
            session = new ServerSession();
            return session;
        }, 1); //������ ���� ��û, ���� �� Session ����
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
        endPoint = new IPEndPoint(iPAddr, 777); //���� ȣ��Ʈ�� IP�ּҿ� ��Ʈ ��ȣ ��������(�⺻ ����)
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

    // ���� �ֱ⸶�� ���������� ȣ��

}
