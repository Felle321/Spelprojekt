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
        public Rectangle rectangle;
        public int targetRoom, targetDoor, sourceRoom, baseRoom, baseID;
        public bool active = false;
        public int direction = 0;
        /// <summary>
        /// Referring to the position of the source Tile
        /// </summary>
        public Point position = Point.Zero;

        public Door(Point tilePosition, Rectangle rectangle)
        {
            this.position = tilePosition;
            this.rectangle = rectangle;
        }

        public Door(Point tilePosition, Rectangle rectangle, int sourceRoom, int targetRoom, int targetDoor, int direction)
        {
            this.position = tilePosition;
            this.rectangle = rectangle;
            this.sourceRoom = sourceRoom;
            this.targetRoom = targetRoom;
            this.targetDoor = targetDoor;
            this.direction = direction;
        }
    }
}
