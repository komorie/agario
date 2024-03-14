using UnityEngine;

public class Player : MonoBehaviour
{

    protected Mover mover;
    protected Eater eater;
    protected PlayerPacketSender packetSender;
    protected PacketReceiver packetReceiver;

    public int PlayerId { get; set; }
    public float Radius { get; set; } = 1.5f;

    protected virtual void Awake()
    {
        mover = GetComponent<Mover>();
        eater = GetComponent<Eater>();
        packetSender = GetComponent<PlayerPacketSender>();
        packetReceiver = PacketReceiver.Instance;
    }
}
