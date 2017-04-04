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
    public static class Extensions
    {
        public static Point ToPoint(this Vector2 v)
        {
            return new Point(Game1.FloorAdv(v.X), Game1.FloorAdv(v.Y));
        }

        public static Vector2 FloorAdv(this Vector2 v)
        {
            return new Vector2(Game1.FloorAdv(v.X), Game1.FloorAdv(v.Y));
        }

        public static Vector2 CeilAdv(this Vector2 v)
        {
            return new Vector2(Game1.CeilAdv(v.X), Game1.CeilAdv(v.Y));
        }

        public static Vector2 Round(this Vector2 v)
        {
            return new Vector2((int)Math.Round(v.X), (int)Math.Round(v.Y));
        }

        public static Vector2 ToVector2(this Point p)
        {
            return new Vector2(p.X, p.Y);
        }

        public static Vector2 ToDirection(this Vector2 v)
        {
            float a = v.ToRadians();
            return new Vector2((float)Math.Cos(a), (float)Math.Sin(a));
        }

        public static float ToRadians(this Vector2 v)
        {
            return (Game1.GetAngle(v, Vector2.Zero) + MathHelper.ToRadians(360)) % MathHelper.ToRadians(360);
        }
    }
}
