﻿using Server.Session;
using Core;
using static S_RoomList;

namespace Server.Game
{
    public class GameRoom : IJobQueue //게임 방
    {
        List<ClientSession> Sessions { get; set; } = new List<ClientSession>();

        List<Food> foodList = new List<Food>(); //방의 푸드 리스트
        
        JobQueue jobQueue = new JobQueue();
        PosSimulatior PosSim = new PosSimulatior();
/*        List<ArraySegment<byte>> pendingList = new List<ArraySegment<byte>>();*/
        Random rand = new Random();
        private object _lock = new object();

        public GameRoom()
        {
            //foodlist에 랜덤 -45, 45 float 좌표를 가진 food 6개 추가
            for (int i = 0; i < 6; i++)
            {
                foodList.Add(new Food()
                {
                    posX = (float)(rand.NextDouble() * 90 - 45),
                    posY = (float)(rand.NextDouble() * 90 - 45)
                });
            }
        }

        public void Push(Action job)
        {
            jobQueue.Push(job);
        }

        public void Enter(ClientSession session) //클라 A가 게임방 입장
        {
            Sessions.Add(session); //들어온 애 세션 리스트에 추가
            session.Room = this; //들어온 애의 방을 이 방으로 설정   

            Player sp = new Player() //해당 세션의 플레이어 생성
            {
                PlayerId = session.SessionId,
                PosX = rand.Next(-40, 40),
                PosY = rand.Next(-40, 40),
                PosZ = 0
            };

            session.MyPlayer = sp; //해당 세션의 플레이어를 방금 만든 플레이어로 설정

            //들어온 플레이어에게, (새로 들어온 플레이어 정보까지 갱신된) 모든 플레이어 리스트를 보내준다
            S_RoomList roomList = new S_RoomList();

            foreach (ClientSession s in Sessions)
            {
                roomList.players.Add(new S_RoomList.Player()
                {
                    isSelf = s.SessionId == session.MyPlayer.PlayerId, //자기 자신인지 확인   
                    playerId = s.MyPlayer.PlayerId,
                    posX = s.MyPlayer.PosX,
                    posY = s.MyPlayer.PosY,
                    posZ = 0
                }); //리스트 만들기
            }

            //Food 추가
            foreach (Food f in foodList)
            {
                roomList.foods.Add(new S_RoomList.Food()
                {
                    posX = f.posX,
                    posY = f.posY
                });
            }   

            session.Send(roomList.Write()); //새로 들어온 애한테만 리스트 패킷 보내기

            S_BroadcastEnterGame enterGame = new S_BroadcastEnterGame
            {
                playerId = sp.PlayerId,
                posX = sp.PosX,
                posY = sp.PosY,
                posZ = 0
            }; //새로 들어온 애의 정보를 모든 플레이어에게 보내기  
            BroadCast(enterGame); //모든 클라에게 보내기
        }

        public void Leave(ClientSession session) //얘 나간다
        {
            //모든 클라에게 알리기
            S_BroadcastLeaveGame leaveGame = new S_BroadcastLeaveGame();
            leaveGame.playerId = session.SessionId;

            Sessions.Remove(session);
            session.Room = null;
            session.MyPlayer = null;

            BroadCast(leaveGame);
        }

        public void Move(ClientSession session, C_Move movePacket) //얘 움직인다
        {
            Player sp = session.MyPlayer;

            //해당하는 플레이어 좌표 변경
            sp.DirX = movePacket.dirX;
            sp.DirY = movePacket.dirY;
            sp.PosX = movePacket.posX;
            sp.PosY = movePacket.posY;
            sp.PosZ = movePacket.posZ;


            //다른 클라들한테 얘 여기에서 어디로 이동한다고 알리기
            S_BroadcastMove move = new S_BroadcastMove();
            move.playerId = session.SessionId;
            move.dirX = sp.DirX;
            move.dirY = sp.DirY;
            move.posX = sp.PosX;
            move.posY = sp.PosY;
            move.posZ = sp.PosZ;
            move.time = movePacket.time;
            BroadCast(move);
        }

        public void EatFood(ClientSession session, C_EatFood eatPacket)
        {
            Food f = foodList[eatPacket.foodId];

            float newFoodX, newFoodY;

            //sessions를 돌면서, 플레이어와 겹치는지 계산해서 안겹치는 위치로 새로 지정
            do
            {
                newFoodX = (float)(rand.NextDouble() * 90 - 45);
                newFoodY = (float)(rand.NextDouble() * 90 - 45);
            }
            while (OverlapWithPlayer(newFoodX, newFoodY));


            f.posX = newFoodX;
            f.posY = newFoodY;  

            S_BroadcastEatFood eat = new S_BroadcastEatFood();  
            eat.playerId = session.SessionId;
            eat.foodId = eatPacket.foodId; //얘가 이 음식 먹었다
            eat.posX = f.posX;
            eat.posY = f.posY; //음식 위치 이동

            BroadCast(eat); //모든 클라에게 보내기   
        }

        //sessions를 돌면서, 플레이어의 좌표 posx, posy와 겹치는 위치인지 확인
        private bool OverlapWithPlayer(float x, float y)
        {
            foreach (var s in Sessions)
            {
                float distance = (float)Math.Sqrt(Math.Pow(s.MyPlayer.PosX - x, 2) + Math.Pow(s.MyPlayer.PosY - y, 2));
                if (distance < s.MyPlayer.Size)
                {
                    return true;  // 겹치는 플레이어 발견
                }
            }
            return false;  // 겹치는 플레이어 없음
        }

        //전부에게 즉시 패킷 보내기
        public void BroadCast(IPacket packet)
        {
            lock (_lock)
            {
                foreach (ClientSession s in Sessions)
                    s.Send(packet.Write());
            }
        }


        /*
                public void Flush()
                {
                    foreach (ClientSession s in Sessions)
                        s.Send(pendingList);    

                    Console.WriteLine($"Flushed {pendingList.Count} items");    
                    pendingList.Clear();
                }*/
    }
}