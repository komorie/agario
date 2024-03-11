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

    public void Init() //�ʱ�ȭ(Room�� �ʱ�ȭ�� ����)
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

    private void Release() //����
    {
        for (int idx = 0; idx < players.Count; idx++)
        {
            players[idx].PlayerEater.EatFoodEvent -= OnPlayerEatFood;
            players[idx].PlayerEater.EatPlayerEvent -= OnPlayerEatPlayer;
        }
        room.AddPlayerEvent -= OnAddPlayer;
        packetReceiver.OnBroadcastLeaveGame -= RecvLeaveGame;
    }

    private void UpdateUI() //UI ����
    {
        players.Sort((x, y) => y.PlayerEater.Radius.CompareTo(x.PlayerEater.Radius)); //���� �������� ������ ��ŷ �迭

        if (players.Count == 1) return;

        for (int i = 0; i < 5; i++) //���� 5���� ���
        {
            if(players.Count <= i) //�������� �����
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


    private void OnAddPlayer(Player newPlayer) //�÷��̾ �߰��Ǿ��� ��, UI ����
    {
        players.Add(newPlayer);
        newPlayer.PlayerEater.EatFoodEvent += OnPlayerEatFood;
        newPlayer.PlayerEater.EatPlayerEvent += OnPlayerEatPlayer;
        UpdateUI();
    }

    private void RecvLeaveGame(S_BroadcastLeaveGame packet) //�÷��̾ ���� �������� �� UI ����
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

    public void OnPlayerEatFood(EatFoodEventArgs args) //� �÷��̾ ������ �Ծ��� �� UI ����
    {
        UpdateUI();
    }

    public void OnPlayerEatPlayer(EatPlayerEventArgs args) //� �÷��̾ �ٸ� �÷��̾ �Ծ��� �� UI ����
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
