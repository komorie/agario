using System;
using UnityEngine;


public struct EatFoodEventArgs
{
    public int playerId;
    public int foodId;
    public S_BroadcastEatFood packet;

    public EatFoodEventArgs(int playerId, int foodId, S_BroadcastEatFood p)
    {
        this.playerId = playerId;
        this.foodId = foodId;
        this.packet = p;
    }
}

public struct EatPlayerEventArgs
{
    public int predatorId;
    public int preyId;
    public S_BroadcastEatPlayer packet;
    public EatPlayerEventArgs(int predatorId, int preyId, S_BroadcastEatPlayer p)
    {
        this.predatorId = predatorId;
        this.preyId = preyId;
        this.packet = p;
    }
}

//음식, 플레이어 섭취를 담당하는 공통 컴포넌트
public class OldEater : MonoBehaviour
{
    public float Radius { get; set; } = 1.5f;
    public event Action<EatFoodEventArgs> EatFoodEvent;
    public event Action<EatPlayerEventArgs> EatPlayerEvent;

    protected virtual void OnEatFood(int playerId, int foodId, S_BroadcastEatFood p = null) => EatFoodEvent?.Invoke(new EatFoodEventArgs(playerId, foodId, p)); //음식을 먹을 시 발생
    protected virtual void OnEatPlayer(int playerId, int preyId, S_BroadcastEatPlayer p = null) => EatPlayerEvent?.Invoke(new EatPlayerEventArgs(playerId, preyId, p)); //플레이어를 먹을 시 발생
}
