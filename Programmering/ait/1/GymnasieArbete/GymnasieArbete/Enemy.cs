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
        public float DamageZone_damage;
        public bool attackPlayer = false;
        public SpriteEffects spriteEffect;
        public Animation animation { get; set; }
        public List<Types> attackable = new List<Types>();
        public Enemy(Vector2 pos, Room room, int level, Animation ani, float HitboxDamage, int HitboxCooldown) : base(new Rectangle((int)pos.X, (int)pos.Y, ani.Width, ani.Height), 0, room, "")
        {
            animation = ani;
            base.sprite = sprite;
            pos = position;
            type = Types.Enemy;

            attackable.Add(Types.Player);
            DamageZone_Hitbox = new DamageZone(Rectangle, HitboxDamage, 60 * HitboxCooldown, 0, ID, false, true, attackable);
            room.damageZones.Add(DamageZone_Hitbox); // Var defineras damagezones?
            Level = level;
        }
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
        //DE NYA VARIABLERNA
        public float range;
        public Vector2 movementMax;
        public bool executingInstruction = false;
        public bool stuck = false;
        public int fallThroughCooldown = 0;

        public override void Draw(Random rand, SpriteBatch spriteBatch, Camera camera, GraphicsDevice graphicsDevice)
        {
            animation.Draw(rand, spriteBatch, camera, new Vector2(Origin.X - animation.Width / 2, Rectangle.Bottom - height) + textureOffset, spriteEffect, 0.5f);
        }

        #region backup
        /*
        public  void UpdateBackupedd(Room room, Random rand, Player player)
        {
            if (platformID == player.platformID && platformID != -1)
            {
                path.Clear();
                executingInstruction = false;
                state = State.idle;
                if (Game1.GetDistance(Origin, player.Origin) > range)
                {
                    if (player.position.X > position.X)
                    {
                        movement.X = movementMax.X;
                        state = State.walking;
                        direction = 1;
                    }
                    else
                    {
                        movement.X = -movementMax.X;
                        state = State.walking;
                        direction = -1;
                    }
                }
                else
                {
                    //ATTACK PLAYER
                    //DU BORDE GÖRA EN ATTACK-FUNKTION
                    //SKRIVER RETURN SÅLÄNGE
                }
            }
            else
            {
                //PATHFINDING
                if (executingInstruction)
                {
                    movement.X = movementPrev.X;
                }
                else
                {
                    state = State.idle;
                    Idle();
                }
                if (platformID != -1)
                {
                    if ((player.platformID != player.platformIDPrev || path.Count <= 0 || (platformID != path[0].platform.X && platformID != path[0].platform.Y)) && player.platformID != -1)
                    {
                        //Update path
                        path = room.GetPathInstructions(platformID, player.platformID, new Point(Rectangle.Width, Rectangle.Height), movementMax);
                        executingInstruction = false;
                    }
                    else if (path.Count > 0)
                    {
                        if (path[0].state != State.idle && !executingInstruction)
                        {
                            if (platformID == path[0].platform.X)
                            {
                                if (position.X == path[0].xPos && state == State.idle)
                                {
                                    movement.X = path[0].movement.X;
                                    state = path[0].state;
                                    if (state == State.jump)
                                        Jump(path[0].movement.Y);
                                    if (movement.X < 0)
                                        direction = -1;
                                    else if (movement.X > 0)
                                        direction = 1;

                                    if (state == State.walking)
                                        direction = direction;

                                    executingInstruction = true;
                                }
                                else if(Math.Abs(path[0].xPos - position.X) < movementMax.X)
                                {
                                    position.X = path[0].xPos;
                                    movement.X = 0;
                                    state = State.idle;
                                }
                                else if (path[0].xPos > position.X)
                                {
                                    movement.X = movementMax.X;
                                    state = State.walking;
                                    direction = 1;
                                }
                                else
                                {
                                    movement.X = -movementMax.X;
                                    state = State.walking;
                                    direction = -1;
                                }
                            }
                            else if(platformID == path[0].platform.Y)
                            {
                                path.RemoveAt(0);
                                executingInstruction = false;
                            }
                            else
                            {
                                //Update path
                                path = room.GetPathInstructions(platformID, player.platformID, new Point(Rectangle.Width, Rectangle.Height), movementMax);
                                executingInstruction = false;
                            }
                        }
                    }
                }
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

            base.Update(room, rand);
        }
        */
        #endregion

        public override void Update(Room room, Random rand, Player player)
        {
            //Sets fallThrough to false after the cooldown has been setto zero
            if (fallThroughCooldown > 0)
                fallThroughCooldown--;
            else if (fallThrough)
                fallThrough = false;

            //Stuck signifies that the enemy can't calculate a viable path to the player
            //If the player or the enemy changes platform the conditions has changed and it should try again
            if(stuck && (player.platformID != player.platformIDPrev || platformID != platformIDPrev))
            {
                stuck = false;
            }

            //Keeping the x-velocity while walking/jumping, etc.
            if (executingInstruction)
                movement.X = movementPrev.X;

            //If the bot is located on the same platform as the player is/was
            if(platformID == player.platformIDPrev)
            {
                //The path is no longer necessary and should be cleared.
                //No instruction is being executed and until further notice the state is "Idle"
                path.Clear();
                executingInstruction = false;
                state = State.idle;
                //If the player is outside the enemy's range it will move left/right to get closer
                if (Game1.GetDistance(Origin, player.Origin) > range)
                {
                    if (player.position.X > position.X)
                    {
                        movement.X = movementMax.X;
                        state = State.walking;
                        direction = 1;
                    }
                    else
                    {
                        movement.X = -movementMax.X;
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
            //If the player is on a different platform we need to get or follow a path
            //This can only be done if the enemy is on a platform
            //The path.count > 0 statement prevents errors since we'll be accessing path[0]
            else if(platformID != -1 && path.Count > 0)
            {
                //If we're in the middle of executing a given instruction
                if(executingInstruction)
                {
                    //If we're not on the platform we started on and we're not in the air we should get to the next instruction
                    if(platformID != path[0].platform.X)
                    {
                        path.RemoveAt(0);
                        executingInstruction = false;
                        
                        //Preventing errors
                        if(path.Count > 0)
                        {
                            //This would indicate that we're standing on the wrong platform
                            if(platformID != path[0].platform.X)
                            {
                                //The current path is no longer correct and we need a new one
                                GetPath(player, room);
                            }
                        }
                    }
                }
                //If the enemy is standing on the correct platform
                else if(platformID == path[0].platform.X)
                {
                    //An instruction with an "idle" state is the equivalent of an error and can't be executed
                    if (path[0].state == State.idle)
                        path.RemoveAt(0);
                    //If the position matches the requirement we're good to go
                    else if(position.X == path[0].xPos || (path[0].variableXPos && (Rectangle.X < room.platforms[path[0].platform.Y].rectangle.Right || Rectangle.Right > room.platforms[path[0].platform.Y].rectangle.X)))
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
                    else if(path[0].variableXPos)
                    {
                        if(room.platforms[path[0].platform.Y].rectangle.Center.X > Rectangle.Center.X)
                        {
                            movement.X = movementMax.X;
                            state = State.walking;
                            direction = 1;
                        }
                        else
                        {
                            movement.X = -movementMax.X;
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
                        movement.X = movementMax.X;
                        state = State.walking;
                        direction = 1;
                    }
                    else
                    {
                        movement.X = -movementMax.X;
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
            else if(platformID != -1)
            {
                //The enemy needs a path and doesn't have one
                GetPath(player, room);
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

        public virtual void GetPath(Player player, Room room)
        {
            path = room.GetPathInstructions(platformID, player.platformIDPrev, new Point(Rectangle.Width, Rectangle.Height), movementMax, 10000);
            if(path.Count < 1)
            {
                stuck = true;
            }
        }

        public virtual void Attack(Player player, Random rand, Room room)
        {
            state = State.attacking;
        }

        public virtual void Idle()
        {
            movement.X = 0;
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

        public class GreenDrone : Enemy
        {
            public GreenDrone(Vector2 pos, Room room, int level, Animation ani, float HitboxDamage, int HitboxCooldown) : base(pos, room, level, ani, HitboxDamage, HitboxCooldown)
            {
                Level = level;
                position = pos;
            }
        }
        public class GroundTroop : Enemy
        {
            public string spr_walking, spr_idle, spr_shooting, spr_charging, spr_bullet, spr_chargedBullet, spr_fall, spr_jump;
            public Animation walking, idle, shooting, charging;
            public State state;
            bool charge = false;
            int chargingTimer = 0;
            int chargingCap = 60 * 2;
            int chargeTimer = 0;
            int chargeCooldown = 60 * 5;
            int attackTimer = 0;
            int shootingTimer = 0;
            float shootingSpeedSprite = 0;

            public GroundTroop(Vector2 pos, Room room, int level, Animation ani, float HitboxDamage, int HitboxCooldown) : base(pos, room, level, ani, HitboxDamage, HitboxCooldown)
            {
                useResistance = false;
                useFriction = false;
                movementMax = new Vector2(15, 40);
                Level = level;
                position = pos;
                spr_walking = "GroundTroop_Walking";
                spr_idle = "GroundTroop_Idle";
                spr_shooting = "GroundTroop_Shooting";
                spr_jump = "GroundTroop_Jump";
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

            public override void Update(Room room, Random rand, Player player)
            {
                base.Update(room, rand, player);
            }

            public override void Attack(Player player, Random rand, Room room)
            {
                base.Attack(player, rand, room);

                DamageZone_Hitbox.Position = position;
                if (Vector2.Distance(player.position, position) < 600 && !charge && rand.Next(0, 1000) <= 10)
                {
                    charge = true;
                }
                else if (Vector2.Distance(player.position, position) < 400 && !charge && shootingTimer >= 60)
                {
                    shootingTimer = 0;
                    state = State.shooting;
                    room.gameObjects.Add(new Projectile(room.gameObjects.Count, new Animation(spr_bullet), Origin + new Vector2(30, 10), new Vector2(10, 0), damage, 2, 1, room, false, false, false, false, 0f, true, spriteEffect, attackable));
                }
                else
                {
                    if (player.position.X > position.X)
                    {
                        direction = 1;
                        if (!charge)
                        {
                            state = State.walking;
                            movement.X += 0.2f;
                        }
                    }
                    if (player.position.X < position.X)
                    {
                        direction = -1;
                        if (!charge)
                        {
                            state = State.walking;
                            movement.X -= 0.2f;
                        }
                    }
                }

                if (charge)
                {
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
                        if (shootingTimer % 15 == 0)
                        {
                            room.gameObjects.Add(new Projectile(room.gameObjects.Count, new Animation(spr_chargedBullet), Origin + new Vector2(30, 10), new Vector2(25, 0), damage, 2, 1, room, false, false, false, false, 0f, true, spriteEffect, attackable));
                            shooting.currentFrame = 0;
                            shooting.speed = shootingSpeedSprite;
                            if (shootingTimer >= 90)
                            {
                                charge = false;
                                chargingTimer = 0;
                                shootingTimer = 0;
                            }
                        }
                    }
                }
                else { shootingTimer++; }
            }

            public override void Draw(Random rand, SpriteBatch spriteBatch, Camera camera, GraphicsDevice graphicsDevice)
            {
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
                    case State.fall:
                        SpriteHandler.Draw(spr_fall, rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                        break;
                    default:
                        break;
                }
                if (shooting.AnimationEnd) { shooting.speed = 0; }

                spriteBatch.DrawString(Game1.fontDebug, path.Count.ToString(), position, Color.Green);
            }
        }

        public class TestEnemy : Enemy
        {

            public TestEnemy(Vector2 pos, Room room, int level, Animation ani, float HitboxDamage, int HitboxCooldown) : base(pos, room, level, ani, HitboxDamage, HitboxCooldown)
            {
                Level = level;
                position = pos;
                hp = 20;
            }

            public override void Update(Room room, Random rand, Player player)
            {
                base.Update(room, rand);
                DamageZone_Hitbox.Position = position;
                if (player.position.X > position.X)
                {
                    movement.X += 0.2f;
                }
                if (player.position.X < position.X)
                {
                    movement.X -= 0.2f;
                }
            }
            public override void Draw(Random rand, SpriteBatch spriteBatch, Camera camera, GraphicsDevice graphicsDevice)
            {
                base.Draw(rand, spriteBatch, camera, graphicsDevice);
            }
        }
    }
}