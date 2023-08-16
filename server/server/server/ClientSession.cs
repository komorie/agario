using Core;
using System.Net;
using System.Text;

namespace Server
{
    class ClientSession : PacketSession //서버 쪽의 통신용 소켓 들고있음 + 통신 시 이벤트에 따른 핸들러 함수들 구현
    {
        public override void OnConnected(EndPoint endPoint)
        {

            Console.WriteLine($"OnConnected: {endPoint}");
            Thread.Sleep(1000);


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
            Console.WriteLine($"Transferred Bytes:{numOfBytes}");
        }
    }
}
