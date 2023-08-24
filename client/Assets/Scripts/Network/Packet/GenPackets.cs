using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using Core;

public enum PacketID
{
	S_BroadcastEnterGame = 1,
	C_LeaveGame = 2,
	S_BroadcastLeaveGame = 3,
	S_PlayerList = 4,
	C_Move = 5,
	S_BroadcastMove = 6,
	
}

public interface IPacket
{
	ushort Protocol { get; } //ID 리턴하는 부분임
	void Read(ArraySegment<byte> segment); //버퍼로 받은 값 역직렬화해서 패킷에 넣기
	ArraySegment<byte> Write(); //패킷을 바이트배열로 직렬화 리턴
}


public class S_BroadcastEnterGame : IPacket
{
    public int playerId;
	public float posX;
	public float posY;
	public float posZ;

    public ushort Protocol { get { return (ushort)PacketID.S_BroadcastEnterGame; } }    

    public void Read(ArraySegment<byte> segment) //버퍼로 받은 값 역직렬화해서 패킷에 넣기
    {
        ushort count = 0;

        Span<byte> seg = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);
        count += sizeof(ushort); //size, packetId가 있는 부분
        
		this.playerId = BitConverter.ToInt32(seg.Slice(count, seg.Length - count));
		count += sizeof(int);
		
		
		this.posX = BitConverter.ToSingle(seg.Slice(count, seg.Length - count));
		count += sizeof(float);
		
		
		this.posY = BitConverter.ToSingle(seg.Slice(count, seg.Length - count));
		count += sizeof(float);
		
		
		this.posZ = BitConverter.ToSingle(seg.Slice(count, seg.Length - count));
		count += sizeof(float);
		

    }

    public ArraySegment<byte> Write() //패킷을 바이트배열로 직렬화 리턴
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096); //버퍼의 부분 원하는 사이즈 만큼 예약하기

        ushort count = 0;
        bool success = true;

        Span<byte> seg = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);

        success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), (ushort)PacketID.S_BroadcastEnterGame); //샌드 버퍼의 부분에 값 작성
        count += sizeof(ushort);          
        
		success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), this.playerId); //샌드 버퍼의 부분에 값 작성
		count += sizeof(int);
		
		
		success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), this.posX); //샌드 버퍼의 부분에 값 작성
		count += sizeof(float);
		
		
		success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), this.posY); //샌드 버퍼의 부분에 값 작성
		count += sizeof(float);
		
		
		success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), this.posZ); //샌드 버퍼의 부분에 값 작성
		count += sizeof(float);
		
        success &= BitConverter.TryWriteBytes(seg, count); //패킷 사이즈는 마지막에 다 계산한 후, 시작 부분에 넣는다(패킷 사이즈가 가변일 수 있으므로)

        if (!success) return null;

        return SendBufferHelper.Close(count); //버퍼에 작성된 용량 이후로 버퍼 인덱스 이동 + 실제로 작성된 버퍼의 부분만 참조
    }
}

public class C_LeaveGame : IPacket
{
    

    public ushort Protocol { get { return (ushort)PacketID.C_LeaveGame; } }    

    public void Read(ArraySegment<byte> segment) //버퍼로 받은 값 역직렬화해서 패킷에 넣기
    {
        ushort count = 0;

        Span<byte> seg = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);
        count += sizeof(ushort); //size, packetId가 있는 부분
        

    }

    public ArraySegment<byte> Write() //패킷을 바이트배열로 직렬화 리턴
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096); //버퍼의 부분 원하는 사이즈 만큼 예약하기

        ushort count = 0;
        bool success = true;

        Span<byte> seg = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);

        success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), (ushort)PacketID.C_LeaveGame); //샌드 버퍼의 부분에 값 작성
        count += sizeof(ushort);          
        
        success &= BitConverter.TryWriteBytes(seg, count); //패킷 사이즈는 마지막에 다 계산한 후, 시작 부분에 넣는다(패킷 사이즈가 가변일 수 있으므로)

        if (!success) return null;

        return SendBufferHelper.Close(count); //버퍼에 작성된 용량 이후로 버퍼 인덱스 이동 + 실제로 작성된 버퍼의 부분만 참조
    }
}

public class S_BroadcastLeaveGame : IPacket
{
    public int playerId;

    public ushort Protocol { get { return (ushort)PacketID.S_BroadcastLeaveGame; } }    

    public void Read(ArraySegment<byte> segment) //버퍼로 받은 값 역직렬화해서 패킷에 넣기
    {
        ushort count = 0;

        Span<byte> seg = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);
        count += sizeof(ushort); //size, packetId가 있는 부분
        
		this.playerId = BitConverter.ToInt32(seg.Slice(count, seg.Length - count));
		count += sizeof(int);
		

    }

    public ArraySegment<byte> Write() //패킷을 바이트배열로 직렬화 리턴
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096); //버퍼의 부분 원하는 사이즈 만큼 예약하기

        ushort count = 0;
        bool success = true;

        Span<byte> seg = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);

        success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), (ushort)PacketID.S_BroadcastLeaveGame); //샌드 버퍼의 부분에 값 작성
        count += sizeof(ushort);          
        
		success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), this.playerId); //샌드 버퍼의 부분에 값 작성
		count += sizeof(int);
		
        success &= BitConverter.TryWriteBytes(seg, count); //패킷 사이즈는 마지막에 다 계산한 후, 시작 부분에 넣는다(패킷 사이즈가 가변일 수 있으므로)

        if (!success) return null;

        return SendBufferHelper.Close(count); //버퍼에 작성된 용량 이후로 버퍼 인덱스 이동 + 실제로 작성된 버퍼의 부분만 참조
    }
}

public class S_PlayerList : IPacket
{
    
	
	public List<Player> players = new List<Player>();
	
	public class Player //멤버 클래스
	{
	    public bool isSelf;
		public int playerId;
		public float dirX;
		public float dirY;
		public float posX;
		public float posY;
		public float posZ;
	
	    public bool Write(Span<byte> seg, ref ushort count) //구조체 Write
	    {
	        bool success = true;
	        
			success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), this.isSelf); //샌드 버퍼의 부분에 값 작성
			count += sizeof(bool);
			
			
			success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), this.playerId); //샌드 버퍼의 부분에 값 작성
			count += sizeof(int);
			
			
			success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), this.dirX); //샌드 버퍼의 부분에 값 작성
			count += sizeof(float);
			
			
			success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), this.dirY); //샌드 버퍼의 부분에 값 작성
			count += sizeof(float);
			
			
			success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), this.posX); //샌드 버퍼의 부분에 값 작성
			count += sizeof(float);
			
			
			success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), this.posY); //샌드 버퍼의 부분에 값 작성
			count += sizeof(float);
			
			
			success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), this.posZ); //샌드 버퍼의 부분에 값 작성
			count += sizeof(float);
			
	        return success;
	    }
	
	    public void Read(ReadOnlySpan<byte> seg, ref ushort count) //구조체 Read
	    {
	        
			this.isSelf = BitConverter.ToBoolean(seg.Slice(count, seg.Length - count));
			count += sizeof(bool);
			
			
			this.playerId = BitConverter.ToInt32(seg.Slice(count, seg.Length - count));
			count += sizeof(int);
			
			
			this.dirX = BitConverter.ToSingle(seg.Slice(count, seg.Length - count));
			count += sizeof(float);
			
			
			this.dirY = BitConverter.ToSingle(seg.Slice(count, seg.Length - count));
			count += sizeof(float);
			
			
			this.posX = BitConverter.ToSingle(seg.Slice(count, seg.Length - count));
			count += sizeof(float);
			
			
			this.posY = BitConverter.ToSingle(seg.Slice(count, seg.Length - count));
			count += sizeof(float);
			
			
			this.posZ = BitConverter.ToSingle(seg.Slice(count, seg.Length - count));
			count += sizeof(float);
			
	    }
	}
	

    public ushort Protocol { get { return (ushort)PacketID.S_PlayerList; } }    

    public void Read(ArraySegment<byte> segment) //버퍼로 받은 값 역직렬화해서 패킷에 넣기
    {
        ushort count = 0;

        Span<byte> seg = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);
        count += sizeof(ushort); //size, packetId가 있는 부분
        
		this.players.Clear();
		ushort playerLen = BitConverter.ToUInt16(seg.Slice(count, seg.Length - count)); //리스트의 사이즈 가져요기
		count += sizeof(ushort);
		for (int i = 0; i < playerLen; i++)
		{
			Player player = new Player();
			player.Read(seg, ref count); //구조체 Read 함수
			players.Add(player);
		}

    }

    public ArraySegment<byte> Write() //패킷을 바이트배열로 직렬화 리턴
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096); //버퍼의 부분 원하는 사이즈 만큼 예약하기

        ushort count = 0;
        bool success = true;

        Span<byte> seg = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);

        success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), (ushort)PacketID.S_PlayerList); //샌드 버퍼의 부분에 값 작성
        count += sizeof(ushort);          
        
		success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), (ushort)this.players.Count); //리스트를 보내기 위해, 먼저 사이즈 넣고
		count += sizeof(ushort);
		foreach (Player player in this.players)
		{
		    success &= player.Write(seg, ref count); //구조체별로 미리 만들어둔 write 함수
		}
		
        success &= BitConverter.TryWriteBytes(seg, count); //패킷 사이즈는 마지막에 다 계산한 후, 시작 부분에 넣는다(패킷 사이즈가 가변일 수 있으므로)

        if (!success) return null;

        return SendBufferHelper.Close(count); //버퍼에 작성된 용량 이후로 버퍼 인덱스 이동 + 실제로 작성된 버퍼의 부분만 참조
    }
}

public class C_Move : IPacket
{
    public float dirX;
	public float dirY;
	public float posX;
	public float posY;
	public float posZ;

    public ushort Protocol { get { return (ushort)PacketID.C_Move; } }    

    public void Read(ArraySegment<byte> segment) //버퍼로 받은 값 역직렬화해서 패킷에 넣기
    {
        ushort count = 0;

        Span<byte> seg = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);
        count += sizeof(ushort); //size, packetId가 있는 부분
        
		this.dirX = BitConverter.ToSingle(seg.Slice(count, seg.Length - count));
		count += sizeof(float);
		
		
		this.dirY = BitConverter.ToSingle(seg.Slice(count, seg.Length - count));
		count += sizeof(float);
		
		
		this.posX = BitConverter.ToSingle(seg.Slice(count, seg.Length - count));
		count += sizeof(float);
		
		
		this.posY = BitConverter.ToSingle(seg.Slice(count, seg.Length - count));
		count += sizeof(float);
		
		
		this.posZ = BitConverter.ToSingle(seg.Slice(count, seg.Length - count));
		count += sizeof(float);
		

    }

    public ArraySegment<byte> Write() //패킷을 바이트배열로 직렬화 리턴
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096); //버퍼의 부분 원하는 사이즈 만큼 예약하기

        ushort count = 0;
        bool success = true;

        Span<byte> seg = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);

        success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), (ushort)PacketID.C_Move); //샌드 버퍼의 부분에 값 작성
        count += sizeof(ushort);          
        
		success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), this.dirX); //샌드 버퍼의 부분에 값 작성
		count += sizeof(float);
		
		
		success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), this.dirY); //샌드 버퍼의 부분에 값 작성
		count += sizeof(float);
		
		
		success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), this.posX); //샌드 버퍼의 부분에 값 작성
		count += sizeof(float);
		
		
		success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), this.posY); //샌드 버퍼의 부분에 값 작성
		count += sizeof(float);
		
		
		success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), this.posZ); //샌드 버퍼의 부분에 값 작성
		count += sizeof(float);
		
        success &= BitConverter.TryWriteBytes(seg, count); //패킷 사이즈는 마지막에 다 계산한 후, 시작 부분에 넣는다(패킷 사이즈가 가변일 수 있으므로)

        if (!success) return null;

        return SendBufferHelper.Close(count); //버퍼에 작성된 용량 이후로 버퍼 인덱스 이동 + 실제로 작성된 버퍼의 부분만 참조
    }
}

public class S_BroadcastMove : IPacket
{
    public int playerId;
	public float dirX;
	public float dirY;
	public float posX;
	public float posY;
	public float posZ;

    public ushort Protocol { get { return (ushort)PacketID.S_BroadcastMove; } }    

    public void Read(ArraySegment<byte> segment) //버퍼로 받은 값 역직렬화해서 패킷에 넣기
    {
        ushort count = 0;

        Span<byte> seg = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);
        count += sizeof(ushort); //size, packetId가 있는 부분
        
		this.playerId = BitConverter.ToInt32(seg.Slice(count, seg.Length - count));
		count += sizeof(int);
		
		
		this.dirX = BitConverter.ToSingle(seg.Slice(count, seg.Length - count));
		count += sizeof(float);
		
		
		this.dirY = BitConverter.ToSingle(seg.Slice(count, seg.Length - count));
		count += sizeof(float);
		
		
		this.posX = BitConverter.ToSingle(seg.Slice(count, seg.Length - count));
		count += sizeof(float);
		
		
		this.posY = BitConverter.ToSingle(seg.Slice(count, seg.Length - count));
		count += sizeof(float);
		
		
		this.posZ = BitConverter.ToSingle(seg.Slice(count, seg.Length - count));
		count += sizeof(float);
		

    }

    public ArraySegment<byte> Write() //패킷을 바이트배열로 직렬화 리턴
    {
        ArraySegment<byte> segment = SendBufferHelper.Open(4096); //버퍼의 부분 원하는 사이즈 만큼 예약하기

        ushort count = 0;
        bool success = true;

        Span<byte> seg = new Span<byte>(segment.Array, segment.Offset, segment.Count);

        count += sizeof(ushort);

        success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), (ushort)PacketID.S_BroadcastMove); //샌드 버퍼의 부분에 값 작성
        count += sizeof(ushort);          
        
		success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), this.playerId); //샌드 버퍼의 부분에 값 작성
		count += sizeof(int);
		
		
		success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), this.dirX); //샌드 버퍼의 부분에 값 작성
		count += sizeof(float);
		
		
		success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), this.dirY); //샌드 버퍼의 부분에 값 작성
		count += sizeof(float);
		
		
		success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), this.posX); //샌드 버퍼의 부분에 값 작성
		count += sizeof(float);
		
		
		success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), this.posY); //샌드 버퍼의 부분에 값 작성
		count += sizeof(float);
		
		
		success &= BitConverter.TryWriteBytes(seg.Slice(count, seg.Length - count), this.posZ); //샌드 버퍼의 부분에 값 작성
		count += sizeof(float);
		
        success &= BitConverter.TryWriteBytes(seg, count); //패킷 사이즈는 마지막에 다 계산한 후, 시작 부분에 넣는다(패킷 사이즈가 가변일 수 있으므로)

        if (!success) return null;

        return SendBufferHelper.Close(count); //버퍼에 작성된 용량 이후로 버퍼 인덱스 이동 + 실제로 작성된 버퍼의 부분만 참조
    }
}

