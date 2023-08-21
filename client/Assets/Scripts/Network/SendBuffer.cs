using System;
using System.Threading;

namespace Core
{
    public static class SendBufferHelper //스레드마다 전역 Send Buffer를 할당한 뒤, 쪼개서 사용하도록 도와주는 클래스
    {
        public static ThreadLocal<SendBuffer> CurrentBuffer = new ThreadLocal<SendBuffer>( () => { return null;  } );

        public static int chunkSize { get; set; } = 6553500;

        public static ArraySegment<byte> Open(int reserveSize) //자동으로 범위 체크하고, 스레드마다 전역으로 생성된 센드 버퍼에서 가져오도록 함
        {
            if (CurrentBuffer.Value == null)
                CurrentBuffer.Value = new SendBuffer(chunkSize);

            if (CurrentBuffer.Value.FreeSize < reserveSize)
                CurrentBuffer.Value = new SendBuffer(chunkSize);

            return CurrentBuffer.Value.Open(reserveSize);
        }

        public static ArraySegment<byte> Close(int usedSize) //스레드마다 전역으로 생성된 센드 버퍼에서 가져오도록 함
        {
            return CurrentBuffer.Value.Close(usedSize);
        }
    }

    public class SendBuffer //일단 크게 잡아주고 Send할 때마다 usedSize 옮기면서 사용할 버퍼
    {
        private byte[] buffer; 
        int usedSize = 0;

        public int FreeSize { get { return buffer.Length - usedSize; } } //남은 공간 크기

        public SendBuffer(int chunkSize)
        {
            buffer = new byte[chunkSize];
        }

        public ArraySegment<byte> Open(int reserveSize) //Send할 버퍼로 사용할 크기만큼의 부분 리턴
        {
            if (reserveSize > FreeSize) return null;
            return new ArraySegment<byte>(buffer, usedSize, reserveSize);
        }

        public ArraySegment<byte> Close(int usedSize) //Open에서 예약한 이후로, 실제로 사용된 부분만큼의 부분 리턴하고 포인터 옮기기
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(buffer, this.usedSize, usedSize);
            this.usedSize += usedSize;
            return segment;
        }
    }
}
