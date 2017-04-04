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
        public Vector2 Origin
        {
            get
            {
                return new Vector2(rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height / 2);
            }
        }
        public int id, targetId;
        public string room;

        public Door(Rectangle rectangle, int id, string room, int targetId)
        {
            this.rectangle = rectangle;
            this.id = id;
            this.targetId = targetId;
        }
    }
}
