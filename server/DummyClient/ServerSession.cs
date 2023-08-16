using Core;
using System.Net;

namespace DummyClient
{
    class ServerSession : Session //클라이언트 쪽에서 소켓 + 통신 시 이벤트에 따른 핸들러 함수들 구현
    {
        public override void OnConnected(EndPoint endPoint)
        {

            Console.WriteLine($"OnConnected: {endPoint}");

            C_PlayerInfoReq packet = new C_PlayerInfoReq() { playerId = 10222, name = "YIM JUN BEOM" };
            packet.skills.Add(new C_PlayerInfoReq.Skill() { id = 1, level = 1, duration = 3.0f });
            packet.skills[0].attributes.Add(new C_PlayerInfoReq.Skill.Attribute() { att = 1235 });
            packet.skills.Add(new C_PlayerInfoReq.Skill() { id = 2, level = 1, duration = 3.0f });
            packet.skills.Add(new C_PlayerInfoReq.Skill() { id = 3, level = 3, duration = 5.0f });
            packet.skills.Add(new C_PlayerInfoReq.Skill() { id = 4, level = 2, duration = 6.0f });


            for (int i = 0; i < 5; i++)
            {
                ArraySegment<byte> seg = packet.Write(); //send 버퍼에 패킷 작성
                
                if (seg != null) Send(seg); //패킷이 작성된 버퍼의 부분 보내기
            }
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine("Disconnect");
        }

        public override int OnRecv(ArraySegment<byte> buffer)
        {
            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + 2);
            Console.WriteLine($"From Server: {size}, {id}");
            return buffer.Count;
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred Bytes:{numOfBytes}");
        }
    }
}
