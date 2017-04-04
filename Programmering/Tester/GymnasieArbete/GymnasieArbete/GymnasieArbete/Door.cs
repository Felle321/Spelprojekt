using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace GymnasieArbete
{
    public class Door
    {
        public int wall;
        public Rectangle rectangle;
        public int targetRoom, targetDoor;
        public int direction = 0;

        public Door(int wall, int targetRoom, int targetDoor, int direction)
        {
            this.wall = wall;
            this.targetRoom = targetRoom;
            this.targetDoor = targetDoor;
            this.direction = direction;
        }
    }
}
