using UnityEngine;
using static S_RoomList;

public class OtherPlayer : Player
{
    private Vector2 inputVector = Vector2.zero;

    private void OnEnable()
    {
        packetReceiver.OnBroadcastMove += RecvBroadcastMove;
        packetReceiver.OnBroadcastEatFood += RecvEatFood;
        packetReceiver.OnBroadcastEatPlayer += RecvEatPlayer; 
    }

    private void OnDisable()
    {
        packetReceiver.OnBroadcastMove -= RecvBroadcastMove;
        packetReceiver.OnBroadcastEatFood -= RecvEatFood;
        packetReceiver.OnBroadcastEatPlayer -= RecvEatPlayer;
    }

    // Update is called once per frame
    void Update()
    {
        mover.Move(inputVector);
    }

    private void RecvBroadcastMove(S_BroadcastMove p)
    {
        if (PlayerId == p.playerId)
        {
            inputVector = new Vector2(p.dirX, p.dirY); //다른 플레이어의 이동 방향
            Vector3 lastPos = new Vector3(p.posX, p.posY, p.posZ);
            float lastTime = p.time;

            mover.LerpStart(inputVector, lastPos, lastTime);
        }
    }
    private void RecvEatFood(S_BroadcastEatFood p)
    {
        if(p.playerId == PlayerId)
        {
            eater.EatFoodComplete(p);
        }
    }
    private void RecvEatPlayer(S_BroadcastEatPlayer p)
    {
        if(p.predatorId == PlayerId)
        {
            eater.EatPlayerComplete(p);
        }
    }
}
