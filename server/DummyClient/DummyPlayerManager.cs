using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyClient
{
    public class DummyPlayerManager
    {
        DummyPlayer myPlayer;
        Dictionary<int, DummyPlayer> players = new Dictionary<int, DummyPlayer>();

        public void Add(S_PlayerList packet) //처음에 접속한 플레이어들 목록 받아서 관리 딕셔너리에 추가함.
        {
            foreach (S_PlayerList.Player p in packet.players)
            {
                if (p.isSelf)
                {
                    DummyPlayer mPlayer = new DummyPlayer()
                    {
                        PlayerId = p.playerId,
                        PosX = p.posX,
                        PosY = p.posY,
                        PosZ = p.posZ
                    };
                    myPlayer = mPlayer;
                }
                else
                {
                    DummyPlayer player = new DummyPlayer()
                    {
                        PlayerId = p.playerId,
                        PosX = p.posX,
                        PosY = p.posY,
                        PosZ = p.posZ
                    };  
                    players.Add(p.playerId, player);
                }

            }
        }

        public void EnterGame(S_BroadcastEnterGame p) //서버에게서 새로운 유저가 들어왔다! 라는 패킷을 받는 경우 -> 일단 MyPlayer일리는 없음(고려 안함)
        {
            if (p.playerId == myPlayer.PlayerId) //이미 있는 플레이어인 경우
            {
                return;
            }
            
            //새 플레이어 추가
            DummyPlayer player = new DummyPlayer()
            {
                PlayerId = p.playerId,
                PosX = p.posX,
                PosY = p.posY,
                PosZ = p.posZ
            };
        }

        public void Move(S_BroadcastMove p)
        {
            //일단 나인경우와 다른 플레이어인 경우 상관 없이 위치만 바꿔줌
            DummyPlayer player;
            if (players.TryGetValue(p.playerId, out player))
            {
                player.PosX = p.posX;
                player.PosY = p.posY;
                player.PosZ = p.posZ;
            }
            
        }

        public void LeaveGame(S_BroadcastLeaveGame p) //누군가 게임 종료했다는 패킷이 왔을 때
        {
            if (myPlayer.PlayerId == p.playerId)
            {
                myPlayer = null;
            }
            else
            {
                //플레이어 있나 확인 후 제거
                DummyPlayer player;
                if (players.TryGetValue(p.playerId, out player))
                {
                    players.Remove(p.playerId);
                }

            }
        }
    }
}
