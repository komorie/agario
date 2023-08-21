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
        IPEndPoint endPoint = new IPEndPoint(iPAddr, 777); //현재 호스트의 IP주소와 포트 번호 가져오기

        Connecter connector = new Connecter();

        connector.Connect(endPoint, () => { return session; }, 1); //서버에 연결 요청, 성공 시 Session 생성, 10회 시도(연결 세션이 10개 생성)
    }

    private void OnApplicationQuit()
    {
        session.Disconnect();
    }
}
