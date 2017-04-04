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
    public class Node
    {
        //targetID - the ID of the target Platform
        public int platformID, ID, targetID;
        public bool connected = false;
        public Point direction;
        public float w, h;
        public int xPos;

        public Point CompleteID
        {
            get
            {
                return new Point(platformID, ID);
            }
        }

        public Node(int platformID, int ID, int targetID)
        {
            this.platformID = platformID;
            this.ID = ID;
            this.targetID = targetID;
        }
        
        public void Initialize(List<Platform> platforms)
        {
            if (platforms[platformID].rectangle.Y < platforms[targetID].rectangle.Y)
                direction.Y = 1;
            else
                direction.Y = -1;

            h = Math.Abs(platforms[platformID].rectangle.Y - platforms[targetID].rectangle.Y);

            if (platforms[targetID].rectangle.X > platforms[platformID].rectangle.Right)
                w = platforms[targetID].rectangle.X - platforms[platformID].rectangle.Right;
            else if (platforms[targetID].rectangle.Right < platforms[platformID].rectangle.X)
                w = platforms[platformID].rectangle.X - platforms[targetID].rectangle.Right;
            else
                w = 0;

            if(direction.X > 0)
            {
                if(w == 0 && direction.Y < 0)
                {
                    xPos = platforms[targetID].rectangle.X;
                }
                else
                {
                    xPos = platforms[platformID].rectangle.Right;
                }
            }
            else
            {
                if (w == 0 && direction.Y < 0)
                {
                    xPos = platforms[targetID].rectangle.Right;
                }
                else
                {
                    xPos = platforms[platformID].rectangle.X;
                }
            }
        }
    }
}
