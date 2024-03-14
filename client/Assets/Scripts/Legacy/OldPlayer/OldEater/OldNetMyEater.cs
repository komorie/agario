using UnityEngine;

//��Ʈ��ũ �÷��� �� �ڽ� �÷��̾��� ����, �÷��̾� ���븦 ����ϴ� ������Ʈ
public class OldNetMyEater : OldEater
{
    Food eatenFood;
    OldPlayer myPlayer; 
    OldPlayer eatenPlayer;
    private PlayerPacketSender packetSender;
    private PacketReceiver packetReceiver;

    private void Awake()
    {
        packetReceiver = PacketReceiver.Instance;
        packetSender = GetComponent<PlayerPacketSender>();
        myPlayer = GetComponent<OldPlayer>();
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

    protected void OnTriggerEnter(Collider other)
    {
        // �浹�� ��ü�� 'Food'
        if (other.TryGetComponent(out eatenFood) == true)
        {
            packetSender.SendEatPacket(eatenFood.FoodId);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (eatenPlayer != null) return;

        if (other.TryGetComponent(out eatenPlayer) == true) //��� �÷��̾�� �Ÿ� ������ ���ƴ��� Ȯ��
        {
            if (eatenPlayer.PlayerEater.Radius < Radius && Vector3.Distance(eatenPlayer.transform.position, transform.position) < Radius)
            {
                packetSender.SendEatPlayerPacket(myPlayer.PlayerId, eatenPlayer.PlayerId);
            }
            else
            {
                eatenPlayer = null;
            }
        }
    }

    private void RecvEatFood(S_BroadcastEatFood p)
    {
        if(eatenFood != null && p.playerId == myPlayer.PlayerId)
        {
            Radius += 0.05f;
            transform.localScale = new Vector3(Radius * 2, Radius * 2, Radius * 2);
            OnEatFood(myPlayer.PlayerId, eatenFood.FoodId, p);
            eatenFood = null;
        }
    }
    private void RecvEatPlayer(S_BroadcastEatPlayer p)
    {
        if (eatenPlayer != null && p.predatorId == myPlayer.PlayerId)
        {
            Radius += (eatenPlayer.PlayerEater.Radius / 2); //���� �÷��̾� ũ�� �ݸ�ŭ ���� �÷��̾� ũ�� ����
            transform.localScale = new Vector3(Radius * 2, Radius * 2, Radius * 2);
            OnEatPlayer(myPlayer.PlayerId, eatenPlayer.PlayerId, p);
            Destroy(eatenPlayer.gameObject);
            eatenPlayer = null;
        }
    }
}
