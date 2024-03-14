using UnityEngine;

public class Eater : MonoBehaviour
{

    private Food food;
    private Player predator;
    private Player prey;
    private Room room;

    private void Awake()
    {
        room = Room.Instance;
        predator = GetComponent<Player>();  
    }

    public bool TryEatFood(Food food) //먹기 시도(중복 방지로 한번만 true 리턴)
    {
        if(this.food == null)
        {
            this.food = food;
            if (!GameScene.IsMulti) EatFoodComplete();
            return true;
        }
        return false;
    }

    public bool TryEatPlayer(Player prey) //먹기 시도(중복 방지로 한번만 true 리턴)
    {
        if (this.prey == null && prey.Radius < predator.Radius && Vector3.Distance(prey.transform.position, predator.transform.position) < predator.Radius)
        {
            this.prey = prey;
            if (!GameScene.IsMulti) EatPlayerComplete();
            return true;
        }
        return false;
    }

    public void EatFoodComplete(S_BroadcastEatFood p = null) //먹기 처리
    {
        if(p != null) food = room.Foods[p.foodId];
        predator.Radius += 0.1f;
        transform.localScale = new Vector3(predator.Radius * 2, predator.Radius * 2, predator.Radius * 2);
        room.PlayerEatFood(food, p);
        food = null;
    }

    public void EatPlayerComplete(S_BroadcastEatPlayer p = null) //먹기 처리
    {
        if (p != null) prey = room.Players[p.preyId];
        predator.Radius += (prey.Radius / 2);
        transform.localScale = new Vector3(predator.Radius * 2, predator.Radius * 2, predator.Radius * 2);
        room.PlayerEatPlayer(prey, p);
        Destroy(prey.gameObject);
        prey = null;    
    }
}
