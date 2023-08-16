using Core;
using System.Net;

namespace Server
{
    internal class Server
    {
        static void Main(string[] args)
        {

            PacketManager.Instance.Register();

            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress iPAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(iPAddr, 777); //현재 호스트의 IP주소와 포트 번호 가져오기

            Listener listener = new Listener(); //연결 대기

            listener.Init(endPoint, () => { return new ClientSession(); }); //연결 시 GameSession 생성

            while (true)
            {

            }

        }
    }
}
