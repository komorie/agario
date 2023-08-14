using Core;
using System.Net;
using System.Text;

namespace Server
{
    class PlayerInfoReq//플레이어 데이터 요청 패킷
    {
        public long playerId;
        public string name;

        public List<SkillInfo> skills = new List<SkillInfo>();

        public struct SkillInfo
        {
            public int id;
            public short level;
            public float duration;

            public bool Write(Span<byte> seg, ref ushort count)
            {
                bool success = true;

                success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), id); //샌드 버퍼의 부분에 값 작성
                count += sizeof(int);

                success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), level); //샌드 버퍼의 부분에 값 작성
                count += sizeof(short);

                success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), duration); //샌드 버퍼의 부분에 값 작성
                count += sizeof(float);

                return success;

            }

            public void Read(ReadOnlySpan<byte> seg, ref ushort count)
            {
                id = BitConverter.ToInt32(seg.Slice(count, seg.Length - count));
                count += sizeof(int);
                level = BitConverter.ToInt16(seg.Slice(count, seg.Length - count));
                count += sizeof(short);
                duration = BitConverter.ToSingle(seg.Slice(count, seg.Length - count));
                count += sizeof(float);
            }
        }

        public void Read(ArraySegment<byte> segment) //버퍼로 받은 값 역직렬화해서 패킷에 넣기
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
            name = Encoding.Unicode.GetString(seg.Slice(count, nameLen));
            count += nameLen;


            //스킬 구조체 리스트를 가져오기
            ushort skillLen = BitConverter.ToUInt16(seg.Slice(count, seg.Length - count)); //사이즈
            count += sizeof(ushort);

            for (int i = 0; i < skillLen; i++)
            {
                SkillInfo skill = new SkillInfo();
                skill.Read(seg, ref count); //구조체 Read 함수
                skills.Add(skill);
            }


        }

        public ArraySegment<byte> Write() //패킷을 바이트배열로 직렬화 리턴
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

            success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Count - count), (ushort)skills.Count); //리스트를 보내기 위해, 먼저 사이즈 넣고
            count += sizeof(ushort);

            foreach (SkillInfo skill in skills)
            {
                success &= skill.Write(seg, ref count); //구조체별로 미리 만들어둔 write 함수
            }



            success &= BitConverter.TryWriteBytes(seg, count); //패킷 사이즈는 마지막에 다 계산한 후, 시작 부분에 넣는다(패킷 사이즈가 가변일 수 있으므로)

            if (!success) return null;

            return SendBufferHelper.Close(count); //버퍼에 작성된 용량 이후로 버퍼 인덱스 이동 + 실제로 작성된 버퍼의 부분만 참조
        }
    }

    class PlayerInfoOk
    {
        public int hp;
        public int attack;

        public void Read(ArraySegment<byte> seg)
        {
            throw new NotImplementedException();
        }

        public ArraySegment<byte> Write()
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

            switch ((PacketID)id) //해당 패킷의 읽기 함수 사용
            {
                case PacketID.PlayerInfoReq:
                    {
                        PlayerInfoReq pac = new PlayerInfoReq();
                        pac.Read(buffer); 
                        Console.WriteLine($"PlayerInfoReq: {pac.playerId}, {pac.name}");

                        foreach (PlayerInfoReq.SkillInfo skill in pac.skills)
                        {
                            Console.WriteLine($"Skill: {skill.id}, {skill.level}, {skill.duration}");
                        }
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
