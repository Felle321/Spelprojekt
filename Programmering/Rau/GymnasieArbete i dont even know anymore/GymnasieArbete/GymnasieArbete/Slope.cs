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
    public class Slope
    {
        public Rectangle rectangle;
        public bool faceRight;
        public float angle;
        float k = 0;
        float angleCos, angleSin;
        public int platformID = -1;

        public Slope(Rectangle newRect, bool faceRight)
        {
            rectangle = newRect;
            this.faceRight = faceRight;
            if (faceRight)
            {
                angle = Game1.GetAngle(new Vector2(rectangle.X + rectangle.Width, rectangle.Y), new Vector2(rectangle.X, rectangle.Y + rectangle.Height));
            }
            else
            {
                angle = Game1.GetAngle(new Vector2(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height), new Vector2(rectangle.X, rectangle.Y)) + MathHelper.ToRadians(180);
            }

            k = rectangle.Height / ((float)rectangle.Width);
            angleCos = (float)Math.Cos(angle);
            angleSin = (float)Math.Sin(angle);
        }

        public void Draw(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            Game1.DrawLine(graphicsDevice, spriteBatch, new Vector2(rectangle.X, rectangle.Y + rectangle.Height), new Vector2(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height), Color.Red);
            if (faceRight)
            {
                Game1.DrawLine(graphicsDevice, spriteBatch, new Vector2(rectangle.X, rectangle.Y + rectangle.Height), new Vector2(rectangle.X + rectangle.Width, rectangle.Y), Color.Red);
                Game1.DrawLine(graphicsDevice, spriteBatch, new Vector2(rectangle.X + rectangle.Width, rectangle.Y), new Vector2(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height), Color.Red);
            }
            else
            {
                Game1.DrawLine(graphicsDevice, spriteBatch, new Vector2(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height), new Vector2(rectangle.X, rectangle.Y), Color.Red);
                Game1.DrawLine(graphicsDevice, spriteBatch, new Vector2(rectangle.X, rectangle.Y), new Vector2(rectangle.X, rectangle.Y + rectangle.Height), Color.Red);
            }
        }

        public float Function(float x)
        {
            if (x < 0)
            {
                return 0;
            }
            else if (x > rectangle.Width)
            {
                return -rectangle.Height;
            }
            else
            {
                if (faceRight)
                {
                    return k * x;
                }
                else
                {
                    return -k * x;
                }
            }
        }

        public float GetOffset(float xPosition, int width)
        {
            if (xPosition + width / 2 - rectangle.X <= 0)
            {
                if (!faceRight)
                    return -rectangle.Height;
                else
                    return Function(xPosition + width - rectangle.X);
            }
            else if (xPosition - rectangle.X + width / 2 > rectangle.Width)
            {
                if (faceRight)
                    return -rectangle.Height;
                else
                    return -(rectangle.Height + Function(xPosition - rectangle.X));
            }
            else
            {
                if (faceRight)
                    return (float)Function(width / 2) * (1 - (float)(xPosition + width / 2 - rectangle.X) / rectangle.Width);
                else
                    return (float)Function(width / 2) * ((float)(xPosition + width / 2 - rectangle.X) / rectangle.Width);
            }
        }
    }
}