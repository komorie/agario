using JetBrains.Annotations;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static S_RoomList;
using static UnityEditor.Experimental.GraphView.GraphView;

public class ScoreBoard : MonoBehaviour
{

    [SerializeField]
    List<TMP_Text> idTexts;
    [SerializeField]
    List<TMP_Text> scoreTexts;

    Room room;
    PacketReceiver packetReceiver;
    private List<Player> players;

    private void Awake()
    {
        room = Room.Instance;
        packetReceiver = PacketReceiver.Instance;
    }

    private void OnEnable()
    {
        room.InitEvent += Init;
    }

    private void OnDisable()
    {
        room.InitEvent -= Init;
        Release();
    }

    public void Init() //초기화(Room이 초기화된 이후)
    {
        players = new List<Player>(room.Players.Values);

        for (int idx = 0; idx < players.Count; idx++)
        {
            players[idx].PlayerEater.EatFoodEvent += OnPlayerEatFood;
            players[idx].PlayerEater.EatPlayerEvent += OnPlayerEatPlayer;
        }
        room.AddPlayerEvent += OnAddPlayer;
        packetReceiver.OnBroadcastLeaveGame += RecvLeaveGame;

        UpdateUI();
    }

    private void Release() //해제
    {
        for (int idx = 0; idx < players.Count; idx++)
        {
            players[idx].PlayerEater.EatFoodEvent -= OnPlayerEatFood;
            players[idx].PlayerEater.EatPlayerEvent -= OnPlayerEatPlayer;
        }
        room.AddPlayerEvent -= OnAddPlayer;
        packetReceiver.OnBroadcastLeaveGame -= RecvLeaveGame;
    }

    private void UpdateUI() //UI 갱신
    {
        players.Sort((x, y) => y.PlayerEater.Radius.CompareTo(x.PlayerEater.Radius)); //점수 기준으로 정렬한 랭킹 배열

        if (players.Count == 1) return;

        for (int i = 0; i < 5; i++) //상위 5위권 출력
        {
            if(players.Count <= i) //나갔으면 지우기
            {
                idTexts[i].text = "";
                scoreTexts[i].text = "";
                continue;
            }

            idTexts[i].text = $"Player {players[i].PlayerId}";
            scoreTexts[i].text = players[i].PlayerEater.Radius.ToString("F2");

            if (players[i].PlayerId == room.MyPlayer.PlayerId)
            {
                idTexts[i].color = Color.yellow;
                scoreTexts[i].color = Color.yellow;
            }
            else
            {
                idTexts[i].color = Color.white;
                scoreTexts[i].color = Color.white;
            }
        }
    }


    private void OnAddPlayer(Player newPlayer) //플레이어가 추가되었을 때, UI 갱신
    {
        players.Add(newPlayer);
        newPlayer.PlayerEater.EatFoodEvent += OnPlayerEatFood;
        newPlayer.PlayerEater.EatPlayerEvent += OnPlayerEatPlayer;
        UpdateUI();
    }

    private void RecvLeaveGame(S_BroadcastLeaveGame packet) //플레이어가 접속 해제했을 때 UI 갱신
    {
        for (int idx = 0; idx < players.Count; idx++)
        {
            if (players[idx].PlayerId == packet.playerId)
            {
                players.RemoveAt(idx);
                break;
            }
        }

        UpdateUI();
    }

    public void OnPlayerEatFood(EatFoodEventArgs args) //어떤 플레이어가 음식을 먹었을 때 UI 갱신
    {
        UpdateUI();
    }

    public void OnPlayerEatPlayer(EatPlayerEventArgs args) //어떤 플레이어가 다른 플레이어를 먹었을 때 UI 갱신
    {
        for (int idx = 0; idx < players.Count; idx++)
        {
            if (players[idx].PlayerId == args.preyId)
            {
                players.RemoveAt(idx);
                break;
            }
        }
        UpdateUI();
    }
}
