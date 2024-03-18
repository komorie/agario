using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacketGenerator
{
    internal class OurPacketFormat //XML에서 패킷의 데이터를 가져와 문자열로 만든 패킷 포맷의 빈칸을 치환하여 패킷 클래스를 생성
    {
        // 0: Register가 들어감
        public static string managerFormat =
@"
using Core;
using System;
using System.Collections.Generic;

class PacketManager //인터페이스와 딕셔너리로 패킷을 생성하는 과정을 추상화해서 자동적으로 수행(OnRecvPacket만 호출해 주면 됨)
{{
    #region Singleton   
    static PacketManager _instance = new PacketManager();

    public static PacketManager Instance {{ get {{ return _instance; }} }}
    #endregion

    PacketManager()
    {{
        Register();
    }}  

    //바이트 배열로부터 패킷을 생성할 때, 종류 Enum에 따라 수행할 생성 함수를 구분하는 딕셔너리
    Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>> makeFunc = new Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>>();

    //생성된 패킷의 종류에 따라 수행될 이벤트 핸들러 함수를 종류 Enum으로 구분
    Dictionary<ushort, Action<PacketSession, IPacket>> handler = new Dictionary<ushort, Action<PacketSession, IPacket>>();  
        
    public void Register() //패킷의 종류에 따라 딕셔너리에 Action을 등록하는 함수   
    {{
        {0}
    }}

    private T MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new() //바이트 배열에서 ID에 따른 패킷 생성
    {{
        T p = new T();
        p.Read(buffer);
        return p;
    }}

    public void HandlerPacket(PacketSession session, IPacket packet) //패킷 생성 이후, 이벤트 핸들러 수행
    {{
        Action<PacketSession, IPacket> action = null;
        if (handler.TryGetValue(packet.Protocol, out action))
            action.Invoke(session, packet);
    }}

    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer, Action<PacketSession, IPacket> callback = null) 
        //어떤 세션에서, 받은 바이트 배열이 어떤 패킷인지 판단 후, 생성 함수 호출
    {{
        ushort count = 0;

        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        Func<PacketSession, ArraySegment<byte>, IPacket> func = null;    
        if (makeFunc.TryGetValue(id, out func)) //받은 패킷의 ID에 따라, 해당하는 패킷 생성 함수를 가져오고
        {{
            IPacket packet = func.Invoke(session, buffer); //있으면 수행
            
            if(callback != null) //따로 패킷을 가지고 수행할 콜백함수를 넣어줬으면 그거 실행하고(유니티에서는 패킷 만들기만 하고 패킷 큐에 넣어주면 되니까 이걸로)
                callback.Invoke(session, packet);
            else
                HandlerPacket(session, packet); //없으면 정해진 이벤트 핸들러 수행    
        }}
    }}
}}
";
        public static string managerRegisterFormat =
@"
        makeFunc.Add((ushort)PacketID.{0}, MakePacket<{0}>); //패킷에 종류에 따라, 바이트 배열로부터 패킷을 생성할 때 수행될 함수를 등록
        handler.Add((ushort)PacketID.{0}, PacketHandler.{0}Handler); //이쪽은 해당 패킷 생성 이후에 수행되어야 할 이벤트 핸들러 함수를 등록
";

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

public interface IPacket
{{
	ushort Protocol {{ get; }} //ID 리턴하는 부분임
	void Read(ArraySegment<byte> segment); //버퍼로 받은 값 역직렬화해서 패킷에 넣기
	ArraySegment<byte> Write(); //패킷을 바이트배열로 직렬화 리턴
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
public class {0} : IPacket
{{
    {1}

    public ushort Protocol {{ get {{ return (ushort)PacketID.{0}; }} }}    

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

public class {0} //멤버 클래스Lis
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
