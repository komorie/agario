using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.Session;
using Core;

namespace Server
{
    public class GameRoom : IJobQueue //게임 방
    {
        List<ClientSession> sessions = new List<ClientSession>();
        JobQueue jobQueue = new JobQueue();
        List<ArraySegment<byte>> pendingList = new List<ArraySegment<byte>>();
        Random rand = new Random();

        public void Push(Action job)
        {
            jobQueue.Push(job); 
        }

        public void Enter(ClientSession session) //클라 A가 게임방 입장
        {    
            //해당 클라 session의 시작 포지션을 -50 50 사이에서 랜덤 결정
            session.PosX = rand.Next(-50, 50);
            session.PosY = rand.Next(-50, 50);
            session.PosZ = 0;

            sessions.Add(session); //들어온 애 세션 리스트에 추가
            session.Room = this; //들어온 애의 방을 이 방으로 설정   

            //들어온 플레이어에게, (새로 들어온 플레이어 정보까지 갱신된) 모든 플레이어 리스트를 보내준다
            S_PlayerList playerList = new S_PlayerList();
            foreach (ClientSession s in sessions)
            {
                playerList.players.Add(new S_PlayerList.Player() //일단 새로 들어온 Player 넣어주고
                {
                    isSelf = (s == session), //(해당 클라 입장에서) 리스트에 있는 플레이어가 자기 자신인지 아닌지 여부
                    playerId = s.SessionId,
                    posX = s.PosX,
                    posY = s.PosY,
                    posZ = s.PosZ
                }); 
            }

            session.Send(playerList.Write()); //새로 들어온 애한테만 보내기

            S_BroadcastEnterGame enterGame = new S_BroadcastEnterGame
            {
                playerId = session.SessionId,
                posX = session.PosX,
                posY = session.PosY,
                posZ = 0
            }; //새로 들어온 애의 정보를 모든 플레이어에게 보내기  
            BroadCast(enterGame.Write()); //모든 클라에게 보내기

        } 

        public void Leave(ClientSession session) //얘 나간다
        {
            sessions.Remove(session);
            session.Room = null;

            //모든 클라에게 알리기
            S_BroadcastLeaveGame leaveGame = new S_BroadcastLeaveGame();
            leaveGame.playerId = session.SessionId;
            BroadCast(leaveGame.Write());
        }

        public void Move(ClientSession session, C_Move movePacket) //얘 움직인다
        {
            //좌표 변경
            session.PosX += movePacket.posX;
            session.PosY += movePacket.posY;
            session.PosZ += movePacket.posZ;


            //얘 여기로 이동했다고 알리기(나중에는 증가량으로 수정해야 할 듯)
            S_BroadcastMove move = new S_BroadcastMove();
            move.playerId = session.SessionId;
            move.posX = session.PosX;
            move.posY = session.PosY;
            move.posZ = session.PosZ;   
            BroadCast(move.Write());    
        }

        public void BroadCast(ArraySegment<byte> segment) 
        {
            pendingList.Add(segment);   //패킷 바이트 배열을 보내야 할 리스트에 추가 -> Flush()에서 연결된 모든 클라에게 보낸다
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
