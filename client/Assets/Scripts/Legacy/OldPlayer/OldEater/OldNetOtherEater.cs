using UnityEngine;


//��Ʈ��ũ �÷��� �� Ÿ �÷��̾��� ����, �÷��̾� ���븦 ����ϴ� ������Ʈ
public class NetOtherEater : OldEater
{
    private OldRoom room;
    private OldPlayer myPlayer;
    private PacketReceiver packetReceiver;

    private void Awake()
    {
        room = OldRoom.Instance;
        myPlayer = GetComponent<OldPlayer>();
        packetReceiver = PacketReceiver.Instance;
    }

    private void OnEnable()
    {
        packetReceiver.OnBroadcastEatFood += RecvEatFood; //���� �Ծ��ٴ� ��Ŷ ���� �� ����
        packetReceiver.OnBroadcastEatPlayer += RecvEatPlayer; //�÷��̾� �Ծ��ٴ� ��Ŷ ���� �� ����
    }

    private void OnDisable()
    {
        packetReceiver.OnBroadcastEatFood -= RecvEatFood;
        packetReceiver.OnBroadcastEatPlayer -= RecvEatPlayer;
    }

    public void RecvEatFood(S_BroadcastEatFood p) //���� ���� ó��
    {
        if(p.playerId == myPlayer.PlayerId)
        {
            Radius += 0.05f;
            transform.localScale = new Vector3(Radius * 2, Radius * 2, Radius * 2);
            OnEatFood(myPlayer.PlayerId, p.foodId, p);
        }
    }

    public void RecvEatPlayer(S_BroadcastEatPlayer p) //�÷��̾� ���� ó��
    {
        OldPlayer prey;
        if (p.predatorId == myPlayer.PlayerId)
        {
            if (room.Players.TryGetValue(p.preyId, out prey))
            { 
                Radius += (prey.PlayerEater.Radius / 2); //���� �÷��̾� ũ�� �ݸ�ŭ ���� �÷��̾� ũ�� ����
                transform.localScale = new Vector3(Radius * 2, Radius * 2, Radius * 2);
                OnEatPlayer(myPlayer.PlayerId, p.preyId, p);
                Destroy(prey.gameObject); //���� �÷��̾� ������Ʈ ����
            }
        }
    }
}
