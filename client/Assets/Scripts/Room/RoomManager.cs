using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomManager : GOSingleton<RoomManager>   
{
    Myplayer myPlayer;
    NetworkManager network;
    public Dictionary<int, Player> Players { get; set; } = new Dictionary<int, Player>();
    public Dictionary<int, Food> Foods { get; set; } = new Dictionary<int, Food>(); 

    GameObject playerPrefab;
    GameObject myPlayerPrefab;
    GameObject foodPrefab;
    GameObject ConfirmUI;

    private void Awake()
    {
        playerPrefab = Resources.Load<GameObject>("Prefabs/Player");    
        myPlayerPrefab = Resources.Load<GameObject>("Prefabs/MyPlayer");
        foodPrefab = Resources.Load<GameObject>("Prefabs/Food");
        ConfirmUI = Resources.Load<GameObject>("Prefabs/ConfirmUI");
        network = NetworkManager.Instance;
    }

    public void InitRoom(S_RoomList packet) //처음에 접속했을 때, 이미 접속해 있던 플레이어들 목록 받아서 관리 딕셔너리에 추가함.
    {

        foreach (S_RoomList.Player p in packet.players)
        {
            if(p.isSelf) //내 플레이어인 경우 플레이어 아이디와 초기 위치 받아오고, 나머지는 그냥 초기화
            {
                GameObject go = Instantiate(myPlayerPrefab);
                Myplayer mPlayer = go.AddComponent<Myplayer>();    

                mPlayer.transform.position = new Vector3(p.posX, p.posY, p.posZ);
                myPlayer = mPlayer;
                myPlayer.PlayerId = p.playerId; 
                myPlayer.Radius = myPlayer.transform.localScale.x * 0.5f;
            }
            else //이미 접속해있던 남인 경우
            {
                GameObject go = Instantiate(playerPrefab);
                Player player = go.AddComponent<Player>();
 
                player.transform.position = new Vector3(p.posX, p.posY, p.posZ);
                player.PlayerId = p.playerId;
                player.Radius = p.radius;
                player.transform.localScale = new Vector3(p.radius * 2, p.radius * 2, p.radius * 2); //반지름 받아와서 사이즈 결정

                Players.Add(p.playerId, player); //딕셔너리에 추가
            }
        }

        for (int f = 0; f < packet.foods.Count; f++) //맵에 있는 음식들 관리 딕셔너리에 추가
        {
            GameObject go = Instantiate(foodPrefab);
            Food food = go.GetComponent<Food>();

            food.FoodId = f;   
            food.transform.position = new Vector3(packet.foods[f].posX, packet.foods[f].posY, 0);
            Foods.Add(f, food);
        }

        if (packet.players.Count < 2) //내가 첫번째로 접속한 경우면 실행 불가
        {
            //confirmUI를 띄워서 매칭 중이라고 표시하고 다른 플레이어 대기
            GameObject go = Instantiate(Resources.Load<GameObject>("Prefabs/ConfirmUI"));
            go.name = "MatchUI";
            ConfirmUI confirmUI = go.GetComponent<ConfirmUI>();
            confirmUI.Init(
                "매칭 중입니다.",
                "취소하기",
                () => {
                    Destroy(go);
                    myPlayer = null;
                    Players.Clear();
                    Foods.Clear();
                    network.Disconnect();
                    SceneManager.LoadScene("TitleScene"); //취소하면 연결 끊고 전부 초기화, 타이틀로 돌아가기
                }
            );
            myPlayer.moveAction.Disable(); //매칭 중에는 움직이지 못하게
        }
    }

    public void EnterGame(S_BroadcastEnterGame p) //서버에게서 새로운 유저가 들어왔다! 라는 패킷을 받는 경우 -> 일단 MyPlayer일리는 없음(고려 안함)
    {
        if(p.playerId != myPlayer.PlayerId) //내가 들어왔다는 알림이 아닌 경우
        {
            GameObject go = Instantiate(playerPrefab);

            Player player = go.AddComponent<Player>(); //새 플레이어 추가
            player.PlayerId = p.playerId;
            player.Radius = 1.5f; //초기값 
            player.transform.position = new Vector3(p.posX, p.posY, p.posZ);
            Players.Add(p.playerId, player);

            if(Players.Count == 1) //매칭 대기중인 상황에서 누가 들어옴
            {
                GameObject matchUI = GameObject.Find("MatchUI");
                if (matchUI != null)
                {
                    Destroy(matchUI); //대기 UI 삭제
                    myPlayer.moveAction.Enable(); //움직일 수 있게    
                }
            }
        }   
    }

    public void Move(S_BroadcastMove p)
    {

        if (myPlayer == null) return;


        //다른 플레이어인 경우 온 패킷대로 위치 조정, 나인 경우는 무시
        if (myPlayer.PlayerId != p.playerId)
        {
            Player player;
            DateTime now = DateTime.UtcNow;
            float currentSecond = now.Hour * 3600 + now.Minute * 60 + now.Second + now.Millisecond * 0.001f;
            float RTT = currentSecond - p.time;


            //벡터3 좌표 오차 구하기
            
/*            Debug.Log($"RTT: {RTT}"); //발송 시간과 도착 시간의 초 차이*/
            if (Players.TryGetValue(p.playerId, out player))
            {
                player.MoveVector = new Vector2(p.dirX, p.dirY); //다른 플레이어의 이동 방향
                player.TargetPosition = new Vector3(p.posX + p.dirX * player.Speed * RTT, p.posY + p.dirY * player.Speed * RTT, p.posZ);
                //다른 플레이어의 실제 위치 예측
                //Myplayer는 프레임당 dir * Speed * Time.deltaTime 만큼 더한 위치로 이동하며 Time.deltaTime은 프레임당 흐른 시간초이다. 즉 dir * Speed는 1초당 이동한 속도가 된다. 즉 1초당 이동거리가 20
                //그럼 dir * Speed * RTT는 RTT초 만큼 이동한 위치이다. 서버에서 온 위치 + dir * Speed * RTT 가 상대의 현재 위치라고 가정하고, 내 클라에서 돌린 위치랑 보간을 실시
                float currentDistance = Vector3.Distance(player.transform.position, player.TargetPosition); //상대 클라가 서버로 보낸 위치와 내 클라가 추측해서 이동시킨 위치의 거리 비교
/*                Debug.Log($"{RTT}초동안 이동한 거리: {p.dirY * player.Speed * RTT}");  */ 
                player.IsLerping = true; //이제 보간 시작
            }
        }
    }

    public void LeaveGame(S_BroadcastLeaveGame p) //누군가 게임 종료(Disconnect)했다는 패킷이 왔을 때
    {
        if(myPlayer == null) return;

        if(myPlayer.PlayerId != p.playerId) //다른 사람이 종료한 경우
        {
            Player player;
            if (Players.TryGetValue(p.playerId, out player)) //종료한 플레이어 오브젝트 있나 확인하고, 있으면 삭제
            {
                Destroy(player.gameObject);
                Players.Remove(p.playerId);
            }
        }
    }

    public void EatFood(S_BroadcastEatFood p) //누가 음식 먹었대
    {
        if(p.playerId != myPlayer.PlayerId) //먹은 플레이어 크기 올리기
        {
            Player player;
            if(Players.TryGetValue(p.playerId, out player))
            {
                player.transform.localScale += 0.1f * Vector3.one;
                player.Radius = player.transform.localScale.x * 0.5f;
            }
        }   

        Food food = Foods[p.foodId];
        food.transform.position = new Vector3(p.posX, p.posY, 0); //먹은 음식 새로운 위치로 스폰

        if(p.playerId == myPlayer.PlayerId)
        {
            food.gameObject.SetActive(true); //내가 먹었던 음식이면 비활성화되어있을 것이므로 재활성화
        }
    }

    public void EatPlayer(S_BroadcastEatPlayer p)
    {
        Player prey;
        Player predator;

        if (p.preyId == myPlayer.PlayerId) //내가 먹힌거면? 나 죽음
        {
            //방 오브젝트 전부 파괴
            Destroy(myPlayer.gameObject);
            myPlayer = null;

            foreach (var player in Players.Values)
            {
                Destroy(player.gameObject);
            }
            Players.Clear();

            foreach (var food in Foods.Values)
            {
                Destroy(food.gameObject);
            }
            Foods.Clear();

            network.Disconnect(); //접속 끊기
            ConfirmUI gameOverUI = Instantiate(ConfirmUI).GetComponent<ConfirmUI>(); //게임오버 UI
            gameOverUI.Init
                (
                    "게임 오버", 
                    "타이틀로",
                    () => { 
                        SceneManager.LoadScene("TitleScene"); 
                    }
                );
            return;
        }

        if (p.predatorId != myPlayer.PlayerId) //내가 먹은 거 아닌경우
        {
            if (Players.TryGetValue(p.predatorId, out predator) && Players.TryGetValue(p.preyId, out prey))
            {
                predator.transform.localScale += (prey.transform.localScale / 2); //먹힌 플레이어 크기 반만큼 먹은 플레이어 크기 증가
                predator.Radius = predator.transform.localScale.x * 0.5f;   //반지름 키우고
                Players.Remove(p.preyId); //먹힌 플레이어 딕셔너리에서 삭제
                Destroy(prey.gameObject); //먹힌 플레이어 오브젝트 삭제
            }
        }

        if(Players.Count == 0) //나말고 다 죽었다
        {
            //방 오브젝트 전부 파괴
            Destroy(myPlayer.gameObject);
            myPlayer = null;

            foreach (var player in Players.Values)
            {
                Destroy(player.gameObject);
            }
            Players.Clear();

            foreach (var food in Foods.Values)
            {
                Destroy(food.gameObject);
            }
            Foods.Clear();

            network.Disconnect(); //접속 끊기
            ConfirmUI gameOverUI = Instantiate(ConfirmUI).GetComponent<ConfirmUI>();
            gameOverUI.Init("게임 승리", "타이틀로", () => { SceneManager.LoadScene("TitleScene"); }); //승리 UI
        }

    }    
}
