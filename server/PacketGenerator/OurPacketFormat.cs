using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacketGenerator
{
    internal class OurPacketFormat //XML에서 패킷의 데이터를 가져와 문자열로 만든 패킷 포맷의 빈칸을 치환하여 패킷 클래스를 생성
    {
        //0 패킷 구별할 이름/번호 목록 (enum)
        //1 패킷 목록
        public static string fileFormat =
@"using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using Core;

public enum PacketID
{{
	{0}
}}

{1}
";
        //0 패킷 이름
        //1 패킷 번호
        public static string packetEnumFormat = 
@"{0} = {1},";


        // {0}: 패킷 이름
        // {1}: 멤버 변수
        // {2}: 멤버 변수 Read
        // {3}: 멤버 변수 Write

        public static string packetFormat =
@"
class {0}
{{
    {1}
    public void Read(ArraySegment<byte> segment) //버퍼로 받은 값 역직렬화해서 패킷에 넣기
    {{
        ushort count = 0;

        Span<byte> seg = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);
        count += sizeof(ushort); //size, packetId가 있는 부분
        {2}

    }}

    public ArraySegment<byte> Write() //패킷을 바이트배열로 직렬화 리턴
    {{
        ArraySegment<byte> segment = SendBufferHelper.Open(4096); //버퍼의 부분 원하는 사이즈 만큼 예약하기

        ushort count = 0;
        bool success = true;

        Span<byte> seg = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);

        success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), (ushort)PacketID.{0}); //샌드 버퍼의 부분에 값 작성
        count += sizeof(ushort);          
        {3}
        success &= BitConverter.TryWriteBytes(seg, count); //패킷 사이즈는 마지막에 다 계산한 후, 시작 부분에 넣는다(패킷 사이즈가 가변일 수 있으므로)

        if (!success) return null;

        return SendBufferHelper.Close(count); //버퍼에 작성된 용량 이후로 버퍼 인덱스 이동 + 실제로 작성된 버퍼의 부분만 참조
    }}
}}
";

        // 0 변수 형식
        // 1 변수 이름
        public static string memberFormat = @"public {0} {1};";

        // {0} 리스트 이름 [대문자]
        // {1} 리스트 이름 [소문자]
        // {2} 멤버 변수들
        // {3} 멤버 변수 Read
        // {4} 멤버 변수 Write
        public static string memberListFormat =
@"

public List<{0}> {1}s = new List<{0}>();

public class {0} //멤버 클래스
{{
    {2}

    public bool Write(Span<byte> seg, ref ushort count) //구조체 Write
    {{
        bool success = true;
        {4}
        return success;
    }}

    public void Read(ReadOnlySpan<byte> seg, ref ushort count) //구조체 Read
    {{
        {3}
    }}
}}
";

        // 0: 변수 이름
        // 1: To 뭐시기인지
        // 2: 변수 형식
        public static string readFormat =
@"
this.{0} = BitConverter.{1}(seg.Slice(count, seg.Length - count));
count += sizeof({2});
";

        // {0} 변수 이름
        // {1} 변수 형식
        public static string readByteFormat =
@"
this.{0} = ({1})segment.Array[segment.Offset + count];
count += sizeof({1});
";

        //0 변수 이름
        public static string readStringFormat =
@"
//string 가져오려면, 먼저 사이즈 가져오기
ushort {0}Len = BitConverter.ToUInt16(seg.Slice(count, seg.Length - count));
count += sizeof(ushort);

//가져온 사이즈만큼, 문자열 가져오기
this.{0} = Encoding.Unicode.GetString(seg.Slice(count, {0}Len));
count += {0}Len;
";

        //0 변수 이름
        //1 변수 형식
        public static string writeFormat =
@"
success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), this.{0}); //샌드 버퍼의 부분에 값 작성
count += sizeof({1});
";

        // {0} 변수 이름
        // {1} 변수 형식
        public static string writeByteFormat =
@"
segment.Array[segment.Offset + count] = (byte)this.{0};
count += sizeof({1});
";

        //0 변수 이름
        public static string writeStringFormat =
@"
ushort {0}Len = (ushort)Encoding.Unicode.GetBytes(this.{0}, 0, this.{0}.Length, segment.Array, segment.Offset + count + sizeof(ushort));          
//문자열의 길이만큼, 바이트로 변환 후 seg에 작성을 한번에! 대신 nameLen을 앞에 작성해야 하므로 위치는 ushort 크기만큼 뒤로 보냄
success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), {0}Len); //문자열의 크기 seg에 작성
count += sizeof(ushort);
count += {0}Len;
";

        // {0} 리스트 이름 [대문자] (구조체 이름)
        // {1} 리스트 이름 [소문자]
        public static string readListFormat =
@"
this.{1}s.Clear();
ushort {1}Len = BitConverter.ToUInt16(seg.Slice(count, seg.Length - count)); //리스트의 사이즈 가져요기
count += sizeof(ushort);
for (int i = 0; i < {1}Len; i++)
{{
	{0} {1} = new {0}();
	{1}.Read(seg, ref count); //구조체 Read 함수
	{1}s.Add({1});
}}";

        // {0} 리스트 이름 [대문자]
        // {1} 리스트 이름 [소문자]
        public static string writeListFormat =
@"
success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), (ushort)this.{1}s.Count); //리스트를 보내기 위해, 먼저 사이즈 넣고
count += sizeof(ushort);
foreach ({0} {1} in this.{1}s)
{{
    success &= {1}.Write(seg, ref count); //구조체별로 미리 만들어둔 write 함수
}}
";

    }
}
