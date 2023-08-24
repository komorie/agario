using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Session
{
    public class SessionManager //세션들 ID로 접근할 수 있게 관리하는 매니저
    {
        //싱글톤
        private static SessionManager sessionManager = new SessionManager();
        public static SessionManager Instance { get { return sessionManager; } }

        int sessionCount = 0;
        Dictionary<int, ClientSession> sessions = new Dictionary<int, ClientSession>();
        object _lock = new object();

        public ClientSession Generate() //세션 생성 후 딕셔너리에 추가
        {
            lock (_lock)
            {
                int sessionId = this.sessionCount++;
                ClientSession session = new ClientSession();
                session.SessionId = sessionId;
                sessions.Add(sessionId, session);

                Console.WriteLine($"Connected : {sessionId}");
                return session;
            }
        }

        public ClientSession Find(int id) //ID로 세션 찾기
        {
            lock (_lock)
            {
                ClientSession session = null;
                sessions.TryGetValue(id, out session);
                return session;
            }
        }

        public void Remove(ClientSession session) //세션 딕셔너리에서 삭제
        {
            lock (_lock)
            {
                sessions.Remove(session.SessionId);
            }
        }
    }
}
