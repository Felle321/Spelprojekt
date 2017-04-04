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
    public abstract class Particle
    {
        public int ID;
        public bool remove = false;
        public static Texture2D pixel;
        public Vector2 position;
        public Vector2 movement;
        public int duration;
        Color color;
        public float rotation = 0;
        public string sprite;
        public float opacity = 1f;
        public float angle = 0f;
        public Vector2 origin = Vector2.Zero;
        public virtual Rectangle Rectangle
        {
            get
            {
                return new Rectangle(0,0,0,0);
            }
        }
        bool delay = true;

        public enum Type
        {
            smoke
        }

        public Type type;

        public Particle(int ID)
        {
            this.ID = ID;
        }

        public virtual void Update(int ID, Room room, Random rand)
        {
            this.ID = ID;
        }

        public virtual void Draw(Random rand, SpriteBatch spriteBatch, Camera camera)
        {
        }

        public class Smoke : Particle
        {
            public float scale, thickness;
            public override Rectangle Rectangle
            {
                get
                {
                    return new Rectangle(Game1.FloorAdv(position.X) - 32, Game1.FloorAdv(position.Y) - 32, 64, 64);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="position"></param>
            /// <param name="movement"></param>
            /// <param name="scale"></param>
            /// <param name="thickness"></param>
            /// <param name="duration">The amount of steps the smoke stays</param>
            /// <param name="color">Color of the smoke</param>
            /// <param name="subs">A list of subparticles, can be left as null</param>
            public Smoke(int ID, Random rand, Vector2 position, Vector2 movement, float angle, float scale, float thickness, int duration, Color color) : base(ID)
            {
                this.sprite = "particle_smoke";
                type = Type.smoke;
                this.position = position;
                this.movement = movement;
                this.angle = angle;
                this.scale = scale;
                this.thickness = thickness;
                this.duration = duration;
                this.color = color;
                movement.Y = (float)-rand.NextDouble() * 2;
                position.Y -= (float)rand.NextDouble() + (float)rand.NextDouble();
                opacity = thickness;
            }

            public override void Update(int ID, Room room, Random rand)
            {
                this.ID = ID;
                duration--;
                if (duration <= 0)
                {
                    remove = true;
                    return;
                }
                movement.X += room.Wind * ((float)rand.NextDouble() + .5f);
                if (movement.Y > -4f / (thickness * 2))
                {
                    movement.Y -= .2f / (thickness * 2);
                }
                else
                    movement.Y = -4f / (thickness * 2);

                if (rotation / Math.Abs(rotation) != movement.X / Math.Abs(movement.X) && Math.Abs(rotation) > .01f)
                    rotation *= .7f;
                else
                    rotation = movement.X / 100 / thickness;

                if(duration < 60 * thickness && duration != 0)
                {
                    opacity = (float)duration / 60 * thickness;
                }
                if(duration < 120 * thickness && duration != 0)
                {
                    scale += (float)duration / 120  / 50;
                }

                origin = new Vector2(32, 32);

                angle += rotation;

                #region Collision
                bool skip = true;
                Vector2 oldPosition = new Vector2(FloorAdv(position.X), FloorAdv(position.Y));
                Vector2 newPosition = new Vector2(FloorAdv(position.X + movement.X), FloorAdv(position.Y + movement.Y));

                //Checks if we can skip the entire process
                #region Skip
                for (int i = 0; i < room.rectangles.Count; i++)
                {
                    if (room.rectangles[i].Contains(position.ToPoint()))
                    {
                        skip = false;
                        break;
                    }
                }
                if (skip)
                {
                    for (int i = 0; i < room.rectanglesJump.Count; i++)
                    {
                        if (room.rectanglesJump[i].Contains(position.ToPoint()))
                        {
                            skip = false;
                            break;
                        }
                    }
                }
                if (skip)
                {
                    for (int i = 0; i < room.slopes.Count; i++)
                    {
                        if (room.slopes[i].rectangle.Contains(position.ToPoint()))
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
                    for (int i = 0; i < room.slopes.Count; i++)
                    {
                        if (room.slopes[i].rectangle.Contains(newPosition.ToPoint()))
                        {
                            if (room.slopes[i].faceRight)
                            {
                                //RIGHT
                                float anglePosition = Game1.GetAngle(new Vector2(room.slopes[i].rectangle.X, room.slopes[i].rectangle.Bottom), position);

                                if (room.slopes[i].angle > anglePosition)
                                {
                                    float offset = room.slopes[i].Function(position.X - room.slopes[i].rectangle.X);
                                    if (offset < 0)
                                        offset = -offset;
                                    offset = Game1.FloorAdv(offset);
                                    position.Y = room.slopes[i].rectangle.Y + room.slopes[i].rectangle.Height - offset;

                                    float length = scale * thickness;
                                    Vector2 normal = Game1.GetVector2(room.slopes[i].angle + MathHelper.ToRadians(90)) * movement.Length() / 2;
                                    movement = new Vector2(normal.X + movement.X, normal.Y + movement.Y);
                                }
                            }
                            else
                            {
                                //LEFT
                                float anglePosition = Game1.GetAngle(new Vector2(room.slopes[i].rectangle.Right, room.slopes[i].rectangle.Bottom), position) + MathHelper.ToRadians(180);

                                if (anglePosition > room.slopes[i].angle)
                                {
                                    float offset = room.slopes[i].Function(position.X - room.slopes[i].rectangle.X);
                                    if (offset > 0)
                                        offset = -offset;
                                    offset = Game1.FloorAdv(offset);
                                    position.Y = room.slopes[i].rectangle.Y - offset;


                                    float length = scale * thickness;
                                    Vector2 normal = Game1.GetVector2(room.slopes[i].angle - MathHelper.ToRadians(90)) * movement.Length() / 2;
                                    movement = new Vector2(normal.X + movement.X, normal.Y + movement.Y);
                                }
                            }
                        }
                    }
                    

                    //Checks for collisions with normal rectangles
                    for (int i = 0; i < room.rectangles.Count; i++)
                    {
                        if (room.rectangles[i].Contains(new Vector2(position.X, newPosition.Y).ToPoint()))
                        {
                            if (movement.Y > 0 && position.Y - room.rectangles[i].Y < movement.Y + 2)
                            {
                                position.Y = room. rectangles[i].Y;
                                movement.Y = 0;
                            }
                            else if (movement.Y < 0 && room.rectangles[i].Bottom - position.Y < Math.Abs(movement.Y) + 2)
                            {
                                position.Y = room.rectangles[i].Bottom;
                                movement.Y = 0;
                            }
                        }

                        if (room.rectangles[i].Contains(new Vector2(newPosition.X, oldPosition.Y).ToPoint()))
                        {
                            if (movement.X > 0 && position.X - room.rectangles[i].X < movement.X + 2)
                            {
                                position.X = room.rectangles[i].X;
                                movement.X = 0;
                            }
                            else if (movement.X < 0 && room.rectangles[i].Right - position.X < Math.Abs(movement.X) + 2)
                            {
                                position.X = room.rectangles[i].Right;
                                movement.X = 0;
                            }
                        }
                        
                    }
                }
                else
                {

                    //SPREAD
                    for (int i = 0; i < room.particles.Count; i++)
                    {
                        if (i != ID)
                        {
                            if (room.particles[i].type == Type.smoke)
                            {
                                int distance = (int)Game1.GetDistance(position, room.particles[i].position);
                                if (distance < World.smokeSpreadRange * scale / thickness 
                                    && distance > 5
                                    && position.X != room.particles[i].position.X 
                                    && position.Y != room.particles[i].position.Y)
                                {
                                    room.particles[i].movement.X += (room.particles[i].position.X - position.X) * thickness / distance * 2 / World.smokeSpread;
                                    room.particles[i].movement.Y += (room.particles[i].position.Y - position.Y) * thickness / distance * 2 / World.smokeSpread;
                                    movement.X -= (room.particles[i].position.X - position.X) * thickness / distance * 2 / World.smokeSpread;
                                    movement.Y -= (room.particles[i].position.Y - position.Y) * thickness / distance * 2 / World.smokeSpread;
                                }
                                else if(distance < 5)
                                {
                                    movement.Y -= .2f;
                                }
                            }
                        }
                    }
                }

                position = new Vector2(position.X + Game1.FloorAdv(movement.X), position.Y + Game1.FloorAdv(movement.Y));
                #endregion
            }

            public override void Draw(Random rand, SpriteBatch spriteBatch, Camera camera)
            {
                if (delay)
                    delay = false;
                else
                    SpriteHandler.Draw(sprite, rand, spriteBatch, camera, position, scale, angle, origin, color, opacity);
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
        }

        public class Dynamic : GameObject
        {
            public bool impact = false;
            public bool additive = false;
            public Color color;
            public float scale;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="rectangle"></param>
            /// <param name="level"></param>
            /// <param name="newRoom"></param>
            /// <param name="spriteKey"></param>
            /// <param name="pixel">Defines if the particle uses a sprite or simply is a pixel with a color</param>
            /// <param name="newColor"></param>
            /// <param name="newScale"></param>
            /// <param name="duration">The number of the steps the particle will be around man</param>
            public Dynamic(Rectangle rectangle, int level, Room newRoom, string spriteKey, bool pixel, Color color, float scale, int duration, bool impact, bool additive) : base( 0, rectangle, level, newRoom, spriteKey)
            {
                hp = duration;
                this.color = color;
                this.scale = scale;
                this.impact = impact;
                this.additive = additive;
            }

            public override void Update(Room room, Random rand)
            {
                hp--;
                base.Update(room, rand);
            }

            public override void Draw(Random rand, SpriteBatch spriteBatch, Camera camera)
            {
                if(sprite == null)
                {
                    spriteBatch.Draw(pixel, position, null, color, 0f, Vector2.Zero, new Vector2(Rectangle.Width * scale, Rectangle.Height * scale), SpriteEffects.None, 0f);
                }
                else
                    base.Draw(rand, spriteBatch, camera);
            }
        }
    }
}