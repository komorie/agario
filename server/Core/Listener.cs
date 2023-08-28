using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class Listener //클라이언트의 연결 요청을 비동기 대기하고, 클라언트와 연결 및 통신용 소켓(을 가지는 세션) 생성
    {
        Socket listenSocket;
        Func<Session> sessionFactory; //생성할 세션 종류를 받음

        public void Init(IPEndPoint endPoint, Func<Session> sessionFactory, int register = 10, int backlog = 100)
        {
            this.sessionFactory += sessionFactory;

            listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            listenSocket.Bind(endPoint); //서버의 엔드포인트에 소켓 연동

            listenSocket.Listen(backlog); //최대 몇개까지 요청 대기하고 그이상 차면 컷

            for (int i = 0; i < register; i++) //register 수만큼 만들어서 접속 대기 (동시다발적인 연결 요청을 전부 처리)     
            {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.UserToken = i;
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
            if (args.SocketError == SocketError.Success)
            {
                Session session = sessionFactory.Invoke();
                
                session.Start(args.AcceptSocket); //해당 클라이언트와의 통신 소켓으로 세션 시작
                 session.OnConnected(args.AcceptSocket.RemoteEndPoint);
            }

            RegisterAccept(args); //그리고 다시 대기...
        }
    }
}
