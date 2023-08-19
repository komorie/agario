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

        public ServerSession Generate() //세션 생성 후 리스트에 추가
        {
            lock (_lock)
            {
                ServerSession session = new ServerSession();
                sessions.Add(session);
                session.SessionId = sessions.Count;
                return session;
            }
        }

        public void SendForEach() //서버로 챗 생성해서 보낸다
        {
            lock (_lock) 
            {
                foreach (ServerSession session in sessions)
                {
                    C_Chat p = new C_Chat();
                    p.chat = "Hello Server!";
                    ArraySegment<byte> segment = p.Write(); 

                    session.Send(segment);
                }
            }
        }
    }
}
