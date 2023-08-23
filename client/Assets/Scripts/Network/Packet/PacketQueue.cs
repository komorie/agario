using System.Collections.Generic;

public class PacketQueue //��Ŷ �ڵ鷯�� �����ϴ� �۾��� ��������� ����Ƽ ���� ������ ���ٺҰ�(���� ������ �ϳ��� �����)
                         //��� �۾��ڵ��� �ϰ��� ����ְ� ���� �����忡�� ������ �����ϴ� ť �ʿ�
{
    public static PacketQueue Instance { get; } = new PacketQueue();

    Queue<IPacket> packetQueue = new Queue<IPacket>();
    object _lock = new object();    

    public void Push(IPacket packet) //�߰�/���Žÿ� ���� �ɾ�� ���̽� ����� �߻� ����
    {
        lock (_lock)
        {
            packetQueue.Enqueue(packet);
        }
    }   

    public IPacket Pop()
    {
        lock (_lock)
        {
            if (packetQueue.Count == 0)
                return null;

            return packetQueue.Dequeue();
        }
    }   

    public List<IPacket> PopAll() //���� ���ؼ� ����Ʈ ����(Pop�� 1�����Ӹ��� 1���ۿ� ������ �� ������)
    {
        lock (_lock)
        {
            List<IPacket> list = new List<IPacket>();
            while (packetQueue.Count > 0)
            {
                list.Add(packetQueue.Dequeue());
            }
            return list;
        }
    }   
}
