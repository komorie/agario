using Server.Session;
using Core;
using static S_RoomList;

namespace Server.Game
{
    public class GameRoom : IJobQueue //게임 방
    {

        private int playerSpeed = 20;
        private float roomSizeX = 50;
        private float roomSizeY = 50;
        private float currentSecond;

        List<ClientSession> Sessions { get; set; } = new List<ClientSession>();

        List<Food> foodList = new List<Food>(); //방의 푸드 리스트
        
        JobQueue jobQueue = new JobQueue();
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

            DateTime now = DateTime.UtcNow;
            currentSecond = now.Hour * 3600 + now.Minute * 60 + now.Second + now.Millisecond * 0.001f;

            Simulate();
        }

        public void Simulate()
        {
            Push(SimulatePlayersPosition); //SimulatePlayersPosition 함수를 호출하도록 jobQueue에 넣음
            JobTimer.Instance.Push(Simulate, 100); //100ms 뒤에 Simulate 또 호출 -> 이렇게 하면 1초에 10번 플레이어 위치 계산
        }

        public void SimulatePlayersPosition() //함수가 호출되는 시점에서, 현재 방에 있는 플레이어들의 위치를 흐른 시간 time에 비례해, dir 방향으로 pos를 업데이트해야 함. 1초당 이동 속도는 Speed
        {
            DateTime now = DateTime.UtcNow;
            float currentSecond = now.Hour * 3600 + now.Minute * 60 + now.Second + now.Millisecond * 0.001f;
            float deltaTime = currentSecond - this.currentSecond;   

            foreach (ClientSession s in Sessions)
            {
                Player p = s.MyPlayer;
                p.PosX += p.DirX * playerSpeed * deltaTime; //위치를 방향 * 속력 * 흐른 시간만큼 이동 
                p.PosY += p.DirY * playerSpeed * deltaTime;

                //방의 범위를 넘어가려고 하면, 다시 방 안으로 넣어준다
                if (p.PosX < -roomSizeX + p.Radius)
                    p.PosX = -roomSizeX + p.Radius;
                else if (p.PosX > roomSizeX - p.Radius)
                    p.PosX = roomSizeX - p.Radius;

                if (p.PosY < -roomSizeY + p.Radius)
                    p.PosY = -roomSizeY + p.Radius;
                else if (p.PosY > roomSizeY - p.Radius)
                    p.PosY = roomSizeY - p.Radius;
            }

            this.currentSecond = currentSecond;
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
                PosZ = 0,
                Radius = 1.5f //초기 반지름
            };

            session.MyPlayer = sp; //해당 세션의 플레이어를 방금 만든 플레이어로 설정
            Console.WriteLine($"Player {sp.PlayerId}: X: {sp.PosX}, Y:{sp.PosY}, Z:{sp.PosZ}"); //로그 출력


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
                    posZ = 0,
                    radius = s.MyPlayer.Radius  
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
                posZ = 0,
            }; //새로 들어온 애의 정보를 모든 플레이어에게 보내기  
            BroadCast(enterGame); //모든 클라에게 보내기
        }

        public void Leave(ClientSession session) //얘 나간다
        {
            //모든 클라에게 알리기
            S_BroadcastLeaveGame leaveGame = new S_BroadcastLeaveGame();
            leaveGame.playerId = session.SessionId;
            Console.WriteLine($"Player {session.SessionId} Leaved"); //로그 출력

            Sessions.Remove(session);
            session.Room = null;
            session.MyPlayer = null;


            BroadCast(leaveGame);
        }

        public void Move(ClientSession session, C_Move movePacket) //얘 움직인다
        {
            Player sp = session.MyPlayer;


            //기본적으로 플레이어가 보낸 위치로 서버에서 플레이어 좌표 변경하되, 서버에 저장된 좌표와 너무 차이가 크면 무시한다 -> 해당 플레이어를 서버에 저장된 좌표로 강제 이동

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

            Console.WriteLine($"Player {sp.PlayerId} Move to DirX: {sp.DirX}, DirY: {sp.DirY}"); //로그 출력

            BroadCast(move);
        }

        public void EatFood(ClientSession session, C_EatFood eatPacket)
        {
            Player sp = session.MyPlayer;
            sp.Radius += 0.05f; //반지름 증가

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

            Console.WriteLine($"Player {sp.PlayerId}: Eat Food"); //로그 출력

            BroadCast(eat); //모든 클라에게 보내기   
        }

        public void EatPlayer(ClientSession clientSession, C_EatPlayer eatPlayerPacket) //얘가 누구 먹었대
        {
            S_BroadcastEatPlayer eatPlayer = new S_BroadcastEatPlayer();    
            eatPlayer.predatorId = clientSession.MyPlayer.PlayerId; //먹은 애

            for (int i = 0; i < Sessions.Count; i++)
            {
                if (Sessions[i].SessionId == eatPlayerPacket.preyId) //먹힌 애 찾아서
                {
                    eatPlayer.preyId = Sessions[i].SessionId; //먹힌 애
                    clientSession.MyPlayer.Radius += (Sessions[i].MyPlayer.Radius / 2); //포식자 크기 증가
                    Console.WriteLine($"Player {eatPlayer.predatorId} killed Player {eatPlayer.preyId}");
                    break;
                }
            }

            BroadCast(eatPlayer); //모든 클라에게 알리기
        }

        //sessions를 돌면서, 플레이어의 좌표 posx, posy와 겹치는 위치인지 확인
        private bool OverlapWithPlayer(float x, float y)
        {
            foreach (var s in Sessions)
            {
                float distance = (float)Math.Sqrt(Math.Pow(s.MyPlayer.PosX - x, 2) + Math.Pow(s.MyPlayer.PosY - y, 2));
                if (distance < s.MyPlayer.Radius)
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
