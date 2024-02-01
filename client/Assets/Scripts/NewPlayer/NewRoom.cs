using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NewRoom : GOSingleton<NewRoom>
{
    int roomSizeX;
    int roomSizeY;

    NewPlayer myPlayer;
    RainSpawner rainSpawner;
    NetworkManager network;
    PacketReceiver packetReceiver;

    public Dictionary<int, NewPlayer> Players { get; set; } = new Dictionary<int, NewPlayer>();
    public Dictionary<int, Food> Foods { get; set; } = new Dictionary<int, Food>();

    GameObject playerPrefab;
    GameObject foodPrefab;
    GameObject myPlayerPrefab;
    GameObject ConfirmUI;

    private void Awake()
    {
        playerPrefab = Resources.Load<GameObject>("Prefabs/Player");
        foodPrefab = Resources.Load<GameObject>("Prefabs/food");
        myPlayerPrefab = Resources.Load<GameObject>("Prefabs/MyPlayer");
        ConfirmUI = Resources.Load<GameObject>("Prefabs/ConfirmUI");
        network = NetworkManager.Instance;
        packetReceiver = PacketReceiver.Instance;
        rainSpawner = GameObject.Find("RainSpawner").GetOrAddComponent<RainSpawner>();
    }

    private void OnEnable() //특정 패킷 받을 시 수행할 함수들 구독
    {
        packetReceiver.OnRoomList += RecvRoomList;
        packetReceiver.OnBroadcastEnterGame += RecvEnterGame;
        packetReceiver.OnBroadcastLeaveGame += RecvLeaveGame;
        packetReceiver.OnBroadcastEatFood += RecvEatFood;
        packetReceiver.OnBroadcastEatPlayer += RecvEatPlayer;
    }

    private void OnDisable()
    {
        packetReceiver.OnRoomList -= RecvRoomList;
        packetReceiver.OnBroadcastEnterGame -= RecvEnterGame;
        packetReceiver.OnBroadcastLeaveGame -= RecvLeaveGame;
        packetReceiver.OnBroadcastEatFood -= RecvEatFood;
        packetReceiver.OnBroadcastEatPlayer -= RecvEatPlayer;
    }

    private void Update()
    {
        CheckPlayerInMap();
    }


    public void CheckPlayerInMap()
    {
        if (myPlayer != null)
        {
            Vector3 myPos = myPlayer.transform.position;

            //프레임이 낮으면 벽 넘어갈 수도 있으므로 체크
            if (myPos.x < -roomSizeX + myPlayer.PlayerEater.Radius - 1)
                myPos.x = -roomSizeX + myPlayer.PlayerEater.Radius;
            else if (myPos.x > roomSizeX - myPlayer.PlayerEater.Radius + 1)
                myPos.x = roomSizeX - myPlayer.PlayerEater.Radius;
            if (myPos.y < -roomSizeY + myPlayer.PlayerEater.Radius - 1)
                myPos.y = -roomSizeY + myPlayer.PlayerEater.Radius;
            else if (myPos.y > roomSizeY - myPlayer.PlayerEater.Radius + 1)
                myPos.y = roomSizeY - myPlayer.PlayerEater.Radius;

            myPlayer.transform.position = myPos;
        }
    }

    public void RecvRoomList(S_RoomList packet) //처음에 접속했을 때, 이미 접속해 있던 플레이어들 목록 받아서 관리 딕셔너리에 추가함.
    {

        foreach (S_RoomList.Player p in packet.players)
        {
            if (p.isSelf) //내 플레이어인 경우 플레이어 아이디와 초기 위치 받아오고, 나머지는 그냥 초기화
            {
                NewPlayer mPlayer = Instantiate(myPlayerPrefab).GetComponent<NewPlayer>();

                mPlayer.transform.position = new Vector3(p.posX, p.posY, p.posZ);
                myPlayer = mPlayer;
                myPlayer.PlayerId = p.playerId;
                myPlayer.PlayerEater.Radius = myPlayer.transform.localScale.x * 0.5f;
            }
            else //이미 접속해있던 남인 경우
            {
                NewPlayer player = Instantiate(playerPrefab).GetComponent<NewPlayer>(); ;

                player.transform.position = new Vector3(p.posX, p.posY, p.posZ);
                player.PlayerId = p.playerId;
                player.PlayerEater.Radius = p.radius;
                player.transform.localScale = new Vector3(p.radius * 2, p.radius * 2, p.radius * 2); //반지름 받아와서 사이즈 결정

                Players.Add(p.playerId, player); //딕셔너리에 추가
            }
        }

        for (int f = 0; f < packet.foods.Count; f++) //맵에 있는 음식들 관리 딕셔너리에 추가
        {
            Food food = Instantiate(foodPrefab).GetComponent<Food>();

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
            KeyMover mover = myPlayer.PlayerMover as KeyMover;
            mover.moveAction.Disable();
        }
    }

    public void RecvEnterGame(S_BroadcastEnterGame p) //서버에게서 새로운 유저가 들어왔다는 패킷을 받는 경우 -> 일단 MyPlayer일리는 없음
    {
        if (p.playerId != myPlayer.PlayerId) //내가 들어왔다는 알림이 아닌 경우
        {
            NewPlayer player = Instantiate(playerPrefab).GetComponent<NewPlayer>(); //새 플레이어 추가
            player.PlayerId = p.playerId;
            player.transform.position = new Vector3(p.posX, p.posY, p.posZ);
            Players.Add(p.playerId, player);

            if (Players.Count == 1) //매칭 대기중인 상황에서 누가 들어옴
            {
                GameObject matchUI = GameObject.Find("MatchUI");
                if (matchUI != null)
                {
                    Destroy(matchUI); //대기 UI 삭제
                    KeyMover mover = myPlayer.PlayerMover as KeyMover;
                    mover.moveAction.Enable();
                }
            }
        }
    }

    public void RecvLeaveGame(S_BroadcastLeaveGame p) //누군가 게임 종료(Disconnect)했다는 패킷이 왔을 때
    {
        NewPlayer player;
        if (Players.TryGetValue(p.playerId, out player)) //종료한 플레이어 오브젝트 있나 확인하고, 있으면 삭제
        {
            Players.Remove(p.playerId);
            Destroy(player.gameObject);
        }
    }

    public void RecvEatFood(S_BroadcastEatFood p) //음식 먹었다는 패킷 받음
    {
        Food food = Foods[p.foodId];
        food.transform.position = new Vector3(p.posX, p.posY, 0); //음식 새로운 위치로 스폰
    }

    public void RecvEatPlayer(S_BroadcastEatPlayer p)
    {
        NewPlayer prey;
        NewPlayer predator;

        if (myPlayer == null) return;

        if (p.preyId == myPlayer.PlayerId) //내가 먹힌거면
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
            Destroy(rainSpawner.gameObject);
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
                predator.PlayerEater.Radius = predator.transform.localScale.x * 0.5f;   //반지름 키우고
                Players.Remove(p.preyId); //먹힌 플레이어 딕셔너리에서 삭제
                Destroy(prey.gameObject); //먹힌 플레이어 오브젝트 삭제
            }
        }
        else //내가 먹은 경우
        {
            if (Players.TryGetValue(p.preyId, out prey))
            {
                myPlayer.transform.localScale += (prey.transform.localScale / 2); //먹힌 플레이어 크기 반만큼 먹은 플레이어 크기 증가
                myPlayer.PlayerEater.Radius = myPlayer.transform.localScale.x * 0.5f;   //반지름 키우고
                Players.Remove(p.preyId); //먹힌 플레이어 딕셔너리에서 삭제
                Destroy(prey.gameObject); //먹힌 플레이어 오브젝트 삭제
            }
        }

        if (Players.Count == 0) //나말고 다 죽은 경우
        {
            //방 오브젝트 전부 파괴
            Destroy(myPlayer.gameObject);
            myPlayer = null;

            foreach (var player in Players.Values) Destroy(player.gameObject);
            Players.Clear();

            foreach (var food in Foods.Values) Destroy(food.gameObject);
            Foods.Clear();

            network.Disconnect(); //접속 끊기
            Destroy(rainSpawner.gameObject);
            ConfirmUI gameOverUI = Instantiate(ConfirmUI).GetComponent<ConfirmUI>();
            gameOverUI.Init("게임 승리", "타이틀로", () => { SceneManager.LoadScene("TitleScene"); }); //승리 UI
        }

    }
}
