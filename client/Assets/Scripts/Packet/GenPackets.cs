using System;
using System.Collections.Generic;
using System.Text;
using Core;

public enum PacketID
{
	C_PlayerInfoReq = 1,
	S_PlayerInfoRes = 2,
	C_Chat = 3,
	S_Chat = 4,
	
}

public interface IPacket
{
	ushort Protocol { get; } //ID 리턴하는 부분임
	void Read(ArraySegment<byte> segment); //버퍼로 받은 값 역직렬화해서 패킷에 넣기
	ArraySegment<byte> Write(); //패킷을 바이트배열로 직렬화 리턴
}


class C_PlayerInfoReq : IPacket
{
    public long playerId;
	public byte testByte;
	public string name;
	
	
	public List<Skill> skills = new List<Skill>();
	
	public class Skill //멤버 클래스
	{
	    public int id;
		public short level;
		public float duration;
		
		
		public List<Attribute> attributes = new List<Attribute>();
		
		public class Attribute //멤버 클래스
		{
		    public int att;
		
		    public bool Write(Span<byte> seg, ref ushort count) //구조체 Write
		    {
		        bool success = true;
		        
				success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), this.att); //샌드 버퍼의 부분에 값 작성
				count += sizeof(int);
				
		        return success;
		    }
		
		    public void Read(ReadOnlySpan<byte> seg, ref ushort count) //구조체 Read
		    {
		        
				this.att = BitConverter.ToInt32(seg.Slice(count, seg.Length - count));
				count += sizeof(int);
				
		    }
		}
		
	
	    public bool Write(Span<byte> seg, ref ushort count) //구조체 Write
	    {
	        bool success = true;
	        
			success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), this.id); //샌드 버퍼의 부분에 값 작성
			count += sizeof(int);
			
			
			success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), this.level); //샌드 버퍼의 부분에 값 작성
			count += sizeof(short);
			
			
			success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), this.duration); //샌드 버퍼의 부분에 값 작성
			count += sizeof(float);
			
			
			success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), (ushort)this.attributes.Count); //리스트를 보내기 위해, 먼저 사이즈 넣고
			count += sizeof(ushort);
			foreach (Attribute attribute in this.attributes)
			{
			    success &= attribute.Write(seg, ref count); //구조체별로 미리 만들어둔 write 함수
			}
			
	        return success;
	    }
	
	    public void Read(ReadOnlySpan<byte> seg, ref ushort count) //구조체 Read
	    {
	        
			this.id = BitConverter.ToInt32(seg.Slice(count, seg.Length - count));
			count += sizeof(int);
			
			
			this.level = BitConverter.ToInt16(seg.Slice(count, seg.Length - count));
			count += sizeof(short);
			
			
			this.duration = BitConverter.ToSingle(seg.Slice(count, seg.Length - count));
			count += sizeof(float);
			
			
			this.attributes.Clear();
			ushort attributeLen = BitConverter.ToUInt16(seg.Slice(count, seg.Length - count)); //리스트의 사이즈 가져요기
			count += sizeof(ushort);
			for (int i = 0; i < attributeLen; i++)
			{
				Attribute attribute = new Attribute();
				attribute.Read(seg, ref count); //구조체 Read 함수
				attributes.Add(attribute);
			}
	    }
	}
	

    public ushort Protocol { get { return (ushort)PacketID.C_PlayerInfoReq; } }    

    public void Read(ArraySegment<byte> segment) //버퍼로 받은 값 역직렬화해서 패킷에 넣기
    {
        ushort count = 0;

        Span<byte> seg = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);
        count += sizeof(ushort); //size, packetId가 있는 부분
        
		this.playerId = BitConverter.ToInt64(seg.Slice(count, seg.Length - count));
		count += sizeof(long);
		
		
		this.testByte = (byte)segment.Array[segment.Offset + count];
		count += sizeof(byte);
		
		
		//string 가져오려면, 먼저 사이즈 가져오기
		ushort nameLen = BitConverter.ToUInt16(seg.Slice(count, seg.Length - count));
		count += sizeof(ushort);
		
		//가져온 사이즈만큼, 문자열 가져오기
		this.name = Encoding.Unicode.GetString(seg.Slice(count, nameLen));
		count += nameLen;
		
		
		this.skills.Clear();
		ushort skillLen = BitConverter.ToUInt16(seg.Slice(count, seg.Length - count)); //리스트의 사이즈 가져요기
		count += sizeof(ushort);
		for (int i = 0; i < skillLen; i++)
		{
			Skill skill = new Skill();
			skill.Read(seg, ref count); //구조체 Read 함수
			skills.Add(skill);
		}

    }

    public ArraySegment<byte> Write() //패킷을 바이트배열로 직렬화 리턴
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096); //버퍼의 부분 원하는 사이즈 만큼 예약하기

        ushort count = 0;
        bool success = true;

        Span<byte> seg = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);

        success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), (ushort)PacketID.C_PlayerInfoReq); //샌드 버퍼의 부분에 값 작성
        count += sizeof(ushort);          
        
		success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), this.playerId); //샌드 버퍼의 부분에 값 작성
		count += sizeof(long);
		
		
		segment.Array[segment.Offset + count] = (byte)this.testByte;
		count += sizeof(byte);
		
		
		ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, segment.Array, segment.Offset + count + sizeof(ushort));          
		//문자열의 길이만큼, 바이트로 변환 후 seg에 작성을 한번에! 대신 nameLen을 앞에 작성해야 하므로 위치는 ushort 크기만큼 뒤로 보냄
		success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), nameLen); //문자열의 크기 seg에 작성
		count += sizeof(ushort);
		count += nameLen;
		
		
		success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), (ushort)this.skills.Count); //리스트를 보내기 위해, 먼저 사이즈 넣고
		count += sizeof(ushort);
		foreach (Skill skill in this.skills)
		{
		    success &= skill.Write(seg, ref count); //구조체별로 미리 만들어둔 write 함수
		}
		
        success &= BitConverter.TryWriteBytes(seg, count); //패킷 사이즈는 마지막에 다 계산한 후, 시작 부분에 넣는다(패킷 사이즈가 가변일 수 있으므로)

        if (!success) return null;

        return SendBufferHelper.Close(count); //버퍼에 작성된 용량 이후로 버퍼 인덱스 이동 + 실제로 작성된 버퍼의 부분만 참조
    }
}

class S_PlayerInfoRes : IPacket
{
    public long playerId;
	public byte testByte;
	public string name;
	
	
	public List<Skill> skills = new List<Skill>();
	
	public class Skill //멤버 클래스
	{
	    public int id;
		public short level;
		public float duration;
	
	    public bool Write(Span<byte> seg, ref ushort count) //구조체 Write
	    {
	        bool success = true;
	        
			success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), this.id); //샌드 버퍼의 부분에 값 작성
			count += sizeof(int);
			
			
			success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), this.level); //샌드 버퍼의 부분에 값 작성
			count += sizeof(short);
			
			
			success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), this.duration); //샌드 버퍼의 부분에 값 작성
			count += sizeof(float);
			
	        return success;
	    }
	
	    public void Read(ReadOnlySpan<byte> seg, ref ushort count) //구조체 Read
	    {
	        
			this.id = BitConverter.ToInt32(seg.Slice(count, seg.Length - count));
			count += sizeof(int);
			
			
			this.level = BitConverter.ToInt16(seg.Slice(count, seg.Length - count));
			count += sizeof(short);
			
			
			this.duration = BitConverter.ToSingle(seg.Slice(count, seg.Length - count));
			count += sizeof(float);
			
	    }
	}
	

    public ushort Protocol { get { return (ushort)PacketID.S_PlayerInfoRes; } }    

    public void Read(ArraySegment<byte> segment) //버퍼로 받은 값 역직렬화해서 패킷에 넣기
    {
        ushort count = 0;

        Span<byte> seg = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);
        count += sizeof(ushort); //size, packetId가 있는 부분
        
		this.playerId = BitConverter.ToInt64(seg.Slice(count, seg.Length - count));
		count += sizeof(long);
		
		
		this.testByte = (byte)segment.Array[segment.Offset + count];
		count += sizeof(byte);
		
		
		//string 가져오려면, 먼저 사이즈 가져오기
		ushort nameLen = BitConverter.ToUInt16(seg.Slice(count, seg.Length - count));
		count += sizeof(ushort);
		
		//가져온 사이즈만큼, 문자열 가져오기
		this.name = Encoding.Unicode.GetString(seg.Slice(count, nameLen));
		count += nameLen;
		
		
		this.skills.Clear();
		ushort skillLen = BitConverter.ToUInt16(seg.Slice(count, seg.Length - count)); //리스트의 사이즈 가져요기
		count += sizeof(ushort);
		for (int i = 0; i < skillLen; i++)
		{
			Skill skill = new Skill();
			skill.Read(seg, ref count); //구조체 Read 함수
			skills.Add(skill);
		}

    }

    public ArraySegment<byte> Write() //패킷을 바이트배열로 직렬화 리턴
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096); //버퍼의 부분 원하는 사이즈 만큼 예약하기

        ushort count = 0;
        bool success = true;

        Span<byte> seg = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);

        success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), (ushort)PacketID.S_PlayerInfoRes); //샌드 버퍼의 부분에 값 작성
        count += sizeof(ushort);          
        
		success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), this.playerId); //샌드 버퍼의 부분에 값 작성
		count += sizeof(long);
		
		
		segment.Array[segment.Offset + count] = (byte)this.testByte;
		count += sizeof(byte);
		
		
		ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, segment.Array, segment.Offset + count + sizeof(ushort));          
		//문자열의 길이만큼, 바이트로 변환 후 seg에 작성을 한번에! 대신 nameLen을 앞에 작성해야 하므로 위치는 ushort 크기만큼 뒤로 보냄
		success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), nameLen); //문자열의 크기 seg에 작성
		count += sizeof(ushort);
		count += nameLen;
		
		
		success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), (ushort)this.skills.Count); //리스트를 보내기 위해, 먼저 사이즈 넣고
		count += sizeof(ushort);
		foreach (Skill skill in this.skills)
		{
		    success &= skill.Write(seg, ref count); //구조체별로 미리 만들어둔 write 함수
		}
		
        success &= BitConverter.TryWriteBytes(seg, count); //패킷 사이즈는 마지막에 다 계산한 후, 시작 부분에 넣는다(패킷 사이즈가 가변일 수 있으므로)

        if (!success) return null;

        return SendBufferHelper.Close(count); //버퍼에 작성된 용량 이후로 버퍼 인덱스 이동 + 실제로 작성된 버퍼의 부분만 참조
    }
}

class C_Chat : IPacket
{
    public string chat;

    public ushort Protocol { get { return (ushort)PacketID.C_Chat; } }    

    public void Read(ArraySegment<byte> segment) //버퍼로 받은 값 역직렬화해서 패킷에 넣기
    {
        ushort count = 0;

        Span<byte> seg = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);
        count += sizeof(ushort); //size, packetId가 있는 부분
        
		//string 가져오려면, 먼저 사이즈 가져오기
		ushort chatLen = BitConverter.ToUInt16(seg.Slice(count, seg.Length - count));
		count += sizeof(ushort);
		
		//가져온 사이즈만큼, 문자열 가져오기
		this.chat = Encoding.Unicode.GetString(seg.Slice(count, chatLen));
		count += chatLen;
		

    }

    public ArraySegment<byte> Write() //패킷을 바이트배열로 직렬화 리턴
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096); //버퍼의 부분 원하는 사이즈 만큼 예약하기

        ushort count = 0;
        bool success = true;

        Span<byte> seg = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);

        success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), (ushort)PacketID.C_Chat); //샌드 버퍼의 부분에 값 작성
        count += sizeof(ushort);          
        
		ushort chatLen = (ushort)Encoding.Unicode.GetBytes(this.chat, 0, this.chat.Length, segment.Array, segment.Offset + count + sizeof(ushort));          
		//문자열의 길이만큼, 바이트로 변환 후 seg에 작성을 한번에! 대신 nameLen을 앞에 작성해야 하므로 위치는 ushort 크기만큼 뒤로 보냄
		success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), chatLen); //문자열의 크기 seg에 작성
		count += sizeof(ushort);
		count += chatLen;
		
        success &= BitConverter.TryWriteBytes(seg, count); //패킷 사이즈는 마지막에 다 계산한 후, 시작 부분에 넣는다(패킷 사이즈가 가변일 수 있으므로)

        if (!success) return null;

        return SendBufferHelper.Close(count); //버퍼에 작성된 용량 이후로 버퍼 인덱스 이동 + 실제로 작성된 버퍼의 부분만 참조
    }
}

class S_Chat : IPacket
{
    public int playerId;
	public string chat;

    public ushort Protocol { get { return (ushort)PacketID.S_Chat; } }    

    public void Read(ArraySegment<byte> segment) //버퍼로 받은 값 역직렬화해서 패킷에 넣기
    {
        ushort count = 0;

        Span<byte> seg = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);
        count += sizeof(ushort); //size, packetId가 있는 부분
        
		this.playerId = BitConverter.ToInt32(seg.Slice(count, seg.Length - count));
		count += sizeof(int);
		
		
		//string 가져오려면, 먼저 사이즈 가져오기
		ushort chatLen = BitConverter.ToUInt16(seg.Slice(count, seg.Length - count));
		count += sizeof(ushort);
		
		//가져온 사이즈만큼, 문자열 가져오기
		this.chat = Encoding.Unicode.GetString(seg.Slice(count, chatLen));
		count += chatLen;
		

    }

    public ArraySegment<byte> Write() //패킷을 바이트배열로 직렬화 리턴
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096); //버퍼의 부분 원하는 사이즈 만큼 예약하기

        ushort count = 0;
        bool success = true;

        Span<byte> seg = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);

        success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), (ushort)PacketID.S_Chat); //샌드 버퍼의 부분에 값 작성
        count += sizeof(ushort);          
        
		success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), this.playerId); //샌드 버퍼의 부분에 값 작성
		count += sizeof(int);
		
		
		ushort chatLen = (ushort)Encoding.Unicode.GetBytes(this.chat, 0, this.chat.Length, segment.Array, segment.Offset + count + sizeof(ushort));          
		//문자열의 길이만큼, 바이트로 변환 후 seg에 작성을 한번에! 대신 nameLen을 앞에 작성해야 하므로 위치는 ushort 크기만큼 뒤로 보냄
		success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), chatLen); //문자열의 크기 seg에 작성
		count += sizeof(ushort);
		count += chatLen;
		
        success &= BitConverter.TryWriteBytes(seg, count); //패킷 사이즈는 마지막에 다 계산한 후, 시작 부분에 넣는다(패킷 사이즈가 가변일 수 있으므로)

        if (!success) return null;

        return SendBufferHelper.Close(count); //버퍼에 작성된 용량 이후로 버퍼 인덱스 이동 + 실제로 작성된 버퍼의 부분만 참조
    }
}

