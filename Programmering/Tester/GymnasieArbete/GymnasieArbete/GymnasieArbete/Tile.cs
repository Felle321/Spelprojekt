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
using System.IO;

namespace GymnasieArbete
{
    public class Tile
    {
        /// <summary>
        /// The ID of the room the tile belongs to
        /// </summary>
        public int roomID;
        /// <summary>
        /// The ID/offset in relation to the rooms position
        /// </summary>
        public Point ID;
        /// <summary>
        /// The actual position of the tile with the position of the room accounted for
        /// </summary>
        public Point position;
        /// <summary>
        /// The x representing a Y position where a door can be placed and the y representing the platformID of the wall separating the rooms
        /// </summary>
        public List<Point> possibleDoorsRight = new List<Point>();
        /// <summary>
        /// The x representing a Y position where a door can be placed and the y representing the platformID of the wall separating the rooms
        /// </summary>
        public List<Point> possibleDoorsLeft = new List<Point>();


        public bool edgeLeft = false;
        public bool edgeRight = false;


        public Tile(Point ID)
        {
            this.ID = ID;
        }
    }
}
