using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : GOSingleton<PlayerManager>   
{
    Myplayer myPlayer;
    Dictionary<int, Player> players = new Dictionary<int, Player>();
    GameObject playerPrefab;

    protected void Awake()
    {
        playerPrefab = Resources.Load<GameObject>("Prefabs/Player");    
    }

    public void Add(S_PlayerList packet) //ó���� ������ �÷��̾�� ��� �޾Ƽ� ���� ��ųʸ��� �߰���.
    {
        foreach (S_PlayerList.Player p in packet.players)
        {
            GameObject go = Instantiate(playerPrefab);

            if(p.isSelf)
            {
                Myplayer mPlayer = go.AddComponent<Myplayer>();    
                mPlayer.transform.position = new Vector3(p.posX, p.posY, p.posZ);
                myPlayer = mPlayer;
                myPlayer.PlayerId = p.playerId;
            }
            else
            {
                Player player = go.AddComponent<Player>();
                player.transform.position = new Vector3(p.posX, p.posY, p.posZ);
                players.Add(p.playerId, player);
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
        players.Add(p.playerId, player);

    }

    public void Move(S_BroadcastMove p)
    {
        //�ϴ� ���ΰ��� �ٸ� �÷��̾��� ��� ��� ���� ��ġ�� �ٲ���
        if (myPlayer.PlayerId == p.playerId)
        {
            myPlayer.transform.position = new Vector3(p.posX, p.posY, p.posZ);
        }
        else
        {
            Player player;
            if (players.TryGetValue(p.playerId, out player))
            {
                player.transform.position = new Vector3(p.posX, p.posY, p.posZ);
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
            if(players.TryGetValue(p.playerId, out player)) //������ �÷��̾� ������Ʈ �ֳ� Ȯ���ϰ�, ������ ����
            {
                Destroy(player.gameObject);
                players.Remove(p.playerId);
            }       
        }
    }
}
