using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    class Listener
    {
        Socket listenSocket;
        Action<Socket> onAcceptHandler;

        public void Init(IPEndPoint endPoint, Action<Socket> onAcceptHandler)
        {
            this.onAcceptHandler += onAcceptHandler;

            listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            listenSocket.Bind(endPoint); //엔드포인트에 소켓 연동

            listenSocket.Listen(10); //최대 10개까지 요청 대기

            for (int i = 0; i < 10; i++)
            {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.Completed += OnAcceptCompleted; //접속 완료 이벤트에 이벤트 핸들러 추가
                RegisterAccept(args);
            }
        }

        private void RegisterAccept(SocketAsyncEventArgs args) //처음 클라이언트의 접속 요청 비동기로 대기
        {
            args.AcceptSocket = null;

            bool pending = listenSocket.AcceptAsync(args); //메인 스레드 논블록. 다른 스레드에서 클라이언트 접속 대기하다 접속하면 정보를 args에 저장, Completed 이벤트
            if (!pending) OnAcceptCompleted(null, args);
        }

        private void OnAcceptCompleted(object? sender, SocketAsyncEventArgs args) //실제로 연결 받음 -> 요청한 클라이언트용 소켓 리턴하는 콜백 함수
        {
            if(args.SocketError == SocketError.Success)
            {
                //성공하면 클라이언트 소켓 리턴하고 수행할 함수를...
                onAcceptHandler?.Invoke(args.AcceptSocket);
            }

            RegisterAccept(args); //그리고 다시 대기...
        }
    }
}
