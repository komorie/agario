using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerManager : GOSingleton<PlayerManager>   
{
    Myplayer myPlayer;
    public Dictionary<int, Player> Players { get; set; } = new Dictionary<int, Player>();
    GameObject playerPrefab;
    GameObject myPlayerPrefab;

    private void Awake()
    {
        playerPrefab = Resources.Load<GameObject>("Prefabs/Player");    
        myPlayerPrefab = Resources.Load<GameObject>("Prefabs/MyPlayer");    
    }

    public void Add(S_PlayerList packet) //ó���� ������ �÷��̾�� ��� �޾Ƽ� ���� ��ųʸ��� �߰���.
    {
        foreach (S_PlayerList.Player p in packet.players)
        {
            if(p.isSelf)
            {
                GameObject go = Instantiate(myPlayerPrefab);
                Myplayer mPlayer = go.AddComponent<Myplayer>();    
                mPlayer.transform.position = new Vector3(p.posX, p.posY, p.posZ);
                myPlayer = mPlayer;
                myPlayer.PlayerId = p.playerId;
            }
            else
            {
                GameObject go = Instantiate(playerPrefab);
                Player player = go.AddComponent<Player>();
                player.transform.position = new Vector3(p.posX, p.posY, p.posZ);
                Players.Add(p.playerId, player);
            }
            
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
            if (Players.TryGetValue(p.playerId, out player))
            {
                player.MoveVector = new Vector2(p.dirX, p.dirY); //�ٸ� �÷��̾��� �̵� ����
                player.TargetPosition = new Vector3(p.posX, p.posY, p.posZ);  //�ٸ� �÷��̾��� ���� ���� ��ġ

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
}
