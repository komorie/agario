using System;
using UnityEngine;


//음식, 플레이어 섭취를 담당하는 공통 컴포넌트
public class Eater : MonoBehaviour
{
    public float Radius { get; set; } = 1.5f;

    public event Action<int> OnEatFood;
    public event Action<int> OnEatPrey;

    public int EatFoodId { get; set; } = -1;
    public int EatPreyId { get; set; } = -1;
}
