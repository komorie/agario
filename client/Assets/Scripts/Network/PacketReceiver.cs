using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacketReceiver : GOSingleton<PacketReceiver> //유니티에서 특정 패킷 받을 시 수행할 함수들 여기 구독
{

    public event Action<S_BroadcastEnterGame> OnBroadcastEnterGame;
    public event Action<S_BroadcastLeaveGame> OnBroadcastLeaveGame;
    public event Action<S_RoomList> OnRoomList;
    public event Action<S_BroadcastMove> OnBroadcastMove;
    public event Action<S_BroadcastEatFood> OnBroadcastEatFood;
    public event Action<S_BroadcastEatPlayer> OnBroadcastEatPlayer;

    public void RecvBroadcastEnterGame (S_BroadcastEnterGame packet) => OnBroadcastEnterGame?.Invoke(packet);
    public void RecvBroadcastLeaveGame (S_BroadcastLeaveGame packet) => OnBroadcastLeaveGame?.Invoke(packet);
    public void RecvRoomList (S_RoomList packet) => OnRoomList?.Invoke(packet);
    public void RecvBroadcastMove (S_BroadcastMove packet) => OnBroadcastMove?.Invoke(packet);
    public void RecvBroadcastEatFood (S_BroadcastEatFood packet) => OnBroadcastEatFood?.Invoke(packet);
    public void RecvBroadcastEatPlayer (S_BroadcastEatPlayer packet) => OnBroadcastEatPlayer?.Invoke(packet);


}
