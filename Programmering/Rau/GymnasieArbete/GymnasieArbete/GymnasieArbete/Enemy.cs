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
        public float damage = 1;
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
            stall,
            jump,
            jumping,
            fall,
            idle,
            attacking
        };

        State state = State.idle;
        List<Instruction> path = new List<Instruction>();
        public float range;

        public bool executingInstruction = false;
        public bool stuck = false;
        public int fallThroughCooldown = 0;
        private Vector2 eyePosOffset;
        public int maxChase = 600;
        protected bool usePath = true;
        public bool UsePath
        {
            get
            {
                return usePath;
            }
            set
            {
                usePath = value;
                path.Clear();
                executingInstruction = false;
                stuck = false;
                chaseTimer = 0;
            }
        }

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
        public bool jumpPause = false;
        public int jumpTimer = 0;
        public Vector2 jumpVector;
        public float walkingSpeed;


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

        public override void Draw(Point offset, Random rand, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Camera camera, Color color)
        {
            animation.Draw(rand, spriteBatch, camera, new Vector2(Origin.X - animation.Width / 2, Rectangle.Bottom - height) + textureOffset, spriteEffect, 0.5f);
        }

        public override void Update(Room room, Random rand)
        {
            if (room.ID == room.player.room)
            {
                DamageZone_Hitbox.Position = position;
                //Sets fallThrough to false after the cooldown has been set to zero
                if (fallThroughCooldown > 0)
                    fallThroughCooldown--;
                else if (fallThrough)
                    fallThrough = false;

                if (chaseTimer > 0)
                    chaseTimer--;

                if (PlayerInSight(room) && usePath)
                {
                    //Stuck signifies that the enemy can't calculate a viable path to the player
                    //If the player or the enemy changes platform the conditions has changed and it should try again
                    if (stuck && (room.player.platformID != room.player.platformIDPrev || platformID != platformIDPrev))
                    {
                        stuck = false;
                    }

                    //Keeping the x-velocity while walking/jumping, etc.
                    if (executingInstruction)
                        movement.X = movementPrev.X;

                    //If the bot is located on the same platform as the room.player is/was
                    if (platformID == room.player.platformIDPrev)
                    {
                        //The path is no longer necessary and should be cleared.
                        //No instruction is being executed and until further notice the state is "Idle"
                        path.Clear();
                        executingInstruction = false;
                        Idle();
                        //If the room.player is outside the enemy's range it will move left/right to get closer
                        if (Game1.GetDistance(Origin, room.player.Origin) > range)
                        {
                            if (room.player.position.X > position.X)
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
                        //If the room.player is in range of the enemy it should attack
                        else
                        {
                            Attack(rand, room);
                        }
                    }
                    //If the room.player is on a different platform we need to get or follow a path
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
                                        GetPath(room);
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
                                state = path[0].state;
                                if (state == State.jump)
                                {
                                    if (!jumpPause )
                                    {
                                        jumpVector = path[0].movement;
                                        jumpPause = true;
                                        jumpTimer = 0;
                                    }
                                    else if (jumpTimer >= 30)
                                    {
                                        executingInstruction = true;
                                        jumpPause = false;
                                        jumpTimer = 0;
                                        Jump(jumpVector.Y, jumpVector.X);
                                    }
                                }
                                else if (state == State.walking)
                                {
                                    movement.X = walkingSpeed * (float)(path[0].movement.X / Math.Abs(path[0].movement.X));
                                    executingInstruction = true;
                                }
                                else if (state == State.fall)
                                {
                                    FallThrough();
                                }

                                if (movement.X < 0)
                                    direction = -1;
                                else if (movement.X > 0)
                                    direction = 1;


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
                            GetPath(room);
                        }
                    }
                    //You can't create a path if you're not on a platform
                    else if (platformID != -1)
                    {
                        //The enemy needs a path and doesn't have one
                        GetPath(room);
                    }
                }
                else
                {
                    Idle();
                }

                if (onGround && (state == State.jumping || state == State.fall) && !jumpPause)
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
                base.Update(room, rand);
            }
        }

        /// <summary>
        /// Gets a path or sets stuck to true depending on the succes of the pathfinding. 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="room"></param>
        public virtual void GetPath(Room room)
        {
            if (room.player.room == room.ID)
            {
                int targetPlatform = room.player.platformID;
                if (room.player.platformID == -1)
                    targetPlatform = room.player.platformIDPrev;

                path = room.GetPathInstructions(platformID, room.player.platformIDPrev, new Point(Rectangle.Width, Rectangle.Height), movementMax, maxPath);
                if (path.Count < 1)
                {
                    stuck = true;
                    Idle();
                }
            }
        }

        /// <summary>
        /// Returns true if the room.player is either in direct sight or if the room.player has been seen within "maxChase"'s limit.
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public virtual bool PlayerInSight(Room room)
        {
            if (room.player.room != room.ID)
                return false;

            if (chaseTimer <= 0)
                return true;

            for (int i = 0; i < room.platforms.Count; i++)
            {
                if (room.platforms[i].rectangle.Intersects(Game1.CreateRectangle(room.player.Rectangle.Center, EyePos.ToPoint())))
                    if (Game1.LineIntersectsRect(EyePos, room.player.Rectangle.Center.ToVector2(), room.platforms[i].rectangle))
                        return false;
            }
            return true;
        }

        public virtual void Attack(Random rand, Room room)
        {
            state = State.attacking;
        }

        public virtual void Idle()
        {
            movement.X = 0;
            state = State.idle;
        }

        public virtual void Jump(float strength, float speed)
        {
            movement.X = speed;
            movement.Y = strength;
            state = State.jumping;
        }

        public virtual void FallThrough()
        {
            fallThrough = true;
            fallThroughCooldown = 5;
        }

        public override void Die(Room room, Random rand)
        {
            int total = (int)(Level + rand.NextDouble() * Level);
            if (total > 100)
            {
                for (int i = 0; i < Math.Floor((decimal)total / 100); i++)
                {
                    room.scraps.Add(new Scrap(new Rectangle((int)position.X + Rectangle.Width / 2, (int)position.Y, SpriteHandler.sprites["Gear3"].width, SpriteHandler.sprites["Gear3"].height), 1, room, "Gear3", new Vector2(rand.Next(-10, 10), rand.Next(-7, -1)), 100, 300, rand));

                }
                total -= 100 * (int)Math.Floor((decimal)total / 100);
            }
            if (total > 10)
            {
                for (int i = 0; i < Math.Floor((decimal)total / 10); i++)
                {
                    room.scraps.Add(new Scrap(new Rectangle((int)position.X + Rectangle.Width / 2, (int)position.Y, SpriteHandler.sprites["Gear2"].width, SpriteHandler.sprites["Gear2"].height), 1, room, "Gear2", new Vector2(rand.Next(-10, 10), rand.Next(-7, -1)), 10, 300, rand));
                }
                total -= 10 * (int)Math.Floor((decimal)total / 10);
            }
            if (total > 0)
            {
                for (int i = 0; i < total; i++)
                {
                    room.scraps.Add(new Scrap(new Rectangle((int)position.X + Rectangle.Width / 2, (int)position.Y, SpriteHandler.sprites["Gear1"].width, SpriteHandler.sprites["Gear1"].height), 1, room, "Gear1", new Vector2(rand.Next(-10, 10), rand.Next(-7, -1)), 1, 300, rand));
                }
            }

            base.Die(room, rand);

        }

        public class Empty : Enemy
        {
            public Empty() : base(Vector2.Zero, null, 0, null, 0, 0)
            {
            }

            public override void Update(Room room, Random rand)
            {
                return;
            }

            public override void Draw(Point offset, Random rand, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Camera camera, Color color)
            {
                return;
            }
        }

        public class Test : Enemy
        {
            public Test(Vector2 position, Room room, int level, Animation animation, float hitboxDamage, int hitboxCooldown) : base(position, room, level, animation, hitboxDamage, hitboxCooldown)
            {
                movementMax = new Vector2(7, 35);
            }
        }
        public class GroundTroop : Enemy
        {
            public string spr_walking, spr_idle, spr_shooting, spr_charging, spr_bullet, spr_chargedBullet, spr_fall, spr_jump, spr_jumping;
            public Animation walking, idle, shooting, charging;
            public bool canAttack;
            bool charge = false;
            int chargingTimer = 0;
            float chargingCap = 60 * 1.5f;
            int chargeTimer = 0;
            float chargeCooldown = 60 * 5;
            int attackTimer = 0;
            int shootingTimer = 0;
            float shootingSpeedSprite = 0;
            bool dropLoot = false;



            public class GroundTroopBullet : Projectile
            {

                public GroundTroopBullet(int ID, Animation ani, Vector2 pos, Vector2 vel, float damage, float knockback, float lifespan, Room room, bool friendly, bool useFriction, bool useGravity, bool useResistance, float bounceFactor, bool dieOnCollision, SpriteEffects spriteEffect, List<GameObject.Types> attackable, bool lookAtMovement) : base(ID, ani, pos, vel, damage, knockback, lifespan, room, friendly, useFriction, useGravity, useResistance, 0f, true, spriteEffect, attackable, lookAtMovement)
                {

                }
                public override void Update(Room room, Random rand)
                {
                    base.Update(room, rand);
                    //Rectangle = new Rectangle((int)position.X + 90, (int)position.Y, 15, 10);
                }
            }
            public GroundTroop(Random rand, Vector2 pos, Room room, int level, Animation ani, float HitboxDamage, int HitboxCooldown) : base(pos, room, level, ani, HitboxDamage, HitboxCooldown)
            {
                movementMax = new Vector2(15, 35);
                walkingSpeed = 7;
                Level = level;
                hp = 1;
                for (int i = 1; i < Level; i++)
                {
                    damage += 1 * Level;
                    //damage += rand.Next((int)-damage / 5, (int)damage / 5);
                }
                float temp = 1;
                for (int i = 0; i < Level; i++)
                {
                    hp += 3 * Level;
                    hp += rand.Next((int)(-hp / 3), (int)(hp / 3));
                    temp += 0.05f;
                }
                hp *= temp;
                hp *= rand.Next(1, 3);
                range = 600;
                position = pos;
                canAttack = false;
                spr_walking = "GroundTroop_Walking";
                spr_idle = "GroundTroop_Idle";
                spr_shooting = "GroundTroop_Shooting";
                spr_jump = "GroundTroop_Jump";
                spr_jumping = "GroundTroop_Jump";
                spr_fall = "GroundTroop_Fall";
                spr_chargedBullet = "GroundTroop_ChargedBullet";
                spr_charging = "GroundTroop_Charging";
                spr_bullet = "GroundTroop_Bullet";
                walking = new Animation(spr_walking);
                idle = new Animation(spr_idle);
                shooting = new Animation(spr_shooting);
                charging = new Animation(spr_charging);
                shootingSpeedSprite = shooting.speed;
            }

            /// <summary>
            /// Returns true if the player is either in direct sight or if the player has been seen within "maxChase"'s limit.
            /// </summary>
            /// <param name="player"></param>
            /// <param name="room"></param>
            /// <returns></returns>
            public bool PlayerInSight1(Room room)
            {
                if (room.player.room != room.ID)
                    return false;

                for (int i = 0; i < room.platforms.Count; i++)
                {
                    if (room.platforms[i].rectangle.Intersects(Game1.CreateRectangle(room.player.Rectangle.Center, Origin.ToPoint())))
                        if (Game1.LineIntersectsRect(Origin, room.player.Rectangle.Center.ToVector2(), room.platforms[i].rectangle))
                            return false;
                }
                if (Game1.LineIntersectsRect(Origin, new Vector2(room.player.Rectangle.Center.ToVector2().X, Origin.Y), room.player.Rectangle))
                {
                    return true;
                }
                else
                    return false;
            }

            public override void Attack(Random rand, Room room)
            {
                canAttack = true;
            }

            public override void Update(Room room, Random rand)
            {
                if (room.ID == room.player.room)
                {
                    base.Update(room, rand);
                    if (!jumpPause)
                    {
                        //base.Update(room, rand);

                        if (!stunned)
                        {
                            if (canAttack && state != State.charging && state != State.shooting && this.PlayerInSight1(room))
                            {
                                if (room.player.position.X > position.X)
                                {
                                    direction = 1;
                                }
                                else
                                {
                                    direction = -1;
                                }
                            }
                            DamageZone_Hitbox.Position = position;
                            if (canAttack && !charge && rand.Next(0, 1500) <= 10 && this.PlayerInSight1(room))
                            {
                                charge = true;
                                shootingTimer = 0;
                                UsePath = false;
                            }
                            else if (canAttack && !charge && shootingTimer >= 60 && this.PlayerInSight1(room))
                            {
                                shootingTimer = 0;
                                state = State.shooting;
                                room.gameObjects.Add(new Projectile(room.gameObjects.Count, new Animation(spr_bullet), Origin + new Vector2(50 * (direction), 10), new Vector2(10 * direction, 0), damage / 10, 2, 1, room, false, false, false, false, 0f, true, spriteEffect, attackable, true));
                            }

                            if (charge)
                            {
                                movement.X = 0;
                                if (chargingTimer <= chargingCap)
                                {
                                    state = State.charging;
                                    chargingTimer++;
                                    // if (chargeTimer % 60 == 0) { charging.speed += 0.05f; }
                                }
                                else
                                {
                                    state = State.shooting;
                                    shootingTimer++;
                                    if (shootingTimer % 30 == 0)
                                    {
                                        room.gameObjects.Add(new Projectile(room.gameObjects.Count, new Animation(spr_chargedBullet), Origin + new Vector2(50 * (direction), 10), new Vector2(25 * direction, 0), damage / 11, 2, 1, room, false, false, false, false, 0f, true, spriteEffect, attackable, true));
                                        shooting.currentFrame = 0;
                                        shooting.speed = shootingSpeedSprite;
                                        if (shootingTimer >= 90)
                                        {
                                            charge = false;
                                            UsePath = true;
                                            chargingTimer = 0;
                                            shootingTimer = 0;
                                        }
                                    }
                                }
                            }
                            else { if (!charge) shootingTimer++; }
                        }
                    }
                    else
                    {
                        jumpTimer++;
                    }
                }
            }
            public override void Draw(Point offset, Random rand, SpriteBatch spriteBatch, GraphicsDevice graphics, Camera camera, Color color)
            {
                spriteBatch.DrawString(Game1.fontDebug, hp.ToString(), new Vector2(position.X, position.Y - 20), Color.Red);
                if (direction == -1)
                { spriteEffect = SpriteEffects.FlipHorizontally; }
                else
                {
                    spriteEffect = SpriteEffects.None;
                }
                Vector2 drawPos = new Vector2(Origin.X - idle.Width / 2, Rectangle.Bottom - height) + textureOffset;
                switch (state)
                {
                    case State.idle:
                        idle.Draw(rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                        break;
                    case State.walking:
                        walking.Draw(rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                        break;
                    case State.charging:
                        charging.Draw(rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                        break;
                    case State.shooting:
                        shooting.Draw(rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                        break;
                    case State.jump:
                        SpriteHandler.Draw(spr_jump, rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                        break;
                    case State.jumping:
                        SpriteHandler.Draw(spr_jumping, rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                        break;
                    case State.fall:
                        SpriteHandler.Draw(spr_fall, rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                        break;
                    default:
                        break;
                }
                if (shooting.animationEnd) { shooting.speed = 0; }

            }
        }

        public class BlizzardBot : Enemy
        {
            public string spr_walking, spr_idle, spr_shooting, spr_charging, spr_snow, spr_fall, spr_jumping, spr_jump, spr_stall;
            public Animation walking, idle, shooting, charging, jumping;
            public bool canAttack;
            public int snowCharge;


            bool dropLoot = false;
            public BlizzardBot(Random rand, Vector2 pos, Room room, int level, Animation ani, float HitboxDamage, int HitboxCooldown) : base(pos, room, level, ani, HitboxDamage, HitboxCooldown)
            {
                movementMax = new Vector2(15, 35);
                walkingSpeed = 7;
                Level = level;
                hp = 1;
                float temp = 1;
                for (int i = 0; i < Level; i++)
                {
                    hp += 3 * Level;
                    hp += rand.Next((int)(-hp / 3), (int)(hp / 3));
                    temp += 0.05f;
                }
                hp *= temp;
                hp *= rand.Next(1, 3);
                range = 600;
                position = pos;
                canAttack = false;
                spr_walking = "BlizzardBot_Jump";
                spr_idle = "BlizzardBot_Jump";
                spr_charging = "BlizzardBot_Jump";
                walking = new Animation(spr_walking);
                idle = new Animation(spr_idle);
                charging = new Animation(spr_charging);
                spr_walking = "BlizzardBot_Walking";
                //spr_idle = "BlizzardBot_Idle";
                spr_shooting = "BlizzardBot_Spray";
                spr_jumping = "BlizzardBot_Fall";
                spr_jump = "BlizzardBot_Jump";
                spr_stall = "BlizzardBot_Stall";
                spr_fall = "BlizzardBot_Fall";
                //spr_charging = "BlizzardBot_Charging";
                spr_snow = "BlizzardBot_Snow";
                walking = new Animation(spr_walking);
                //idle = new Animation(spr_idle);
                shooting = new Animation(spr_shooting);
                jumping = new Animation(spr_jumping);
                //charging = new Animation(spr_charging);
            }

            public class Snow : Particle.Dynamic
            {

                bool switchDir = false;
                float dir = 1;
                public Snow(Rectangle rectangle, int level, Room newRoom, Vector2 movement, Vector2 deAcceleration, string spriteKey, bool pixel, Color color, float scale, int duration, bool impact, bool additive) : base(rectangle, level, newRoom, movement, deAcceleration, spriteKey, pixel, color, scale, duration, impact, additive, true)
                {

                }

                public override void Update(Room room, Random rand)
                {

                    base.Update(room, rand);

                    if (rand.Next(0, 200) <= 5) { switchDir = true; }
                    if (switchDir)
                    {
                        dir *= -1;
                        switchDir = false;
                    }
                    if (!onGround) { movement.X += (0.3f * dir); }
                    if (movement.Y > 0)
                    {
                        if (movement.Y <= 5)
                            movement.Y += 0.2f;
                        else
                            movement.Y = 5;
                    }

                    if (Rectangle.Intersects(room.player.Rectangle))
                    {
                        live = false;
                        remove = true;

                        room.player.Buffs.Add(new Buff(1f, 0.4f, 1f, 30, "SnowSlow", false));
                    }
                    else if (rand.Next(0, 1000) < 20 && !onGround)
                    {
                        live = false;
                        remove = true;
                    }
                }
            }

            public override void Attack(Random rand, Room room)
            {
                base.Attack(rand, room);
            }

            public void SpewSnow(Random rand, Room room)
            {
                for (int i = 0; i < 5; i++)
                {
                    room.gameObjects.Add(new Snow(new Rectangle((int)position.X + SpriteHandler.sprites[spr_shooting].width / 2, (int)position.Y, SpriteHandler.sprites[spr_snow].width, SpriteHandler.sprites[spr_snow].height), 1, room, new Vector2(rand.Next(-90, 90), rand.Next(-80, -10)), new Vector2(0.95f, 0.95f), spr_snow, false, Color.White, 1f, 60 * 20, false, false));
                }
            }

            public override void Update(Room room, Random rand)
            {
                if (room.ID == room.player.room)
                {
                    if (!jumpPause)
                    {
                        base.Update(room, rand);
                        snowCharge++;
                        if (snowCharge >= 300)
                        {
                            if (rand.Next(0, 100) <= 1)
                            {
                                canAttack = true;
                            }
                        }
                    }
                    else
                    {
                        if (jumpTimer <= 0)
                        {
                            jumping.currentFrame = 0;
                            jumping.speed = 0.1f;
                        }
                        jumpTimer++;
                        if (jumping.animationEnd) { jumping.speed = 0; }
                        if (jumpTimer >= 30) { base.Update(room, rand); }
                    }
                    if (canAttack)
                    {
                        if (snowCharge >= 300)
                            snowCharge = 0;
                        if (snowCharge <= 180)
                        {
                            SpewSnow(rand, room);
                        }
                        else
                        {
                            canAttack = false;
                            snowCharge = 0;
                        }
                    }
                }
            }

            public override void Draw(Point offset, Random rand, SpriteBatch spriteBatch, GraphicsDevice graphics, Camera camera, Color color)
            {
                if (direction == -1)
                { spriteEffect = SpriteEffects.FlipHorizontally; }
                else
                {
                    spriteEffect = SpriteEffects.None;
                }
                Vector2 drawPos = new Vector2(Origin.X - idle.Width / 2, Rectangle.Bottom - height) + textureOffset;
                switch (state)
                {
                    case State.idle:
                        idle.Draw(rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                        break;
                    case State.walking:
                        walking.Draw(rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                        break;
                    case State.charging:
                        charging.Draw(rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                        break;
                    case State.attacking:
                        shooting.Draw(rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                        break;
                    case State.jump:
                        SpriteHandler.Draw(spr_jump, rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                        break;
                    case State.jumping:
                        jumping.Draw(rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                        break;
                    case State.fall:
                        SpriteHandler.Draw(spr_fall, rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                        break;
                    case State.stall:
                        SpriteHandler.Draw(spr_fall, rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                        break;
                    default:
                        break;
                }
                if (shooting.animationEnd) { shooting.speed = 0; }
            }
        }
        public class FireBot : Enemy
        {
            public string spr_walking, spr_idle, spr_shooting, spr_charging, spr_fire, spr_fall, spr_jump, spr_jumping;
            public Animation walking, idle, shooting, charging;
            public Vector2 shootLeft, shootUp, shootRight;
            public bool canAttack;
            public bool firstTime = true;
            public int counter = 0;

            bool dropLoot = false;
            public FireBot(Random rand, Vector2 pos, Room room, int level, Animation ani, float HitboxDamage, int HitboxCooldown) : base(pos, room, level, ani, HitboxDamage, HitboxCooldown)
            {
                movementMax = new Vector2(15, 35);
                walkingSpeed = 7;
                Level = level;
                hp = 1;
                float temp = 1;
                for (int i = 0; i < Level; i++)
                {
                    hp += 3 * Level;
                    hp += rand.Next((int)(-hp / 3), (int)(hp / 3));
                    temp += 0.05f;
                }
                hp *= temp;
                hp *= rand.Next(1, 3);
                range = 600;
                position = pos;
                canAttack = false;
                spr_walking = "GroundTroop_Walking";
                spr_idle = "GroundTroop_Idle";
                spr_shooting = "GroundTroop_Shooting";
                spr_jump = "GroundTroop_Jump";
                spr_fall = "GroundTroop_Fall";
                spr_charging = "GroundTroop_Charging";
                walking = new Animation(spr_walking);
                idle = new Animation(spr_idle);
                shooting = new Animation(spr_shooting);
                charging = new Animation(spr_charging);
                //spr_walking = "FireBot_Walking";
                //spr_idle = "FireBot_Idle";
                //spr_shooting = "FireBot_Shooting";
                //spr_jump = "FireBot_Jump";
                //spr_fall = "FireBot_Fall";
                //spr_charging = "FireBot_Charging";
                //spr_fire = "FireBot_Snow";
                //walking = new Animation(spr_walking);
                //idle = new Animation(spr_idle);
                //shooting = new Animation(spr_shooting);
                //charging = new Animation(spr_charging);

            }

            public override void Attack(Random rand, Room room)
            {
                base.Attack(rand, room);
            }

            public void SpewFire(Random rand, Room room)
            {
                for (int i = 0; i < 3; i++)
                {
                    room.particles.Add(new Particle.Fire(room.particles.Count, room, rand, shootLeft, new Vector2(rand.Next(-20, -4), rand.Next(-4, 4)), 1f, 1f, rand.Next(5, 20), Color.Firebrick));
                }
                for (int i = 0; i < 3; i++)
                {
                    room.particles.Add(new Particle.Fire(room.particles.Count, room, rand, shootUp, new Vector2(rand.Next(-4, 4), rand.Next(-20, -4)), 1f, 1f, rand.Next(5, 20), Color.Firebrick));
                }
                for (int i = 0; i < 3; i++)
                {
                    room.particles.Add(new Particle.Fire(room.particles.Count, room, rand, shootRight, new Vector2(rand.Next(4, 20), rand.Next(-4, 4)), 1f, 1f, rand.Next(5, 20), Color.Firebrick));
                }
            }

            public override void Update(Room room, Random rand)
            {
                if (!jumpPause)
                {
                    base.Update(room, rand);
                    shootLeft = new Vector2(position.X, position.Y + (SpriteHandler.sprites[spr_shooting].height / 2));
                    shootUp = new Vector2(position.X + (SpriteHandler.sprites[spr_shooting].width / 2), position.Y);
                    shootRight = new Vector2(position.X + (SpriteHandler.sprites[spr_shooting].width), position.Y);
                    counter++;
                    if (counter >= 100 && counter <= 250)
                    {
                        UsePath = false;
                        SpewFire(rand, room);
                        if (firstTime)
                        {
                            room.damageZones.Add(new DamageZone(new Rectangle((int)shootLeft.X - 200, (int)shootLeft.Y, 200, 100), 10, 60, 150, room.damageZones.Count, false, true, attackable));
                            room.damageZones.Add(new DamageZone(new Rectangle((int)shootUp.X - (SpriteHandler.sprites[spr_shooting].width / 2), (int)shootUp.Y - 200, 100, 200), 10, 60, 150, room.damageZones.Count, false, true, attackable));
                            room.damageZones.Add(new DamageZone(new Rectangle((int)shootLeft.X + SpriteHandler.sprites[spr_shooting].width, (int)shootLeft.Y, 200, 100), 10, 60, 150, room.damageZones.Count, false, true, attackable));
                            firstTime = false;
                        }
                    }
                }
                else
                {
                    jumpTimer++;
                }
            }
            public override void Draw(Point offset, Random rand, SpriteBatch spriteBatch, GraphicsDevice graphics, Camera camera, Color color)
            {
                if (direction == -1)
                { spriteEffect = SpriteEffects.FlipHorizontally; }
                else
                {
                    spriteEffect = SpriteEffects.None;
                }
                Vector2 drawPos = new Vector2(Origin.X - idle.Width / 2, Rectangle.Bottom - height) + textureOffset;
                switch (state)
                {
                    case State.idle:
                        idle.Draw(rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                        break;
                    case State.walking:
                        walking.Draw(rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                        break;
                    case State.charging:
                        charging.Draw(rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                        break;
                    case State.shooting:
                        shooting.Draw(rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                        break;
                    case State.jump:
                        SpriteHandler.Draw(spr_jump, rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                        break;
                    case State.jumping:
                        SpriteHandler.Draw(spr_jumping, rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                        break;
                    case State.fall:
                        SpriteHandler.Draw(spr_fall, rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                        break;
                    default:
                        break;
                }
                if (shooting.animationEnd) { shooting.speed = 0; }
            }
        }

        public class GeminiBot : Enemy
        {
            public string spr_idle, spr_walkLeft, spr_walkRight, spr_shootLeft, spr_shootRight, spr_shootRight_Broken, spr_shootLeft_Broken, spr_destroyRight, spr_destroyLeft, spr_dieRight, spr_dieLeft;
            public Animation walkLeft, walkRight, shootLeft, shootRight, shootRight_Broken, shootLeft_Broken, destroyRight, destroyLeft, dieRight, dieLeft;
            public bool broken;
            public bool leftBroken;
            public bool rightBroken;
            public bool canAttack;
            public float shootingTimer;
            public float leftHp;
            public float rightHp;
            public bool leftDestroy;
            public bool rightDestroy;
            public Vector2 shootLeftOrigin;
            public Vector2 shootRightOrigin;
            public Vector2 shootLeftOrigin_Broken;
            public Vector2 shootRightOrigin_Broken;

            bool dropLoot = false;
            public GeminiBot(Random rand, Vector2 pos, Room room, int level, Animation ani, float HitboxDamage, int HitboxCooldown) : base(pos, room, level, ani, HitboxDamage, HitboxCooldown)
            {
                shootLeftOrigin = new Vector2(7, 27);
                shootRightOrigin = new Vector2(127, 27);
                shootLeftOrigin_Broken = new Vector2(20, 12);
                shootRightOrigin_Broken = new Vector2(103, 12);
                broken = false;
                leftDestroy = false;
                rightDestroy = false;
                rightBroken = false;
                leftBroken = false;
                movementMax = new Vector2(15, 35);
                walkingSpeed = 7;
                Level = level;
                shootingTimer = 0;
                leftHp = 1;
                float temp = 1;
                for (int i = 0; i < Level; i++)
                {
                    leftHp += 3 * Level;
                    leftHp += rand.Next((int)(-hp / 3), (int)(hp / 3));
                    temp += 0.05f;
                }
                leftHp *= temp;
                leftHp *= rand.Next(3, 8);
                rightHp = 1;
                float temp1 = 1;
                for (int i = 0; i < Level; i++)
                {
                    rightHp += 3 * Level;
                    rightHp += rand.Next((int)(-hp / 3), (int)(hp / 3));
                    temp += 0.05f;
                }
                rightHp *= temp;
                rightHp *= rand.Next(1, 3);
                leftHp /= 3;
                rightHp /= 3;
                range = 1000;
                position = pos;
                canAttack = false;
                spr_idle = "GeminiBot_Idle";
                spr_walkLeft = "GeminiBot_WalkLeft";
                spr_walkRight = "GeminiBot_WalkRight";
                spr_shootLeft = "GeminiBot_ShootLeft";
                spr_shootRight = "GeminiBot_ShootRight";
                spr_shootLeft_Broken = "GeminiBot_ShootLeft_Broken";
                spr_shootRight_Broken = "GeminiBot_ShootRight_Broken";
                spr_dieLeft = "GeminiBot_DieLeft";
                spr_dieRight = "GeminiBot_DieRight";
                spr_destroyLeft = "GeminiBot_DestroyLeft";
                spr_destroyRight = "GeminiBot_DestroyRight";
                walkLeft = new Animation(spr_walkLeft);
                walkRight = new Animation(spr_walkRight);
                shootLeft = new Animation(spr_shootLeft);
                shootRight = new Animation(spr_shootRight);
                dieLeft = new Animation(spr_dieLeft);
                dieRight = new Animation(spr_dieRight);
                destroyLeft = new Animation(spr_destroyLeft);
                destroyRight = new Animation(spr_destroyRight);
                shootLeft_Broken = new Animation(spr_shootLeft_Broken);
                shootRight_Broken = new Animation(spr_shootRight_Broken);

            }
            public override void Attack(Random rand, Room room)
            {
                canAttack = true;
            }
            public override void Update(Room room, Random rand)
            {
                if (room.ID == room.player.room)
                {
                    if (!jumpPause)
                    {
                        if (!broken)
                            base.Update(room, rand);


                        if (dieRight.animationEnd || dieLeft.animationEnd)
                        {
                            Die(room, rand);
                        }
                        else
                        {
                            if (destroyLeft.animationEnd)
                            {
                                leftBroken = true;
                                broken = true;
                            }
                            else if (destroyRight.animationEnd)
                            {
                                rightBroken = true;
                                broken = true;
                            }
                            if (!broken)
                            {
                                if (canAttack && shootingTimer > 60)
                                {
                                    state = State.attacking;
                                    if ((shootLeft.currentFrame == shootLeft.framesTotal - 1 || shootRight.currentFrame == shootRight.framesTotal - 1))
                                    {
                                        if (direction > 0)
                                        {
                                            room.gameObjects.Add(new Projectile(room.gameObjects.Count, new Animation("GroundTroop_Bullet"), position + shootRightOrigin, Game1.GetVector2(Game1.GetAngle(position + shootRightOrigin, room.gameObjects[0].position)) * new Vector2(15, 15), damage, 2, 200, room, false, false, false, false, 0, true, spriteEffect, attackable, true));
                                        }
                                        else
                                        {
                                            room.gameObjects.Add(new Projectile(room.gameObjects.Count, new Animation("GroundTroop_Bullet"), position + shootLeftOrigin, Game1.GetVector2(Game1.GetAngle(position + shootLeftOrigin, room.gameObjects[0].position)) * new Vector2(-15, -15), damage, 2, 200, room, false, false, false, false, 0, true, spriteEffect, attackable, true));
                                        }
                                        shootingTimer = 0;
                                    }
                                }
                            }
                            if (broken)
                            {
                                state = State.shooting;
                                if (canAttack && shootingTimer > 20)
                                {
                                    if ((shootLeft.currentFrame == shootLeft.framesTotal - 1 || shootRight.currentFrame == shootRight.framesTotal - 1))
                                    {
                                        if (leftBroken)
                                        {
                                            room.gameObjects.Add(new Projectile(room.gameObjects.Count, new Animation("GroundTroop_Bullet"), position + shootRightOrigin_Broken, Game1.GetVector2(Game1.GetAngle(position + shootRightOrigin_Broken, room.gameObjects[0].position)) * new Vector2(15, 15), damage, 2, 200, room, false, false, false, false, 0, true, spriteEffect, attackable, true));
                                        }
                                        if (rightBroken)
                                        {
                                            room.gameObjects.Add(new Projectile(room.gameObjects.Count, new Animation("GroundTroop_Bullet"), position + shootLeftOrigin_Broken, Game1.GetVector2(Game1.GetAngle(position + shootLeftOrigin_Broken, room.gameObjects[0].position)) * new Vector2(-15, -15), damage, 2, 200, room, false, false, false, false, 0, true, spriteEffect, attackable, true));
                                        }
                                        shootingTimer = 0;
                                    }
                                }
                            }
                            shootingTimer++;
                        }
                    }
                    else
                    {
                        jumpTimer++;
                    }
                }
            }
            public override void TakeDamage(Room room, int sourceID, float damage)
            {
                if (room.gameObjects[sourceID].positionPrev.X >= positionPrev.X + Rectangle.Width / 2 && rightHp >= -1)
                {
                    rightHp -= (int)Math.Round(damage);
                    if (World.displayDamageGameObjects)
                        room.effectObjects.Add(new EffectObject(position, movement, World.fontDamage, ((int)Math.Round(damage)).ToString(), 120));
                }
                if (room.gameObjects[sourceID].positionPrev.X <= positionPrev.X + Rectangle.Width / 2 && leftHp >= -1)
                {
                    leftHp -= (int)Math.Round(damage);
                    if (World.displayDamageGameObjects)
                        room.effectObjects.Add(new EffectObject(position, movement, World.fontDamage, ((int)Math.Round(damage)).ToString(), 120));
                }
            }

            public override void Draw(Point offset, Random rand, SpriteBatch spriteBatch, GraphicsDevice graphics, Camera camera, Color color)
            {
                spriteEffect = SpriteEffects.None;
                Vector2 drawPos = new Vector2(Origin.X - walkLeft.Width / 2, Rectangle.Bottom - height) + textureOffset;
                switch (state)
                {
                    case State.walking:
                        if (leftHp < 0)
                        {
                            destroyLeft.Draw(rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                        }
                        else if (rightHp < 0)
                        {
                            destroyRight.Draw(rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                        }
                        else if (direction > 0)
                            walkRight.Draw(rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                        else
                            walkLeft.Draw(rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                        break;

                    case State.attacking:
                        if (!broken)
                        {
                            if (leftHp < 0)
                            {
                                destroyLeft.Draw(rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                            }
                            else if (rightHp < 0)
                            {
                                destroyRight.Draw(rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                            }
                            else if (direction > 0)
                                shootLeft.Draw(rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                            else
                                shootRight.Draw(rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);

                        }
                        else
                        {
                            if (leftBroken && rightHp < 0)
                            {
                                dieRight.Draw(rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                            }
                            else if (rightBroken && leftHp < 0)
                            {
                                dieLeft.Draw(rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                            }
                            else if (rightBroken)
                                shootLeft_Broken.Draw(rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                            else if (leftBroken)
                                shootRight_Broken.Draw(rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                        }
                        break;
                    case State.shooting:
                        if (!broken)
                        {
                            if (leftHp < 0)
                            {
                                destroyLeft.Draw(rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                            }
                            else if (rightHp < 0)
                            {
                                destroyRight.Draw(rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                            }
                            else if (direction > 0)
                                shootLeft.Draw(rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                            else
                                shootRight.Draw(rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);

                        }
                        else
                        {
                            if (leftBroken && rightHp < 0)
                            {
                                dieRight.Draw(rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                            }
                            else if (rightBroken && leftHp < 0)
                            {
                                dieLeft.Draw(rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                            }
                            else if (direction > 0)
                                shootLeft_Broken.Draw(rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                            else
                                shootRight_Broken.Draw(rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                        }
                        break;
                    case State.idle:
                        SpriteHandler.Draw(spr_idle, rand, spriteBatch, camera, drawPos, spriteEffect, 0.5f);
                        break;
                    case State.jumping:
                        SpriteHandler.Draw(spr_idle, rand, spriteBatch, camera, drawPos, spriteEffect, 0.5f);
                        break;
                    case State.jump:
                        SpriteHandler.Draw(spr_idle, rand, spriteBatch, camera, drawPos, spriteEffect, 0.5f);
                        break;
                    default:
                        break;
                }
            }
        }
        public class LavaBot : Enemy
        {
            public string spr_walking, spr_idle, spr_shooting, spr_charging, spr_lava, spr_fall, spr_jumping, spr_jump, spr_stall;
            public Animation walking, idle, shooting, charging, jumping;
            public bool canAttack;
            public int lavaCharge;

            bool dropLoot = false;
            public LavaBot(Random rand, Vector2 pos, Room room, int level, Animation ani, float HitboxDamage, int HitboxCooldown) : base(pos, room, level, ani, HitboxDamage, HitboxCooldown)
            {

                movementMax = new Vector2(15, 35);
                walkingSpeed = 7;
                lavaCharge = 150;
                Level = level;
                hp = 1;
                for (int i = 0; i < Level; i++)
                {
                    damage += 1 * Level;
                    //damage += rand.Next((int)-damage / 5, (int)damage / 5);
                }
                float temp = 1;
                for (int i = 0; i < Level; i++)
                {
                    hp += 3 * Level;
                    hp += rand.Next((int)(-hp / 10), (int)(hp / 10));
                    temp += 0.05f;
                }
                hp *= temp;
                hp *= rand.Next(1, 2);
                range = 600;
                position = pos;
                canAttack = false;
                spr_walking = "LavaBot_Jump";
                spr_idle = "LavaBot_Jump";
                spr_charging = "LavaBot_Jump";
                walking = new Animation(spr_walking);
                idle = new Animation(spr_idle);
                charging = new Animation(spr_charging);
                spr_walking = "LavaBot_Walking";
                //spr_idle = "LavaBot_Idle";
                spr_shooting = "LavaBot_Spray";
                spr_jumping = "LavaBot_Fall";
                spr_jump = "LavaBot_Jump";
                spr_stall = "LavaBot_Stall";
                spr_fall = "LavaBot_Fall";
                //spr_charging = "LavaBot_Charging";
                spr_lava = "LavaBot_Lava";
                walking = new Animation(spr_walking);
                //idle = new Animation(spr_idle);
                shooting = new Animation(spr_shooting);
                jumping = new Animation(spr_jumping);
                //charging = new Animation(spr_charging);
            }

            public class Lava : Particle.Dynamic
            {
                float dir = 1;
                float dmgCounter = 0;
                float Damage;
                bool first;
                int timeCounter = 0;
                int time;
                public Lava(Rectangle rectangle, int level, Room newRoom, Vector2 movement, Vector2 deAcceleration, string spriteKey, bool pixel, Color color, float scale, int duration, bool impact, bool additive, float damage) : base(rectangle, level, newRoom, movement, deAcceleration, spriteKey, pixel, color, scale, duration, impact, additive, true)
                {
                    Damage = damage;
                    first = true;
                }

                public override void Update(Room room, Random rand)
                {

                    base.Update(room, rand);
                    timeCounter++;
                    if (first)
                    {
                        int randomColor = rand.Next(10, 150);
                        this.color = new Color(255, randomColor, 0);
                        time = rand.Next(180);
                        first = false;

                    }
                    if (timeCounter >= time)
                    {
                        timeCounter = 0;
                        dir *= -1;
                    }
                    if (onGround) { movement.X += (0.4f * dir); }

                    dmgCounter++;

                    if (Rectangle.Intersects(room.player.Rectangle) && dmgCounter >= 60)
                    {
                        room.player.hp -= (Damage / 200);
                        dmgCounter = 0;
                    }
                }
                public override void DrawLight(Point offset, Random rand, SpriteBatch spriteBatch, Camera camera)
                {
                    //SpriteHandler.Draw("gradient", rand, spriteBatch, camera, position, 0.3f, angle, new Vector2(128, 128), color, 0.1f, SpriteEffects.None, 0.5f);
                }
                public override void Draw(Point offset, Random rand, SpriteBatch spriteBatch, GraphicsDevice graphics, Camera camera, Color color)
                {
                    base.Draw(offset, rand, spriteBatch, graphics, camera, this.color);
                }

            }

            public override void Attack(Random rand, Room room)
            {
                base.Attack(rand, room);
            }

            public void SpewLava(Random rand, Room room)
            {
                for (int i = 0; i < 5; i++)
                {
                    room.gameObjects.Add(new Lava(new Rectangle((int)position.X + SpriteHandler.sprites[spr_shooting].width / 2, (int)position.Y, SpriteHandler.sprites[spr_lava].width, SpriteHandler.sprites[spr_lava].height), 1, room, new Vector2(rand.Next(-60, 60), rand.Next(-80, -10)), new Vector2(0.95f, 0.95f), spr_lava, false, Color.White, 1f, 60 * 5, false, false, damage));
                }
            }

            public override void Update(Room room, Random rand)
            {
                if (room.ID == room.player.room)
                {
                    if (!jumpPause)
                    {
                        base.Update(room, rand);

                        lavaCharge++;
                        if (lavaCharge >= 150 && !canAttack)
                        {
                            if (rand.Next(0, 100) <= 1)
                            {
                                lavaCharge = 0;
                                canAttack = true;
                            }
                        }
                        if (canAttack)
                        {
                            if (lavaCharge <= 180)
                            {
                                SpewLava(rand, room);
                            }
                            else
                            {
                                canAttack = false;
                                lavaCharge = 0;
                            }
                        }
                    }
                    else
                    {
                        if (jumpTimer <= 0)
                        {
                            jumping.currentFrame = 0;
                            jumping.speed = 0.1f;
                        }
                        jumpTimer++;
                        if (jumping.animationEnd) { jumping.speed = 0; }
                        if (jumpTimer >= 30) { base.Update(room, rand); }
                    }
                }
            }

            public override void Draw(Point offset, Random rand, SpriteBatch spriteBatch, GraphicsDevice graphics, Camera camera, Color color)
            {
                spriteBatch.DrawString(Game1.fontDebug, hp.ToString(), new Vector2(position.X, position.Y - 20), Color.Red);
                if (direction == -1)
                { spriteEffect = SpriteEffects.FlipHorizontally; }
                else
                {
                    spriteEffect = SpriteEffects.None;
                }
                Vector2 drawPos = new Vector2(Origin.X - idle.Width / 2, Rectangle.Bottom - height) + textureOffset;
                switch (state)
                {
                    case State.idle:
                        idle.Draw(rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                        break;
                    case State.walking:
                        walking.Draw(rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                        break;
                    case State.charging:
                        charging.Draw(rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                        break;
                    case State.attacking:
                        shooting.Draw(rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                        break;
                    case State.jump:
                        SpriteHandler.Draw(spr_jump, rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                        break;
                    case State.jumping:
                        jumping.Draw(rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                        break;
                    case State.fall:
                        SpriteHandler.Draw(spr_fall, rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                        break;
                    case State.stall:
                        SpriteHandler.Draw(spr_fall, rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                        break;
                    default:
                        break;
                }
                if (shooting.animationEnd) { shooting.speed = 0; }
            }
        }

        public static Enemy empty()
        {
            return new Enemy.Empty();
        }
    }
}