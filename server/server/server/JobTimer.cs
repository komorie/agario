using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    struct JobTimerElem : IComparable<JobTimerElem>
    {
        public int execTick; // 실행될 시간
        public Action action; //실행될 작업

        public int CompareTo(JobTimerElem other) //남은 실행주기를 비교해 작은 쪽부터 꺼내도록, 우선순위큐에서 비교하기위해 구현해야함.
        {
            return this.execTick - other.execTick;
        }
    }

    public class JobTimer
    {
        PriorityQueue<JobTimerElem, JobTimerElem> pq = new PriorityQueue<JobTimerElem, JobTimerElem>(); 
        object _lock = new object();    

        public static JobTimer Instance { get; } = new JobTimer(); //싱글톤

        public void Push(Action action, int tickAfter = 0)
        {
            JobTimerElem job;
            job.execTick = System.Environment.TickCount + tickAfter; //현재 시간 + 얼마 후에 실행될지 = 실행될 시간
            job.action = action;

            lock (_lock)
            {
                pq.Enqueue(job, job); //우선순위 큐에 넣기   
            }
        }   

        public void Flush() //계속 실행시간 확인하면서 큐에 있는 작업들 실행  
        {
            while (true)
            {
                int now = System.Environment.TickCount; //현재 시간
                JobTimerElem job;

                lock (_lock)
                {
                    if (pq.Count == 0) //큐가 비었으면
                        break;

                    job = pq.Peek(); //다음으로 실행할 작업 가져오고(우선순위 큐이므로 제일 가까운 실행시간을 가진 작업을 가져옴)
                    if (job.execTick > now) //아직 실행할 시간을 안 지난 경우
                        break;

                    pq.Dequeue(); //실행할 시간이 되었으면 큐에서 제거
                }

                job.action.Invoke(); //실행
            }   
        }
    }
}
