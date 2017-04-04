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
    public abstract class Enemy : GameObject
    {
        public int Level { get; set; }
        public int direction = 1;
        public float damage = 5;
        public DamageZone DamageZone_Hitbox { get; set; }
        public float damageZone_damage;
        public bool attackPlayer = false;
        public SpriteEffects spriteEffect;
        public Animation animation { get; set; }
        public List<Types> attackable = new List<Types>();
        public enum State
        {
            walking,
            shooting,
            charging,
            jump,
            fall,
            idle,
            attacking
        };
        State state = State.idle;
        List<Instruction> path = new List<Instruction>();
        public float range;
        public Vector2 movementMax;
        public bool executingInstruction = false;
        public bool stuck = false;
        public int fallThroughCooldown = 0;
        private Vector2 eyePosOffset;
        public int maxChase = 600;
        /// <summary>
        /// Defines how many tries the pathfinder can make to find a suitable platform. OBS This does not mean that a path of 10 platforms takes 10 tries. It grows exponentially.
        /// </summary>
        public int maxPath = 10;
        private int chaseTimer = 0;
        public Vector2 EyePos
        {
            get
            {
                return position + eyePosOffset;
            }
        }
        protected bool usePath = true;
        public float walkingSpeed = 7;
        public bool UsePath
        {
            get
            {
                return usePath;
            }
            set
            {
                usePath = value;
                if (!value)
                {
                    path.Clear();
                    executingInstruction = false;
                    stuck = false;
                    chaseTimer = 0;
                }
            }
        }


        public Enemy(Vector2 position, Room room, int level, Animation animation, float hitboxDamage, int hitboxCooldown) : base(new Rectangle((int)position.X, (int)position.Y, animation.Width, animation.Height), 0, room, "")
        {
            this.animation = animation;
            base.sprite = sprite;
            this.position = position;
            this.useResistance = false;
            this.useFriction = false;
            type = Types.Enemy;
            eyePosOffset = new Vector2(Rectangle.Width / 2, Rectangle.Height / 2);

            attackable.Add(Types.Player);
            this.DamageZone_Hitbox = new DamageZone(Rectangle, hitboxDamage, 60 * hitboxCooldown, 0, ID, false, true, attackable);
            room.damageZones.Add(DamageZone_Hitbox);
            Level = level;
        }

        public override void Draw(Point offset, Random rand, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Camera camera)
        {
            animation.Draw(rand, spriteBatch, camera, new Vector2(Origin.X - animation.Width / 2 + offset.X, Rectangle.Bottom - height + offset.Y) + textureOffset, spriteEffect, 0.5f);
        }

        public override void Update(Room room, Random rand, Player player)
        {
            //Sets fallThrough to false after the cooldown has been set to zero
            if (fallThroughCooldown > 0)
                fallThroughCooldown--;
            else if (fallThrough)
                fallThrough = false;

            if (chaseTimer > 0)
                chaseTimer--;

            if (PlayerInSight(player, room) && UsePath)
            {
                //Stuck signifies that the enemy can't calculate a viable path to the player
                //If the player or the enemy changes platform the conditions has changed and it should try again
                if (stuck && (player.platformID != player.platformIDPrev || platformID != platformIDPrev))
                {
                    stuck = false;
                }

                //Keeping the x-velocity while walking/jumping, etc.
                if (executingInstruction)
                    movement.X = movementPrev.X;

                //If the bot is located on the same platform as the player is/was
                if ((platformID == player.platformID || (platformID == player.platformIDPrev && player.platformID == -1)) && platformID != -1)
                {
                    //The path is no longer necessary and should be cleared.
                    //No instruction is being executed and until further notice the state is "Idle"
                    path.Clear();
                    executingInstruction = false;
                    Idle();
                    //If the player is outside the enemy's range it will move left/right to get closer
                    if (Game1.GetDistance(Origin, player.Origin) > range)
                    {
                        if (player.position.X > position.X)
                        {
                            movement.X = walkingSpeed;
                            state = State.walking;
                            direction = 1;
                        }
                        else
                        {
                            movement.X = -walkingSpeed;
                            state = State.walking;
                            direction = -1;
                        }
                    }
                    //If the player is in range of the enemy it should attack
                    else
                    {
                        Attack(player, rand, room);
                    }
                }
                //The player has changed it's platform and we now need a new path
                else if (player.platformID != player.platformIDPrev && player.platformID != -1 && platformID != -1)
                    GetPath(player, room);
                //If the player is on a different platform we need to get or follow a path
                //This can only be done if the enemy is on a platform
                //The path.count > 0 statement prevents errors since we'll be accessing path[0]
                else if (platformID != -1 && path.Count > 0)
                {
                    //If we're in the middle of executing a given instruction
                    if (executingInstruction)
                    {
                        //If we're not on the platform we started on and we're not in the air we should get to the next instruction
                        if (platformID != path[0].platform.X)
                        {
                            path.RemoveAt(0);
                            executingInstruction = false;

                            //Preventing errors
                            if (path.Count > 0)
                            {
                                //This would indicate that we're standing on the wrong platform
                                if (platformID != path[0].platform.X)
                                {
                                    //The current path is no longer correct and we need a new one
                                    GetPath(player, room);
                                }
                            }
                        }
                    }
                    //If the enemy is standing on the correct platform
                    else if (platformID == path[0].platform.X)
                    {
                        //An instruction with an "idle" state is the equivalent of an error and can't be executed
                        if (path[0].state == State.idle)
                            path.RemoveAt(0);
                        //If the position matches the requirement we're good to go
                        else if (position.X == path[0].xPos || (path[0].variableXPos && (Rectangle.Right < room.platforms[path[0].platform.Y].rectangle.Right && Rectangle.X > room.platforms[path[0].platform.Y].rectangle.X)))
                        {
                            //Execute instruction
                            movement.X = path[0].movement.X;
                            state = path[0].state;

                            if (state == State.jump)
                                Jump(path[0].movement.Y);
                            else if (state == State.fall)
                            {
                                FallThrough();
                            }

                            if (movement.X < 0)
                                direction = -1;
                            else if (movement.X > 0)
                                direction = 1;

                            executingInstruction = true;
                        }
                        else if (path[0].variableXPos)
                        {
                            if (room.platforms[path[0].platform.Y].rectangle.Center.X > Rectangle.Center.X)
                            {
                                movement.X = walkingSpeed;
                                state = State.walking;
                                direction = 1;
                            }
                            else
                            {
                                movement.X = -walkingSpeed;
                                state = State.walking;
                                direction = -1;
                            }
                        }
                        else if (Math.Abs(path[0].xPos - position.X) < movementMax.X)
                        {
                            //If the enemy is close enough it's position gets set to match the requirement
                            position.X = path[0].xPos;
                            movement.X = 0;
                            state = State.idle;
                        }
                        //If the enemy's position isn't equal (or close enough) to the required xPos it will try to move to the left/right
                        else if (path[0].xPos > position.X)
                        {
                            movement.X = walkingSpeed;
                            state = State.walking;
                            direction = 1;
                        }
                        else
                        {
                            movement.X = -walkingSpeed;
                            state = State.walking;
                            direction = -1;
                        }
                    }
                    //If the enemy executing an instruction and is not on the platform of the next instruction
                    else
                    {
                        //This would mean the path has not been followed and we're no longer on it
                        //The current path is no longer correct and we need a new one
                        GetPath(player, room);
                    }
                }
                //You can't create a path if you're not on a platform
                else if (platformID != -1)
                {
                    //The enemy needs a path and doesn't have one
                    GetPath(player, room);
                }
            }
            else
            {
                Idle();
            }

            if (onGround && (state == State.jump || state == State.fall))
                state = State.idle;

            if (direction <= 0)
            {
                spriteEffect = SpriteEffects.FlipHorizontally;
            }
            else
            {
                spriteEffect = SpriteEffects.None;
            }

            //Basic update (mostly physics, etc.)
            base.Update(room, rand, player);
        }

        /// <summary>
        /// Gets a path or sets stuck to true depending on the succes of the pathfinding. 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="room"></param>
        public virtual void GetPath(Player player, Room room)
        {
            executingInstruction = false;
            int targetPlatform = player.platformID;
            if (player.platformID == -1)
                targetPlatform = player.platformIDPrev;

            path = room.GetPathInstructions(platformID, targetPlatform, new Point(Rectangle.Width, Rectangle.Height), movementMax, maxPath);
            if (path.Count < 1)
            {
                stuck = true;
                Idle();
            }
        }

        /// <summary>
        /// Returns true if the player is either in direct sight or if the player has been seen within "maxChase"'s limit.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="room"></param>
        /// <returns></returns>
        public virtual bool PlayerInSight(Player player, Room room)
        {
            //TEMPTEMPTEMPTEMPTEMPTEMPTEMPTEMPTEMPTEMPTEMPTEMP
            return true;

            if (chaseTimer <= 0)
                return true;

            for (int i = 0; i < room.platforms.Count; i++)
            {
                if(room.platforms[i].rectangle.Intersects(Game1.RectangleCreate(player.Rectangle.Center, EyePos.ToPoint())))
                    if (Game1.LineIntersectsRect(EyePos, player.Rectangle.Center.ToVector2(), room.platforms[i].rectangle))
                        return false;
            }
            return true;
        }

        public virtual void Attack(Player player, Random rand, Room room)
        {
            state = State.attacking;
        }

        public virtual void Idle()
        {
            movement.X = 0;
            state = State.idle;
        }

        public virtual void Jump(float strength)
        {
            movement.Y = strength;
        }

        public virtual void FallThrough()
        {
            fallThrough = true;
            fallThroughCooldown = 5;
        }



        public class Test : Enemy
        {
            public Test(Vector2 position, Room room, int level, Animation animation, float hitboxDamage, int hitboxCooldown) : base(position, room, level, animation, hitboxDamage, hitboxCooldown)
            {
                movementMax = new Vector2(10, 30);
                walkingSpeed = 7;
            }
        }
    }
}