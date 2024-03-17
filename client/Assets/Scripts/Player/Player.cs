using UnityEngine;

public class Player : MonoBehaviour
{
    protected Mover mover;
    protected Eater eater;
    protected PacketSender packetSender;
    protected PacketReceiver packetReceiver;

    public BeamAttack beamAttack { get; private set; }
    public Stealth stealth { get; private set; }

    protected Vector2 inputVector = Vector2.zero;
    protected Vector2 dir = Vector2.right;

    public int PlayerId { get; set; }
    public float Radius { get; set; } = 1.5f;

    protected virtual void Awake()
    {
        mover = GetComponent<Mover>();
        eater = GetComponent<Eater>();
        beamAttack = GetComponent<BeamAttack>();    
        stealth = GetComponent<Stealth>();  
        packetSender = GetComponent<PacketSender>();
        packetReceiver = PacketReceiver.Instance;
    }
}
