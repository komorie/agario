using Core;
using System;
using System.Net;

public class ServerSession : PacketSession //특정 서버와의 통신용 소켓 + 통신 시 이벤트에 따른 핸들러 함수들 구현
{
    public int SessionId { get; set; }

    public override void OnConnected(EndPoint endPoint)
    {

        Console.WriteLine($"OnConnected: {endPoint}");

    }

    public override void OnDisconnected(EndPoint endPoint)
    {
        Console.WriteLine("Disconnect");
    }

    public override void OnRecvPacket(ArraySegment<byte> buffer)
    {
        PacketManager.Instance.OnRecvPacket(this, buffer);  
    }

    public override void OnSend(int numOfBytes)
    {
/*            Console.WriteLine($"Transferred Bytes in Client:{numOfBytes}");*/
    }
}

