using UnityEngine;

public class Player : MonoBehaviour
{

    public Mover mover;
    public Eater eater;
    public BeamAttack beamAttack;
    public PacketSender packetSender;
    public PacketReceiver packetReceiver;

    protected Vector2 inputVector = Vector2.zero;
    protected Vector2 dir = Vector2.right;

    public int PlayerId { get; set; }
    public float Radius { get; set; } = 1.5f;

    protected virtual void Awake()
    {
        mover = GetComponent<Mover>();
        eater = GetComponent<Eater>();
        beamAttack = GetComponent<BeamAttack>();    
        packetSender = GetComponent<PacketSender>();
        packetReceiver = PacketReceiver.Instance;
    }
}
