using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Server
{
    abstract class Packet
    {
        public ushort size; //네트워크로 보내니 최대한 사이즈 줄이기
        public ushort packetId;
        public abstract ArraySegment<byte> Write(); //버퍼에 요 패킷 데이터 작성
        public abstract void Read(ArraySegment<byte> seg); //버퍼에서 요 패킷 데이터 읽기

    }

    class PlayerInfoReq : Packet //플레이어 데이터 요청 패킷
    {
        public long playerId;
        public string name;
        public PlayerInfoReq()
        {
            packetId = (ushort)PacketID.PlayerInfoReq;
        }

        public override void Read(ArraySegment<byte> segment) //버퍼로 받은 값 역직렬화해서 패킷에 넣기
        {
            ushort count = 0;

            Span<byte> seg = new Span<byte>(segment.Array, segment.Offset, segment.Count);

            count += sizeof(ushort);
            count += sizeof(ushort); //size, packetId가 있는 부분

            playerId = BitConverter.ToInt64(seg.Slice(count, seg.Length - count));
            count += sizeof(long);

            //string 가져오려면, 먼저 사이즈 가져오기
            ushort nameLen = BitConverter.ToUInt16(seg.Slice(count, seg.Length - count));
            count += sizeof(ushort);

            //가져온 사이즈만큼, 문자열 가져오기
            name = Encoding.Unicode.GetString(seg.Slice(count, seg.Length - count));

        }
        public override ArraySegment<byte> Write() //패킷을 바이트배열로 직렬화 리턴
        {
            ArraySegment<byte> seg = SendBufferHelper.Open(4096); //버퍼의 부분 원하는 사이즈 만큼 예약하기

            ushort count = 0;
            bool success = true;

            count += sizeof(ushort);

            success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Count - count), (ushort)PacketID.PlayerInfoReq); //샌드 버퍼의 부분에 값 작성
            count += sizeof(ushort);

            success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Count - count), playerId); //샌드 버퍼의 부분에 값 작성
            count += sizeof(long);

            /*            ushort nameLen = (ushort)Encoding.Unicode.GetByteCount(name);  //가변 크기인 변수는 사이즈 먼저 가져오고(이미지, 문자열 등)
                        success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Count - count), nameLen); //버퍼에 작성
                        count += sizeof(ushort); //그만큼 인덱스 뒤로 이동

                        Array.Copy(Encoding.Unicode.GetBytes(name), 0, seg.Array, count, nameLen); //문자열 바이트배열 버퍼로 복사
                        count += nameLen;*/

            ushort nameLen = (ushort)Encoding.Unicode.GetBytes(name, 0, name.Length, seg.Array, seg.Offset + count + sizeof(ushort));
            //문자열의 길이만큼, 바이트로 변환 후 seg에 작성을 한번에! 대신 nameLen을 앞에 작성해야 하므로 위치는 ushort 크기만큼 뒤로 보냄

            success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Count - count), nameLen); //문자열의 크기 seg에 작성
            count += sizeof(ushort);
            count += nameLen;

            success &= BitConverter.TryWriteBytes(seg, count); //패킷 사이즈는 마지막에 다 계산한 후, 시작 부분에 넣는다(패킷 사이즈가 가변일 수 있으므로)

            if (!success) return null;

            return SendBufferHelper.Close(count); //버퍼에 작성된 용량 이후로 버퍼 인덱스 이동 + 실제로 작성된 버퍼의 부분만 참조
        }
    }
    class PlayerInfoOk : Packet
    {
        public int hp;
        public int attack;

        public override void Read(ArraySegment<byte> seg)
        {
            throw new NotImplementedException();
        }

        public override ArraySegment<byte> Write()
        {
            throw new NotImplementedException();
        }
    }

    public enum PacketID
    {
        PlayerInfoReq = 1,
        PlayerInfoOk = 2,
    }
    class ClientSession : PacketSession //서버 쪽의 통신용 소켓 들고있음 + 통신 시 이벤트에 따른 핸들러 함수들 구현
    {
        public override void OnConnected(EndPoint endPoint)
        {

            Console.WriteLine($"OnConnected: {endPoint}");
            Thread.Sleep(1000);
            Disconnect();

        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine("Disconnect");
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            ushort count = 0;

            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;
            Console.WriteLine($"PacketSize: {size}, PacketId: {id}");

            switch ((PacketID)id) //패킷 종류에 따라
            {
                case PacketID.PlayerInfoReq:
                    {
                        PlayerInfoReq pac = new PlayerInfoReq();
                        pac.Read(buffer);
                        Console.WriteLine($"PlayerInfoReq: {pac.playerId}, {pac.name}");
                        break;
                    }

                case PacketID.PlayerInfoOk:
                    break;
                default:
                    break;
            }

        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred Bytes:{numOfBytes}");
        }
    }
}
