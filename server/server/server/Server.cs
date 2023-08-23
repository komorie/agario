using Core;
using Server.Session;
using System.Net;

namespace Server
{
    internal class Server
    {
        private static Listener listener = new Listener(); //연결 대기
        public static GameRoom gameRoom = new GameRoom(); //게임방 생성
                                 
        private static void FlushRoom()
        {
            gameRoom.Push(gameRoom.Flush); //게임방에 있는 클라들에게 패킷 전달(일단 1번 실행)
            JobTimer.Instance.Push(FlushRoom, 250); //250ms마다 패킷 전달하도록 잡타이머에 넣기
        }   

        static void Main(string[] args)
        {
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress iPAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(iPAddr, 777); //현재 호스트의 IP주소와 포트 번호 가져오기


            listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); }, 1000); //연결 시 GameSession 생성

            JobTimer.Instance.Push(FlushRoom);

            while (true)
            {
                JobTimer.Instance.Flush(); //잡타이머에 넣은 작업들 실행    
            }

        }
    }
}
