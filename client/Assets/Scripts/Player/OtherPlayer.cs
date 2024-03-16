using System;
using System.Collections.Generic;
using UnityEngine;
using static S_RoomList;

public class OtherPlayer : Player
{
    private void OnEnable()
    {
        packetReceiver.OnBroadcastMove += RecvBroadcastMove;
        packetReceiver.OnBroadcastEatFood += RecvEatFood;
        packetReceiver.OnBroadcastEatPlayer += RecvEatPlayer; 
        packetReceiver.OnBroadcastBeamStart += RecvBeamStart;   
        packetReceiver.OnBroadcastBeamHit += RecvBeamHit;

    }

    private void OnDisable()
    {
        packetReceiver.OnBroadcastMove -= RecvBroadcastMove;
        packetReceiver.OnBroadcastEatFood -= RecvEatFood;
        packetReceiver.OnBroadcastEatPlayer -= RecvEatPlayer;
        packetReceiver.OnBroadcastBeamStart -= RecvBeamStart;
        packetReceiver.OnBroadcastBeamHit -= RecvBeamHit;
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

            mover.AdjustStart(inputVector, lastPos);
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
    private void RecvBeamStart(S_BroadcastBeamStart p)
    {
        if(p.userId == PlayerId)
        {
            Vector2 dir = new Vector2(p.dirX, p.dirY);
            StartCoroutine(beamAttack.BeamCharge(dir, Radius));
        }       
    }


    private void RecvBeamHit(S_BroadcastBeamHit p)
    {
        if(p.userId == PlayerId)
        {
            List<int> playerIds = new List<int>();
            foreach (S_BroadcastBeamHit.HitPlayer player in p.hitPlayers) playerIds.Add(player.playerId);
            beamAttack.BeamHit(playerIds);
        }
    }
}
