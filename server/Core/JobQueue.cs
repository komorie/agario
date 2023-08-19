using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{

    public interface IJobQueue
    {
        void Push(Action job);
    }

    public class JobQueue 
        //멀티 쓰레드(비동기)로 패킷을 받아, 각각의 스레드가 핸들러를 수행할 때 Lock이 있는 구간에 진입하면 한 스레드만이 수행할 수 있기에 작업이 정체된다.
        //이러다 클라 수가 늘어날수록 사용 중인 스레드 증가 -> 스레드풀에서 스레드를 많이 가져오게 되고, 결국 새롭게 스레드를 생성하게 되는데 비효율적인 과정이다.
        //이를 해결하기 위해 핸들러에서는 특정 클래스에서 IJobQueue를 구현해, 해당 스레드에 수행할 작업(Action 형태)를 Push로 집어넣고 종료한다. (Lock이 필요한 작업이라도 일단 집어넣고 종료하면 되니 스레드가 빨리 종료 -> 재사용이 원활)
        //또한 네트워크 
        //해당 클래스의 JobQueue에서 job들을 꺼내와 수행하는 작업은, flush 변수를 통해 한 스레드만 진입하도록 제어
    {
        Queue<Action> jobQueue = new Queue<Action>();   
        object _lock = new object();
        bool _flush = false;

        public Action Pop()
        {
            lock (_lock) //가져오는 동안에는 접근 막기
            {
                //count check
                if (jobQueue.Count == 0)
                {
                    _flush = false;
                    return null;
                }    

                return jobQueue.Dequeue();
            }
        }

        public void Push(Action job) //일감 넣기
        {
            bool flush = false;

            lock (_lock) //넣는 동안에는 접근 막기
            {
                jobQueue.Enqueue(job);
                if(_flush == false)
                {
                    flush = _flush = true;
                }
            }
            if (flush) Flush();
        }

        private void Flush() //잡큐에서 일감 전부 수행
        {
            while (true)
            {
                Action action = Pop(); //일감 가져와서
                if (action == null)
                    return;

                action.Invoke(); //수행한다(이떄 액션이 락이 필요하든 하지 않든, Push에서 처리를 해두었기 때문에 한 스레드만 action을 호출한다)

            }
        }
    }
}
