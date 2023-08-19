using Core;
using Server.Session;
using System.Net;

namespace Server
{
    internal class Server
    {
        private static Listener listener = new Listener(); //연결 대기
        public static GameRoom gameRoom = new GameRoom(); //채팅방 생성    

        static void Main(string[] args)
        {
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress iPAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(iPAddr, 777); //현재 호스트의 IP주소와 포트 번호 가져오기


            listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); }, 1000); //연결 시 GameSession 생성

            while (true)
            {
                gameRoom.Push(() => { gameRoom.Flush(); }); //0.25초마다 쌓여있는 패킷들 한번에 보내도록 지시
                Thread.Sleep(250);
            }

        }
    }
}
