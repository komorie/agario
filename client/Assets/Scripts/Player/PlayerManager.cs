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

    public void Add(S_PlayerList packet) //처음에 접속한 플레이어들 목록 받아서 관리 딕셔너리에 추가함.
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

    public void EnterGame(S_BroadcastEnterGame p) //서버에게서 새로운 유저가 들어왔다! 라는 패킷을 받는 경우 -> 일단 MyPlayer일리는 없음(고려 안함)
    {
        if(p.playerId == myPlayer.PlayerId) //이미 있는 플레이어인 경우
        {
            return;
        }   

        GameObject go = Instantiate(playerPrefab);  

        Player player = go.AddComponent<Player>(); //새 플레이어 추가
        player.transform.position = new Vector3(p.posX, p.posY, p.posZ);
        Players.Add(p.playerId, player);

    }

    public void Move(S_BroadcastMove p)
    {
        //다른 플레이어인 경우 온 패킷대로 위치 조정, 나인 경우는 무시
        if (myPlayer.PlayerId != p.playerId)
        {
            Player player;
            if (Players.TryGetValue(p.playerId, out player))
            {
                player.MoveVector = new Vector2(p.dirX, p.dirY); //다른 플레이어의 이동 방향
                player.TargetPosition = new Vector3(p.posX, p.posY, p.posZ);  //다른 플레이어의 실제 현재 위치

                player.IsLerping = true; //이제 보간 시작
            }
        }
    }

    public void LeaveGame(S_BroadcastLeaveGame p) //누군가 게임 종료했다는 패킷이 왔을 때
    {
        if(myPlayer.PlayerId == p.playerId)
        {
            Destroy(myPlayer.gameObject);
            myPlayer = null;
        }
        else
        {
            Player player;
            if(Players.TryGetValue(p.playerId, out player)) //종료한 플레이어 오브젝트 있나 확인하고, 있으면 삭제
            {
                Destroy(player.gameObject);
                Players.Remove(p.playerId);
            }       
        }
    }
}
