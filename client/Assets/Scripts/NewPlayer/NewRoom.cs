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

    private void OnEnable() //Ư�� ��Ŷ ���� �� ������ �Լ��� ����
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

            //�������� ������ �� �Ѿ ���� �����Ƿ� üũ
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

    public void RecvRoomList(S_RoomList packet) //ó���� �������� ��, �̹� ������ �ִ� �÷��̾�� ��� �޾Ƽ� ���� ��ųʸ��� �߰���.
    {

        foreach (S_RoomList.Player p in packet.players)
        {
            if (p.isSelf) //�� �÷��̾��� ��� �÷��̾� ���̵�� �ʱ� ��ġ �޾ƿ���, �������� �׳� �ʱ�ȭ
            {
                NewPlayer mPlayer = Instantiate(myPlayerPrefab).GetComponent<NewPlayer>();

                mPlayer.transform.position = new Vector3(p.posX, p.posY, p.posZ);
                myPlayer = mPlayer;
                myPlayer.PlayerId = p.playerId;
                myPlayer.PlayerEater.Radius = myPlayer.transform.localScale.x * 0.5f;
            }
            else //�̹� �������ִ� ���� ���
            {
                NewPlayer player = Instantiate(playerPrefab).GetComponent<NewPlayer>(); ;

                player.transform.position = new Vector3(p.posX, p.posY, p.posZ);
                player.PlayerId = p.playerId;
                player.PlayerEater.Radius = p.radius;
                player.transform.localScale = new Vector3(p.radius * 2, p.radius * 2, p.radius * 2); //������ �޾ƿͼ� ������ ����

                Players.Add(p.playerId, player); //��ųʸ��� �߰�
            }
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
            mover.moveAction.Disable();
        }
    }

    public void RecvEnterGame(S_BroadcastEnterGame p) //�������Լ� ���ο� ������ ���Դٴ� ��Ŷ�� �޴� ��� -> �ϴ� MyPlayer�ϸ��� ����
    {
        if (p.playerId != myPlayer.PlayerId) //���� ���Դٴ� �˸��� �ƴ� ���
        {
            NewPlayer player = Instantiate(playerPrefab).GetComponent<NewPlayer>(); //�� �÷��̾� �߰�
            player.PlayerId = p.playerId;
            player.transform.position = new Vector3(p.posX, p.posY, p.posZ);
            Players.Add(p.playerId, player);

            if (Players.Count == 1) //��Ī ������� ��Ȳ���� ���� ����
            {
                GameObject matchUI = GameObject.Find("MatchUI");
                if (matchUI != null)
                {
                    Destroy(matchUI); //��� UI ����
                    KeyMover mover = myPlayer.PlayerMover as KeyMover;
                    mover.moveAction.Enable();
                }
            }
        }
    }

    public void RecvLeaveGame(S_BroadcastLeaveGame p) //������ ���� ����(Disconnect)�ߴٴ� ��Ŷ�� ���� ��
    {
        NewPlayer player;
        if (Players.TryGetValue(p.playerId, out player)) //������ �÷��̾� ������Ʈ �ֳ� Ȯ���ϰ�, ������ ����
        {
            Players.Remove(p.playerId);
            Destroy(player.gameObject);
        }
    }

    public void RecvEatFood(S_BroadcastEatFood p) //���� �Ծ��ٴ� ��Ŷ ����
    {
        Food food = Foods[p.foodId];
        food.transform.position = new Vector3(p.posX, p.posY, 0); //���� ���ο� ��ġ�� ����
    }

    public void RecvEatPlayer(S_BroadcastEatPlayer p)
    {
        NewPlayer prey;
        NewPlayer predator;

        if (myPlayer == null) return;

        if (p.preyId == myPlayer.PlayerId) //���� �����Ÿ�
        {
            //�� ������Ʈ ���� �ı�
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

            network.Disconnect(); //���� ����
            Destroy(rainSpawner.gameObject);
            ConfirmUI gameOverUI = Instantiate(ConfirmUI).GetComponent<ConfirmUI>(); //���ӿ��� UI
            gameOverUI.Init
                (
                    "���� ����",
                    "Ÿ��Ʋ��",
                    () => {
                        SceneManager.LoadScene("TitleScene");
                    }
                );
            return;
        }

        if (p.predatorId != myPlayer.PlayerId) //���� ���� �� �ƴѰ��
        {
            if (Players.TryGetValue(p.predatorId, out predator) && Players.TryGetValue(p.preyId, out prey))
            {
                predator.transform.localScale += (prey.transform.localScale / 2); //���� �÷��̾� ũ�� �ݸ�ŭ ���� �÷��̾� ũ�� ����
                predator.PlayerEater.Radius = predator.transform.localScale.x * 0.5f;   //������ Ű���
                Players.Remove(p.preyId); //���� �÷��̾� ��ųʸ����� ����
                Destroy(prey.gameObject); //���� �÷��̾� ������Ʈ ����
            }
        }
        else //���� ���� ���
        {
            if (Players.TryGetValue(p.preyId, out prey))
            {
                myPlayer.transform.localScale += (prey.transform.localScale / 2); //���� �÷��̾� ũ�� �ݸ�ŭ ���� �÷��̾� ũ�� ����
                myPlayer.PlayerEater.Radius = myPlayer.transform.localScale.x * 0.5f;   //������ Ű���
                Players.Remove(p.preyId); //���� �÷��̾� ��ųʸ����� ����
                Destroy(prey.gameObject); //���� �÷��̾� ������Ʈ ����
            }
        }

        if (Players.Count == 0) //������ �� ���� ���
        {
            //�� ������Ʈ ���� �ı�
            Destroy(myPlayer.gameObject);
            myPlayer = null;

            foreach (var player in Players.Values) Destroy(player.gameObject);
            Players.Clear();

            foreach (var food in Foods.Values) Destroy(food.gameObject);
            Foods.Clear();

            network.Disconnect(); //���� ����
            Destroy(rainSpawner.gameObject);
            ConfirmUI gameOverUI = Instantiate(ConfirmUI).GetComponent<ConfirmUI>();
            gameOverUI.Init("���� �¸�", "Ÿ��Ʋ��", () => { SceneManager.LoadScene("TitleScene"); }); //�¸� UI
        }

    }
}
