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

    private void OnEnable() //Ư�� ��Ŷ ���� �� ������ �Լ��� ����
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
        for (int p = 1; p <= AIPLAYER_COUNT; p++) //AI �÷��̾� ���� ����
        {
            //�÷��̾�� ��ġ���� ����ؼ� �Ȱ�ġ�� ��ġ�� ���� ����
            //������ �޾ƿͼ� ������ ����
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

            Players.Add(player.PlayerId, player); //��ųʸ��� �߰�
        }

        for (int f = 0; f < FOOD_COUNT; f++) //���� ���� ����
        {
            Food food = Instantiate(foodPrefab).GetComponent<Food>();

            food.FoodId = f;
            float posX = Random.value * (ROOMSIZE_X - 5) * 2 - (ROOMSIZE_X - 5);
            float posY = Random.value * (ROOMSIZE_Y - 5) * 2 - (ROOMSIZE_Y - 5);
            food.transform.position = new Vector3(posX, posY, 0);
            Foods.Add(f, food);
        }
    }

    public void InitMultiRoom(S_RoomList packet) //ó���� �������� ��, �̹� ������ �ִ� �÷��̾�� ��� �޾Ƽ� ���� ��ųʸ��� �߰���.
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
            player.transform.localScale = new Vector3(p.radius * 2, p.radius * 2, p.radius * 2); //������ �޾ƿͼ� ������ ����

            player.PlayerId = p.playerId;
            player.PlayerEater.Radius = p.radius;
            player.PlayerEater.EatFood += OnPlayerEatFood;
            player.PlayerEater.EatPlayer += OnPlayerEatPlayer;

            Players.Add(p.playerId, player); //��ųʸ��� �߰�

        }

        for (int f = 0; f < packet.foods.Count; f++) //�ʿ� �ִ� ���ĵ� ���� ��ųʸ��� �߰�
        {
            Food food = Instantiate(foodPrefab).GetComponent<Food>();

            food.FoodId = f;
            food.transform.position = new Vector3(packet.foods[f].posX, packet.foods[f].posY, 0);
            Foods.Add(f, food);
        }

        if (packet.players.Count < 2) //���� ù��°�� ������ ���� ���� �Ұ�
        {
            //confirmUI�� ����� ��Ī ���̶�� ǥ���ϰ� �ٸ� �÷��̾� ���
            GameObject go = Instantiate(Resources.Load<GameObject>("Prefabs/ConfirmUI"));
            go.name = "MatchUI";
            ConfirmUI confirmUI = go.GetComponent<ConfirmUI>();

            confirmUI.Init(
                "��Ī ���Դϴ�.",
                "����ϱ�",
                () => {
                    Destroy(go);
                    myPlayer = null;
                    Players.Clear();
                    Foods.Clear();
                    network.Disconnect();
                    SceneManager.LoadScene("TitleScene"); //����ϸ� ���� ���� ���� �ʱ�ȭ, Ÿ��Ʋ�� ���ư���
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

    public void RecvEnterGame(S_BroadcastEnterGame p) //�������Լ� ���ο� ������ ���Դٴ� ��Ŷ�� �޴� ���
    {
        if (p.playerId == myPlayer.PlayerId) return; 

        Player player = Instantiate(playerPrefab).GetComponent<Player>(); //�� �÷��̾� �߰�

        player.transform.position = new Vector3(p.posX, p.posY, p.posZ);
        player.PlayerId = p.playerId;
        player.PlayerEater.EatFood += OnPlayerEatFood;
        player.PlayerEater.EatPlayer += OnPlayerEatPlayer;

        Players.Add(p.playerId, player);

        if (Players.Count == 2) //��Ī ������� ��Ȳ���� ���� ����
        {
            GameObject matchUI = GameObject.Find("MatchUI");
            if (matchUI != null)
            {
                Destroy(matchUI); //��� UI ����
                KeyMover mover = myPlayer.PlayerMover as KeyMover;
                mover.MoveAction.Enable();
            }
        }
    }

    public void RecvLeaveGame(S_BroadcastLeaveGame p) //������ ���� ����(Disconnect)�ߴٴ� ��Ŷ�� ���� ��
    {
        Player player;
        if (Players.TryGetValue(p.playerId, out player)) //������ �÷��̾� ������Ʈ �ֳ� Ȯ���ϰ�, ������ ����
        {
            Players.Remove(p.playerId);
            Destroy(player.gameObject);
        }

        if (Players.Count == 1) //���� ���� ������ ������ �� ó��
        {
            ShowGameOverUI(true);
            DestroyRoom();
        }
    }

    public void OnPlayerEatFood(EatFoodEventArgs args) //� �÷��̾ ������ �Ծ��� ��
    {
        if (!GameScene.IsMulti)
        {
            //�̱� �÷��� �� ����
        }
        else
        {
            //��Ƽ �÷��� �� ����: �������� ������ ���ο� ��ġ ����
            Food food = Foods[args.packet.foodId];
            food.transform.position = new Vector3(args.packet.posX, args.packet.posY, 0);
        }
    }

    public void OnPlayerEatPlayer(EatPlayerEventArgs args) //� �÷��̾ �ٸ� �÷��̾ �Ծ��� ��
    {
        Player prey;
        if (Players.TryGetValue(args.preyId, out prey)) //���� �÷��̾� ��ųʸ����� ����
        {
            Players.Remove(args.preyId);
            prey.PlayerEater.EatFood -= OnPlayerEatFood;
            prey.PlayerEater.EatPlayer -= OnPlayerEatPlayer;
        }

        if (args.preyId == myPlayer.PlayerId || Players.Count == 1) //���� ���� ������ ������ �� ó��
        {
            ShowGameOverUI(args.preyId != myPlayer.PlayerId);
            DestroyRoom(); 
        }
    }

    private void ShowGameOverUI(bool isWin = true) //���� ���� UI
    {
        ConfirmUI gameOverUI = Instantiate(ConfirmUI).GetComponent<ConfirmUI>(); //���ӿ��� UI
        string endString = isWin ? "���� �¸�" : "���� �й�";
        gameOverUI.Init(endString, "Ÿ��Ʋ��", () => { SceneManager.LoadScene("TitleScene"); });
    }

    private void DestroyRoom() //�� �ı�
    {
        //�� ������Ʈ ���� �ı�
        Destroy(myPlayer.gameObject);
        foreach (var player in Players.Values) Destroy(player.gameObject);
        foreach (var food in Foods.Values) Destroy(food.gameObject);

        myPlayer = null;
        Players.Clear();
        Foods.Clear();

        if (GameScene.IsMulti) //��Ƽ�÷��� ���̸� ���� ����
        {
            network.Disconnect();
        }
    }
}
