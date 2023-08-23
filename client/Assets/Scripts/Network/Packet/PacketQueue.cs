using System.Collections.Generic;

public class PacketQueue //패킷 핸들러를 수행하는 작업자 스레드들은 유니티 게임 루프에 접근불가(메인 스레드 하나만 실행됨)
                         //고로 작업자들이 일감을 집어넣고 메인 스레드에서 가져가 수행하는 큐 필요
{
    public static PacketQueue Instance { get; } = new PacketQueue();

    Queue<IPacket> packetQueue = new Queue<IPacket>();
    object _lock = new object();    

    public void Push(IPacket packet) //추가/제거시에 락을 걸어야 레이스 컨디션 발생 안함
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

    public List<IPacket> PopAll() //전부 팝해서 리스트 리턴(Pop은 1프레임마다 1번밖에 수행할 수 없으니)
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
