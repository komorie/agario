using UnityEngine;


//��Ʈ��ũ�� ���� �÷��̾��� ����, �÷��̾� ���븦 ����ϴ� ������Ʈ
public class NetworkEater : Eater
{
    private NewRoom room;
    private NewPlayer player;
    private PacketReceiver packetReceiver;

    private void Awake()
    {
        room = NewRoom.Instance;
        player = GetComponent<NewPlayer>();
        packetReceiver = PacketReceiver.Instance;
    }

    private void OnEnable()
    {
        packetReceiver.OnBroadcastEatFood += RecvEatFood;
        packetReceiver.OnBroadcastEatPlayer += RecvEatPlayer;
    }

    private void OnDisable()
    {
        packetReceiver.OnBroadcastEatFood -= RecvEatFood;
        packetReceiver.OnBroadcastEatPlayer -= RecvEatPlayer;
    }

    public void RecvEatFood(S_BroadcastEatFood p) //���� �Ծ��ٴ� ��Ŷ ����
    {
        if (p.playerId == player.PlayerId) //���� �÷��̾�� ũ�� �ø���
        {
            transform.localScale += 0.1f * Vector3.one;
            Radius = transform.localScale.x * 0.5f;
            EatFoodId = p.foodId;
        }
    }

    //��Ŷ�� ũ�� ���� �ϴ°͵�
    public void RecvEatPlayer(S_BroadcastEatPlayer p) //�÷��̾� �Ծ��ٴ� ��Ŷ ����
    {
        NewPlayer prey;

        if (p.predatorId == player.PlayerId)
        {
            if (room.Players.TryGetValue(p.preyId, out prey))
            {
                player.transform.localScale += (prey.transform.localScale / 2); //���� �÷��̾� ũ�� �ݸ�ŭ ���� �÷��̾� ũ�� ����
                Radius = transform.localScale.x * 0.5f;   //������ Ű���
                Destroy(prey.gameObject); //���� �÷��̾� ������Ʈ ����
                EatPreyId = p.preyId;
            }
        }
    }
}
