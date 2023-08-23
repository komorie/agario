using System;

namespace Core
{
    public class RecvBuffer //받는데 사용하는 버퍼 클래스인데 TCP 통신 상 패킷을 부분적으로 받았을 때, 합쳐서 처리하기 위한 기능을 수행
    {
        private ArraySegment<byte> buffer;  

        private int readPos; //받은 패킷을 컨텐츠 영역에서 처리할 때 buffer의 해당 인덱스부터 처리함
        private int writePos; //클라에게서 받을 때 buffer의 해당 인덱스부터 작성

        public int DataSize { get { return writePos - readPos; } } //현재 쌓여있는 데이터 크기
        public int FreeSize { get { return buffer.Count - writePos; } }  //버퍼의 남은 공간. 일정 크기 미만일 시 pos를 초기화해야
        
        public ArraySegment<byte> ReadSegment //버퍼에서 읽어갈 단위만큼 가리키는 부분배열
        { 
            get { return new ArraySegment<byte>(buffer.Array, buffer.Offset + readPos, DataSize); } 
        }

        public ArraySegment<byte> WriteSegment //버퍼에서 쓸수있는 범위를 가져오는 부분배열
        {
            get { return new ArraySegment<byte>(buffer.Array, buffer.Offset + writePos, FreeSize); }
        }

        public RecvBuffer(int bufferSize)
        {
            buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
        }


        public void Clean()
        {
            int dataSize = DataSize;
            if (dataSize == 0) //r w 일치하면 데이터 없음
            {
                readPos = writePos = 0;
            }
            else //아니면 r, w 사이거 앞으로 옮겨오기
            {
                Array.Copy(buffer.Array, buffer.Offset + readPos, buffer.Array, buffer.Offset, dataSize);
                readPos = 0;
                writePos = dataSize;    
            }
        }

        public bool OnRead(int numOfBytes) //읽었을 때, 포인터 옮기고 유효한지 체크
        {
            if (numOfBytes > DataSize)
            {
                return false;
            }
            readPos += numOfBytes;
            return true;
        }

        public bool OnWrite(int numOfBytes) //받았을 때, 포인터 옮기고 유효한지 체크
        {
            if (numOfBytes > FreeSize)
            {
                return false;
            }
            writePos += numOfBytes;
            return true;
        }


    }
}
