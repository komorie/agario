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

    public void InitRoom(S_RoomList packet) //ó���� �������� ��, �̹� ������ �ִ� �÷��̾�� ��� �޾Ƽ� ���� ��ųʸ��� �߰���.
    {

        foreach (S_RoomList.Player p in packet.players)
        {
            if(p.isSelf) //�� �÷��̾��� ��� �÷��̾� ���̵�� �ʱ� ��ġ �޾ƿ���, �������� �׳� �ʱ�ȭ
            {
                GameObject go = Instantiate(myPlayerPrefab);
                Myplayer mPlayer = go.AddComponent<Myplayer>();    

                mPlayer.transform.position = new Vector3(p.posX, p.posY, p.posZ);
                myPlayer = mPlayer;
                myPlayer.PlayerId = p.playerId; 
                myPlayer.Radius = myPlayer.transform.localScale.x * 0.5f;
            }
            else //�̹� �������ִ� ���� ���
            {
                GameObject go = Instantiate(playerPrefab);
                Player player = go.AddComponent<Player>();
 
                player.transform.position = new Vector3(p.posX, p.posY, p.posZ);
                player.PlayerId = p.playerId;
                player.Radius = p.radius;
                player.transform.localScale = new Vector3(p.radius * 2, p.radius * 2, p.radius * 2); //������ �޾ƿͼ� ������ ����

                Players.Add(p.playerId, player); //��ųʸ��� �߰�
            }
        }

        for (int f = 0; f < packet.foods.Count; f++) //�ʿ� �ִ� ���ĵ� ���� ��ųʸ��� �߰�
        {
            GameObject go = Instantiate(foodPrefab);
            Food food = go.GetComponent<Food>();

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
            myPlayer.moveAction.Disable(); //��Ī �߿��� �������� ���ϰ�
        }
    }

    public void EnterGame(S_BroadcastEnterGame p) //�������Լ� ���ο� ������ ���Դ�! ��� ��Ŷ�� �޴� ��� -> �ϴ� MyPlayer�ϸ��� ����(��� ����)
    {
        if(p.playerId != myPlayer.PlayerId) //���� ���Դٴ� �˸��� �ƴ� ���
        {
            GameObject go = Instantiate(playerPrefab);

            Player player = go.AddComponent<Player>(); //�� �÷��̾� �߰�
            player.PlayerId = p.playerId;
            player.Radius = 1.5f; //�ʱⰪ 
            player.transform.position = new Vector3(p.posX, p.posY, p.posZ);
            Players.Add(p.playerId, player);

            if(Players.Count == 1) //��Ī ������� ��Ȳ���� ���� ����
            {
                GameObject matchUI = GameObject.Find("MatchUI");
                if (matchUI != null)
                {
                    Destroy(matchUI); //��� UI ����
                    myPlayer.moveAction.Enable(); //������ �� �ְ�    
                }
            }
        }   
    }

    public void Move(S_BroadcastMove p)
    {

        if (myPlayer == null) return;


        //�ٸ� �÷��̾��� ��� �� ��Ŷ��� ��ġ ����, ���� ���� ����
        if (myPlayer.PlayerId != p.playerId)
        {
            Player player;
            DateTime now = DateTime.UtcNow;
            float currentSecond = now.Hour * 3600 + now.Minute * 60 + now.Second + now.Millisecond * 0.001f;
            float RTT = currentSecond - p.time;


            //����3 ��ǥ ���� ���ϱ�
            
/*            Debug.Log($"RTT: {RTT}"); //�߼� �ð��� ���� �ð��� �� ����*/
            if (Players.TryGetValue(p.playerId, out player))
            {
                player.MoveVector = new Vector2(p.dirX, p.dirY); //�ٸ� �÷��̾��� �̵� ����
                player.TargetPosition = new Vector3(p.posX + p.dirX * player.Speed * RTT, p.posY + p.dirY * player.Speed * RTT, p.posZ);
                //�ٸ� �÷��̾��� ���� ��ġ ����
                //Myplayer�� �����Ӵ� dir * Speed * Time.deltaTime ��ŭ ���� ��ġ�� �̵��ϸ� Time.deltaTime�� �����Ӵ� �帥 �ð����̴�. �� dir * Speed�� 1�ʴ� �̵��� �ӵ��� �ȴ�. �� 1�ʴ� �̵��Ÿ��� 20
                //�׷� dir * Speed * RTT�� RTT�� ��ŭ �̵��� ��ġ�̴�. �������� �� ��ġ + dir * Speed * RTT �� ����� ���� ��ġ��� �����ϰ�, �� Ŭ�󿡼� ���� ��ġ�� ������ �ǽ�
                float currentDistance = Vector3.Distance(player.transform.position, player.TargetPosition); //��� Ŭ�� ������ ���� ��ġ�� �� Ŭ�� �����ؼ� �̵���Ų ��ġ�� �Ÿ� ��
/*                Debug.Log($"{RTT}�ʵ��� �̵��� �Ÿ�: {p.dirY * player.Speed * RTT}");  */ 
                player.IsLerping = true; //���� ���� ����
            }
        }
    }

    public void LeaveGame(S_BroadcastLeaveGame p) //������ ���� ����(Disconnect)�ߴٴ� ��Ŷ�� ���� ��
    {
        if(myPlayer == null) return;

        if(myPlayer.PlayerId != p.playerId) //�ٸ� ����� ������ ���
        {
            Player player;
            if (Players.TryGetValue(p.playerId, out player)) //������ �÷��̾� ������Ʈ �ֳ� Ȯ���ϰ�, ������ ����
            {
                Destroy(player.gameObject);
                Players.Remove(p.playerId);
            }
        }
    }

    public void EatFood(S_BroadcastEatFood p) //���� ���� �Ծ���
    {
        if(p.playerId != myPlayer.PlayerId) //���� �÷��̾� ũ�� �ø���
        {
            Player player;
            if(Players.TryGetValue(p.playerId, out player))
            {
                player.transform.localScale += 0.1f * Vector3.one;
                player.Radius = player.transform.localScale.x * 0.5f;
            }
        }   

        Food food = Foods[p.foodId];
        food.transform.position = new Vector3(p.posX, p.posY, 0); //���� ���� ���ο� ��ġ�� ����

        if(p.playerId == myPlayer.PlayerId)
        {
            food.gameObject.SetActive(true); //���� �Ծ��� �����̸� ��Ȱ��ȭ�Ǿ����� ���̹Ƿ� ��Ȱ��ȭ
        }
    }

    public void EatPlayer(S_BroadcastEatPlayer p)
    {
        Player prey;
        Player predator;

        if (p.preyId == myPlayer.PlayerId) //���� �����Ÿ�? �� ����
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
                predator.Radius = predator.transform.localScale.x * 0.5f;   //������ Ű���
                Players.Remove(p.preyId); //���� �÷��̾� ��ųʸ����� ����
                Destroy(prey.gameObject); //���� �÷��̾� ������Ʈ ����
            }
        }

        if(Players.Count == 0) //������ �� �׾���
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
            ConfirmUI gameOverUI = Instantiate(ConfirmUI).GetComponent<ConfirmUI>();
            gameOverUI.Init("���� �¸�", "Ÿ��Ʋ��", () => { SceneManager.LoadScene("TitleScene"); }); //�¸� UI
        }

    }    
}
