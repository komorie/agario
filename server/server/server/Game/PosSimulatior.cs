using Server.Session;
using System.Numerics;


namespace Server.Game
{
    public class PosSimulatior
    {
        float UpdateInterval = 0.25f; // 0.25초마다 업데이트
        int Speed = 20;

        // 이전 패킷 정보
        Vector2 lastReceivedPosition; // 2D 게임을 가정합니다.
        Vector2 lastReceivedDirection;
        float lastReceivedTime;

        public void OnPacketReceived(C_Move movePacket)
        {
            lastReceivedPosition = new Vector2(movePacket.posX, movePacket.posY);
            lastReceivedDirection = new Vector2(movePacket.dirX, movePacket.dirY);
            lastReceivedTime = movePacket.time;
        }

        public void Update()
        {

            float currentTime = (float)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            float elapsedTime = currentTime - lastReceivedTime;

            // 패킷을 받은 이후의 시간이 UpdateInterval 이상 지났는지 확인
            if (elapsedTime >= UpdateInterval)
            {
                // 예측 로직
                Vector2 movement = new Vector2(lastReceivedDirection.X, lastReceivedDirection.Y);
                Vector2 predictedPosition = lastReceivedPosition + Vector2.Normalize(movement) * Speed * elapsedTime;

                // 예측된 위치 업데이트
                lastReceivedPosition = predictedPosition;

                // 시간 재설정
                lastReceivedTime = currentTime;
            }
        }
    }
}
