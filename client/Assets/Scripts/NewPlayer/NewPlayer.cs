using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewPlayer : MonoBehaviour
{
    public int PlayerId { get; set; }

    public Mover PlayerMover { get; set; }
    public Eater PlayerEater { get; set; }
    public PlayerPacketSender PlayerPacketSender { get; set; }

    private void Awake()
    {
        PlayerMover = GetComponent<Mover>();
        PlayerEater = GetComponent<Eater>();    
        PlayerPacketSender = GetComponent<PlayerPacketSender>();    
    }
}
