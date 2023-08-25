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
                session.DirX = rand.Next(-1, 2);    
                session.DirY = rand.Next(-1, 2);    
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

                    movePacket.dirX = session.DirX; 
                    movePacket.dirY = session.DirY;

                    if(movePacket.dirX == 1 && movePacket.dirY == 1) //대각선 방향이므로 제곱근 씌워서 방향벡터의 크기를 구하기
                    {
                        movePacket.dirX = (float)Math.Sqrt(0.5);
                        movePacket.dirY = (float)Math.Sqrt(0.5);
                    }
                    else if(movePacket.dirX == -1 && movePacket.dirY == -1)
                    {
                        movePacket.dirX = -(float)Math.Sqrt(0.5);
                        movePacket.dirY = -(float)Math.Sqrt(0.5);
                    }
                    else if(movePacket.dirX == -1 && movePacket.dirY == 1)
                    {
                        movePacket.dirX = -(float)Math.Sqrt(0.5);
                        movePacket.dirY = (float)Math.Sqrt(0.5);
                    }
                    else if(movePacket.dirX == 1 && movePacket.dirY == -1)
                    {
                        movePacket.dirX = (float)Math.Sqrt(0.5);
                        movePacket.dirY = -(float)Math.Sqrt(0.5);
                    }

                    DateTime now = DateTime.UtcNow;
                    float currentSecond = now.Hour * 3600 + now.Minute * 60 + now.Second + now.Millisecond * 0.001f;
                    movePacket.posX = session.PosX;
                    movePacket.posY = session.PosY;
                    movePacket.posZ = 0;
                    movePacket.time = currentSecond;    
                    session.Send(movePacket.Write());

                    //이동 패킷 -> 1초 후 -> 정지 패킷 -> 1초 후 -> 이동 패킷
                    session.DirX = session.DirX == 0 ? rand.Next(-1, 2) : 0;    
                    session.DirY = session.DirX == 0 ? rand.Next(-1, 2) : 0;
                    session.PosX += movePacket.dirX * 20;
                    session.PosY += movePacket.dirY * 20;    
                }   
            }
        }
    }
}
