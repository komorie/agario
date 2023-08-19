using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public abstract class PacketSession : Session
    {
        public static readonly int HeaderSize = 2;

        // [size (2)] [packet (2)] ...
        public override sealed int OnRecv(ArraySegment<byte> buffer) //sealed: 오버라이드 막기, 패킷 분석
        {
            int processLen = 0;
            int packetCount = 0;    


            //기본적으로 TCP는 패킷이 나뉘어져 올 위험이 있음. 그러나 순서는 보장되므로 합칠 수 있다.
            while (true)
            {
                if (buffer.Count < HeaderSize) break; //ushort


                ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset); //첫 ushort인 size 가져옴
                if (buffer.Count < dataSize) break; //버퍼 크기가 size에 못미치면 break. 패킷의 이후 부분이 계속 전송되면 언젠가는 완전체가 되어 이 부분을 넘어갈 거임

                
                OnRecvPacket(new ArraySegment<byte>(buffer.Array, buffer.Offset, dataSize)); //사이즈만큼의 부분을 조립하기 위해...

                processLen += dataSize;
                packetCount++;

                buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize); //다음 패킷을 가리키는 부분 배열로 위 과정을 반복한다
                //일단 클라에게서 온 패킷들을 전부 다 처리하기 위함. 언젠가 break가 될 때까지
            }

            if (packetCount > 1)
                Console.WriteLine($"받은 패킷들을 분리해서 처리함. 패킷 개수: {packetCount}");

            return processLen;
        }

        public abstract void OnRecvPacket(ArraySegment<byte> buffer);
    }

    public abstract class Session //소켓을 통한 데이터 송수신 담당(클라이언트와 서버 양 측 다 세션을 사용하여 통신). 상대 측의 대리인
    {
        Socket sessionSocket;

        int disconnected = 0;
        Queue<ArraySegment<byte>> sendQueue = new Queue<ArraySegment<byte>>();

        RecvBuffer recvBuffer = new RecvBuffer(65535);

        List<ArraySegment<byte>> pendingList = new List<ArraySegment<byte>>(); //대기중인 바이트 배열의 '배열의 부분' 구조체
        SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs sendArgs = new SocketAsyncEventArgs();

        object _lock = new object();

        //실제로 세션을 이용한 통신에 따라 실행되는, 상속해서 구현할 이벤트 함수들
        public abstract void OnConnected(EndPoint endPoint); 
        public abstract int OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endPoint);   

        private void Clear()
        {
            sendQueue.Clear();  
            pendingList.Clear();
        }

        public void Start(Socket socket)
        {
            sessionSocket = socket;

            recvArgs.Completed += OnRecvCompleted;
            recvArgs.SetBuffer(new byte[1024], 0, 1024); //데이터 받을 때 담길 바이트 배열 세트

            sendArgs.Completed += OnSendCompleted;

            RegisterRecv();
        }

        public void Disconnect()
        {
            if(Interlocked.Exchange(ref disconnected, 1) == 1) //Test-And-Set으로 동시에 연결 해제하지 않는가 확인
            {
                return;
            }

            OnDisconnected(sessionSocket.RemoteEndPoint);   

            sessionSocket.Shutdown(SocketShutdown.Both);
            sessionSocket.Close();

            Clear();
        }

        public void Send(ArraySegment<byte> sendBuff) 
        {
            lock(_lock) //Send를 여러 쓰레드에서 호출할 수 있으니 일단 락
            {
                sendQueue.Enqueue(sendBuff); //보낼 버프를 큐에 넣는다.
                if (pendingList.Count == 0) RegisterSend(); //다른 스레드에서 Args로 Send 시도중이면 그냥 종료

                //이렇게 큐와 count 체크를 하는 이유는, args를 하나만 쓰고 args에 보낼 버퍼를 지정하는 식으로 하면(하나만 쓰는 건 효율성을 위함),
                //멀티스레드 환경에서 Send를 여러 스레드에서 할 때,
                //args의 전송 작업이 끝나기 전에 다른 쓰레드로 전환되어 버퍼를 다른 Send의 내용물로 바꿔버리는 경우가 존재할 수 있음...
            }
        }

        public void Send(List<ArraySegment<byte>> sendBuffList)
        {
            if (sendBuffList.Count == 0)
            {
                return;
            }

            lock (_lock) //자료구조에 접근한다면 락
            {
                foreach (ArraySegment<byte> sendBuff in sendBuffList)
                {
                    sendQueue.Enqueue(sendBuff);    //보내기 큐에 넣기
                }

                if(pendingList.Count == 0) 
                    RegisterSend(); //다른 스레드에서 Args로 Send 시도중이면 그냥 종료
            }
        }

        #region 네트워크 통신


        private void RegisterSend()
        {
            if(disconnected == 1) return; //연결 해제된 경우

            while (sendQueue.Count > 0) //현재 클라이언트에게 보내기로 요청된 바이트 배열 전부 꺼내서, socket에 리스트로 추가
            {
                ArraySegment<byte> sendBuff = sendQueue.Dequeue(); //여기서, 다른 여러 스레드의 TLS로 생성된 CurrentBuffer -> SendBuffer에 접근할 수 있지만...
                pendingList.Add(sendBuff); //sendbuffer에서 읽어오기만 하는 것이므로 레이스 컨디션 걱정은 없음
            }

            sendArgs.BufferList = pendingList; //그 사이에 요청된 바이트 배열들을 모아서 SendAsync 한번에 보낼 수 있음 (횟수 최적화)

            try
            {
                bool pending = sessionSocket.SendAsync(sendArgs); //비동기 전송
                if (!pending) OnSendCompleted(null, sendArgs);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Register Failed! {e}");
                throw;
            }
        }

        private void OnSendCompleted(object? sender, SocketAsyncEventArgs args)
        {
            lock (_lock)
            {
                if(args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        sendArgs.BufferList = null;
                        pendingList.Clear();

                        OnSend(sendArgs.BytesTransferred);

                        if(sendQueue.Count > 0) //전송 작업을 하는 사이에, 다른 스레드에서 큐에 보낼 버퍼를 등록했다.
                        {
                            RegisterSend(); //그럼 반복해서 다시 큐에서 빼기 후 전송
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        throw;
                    }
                }
                else
                {
                    Disconnect();
                }
            }
        }

        private void RegisterRecv()
        {
            recvBuffer.Clean();

            ArraySegment<byte> segment = recvBuffer.WriteSegment; //남은 공간만큼 가져오기
            recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

            //ArraySegment는 기본적으로 구조체긴 한데, 안에는 배열에 대한 참조를 갖고 있음.
            //즉 복사한다고 해도 배열의 참조값이 복사되는 거지 배열을 통째로 복사하는 건 아니며
            //setbuffer로 recvbuffer의 arraysegment를 지역 참조변수로 지정해도, 원본이 지정이 되는 것임.

            try
            {
                bool pending = sessionSocket.ReceiveAsync(recvArgs); //패킷 받으면 해당되는 정보를 args에 저장, Completed 실행
                if (!pending) OnRecvCompleted(null, recvArgs);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Register Failed! {e}");
                throw;
            }
        }

        private void OnRecvCompleted(object? sender, SocketAsyncEventArgs args)
        {
            if(args.BytesTransferred > 0 && args.SocketError == SocketError.Success && args.Buffer != null)
            {
                try
                {
                    if(recvBuffer.OnWrite(args.BytesTransferred) == false) //남은범위보다 큰게 들어왓
                    {
                        Disconnect();
                        return;
                    }

                    //컨텐츠 쪽으로 데이터 넘겨주기 -> ReadSegment 만큼
                    int processLen = OnRecv(recvBuffer.ReadSegment);
                    if(processLen < 0 || processLen > recvBuffer.DataSize) //받은 패킷 크기가 쌓여있는 크기보다 크거나 0이면 문제 있는거
                    {
                        Disconnect();
                        return;
                    }

                    if (recvBuffer.OnRead(processLen) == false) //읽기가 된 건데... Read 커서 이동에 문제 있으면
                    {
                        Disconnect();
                        return;
                    }

                    RegisterRecv();
                }
                catch(Exception e) 
                {
                    Console.WriteLine($"RecvCompleted Fail: {e}");
                }
            }
            else
            {
                Disconnect();
            }
        }
        #endregion
    }
}
