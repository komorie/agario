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

//����, �÷��̾� ���븦 ����ϴ� ���� ������Ʈ
public class OldEater : MonoBehaviour
{
    public float Radius { get; set; } = 1.5f;
    public event Action<EatFoodEventArgs> EatFoodEvent;
    public event Action<EatPlayerEventArgs> EatPlayerEvent;

    protected virtual void OnEatFood(int playerId, int foodId, S_BroadcastEatFood p = null) => EatFoodEvent?.Invoke(new EatFoodEventArgs(playerId, foodId, p)); //������ ���� �� �߻�
    protected virtual void OnEatPlayer(int playerId, int preyId, S_BroadcastEatPlayer p = null) => EatPlayerEvent?.Invoke(new EatPlayerEventArgs(playerId, preyId, p)); //�÷��̾ ���� �� �߻�
}
