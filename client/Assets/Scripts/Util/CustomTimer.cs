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

}
