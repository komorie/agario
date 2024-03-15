using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public class Player
    {
        public int PlayerId { get; set; } //ID
        public float DirX { get; set; }
        public float DirY { get; set; } //이동 방향

        public float moveTime { get; set; } 

        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; } //위치

        public float Radius { get; set; }   //크기(반지름)
    }
}
