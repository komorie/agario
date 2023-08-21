using Core;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    ServerSession session = new ServerSession();

    private void Awake()
    {
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress iPAddr = ipHost.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(iPAddr, 777); //���� ȣ��Ʈ�� IP�ּҿ� ��Ʈ ��ȣ ��������

        Connecter connector = new Connecter();

        connector.Connect(endPoint, () => { return session; }, 1); //������ ���� ��û, ���� �� Session ����, 10ȸ �õ�(���� ������ 10�� ����)
    }

    private void OnApplicationQuit()
    {
        session.Disconnect();
    }
}
