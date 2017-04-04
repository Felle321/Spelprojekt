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
    public class RoomReference
    {
        public Point position;
        public int reference;
        public bool cleared;
        /// <summary>
        /// The X represents the wall being shortened to make a door and the Y represents the direction (1 or -1)
        /// </summary>
        public List<Door> doors = new List<Door>();

        /// <summary>
        /// Initializes a roomreference
        /// </summary>
        /// <param name="position"></param>
        /// <param name="reference"></param>
        public RoomReference(Point position, int reference)
        {
            this.position = position;
            this.reference = reference;
        }
    }
}
