using System;
using System.Diagnostics;
using UnityEngine;

public class CustomStopwatch : Stopwatch //시작 시간을 정할 수 있는 스톱워치 클래스
{

    public TimeSpan StartOffset { get; private set; }

    public void SetStartOffSet(float elapsedSeconds)
    {
        StartOffset = TimeSpan.FromSeconds(elapsedSeconds);
    }

    public float ElapsedSeconds
    {
        get
        {
            return (float)(base.Elapsed.TotalSeconds + StartOffset.TotalSeconds);
        }
    }
}

public class CustomTimer : GOSingleton<CustomTimer>
{
    public CustomStopwatch StopWatch { get; set; } = new CustomStopwatch();
    PacketReceiver packetReceiver;

    private void Awake()
    {
        packetReceiver = PacketReceiver.Instance;   
    }

    private void OnEnable() //특정 패킷 받을 시 수행할 함수들 구독
    {
        packetReceiver.OnBroadcastServerTime += RecvServerTime;
    }

    private void OnDisable()
    {
        packetReceiver.OnBroadcastServerTime -= RecvServerTime;
    }

    private void RecvServerTime(S_BroadcastServerTime time) //서버 시간과 동기화
    {
        StopWatch.Reset();
        StopWatch.SetStartOffSet(time.serverTime);
        StopWatch.Start();
    }
}
