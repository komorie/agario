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
                session.DirX = 0;
                session.DirY = 0; 
                return session;
            }
        }

        public void SendForEach() //현재 위치에서 랜덤한 방향으로 이동 패킷 만들어 보내기
        {
            lock (_lock) 
            {
                foreach (ServerSession session in sessions)
                {
                    if(session.SessionId == -1)
                    {
                        continue;
                    }   

                    //랜덤한 방향으로 이동하는 패킷 만들어 보내기(나 일로 갔다)
                    C_Move movePacket = new C_Move();

                    movePacket.dirX = 0; 
                    movePacket.dirY = 0;

                    DateTime now = DateTime.UtcNow;
                    float currentSecond = now.Hour * 3600 + now.Minute * 60 + now.Second + now.Millisecond * 0.001f;
                    movePacket.posX = session.PosX;
                    movePacket.posY = session.PosY;
                    movePacket.posZ = 0;
                    movePacket.time = currentSecond;    
                    session.Send(movePacket.Write());

                    session.DirX = 0;    
                    session.DirY = 0;
                    session.PosX = rand.Next(-45, 45);
                    session.PosY = rand.Next(-45, 45);
                }   
            }
        }
    }
}
