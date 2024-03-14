using UnityEngine;

public class OldPlayer : MonoBehaviour
{
    public int PlayerId { get; set; }

    public float Radius { get; set; }

    public OldMover PlayerMover { get; set; }
    public OldEater PlayerEater { get; set; }
    public PlayerPacketSender PacketSender { get; set; }

    private void Awake()
    {
        PlayerMover = GetComponent<OldMover>();
        PlayerEater = GetComponent<OldEater>();    
        PacketSender = GetComponent<PlayerPacketSender>();    
    }
}
