using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Core
{
    class Program
    {
        private static Listener listener = new Listener();

        private static void OnAcceptHandler(Socket clientSocket) //클라이언트와 연결됐을 시 실행할 콜백함수
        {
            try
            {
                Session session = new Session();
                session.Start(clientSocket);

                byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome!"); //문자열 바이트 배열로 인코딩
                session.Send(sendBuff);

                Thread.Sleep(1000);

                session.Disconnect();

            }
            catch (Exception)
            {

                throw;
            }
        }

        static void Main(string[] args)
        {
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress iPAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(iPAddr, 777); //현재 호스트의 IP주소와 포트 번호 가져오기

            listener.Init(endPoint, OnAcceptHandler);

            while(true)
            {

            }


        }
    }
}