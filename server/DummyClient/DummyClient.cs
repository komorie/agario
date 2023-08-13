using System.Net.Sockets;
using System.Net;
using System.Text;
using Core;

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

            connector.Connect(endPoint, () => { return new ServerSession(); }); //서버에 연결 요청, 성공 시 GameSession 생성

            while (true)
            {

            }
       
        }
    }
}