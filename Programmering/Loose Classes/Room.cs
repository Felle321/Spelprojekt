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
    public class Room
    {
        public List<Rectangle> rectangles = new List<Rectangle>();
        public List<Rectangle> rectanglesJump = new List<Rectangle>();
        public List<Slope> slopes = new List<Slope>();
        public List<Overlay> overlays = new List<Overlay>();
        public List<GameObject> gameObjects = new List<GameObject>();
        public List<Particle> particles = new List<Particle>();
        public List<EffectObject> effectObjects = new List<EffectObject>();
        public Dictionary<Vector2, Texture2D> backgrounds = new Dictionary<Vector2, Texture2D>();
        private float wind = 0f;
        public float Wind
        {
            get
            {
                if (windPositive)
                    return wind;
                else
                    return -wind;
            }
        }
        public float windMagnitude = 0;
        public bool windBool = true;
        private bool windPositive = true;

        public Room(Random rand, bool windBool, float windMagnitude)
        {
            if (windBool)
            {
                if (windMagnitude == null)
                    this.windMagnitude = rand.Next(World.maxWind);
                else
                    this.windMagnitude = windMagnitude;
            }
            else
            {
                this.windBool = false;
            }
        }

        public void Update(Random rand)
        {
            //Update wind
            if (windBool)
            {
                if(Math.Floor(rand.NextDouble() * (100 * windMagnitude)) == 0)
                {
                    windPositive = !windPositive;
                }

                wind = ((rand.Next(5) / (float)10) + .25f) * windMagnitude;
            }

            for (int i = 0; i < gameObjects.Count; i++)
            {
                if (gameObjects[i].remove)
                {
                    gameObjects.RemoveAt(i);
                }
                else
                {
                    gameObjects[i].ID = i;
                    gameObjects[i].Update(this, rand);
                }
            }
            
            for (int i = 0; i < particles.Count; i++)
            {
                if (particles[i].remove)
                    particles.RemoveAt(i);
                else
                    particles[i].Update(i, this, rand);
            }
            
        }

        public void Draw(Random rand, SpriteBatch spriteBatch, Camera camera)
        {
            for (int i = 0; i < gameObjects.Count; i++)
            {
                gameObjects[i].Draw(rand, spriteBatch, camera);
            }

            for (int i = 0; i < particles.Count; i++)
            {
                particles[i].Draw(rand, spriteBatch, camera);
            }

            foreach (Vector2 key in backgrounds.Keys)
            {
                spriteBatch.Draw(backgrounds[key], new Vector2(key.X * World.tileOffsetX, key.Y * World.tileOffsetY), Color.White);
            }

            for (int i = 0; i < effectObjects.Count; i++)
            {
                effectObjects[i].Draw(rand, spriteBatch, camera);
            }
        }

        public void DrawLights(Random rand, SpriteBatch spriteBatch, Camera camera)
        {
            for (int i = 0; i < gameObjects.Count; i++)
            {
                gameObjects[i].DrawLight(rand, spriteBatch, camera);
            }
        }

        public void DrawCollisions(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            for (int i = 0; i < rectangles.Count; i++)
            {
                Game1.DrawRectangle(graphicsDevice, spriteBatch, rectangles[i], Color.Red);
            }
            for (int i = 0; i < rectanglesJump.Count; i++)
            {
                Game1.DrawRectangle(graphicsDevice, spriteBatch, rectanglesJump[i], Color.Green);
            }
            for (int i = 0; i < slopes.Count; i++)
            {
                slopes[i].Draw(graphicsDevice, spriteBatch);
            }
            for (int i = 0; i < particles.Count; i++)
            {
                //Game1.DrawLine(graphicsDevice, spriteBatch, particles[i].position, particles[i].position + new Vector2((float)Math.Cos(particles[i].angle) * 5, (float)Math.Sin(particles[i].angle) * 5), Color.Red);
                //Game1.DrawRectangle(graphicsDevice, spriteBatch, particles[i].Rectangle, Color.White);
            }
        }

        public void DrawDebug(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            Vector2 windPosition = new Vector2(150, 300);
            Game1.DrawLine(graphicsDevice, spriteBatch, windPosition, windPosition + new Vector2(Wind * 200, 0), Color.Black);
            spriteBatch.DrawString(Game1.fontDebug, "GameObjects: " + gameObjects.Count.ToString(), new Vector2(20, 500), Color.Red);
            spriteBatch.DrawString(Game1.fontDebug, "Particles: " + particles.Count.ToString(), new Vector2(20, 550), Color.Red);
        }

        /// <summary>
        /// Floors the given number, reverses if negative
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static int FloorAdv(float number)
        {
            if (number < 0)
                return (int)Math.Ceiling(number);
            else
                return (int)Math.Floor(number);
        }

        /// <summary>
        /// Moves an object according to a Vector2 and returns an offset for the texture (slopes)
        /// </summary>
        /// <param name="position">Position of the object</param>
        /// <param name="rectangle">Rectangle of the object</param>
        /// <param name="movement">Vector to move by</param>
        /// <param name="onGround">OnGround bool</param>
        /// <returns></returns>
        public Vector2 MoveObject(ref Vector2 position, Rectangle rectangle, ref Vector2 movement, ref bool onGround, ref bool onWall, ref bool onWallRight, bool onGroundPrev, ref Slope slope, ref bool onSlope, float bounceFactor)
        {
            onGround = false;
            onSlope = false;
            bool skip = true;
            Point oldPosition = new Point(FloorAdv(position.X), FloorAdv(position.Y));
            Point newPosition = new Point(FloorAdv(position.X + movement.X), FloorAdv(position.Y + movement.Y));
            Rectangle oldRectangle = rectangle;
            Rectangle newRectangle = new Rectangle(newPosition.X, newPosition.Y, rectangle.Width, rectangle.Height);

            //Checks if we can skip the entire process
            #region Skip
            for (int i = 0; i < rectangles.Count; i++)
            {
                if (newRectangle.Intersects(rectangles[i]))
                {
                    skip = false;
                    break;
                }
            }
            if (skip)
            {
                for (int i = 0; i < rectanglesJump.Count; i++)
                {
                    if (newRectangle.Intersects(rectanglesJump[i]))
                    {
                        skip = false;
                        break;
                    }
                }
            }
            if (skip)
            {
                for (int i = 0; i < slopes.Count; i++)
                {
                    if (newRectangle.Intersects(slopes[i].rectangle))
                    {
                        skip = false;
                        break;
                    }
                }
            }
            #endregion

            if (!skip)
            {
                //Checks for collision with slopes
                for (int j = 0; j < slopes.Count; j++)
                {
                    if (rectangle.Intersects(slopes[j].rectangle) || Game1.LineIntersectsLine(new Vector2(slopes[j].rectangle.X, slopes[j].rectangle.Y + slopes[j].rectangle.Height), new Vector2(slopes[j].rectangle.X + slopes[j].rectangle.Width, slopes[j].rectangle.Y), oldPosition.ToVector2(), newPosition.ToVector2()))
                    {
                        if (slopes[j].faceRight)
                        {
                            //RIGHT
                            float anglePosition = Game1.GetAngle(new Vector2(slopes[j].rectangle.X, slopes[j].rectangle.Bottom), new Vector2(position.X + FloorAdv(movement.X) + rectangle.Width, position.Y + FloorAdv(movement.Y)));

                            if (Game1.LineIntersectsRect(new Vector2(slopes[j].rectangle.X, slopes[j].rectangle.Y + slopes[j].rectangle.Height), new Vector2(slopes[j].rectangle.X + slopes[j].rectangle.Width, slopes[j].rectangle.Y), newRectangle)
                                || (onGroundPrev && movement.Y == 0 && Game1.LineIntersectsRect(new Vector2(slopes[j].rectangle.X, slopes[j].rectangle.Y + slopes[j].rectangle.Height), new Vector2(slopes[j].rectangle.X + slopes[j].rectangle.Width, slopes[j].rectangle.Y), new Rectangle((int)position.X + FloorAdv(movement.X), (int)position.Y + FloorAdv(-movement.X), rectangle.Width, rectangle.Height)))
                                || slopes[j].angle > anglePosition
                                || Game1.LineIntersectsLine(new Vector2(slopes[j].rectangle.X, slopes[j].rectangle.Y + slopes[j].rectangle.Height), new Vector2(slopes[j].rectangle.X + slopes[j].rectangle.Width, slopes[j].rectangle.Y), oldPosition.ToVector2(), newPosition.ToVector2()))
                            {
                                onSlope = true;
                                slope = slopes[j];
                                float offset = slopes[j].Function(position.X - slopes[j].rectangle.X + rectangle.Width);
                                if (offset < 0)
                                    offset = -offset;
                                offset = FloorAdv(offset);
                                position.X = position.X + FloorAdv(movement.X);
                                position.Y = slopes[j].rectangle.Y + slopes[j].rectangle.Height - offset - rectangle.Height;

                                if (bounceFactor == 0)
                                {
                                    onGround = true;
                                    movement.Y = 0;
                                    return new Vector2(0, slopes[j].GetOffset(position.X, rectangle.Width));
                                }
                                else
                                {
                                    float length = movement.Length() * bounceFactor * World.friction * 1 / rectangle.Width * rectangle.Height;
                                    if (length < 1)
                                    {
                                        movement = Vector2.Zero;
                                    }
                                    Vector2 normal = Game1.GetVector2(slopes[j].angle + MathHelper.ToRadians(90)) * movement.Length() / 2;
                                    movement = new Vector2(normal.X + movement.X, normal.Y + movement.Y);
                                    if (movement.X < 0)
                                        movement.X *= World.friction * 0.8f;
                                    return Vector2.Zero;
                                }
                            }
                            else if(bounceFactor == 0 && onGroundPrev && movement.Y >= 0)
                            {
                                onSlope = true;
                                slope = slopes[j];
                                position.X = position.X + FloorAdv(movement.X);
                                int offset = FloorAdv(slopes[j].Function(position.X - slopes[j].rectangle.X + rectangle.Width));
                                if (offset < 0)
                                    offset = -offset;
                                offset = FloorAdv(offset);
                                position.Y = slopes[j].rectangle.Y + slopes[j].rectangle.Height - offset - rectangle.Height;
                                onGround = true;
                                movement.Y = 0;
                                return new Vector2(0, slopes[j].GetOffset(position.X, rectangle.Width));
                            }
                        }
                        else
                        {
                            //LEFT
                            float anglePosition = Game1.GetAngle(new Vector2(slopes[j].rectangle.Right, slopes[j].rectangle.Bottom), position + new Vector2(FloorAdv(movement.X), FloorAdv(movement.Y))) + MathHelper.ToRadians(180);
                            
                            if (Game1.LineIntersectsRect(new Vector2(slopes[j].rectangle.X, slopes[j].rectangle.Y), new Vector2(slopes[j].rectangle.X + slopes[j].rectangle.Width, slopes[j].rectangle.Y + slopes[j].rectangle.Height), new Rectangle((int)position.X + FloorAdv(movement.X), (int)position.Y + FloorAdv(movement.Y), rectangle.Width, rectangle.Height))
                                || (onGroundPrev && movement.Y == 0 && Game1.LineIntersectsRect(new Vector2(slopes[j].rectangle.X, slopes[j].rectangle.Y), new Vector2(slopes[j].rectangle.X + slopes[j].rectangle.Width, slopes[j].rectangle.Y + slopes[j].rectangle.Height), new Rectangle((int)position.X + FloorAdv(movement.X), (int)position.Y + FloorAdv(movement.X), rectangle.Width, rectangle.Height)))
                                || anglePosition > slopes[j].angle)
                            {
                                slope = slopes[j];
                                onSlope = true;
                                position.X = position.X + FloorAdv(movement.X);
                                float offset = slopes[j].Function(position.X - slopes[j].rectangle.X);
                                if (offset > 0)
                                    offset = -offset;
                                offset = FloorAdv(offset);
                                position.Y = slopes[j].rectangle.Y - offset - rectangle.Height;

                                if (bounceFactor == 0)
                                {
                                    onGround = true;
                                    movement.Y = 0;
                                    return new Vector2(0, -slopes[j].GetOffset(position.X, rectangle.Width));
                                }
                                else
                                {
                                    float length = movement.Length() * bounceFactor * World.friction * 1 / rectangle.Width * rectangle.Height;
                                    if (length < 1)
                                    {
                                        movement = Vector2.Zero;
                                    }
                                    Vector2 normal = Game1.GetVector2(slopes[j].angle - MathHelper.ToRadians(90)) * movement.Length() / 2;
                                    movement = new Vector2(normal.X + movement.X, normal.Y + movement.Y);
                                    if (movement.X > 0)
                                        movement.X *= World.friction * 0.8f;
                                    return Vector2.Zero;
                                }
                            }
                            else if(bounceFactor == 0 && onGroundPrev && movement.Y >= 0)
                            {
                                slope = slopes[j];
                                onSlope = true;
                                position.X = position.X + FloorAdv(movement.X);
                                float offset = (float)Math.Floor(slopes[j].Function(position.X - slopes[j].rectangle.X));
                                if (offset > 0)
                                    offset = -offset;
                                offset = FloorAdv(offset);
                                position.Y = slopes[j].rectangle.Y - offset - rectangle.Height;
                                onGround = true;
                                movement.Y = 0;
                                return new Vector2(0, -slopes[j].GetOffset(position.X, rectangle.Width));
                            }
                        }
                    }
                    else if(new Rectangle(newPosition.X, newPosition.Y + 1, rectangle.Width, rectangle.Height).Intersects(slopes[j].rectangle))
                    {
                        if((slopes[j].faceRight && rectangle.Right >= slopes[j].rectangle.Right) || (!slopes[j].faceRight && rectangle.Left <= slopes[j].rectangle.Left))
                        {
                            onGround = true;
                            movement.Y = 0;
                        }
                    }
                }


                //Checks for collisions with normal rectangles
                for (int j = 0; j < rectangles.Count; j++)
                {
                    if (new Rectangle(oldPosition.X, newPosition.Y, rectangle.Width, rectangle.Height).Intersects(rectangles[j]))
                    {
                        if (movement.Y > 0 && rectangle.Bottom - rectangles[j].Y < movement.Y)
                        {
                            position.Y = rectangles[j].Y - rectangle.Height;
                            onGround = true;
                        }
                        else if (movement.Y < 0 )
                        {
                            position.Y = rectangles[j].Y + rectangles[j].Height;
                        }
                        if (bounceFactor <= 0)
                            movement.Y = 0;
                        else
                        {
                            movement.Y = movement.Y * bounceFactor * -1;
                        }
                    }
                    else if (movement.Y != 0)
                    {
                        onGround = false;
                    }
                    else if (new Rectangle(oldPosition.X, newPosition.Y + 1, rectangle.Width, rectangle.Height).Intersects(rectangles[j]))
                    {
                        onGround = true;
                    }
                    

                    if (new Rectangle(newPosition.X, oldPosition.Y, rectangle.Width, rectangle.Height).Intersects(rectangles[j]))
                    {
                        if (movement.X > 0 && (rectangle.Right - rectangles[j].X) / 2 < movement.X)
                        {
                            position.X = rectangles[j].X - rectangle.Width;
                            onWall = true;
                            onWallRight = true;
                        }
                        else if (movement.X < 0 && (rectangles[j].Right - rectangle.X) / 2 < Math.Abs(movement.X))
                        {
                            position.X = rectangles[j].X + rectangles[j].Width;
                            onWall = true;
                            onWallRight = false;
                        }

                        if (bounceFactor <= 0)
                        {
                            movement.X = 0;
                        }
                        else
                        {
                            movement.X = movement.X * bounceFactor * -1;
                        }

                        if (movement.Y > 0)
                            movement.Y *= World.friction;
                    }
                }


                //Checks for collision with jump-through rectangles
                for (int j = 0; j < rectanglesJump.Count; j++)
                {
                    if (newRectangle.Intersects(rectanglesJump[j]))
                    {
                        if (oldRectangle.Bottom - 1 < rectanglesJump[j].Y && movement.Y > 0)
                        {
                            position.Y = rectanglesJump[j].Y - rectangle.Height;
                            movement.Y = 0;
                            onGround = true;
                        }
                    }
                }
            }
            else
            {
                if (movement.Y >= 0)
                {
                    for (int i = 0; i < rectangles.Count; i++)
                    {
                        if (new Rectangle((int)(newPosition.X), (int)(newPosition.Y + 1), rectangle.Width, rectangle.Height).Intersects(rectangles[i]))
                        {
                            onGround = true;
                            break;
                        }
                    }
                    
                    
                    for (int i = 0; i < slopes.Count; i++)
                    {
                        if (slopes[i].faceRight)
                        {
                            if (Game1.LineIntersectsRect(new Vector2(slopes[i].rectangle.X, slopes[i].rectangle.Y + slopes[i].rectangle.Height), new Vector2(slopes[i].rectangle.X + slopes[i].rectangle.Width, slopes[i].rectangle.Y), new Rectangle((int)position.X + FloorAdv(movement.X), (int)position.Y + 1, rectangle.Width, rectangle.Height)))
                            {
                                onGround = true;
                            }
                        }
                        else
                        {
                            if (Game1.LineIntersectsRect(new Vector2(slopes[i].rectangle.X, slopes[i].rectangle.Y), new Vector2(slopes[i].rectangle.X + slopes[i].rectangle.Width, slopes[i].rectangle.Y + slopes[i].rectangle.Height), new Rectangle((int)position.X + FloorAdv(movement.X), (int)position.Y + 1, rectangle.Width, rectangle.Height)))
                            {
                                onGround = true;
                            }
                        }
                    }
                    
                }
            }
            position += new Vector2((int)Game1.FloorAdv(movement.X), (int)Game1.FloorAdv(movement.Y));
            return Vector2.Zero;
        }

        public void CreateEffectObj(Vector2 position, Vector2 movement, Sprite sprite, EffectObject.Type type)
        {
            
        }
    }
}
