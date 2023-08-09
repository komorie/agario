using System.Net.Sockets;
using System.Net;
using System.Text;

namespace DummyClient
{
    internal class DummyClient
    {
        static void Main(string[] args)
        {
            while(true)
            {
                try
                {
                    string host = Dns.GetHostName();
                    IPHostEntry ipHost = Dns.GetHostEntry(host);
                    IPAddress iPAddr = ipHost.AddressList[0];
                    IPEndPoint endPoint = new IPEndPoint(iPAddr, 777); //현재 호스트의 IP주소와 포트 번호 가져오기

                    Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp); //TCP 소켓 생성

                    socket.Connect(endPoint); //서버쪽 엔드포인트에 연결
                    Console.WriteLine($"{socket.RemoteEndPoint}"); //반대쪽 엔드포인트 출력

                    byte[] sendBuff = Encoding.UTF8.GetBytes("I'm Client");
                    int sendBytes = socket.Send(sendBuff);

                    byte[] recvBuff = new byte[1024];
                    int recvBytes = socket.Receive(recvBuff);
                    string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
                    Console.WriteLine(recvData);

                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
                catch (Exception)
                {

                    throw;
                }

                Thread.Sleep(100);
            }
            
        }
    }
}