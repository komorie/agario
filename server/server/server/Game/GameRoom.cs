using Server.Session;
using Core;
using static S_RoomList;
using System.Diagnostics;

namespace Server.Game
{
    public class GameRoom : IJobQueue //게임 방
    {

        private int total = 0;
        private int playerSpeed = 20;
        private int foodCount = 20;
        private float roomSizeX = 100;
        private float roomSizeY = 100;
        private float currentSecond;

        private Stopwatch stopwatch = new Stopwatch();

        List<ClientSession> Sessions { get; set; } = new List<ClientSession>();

        List<Food> foodList = new List<Food>(); //방의 푸드 리스트
        
        JobQueue jobQueue = new JobQueue();
/*        List<ArraySegment<byte>> pendingList = new List<ArraySegment<byte>>();*/
        Random rand = new Random();
        private object _lock = new object();

        public GameRoom()
        {
            //foodlist에 랜덤 float 좌표를 가진 food 20개 추가
            for (int i = 0; i < foodCount; i++)
            {
                foodList.Add(new Food()
                {
                    posX = (float)(rand.NextDouble() * (roomSizeX - 5) * 2 - (roomSizeX - 5)),
                    posY = (float)(rand.NextDouble() * (roomSizeY - 5) * 2 - (roomSizeY - 5))
                });
            }

            // Stopwatch 시작
            stopwatch.Start();
            currentSecond = 0;

            Simulate();
/*            SyncTime();*/
        }

/*        public void SyncTime()
        {
            Push(BroadCastTime);
            JobTimer.Instance.Push(SyncTime, 30000); //30초 뒤에 Simulate 또 호출 -> 클라이언트에게 시간 전송 -> 30초마다 시계 동기화
        }*/

/*        public void BroadCastTime()
        {
            Console.WriteLine($"Elapsed Second: {stopwatch.Elapsed.TotalSeconds}");
            S_BroadcastServerTime st = new S_BroadcastServerTime();
            st.serverTime = (float)stopwatch.Elapsed.TotalSeconds;
            BroadCast(st); //서버의 시간 전송
        }*/

        public void Simulate()
        {
            Push(SimulatePlayersPosition); //SimulatePlayersPosition 함수를 호출하도록 jobQueue에 넣음
            JobTimer.Instance.Push(Simulate, 100); //100ms 뒤에 Simulate 또 호출 -> 이렇게 하면 1초에 10번 플레이어 위치 계산
        }

        public void SimulatePlayersPosition() //함수가 호출되는 시점에서, 현재 방에 있는 플레이어들의 위치를 흐른 시간 time에 비례해, dir 방향으로 pos를 업데이트해야 함. 1초당 이동 속도는 Speed
        {
            float currentSecond = (float)stopwatch.Elapsed.TotalSeconds;
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
            S_BroadcastServerTime st = new S_BroadcastServerTime();
            st.serverTime = (float)stopwatch.Elapsed.TotalSeconds;
            session.Send(st.Write()); //서버의 시간 전송

            //sessions를 돌면서, 플레이어와 겹치는지 계산해서 안겹치는 위치로 새로 지정
            int newPlayerX, newPlayerY;
            do
            {
                newPlayerX = rand.Next((int)-roomSizeX + 10, (int)roomSizeX - 10);
                newPlayerY = rand.Next((int)-roomSizeY + 10, (int)roomSizeY - 10);
            }
            while (OverlapWithPlayer(newPlayerX, newPlayerY));

            session.SessionId = ++total;
            Console.WriteLine($"Player {session.SessionId} Entered"); //로그 출력
            Sessions.Add(session); //들어온 애 세션 리스트에 추가
            session.Room = this; //들어온 애의 방을 이 방으로 설정 

            Player sp = new Player() //해당 세션의 플레이어 생성
            {
                PlayerId = session.SessionId,
                PosX = newPlayerX,
                PosY = newPlayerY,
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

            foreach (ClientSession s in Sessions) //움직이고 있다는 패킷 보내기
            {
                Player p = s.MyPlayer;
                S_BroadcastMove move = new S_BroadcastMove();
                move.playerId = session.SessionId;
                move.dirX = p.DirX;
                move.dirY = p.DirY;
                move.posX = p.PosX;
                move.posY = p.PosY;
                move.posZ = p.PosZ;
                move.time = p.moveTime;
                session.Send(move.Write());
            }

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

            //해당하는 플레이어 좌표 변경
            sp.DirX = movePacket.dirX;
            sp.DirY = movePacket.dirY;
            sp.PosX = movePacket.posX;
            sp.PosY = movePacket.posY;
            sp.PosZ = movePacket.posZ;
            sp.moveTime = movePacket.time;


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
            sp.Radius += 0.1f; //반지름 증가

            Food f = foodList[eatPacket.foodId];

            float newFoodX, newFoodY;

            //sessions를 돌면서, 플레이어와 겹치는지 계산해서 안겹치는 위치로 새로 지정
            do
            {
                newFoodX = (float)rand.NextDouble() * (roomSizeX - 5) * 2 - (roomSizeX - 5);
                newFoodY = (float)rand.NextDouble() * (roomSizeY - 5) * 2 - (roomSizeY - 5);
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

        public void EatPlayer(ClientSession session, C_EatPlayer eatPlayerPacket) //얘가 누구 먹었대
        {
            S_BroadcastEatPlayer eatPlayer = new S_BroadcastEatPlayer();    
            eatPlayer.predatorId = session.MyPlayer.PlayerId; //먹은 애

            for (int i = 0; i < Sessions.Count; i++)
            {
                if (Sessions[i].SessionId == eatPlayerPacket.preyId) //먹힌 애 찾아서
                {
                    eatPlayer.preyId = Sessions[i].SessionId; //먹힌 애
                    session.MyPlayer.Radius += (Sessions[i].MyPlayer.Radius / 2); //포식자 크기 증가
                    Console.WriteLine($"Player {eatPlayer.predatorId} killed Player {eatPlayer.preyId}");
                    break;
                }
            }

            BroadCast(eatPlayer); //모든 클라에게 알리기
        }

        public void BeamStart(ClientSession session, C_BeamStart beamStartPacket) //얘가 빔 쏜다 
        {
            S_BroadcastBeamStart beamStart = new S_BroadcastBeamStart();
            beamStart.userId = session.MyPlayer.PlayerId;   
            beamStart.dirX = beamStartPacket.dirX;
            beamStart.dirY = beamStartPacket.dirY;

            Console.WriteLine($"Player {session.MyPlayer.PlayerId} Charged Beam"); //로그 출력

            BroadCast(beamStart);
        }

        public void BeamHit(ClientSession session, C_BeamHit beamHitPacket) //우선 임시적으로 클라에서 맞았는지 처리하고, 시간 나면 서버에서 한번 검증하도록 개선
        {
            S_BroadcastBeamHit beamHit = new S_BroadcastBeamHit();  
            beamHit.userId = session.MyPlayer.PlayerId; 
            foreach (C_BeamHit.HitPlayer hitPlayer in beamHitPacket.hitPlayers)
            {
                S_BroadcastBeamHit.HitPlayer p = new S_BroadcastBeamHit.HitPlayer();
                p.playerId = hitPlayer.playerId;
                beamHit.hitPlayers.Add(p);
            }
            Console.WriteLine($"Player {session.MyPlayer.PlayerId} Fired Beam"); //로그 출력
            BroadCast(beamHit);
        }

        public void Stealth(ClientSession session, C_Stealth stealthPacket)
        {
            S_BroadcastStealth stealth = new S_BroadcastStealth();    
            stealth.userId = stealthPacket.userId;
            Console.WriteLine($"Player {session.MyPlayer.PlayerId} Hided"); //로그 출력
            BroadCast(stealth);
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
