using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Server
{

    class Packet
    {
        public ushort size; //네트워크로 보내니 최대한 사이즈 줄이기
        public ushort packetId;

    }

    class LoginOkPacket : Packet
    {

    }

    class GameSession : PacketSession //서버 쪽의 통신용 소켓 들고있음 + 통신 시 이벤트에 따른 핸들러 함수들 구현
    {
        public override void OnConnected(EndPoint endPoint)
        {

            Console.WriteLine($"OnConnected: {endPoint}");


            Packet packet = new Packet() { size = 4, packetId = 70 };

            ArraySegment<byte> openSegment = SendBufferHelper.Open(4096); //버퍼의 부분 원하는 사이즈 만큼 예약하기
            byte[] buffer1 = BitConverter.GetBytes(packet.size);
            byte[] buffer2 = BitConverter.GetBytes(packet.packetId);
            Array.Copy(buffer1, 0, openSegment.Array, openSegment.Offset, buffer1.Length);
            Array.Copy(buffer2, 0, openSegment.Array, openSegment.Offset + buffer1.Length, buffer2.Length); //패킷 바이트로 바꾸고, 센드 버퍼에 작성
            ArraySegment<byte> sendBuff = SendBufferHelper.Close(packet.size); //버퍼에 작성된 용량 이후로 버퍼 인덱스 이동 + 실제로 작성된 버퍼의 부분만 참조
            Send(sendBuff); //패킷이 작성된 버퍼의 부분 보내기

            Thread.Sleep(1000);
            Disconnect();

        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine("Disconnect");
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + 2);
            Console.WriteLine($"PacketSize: {size}, PacketId: {id}"); 

        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred Bytes:{numOfBytes}");
        }
    }

    internal class Server
    {
        static void Main(string[] args)
        {

            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress iPAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(iPAddr, 777); //현재 호스트의 IP주소와 포트 번호 가져오기

            Listener listener = new Listener(); //연결 대기

            listener.Init(endPoint, () => { return new GameSession(); }); //연결 시 GameSession 생성

            while (true)
            {

            }

        }
    }
}
