/*using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace GymnasieArbete_older
{
    /// <summary>
    /// Handles all of the players input on keyboard and Xbox controller.
    /// Needs World.cs with int "AttackCD", calling it through World.AttackCD (standard cooldowns between each attacks) 
    /// Also needs Weapon.cs (The Weapon Base file)
    /// </summary>
    class PlayerInput
    {
        public KeyboardState keyboard;
        public KeyboardState keyboardPrev;
        public GamePadState Controller { get; set; }
        public bool Controller_Connected { get; set; }
        public Keys KeyLeft_Attack1 { get; set; }
        public Keys KeyLeft_Attack2 { get; set; }
        public Keys KeyLeft_Attack3 { get; set; }
        public Keys KeyRight_Attack1 { get; set; }
        public Keys KeyRight_Attack2 { get; set; }
        public Keys KeyRight_Attack3 { get; set; }
        public Keys KeyLeft { get; set; }
        public Keys KeyRight { get; set; }
        public Keys KeyJump { get; set; }
        public Keys KeyDash { get; set; }
        public Keys KeyMoveRight { get; set; }
        public Keys KeyMoveLeft { get; set; }
        public Keys LookDiagnle { get; set; }
        public bool onGround { get; set; }

        public RightHand rightAttack;
        public LeftHand leftAttack;

        private bool usingLeft;
        private bool canAttackLeft;
        private bool canAttackRight;
        private bool usingRight;
        private int dashTimer = 60;
        private float chargeTimer_R = 0;
        private float chargeTimer_L = 0;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="LA1">Left Attack 1</param>
        /// <param name="LA2">Left Attack 2</param>
        /// <param name="LA3">Left Attack 3</param>
        /// <param name="RA1">Right Attack 1</param>
        /// <param name="RA2">Right Attack 2</param>
        /// <param name="RA3">Right Attack 3</param>
        /// <param name="jump"></param>
        /// <param name="dash"></param>
        public PlayerInput(Keys LA1, Keys LA2, Keys LA3, Keys RA1, Keys RA2, Keys RA3, Keys jump, Keys dash, Keys moveRight, Keys moveLeft)
        {
            KeyLeft_Attack1 = LA1;
            KeyLeft_Attack2 = LA2;
            KeyLeft_Attack3 = LA3;
            KeyRight_Attack1 = RA1;
            KeyRight_Attack2 = RA2;
            KeyRight_Attack3 = RA3;
            KeyJump = jump;
            KeyDash = dash;
            KeyMoveRight = moveRight;
            KeyMoveLeft = moveLeft;
            usingLeft = false;
            usingRight = false;

            rightAttack = RightHand.a0;
            leftAttack = LeftHand.a0;
            Controller = GamePad.GetState(PlayerIndex.One);
            Controller_Connected = false;
        }
        /// <summary>
        /// Send in the weapon in the left hand, the right hand, the player object, the current keyboardstate, the previous keyboardstate
        /// </summary>
        /// <param name="player.leftHand"></param>
        /// <param name="player.rightHand"></param>
        /// <param name="player"></param>
        /// <param name="keyboard"></param>
        /// <param name="keyboardPrev"></param>
        public void Update(Player player, Room room)
        {
            keyboard = Keyboard.GetState();
            player.leftHand.Update(player, room);
            player.rightHand.Update(player, room);
            Controller = GamePad.GetState(PlayerIndex.One);

            if (Controller.IsConnected) { Controller_Connected = true; }
            else { Controller_Connected = false; }

            if (dashTimer <= 60) { dashTimer++; }


            #region Controls
            if (Controller_Connected)
            {

                if (Controller.Buttons.X == ButtonState.Pressed)
                {

                    if (usingRight && rightAttack == RightHand.a0 || rightAttack == RightHand.a1)
                    {
                        rightAttack = RightHand.a1;
                        player.rightHand.Attack1();
                    }

                    if (usingLeft && leftAttack == LeftHand.a0 || leftAttack == LeftHand.a1)
                    {

                        leftAttack = LeftHand.a1;
                        player.leftHand.Attack1();
                    }
                }
                else if (Controller.Buttons.X == ButtonState.Released)
                {
                    if (usingRight)
                    {
                        rightAttack = RightHand.a0;
                    }
                    if (usingLeft)
                    {
                        leftAttack = LeftHand.a0;
                    }
                }

                if (Controller.Buttons.Y == ButtonState.Pressed)
                {
                    if (usingRight && rightAttack == RightHand.a0 || rightAttack == RightHand.a2)
                    {
                        rightAttack = RightHand.a2;
                        player.rightHand.Attack2();
                    }

                    if (usingLeft && leftAttack == LeftHand.a0 || leftAttack == LeftHand.a2)
                    {
                        leftAttack = LeftHand.a2;
                        player.leftHand.Attack2();
                    }

                }
                else if (Controller.Buttons.Y == ButtonState.Released)
                {
                    if (usingRight)
                    {
                        rightAttack = RightHand.a0;
                    }
                    if (usingLeft)
                    {
                        leftAttack = LeftHand.a0;
                    }
                }
                if (Controller.Buttons.B == ButtonState.Pressed)
                {

                    if (usingRight && rightAttack == RightHand.a0 || rightAttack == RightHand.a3)
                    {
                        rightAttack = RightHand.a3;
                        player.rightHand.Attack3();
                    }

                    if (usingLeft && leftAttack == LeftHand.a0 || leftAttack == LeftHand.a3)
                    {
                        leftAttack = LeftHand.a3;
                        player.leftHand.Attack3();
                    }

                }
                else if (Controller.Buttons.B == ButtonState.Released)
                {
                    if (usingRight)
                    {
                        rightAttack = RightHand.a0;
                    }
                    if (usingLeft)
                    {
                        leftAttack = LeftHand.a0;
                    }
                }
                if (Controller.Triggers.Left > 0.5f)
                {
                    usingLeft = true;
                }
                else if (Controller.Triggers.Left < 0.2f)
                {
                    usingLeft = false;
                }
                if (Controller.Triggers.Right > 0.5f)
                {
                    usingRight = true;
                }
                else if (Controller.Triggers.Right < 0.2f)
                {
                    usingRight = false;
                }
                if (Controller.Buttons.LeftShoulder == ButtonState.Pressed && dashTimer >= 60)
                {
                    player.Dash();
                    dashTimer = 0;
                }
                if (Controller.ThumbSticks.Left.X < -0.3f)
                {
                    if (!usingRight && !usingLeft)
                    {
                        player.direction = -1;
                    }
                    player.MoveLeft();
                }
                if (Controller.ThumbSticks.Left.X > 0.3f)
                {
                    if (!usingRight && !usingLeft) { player.direction = 1; }
                    player.MoveRight();
                }
                if (Controller.Buttons.A == ButtonState.Pressed)
                {
                    player.Jump();
                }
                if (Controller.Buttons.RightShoulder == ButtonState.Pressed)
                {
                    player.LookDiagonal = true;
                }
                else
                {
                    player.LookDiagonal = false;
                }

            }
            else
            {
                if (keyboard.IsKeyDown(Keys.RightShift))
                {
                    player.LookDiagonal = true;
                }
                else
                {
                    player.LookDiagonal = false;
                }

                if (keyboard.IsKeyDown(Keys.Right))
                {
                    if (!usingRight && !usingLeft) { player.direction = 1; }
                    player.MoveRight();
                }
                if (keyboard.IsKeyDown(Keys.Left))
                {
                    if (!usingRight && !usingLeft) { player.direction = -1; }
                    player.MoveLeft();
                }

                if (keyboard.IsKeyDown(Keys.Up))
                {
                    player.Jump();
                }
                if (keyboard.IsKeyDown(Keys.Down) && dashTimer >= 60)
                {
                    player.Dash();
                    dashTimer = 0;
                }

                if (keyboard.IsKeyDown(Keys.D) && rightAttack == RightHand.a0 || rightAttack == RightHand.a1)
                {
                    rightAttack = RightHand.a1;
                    player.rightHand.Attack1();
                }
                else if (keyboardPrev.IsKeyDown(Keys.D) && keyboard.IsKeyUp(Keys.D))
                {
                    rightAttack = RightHand.a0;
                }

                if (keyboard.IsKeyDown(Keys.S) && rightAttack == RightHand.a0 || rightAttack == RightHand.a2)
                {
                    rightAttack = RightHand.a2;
                    player.rightHand.Attack2();
                }
                else if (keyboardPrev.IsKeyDown(Keys.S) && keyboard.IsKeyUp(Keys.S))
                {
                    rightAttack = RightHand.a0;
                }

                if (keyboard.IsKeyDown(Keys.A) && rightAttack == RightHand.a0 || rightAttack == RightHand.a3)
                {
                    rightAttack = RightHand.a3;
                    player.rightHand.Attack3();
                }
                else if (keyboardPrev.IsKeyDown(Keys.A) && keyboard.IsKeyUp(Keys.A))
                {
                    rightAttack = RightHand.a0;
                }

                if (keyboard.IsKeyDown(Keys.E) && leftAttack == LeftHand.a0 || leftAttack == LeftHand.a1)
                {
                    leftAttack = LeftHand.a1;
                    player.leftHand.Attack1();
                }
                else if (keyboardPrev.IsKeyDown(Keys.E) && keyboard.IsKeyUp(Keys.E))
                {
                    leftAttack = LeftHand.a0;
                }

                if (keyboard.IsKeyDown(Keys.W) && leftAttack == LeftHand.a0 || leftAttack == LeftHand.a2)
                {
                    leftAttack = LeftHand.a2;
                    player.leftHand.Attack2();
                }
                else if (keyboardPrev.IsKeyDown(Keys.W) && keyboard.IsKeyUp(Keys.W))
                {
                    leftAttack = LeftHand.a0;
                }

                if (keyboard.IsKeyDown(Keys.Q) && leftAttack == LeftHand.a0 || leftAttack == LeftHand.a3)
                {
                    leftAttack = LeftHand.a3;
                    player.leftHand.Attack3();
                }
                else if (keyboardPrev.IsKeyDown(Keys.Q) && keyboard.IsKeyUp(Keys.Q))
                {
                    leftAttack = LeftHand.a0;
                }


                if (keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.S) || keyboard.IsKeyDown(Keys.D))
                {
                    usingRight = true;
                }
                else { usingRight = false; }

                if (keyboard.IsKeyDown(Keys.E) || keyboard.IsKeyDown(Keys.W) || keyboard.IsKeyDown(Keys.Q))
                {
                    usingLeft = true;
                }
                else { usingLeft = false; }

            }
            #endregion
            keyboardPrev = Keyboard.GetState();
        }

    }
}
*/