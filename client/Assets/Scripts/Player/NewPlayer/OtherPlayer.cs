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
        //온 패킷대로 위치 조정
        if (PlayerId == p.playerId)
        {
            inputVector = new Vector2(p.dirX, p.dirY); //다른 플레이어의 이동 방향
            Vector3 lastPos = new Vector3(p.posX, p.posY, p.posZ);
            float lastTime = p.time;

            mover.StartLerp(inputVector, lastPos, lastTime);
        }
    }
}
