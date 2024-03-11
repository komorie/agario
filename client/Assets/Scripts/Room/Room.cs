using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class Room : GOSingleton<Room>
{
    private const int ROOMSIZE_X = 100;
    private const int ROOMSIZE_Y = 100;
    private const int FOOD_COUNT = 50;
    private const int AIPLAYER_COUNT = 10;
    public const float DEFAULT_RADIUS = 1.5f;

    public Player MyPlayer { get; set; }
    NetworkManager network;
    PacketReceiver packetReceiver;

    public Dictionary<int, Player> Players { get; set; } = new Dictionary<int, Player>();
    public Dictionary<int, Food> Foods { get; set; } = new Dictionary<int, Food>();
    
    GameObject foodPrefab;
    GameObject aiPlayerPrefab;
    GameObject netPlayerPrefab;
    GameObject singleMyPlayerPrefab;
    GameObject multiMyPlayerPrefab;
    GameObject confirmUIPrefab;
    GameObject scoreBoardPrefab;

    ConfirmUI matchUI;

    public Action InitEvent;
    public Action<Player> AddPlayerEvent;

    private void Awake()
    {
        foodPrefab = Resources.Load<GameObject>("Prefabs/food");
        aiPlayerPrefab = Resources.Load<GameObject>("Prefabs/AIPlayer");
        netPlayerPrefab = Resources.Load<GameObject>("Prefabs/NetPlayer");
        singleMyPlayerPrefab = Resources.Load<GameObject>("Prefabs/SingleMyPlayer");
        multiMyPlayerPrefab = Resources.Load<GameObject>("Prefabs/MultiMyPlayer");
        confirmUIPrefab = Resources.Load<GameObject>("Prefabs/ConfirmUI");
        scoreBoardPrefab = Resources.Load<GameObject>("Prefabs/ScoreBoard");

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
            float newPlayerX, newPlayerY;
            do
            {
                newPlayerX = Random.value * (ROOMSIZE_X - 10) * 2 - (ROOMSIZE_X - 10);
                newPlayerY = Random.value * (ROOMSIZE_Y - 10) * 2 - (ROOMSIZE_Y - 10);
            }
            while (OverlapWithPlayer(newPlayerX, newPlayerY));

            //반지름 받아와서 사이즈 결정
            Player player;

            if (p == 1)
            {
                player = Instantiate(singleMyPlayerPrefab).GetComponent<Player>();
                MyPlayer = player;
            }
            else
            {
                player = Instantiate(aiPlayerPrefab).GetComponent<Player>();
            }

            player.PlayerId = p;
            player.PlayerEater.Radius = DEFAULT_RADIUS;
            player.PlayerEater.EatFoodEvent += OnPlayerEatFood;
            player.PlayerEater.EatPlayerEvent += OnPlayerEatPlayer;

            player.transform.position = new Vector3(newPlayerX, newPlayerY, 0);

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

        InitEvent?.Invoke();

    }

    public void InitMultiRoom(S_RoomList packet) //처음에 접속했을 때, 이미 접속해 있던 플레이어들 목록 받아서 관리 딕셔너리에 추가함.
    {
        foreach (S_RoomList.Player p in packet.players)
        {
            Player player;

            if (p.isSelf)
            {
                player = Instantiate(multiMyPlayerPrefab).GetComponent<Player>();
                MyPlayer = player;
            }
            else
            {
                player = Instantiate(netPlayerPrefab).GetComponent<Player>();
            }

            player.transform.position = new Vector3(p.posX, p.posY, p.posZ);
            player.transform.localScale = new Vector3(p.radius * 2, p.radius * 2, p.radius * 2); //반지름 받아와서 사이즈 결정

            player.PlayerId = p.playerId;
            player.PlayerEater.Radius = p.radius;
            player.PlayerEater.EatFoodEvent += OnPlayerEatFood;
            player.PlayerEater.EatPlayerEvent += OnPlayerEatPlayer;

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
            matchUI = Instantiate(confirmUIPrefab).GetComponent<ConfirmUI>();

            matchUI.Init(
                "매칭 중입니다.",
                "취소하기",
                () => {
                    Destroy(matchUI.gameObject);
                    MyPlayer = null;
                    Players.Clear();
                    Foods.Clear();
                    network.Disconnect();
                    SceneManager.LoadScene("TitleScene"); //취소하면 연결 끊고 전부 초기화, 타이틀로 돌아가기
                }
            );

            KeyMover mover = MyPlayer.PlayerMover as KeyMover;
            mover.MoveAction.Disable();
        }

        InitEvent?.Invoke();
    }

    public void RecvRoomList(S_RoomList packet)
    {
        InitMultiRoom(packet);
    }

    public void RecvEnterGame(S_BroadcastEnterGame p) //서버에게서 새로운 유저가 들어왔다는 패킷을 받는 경우
    {
        if (p.playerId == MyPlayer.PlayerId) return; 

        Player player = Instantiate(netPlayerPrefab).GetComponent<Player>(); //새 플레이어 추가

        player.transform.position = new Vector3(p.posX, p.posY, p.posZ);
        player.PlayerId = p.playerId;
        player.PlayerEater.EatFoodEvent += OnPlayerEatFood;
        player.PlayerEater.EatPlayerEvent += OnPlayerEatPlayer;

        Players.Add(p.playerId, player);

        if (Players.Count == 2) //매칭 대기중인 상황에서 누가 들어옴
        {
            if (matchUI != null)
            {
                Destroy(matchUI.gameObject); //대기 UI 삭제
                KeyMover mover = MyPlayer.PlayerMover as KeyMover;
                mover.MoveAction.Enable();
            }
        }

        AddPlayerEvent?.Invoke(player);
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
        Food food = Foods[args.foodId];
        if (!GameScene.IsMulti)
        {
            //싱글 플레이 시 로직: 직접 음식의 새로운 위치 지정
            float posX, posY;

            do
            {
                posX = Random.value * (ROOMSIZE_X - 5) * 2 - (ROOMSIZE_X - 5);
                posY = Random.value * (ROOMSIZE_Y - 5) * 2 - (ROOMSIZE_Y - 5);
            }
            while (OverlapWithPlayer(posX, posY)); //플레이어와 겹치지 않는 위치

            food.transform.position = new Vector3(posX, posY, 0);
        }
        else
        {
            //멀티 플레이 시 로직: 서버에서 음식의 새로운 위치 지정
            food.transform.position = new Vector3(args.packet.posX, args.packet.posY, 0);
        }
    }

    public void OnPlayerEatPlayer(EatPlayerEventArgs args) //어떤 플레이어가 다른 플레이어를 먹었을 때
    {
        Player prey;
        if (Players.TryGetValue(args.preyId, out prey)) //먹힌 플레이어 딕셔너리에서 삭제
        {
            Players.Remove(args.preyId);
            prey.PlayerEater.EatFoodEvent -= OnPlayerEatFood;
            prey.PlayerEater.EatPlayerEvent -= OnPlayerEatPlayer;
        }

        if (args.preyId == MyPlayer.PlayerId || Players.Count == 1) //게임 종료 조건을 만족할 시 처리
        {
            ShowGameOverUI(args.preyId != MyPlayer.PlayerId);
            DestroyRoom(); 
        }
    }

    private void ShowGameOverUI(bool isWin = true) //게임 오버 UI
    {
        ConfirmUI gameOverUI = Instantiate(confirmUIPrefab).GetComponent<ConfirmUI>(); //게임오버 UI
        string endString = isWin ? "게임 승리" : "게임 패배";
        gameOverUI.Init(endString, "타이틀로", () => { SceneManager.LoadScene("TitleScene"); });
    }

    private void DestroyRoom() //방 파괴
    {
        //방 오브젝트 전부 파괴
        Destroy(MyPlayer.gameObject);
        foreach (var player in Players.Values) Destroy(player.gameObject);
        foreach (var food in Foods.Values) Destroy(food.gameObject);

        MyPlayer = null;
        Players.Clear();
        Foods.Clear();

        if (GameScene.IsMulti) //멀티플레이 중이면 연결 해제
        {
            network.Disconnect();
        }
    }
    private bool OverlapWithPlayer(float x, float y)
    {
        foreach (var p in Players)
        {
            Vector3 pVec = p.Value.gameObject.transform.position;
            float distance = (float)Math.Sqrt(Math.Pow(pVec.x - x, 2) + Math.Pow(pVec.y - y, 2));
            if (distance < p.Value.PlayerEater.Radius)
            {
                return true;  // 겹치는 플레이어 발견
            }
        }
        return false;  // 겹치는 플레이어 없음
    }
}
