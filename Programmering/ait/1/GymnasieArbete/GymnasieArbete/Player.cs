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
        public int Level { get; set; }
        int experience = 0;
        int experienceCap = 1000;
        float speedLeft, speedRight, speedJump;
        public string spr_idle, spr_running, spr_dash, spr_jump, spr_fall, spr_stall;
        public KeyboardState keyboard;
        public KeyboardState keyboardPrev;
        public bool Dashing = false;
        public List<Weapon> Weapons = new List<Weapon>();

        private int dashTimer = 0;
        public int DashTime { get; set; }
        enum State
        {
            idle,
            running,
            dash,
            jump,
            fall,
            stall
        }
        State state = State.idle;
        protected SpriteEffects spriteEffect;

        public Player(Rectangle rectangle, int level, Room newRoom) : base(rectangle, level, newRoom, null)
        {
            spr_running = "player_running";
            spr_jump = "player_jump";
            spr_fall = "player_fall";
            spr_idle = "player_idle";
            spr_stall = "player_stall";
            spr_dash = "player_dash";
            SpriteHandler.sprites[spr_running].speed = 0.35f;
            SpriteHandler.sprites[spr_dash].speed = 0.36f;
            speedLeft = -10;
            speedRight = 10;
            speedJump = -40;
            DashTime = 30;
            Level = level;
            hp = 100;
        }

        public override void Update(Room room, Random rand)
        {
            base.Update(room, rand);
            
            keyboard = Keyboard.GetState();
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
            if(hp <= 0)
            {
                Console.WriteLine();
            }
            #endregion

            dashTimer++;
            #region Input
            if (keyboard.IsKeyDown(Keys.Right))
            {
                if (!usingRight && !usingLeft) { direction = 1; }
                if (onGround && !Dashing) { state = State.running; }
                MoveRight();
            }
            else if (onGround) { state = State.idle; }

            if (keyboard.IsKeyDown(Keys.Left))
            {
                if (!usingRight && !usingLeft) { direction = -1; }
                if (onGround) { state = State.running; }
                MoveLeft();
            }
            else if (onGround && !keyboard.IsKeyDown(Keys.Right)) { state = State.idle; }
            if (keyboard.IsKeyDown(Keys.Up))
            {
                Jump();
            }
            if (keyboard.IsKeyDown(Keys.Down) && dashTimer >= DashTime)
            {
                Dash();
                dashTimer = 0;
                Dashing = true;
            }
            #endregion
            if (Dashing) { state = State.dash; }
            if(SpriteHandler.sprites[spr_dash].AnimationEnd) { Dashing = false; }
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

        public virtual void MoveLeft()
        {
            if (movement.X > speedLeft)
                movement.X = speedLeft;
        }
        public virtual void MoveRight()
        {
            if (movement.X < speedRight)
                movement.X = speedRight;
        }
        public virtual void Jump()
        {
            if (onGround)
            {
                movement.Y = speedJump;
                onGround = false;
            }
        }

        public override void Draw(Random rand, SpriteBatch spriteBatch, Camera camera, GraphicsDevice graphicsDevice)
        {
            if (direction < 0)
                spriteEffect = SpriteEffects.FlipHorizontally;
            else
                spriteEffect = SpriteEffects.None;

            Vector2 drawPos = new Vector2(Origin.X - SpriteHandler.sprites[spr_idle].width / 2, Rectangle.Bottom - height) + textureOffset;

            leftHand.Draw(this, rand, spriteBatch, camera, true);
            switch (state)
            {
                case State.idle:
                    SpriteHandler.Draw(spr_idle, rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
                    break;
                case State.running:
                    SpriteHandler.Draw(spr_running, rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White,1 , spriteEffect, 0.5f);
                    break;
                case State.dash:
                    SpriteHandler.Draw(spr_dash, rand, spriteBatch, camera, drawPos, 1f, 0f, Vector2.Zero, Color.White, 1, spriteEffect, 0.5f);
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
            rightHand.Draw(this, rand, spriteBatch, camera, false);

            if (SpriteHandler.sprites["Energy_lIdle"].currentFrame != SpriteHandler.sprites[spr_idle].currentFrame)
            {
                SpriteHandler.sprites["Energy_lIdle"].currentFrame = SpriteHandler.sprites[spr_idle].currentFrame;
            }
            if (SpriteHandler.sprites["Energy_rIdle"].currentFrame != SpriteHandler.sprites[spr_idle].currentFrame)
            {
                SpriteHandler.sprites["Energy_rIdle"].currentFrame = SpriteHandler.sprites[spr_idle].currentFrame;
            }

        }
    }
}
