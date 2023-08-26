using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static S_RoomList;

public class RoomManager : GOSingleton<RoomManager>   
{
    Myplayer myPlayer;
    public Dictionary<int, Player> Players { get; set; } = new Dictionary<int, Player>();
    public Dictionary<int, Food> Foods { get; set; } = new Dictionary<int, Food>(); 

    GameObject playerPrefab;
    GameObject myPlayerPrefab;
    GameObject foodPrefab;

    private void Awake()
    {
        playerPrefab = Resources.Load<GameObject>("Prefabs/Player");    
        myPlayerPrefab = Resources.Load<GameObject>("Prefabs/MyPlayer");
        foodPrefab = Resources.Load<GameObject>("Prefabs/Food");
    }

    public void InitRoom(S_RoomList packet) //ó���� ������ �÷��̾�� ��� �޾Ƽ� ���� ��ųʸ��� �߰���.
    {
        foreach (S_RoomList.Player p in packet.players)
        {
            if(p.isSelf)
            {
                GameObject go = Instantiate(myPlayerPrefab);
                Myplayer mPlayer = go.AddComponent<Myplayer>();    
                mPlayer.transform.position = new Vector3(p.posX, p.posY, p.posZ);
                myPlayer = mPlayer;
                myPlayer.PlayerId = p.playerId;
                myPlayer.Radius = myPlayer.transform.localScale.x * 0.5f;
            }
            else
            {
                GameObject go = Instantiate(playerPrefab);
                Player player = go.AddComponent<Player>();
                player.transform.position = new Vector3(p.posX, p.posY, p.posZ);
/*                player.Radius = myPlayer.transform.localScale.x * 0.5f;*/
                Players.Add(p.playerId, player);
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
    }

    public void EnterGame(S_BroadcastEnterGame p) //�������Լ� ���ο� ������ ���Դ�! ��� ��Ŷ�� �޴� ��� -> �ϴ� MyPlayer�ϸ��� ����(��� ����)
    {
        if(p.playerId == myPlayer.PlayerId) //�̹� �ִ� �÷��̾��� ���
        {
            return;
        }   

        GameObject go = Instantiate(playerPrefab);  

        Player player = go.AddComponent<Player>(); //�� �÷��̾� �߰�
        player.transform.position = new Vector3(p.posX, p.posY, p.posZ);
        Players.Add(p.playerId, player);

    }

    public void Move(S_BroadcastMove p)
    {
        //�ٸ� �÷��̾��� ��� �� ��Ŷ��� ��ġ ����, ���� ���� ����
        if (myPlayer.PlayerId != p.playerId)
        {
            Player player;
            DateTime now = DateTime.UtcNow;
            float currentSecond = now.Hour * 3600 + now.Minute * 60 + now.Second + now.Millisecond * 0.001f;
            float RTT = currentSecond - p.time;


            //����3 ��ǥ ���� ���ϱ�
            
            Debug.Log($"RTT: {RTT}"); //�߼� �ð��� ���� �ð��� �� ����
            if (Players.TryGetValue(p.playerId, out player))
            {
                player.MoveVector = new Vector2(p.dirX, p.dirY); //�ٸ� �÷��̾��� �̵� ����
                player.TargetPosition = new Vector3(p.posX + p.dirX * player.Speed * RTT, p.posY + p.dirY * player.Speed * RTT, p.posZ);
                //�ٸ� �÷��̾��� ���� ��ġ ����
                //Myplayer�� �����Ӵ� dir * Speed * Time.deltaTime ��ŭ ���� ��ġ�� �̵��ϸ� Time.deltaTime�� �����Ӵ� �帥 �ð����̴�. �� dir * Speed�� 1�ʴ� �̵��� �ӵ��� �ȴ�. �� 1�ʴ� �̵��Ÿ��� 20
                //�׷� dir * Speed * RTT�� RTT�� ��ŭ �̵��� ��ġ�̴�. �������� �� ��ġ + dir * Speed * RTT �� ����� ���� ��ġ��� �����ϰ�, �� Ŭ�󿡼� ���� ��ġ�� ������ �ǽ�
                float currentDistance = Vector3.Distance(player.transform.position, player.TargetPosition); //��� Ŭ�� ������ ���� ��ġ�� �� Ŭ�� �����ؼ� �̵���Ų ��ġ�� �Ÿ� ��
                Debug.Log($"{RTT}�ʵ��� �̵��� �Ÿ�: {p.dirY * player.Speed * RTT}");   
                player.IsLerping = true; //���� ���� ����
            }
        }
    }

    public void LeaveGame(S_BroadcastLeaveGame p) //������ ���� �����ߴٴ� ��Ŷ�� ���� ��
    {
        if(myPlayer.PlayerId == p.playerId)
        {
            Destroy(myPlayer.gameObject);
            myPlayer = null;
        }
        else
        {
            Player player;
            if(Players.TryGetValue(p.playerId, out player)) //������ �÷��̾� ������Ʈ �ֳ� Ȯ���ϰ�, ������ ����
            {
                Destroy(player.gameObject);
                Players.Remove(p.playerId);
            }       
        }
    }

    public void EatFood(S_BroadcastEatFood p)
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
}
