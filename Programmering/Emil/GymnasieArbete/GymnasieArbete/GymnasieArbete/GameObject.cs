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
    public abstract class GameObject
    {
        public Vector2 position, positionPrev, textureOffset;
        public Vector2 movement = Vector2.Zero;
        public Vector2 movementPrev = Vector2.Zero;
        public int Level;
        public int width = 1;
        public int height = 1;
        public string sprite, spriteLight;
        public float angle = 0;
        public float rotation = 0;
        public int ID = 0;
        public bool stunned = false;
        public int stunTimer = 0;
        public int stunTime = 0;
        public enum Types
        {
            Enemy,
            Projectile,
            Prop,
            Player,

        };
        public Types type;
        public float hp = 1;
        float hpFactor;
        /// <summary>
        /// Right = 1
        /// Left = -1
        /// </summary>
        public int direction = 1;
        public bool onWall = false;
        public bool onWallPrev = false;
        public bool onWallRight = false;
        public bool onGround = false;
        public bool onGroundPrev = false;
        public bool onSlope = false;
        public bool useGravity = true;
        public bool useResistance = true;
        public bool useFriction = true;
        public Slope slope = null;
        public bool live = true;
        public bool remove = false;
        /// <summary>
        /// 0 = no bounce ||
        /// 0.5 = 50% bounce back ||
        /// 1 = 100% bounce back
        /// </summary>
        public float bounceFactor = 0.0f;
        //The lower the stronger 0 < x <= 1
        public float airResitance = .95f;
        public float friction = .90f;
        public bool fallThrough = false;
        public int platformID = -1;
        public int platformIDPrev = -1;
        public Vector2 movementMax;
        public List<Buff> Buffs = new List<Buff>();

        public Rectangle Rectangle
        {
            get
            {
                return new Rectangle((int)position.X, (int)position.Y, width, height);
            }
            set
            {
                position = new Vector2(value.X, value.Y);
            }
        }
        public Vector2 Origin
        {
            get
            {
                return new Vector2(position.X + width / 2, position.Y + height / 2);
            }
            set
            {
                position = new Vector2(value.X - width / 2, value.Y - height / 2);
            }
        }

        public GameObject(Rectangle rectangle, int level, Room newRoom, string spriteKey)
        {
            this.ID = newRoom.GetID();
            if (spriteKey != null && spriteKey != "")
                sprite = spriteKey;
            Rectangle = rectangle;
            width = rectangle.Width;
            height = rectangle.Height;
        }

        public virtual void Update(Room room, Random rand)
        {

            if (live)
            {
                if (hp <= 0)
                {
                    live = false;
                }
                onGroundPrev = onGround;
                onWallPrev = onWall;
                movementPrev = movement;
                positionPrev = position;


                //Gravity
                if ((!onGround && useGravity) || onSlope)
                {
                    if (onSlope && slope != null)
                    {
                        movement += new Vector2(World.gravity.X * (1 - Game1.GetVector2(slope.angle).X), World.gravity.Y * (1 - Game1.GetVector2(slope.angle).Y)) * 0.5f;
                    }
                    else
                    {
                        movement += World.gravity;
                    }
                }


                if (useResistance)
                {
                    movement.X *= World.airResitance;
                    movement.Y *= World.airResitance;
                }

                //Angle
                angle += rotation;
                angle %= MathHelper.ToRadians(360);

                //Move
                if (!stunned)
                    movement = GetMovement();
                else
                {
                    stunTimer++;
                    if (stunTimer >= stunTime)
                    {
                        stunTimer = 0;
                        stunTime = 0;
                        stunned = false;
                    }
                }
                textureOffset = room.MoveObject(ref position, Rectangle, ref movement, ref onGround, ref onWall, ref onWallRight, onGroundPrev, ref slope, ref onSlope, bounceFactor, ref platformID, ref platformIDPrev, fallThrough);
                //Round down
                if (Math.Abs(movement.X) < .002f && movement.X != 0)
                    movement.X = 0;
                if (Math.Abs(movement.Y) < .002f && movement.Y != 0 || onGround)
                    movement.Y = 0;

                if (onGround && useFriction)
                    movement.X *= World.friction;

                if (Buffs.Count > 1)
                {
                    for (int i = 0; i < Buffs.Count; i++)
                    {
                        for (int j = 0; j < Buffs.Count; j++)
                        {
                            if (Buffs[i].buffTag == Buffs[j].buffTag && !Buffs[j].canStack && !Buffs[j].applied)
                            {
                                Buffs[i].duration = Buffs[j].duration;
                                Buffs.RemoveAt(j);
                                i = 0;
                                j = 0;
                                break;
                            }
                        }
                    }
                }
                for (int i = 0; i < Buffs.Count; i++)
                {
                    Buffs[i].Update();

                    if (Buffs[i].applied)
                    {
                        if (Buffs[i].duration <= 0)
                        {
                            if (Buffs[i].hpFactor)
                            {
                                hp = (int)(hp / Buffs[i].hp);
                            }
                            else
                            {
                                hp -= (int)Buffs[i].hp;
                            }
                            if (Buffs[i].speedFactor)
                            {
                                movementMax.X /= Buffs[i].speed;
                            }
                            else
                            {
                                movementMax.X -= Buffs[i].speed;
                            }
                            if (Buffs[i].jumpFactor)
                            {
                                movementMax.Y /= Buffs[i].jumpSpeed;
                            }
                            else
                            {
                                movementMax.Y -= Buffs[i].jumpSpeed;
                            }
                            Buffs.RemoveAt(i);
                        }
                    }
                    else
                    {
                        if (Buffs[i].hpFactor)
                        {
                            hp = (int)(hp * Buffs[i].hp);
                        }
                        else
                        {
                            hp += (int)Buffs[i].hp;
                        }
                        if (Buffs[i].speedFactor)
                        {
                            movementMax.X *= Buffs[i].speed;
                        }
                        else
                        {
                            movementMax.X += Buffs[i].speed;
                        }
                        if (Buffs[i].jumpFactor)
                        {
                            movementMax.Y *= Buffs[i].jumpSpeed;
                        }
                        else
                        {
                            movementMax.Y += Buffs[i].jumpSpeed;
                        }
                        Buffs[i].applied = true;
                    }
                }
            }
            else
            {
                Die(room, rand);
            }
        }

        public virtual void Update(Room room, Random rand, Player player)
        {
            Update(room, rand);
        }

        /// <summary>
        /// The action the object performs when it stops living
        /// </summary>
        public virtual void Die(Room room, Random rand)
        {
            //This makes sure that the Die function cant be executed twice
            live = false;
            remove = true;
        }




        /// <summary>
        /// Draws the sprite of a gameobject
        /// </summary>
        /// <param name="rand"></param>
        /// <param name="spriteBatch"></param>
        /// <param name="camera">The active camera</param>
        public virtual void Draw(Point offset, Random rand, SpriteBatch spriteBatch, GraphicsDevice graphics, Camera camera)
        {
            if (sprite != "")
                SpriteHandler.Draw(sprite, rand, spriteBatch, camera, position, SpriteEffects.None, 0.5f);
        }

        /// <summary>
        /// Draws a red rectangle of the rectangle used for the players collision
        /// </summary>
        /// <param name="graphicsDevice"></param>
        /// <param name="spriteBatch"></param>
        public virtual void DrawCol(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            Game1.DrawRectangle(graphicsDevice, spriteBatch, new Rectangle(Rectangle.X, Rectangle.Y, Rectangle.Width, Rectangle.Height), Color.Red);
        }

        /// <summary>
        /// Draws the light of a gameobject
        /// </summary>
        /// <param name="rand"></param>
        /// <param name="spriteBatch"></param>
        /// <param name="camera">The active camera</param>
        public virtual void DrawLight(Point offset, Random rand, SpriteBatch spriteBatch, Camera camera)
        {
            if (spriteLight != null)
                SpriteHandler.Draw(spriteLight, rand, spriteBatch, camera, position, SpriteEffects.None, 0.5f);
        }

        public virtual Vector2 GetMovement()
        {
            return movement;
        }

        /// <summary>
        /// Makes the object take damage based on the input
        /// </summary>
        /// <param name="room">The room in which the objects are</param>
        /// <param name="sourceID">The ID of the object causing the damage</param>
        /// <param name="damage"></param>
        public virtual void TakeDamage(Room room, int sourceID, float damage)
        {
            hp -= (int)Math.Round(damage);
            if (World.displayDamageGameObjects)
                room.effectObjects.Add(new EffectObject(position, movement, World.fontDamage, ((int)Math.Round(damage)).ToString(), 120));
        }

        /// <summary>
        /// Returns the hitbox of the object
        /// </summary>
        public virtual Rectangle GetHitBox()
        {
            return Rectangle;
        }
        public class Scrap : GameObject
        {
            public int value;
            public Scrap(Rectangle rectangle, int level, Room newRoom, string spriteKey, Vector2 movement, int value) : base(rectangle, level, newRoom, spriteKey)
            {
                this.movement = movement;
                this.value = value;
            }

        }

        public class Chest : GameObject
        {
            public bool used;
            public bool spew;
            public int spewCounter = 0;
            public bool locked;
            public int total;
            public Chest(Rectangle rectangle, int level, Room newRoom, string spriteKey, bool locked) : base(rectangle, level, newRoom, spriteKey)
            {
                this.locked = locked;
                used = false;
                spew = false;
            }

            public override void Update(Room room, Random rand)
            {
                if(Rectangle.Intersects(room.gameObjects[0].Rectangle) && !locked && !used && !spew)
                {
                    spew = true;
                    total = (int)(room.gameObjects[0].Level + rand.NextDouble() * room.gameObjects[0].Level);
                    total *= rand.Next(7, 20);
                }
                if(spew && !used)
                {
                    spewCounter++;
                    if (spewCounter % 5 == 0)
                    {
                        if (total <= 0)
                        {
                            spew = false;
                            used = true;
                        }

                        if (total > 100)
                        {
                            room.scraps.Add(new Scrap(new Rectangle((int)position.X + Rectangle.Width / 2, (int)position.Y, SpriteHandler.sprites["Gear3"].width, SpriteHandler.sprites["Gear3"].height), 1, room, "Gear3", new Vector2(rand.Next(-10, 10), rand.Next(-7, -1)), 100));
                            total -= 100;
                        }
                        if (total > 10 && total < 100)
                        {
                            room.scraps.Add(new Scrap(new Rectangle((int)position.X + Rectangle.Width / 2, (int)position.Y, SpriteHandler.sprites["Gear2"].width, SpriteHandler.sprites["Gear2"].height), 1, room, "Gear2", new Vector2(rand.Next(-10, 10), rand.Next(-7, -1)), 10));
                            total -= 10;
                        }
                        if (total > 0 && total < 10)
                        {
                            room.scraps.Add(new Scrap(new Rectangle((int)position.X + Rectangle.Width / 2, (int)position.Y, SpriteHandler.sprites["Gear1"].width, SpriteHandler.sprites["Gear1"].height), 1, room, "Gear1", new Vector2(rand.Next(-10, 10), rand.Next(-7, -1)), 1));
                            total -= 1;
                        }
                    }
                }
            }

            public override void Draw(Point offset, Random rand, SpriteBatch spriteBatch, GraphicsDevice graphics, Camera camera)
            {
                base.Draw(offset, rand, spriteBatch, graphics, camera);
            }
            public override void DrawLight(Point offset, Random rand, SpriteBatch spriteBatch, Camera camera)
            {
                base.DrawLight(offset, rand, spriteBatch, camera);
            }
        }
    }
}