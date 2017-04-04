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
        public float opacityLight = 1f;
        public float angle = 0f;
        public float scale = 1f;
        public float scaleLight = 1f;
        public Vector2 origin = Vector2.Zero;
        public virtual Rectangle Rectangle
        {
            get
            {
                return new Rectangle(0, 0, 0, 0);
            }
        }
        bool delay = true;

        public enum Type
        {
            Smoke,
            Fire
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

        public virtual void Draw(Random rand, SpriteBatch spriteBatch, Camera camera, float depth)
        {
        }

        public virtual void DrawLight(Random rand, SpriteBatch spriteBatch, Camera camera)
        {
        }

        public class Smoke : Particle
        {
            public float thickness;
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
            public Smoke(int ID, Random rand, Vector2 position, Vector2 movement, float angle, float scale, float thickness, int duration, Color color) : base(ID)
            {
                this.sprite = "particle_smoke";
                type = Type.Smoke;
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
                origin = new Vector2(32, 32);
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
                if (movement.Y > -5f / (thickness * 2))
                {
                    movement.Y -= .3f / (thickness * 2);
                }
                else
                    movement.Y = -5f / (thickness * 2);

                if (rotation / Math.Abs(rotation) != movement.X / Math.Abs(movement.X) && Math.Abs(rotation) > .01f)
                    rotation *= .7f;
                else
                    rotation = movement.X / 100 / thickness;

                if (duration < 60 * thickness && duration != 0)
                {
                    opacity = (float)duration / 60 * thickness;
                }
                if (duration < 120 * thickness && duration != 0)
                {
                    scale += (float)duration / 120 / 50;
                }


                angle += rotation;

                #region Collision
                bool skip = true;
                Vector2 oldPosition = new Vector2(Game1.FloorAdv(position.X), Game1.FloorAdv(position.Y));
                Vector2 newPosition = new Vector2(Game1.FloorAdv(position.X + movement.X), Game1.FloorAdv(position.Y + movement.Y));

                //Checks if we can skip the entire process
                #region Skip
                for (int i = 0; i < room.platforms.Count; i++)
                {
                    if (room.platforms[i].rectangle.Contains(position.ToPoint()))
                    {
                        skip = false;
                        break;
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
                    for (int i = 0; i < room.platforms.Count; i++)
                    {
                        if (room.platforms[i].rectangle.Contains(new Vector2(position.X, newPosition.Y).ToPoint()))
                        {
                            if (movement.Y > 0 && position.Y - room.platforms[i].rectangle.Y < movement.Y + 2)
                            {
                                position.Y = room.platforms[i].rectangle.Y;
                                movement.Y = 0;
                            }
                            else if (movement.Y < 0 && room.platforms[i].rectangle.Bottom - position.Y < Math.Abs(movement.Y) + 2)
                            {
                                position.Y = room.platforms[i].rectangle.Bottom;
                                movement.Y = 0;
                            }
                        }

                        if (room.platforms[i].rectangle.Contains(new Vector2(newPosition.X, oldPosition.Y).ToPoint()))
                        {
                            if (movement.X > 0 && position.X - room.platforms[i].rectangle.X < movement.X + 2)
                            {
                                position.X = room.platforms[i].rectangle.X;
                                movement.X = 0;
                            }
                            else if (movement.X < 0 && room.platforms[i].rectangle.Right - position.X < Math.Abs(movement.X) + 2)
                            {
                                position.X = room.platforms[i].rectangle.Right;
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
                            if (room.particles[i].type == Type.Smoke)
                            {
                                int distance = (int)Game1.GetDistance(position, room.particles[i].position);
                                if (distance < World.smokeSpreadRange * scale / thickness
                                    && distance > 5
                                    && position.X != room.particles[i].position.X
                                    && position.Y != room.particles[i].position.Y)
                                {
                                    float xForce = (room.particles[i].position.X - position.X) * thickness / distance * 2 / World.smokeSpread;
                                    float yForce = (room.particles[i].position.Y - position.Y) * thickness / distance * 2 / World.smokeSpread;
                                    room.particles[i].movement.X += xForce;
                                    room.particles[i].movement.Y += yForce;
                                    movement.X -= xForce;
                                    movement.Y -= yForce;
                                }
                                else if (distance < 5)
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

            public override void Draw(Random rand, SpriteBatch spriteBatch, Camera camera, float depth)
            {
                if (delay)
                    delay = false;
                else
                    SpriteHandler.Draw(sprite, rand, spriteBatch, camera, position, scale, angle, origin, color, opacity, SpriteEffects.None, depth);
            }
        }

        public class Fire : Particle
        {
            public float thickness;
            public int spriteNum = 0;
            int initialDuration = 0;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="position"></param>
            /// <param name="movement"></param>
            /// <param name="scale"></param>
            /// <param name="duration">The amount of steps the smoke stays</param>
            /// <param name="color">Color of the smoke</param>
            public Fire(int ID, Room room, Random rand, Vector2 position, Vector2 movement, float scale, float thickness, int duration, Color color) : base(ID)
            {
                this.sprite = "particle_fire";
                type = Type.Fire;
                this.position = position;
                this.movement = movement;
                this.scale = scale;
                this.duration = duration;
                this.color = color;
                this.thickness = thickness;
                movement.Y = (float)-rand.NextDouble() * 2;
                position.Y -= (float)rand.NextDouble() + (float)rand.NextDouble();
                opacity = thickness * .8f;
                origin = new Vector2(32, 32);
                angle = (float)rand.NextDouble() * 2 * 3.14f;
                spriteNum = rand.Next(SpriteHandler.sprites["particle_fire"].framesTotal);
                initialDuration = duration;
                if (rand.Next(12) != 0)
                    room.particles.Add(new Smoke(room.particles.Count, rand, position, new Vector2(movement.X, movement.Y / 2), angle, scale, thickness / 2, initialDuration * 2, Color.Black));
            }

            public override void Update(int ID, Room room, Random rand)
            {
                angle = (float)Math.Atan2(movement.Y, movement.X);
                this.ID = ID;
                duration--;
                if (duration <= 0)
                {
                    room.particles.Add(new Smoke(room.particles.Count, rand, position, new Vector2(movement.X, movement.Y / 2), angle, scale, thickness, initialDuration, Color.Black));
                    remove = true;
                    return;
                }

                movement.X += room.Wind * ((float)rand.NextDouble() + .5f);
                if (movement.Y > -10f / (thickness * 2))
                {
                    movement.Y -= .6f / (thickness * 2);
                }
                else
                    movement.Y = -10f / (thickness * 2);

                if (duration < 10 * thickness && duration != 0)
                {
                    opacity = ((float)duration / initialDuration * thickness) * .8f;
                }

                #region Collision
                bool skip = true;
                Vector2 oldPosition = new Vector2(Game1.FloorAdv(position.X), Game1.FloorAdv(position.Y));
                Vector2 newPosition = new Vector2(Game1.FloorAdv(position.X + movement.X), Game1.FloorAdv(position.Y + movement.Y));

                //Checks if we can skip the entire process
                #region Skip
                for (int i = 0; i < room.platforms.Count; i++)
                {
                    if (room.platforms[i].rectangle.Contains(position.ToPoint()))
                    {
                        skip = false;
                        break;
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
                    for (int i = 0; i < room.platforms.Count; i++)
                    {
                        if (room.platforms[i].rectangle.Contains(new Vector2(position.X, newPosition.Y).ToPoint()))
                        {
                            if (movement.Y > 0 && position.Y - room.platforms[i].rectangle.Y < movement.Y + 2)
                            {
                                position.Y = room.platforms[i].rectangle.Y;
                                movement.Y = 0;
                            }
                            else if (movement.Y < 0 && room.platforms[i].rectangle.Bottom - position.Y < Math.Abs(movement.Y) + 2)
                            {
                                position.Y = room.platforms[i].rectangle.Bottom;
                                movement.Y = 0;
                            }
                        }

                        if (room.platforms[i].rectangle.Contains(new Vector2(newPosition.X, oldPosition.Y).ToPoint()))
                        {
                            if (movement.X > 0 && position.X - room.platforms[i].rectangle.X < movement.X + 2)
                            {
                                position.X = room.platforms[i].rectangle.X;
                                movement.X = 0;
                            }
                            else if (movement.X < 0 && room.platforms[i].rectangle.Right - position.X < Math.Abs(movement.X) + 2)
                            {
                                position.X = room.platforms[i].rectangle.Right;
                                movement.X = 0;
                            }
                        }

                    }
                }

                position = new Vector2(position.X + Game1.FloorAdv(movement.X), position.Y + Game1.FloorAdv(movement.Y));
                #endregion
            }

            public override void Draw(Random rand, SpriteBatch spriteBatch, Camera camera, float depth)
            {
                if (delay)
                    delay = false;
                else
                {
                    SpriteHandler.Draw(sprite, rand, spriteBatch, camera, position, scale, angle, origin, color, opacity, SpriteEffects.None, spriteNum);
                }
            }

            public override void DrawLight(Random rand, SpriteBatch spriteBatch, Camera camera)
            {
                SpriteHandler.Draw("gradient", rand, spriteBatch, camera, position, scale, 0, origin, color, opacityLight, SpriteEffects.None, 0);
            }
        }

        public class Static : Particle
        {
            Vector2 deAcceleration, scaleVector2;
            string spriteKey;
            float startDuration;
            public Static(int ID, string spriteKey, Random rand, Vector2 position, Vector2 movement, Vector2 deAcceleration, float scale, float scaleLight, int duration, Color color) : base(ID)
            {
                this.scaleLight = scaleLight;
                this.spriteKey = spriteKey;
                type = Type.Smoke;
                this.position = position;
                this.movement = movement;
                this.scale = scale;
                this.scaleVector2 = Vector2.Zero;
                this.duration = duration;
                startDuration = duration;
                this.deAcceleration = deAcceleration;
                this.color = color;
                origin = new Vector2(32, 32);
                angle = (float)Math.Atan2(movement.Y, movement.X);
            }

            public Static(int ID, string spriteKey, Random rand, Vector2 position, Vector2 movement, Vector2 deAcceleration, Vector2 scale, float scaleLight, int duration, Color color) : base(ID)
            {
                this.scaleLight = scaleLight;
                this.spriteKey = spriteKey;
                type = Type.Smoke;
                this.position = position;
                this.movement = movement;
                this.scale = 0;
                this.scaleVector2 = scale;
                this.duration = duration;
                startDuration = duration;
                this.deAcceleration = deAcceleration;
                this.color = color;
                origin = new Vector2(32, 32);
                angle = (float)Math.Atan2(movement.Y, movement.X);
            }

            public override void Update(int ID, Room room, Random rand)
            {
                base.Update(ID, room, rand);
                duration--;
                if (duration <= 0)
                {
                    remove = true;
                    return;
                }
                position += movement;
                movement *= deAcceleration;
                angle = (float)Math.Atan2(movement.Y, movement.X);
            }

            public override void Draw(Random rand, SpriteBatch spriteBatch, Camera camera, float depth)
            {
                if(scale == 0)
                    SpriteHandler.Draw(spriteKey, rand, spriteBatch, camera, position, scaleVector2, angle, new Vector2(SpriteHandler.sprites[spriteKey].width / 2, SpriteHandler.sprites[spriteKey].height / 2), Color.White, 1f, SpriteEffects.None, depth);
                else
                    SpriteHandler.Draw(spriteKey, rand, spriteBatch, camera, position, scale, angle, new Vector2(SpriteHandler.sprites[spriteKey].width/2,SpriteHandler.sprites[spriteKey].height/2), Color.White, 1f, SpriteEffects.None, depth);
            }

            public override void DrawLight(Random rand, SpriteBatch spriteBatch, Camera camera)
            {
                SpriteHandler.Draw("gradient", rand, spriteBatch, camera, position, scaleLight, angle, new Vector2(128, 128), color, (duration / (startDuration * 1.2f)) * opacityLight, SpriteEffects.None, 0.5f);
            }

        }


        public class Dynamic : GameObject
        {
            public bool impact = false;
            public bool additive = false;
            public Color color;
            public float scale;
            public Vector2 deAcceleration;

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
            public Dynamic(Rectangle rectangle, int level, Room newRoom, Vector2 movement, Vector2 deAcceleration,string spriteKey, bool pixel, Color color, float scale, int duration, bool impact, bool additive) : base(rectangle, level, newRoom, spriteKey)
            {
                hp = duration;
                this.movement = movement;
                this.deAcceleration = deAcceleration;
                this.color = color;
                this.scale = scale;
                this.impact = impact;
                this.additive = additive;
                angle = (float)Math.Atan2(this.movement.Y, this.movement.X);
            }

            public override void Update(Room room, Random rand)
            {
                base.Update(room, rand);
                hp--;
                angle = (float)Math.Atan2(movement.Y, movement.X);
                if(deAcceleration != Vector2.Zero)
                {
                    movement *= deAcceleration;
                }
                
            }

            public override void Draw(Random rand, SpriteBatch spriteBatch, GraphicsDevice graphics, Camera camera)
            {
                if (sprite == null)
                {
                    spriteBatch.Draw(pixel, position, null, color, angle, Vector2.Zero, new Vector2(Rectangle.Width * scale, Rectangle.Height * scale), SpriteEffects.None, 0f);
                }
                else
                    base.Draw(rand, spriteBatch, graphics, camera);
            }
            
        }
        public class Bounce : Particle.Dynamic
        {
            public Bounce(Rectangle rectangle, int level, Room newRoom, Vector2 movement, Vector2 deAcceleration, float bounceFactor, bool useGravity, bool useResistance, string spriteKey, bool pixel, Color color, float scale, int duration, bool impact, bool additive) : base(rectangle, level, newRoom, movement, deAcceleration, spriteKey, pixel, color, scale, duration, impact, additive)
            {
                this.useGravity = useGravity;
                this.bounceFactor = bounceFactor;
                this.useResistance = useResistance;
            }
        }
        
    }
   
}