﻿using Core;
using System.Net;
using System.Text;

namespace Server.Session
{
    public class ClientSession : PacketSession //특정 클라이언트와의 통신용 소켓 들고있음 + 통신 시 이벤트에 따른 핸들러 함수들 구현
    {
        public int SessionId { get; set; }
        public GameRoom Room { get; set; }

        private object _lock = new object();

        public override void OnConnected(EndPoint endPoint)
        {
            Server.gameRoom.Push(() => { Server.gameRoom.Enter(this); });
            Thread.Sleep(1000);
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            SessionManager.Instance.Remove(this);   

            if(Room != null)
            {
                GameRoom room = Room;
                room.Push(() => { room.Leave(this); });
                Room = null;
            }
            Console.WriteLine($"Disconnect: {endPoint}");
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnSend(int numOfBytes)
        {
/*            Console.WriteLine($"Transferred Bytes in Server:{numOfBytes}");*/
        }

       
    }
}
