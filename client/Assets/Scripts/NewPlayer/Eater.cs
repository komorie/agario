using System;
using UnityEngine;


//����, �÷��̾� ���븦 ����ϴ� ���� ������Ʈ
public class Eater : MonoBehaviour
{
    public float Radius { get; set; } = 1.5f;

    public event Action<int> OnEatFood;
    public event Action<int> OnEatPrey;

    public int EatFoodId { get; set; } = -1;
    public int EatPreyId { get; set; } = -1;
}
