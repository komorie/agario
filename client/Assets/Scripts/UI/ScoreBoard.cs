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

    public void Init(Dictionary<int, Player> players, int myPlayerId) //초기화(Room이 초기화된 이후)
    {
        this.players = new List<Player>(players.Values);
        this.myPlayerId = myPlayerId;
        UpdateUI();
    }

    private void UpdateUI() //UI 갱신
    {
        players.Sort((x, y) => y.Radius.CompareTo(x.Radius)); //점수 기준으로 정렬한 랭킹 배열

        for (int i = 0; i < 5; i++) //상위 5위권 출력
        {
            if(players.Count <= i) //나갔으면 지우기
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

    public void UpdateAddPlayer(Player player) //Room에 플레이어가 추가되었을 때, UI 갱신
    {
        players.Add(player);
        UpdateUI();
    }

    public void UpdateRemovePlayer(Player player) //Room에 플레이어가 접속 해제했을 때 UI 갱신
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
