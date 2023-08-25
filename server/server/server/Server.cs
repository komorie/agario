using Core;
using Server.Game;
using Server.Session;
using System.Net;

namespace Server
{
    internal class Server
    {
        private static Listener listener = new Listener(); //연결 대기
        public static GameRoom gameRoom = new GameRoom(); //게임방 생성

        static void Main(string[] args)
        {
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress iPAddr = IPAddress.Parse("fe80::e090:d9bc:4ae7:9a9d%16");
            IPEndPoint endPoint = new IPEndPoint(iPAddr, 777); //현재 호스트의 IP주소와 포트 번호 가져오기
            JobTimer jobTimer = new JobTimer(); //타이머 생성

            listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); }, 100); //연결 시 Session 생성


            while (true)
            {
            }

        }
    }
}
