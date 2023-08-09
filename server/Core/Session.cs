using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    class Session
    {
        Socket sessionSocket;
        int disconnected = 0;

        public void Start(Socket socket)
        {
            sessionSocket = socket;

            SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
            recvArgs.Completed += OnRecvCompleted;
            recvArgs.SetBuffer(new byte[1024], 0, 1024); //데이터 받을 때 담길 바이트 배열 세트


            RegisterRecv(recvArgs);
        }

        public void Disconnect()
        {
            if(Interlocked.Exchange(ref disconnected, 1) == 1) //Test-And-Set으로 동시에 연결 해제하지 않는가 확인
            {
                return;
            }
            sessionSocket.Shutdown(SocketShutdown.Both);
            sessionSocket.Close();
        }

        #region 네트워크 통신

        public void Send(byte[] sendBuff)
        {
            sessionSocket.Send(sendBuff);   
        }

        private void RegisterRecv(SocketAsyncEventArgs args)
        {

            bool pending = sessionSocket.ReceiveAsync(args); //패킷 받으면 해당되는 정보를 args에 저장, Completed 실행
            if (!pending) OnRecvCompleted(null, args);
        }

        private void OnRecvCompleted(object? sender, SocketAsyncEventArgs args)
        {
            if(args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    string recvData = Encoding.UTF8.GetString(args.Buffer, args.Offset, args.BytesTransferred);

                    Console.WriteLine($"From Client: {recvData}");
                    RegisterRecv(args);
                }
                catch(Exception e) 
                {
                    Console.WriteLine($"RecvCompleted Fail: {e}");
                }
            }
            else
            {

            }
        }
        #endregion
    }
}
