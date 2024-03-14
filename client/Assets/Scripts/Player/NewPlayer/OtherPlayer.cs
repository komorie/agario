using UnityEngine;

public class OtherPlayer : Player
{
    private NewMover mover;
    private PacketReceiver packetReceiver;

    private Vector2 inputVector = Vector2.zero;

    void Awake()
    {
        PlayerMover = GetComponent<Mover>();
        PlayerEater = GetComponent<Eater>();
        PacketSender = GetComponent<PlayerPacketSender>();

        mover = GetComponent<NewMover>();
        packetReceiver = PacketReceiver.Instance;
    }

    private void OnEnable()
    {
        packetReceiver.OnBroadcastMove += RecvBroadcastMove;
    }

    private void OnDisable()
    {
        packetReceiver.OnBroadcastMove -= RecvBroadcastMove;
    }

    // Update is called once per frame
    void Update()
    {
        mover.Move(inputVector);
    }

    private void RecvBroadcastMove(S_BroadcastMove p)
    {
        //�� ��Ŷ��� ��ġ ����
        if (PlayerId == p.playerId)
        {
            inputVector = new Vector2(p.dirX, p.dirY); //�ٸ� �÷��̾��� �̵� ����
            Vector3 lastPos = new Vector3(p.posX, p.posY, p.posZ);
            float lastTime = p.time;

            mover.StartLerp(inputVector, lastPos, lastTime);
        }
    }
}
