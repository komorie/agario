using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.Session;
using Core;

namespace Server
{
    public class GameRoom : IJobQueue //채팅방
    {
        List<ClientSession> sessions = new List<ClientSession>();
        JobQueue jobQueue = new JobQueue();
        List<ArraySegment<byte>> pendingList = new List<ArraySegment<byte>>();  

        public void Push(Action job)
        {
            jobQueue.Push(job); 
        }

        public void Enter(ClientSession session) //게임방 입장
        {    
            sessions.Add(session);
            session.Room = this;
        } 

        public void Leave(ClientSession session) //방 퇴장
        {
            sessions.Remove(session);
            session.Room = null;
        }

        public void BroadCast(ClientSession session, string chat) //어떠한 클라한테 채팅이 오면... 내 방에 있는 모든 클라에게 채팅 전달
        {
            S_Chat packet = new S_Chat();
            packet.playerId = session.SessionId;
            packet.chat = $"Hello Server, I am {packet.playerId}";
            ArraySegment<byte> segment = packet.Write();    //패킷 생성

            pendingList.Add(segment);   //패킷을 보내야 할 리스트에 추가 
        }
        public void Flush()
        {
            foreach (ClientSession s in sessions)
                s.Send(pendingList);    

            Console.WriteLine($"Flushed {pendingList.Count} items");    
            pendingList.Clear();
        }
    }
}
