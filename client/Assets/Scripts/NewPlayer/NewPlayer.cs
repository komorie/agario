using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static S_RoomList;

public class NewPlayer : MonoBehaviour
{
    public int PlayerId { get; set; }

    public Mover PlayerMover { get; set; }
    public Eater PlayerEater { get; set; }
    public PlayerPacketSender PacketSender { get; set; }

    private void Awake()
    {
        PlayerMover = GetComponent<Mover>();
        PlayerEater = GetComponent<Eater>();    
        PacketSender = GetComponent<PlayerPacketSender>();    
    }
}
