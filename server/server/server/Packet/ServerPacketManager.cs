
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
    Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>> makeFunc = new Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>>();

    //생성된 패킷의 종류에 따라 수행될 이벤트 핸들러 함수를 종류 Enum으로 구분
    Dictionary<ushort, Action<PacketSession, IPacket>> handler = new Dictionary<ushort, Action<PacketSession, IPacket>>();  
        
    public void Register() //패킷의 종류에 따라 딕셔너리에 Action을 등록하는 함수   
    {
        
        makeFunc.Add((ushort)PacketID.C_LeaveGame, MakePacket<C_LeaveGame>); //패킷에 종류에 따라, 바이트 배열로부터 패킷을 생성할 때 수행될 함수를 등록
        handler.Add((ushort)PacketID.C_LeaveGame, PacketHandler.C_LeaveGameHandler); //이쪽은 해당 패킷 생성 이후에 수행되어야 할 이벤트 핸들러 함수를 등록

		
        makeFunc.Add((ushort)PacketID.C_Move, MakePacket<C_Move>); //패킷에 종류에 따라, 바이트 배열로부터 패킷을 생성할 때 수행될 함수를 등록
        handler.Add((ushort)PacketID.C_Move, PacketHandler.C_MoveHandler); //이쪽은 해당 패킷 생성 이후에 수행되어야 할 이벤트 핸들러 함수를 등록

		
        makeFunc.Add((ushort)PacketID.C_EatFood, MakePacket<C_EatFood>); //패킷에 종류에 따라, 바이트 배열로부터 패킷을 생성할 때 수행될 함수를 등록
        handler.Add((ushort)PacketID.C_EatFood, PacketHandler.C_EatFoodHandler); //이쪽은 해당 패킷 생성 이후에 수행되어야 할 이벤트 핸들러 함수를 등록

		
        makeFunc.Add((ushort)PacketID.C_EatPlayer, MakePacket<C_EatPlayer>); //패킷에 종류에 따라, 바이트 배열로부터 패킷을 생성할 때 수행될 함수를 등록
        handler.Add((ushort)PacketID.C_EatPlayer, PacketHandler.C_EatPlayerHandler); //이쪽은 해당 패킷 생성 이후에 수행되어야 할 이벤트 핸들러 함수를 등록

		
        makeFunc.Add((ushort)PacketID.C_BeamStart, MakePacket<C_BeamStart>); //패킷에 종류에 따라, 바이트 배열로부터 패킷을 생성할 때 수행될 함수를 등록
        handler.Add((ushort)PacketID.C_BeamStart, PacketHandler.C_BeamStartHandler); //이쪽은 해당 패킷 생성 이후에 수행되어야 할 이벤트 핸들러 함수를 등록

		
        makeFunc.Add((ushort)PacketID.C_BeamHit, MakePacket<C_BeamHit>); //패킷에 종류에 따라, 바이트 배열로부터 패킷을 생성할 때 수행될 함수를 등록
        handler.Add((ushort)PacketID.C_BeamHit, PacketHandler.C_BeamHitHandler); //이쪽은 해당 패킷 생성 이후에 수행되어야 할 이벤트 핸들러 함수를 등록

		
        makeFunc.Add((ushort)PacketID.C_Stealth, MakePacket<C_Stealth>); //패킷에 종류에 따라, 바이트 배열로부터 패킷을 생성할 때 수행될 함수를 등록
        handler.Add((ushort)PacketID.C_Stealth, PacketHandler.C_StealthHandler); //이쪽은 해당 패킷 생성 이후에 수행되어야 할 이벤트 핸들러 함수를 등록

		
    }

    private T MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new() //바이트 배열에서 ID에 따른 패킷 생성
    {
        T p = new T();
        p.Read(buffer);
        return p;
    }

    public void HandlerPacket(PacketSession session, IPacket packet) //패킷 생성 이후, 이벤트 핸들러 수행
    {
        Action<PacketSession, IPacket> action = null;
        if (handler.TryGetValue(packet.Protocol, out action))
            action.Invoke(session, packet);
    }

    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer, Action<PacketSession, IPacket> callback = null) 
        //어떤 세션에서, 받은 바이트 배열이 어떤 패킷인지 판단 후, 생성 함수 호출
    {
        ushort count = 0;

        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        Func<PacketSession, ArraySegment<byte>, IPacket> func = null;    
        if (makeFunc.TryGetValue(id, out func)) //받은 패킷의 ID에 따라, 해당하는 패킷 생성 함수를 가져오고
        {
            IPacket packet = func.Invoke(session, buffer); //있으면 수행
            
            if(callback != null) //따로 패킷을 가지고 수행할 콜백함수를 넣어줬으면 그거 실행하고(유니티에서는 패킷 만들기만 하고 패킷 큐에 넣어주면 되니까 이걸로)
                callback.Invoke(session, packet);
            else
                HandlerPacket(session, packet); //없으면 정해진 이벤트 핸들러 수행    
        }
    }
}
