
using Core;
using System;
using System.Collections.Generic;

class PacketManager //인터페이스와 딕셔너리로 패킷을 생성하는 과정을 추상화해서 자동적으로 수행(OnRecvPacket만 호출해 주면 됨)
{
    #region Singleton   
    static PacketManager _instance = new PacketManager();

    public static PacketManager Instance { get { return _instance; } }
    #endregion

    PacketManager()
    {
        Register();
    }  

    //바이트 배열로부터 패킷을 생성할 때, 종류 Enum에 따라 수행할 생성 함수를 구분하는 딕셔너리
    Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>>();

    //생성된 패킷의 종류에 따라 수행될 이벤트 핸들러 함수를 종류 Enum으로 구분
    Dictionary<ushort, Action<PacketSession, IPacket>> handler = new Dictionary<ushort, Action<PacketSession, IPacket>>();  
        
    public void Register() //패킷의 종류에 따라 딕셔너리에 Action을 등록하는 함수   
    {
        
        onRecv.Add((ushort)PacketID.C_PlayerInfoReq, MakePacket<C_PlayerInfoReq>); //패킷에 종류에 따라, 바이트 배열로부터 패킷을 생성할 때 수행될 함수를 등록
        handler.Add((ushort)PacketID.C_PlayerInfoReq, PacketHandler.C_PlayerInfoReqHandler); //이쪽은 해당 패킷 생성 이후에 수행되어야 할 이벤트 핸들러 함수를 등록

		
        onRecv.Add((ushort)PacketID.C_Chat, MakePacket<C_Chat>); //패킷에 종류에 따라, 바이트 배열로부터 패킷을 생성할 때 수행될 함수를 등록
        handler.Add((ushort)PacketID.C_Chat, PacketHandler.C_ChatHandler); //이쪽은 해당 패킷 생성 이후에 수행되어야 할 이벤트 핸들러 함수를 등록

		
    }

    private void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new() //바이트 배열에서 패킷 생성 후, 생성 이벤트 핸들러 수행
    {
        T p = new T();
        p.Read(buffer); 

        Action<PacketSession, IPacket> action = null;
        if (handler.TryGetValue(p.Protocol, out action))
            action.Invoke(session, p);  
    }

    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer) //어떤 세션에서, 받은 바이트 배열이 어떤 패킷인지 판단 후, 생성 함수 호출
    {
        ushort count = 0;

        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        Action<PacketSession, ArraySegment<byte>> action = null;    
        if (onRecv.TryGetValue(id, out action)) //받은 패킷의 종류에 따라, 해당하는 패킷 생성 함수를 수행
            action.Invoke(session, buffer);
    }
}
