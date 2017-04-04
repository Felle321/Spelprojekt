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

        public static float ToFloat(this string s)
        {
            char dec;
            if (s.Contains('.'))
            {
                dec = '.';
            }
            else
                dec = ',';

            string[] array = s.Split(dec);

            if (array.Length < 2)
            {
                return int.Parse(s);
            }
            else return int.Parse(array[0]) + float.Parse(array[1]) / (float)Math.Pow(10, array[1].Length);
        }

        /// <summary>
        /// Returns a Point-variable representing the Rectangle's position
        /// </summary>
        /// <param name="r">Rectangle</param>
        /// <returns>Point-variable representing the Rectangle's position K</returns>
        public static Point ToPoint(this Rectangle r)
        {
            return new Point(r.X, r.Y);
        }

        public static Vector2 ToVector2(this Rectangle r)
        {
            return new Vector2(r.X, r.Y);
        }

        /// <summary>
        /// Returns a list of poinst representing the source (X) and the target (Y) platformIDs.
        /// </summary>
        /// <param name="instrs"></param>
        /// <returns></returns>
        public static List<Point> ToPoints(this List<Instruction> instrs)
        {
            List<Point> ret = new List<Point>();
            for (int i = 0; i < instrs.Count; i++)
            {
                ret.Add(instrs[i].platform);
            }
            return ret;
        }

        public static int ToDirection(this bool b)
        {
            if (b)
                return 1;
            else return -1;
        }
    }
}
