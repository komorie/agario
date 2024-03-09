using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Room : GOSingleton<Room>
{
    private const int ROOMSIZE_X = 50;
    private const int ROOMSIZE_Y = 50;
    private const int FOOD_COUNT = 50;
    private const int AIPLAYER_COUNT = 10;

    Player myPlayer;
    NetworkManager network;
    PacketReceiver packetReceiver;

    public Dictionary<int, Player> Players { get; set; } = new Dictionary<int, Player>();
    public Dictionary<int, Food> Foods { get; set; } = new Dictionary<int, Food>();

    GameObject playerPrefab;
    GameObject foodPrefab;
    GameObject myPlayerPrefab;
    GameObject ConfirmUI;

    private void Awake()
    {
        playerPrefab = Resources.Load<GameObject>("Prefabs/NewPlayer");
        foodPrefab = Resources.Load<GameObject>("Prefabs/food");
        myPlayerPrefab = Resources.Load<GameObject>("Prefabs/NewMyPlayer");
        ConfirmUI = Resources.Load<GameObject>("Prefabs/ConfirmUI");
        packetReceiver = PacketReceiver.Instance;

        if (GameScene.IsMulti) network = NetworkManager.Instance;
        if (!GameScene.IsMulti) InitSingleRoom();
    }

    private void OnEnable() //특정 패킷 받을 시 수행할 함수들 구독
    {
        packetReceiver.OnRoomList += RecvRoomList;
        packetReceiver.OnBroadcastEnterGame += RecvEnterGame;
        packetReceiver.OnBroadcastLeaveGame += RecvLeaveGame;
    }

    private void OnDisable()
    {
        packetReceiver.OnRoomList -= RecvRoomList;
        packetReceiver.OnBroadcastEnterGame -= RecvEnterGame;
        packetReceiver.OnBroadcastLeaveGame -= RecvLeaveGame;
    }

    public void InitSingleRoom()
    {
        for (int p = 1; p <= AIPLAYER_COUNT; p++) //AI 플레이어 랜덤 생성
        {
            //플레이어와 겹치는지 계산해서 안겹치는 위치로 새로 지정
            //반지름 받아와서 사이즈 결정
            Player player;

            if (p == 1)
            {
                player = Instantiate(myPlayerPrefab).GetComponent<Player>();
                myPlayer = player;
            }
            else
            {
                player = Instantiate(playerPrefab).GetComponent<Player>();
            }

            player.PlayerId = p;
            player.PlayerEater.Radius = 1.5f;
            player.PlayerEater.EatFood += OnPlayerEatFood;
            player.PlayerEater.EatPlayer += OnPlayerEatPlayer;

            Players.Add(player.PlayerId, player); //딕셔너리에 추가
        }

        for (int f = 0; f < FOOD_COUNT; f++) //음식 랜덤 생성
        {
            Food food = Instantiate(foodPrefab).GetComponent<Food>();

            food.FoodId = f;
            float posX = Random.value * (ROOMSIZE_X - 5) * 2 - (ROOMSIZE_X - 5);
            float posY = Random.value * (ROOMSIZE_Y - 5) * 2 - (ROOMSIZE_Y - 5);
            food.transform.position = new Vector3(posX, posY, 0);
            Foods.Add(f, food);
        }
    }

    public void InitMultiRoom(S_RoomList packet) //처음에 접속했을 때, 이미 접속해 있던 플레이어들 목록 받아서 관리 딕셔너리에 추가함.
    {
        foreach (S_RoomList.Player p in packet.players)
        {
            Player player;

            if (p.isSelf)
            {
                player = Instantiate(myPlayerPrefab).GetComponent<Player>();
                myPlayer = player;
            }
            else
            {
                player = Instantiate(playerPrefab).GetComponent<Player>();
            }

            player.transform.position = new Vector3(p.posX, p.posY, p.posZ);
            player.transform.localScale = new Vector3(p.radius * 2, p.radius * 2, p.radius * 2); //반지름 받아와서 사이즈 결정

            player.PlayerId = p.playerId;
            player.PlayerEater.Radius = p.radius;
            player.PlayerEater.EatFood += OnPlayerEatFood;
            player.PlayerEater.EatPlayer += OnPlayerEatPlayer;

            Players.Add(p.playerId, player); //딕셔너리에 추가

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
            mover.MoveAction.Disable();
        }
    }

    public void RecvRoomList(S_RoomList packet)
    {
        InitMultiRoom(packet);
    }

    public void RecvEnterGame(S_BroadcastEnterGame p) //서버에게서 새로운 유저가 들어왔다는 패킷을 받는 경우
    {
        if (p.playerId == myPlayer.PlayerId) return; 

        Player player = Instantiate(playerPrefab).GetComponent<Player>(); //새 플레이어 추가

        player.transform.position = new Vector3(p.posX, p.posY, p.posZ);
        player.PlayerId = p.playerId;
        player.PlayerEater.EatFood += OnPlayerEatFood;
        player.PlayerEater.EatPlayer += OnPlayerEatPlayer;

        Players.Add(p.playerId, player);

        if (Players.Count == 2) //매칭 대기중인 상황에서 누가 들어옴
        {
            GameObject matchUI = GameObject.Find("MatchUI");
            if (matchUI != null)
            {
                Destroy(matchUI); //대기 UI 삭제
                KeyMover mover = myPlayer.PlayerMover as KeyMover;
                mover.MoveAction.Enable();
            }
        }
    }

    public void RecvLeaveGame(S_BroadcastLeaveGame p) //누군가 게임 종료(Disconnect)했다는 패킷이 왔을 때
    {
        Player player;
        if (Players.TryGetValue(p.playerId, out player)) //종료한 플레이어 오브젝트 있나 확인하고, 있으면 삭제
        {
            Players.Remove(p.playerId);
            Destroy(player.gameObject);
        }

        if (Players.Count == 1) //게임 종료 조건을 만족할 시 처리
        {
            ShowGameOverUI(true);
            DestroyRoom();
        }
    }

    public void OnPlayerEatFood(EatFoodEventArgs args) //어떤 플레이어가 음식을 먹었을 때
    {
        if (!GameScene.IsMulti)
        {
            //싱글 플레이 시 로직
        }
        else
        {
            //멀티 플레이 시 로직: 서버에서 음식의 새로운 위치 지정
            Food food = Foods[args.packet.foodId];
            food.transform.position = new Vector3(args.packet.posX, args.packet.posY, 0);
        }
    }

    public void OnPlayerEatPlayer(EatPlayerEventArgs args) //어떤 플레이어가 다른 플레이어를 먹었을 때
    {
        Player prey;
        if (Players.TryGetValue(args.preyId, out prey)) //먹힌 플레이어 딕셔너리에서 삭제
        {
            Players.Remove(args.preyId);
            prey.PlayerEater.EatFood -= OnPlayerEatFood;
            prey.PlayerEater.EatPlayer -= OnPlayerEatPlayer;
        }

        if (args.preyId == myPlayer.PlayerId || Players.Count == 1) //게임 종료 조건을 만족할 시 처리
        {
            ShowGameOverUI(args.preyId != myPlayer.PlayerId);
            DestroyRoom(); 
        }
    }

    private void ShowGameOverUI(bool isWin = true) //게임 오버 UI
    {
        ConfirmUI gameOverUI = Instantiate(ConfirmUI).GetComponent<ConfirmUI>(); //게임오버 UI
        string endString = isWin ? "게임 승리" : "게임 패배";
        gameOverUI.Init(endString, "타이틀로", () => { SceneManager.LoadScene("TitleScene"); });
    }

    private void DestroyRoom() //방 파괴
    {
        //방 오브젝트 전부 파괴
        Destroy(myPlayer.gameObject);
        foreach (var player in Players.Values) Destroy(player.gameObject);
        foreach (var food in Foods.Values) Destroy(food.gameObject);

        myPlayer = null;
        Players.Clear();
        Foods.Clear();

        if (GameScene.IsMulti) //멀티플레이 중이면 연결 해제
        {
            network.Disconnect();
        }
    }
}
