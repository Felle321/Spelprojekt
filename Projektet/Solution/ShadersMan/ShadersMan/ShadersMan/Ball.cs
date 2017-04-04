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

namespace ShadersMan
{
    public class Ball
    {
        public Vector2 position, positionPrev, movement, movementPrev;

        public Ball()
        {
            position = new Vector2(-100, -100);
            positionPrev = position;
            movement = Vector2.Zero;
        }

        public void Update(Vector2 pos, Vector2 posPrev)
        {
            movementPrev = movement;
            movement.Y += 0.5f;

            if (pos != null)
                position = pos;

            movement = pos - posPrev;

            positionPrev = posPrev;
        }
    }
}
