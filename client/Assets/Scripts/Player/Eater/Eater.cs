using System;
using UnityEngine;


public struct EatFoodEventArgs
{
    public int foodId;
    public S_BroadcastEatFood packet;

    public EatFoodEventArgs(int foodId, S_BroadcastEatFood p)
    {
        this.foodId = foodId;
        this.packet = p;
    }
}

public struct EatPlayerEventArgs
{
    public int preyId;
    public S_BroadcastEatPlayer packet;
    public EatPlayerEventArgs(int preyId, S_BroadcastEatPlayer p)
    {
        this.preyId = preyId;
        this.packet = p;
    }
}

//����, �÷��̾� ���븦 ����ϴ� ���� ������Ʈ
public class Eater : MonoBehaviour
{
    public float Radius { get; set; } = 1.5f;
    public event Action<EatFoodEventArgs> EatFood;
    public event Action<EatPlayerEventArgs> EatPlayer;

    protected virtual void OnEatFood(int foodId, S_BroadcastEatFood p = null) => EatFood?.Invoke(new EatFoodEventArgs(foodId, p)); //������ ���� �� �߻�
    protected virtual void OnEatPlayer(int preyId, S_BroadcastEatPlayer p = null) => EatPlayer?.Invoke(new EatPlayerEventArgs(preyId, p)); //�÷��̾ ���� �� �߻�
}
