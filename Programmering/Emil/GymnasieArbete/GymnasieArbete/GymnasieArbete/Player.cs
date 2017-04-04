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
    public class Player : GameObject
    {

        public Weapon leftHand;
        public Weapon rightHand;
        public bool usingLeft;
        public bool usingRight;
        public bool LookDiagonal = false;
        int experience = 0;
        int experienceCap = 1000;
        float speedLeft, speedRight, speedJump;
        public string spr_idle, spr_running, spr_dash, spr_jump, spr_fall, spr_stall, spr_back;
        public bool doubleJump = true;
        public KeyboardState keyboard;
        public KeyboardState keyboardPrev;
        public bool Dashing = false;
        public int Scraps { get; set; }
        public float maxHp;
        public List<Weapon> Weapons = new List<Weapon>();
        public int room = 0;
        public int roomPrev = 0;
        public int wallJumpDelay = 0;

        private int dashTimer = 0;
        public int DashTime { get; set; }
        public enum State
        {
            idle,
            running,
            dash,
            jump,
            fall,
            stall,
            back
        }
        public State state = State.idle;
        protected SpriteEffects spriteEffect;

        public Player(Rectangle rectangle, int level, Room newRoom) : base(rectangle, level, newRoom, null)
        {
            Scraps = 0;
            type = Types.Player;
            spr_running = "player_running";
            spr_jump = "player_jump";
            spr_fall = "player_fall";
            spr_idle = "player_idle";
            spr_stall = "player_stall";
            spr_dash = "player_dash";
            spr_back = "player_back";
            //SpriteHandler.sprites[spr_running].speed = 0.35f;
            //SpriteHandler.sprites[spr_dash].speed = 0.36f;
            movementMax = new Vector2(10, 30);
            speedJump = -30;
            DashTime = 30;
            Level = level;
            hp = 50;
        }

        public override void Update(Room room, Random rand)
        {
            maxHp = leftHand.hp + rightHand.hp;
            if (hp > maxHp) { hp = maxHp; }
            keyboard = Keyboard.GetState();
            
            base.Update(room, rand);

            if (keyboard.IsKeyDown(Keys.L) && keyboardPrev.IsKeyUp(Keys.L))
                room.gameObjects.Add(new GameObject.Chest(new Rectangle(100, 100, 20, 20), 1, room, "LavaBot_Stall", false));

                for (int i = 0; i < room.scraps.Count; i++)
            {
                if (Rectangle.Intersects(room.scraps[i].Rectangle))
                {
                    room.scraps[i].Die(room, rand);
                    Scraps += room.scraps[i].value;
                }
            }

            if (!onWall|| onGround)
                wallJumpDelay = 0;

            if (onGround && Math.Abs(movement.X) > 0 && !(!keyboard.IsKeyDown(Keys.Right) && !keyboard.IsKeyDown(Keys.Left) && Game1.Controller.ThumbSticks.Left.X > -0.3f && Game1.Controller.ThumbSticks.Left.X < 0.3f))
                state = State.running;
            else if (onGround && ((keyboard.IsKeyUp(Keys.Down) && Game1.Controller.Buttons.B == ButtonState.Released) || dashTimer < DashTime))
                state = State.idle;


            if (Game1.reassEffects)
            {
                if (state == State.running && (Math.Floor(SpriteHandler.sprites[spr_running].currentFrame) == 0 || Math.Floor(SpriteHandler.sprites[spr_running].currentFrame) == 6))
                {
                    for (int i = 0; i < (int)(Math.Abs(movement.X) / 3); i++)
                    {
                        SpawnDust(room, rand, new Vector2(Rectangle.Center.X, Rectangle.Bottom - 4), -movement * .4f);
                    }
                }

                if (state == State.running && Math.Abs(movement.X) > 1 && (!keyboard.IsKeyDown(Keys.Right) && !keyboard.IsKeyDown(Keys.Left) && Game1.Controller.ThumbSticks.Left.X > -0.3f && Game1.Controller.ThumbSticks.Left.X < 0.3f))
                {
                    SpawnDust(room, rand, new Vector2(Rectangle.Center.X, Rectangle.Bottom - 4), movement * .85f);
                }

                if (state == State.dash)
                {
                    SpawnDust(room, rand, new Vector2(Rectangle.Center.X, Rectangle.Bottom - 4), -movement * .5f);
                }

                if (onGround && !onGroundPrev)
                {
                    for (int i = 0; i < Rectangle.Width / 3; i++)
                    {
                        SpawnDust(room, rand, new Vector2(Rectangle.X + rand.Next(Rectangle.Width), Rectangle.Bottom - 4), new Vector2(((float)rand.NextDouble() - .5f) * movementPrev.Y * .2f, (float)rand.NextDouble() - .5f));
                    }
                }
            }


            for (int i = 0; i < room.scraps.Count; i++)
            {
                if (Rectangle.Intersects(room.scraps[i].Rectangle))
                {
                    room.scraps[i].Die(room, rand);
                    Scraps += room.scraps[i].value;
                }
            }
            
            if (wallJumpDelay > 0)
            wallJumpDelay--;

            dashTimer++;
            #region Input

            if (!Game1.Controller_Connected)
            {
                if (keyboard.IsKeyDown(Keys.Right))
                {
                    if (!usingRight && !usingLeft) { direction = 1; }
                    if (direction == -1)
                    {
                        speedRight = movementMax.X * 0.6f;
                        state = State.back;
                    }
                    else
                    {
                        speedRight = movementMax.X;
                    }
                    MoveRight(room, rand);
                }

                if (keyboard.IsKeyDown(Keys.Left))
                {
                    if (!usingRight && !usingLeft) { direction = -1; }
                    if (direction == 1)
                    {
                        speedLeft = -movementMax.X * 0.6f;
                        state = State.back;
                        SpriteHandler.sprites[leftHand.spr_lRunning].speed /= 2;
                        SpriteHandler.sprites[rightHand.spr_rRunning].speed /= 2;
                    }
                    else
                    {
                        speedLeft = -movementMax.X;
                    }
                    MoveLeft(room, rand);
                }

                if (keyboard.IsKeyDown(Keys.Up) && keyboardPrev.IsKeyUp(Keys.Up))
                {
                    Jump(room, rand);
                }

                if (keyboard.IsKeyDown(Keys.Down) && dashTimer >= DashTime)
                {
                    Dash();
                    dashTimer = 0;
                    Dashing = true;
                }
            }
            else
            {
                if (Game1.Controller.ThumbSticks.Left.X > 0.3f)
                {
                    if (!usingRight && !usingLeft) { direction = 1; }
                    if (direction == -1)
                    {
                        speedRight = movementMax.X * 0.6f;
                        state = State.back;
                    }
                    else
                    {
                        speedRight = movementMax.X;
                    }
                    MoveRight(room, rand);
                }

                if (Game1.Controller.ThumbSticks.Left.X < -0.3f)
                {
                    if (!usingRight && !usingLeft) { direction = -1; }
                    if (direction == 1)
                    {
                        speedLeft = -movementMax.X * 0.6f;
                        state = State.back;
                        SpriteHandler.sprites[leftHand.spr_lRunning].speed /= 2;
                        SpriteHandler.sprites[rightHand.spr_rRunning].speed /= 2;
                    }
                    else
                    {
                        speedLeft = -movementMax.X;
                    }
                    MoveLeft(room, rand);
                }

                if (Game1.Controller.Buttons.A == ButtonState.Pressed && Game1.ControllerPrev.Buttons.A == ButtonState.Released)
                {
                    Jump(room, rand);
                }
                if (Game1.Controller.Buttons.B == ButtonState.Pressed && dashTimer >= DashTime)
                {
                    Dash();
                    dashTimer = 0;
                    Dashing = true;
                }
            }
            #endregion

            #region sprite Shit
            if (movement.Y >= 2 && !onGround)
            {
                state = State.fall;
                leftHand.state = Weapon.WeaponState.fall;
                rightHand.state = Weapon.WeaponState.fall;
            }
            else if (movement.Y >= -3 && !onGround)
            {
                state = State.stall;
                leftHand.state = Weapon.WeaponState.stall;
                rightHand.state = Weapon.WeaponState.stall;
            }
            else if (movement.Y <= -3 && !onGround)
            {
                state = State.jump;
                leftHand.state = Weapon.WeaponState.jump;
                rightHand.state = Weapon.WeaponState.jump;
            }


            #endregion
            if (Dashing) { state = State.dash; }

            if (SpriteHandler.sprites[spr_dash].animationEnd)
            {
                Dashing = false;
            }
            if (onGround)
            {
                doubleJump = true;
                onWall = false;
            }
            if (onGround && (!keyboard.IsKeyDown(Keys.Right) && !keyboard.IsKeyDown(Keys.Left) && Game1.Controller.ThumbSticks.Left.X > -0.3f && Game1.Controller.ThumbSticks.Left.X < 0.3f))
            {
                state = State.idle;
                leftHand.state = Weapon.WeaponState.idle;
                rightHand.state = Weapon.WeaponState.idle;
            }
            keyboardPrev = Keyboard.GetState();

        }

        public void LevelUp()
        {
            int temp = experience;
            experience = 0;
            experience += (temp - experienceCap);
            Level += 1;
        }

        public void Dash()
        {
            if (movement.X > 0)
            { movement = new Vector2(movement.X + (25), movement.Y); }
            else { movement = new Vector2(movement.X + (25 * -1), movement.Y); }
        }

        public virtual void MoveLeft(Room room, Random rand)
        {
            if (!onWall || onGround)
            {
                if (movement.X > 0)
                {
                    //Acceleration
                    movement.X += speedLeft * Game1.playerAcceleration * Game1.playerTurnFactor;
                }
                else if (movement.X > speedLeft * .5f)
                {
                    //Acceleration
                    movement.X += speedLeft * Game1.playerAcceleration * .23f;
                }
                else if (movement.X > speedLeft * .9f)
                {
                    //Acceleration
                    movement.X += speedLeft * Game1.playerAcceleration;
                }
                else if (movement.X > speedLeft) movement.X = speedLeft;
            }
            else if (onWallRight)
            {
                if (wallJumpDelay == -1)
                {
                    //WALLJUMP
                    movement.X = speedLeft * 1.75f;
                    movement.Y = speedJump * .65f;
                    state = State.jump;

                    if (Game1.reassEffects)
                    {
                        for (int i = 0; i < 14; i++)
                        {
                            SpawnDust(room, rand, new Vector2(Rectangle.Center.X, Rectangle.Y + rand.Next(Rectangle.Height)), -movement * .4f + new Vector2(0, rand.Next(6) - 3));
                        }
                    }
                }
                else if (wallJumpDelay == 0 && !onGround)
                {
                    wallJumpDelay = World.wallJumpDelay;
                }
                else
                {
                    movement.Y *= .8f - (wallJumpDelay / World.wallJumpDelay) * .5f;
                }
            }
            else if (!onWallRight && movement.Y > 0)
                movement.Y *= .8f;
        }
        public virtual void MoveRight(Room room, Random rand)
        {
            if (!onWall || onGround)
            {
                if (movement.X < 0)
                {
                    //Acceleration
                    movement.X += speedRight * Game1.playerAcceleration * Game1.playerTurnFactor;
                }
                else if (movement.X < speedRight * .5f)
                {
                    //Acceleration
                    movement.X += speedRight * Game1.playerAcceleration * .23f;
                }
                else if (movement.X < speedRight * .9f)
                {
                    //Acceleration
                    movement.X += speedRight * Game1.playerAcceleration;
                }
                else if (movement.X < speedRight) movement.X = speedRight;
            }
            else if (!onWallRight)
            {
                if (wallJumpDelay == -1)
                {
                    //WALLJUMP
                    movement.X = speedRight * 1.75f;
                    movement.Y = speedJump * .65f;
                    state = State.jump;

                    if (Game1.reassEffects)
                    {
                        for (int i = 0; i < 14; i++)
                        {
                            SpawnDust(room, rand, new Vector2(Rectangle.Center.X, Rectangle.Y + rand.Next(Rectangle.Height)), -movement * .4f + new Vector2(0, rand.Next(6) - 3));
                        }
                    }
                }
                else if (wallJumpDelay == 0)
                {
                    wallJumpDelay = World.wallJumpDelay;
                }
                else
                {
                    movement.Y *= .8f - (wallJumpDelay / World.wallJumpDelay) * .5f;
                }
            }
            else if (onWallRight && movement.Y > 0)
                movement.Y *= .8f;
        }
        public virtual void Jump(Room room, Random rand)
        {
            if (onGround)
            {
                movement.Y = speedJump;
                onGround = false;
                if (Game1.reassEffects)
                {
                    for (int i = 0; i < Rectangle.Width / 4; i++)
                    {
                        SpawnDust(room, rand, new Vector2(Rectangle.X + rand.Next(Rectangle.Width), Rectangle.Bottom - 4), new Vector2(((float)rand.NextDouble() - .5f) * speedJump * 3f, (float)rand.NextDouble() - 1));
                    }
                }
            }
            else if (!onGround && doubleJump)
            {
                doubleJump = false;
                movement.Y = speedJump;

                if (Game1.reassEffects)
                {
                    for (int i = 0; i < Rectangle.Width / 4; i++)
                    {
                        SpawnDust(room, rand, new Vector2(Rectangle.X + rand.Next(Rectangle.Width), Rectangle.Bottom - 4), new Vector2(((float)rand.NextDouble() - .5f) * speedJump * .3f, (float)rand.NextDouble() - .5f));
                    }
                }
            }

            if(!onGround && onWall)
            {
                wallJumpDelay = -1;
            }
        }

        public void SpawnDust(Room room, Random rand, Vector2 position, Vector2 movement)
        {
            room.particles.Add(new Particle.Smoke(room.particles.Count, rand, position, movement, 0f, (float)rand.NextDouble() * .5f + .1f, .115f, rand.Next(5) + 7, new Color(rand.Next(20) + 130, rand.Next(20) + 130, rand.Next(20) + 130), false));
        }

        public override void Draw(Point offset, Random rand, SpriteBatch spriteBatch, GraphicsDevice graphics, Camera camera)
        {
            if (direction < 0)
                spriteEffect = SpriteEffects.FlipHorizontally;
            else
                spriteEffect = SpriteEffects.None;

            Vector2 drawPos = new Vector2(Origin.X - SpriteHandler.sprites[spr_idle].width / 2, Rectangle.Bottom - height) + textureOffset;

            rightHand.Draw(this, rand, spriteBatch, graphics, camera, false);
            switch (state)
            {

                case State.idle:
                    SpriteHandler.Draw(spr_idle, rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                    break;
                case State.running:
                    SpriteHandler.sprites[spr_running].speed = Math.Abs(movement.X) * .035f;
                    SpriteHandler.Draw(spr_running, rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                    break;
                case State.dash:
                    SpriteHandler.Draw(spr_dash, rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                    break;
                case State.back:
                    SpriteHandler.Draw(spr_back, rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                    break;
                case State.jump:
                    SpriteHandler.Draw(spr_jump, rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                    break;
                case State.fall:
                    SpriteHandler.Draw(spr_fall, rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                    break;
                case State.stall:
                    SpriteHandler.Draw(spr_stall, rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                    break;
                default:
                    break;
            }

            leftHand.Draw(this, rand, spriteBatch, graphics, camera, true);
            if (SpriteHandler.sprites["Energy_lIdle"].currentFrame != SpriteHandler.sprites[spr_idle].currentFrame)
            {
                SpriteHandler.sprites["Energy_lIdle"].currentFrame = SpriteHandler.sprites[spr_idle].currentFrame;
            }
            if (SpriteHandler.sprites["Energy_rIdle"].currentFrame != SpriteHandler.sprites[spr_idle].currentFrame)
            {
                SpriteHandler.sprites["Energy_rIdle"].currentFrame = SpriteHandler.sprites[spr_idle].currentFrame;
            }

        }

        public override void TakeDamage(Room room, int sourceID, float damage)
        {
            base.TakeDamage(room, sourceID, damage);

            room.overlays.Add(new Overlay("overlay_damage", 10,  damage / hp, Color.Red));
            room.AddShake(new Vector2(damage/hp * 20, damage/hp * 20), 4);
        }
    }
}
