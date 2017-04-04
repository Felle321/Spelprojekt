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
    public class Platform
    {
        public enum Type
        {
            Rectangle,
            JumpThrough
        }
        public Type type = Type.Rectangle;
        public Rectangle rectangle;
        public List<Node> nodes = new List<Node>();
        public int ID;
        public List<Point> connectedSlopes = new List<Point>();
        public Vector2 origin;
        public bool usable = true;

        public Platform(int ID, Rectangle rectangle, Type type)
        {
            this.ID = ID;
            this.rectangle = rectangle;
            this.type = type;
            this.origin = new Vector2(rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height / 2);
        }

        public void Initialize(List<Platform> platforms, List<Slope> slopes, List<Tile> tiles)
        {
            for (int i = 0; i < platforms.Count; i++)
            {
                if ((Math.Abs(platforms[i].rectangle.Bottom - rectangle.Y) < 2 && platforms[i].rectangle.X <= rectangle.X && platforms[i].rectangle.Right >= rectangle.Right))
                {
                    usable = false;
                    return;
                }
            }

            usable = false;
            for (int i = 0; i < tiles.Count; i++)
            {
                if (tiles[i].ID == new Point((int)Math.Floor((rectangle.Y - 10) / (float)1024), (int)Math.Floor(rectangle.X / (float)1024)))
                {
                    usable = true;
                    break;
                }
            }

            if (!usable)
                return;

            for (int i = 0; i < slopes.Count; i++)
            {
                if (Game1.GetDistance(rectangle.Center, slopes[i].rectangle.Center) < 400)
                {
                    Rectangle rect = slopes[i].rectangle;
                    //rect.Inflate(20, 2);
                    //rect.Location = new Point(rect.Location.X - 10, rect.Location.Y - 1);
                    if (rect.Intersects(rectangle))
                    {
                        connectedSlopes.Add(new Point(i, slopes[i].faceRight.ToDirection()));
                    }
                }
            }
        }

        public void GetNodes(List<Platform> platforms)
        {
            if (usable)
            {
                for (int i = 0; i < platforms.Count; i++)
                {
                    if (Game1.GetDistance(platforms[i].rectangle.ToPoint(), rectangle.ToPoint()) < 2000 && platforms[i].usable && platforms[i].rectangle.Y >= rectangle.Y && i != ID)
                    {
                        bool possible = false;
                        int direction = 1;

                        if (type == Type.JumpThrough)
                        {
                            if (platforms[i].rectangle.X > rectangle.X && platforms[i].rectangle.Right < rectangle.Right)
                            {
                                for (int j = 0; j < 10; j++)
                                {
                                    bool lPossible = true;
                                    for (int k = 0; k < platforms.Count; k++)
                                    {
                                        if (i != k && k != ID &&
                                            platforms[k].rectangle.Intersects(new Rectangle(platforms[i].rectangle.X, rectangle.Y - 1, platforms[i].rectangle.Right, platforms[i].rectangle.Bottom - rectangle.Y + 1)))
                                        {
                                            if (Game1.LineIntersectsRect(new Vector2(platforms[i].rectangle.X + (platforms[i].rectangle.Width / 10) * j, rectangle.Y - 1), new Vector2(platforms[i].rectangle.X + (platforms[i].rectangle.Width / 10) * j, platforms[i].rectangle.Y - 1), platforms[k].rectangle))
                                            {
                                                lPossible = false;
                                                break;
                                            }
                                        }
                                    }

                                    if (lPossible)
                                    {
                                        possible = true;
                                        direction = 0;
                                        break;
                                    }
                                }
                            }
                            else if (platforms[i].rectangle.X > rectangle.X && platforms[i].rectangle.X < rectangle.Right)
                            {
                                //Overlaps, offset to the right
                                int width = platforms[i].rectangle.X - rectangle.Right;

                                for (int j = 0; j < 10; j++)
                                {
                                    bool lPossible = true;
                                    for (int k = 0; k < platforms.Count; k++)
                                    {
                                        if (i != k && k != ID &&
                                            platforms[k].rectangle.Intersects(new Rectangle(platforms[i].rectangle.X, rectangle.Y - 1, platforms[i].rectangle.Right, platforms[i].rectangle.Bottom - rectangle.Y + 1)))
                                        {
                                            if (Game1.LineIntersectsRect(new Vector2(platforms[i].rectangle.X + (width / 10) * j, rectangle.Y - 1), new Vector2(platforms[i].rectangle.X + (width / 10) * j, platforms[i].rectangle.Y - 1), platforms[k].rectangle))
                                            {
                                                lPossible = false;
                                                break;
                                            }
                                        }
                                    }

                                    if (lPossible)
                                    {
                                        possible = true;
                                        direction = 0;
                                        break;
                                    }
                                }
                            }
                            else if (platforms[i].rectangle.Right < rectangle.Right && platforms[i].rectangle.Right > rectangle.X)
                            {
                                //Overlaps, offset to the left
                                int width = platforms[i].rectangle.Right - rectangle.X;

                                for (int j = 0; j < 10; j++)
                                {
                                    bool lPossible = true;
                                    for (int k = 0; k < platforms.Count; k++)
                                    {
                                        if (i != k && k != ID &&
                                            platforms[k].rectangle.Intersects(new Rectangle(platforms[i].rectangle.X, rectangle.Y - 1, platforms[i].rectangle.Right, platforms[i].rectangle.Bottom - rectangle.Y + 1)))
                                        {
                                            if (Game1.LineIntersectsRect(new Vector2(rectangle.X + (width / 10) * j, rectangle.Y - 1), new Vector2(platforms[i].rectangle.X + (width / 10) * j, platforms[i].rectangle.Y - 1), platforms[k].rectangle))
                                            {
                                                lPossible = false;
                                                break;
                                            }
                                        }
                                    }

                                    if (lPossible)
                                    {
                                        possible = true;
                                        direction = 0;
                                        break;
                                    }
                                }
                            }
                        }

                        if (platforms[i].rectangle.X < rectangle.X && !possible)
                        {
                            int width;
                            if (platforms[i].rectangle.Right < rectangle.X)
                                width = platforms[i].rectangle.Width;
                            else
                                width = rectangle.X - platforms[i].rectangle.X;

                            for (int j = 0; j < 10; j++)
                            {
                                bool lPossible = true;

                                for (int k = 0; k < platforms.Count; k++)
                                {
                                    if (k != ID && i != k &&
                                        platforms[k].rectangle.Intersects(new Rectangle(platforms[i].rectangle.X, rectangle.Y - 1, rectangle.Right - platforms[i].rectangle.X, platforms[i].rectangle.Bottom - rectangle.Y + 1)))
                                    {
                                        if (Game1.LineIntersectsRect(new Vector2(rectangle.X, rectangle.Y - 1), new Vector2(platforms[i].rectangle.X + (width / 10) * j, platforms[i].rectangle.Y - 1), platforms[k].rectangle))
                                        {
                                            lPossible = false;
                                            break;
                                        }
                                    }
                                }

                                if (lPossible)
                                {
                                    possible = true;
                                    direction = -1;
                                    break;
                                }
                            }
                        }

                        if (!possible && rectangle.Right < platforms[i].rectangle.Right)
                        {
                            int width;
                            if (platforms[i].rectangle.X < rectangle.Right)
                            {
                                width = platforms[i].rectangle.Right - rectangle.Right;
                            }
                            else
                            {
                                width = platforms[i].rectangle.Width;
                            }

                            for (int j = 0; j < 10; j++)
                            {
                                bool lPossible = true;

                                for (int k = 0; k < platforms.Count; k++)
                                {
                                    if (k != ID && i != k && platforms[k].rectangle.Intersects(new Rectangle(rectangle.Right, rectangle.Y - 1, platforms[i].rectangle.Right - rectangle.Right, platforms[i].rectangle.Y - rectangle.Y)))
                                    {
                                        if (Game1.LineIntersectsRect(new Vector2(rectangle.Right - 1, rectangle.Y - 1), new Vector2(platforms[i].rectangle.Right - (width / 10) * j, platforms[i].rectangle.Y - 1), platforms[k].rectangle))
                                        {
                                            lPossible = false;
                                            break;
                                        }
                                    }
                                }

                                if (lPossible)
                                {
                                    possible = true;
                                    direction = 1;
                                    break;
                                }
                            }
                        }

                        if (possible)
                        {
                            nodes.Add(new Node(ID, nodes.Count, i));
                            nodes[nodes.Count - 1].direction.X = direction;

                            if ((platforms[i].rectangle.Right + 1 == rectangle.X || platforms[i].rectangle.X - 1 == rectangle.Right) && platforms[i].rectangle.Y == rectangle.Y)
                            {
                                nodes[nodes.Count - 1].connected = true;
                            }

                            if (rectangle.Y != platforms[i].rectangle.Y)
                            {
                                platforms[i].nodes.Add(new Node(i, platforms[i].nodes.Count, ID));
                                platforms[i].nodes[platforms[i].nodes.Count - 1].direction.X = -direction;
                            }
                        }

                        for (int j = 0; j < connectedSlopes.Count; j++)
                        {
                            if (platforms[i].connectedSlopes.Contains(connectedSlopes[j]))
                            {
                                nodes.Add(new Node(ID, nodes.Count, i));
                                nodes[nodes.Count - 1].connected = true;
                                nodes[nodes.Count - 1].direction.X = connectedSlopes[j].Y;
                            }
                        }
                    }
                }
            }
        }

        public virtual void Draw(Point offset, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            if (type == Type.Rectangle)
            {
                if (usable)
                    Game1.DrawRectangle(graphicsDevice, spriteBatch, new Rectangle(rectangle.X + offset.X, rectangle.Y + offset.Y, rectangle.Width, rectangle.Height), Color.Green);
                else
                    Game1.DrawRectangle(graphicsDevice, spriteBatch, new Rectangle(rectangle.X + offset.X, rectangle.Y + offset.Y, rectangle.Width, rectangle.Height), Color.Red);
            }
            else
            {
                if (usable)
                    Game1.DrawRectangle(graphicsDevice, spriteBatch, new Rectangle(rectangle.X + offset.X, rectangle.Y + offset.Y, rectangle.Width, rectangle.Height), Color.Blue);
                else
                    Game1.DrawRectangle(graphicsDevice, spriteBatch, new Rectangle(rectangle.X + offset.X, rectangle.Y + offset.Y, rectangle.Width, rectangle.Height), Color.Black);
            }
        }

        public virtual void DrawID(Point offset, SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(Game1.fontDebug, ID.ToString(), rectangle.ToVector2() + offset.ToVector2(), Color.Blue);
        }
    }
}
