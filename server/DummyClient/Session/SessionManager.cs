using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyClient.Session
{
    public class SessionManager
    {
        //싱글톤
        private static SessionManager sessionManager = new SessionManager();
        public static SessionManager Instance { get { return sessionManager; } }

        List<ServerSession> sessions = new List<ServerSession>();

        object _lock = new object();

        Random rand = new Random(); 

        public ServerSession Generate() //세션 생성 후 리스트에 추가
        {
            lock (_lock)
            {
                ServerSession session = new ServerSession();
                sessions.Add(session);  
                return session;
            }
        }

        public void SendForEach() //현재 위치에서 랜덤한 방향으로 이동 패킷 만들어 보내기
        {
            lock (_lock) 
            {
                foreach (ServerSession session in sessions)
                {
                    //랜덤한 방향으로 이동 패킷 만들어 보내기
                    C_Move movePacket = new C_Move();
                    movePacket.posX = rand.Next(-50, 50);
                    movePacket.posY = rand.Next(-50, 50);
                    movePacket.posZ = rand.Next(-50, 50);
                    session.Send(movePacket.Write());
                }   
            }
        }
    }
}
