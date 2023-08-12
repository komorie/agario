using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class Connecter //클라이언트 쪽에서 사용하여, 연결 요청을 비동기적으로 수행하기. 혹은 서버끼리 통신 시 필요
    {
        Func<Session> sessionFactory; //어떤 세션을 생성할지

        public void Connect(IPEndPoint endPoint, Func<Session> sessionFactory)
        {

            this.sessionFactory = sessionFactory;

            Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp); //TCP 소켓 생성

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += OnConnectColmpleted;
            args.RemoteEndPoint = endPoint;  //상대방 주소 넣고
            args.UserToken = socket; //이 소켓이 연결을 한다고.. 유저토큰으로 전달

            RegisterConnect(args);
        }

        private void RegisterConnect(SocketAsyncEventArgs args)
        {
            Socket socket = args.UserToken as Socket;

            if (socket == null) return;

            bool pending = socket.ConnectAsync(args);
            if (!pending)
            { 
                OnConnectColmpleted(null, args); 
            }
        }

        private void OnConnectColmpleted(object? sender, SocketAsyncEventArgs args)
        {
            if(args.SocketError == SocketError.Success)
            {
                Session session = sessionFactory.Invoke();
                session.Start(args.ConnectSocket); //연결 성공시 연결한 이쪽 소켓으로 상대편과의 세션 생성
                session.OnConnected(args.RemoteEndPoint);
            }
            else
            {
                Console.WriteLine($"Connect Fail: {args.SocketError}");
            }
        }
    }
}
