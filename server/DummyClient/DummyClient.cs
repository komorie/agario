using System.Net.Sockets;
using System.Net;
using System.Text;
using Core;
using DummyClient.Session;

namespace DummyClient
{ 
    internal class DummyClient
    {

        static void Main(string[] args)
        {

            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress iPAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(iPAddr, 777); //현재 호스트의 IP주소와 포트 번호 가져오기

            Connecter connector = new Connecter();

            connector.Connect(endPoint, () => { return SessionManager.Instance.Generate(); }, 30); //서버에 연결 요청, 성공 시 Session 생성, 10회 시도(연결 세션이 10개 생성)

            while (true)
            {
                try
                {
                    SessionManager.Instance.SendForEach(); //현재 서버와 연결된 세션이 10개 생성 -> 10번 패킷 보냄(10개의 클라와 연결됐을때의 서버 동작을 확인하기 위함)
                }
                catch (Exception)
                {
                    throw;
                }

                Thread.Sleep(1000); //한 플레이어마다 1초에 10번 전송
            }

        }
    }
}