using System;

public class PacketReceiver : GOSingleton<PacketReceiver> //유니티에서 특정 패킷 받을 시 수행할 함수들 여기 구독
{

    public event Action<S_BroadcastEnterGame> OnBroadcastEnterGame;
    public event Action<S_BroadcastLeaveGame> OnBroadcastLeaveGame;
    public event Action<S_RoomList> OnRoomList;
    public event Action<S_BroadcastMove> OnBroadcastMove;
    public event Action<S_BroadcastEatFood> OnBroadcastEatFood;
    public event Action<S_BroadcastEatPlayer> OnBroadcastEatPlayer;
    public event Action<S_BroadcastServerTime> OnBroadcastServerTime;
    public event Action<S_BroadcastBeamStart> OnBroadcastBeamStart; 
    public event Action<S_BroadcastBeamHit> OnBroadcastBeamHit; 


    public void RecvBroadcastEnterGame (S_BroadcastEnterGame packet) => OnBroadcastEnterGame?.Invoke(packet);
    public void RecvBroadcastLeaveGame (S_BroadcastLeaveGame packet) => OnBroadcastLeaveGame?.Invoke(packet);
    public void RecvRoomList (S_RoomList packet) => OnRoomList?.Invoke(packet);
    public void RecvBroadcastMove (S_BroadcastMove packet) => OnBroadcastMove?.Invoke(packet);
    public void RecvBroadcastEatFood (S_BroadcastEatFood packet) => OnBroadcastEatFood?.Invoke(packet);
    public void RecvBroadcastEatPlayer (S_BroadcastEatPlayer packet) => OnBroadcastEatPlayer?.Invoke(packet);
    public void RecvBroadcastServerTime(S_BroadcastServerTime packet) => OnBroadcastServerTime?.Invoke(packet);
    public void RecvBroadcastBeamStart(S_BroadcastBeamStart packet) => OnBroadcastBeamStart?.Invoke(packet);    
    public void RecvBroadcastBeamHit(S_BroadcastBeamHit packet) => OnBroadcastBeamHit?.Invoke(packet);  

}
