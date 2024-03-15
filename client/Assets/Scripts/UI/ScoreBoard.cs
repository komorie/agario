using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreBoard : MonoBehaviour
{

    [SerializeField]
    List<TMP_Text> idTexts;
    [SerializeField]
    List<TMP_Text> scoreTexts;

    int myPlayerId;
    private List<Player> players;

    public void Init(Dictionary<int, Player> players, int myPlayerId) //�ʱ�ȭ(Room�� �ʱ�ȭ�� ����)
    {
        this.players = new List<Player>(players.Values);
        this.myPlayerId = myPlayerId;
        UpdateUI();
    }

    private void UpdateUI() //UI ����
    {
        players.Sort((x, y) => y.Radius.CompareTo(x.Radius)); //���� �������� ������ ��ŷ �迭

        for (int i = 0; i < 5; i++) //���� 5���� ���
        {
            if(players.Count <= i) //�������� �����
            {
                idTexts[i].text = "";
                scoreTexts[i].text = "";
                continue;
            }

            idTexts[i].text = $"Player {players[i].PlayerId}";
            scoreTexts[i].text = players[i].Radius.ToString("F2");

            if (players[i].PlayerId == myPlayerId)
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

    public void UpdateEatFood()
    {
        UpdateUI();
    }

    public void UpdateAddPlayer(Player player) //Room�� �÷��̾ �߰��Ǿ��� ��, UI ����
    {
        players.Add(player);
        UpdateUI();
    }

    public void UpdateRemovePlayer(Player player) //Room�� �÷��̾ ���� �������� �� UI ����
    {
        for (int idx = 0; idx < players.Count; idx++)
        {
            if (players[idx].PlayerId == player.PlayerId)
            {
                players.RemoveAt(idx);
                break;
            }
        }
        UpdateUI();
    }
}
