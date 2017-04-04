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
    /// <summary>
    /// This is a class for a base weapon system
    /// </summary>
    public abstract class Weapon
    {
        //None is for general non-type weapon
        public KeyboardState keyboard;
        public KeyboardState keyboardPrev;
        public enum WeaponState
        {
            jump,
            shoot1_1,
            shoot1_2,
            shoot1_3,
            shoot2_1,
            shoot2_2,
            shoot2_3,
            shoot3_1,
            shoot3_2,
            run,
            idle,
            fall,
            stall
        };
        public WeaponState state;
        public enum e_Attacks
        {
            a0,
            a1,
            a2,
            a3
        };
        public e_Attacks e_attacksPrev = e_Attacks.a0;
        public e_Attacks e_attacks = e_Attacks.a0;
        /// <summary>
        /// True =  Left Hand
        /// False = Right Hand
        /// </summary>
        public float leftDepth = 0.51f;
        public float rightDepth = 0.49f;
        public bool leftHand;
        public string currentSprite;
        public float ChargeTimerCap { get; set; }
        public float ChargeTimer { get; set; }
        public bool releaseCharge { get; set; }
        public bool hasCharge { get; set; }
        public Projectile chargedProj;
        public bool Attack1Bool = false;
        public bool Attack2Bool = false;
        public bool Attack3Bool = false;
        public string Name { get; set; }
        public int Level { get; set; }
        public float Damage { get; set; }
        public float AttackSpeed { get; set; }
        public float RoomCharge { get; set; }
        public List<List<int>> AttackUpgradeList { get; set; }
        public Vector2 ShootOrigin { get; set; }
        public Vector2 LeftOrigin { get; set; }
        public Vector2 RightOrigin { get; set; }
        public float attackTimer = 0;
        public string spr_lJump, spr_lStall, spr_lFall, spr_lShoot1_1, spr_lShoot1_2, spr_lShoot1_3, spr_lShoot2_1, spr_lShoot2_2, spr_lShoot2_3, spr_lShoot3_1, spr_lShoot3_2, spr_lIdle, spr_lRunning, spr_rJump, spr_rStall, spr_rFall, spr_rShoot1_1, spr_rShoot1_2, spr_rShoot1_3, spr_rShoot2_1, spr_rShoot2_2, spr_rShoot2_3, spr_rShoot3_1, spr_rShoot3_2, spr_rIdle, spr_rRunning;
        protected SpriteEffects spriteEffect;
        protected Color drawBulletColor = Color.White;
        protected List<Rectangle> drawBulletRects = new List<Rectangle>();
        public List<GameObject.Types> attackable = new List<GameObject.Types>();
        public float hp = 0;
        public Weapon()
        {

            #region Weapon Upgrades
            List<int> attack1 = new List<int>();
            attack1.Add(0);
            attack1.Add(0);
            attack1.Add(0);
            List<int> attack2 = new List<int>();
            attack2.Add(0);
            attack2.Add(0);
            attack2.Add(0);
            List<int> attack3 = new List<int>();
            attack3.Add(0);
            attack3.Add(0);

            AttackUpgradeList = new List<List<int>>();
            AttackUpgradeList.Add(attack1);
            AttackUpgradeList.Add(attack2);
            AttackUpgradeList.Add(attack3);
            #endregion


        }
        /// <summary>
        /// Current Experience on Weapon
        /// </summary>
        public int Experience { get; set; }
        /// <summary>
        /// Experience needed to level up
        /// </summary>
        public int ExperienceCap { get; set; }

        /// <summary>
        /// Shoots a bullet with a chosen color. Returns the ID of a potential gameObject.
        /// Returns -1 if the bullet hits a platform or gets out of range.
        /// </summary>
        /// <param name="position">The starting position</param>
        /// <param name="movement">A vector representing the direction. Doesn't need to be normalized</param>
        /// <param name="damage">The damage the bullet will deal</param>
        /// <param name="range">Amount of times "movement" is added to the position</param>
        /// <param name="rectangles"></param>
        /// <param name="gameObjects"></param>
        /// <param name="color">Color of the trail</param>
        /// <returns></returns>
        public virtual int ShootBullet(Room room, Vector2 pos, Vector2 movement, float damage, int range, List<Platform> platforms, List<GameObject> gameObjects, Color color, Player player)
        {
            drawBulletColor = color;

            Vector2 srcPos = pos;

            // movement = Game1.GetNormalVector(movement);

            for (int i = 0; i < range / 10; i++)
            {
                for (int j = 0; j < platforms.Count; j++)
                {
                    if (platforms[j].rectangle.Contains(pos.ToPoint()))
                    {
                        //Hit wall/floor
                        drawBulletRects.Add(new Rectangle((int)srcPos.X, (int)srcPos.Y, (int)pos.X, (int)pos.Y));
                        //ADD STUFF LIKE PARTICLES ETC. HERE
                        return -1;
                    }
                }

                for (int j = 0; j < gameObjects.Count; j++)
                {
                    if (gameObjects[j].GetHitBox().Contains(pos.ToPoint()))
                    {
                        if (gameObjects[j] != player && gameObjects[j].type != GameObject.Types.Projectile)
                        {
                            //Hit a gameObject
                            drawBulletRects.Add(new Rectangle((int)srcPos.X, (int)srcPos.Y, (int)pos.X, (int)pos.Y));
                            //ADD STUFF LIKE PARTICLES, DAMAGE ETC. HERE
                            gameObjects[j].TakeDamage(room, 0, damage);
                            return gameObjects[j].ID;
                        }
                    }
                }

                pos += movement * 10;
            }


            drawBulletRects.Add(new Rectangle((int)srcPos.X, (int)srcPos.Y, (int)(pos.X), (int)(pos.Y)));
            return -1;
        }

        public virtual void Check_Attack3(bool leftHand)
        {
            if (!Game1.Controller_Connected)
            {
                if (leftHand)
                {
                    if (keyboard.IsKeyDown(Keys.E) && (e_attacks == e_Attacks.a0 || e_attacks == e_Attacks.a3))
                    {
                        e_attacks = e_Attacks.a3;
                        Attack3();
                    }
                    else if (keyboardPrev.IsKeyDown(Keys.E) && e_attacks == e_Attacks.a3)
                    {
                        if (e_attacks != e_Attacks.a0)
                        {
                            releaseCharge = true;
                        }
                        e_attacks = e_Attacks.a0;
                    }
                }
                else
                {
                    if (keyboard.IsKeyDown(Keys.D) && (e_attacks == e_Attacks.a0 || e_attacks == e_Attacks.a3))
                    {
                        e_attacks = e_Attacks.a3;
                        Attack3();
                    }
                    else if (keyboardPrev.IsKeyDown(Keys.D) && e_attacks == e_Attacks.a3)
                    {
                        if (e_attacks != e_Attacks.a0)
                        {
                            releaseCharge = true;
                        }
                        e_attacks = e_Attacks.a0;
                    }
                }
            }
            else
            {
                if (leftHand)
                {
                    if (Game1.Controller.Buttons.X == ButtonState.Pressed && (e_attacks == e_Attacks.a0 || e_attacks == e_Attacks.a3))
                    {
                        e_attacks = e_Attacks.a3;
                        Attack3();
                    }
                    else if (Game1.Controller.Buttons.X == ButtonState.Released && e_attacks == e_Attacks.a3)
                    {
                        if (e_attacks != e_Attacks.a0)
                        {
                            releaseCharge = true;
                        }
                        e_attacks = e_Attacks.a0;
                    }
                }
                else
                {
                    if (Game1.Controller.Buttons.Y == ButtonState.Pressed && (e_attacks == e_Attacks.a0 || e_attacks == e_Attacks.a3))
                    {
                        e_attacks = e_Attacks.a3;
                        Attack3();
                    }
                    else if (Game1.Controller.Buttons.Y == ButtonState.Released && e_attacks == e_Attacks.a3)
                    {
                        if (e_attacks != e_Attacks.a0)
                        {
                            releaseCharge = true;
                        }
                        e_attacks = e_Attacks.a0;
                    }
                }
            }
        }
        public virtual void Check_Attack2(bool leftHand)
        {
            if (!Game1.Controller_Connected)
            {
                if (leftHand)
                {
                    if (keyboard.IsKeyDown(Keys.W) && (e_attacks == e_Attacks.a0 || e_attacks == e_Attacks.a2))
                    {
                        e_attacks = e_Attacks.a2;
                        Attack2();
                    }
                    else if (keyboardPrev.IsKeyDown(Keys.W) && e_attacks == e_Attacks.a2)
                    {
                        if (e_attacks != e_Attacks.a0)
                        {
                            releaseCharge = true;
                        }
                        e_attacks = e_Attacks.a0;
                    }
                }
                else
                {
                    if (keyboard.IsKeyDown(Keys.S) && (e_attacks == e_Attacks.a0 || e_attacks == e_Attacks.a2))
                    {
                        e_attacks = e_Attacks.a2;
                        Attack2();
                    }
                    else if (keyboardPrev.IsKeyDown(Keys.S) && e_attacks == e_Attacks.a2)
                    {
                        if (e_attacks != e_Attacks.a0)
                        {
                            releaseCharge = true;
                        }
                        e_attacks = e_Attacks.a0;
                    }
                }
            }
            else
            {
                if (leftHand)
                {
                    if (Game1.Controller.Buttons.LeftShoulder == ButtonState.Pressed && (e_attacks == e_Attacks.a0 || e_attacks == e_Attacks.a2))
                    {
                        e_attacks = e_Attacks.a2;
                        Attack2();
                    }
                    else if (Game1.Controller.Buttons.LeftShoulder == ButtonState.Released && e_attacks == e_Attacks.a2)
                    {
                        if (e_attacks != e_Attacks.a0)
                        {
                            releaseCharge = true;
                        }
                        e_attacks = e_Attacks.a0;
                    }
                }
                else
                {
                    if (Game1.Controller.Buttons.RightShoulder == ButtonState.Pressed && (e_attacks == e_Attacks.a0 || e_attacks == e_Attacks.a2))
                    {
                        e_attacks = e_Attacks.a2;
                        Attack2();
                    }
                    else if (Game1.Controller.Buttons.RightShoulder == ButtonState.Released && e_attacks == e_Attacks.a2)
                    {
                        if (e_attacks != e_Attacks.a0)
                        {
                            releaseCharge = true;
                        }
                        e_attacks = e_Attacks.a0;
                    }
                }
            }
        }
        public virtual void Check_Attack1(bool leftHand)
        {
            if (!Game1.Controller_Connected)
            {
                if (leftHand)
                {
                    if (keyboard.IsKeyDown(Keys.Q) && (e_attacks == e_Attacks.a0 || e_attacks == e_Attacks.a1))
                    {
                        e_attacks = e_Attacks.a1;
                        Attack1();
                    }
                    else if (keyboardPrev.IsKeyDown(Keys.Q) && e_attacks == e_Attacks.a1)
                    {
                        if (e_attacks != e_Attacks.a0)
                        {
                            releaseCharge = true;
                        }
                        e_attacks = e_Attacks.a0;
                    }
                }
                else
                {
                    if (keyboard.IsKeyDown(Keys.A) && (e_attacks == e_Attacks.a0 || e_attacks == e_Attacks.a1))
                    {
                        e_attacks = e_Attacks.a1;
                        Attack1();
                    }
                    else if (keyboardPrev.IsKeyDown(Keys.A) && e_attacks == e_Attacks.a1)
                    {
                        if (e_attacks != e_Attacks.a0)
                        {
                            releaseCharge = true;
                        }
                        e_attacks = e_Attacks.a0;
                    }
                }
            }
            else
            {
                if (leftHand)
                {
                    if (Game1.Controller.Triggers.Left > 0.5f && (e_attacks == e_Attacks.a0 || e_attacks == e_Attacks.a1))
                    {
                        e_attacks = e_Attacks.a1;
                        Attack1();
                    }
                    else if (Game1.Controller.Triggers.Left < 0.5f && e_attacks == e_Attacks.a1)
                    {
                        if (e_attacks != e_Attacks.a0)
                        {
                            releaseCharge = true;
                        }
                        e_attacks = e_Attacks.a0;
                    }
                }
                else
                {
                    if (Game1.Controller.Triggers.Right > 0.5f && (e_attacks == e_Attacks.a0 || e_attacks == e_Attacks.a1))
                    {
                        e_attacks = e_Attacks.a1;
                        Attack1();
                    }
                    else if (Game1.Controller.Triggers.Right < 0.5f && e_attacks == e_Attacks.a1)
                    {
                        if (e_attacks != e_Attacks.a0)
                        {
                            releaseCharge = true;
                        }
                        e_attacks = e_Attacks.a0;
                    }
                }
            }
        }

        public virtual void Attack1()
        {

        }
        public virtual void Attack2()
        {

        }
        public virtual void Attack3()
        {

        }

        public virtual void ChargeRelease(Projectile proj, Vector2 origin, Room room)
        {
            proj.Origin = origin;
            room.gameObjects.Add(proj);
            releaseCharge = false;
            ChargeTimer = 0;
        }

        public virtual void Update(Player player, Room room, bool leftHand)
        {
            attackTimer++;

            if (ChargeTimer > ChargeTimerCap) { ChargeTimer = ChargeTimerCap; }

            if (Experience >= ExperienceCap)
            {
                LevelUp();
            }

            if (player.state == Player.State.idle)
            {
                state = WeaponState.idle;
            }

            if ((player.movement.X >= 0.6f || player.movement.X <= -0.6f && player.onGround) && player.state != Player.State.idle)
            {
                state = WeaponState.run;
            }


            if (keyboard.IsKeyDown(Keys.E) || keyboard.IsKeyDown(Keys.W) || keyboard.IsKeyDown(Keys.Q) || Game1.Controller.Triggers.Left > 0.5f || Game1.Controller.Buttons.LeftShoulder == ButtonState.Pressed || Game1.Controller.Buttons.X == ButtonState.Pressed)
            {
                player.usingLeft = true;
            }
            else
            {
                player.usingLeft = false;
            }

            if (keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.S) || keyboard.IsKeyDown(Keys.D) || Game1.Controller.Triggers.Right > 0.5f || Game1.Controller.Buttons.RightShoulder == ButtonState.Pressed || Game1.Controller.Buttons.Y == ButtonState.Pressed)
            {
                player.usingRight = true;
            }
            else
            {
                player.usingRight = false;
            }
        }
        /// <summary>
        /// Level up function
        /// </summary>
        public void LevelUp()
        {
            Damage += 1.5f * Level;
            if (AttackSpeed < 6)
                AttackSpeed += 0.05f;
            hp += 3 * Level;

            int temp = Experience;
            Experience = 0;
            Experience += (temp - ExperienceCap);
            Level += 1;

        }
        /// <summary>
        /// Returns shooting origin for the type of hand and offset
        /// 
        /// </summary>
        /// <param name="playerPos"></param>
        /// <param name="xOffset"></param>
        /// <param name="yOffset"></param>
        /// <param name="leftHand"></param>
        /// <returns></returns>
        public Vector2 HandOrigin(Vector2 playerPos, float xOffset, float yOffset)
        {
            return new Vector2(playerPos.X + xOffset, playerPos.Y + yOffset);
        }

        public virtual void Draw(Player player, Random rand, SpriteBatch spriteBatch, GraphicsDevice graphics, Camera camera, bool leftHand)
        {
            while (drawBulletRects.Count > 0)
            {
                Game1.DrawLine(graphics, spriteBatch, new Vector2(drawBulletRects[0].X, drawBulletRects[0].Y), new Vector2(drawBulletRects[0].Width, drawBulletRects[0].Height), drawBulletColor);
                drawBulletRects.RemoveAt(0);
            }
        }

        public virtual void DrawLight()
        {

        }

        public class EnergyBlaster : Weapon
        {
            public class ExplodingEnergyBall : Projectile
            {
                public string smallerBall;
                List<GameObject.Types> attackable = new List<Types>();
                public ExplodingEnergyBall(int ID, string smallerball, string sprite, Vector2 pos, Vector2 vel, float damage, float knockback, float lifespan, Room room, bool friendly, bool useFriction, bool useGravity, bool useResistance, float bounceFactor, bool dieOnCollision, SpriteEffects spriteEffect, List<GameObject.Types> attackable, bool lookAtMovement, float friction, float resistance, float scale) : base(ID, sprite, pos, vel, damage, knockback, lifespan, room, friendly, useFriction, useGravity, useResistance, bounceFactor, dieOnCollision, spriteEffect, attackable, lookAtMovement, friction, resistance, scale)
                {
                    smallerBall = smallerball;
                    this.attackable = attackable;
                }

                public override void Update(Room room, Random rand)
                {
                    base.Update(room, rand);
                    if (!live)
                    {
                        /*
                        room.gameObjects.Add(new Projectile(smallerBall, position, new Vector2(0, 1) * new Vector2(5,5), Damage, Knockback, 0.1f, room, true, false, false, false, 0, true));
                        room.gameObjects.Add(new Projectile(smallerBall, position, new Vector2(1, 1) * new Vector2(5, 5), Damage, Knockback, 0.1f, room, true, false, false, false, 0, true));
                        room.gameObjects.Add(new Projectile(smallerBall, position, new Vector2(1, 0) * new Vector2(5, 5), Damage, Knockback, 0.1f, room, true, false, false, false, 0, true));
                        room.gameObjects.Add(new Projectile(smallerBall, position, new Vector2(1, -1) * new Vector2(5, 5), Damage, Knockback, 0.1f, room, true, false, false, false, 0, true));
                        room.gameObjects.Add(new Projectile(smallerBall, position, new Vector2(0, -1) * new Vector2(5, 5), Damage, Knockback, 0.1f, room, true, false, false, false, 0, true));
                        room.gameObjects.Add(new Projectile(smallerBall, position, new Vector2(-1, -1) * new Vector2(5, 5), Damage, Knockback, 0.1f, room, true, false, false, false, 0, true));
                        room.gameObjects.Add(new Projectile(smallerBall, position, new Vector2(-1, 0) * new Vector2(5, 5), Damage, Knockback, 0.1f, room, true, false, false, false, 0, true));
                        room.gameObjects.Add(new Projectile(smallerBall, position, new Vector2(-1, 1) * new Vector2(5, 5), Damage, Knockback, 0.1f, room, true, false, false, false, 0, true));
                    */
                        for (int i = 0; i < rand.Next(6, 12); i++)
                        {
                            room.gameObjects.Add(new Projectile(room.gameObjects.Count, smallerBall, position, new Vector2((float)rand.NextDouble() * rand.Next(-1, 2), (float)rand.NextDouble() * rand.Next(-1, 2)) * new Vector2(20, 20), Damage, Knockback, 0.1f + (float)rand.NextDouble() * 0.1f, room, true, false, false, false, 0, true, spriteEffect, attackable, true));
                        }
                        Animation animation = new Animation("sprite");
                        if (animation.animationEnd)
                            animation = new Animation("andra sprite");
                    }
                }
            }

            public class SmallWave : Projectile
            {
                Animation startAnimation, midAnimation;
                int aniController = 0;
                public SmallWave(int ID, Animation startAni, Animation midAni, Vector2 pos, Vector2 vel, float damage, float knockback, float lifespan, Room room, bool friendly, bool useFriction, bool useGravity, bool useResistance, float bounceFactor, bool dieOnCollision, SpriteEffects spriteEffect, List<GameObject.Types> attackable, bool lookAtMovement, float friction, float resistance, float scale) : base(ID, startAni, pos, vel, damage, knockback, lifespan, room, friendly, useFriction, useGravity, useResistance, bounceFactor, dieOnCollision, spriteEffect, attackable, lookAtMovement, friction, resistance, scale)
                {
                    startAnimation = startAni;
                    midAnimation = midAni;
                    position = pos;

                }
                public override void Draw(Point offset, Random rand, SpriteBatch spriteBatch, GraphicsDevice graphics, Camera camera, Color color)
                {
                    if (aniController == 0)
                    {
                        startAnimation.Draw(rand, spriteBatch, camera, position, spriteEffect, 0.50f);
                        if (startAnimation.animationEnd)
                        {
                            aniController++;
                        }
                    }
                    if (aniController == 1)
                    {
                        midAnimation.Draw(rand, spriteBatch, camera, position, spriteEffect, 0.50f);
                    }

                }
            }

            //public class Laser : Projectile
            //{
            //    public Laser(int ID, Animation ani, Vector2 pos, Vector2 vel, float damage, float knockback, float lifespan, Room room, bool friendly, bool useFriction, bool useGravity, bool useResistance, float bounceFactor, bool dieOnCollision, SpriteEffects spriteEffect, List<GameObject.Types> attackable, float friction, float resistance, float scale) : base(ID, startAni, pos, vel, damage, knockback, lifespan, room, friendly, useFriction, useGravity, useResistance, bounceFactor, dieOnCollision, spriteEffect, attackable, friction, resistance, scale)
            //    {

            //        position = pos;

            //    }
            //}
            private Random rand;
            private string spr_energyballSmall, spr_energyballCharged, spr_smallWave, spr_biggerWave, spr_xWave;
            public List<GameObject.Types> attackable = new List<GameObject.Types>();
            public EnergyBlaster(GraphicsDevice graphicsDevice) : base()
            {
                rand = new Random();
                spr_energyballSmall = "Energy_Attack1_1";
                spr_energyballCharged = "Energy_Attack1_2";
                spr_smallWave = "Energy_Attack2_1";
                spr_biggerWave = "Energy_Attack2_2";
                spr_xWave = "Energy_Attack2_3";
                spr_lFall = "Energy_lFall";
                spr_lIdle = "Energy_lIdle";
                spr_lJump = "Energy_lJump";
                spr_lStall = "Energy_lStall";
                spr_lRunning = "Energy_lRunning";
                spr_lShoot1_1 = "Energy_lShoot1_1";
                spr_lShoot2_1 = "Energy_lShoot2_1";
                spr_rShoot1_1 = "Energy_rShoot1_1";
                spr_rShoot2_1 = "Energy_rShoot2_1";
                spr_rFall = "Energy_rFall";
                spr_rIdle = "Energy_rIdle";
                spr_rJump = "Energy_rJump";
                spr_rStall = "Energy_rStall";
                spr_rRunning = "Energy_rRunning";

                attackable.Add(GameObject.Types.Enemy);
                attackable.Add(GameObject.Types.Prop);




                this.Name = "Energiya pushki";
                this.Level = 1;
                this.Experience = 0;
                this.ExperienceCap = 1000;
                this.Damage = 1;
                this.AttackSpeed = 5;
                AttackUpgradeList[0][0] = 1;
                AttackUpgradeList[1][0] = 1;
                AttackUpgradeList[2][0] = 1;
                hp = 25;


            }

            public override void Update(Player player, Room room, bool leftHand)
            {
                base.Update(player, room, leftHand);
                keyboard = Keyboard.GetState();
                SpriteHandler.sprites[spr_lRunning].speed = SpriteHandler.sprites[player.spr_running].speed;
                SpriteHandler.sprites[spr_rRunning].speed = SpriteHandler.sprites[player.spr_running].speed;
                if (spriteEffect != SpriteEffects.FlipHorizontally)
                {

                    if (leftHand)
                    {
                        if (player.state == Player.State.running || player.state == Player.State.back)
                        {
                            ShootOrigin = HandOrigin(player.position, 50 + 5, 53) + player.textureOffset;
                        }
                        else
                        {
                            ShootOrigin = HandOrigin(player.position, 50 + 15, 35) + player.textureOffset;
                        }
                    }
                    else
                    {
                        if (player.state == Player.State.running || player.state == Player.State.back)
                        {
                            ShootOrigin = HandOrigin(player.position, 50 - 6, 53) + player.textureOffset;
                        }
                        else
                        {
                            ShootOrigin = HandOrigin(player.position, 50 - 5, 33) + player.textureOffset;
                        }
                    }
                }
                if (spriteEffect == SpriteEffects.FlipHorizontally)
                {
                    if (leftHand)
                    {
                        if (player.state == Player.State.running || player.state == Player.State.back)
                        {
                            ShootOrigin = HandOrigin(player.position, -10, 53) + player.textureOffset;
                        }
                        else
                        {
                            ShootOrigin = HandOrigin(player.position, 50 - 70, 35) + player.textureOffset;
                        }
                    }
                    else
                    {
                        if (player.state == Player.State.running || player.state == Player.State.back)
                        {
                            ShootOrigin = HandOrigin(player.position, 10, 53) + player.textureOffset;
                        }
                        else
                        {
                            ShootOrigin = HandOrigin(player.position, 5, 33) + player.textureOffset;
                        }
                    }
                }

                Check_Attack1(leftHand);
                Check_Attack2(leftHand);
                Check_Attack3(leftHand);

                if (e_attacks == e_Attacks.a1)
                {

                    if (AttackUpgradeList[0][2] == 1)
                    {
                        hasCharge = true;
                        if (player.state != Player.State.running && player.state != Player.State.back) { state = WeaponState.shoot1_1; }
                        ChargeTimer++;
                        ChargeTimerCap = 60 * 2;
                        chargedProj = new ExplodingEnergyBall(room.gameObjects.Count, spr_energyballSmall, spr_energyballCharged, ShootOrigin - new Vector2(0, SpriteHandler.sprites[spr_energyballCharged].height / 2), new Vector2(10 * player.direction, 0), Damage, 0, 0.2f + (float)(ChargeTimer / ChargeTimerCap) * 1.5f, room, true, false, false, false, 0, true, spriteEffect, attackable, true, 0, 0, (float)(ChargeTimer / ChargeTimerCap));
                    }
                    else if (AttackUpgradeList[0][1] == 1)
                    {
                        hasCharge = true;
                        if (player.state != Player.State.running && player.state != Player.State.back)
                        {
                            state = WeaponState.shoot1_1;
                        }
                        ChargeTimer++;
                        ChargeTimerCap = 90 / AttackSpeed;
                        Animation ani = new Animation(spr_energyballCharged);
                        chargedProj = new Projectile(room.gameObjects.Count, spr_energyballCharged, ShootOrigin - new Vector2(0, SpriteHandler.sprites[spr_energyballCharged].height / 2), new Vector2(10 * player.direction, 0), Damage, 0, 0.2f + (float)(ChargeTimer / ChargeTimerCap) * 1.5f, room, true, false, false, false, 0, true, spriteEffect, attackable, true, 0, 0, (float)(ChargeTimer / ChargeTimerCap));
                    }
                    else if (AttackUpgradeList[0][0] == 1)
                    {
                        hasCharge = false;
                        if (player.state != Player.State.running && player.state != Player.State.back)
                        {
                            state = WeaponState.shoot1_1;
                        }
                        if (attackTimer >= (float)(60 / (float)(AttackSpeed * 1.2)))
                        {
                            SpriteHandler.sprites[spr_lShoot1_1].currentFrame = 0;
                            SpriteHandler.sprites[spr_rShoot1_1].currentFrame = 0;
                            SpriteHandler.sprites[spr_rShoot1_1].speed = 0.7f;
                            SpriteHandler.sprites[spr_lShoot1_1].speed = 0.7f;
                            float temp = (float)rand.NextDouble() + 0.5f;
                            Animation ani = new Animation(spr_energyballSmall);
                            ani.speed = (6 / temp) / 60;
                            room.gameObjects.Add(new Projectile(room.gameObjects.Count, ani, ShootOrigin - new Vector2(0, SpriteHandler.sprites[spr_energyballSmall].height / 4), new Vector2(15 * player.direction, 0), Damage, 0, temp, room, true, false, false, false, 0, true, spriteEffect, attackable, true));
                            attackTimer = 0;
                        }
                    }
                    if (SpriteHandler.sprites[spr_rShoot1_1].animationEnd) { SpriteHandler.sprites[spr_rShoot1_1].speed = 0f; }
                    if (SpriteHandler.sprites[spr_lShoot1_1].animationEnd) { SpriteHandler.sprites[spr_lShoot1_1].speed = 0f; }
                }
                if (e_attacks == e_Attacks.a2)
                {
                    if (AttackUpgradeList[1][2] == 1)
                    {
                        hasCharge = false;
                        if (player.state != Player.State.running && player.state != Player.State.back)
                        {
                            state = WeaponState.shoot2_1;
                        }
                        room.gameObjects.Add(new Projectile(room.gameObjects.Count, spr_xWave, ShootOrigin - new Vector2(0, SpriteHandler.sprites[spr_xWave].height / 3), new Vector2(10 * player.direction, 0), Damage, 1, 2, room, true, false, false, false, 0, true, spriteEffect, attackable, true));
                    }
                    else if (AttackUpgradeList[1][1] == 1)
                    {
                        hasCharge = false;
                        if (player.state != Player.State.running && player.state != Player.State.back)
                        {
                            state = WeaponState.shoot2_1;
                        }
                        room.gameObjects.Add(new Projectile(room.gameObjects.Count, spr_biggerWave, ShootOrigin - new Vector2(0, SpriteHandler.sprites[spr_biggerWave].height / 3), new Vector2(13 * player.direction, 0), Damage, 2, 3, room, true, false, false, false, 0, true, spriteEffect, attackable, true));
                    }
                    else if (AttackUpgradeList[1][0] == 1)
                    {
                        float attackspdDeviation = 0.5f;
                        hasCharge = false;
                        if (state != WeaponState.run)
                        {
                            state = WeaponState.shoot2_1;
                        }
                        if (attackTimer >= (float)(60 / (float)(AttackSpeed * attackspdDeviation)))
                        {
                            SpriteHandler.sprites[spr_rShoot2_1].speed = 0.7f;
                            SpriteHandler.sprites[spr_lShoot2_1].speed = 0.7f;
                            SpriteHandler.sprites[spr_rShoot2_1].currentFrame = 0;
                            SpriteHandler.sprites[spr_lShoot2_1].currentFrame = 0;
                            Animation startAni = new Animation("Energy_Attack2_1Start");
                            startAni.speed = 0.4f;
                            Animation midAni = new Animation("Energy_Attack2_1Live");
                            midAni.speed = 0.7f;
                            room.gameObjects.Add(new SmallWave(room.gameObjects.Count, startAni, midAni, ShootOrigin - new Vector2(0, startAni.Height / 3), new Vector2(15 * player.direction, 0), Damage, 2, rand.Next(1, 3) * 0.5f, room, true, false, false, false, 0, true, spriteEffect, attackable, true, 0, 0, 1));
                            attackTimer = 0;
                        }
                        if (SpriteHandler.sprites[spr_rShoot2_1].animationEnd) { SpriteHandler.sprites[spr_rShoot2_1].speed = 0f; }
                        if (SpriteHandler.sprites[spr_lShoot2_1].animationEnd) { SpriteHandler.sprites[spr_lShoot2_1].speed = 0f; }
                    }
                }
                if (e_attacks == e_Attacks.a3)
                {
                    if (AttackUpgradeList[1][1] == 1)
                    {

                    }
                    else if (AttackUpgradeList[1][0] == 1)
                    {

                    }
                }

                if (hasCharge && releaseCharge)
                {
                    ChargeRelease(chargedProj, ShootOrigin, room);
                }

                keyboardPrev = Keyboard.GetState();
            }


            public override void Draw(Player player, Random rand, SpriteBatch spriteBatch, GraphicsDevice graphics, Camera camera, bool leftHand)
            {
                base.Draw(player, rand, spriteBatch, graphics, camera, leftHand);
                if (player.direction < 0)
                    spriteEffect = SpriteEffects.FlipHorizontally;
                else
                    spriteEffect = SpriteEffects.None;

                Vector2 drawPos = new Vector2(player.Origin.X - SpriteHandler.sprites[spr_lIdle].width / 2, player.Rectangle.Bottom - player.height) + player.textureOffset;
                if (leftHand)
                {
                    switch (state)
                    {
                        case WeaponState.idle:
                            currentSprite = spr_lIdle;
                            SpriteHandler.Draw(spr_lIdle, rand, spriteBatch, camera, drawPos - new Vector2(1 * player.direction, 0), spriteEffect, leftDepth);
                            break;
                        case WeaponState.jump:
                            currentSprite = spr_lJump;
                            SpriteHandler.Draw(spr_lJump, rand, spriteBatch, camera, drawPos, spriteEffect, leftDepth);
                            break;
                        case WeaponState.run:
                            currentSprite = spr_lRunning;
                            SpriteHandler.Draw(spr_lRunning, rand, spriteBatch, camera, drawPos, spriteEffect, leftDepth);
                            break;
                        case WeaponState.fall:
                            currentSprite = spr_lFall;
                            SpriteHandler.Draw(spr_lFall, rand, spriteBatch, camera, drawPos, spriteEffect, leftDepth);
                            break;
                        case WeaponState.stall:
                            currentSprite = spr_lStall;
                            SpriteHandler.Draw(spr_lStall, rand, spriteBatch, camera, drawPos, spriteEffect, leftDepth);
                            break;
                        case WeaponState.shoot1_1:
                            currentSprite = spr_lShoot1_1;
                            SpriteHandler.Draw(spr_lShoot1_1, rand, spriteBatch, camera, drawPos, spriteEffect, leftDepth);
                            break;
                        case WeaponState.shoot1_2:
                            currentSprite = spr_lShoot1_1;
                            SpriteHandler.Draw(spr_lShoot1_1, rand, spriteBatch, camera, drawPos, spriteEffect, leftDepth);
                            break;
                        case WeaponState.shoot1_3:
                            currentSprite = spr_lShoot1_1;
                            SpriteHandler.Draw(spr_lShoot1_1, rand, spriteBatch, camera, drawPos, spriteEffect, leftDepth);
                            break;
                        case WeaponState.shoot2_1:
                            currentSprite = spr_lShoot2_1;
                            SpriteHandler.Draw(spr_lShoot2_1, rand, spriteBatch, camera, drawPos, spriteEffect, leftDepth);
                            break;
                        case WeaponState.shoot2_2:
                            currentSprite = spr_lShoot2_1;
                            SpriteHandler.Draw(spr_lShoot2_1, rand, spriteBatch, camera, drawPos, spriteEffect, leftDepth);
                            break;
                        case WeaponState.shoot2_3:
                            currentSprite = spr_lShoot2_1;
                            SpriteHandler.Draw(spr_lShoot2_1, rand, spriteBatch, camera, drawPos, spriteEffect, leftDepth);
                            break;
                        case WeaponState.shoot3_1:
                            currentSprite = spr_lShoot2_1;
                            SpriteHandler.Draw(spr_lShoot2_1, rand, spriteBatch, camera, drawPos, spriteEffect, leftDepth);
                            break;
                        case WeaponState.shoot3_2:
                            currentSprite = spr_lShoot2_1;
                            SpriteHandler.Draw(spr_lShoot2_1, rand, spriteBatch, camera, drawPos, spriteEffect, leftDepth);
                            break;
                        default:
                            break;
                    }

                }
                else
                {
                    switch (state)
                    {
                        case WeaponState.idle:
                            currentSprite = spr_rIdle;
                            SpriteHandler.Draw(spr_rIdle, rand, spriteBatch, camera, drawPos, spriteEffect, rightDepth);
                            break;
                        case WeaponState.jump:
                            currentSprite = spr_rJump;
                            SpriteHandler.Draw(spr_rJump, rand, spriteBatch, camera, drawPos, spriteEffect, rightDepth);
                            break;
                        case WeaponState.run:
                            currentSprite = spr_rRunning;
                            SpriteHandler.Draw(spr_rRunning, rand, spriteBatch, camera, drawPos, spriteEffect, rightDepth);
                            break;
                        case WeaponState.fall:
                            currentSprite = spr_rFall;
                            SpriteHandler.Draw(spr_rFall, rand, spriteBatch, camera, drawPos, spriteEffect, rightDepth);
                            break;
                        case WeaponState.stall:
                            currentSprite = spr_rStall;
                            SpriteHandler.Draw(spr_rStall, rand, spriteBatch, camera, drawPos, spriteEffect, leftDepth);
                            break;
                        case WeaponState.shoot1_1:
                            currentSprite = spr_rShoot1_1;
                            SpriteHandler.Draw(spr_rShoot1_1, rand, spriteBatch, camera, drawPos, spriteEffect, rightDepth);
                            break;
                        case WeaponState.shoot1_2:
                            currentSprite = spr_rShoot1_1;
                            SpriteHandler.Draw(spr_rShoot1_1, rand, spriteBatch, camera, drawPos, spriteEffect, rightDepth);
                            break;
                        case WeaponState.shoot1_3:
                            currentSprite = spr_rShoot1_1;
                            SpriteHandler.Draw(spr_rShoot1_1, rand, spriteBatch, camera, drawPos, spriteEffect, rightDepth);
                            break;
                        case WeaponState.shoot2_1:
                            currentSprite = spr_rShoot2_1;
                            SpriteHandler.Draw(spr_rShoot2_1, rand, spriteBatch, camera, drawPos, spriteEffect, rightDepth);
                            break;
                        case WeaponState.shoot2_2:
                            currentSprite = spr_rShoot2_1;
                            SpriteHandler.Draw(spr_rShoot2_1, rand, spriteBatch, camera, drawPos, spriteEffect, rightDepth);
                            break;
                        case WeaponState.shoot2_3:
                            currentSprite = spr_rShoot2_1;
                            SpriteHandler.Draw(spr_rShoot2_1, rand, spriteBatch, camera, drawPos, spriteEffect, rightDepth);
                            break;
                        case WeaponState.shoot3_1:
                            currentSprite = spr_rShoot2_1;
                            SpriteHandler.Draw(spr_rShoot2_1, rand, spriteBatch, camera, drawPos, spriteEffect, rightDepth);
                            break;
                        case WeaponState.shoot3_2:
                            currentSprite = spr_rShoot2_1;
                            SpriteHandler.Draw(spr_rShoot2_1, rand, spriteBatch, camera, drawPos, spriteEffect, rightDepth);
                            break;
                        default:
                            break;
                    }
                }

            }
        }

        public class RedBlaster : Weapon
        {
            private Random rand;
            public string spr_plasmaBall, spr_smallParticle, spr_smallerParticle, spr_plasmaUlt;
            bool firstTime = true;
            public DamageZone dmgZone;



            public class ExplodingBall : Projectile
            {
                public List<GameObject.Types> attackable = new List<GameObject.Types>();
                public ExplodingBall(int ID, Animation ani, Vector2 pos, Vector2 vel, float damage, float knockback, float lifespan, Room room, bool friendly, bool useFriction, bool useGravity, bool useResistance, float bounceFactor, bool dieOnCollision, SpriteEffects spriteEffect, List<GameObject.Types> attackable, bool lookAtMovement) : base(ID, ani, pos, vel, damage, knockback, lifespan, room, friendly, useFriction, useGravity, useResistance, 0f, true, spriteEffect, attackable, lookAtMovement)
                {
                    this.attackable = attackable;
                }

                public override void Update(Room room, Random rand)
                {
                    base.Update(room, rand);
                    if (!live)
                    {
                        for (int i = 0; i < 1000; i++)
                        {
                            int random = rand.Next(0, 10);
                            Particle.Static plasmaSpew;
                            if (random >= 6)
                            {
                                Vector2 dirTemp = new Vector2(rand.Next(-1, 2), rand.Next(-1, 2));
                                Vector2 speedTemp = new Vector2(rand.Next(1, 40), rand.Next(1, 40));
                                plasmaSpew = new Particle.Static(room.gameObjects.Count, "Red_SmallParticle", rand, position, dirTemp * speedTemp, new Vector2(0.9f, 0.9f), 1f, 0.2f, (int)(rand.NextDouble() * rand.Next(5, 20)), Color.Red);
                            }
                            else
                            {
                                plasmaSpew = new Particle.Static(room.gameObjects.Count, "Red_SmallerParticle", rand, position, new Vector2(rand.Next(-1, 2), rand.Next(-1, 2)) * new Vector2(rand.Next(1, 40), rand.Next(1, 40)), new Vector2(0.93f, 0.93f), 1f, 0.2f, (int)(rand.NextDouble() * rand.Next(5, 20)), Color.Red);
                            }

                            room.particles.Add(plasmaSpew);
                        }
                        Game1.camera.AddShake(new Vector2(6, 8), 3);
                        room.damageZones.Add(new DamageZone(new Rectangle((int)position.X - 112, (int)position.Y - 112, 225, 225), Damage, 60, 30, ID, true, false, attackable));
                        remove = true;
                    }
                }
                public override void Die(Room room, Random rand)
                {
                    live = false;
                }
            }
            public RedBlaster(GraphicsDevice graphicsDevice) : base()
            {

                rand = new Random();
                spr_plasmaBall = "Red_Attack1_1";
                spr_smallParticle = "Red_SmallParticle";
                spr_smallerParticle = "Red_SmallerParticle";
                spr_lFall = "Red_lFall";
                spr_lIdle = "Red_lIdle";
                spr_lJump = "Red_lJump";
                spr_lRunning = "Red_lRunning";
                spr_lShoot1_1 = "Red_lShoot1_1";
                spr_lShoot2_1 = "Red_lShoot2_1";
                spr_rShoot1_1 = "Red_rShoot1_1";
                spr_rShoot2_1 = "Red_rShoot2_1";
                spr_rFall = "Red_rFall";
                spr_rIdle = "Red_rIdle";
                spr_rJump = "Red_rJump";
                spr_rRunning = "Red_rRunning";
                spr_plasmaUlt = "Red_Attack3_1";
                attackable.Add(GameObject.Types.Enemy);
                attackable.Add(GameObject.Types.Prop);




                this.Name = "Plasma Shooter";
                this.Level = 1;
                this.Experience = 0;
                this.ExperienceCap = 1000;
                this.Damage = 1;
                this.AttackSpeed = 5;
                AttackUpgradeList[0][0] = 1;
                AttackUpgradeList[1][2] = 1;
                AttackUpgradeList[2][0] = 1;
                hp = 25;


            }

            public class SplitBall : Projectile
            {
                public List<GameObject.Types> attackable = new List<GameObject.Types>();
                bool lastUpgrade;
                public SplitBall(int ID, Animation ani, Vector2 pos, Vector2 vel, float damage, float knockback, float lifespan, Room room, bool friendly, bool useFriction, bool useGravity, bool useResistance, float bounceFactor, bool dieOnCollision, SpriteEffects spriteEffect, List<GameObject.Types> attackable, bool lookAtMovement, bool LastUpgrade) : base(ID, ani, pos, vel, damage, knockback, lifespan, room, friendly, useFriction, useGravity, useResistance, 0f, true, spriteEffect, attackable, lookAtMovement)
                {
                    lastUpgrade = LastUpgrade;
                    this.attackable = attackable;
                }

                public override void Update(Room room, Random rand)
                {
                    base.Update(room, rand);
                    for (int i = 0; i < 10; i++)
                    {
                        int random = rand.Next(0, 10);
                        Particle.Static plasmaSpew;
                        if (random >= 6)
                        {
                            plasmaSpew = new Particle.Static(room.gameObjects.Count, "BAMF_Particle", rand, position, new Vector2(movement.X - (movement.X / Math.Abs(movement.X) * rand.Next(Math.Abs(Game1.FloorAdv(movement.X * 0.9f)), Math.Abs(Game1.FloorAdv(movement.X * 1.2f)))), movement.Y - (movement.Y / Math.Abs(movement.Y) * rand.Next(Math.Abs(Game1.FloorAdv(movement.Y * 0.9f)), Math.Abs(Game1.FloorAdv(movement.Y * 1.2f))))), new Vector2(1f, 1f), new Vector2(2, 1), 0.3f, rand.Next(2, 10), Color.Red);
                        }
                        else
                        {
                            plasmaSpew = new Particle.Static(room.gameObjects.Count, "BAMF_Particle", rand, position, new Vector2(movement.X - (movement.X / Math.Abs(movement.X) * rand.Next(Math.Abs(Game1.FloorAdv(movement.X * 0.9f)), Math.Abs(Game1.FloorAdv(movement.X * 1.2f)))), movement.Y - (movement.Y / Math.Abs(movement.Y) * rand.Next(Math.Abs(Game1.FloorAdv(movement.Y * 0.9f)), Math.Abs(Game1.FloorAdv(movement.Y * 1.2f))))), new Vector2(1f, 1f), new Vector2(1, 1), 0.3f, rand.Next(2, 10), Color.Red);
                        }
                        room.particles.Add(plasmaSpew);
                    }
                    if (!live)
                    {
                        if (lastUpgrade)
                        {
                            room.gameObjects.Add(new SplitBall(room.gameObjects.Count, new Animation("Red_Attack1_1"), position, new Vector2(rand.Next(2, 10), rand.Next(-15, -4)), Damage, 1, 2, room, true, false, true, false, 0, true, spriteEffect, attackable, true, false));
                            room.gameObjects.Add(new SplitBall(room.gameObjects.Count, new Animation("Red_Attack1_1"), position, new Vector2(rand.Next(-10, -2), rand.Next(-15, -4)), Damage, 1, 2, room, true, false, true, false, 0, true, spriteEffect, attackable, true, false));
                        }
                        else
                        {
                            room.gameObjects.Add(new Projectile(room.gameObjects.Count, new Animation("Red_Attack1_1"), position, new Vector2(rand.Next(2, 10), rand.Next(-15, -4)), Damage, 1, 2, room, true, false, true, false, 0, true, spriteEffect, attackable, true, 0f, 0.8f, 1f));
                            room.gameObjects.Add(new Projectile(room.gameObjects.Count, new Animation("Red_Attack1_1"), position, new Vector2(rand.Next(-10, -2), rand.Next(-15, -4)), Damage, 1, 2, room, true, false, true, false, 0, true, spriteEffect, attackable, true, 0f, 0.8f, 1f));
                        }
                        //Game1.camera.AddShake(new Vector2(6, 8), 3);
                        remove = true;
                    }
                }
                public override void Die(Room room, Random rand)
                {
                    live = false;
                }
            }

            public override void Update(Player player, Room room, bool leftHand)
            {

                base.Update(player, room, leftHand);
                keyboard = Keyboard.GetState();
                SpriteHandler.sprites[spr_lRunning].speed = SpriteHandler.sprites[player.spr_running].speed;
                SpriteHandler.sprites[spr_rRunning].speed = SpriteHandler.sprites[player.spr_running].speed;
                if (spriteEffect != SpriteEffects.FlipHorizontally)
                {

                    if (leftHand)
                    {
                        if (player.state == Player.State.running || player.state == Player.State.back)
                        {
                            ShootOrigin = HandOrigin(player.position, 50 + 10, 53) + player.textureOffset;
                        }
                        else
                        {
                            ShootOrigin = HandOrigin(player.position, 50 + 15, 33) + player.textureOffset;
                        }
                    }
                    else
                    {
                        if (player.state == Player.State.running || player.state == Player.State.back)
                        {
                            ShootOrigin = HandOrigin(player.position, 50 - 6, 53) + player.textureOffset;
                        }
                        else
                        {
                            ShootOrigin = HandOrigin(player.position, 50 - 5, 33) + player.textureOffset;
                        }
                    }
                }
                if (spriteEffect == SpriteEffects.FlipHorizontally)
                {
                    if (leftHand)
                    {
                        if (player.state == Player.State.running || player.state == Player.State.back)
                        {
                            ShootOrigin = HandOrigin(player.position, 50 - 60, 53) + player.textureOffset;
                        }
                        else
                        {
                            ShootOrigin = HandOrigin(player.position, 50 - 65, 33) + player.textureOffset;
                        }
                    }
                    else
                    {
                        if (player.state == Player.State.running || player.state == Player.State.back)
                        {
                            ShootOrigin = HandOrigin(player.position, 10, 53) + player.textureOffset;
                        }
                        else
                        {
                            ShootOrigin = HandOrigin(player.position, 5, 33) + player.textureOffset;
                        }
                    }
                }

                Check_Attack1(leftHand);
                Check_Attack2(leftHand);
                Check_Attack3(leftHand);

                if (e_attacks == e_Attacks.a1)
                {

                    if (AttackUpgradeList[0][2] == 1)
                    {
                        hasCharge = false;
                        if (player.state != Player.State.running && player.state != Player.State.back) { state = WeaponState.shoot1_1; }
                        if (attackTimer >= (float)(60 / (float)(AttackSpeed * 0.5f)))
                        {
                            attackTimer = 0;
                            room.gameObjects.Add(new SplitBall(room.gameObjects.Count, new Animation("Red_Attack1_1"), ShootOrigin, new Vector2(rand.Next(17, 22) * player.direction, rand.Next(-9, -7)), Damage, 1, 2, room, true, false, true, false, 0, true, spriteEffect, attackable, true, true));
                        }
                    }
                    else if (AttackUpgradeList[0][1] == 1)
                    {
                        hasCharge = false;
                        if (player.state != Player.State.running && player.state != Player.State.back)
                        {
                            state = WeaponState.shoot1_1;
                        }
                        if (attackTimer >= (float)(60 / (float)(AttackSpeed * 0.5f)))
                        {
                            attackTimer = 0;
                            room.gameObjects.Add(new SplitBall(room.gameObjects.Count, new Animation("Red_Attack1_1"), ShootOrigin, new Vector2(rand.Next(17, 22) * player.direction, rand.Next(-9, -7)), Damage, 1, 2, room, true, false, true, false, 0, true, spriteEffect, attackable, true, false));
                        }
                    }
                    else if (AttackUpgradeList[0][0] == 1)
                    {
                        hasCharge = false;
                        if (player.state != Player.State.running && player.state != Player.State.back)
                        {
                            state = WeaponState.shoot1_1;
                        }
                        if (attackTimer >= (float)(60 / (float)(AttackSpeed * 0.5f)))
                        {
                            Animation ani = new Animation(spr_plasmaBall);
                            room.gameObjects.Add(new Projectile(room.gameObjects.Count, ani, ShootOrigin - new Vector2(0, SpriteHandler.sprites[spr_plasmaBall].height / 3), new Vector2(rand.Next(17, 22) * player.direction, rand.Next(-9, -7)), Damage, 1, 2, room, true, false, true, false, 0, true, spriteEffect, attackable, true, 0f, 0.8f, 1f));
                            attackTimer = 0;
                        }
                    }
                    if (SpriteHandler.sprites[spr_rShoot1_1].animationEnd) { SpriteHandler.sprites[spr_rShoot1_1].speed = 0f; }
                    if (SpriteHandler.sprites[spr_lShoot1_1].animationEnd) { SpriteHandler.sprites[spr_lShoot1_1].speed = 0f; }
                }
                if (e_attacks == e_Attacks.a2)
                {

                    if (AttackUpgradeList[1][2] == 1)
                    {
                        hasCharge = false;
                        if (player.state != Player.State.running && player.state != Player.State.back)
                        {
                            state = WeaponState.shoot1_1;
                        }
                        if (firstTime)
                        {
                            dmgZone = new DamageZone(new Rectangle((int)(player.position.X + player.width), (int)(player.position.Y - 50), 350, player.height + 100), Damage, 10, 0, player.ID, true, true, attackable);
                            room.damageZones.Add(dmgZone);
                            firstTime = false;
                        }
                        if (player.direction == 1)
                        {
                            dmgZone.Position = new Vector2(player.position.X + (player.width), player.position.Y - 50);
                        }
                        if (player.direction == -1)
                        {
                            dmgZone.Position = new Vector2(player.position.X - dmgZone.rectangle.Width, player.position.Y - 50);
                        }
                        for (int i = 0; i < 50; i++)
                        {
                            int random = rand.Next(0, 10);
                            Particle.Static plasmaSpew;
                            if (random >= 6)
                            {
                                plasmaSpew = new Particle.Static(room.gameObjects.Count, spr_smallParticle, rand, ShootOrigin, new Vector2((rand.Next(3, 18) + Math.Abs(player.movement.X)) * player.direction, rand.Next(-8, 8) - player.movement.Y), new Vector2(0.93f, 0.93f), 1f, 0.15f, (int)(rand.NextDouble() * 30), Color.Red);
                            }
                            else
                            {
                                plasmaSpew = new Particle.Static(room.gameObjects.Count, spr_smallerParticle, rand, ShootOrigin, new Vector2((rand.Next(3, 25) + Math.Abs(player.movement.X)) * player.direction, rand.Next(-10, 10) - player.movement.Y), new Vector2(0.93f, 0.93f), 1f, 0.15f, (int)(rand.NextDouble() * 60), Color.Red);
                            }

                            room.particles.Add(plasmaSpew);
                        }

                    }
                    else if (AttackUpgradeList[1][1] == 1)
                    {
                        hasCharge = false;
                        if (player.state != Player.State.running && player.state != Player.State.back)
                        {
                            state = WeaponState.shoot1_1;
                        }
                        if (firstTime)
                        {
                            dmgZone = new DamageZone(new Rectangle((int)(player.position.X + player.width), (int)(player.position.Y - 50), 200, player.height + 100), Damage / 3, 10, 0, player.ID, true, true, attackable);
                            room.damageZones.Add(dmgZone);
                            firstTime = false;
                        }
                        if (player.direction == 1)
                        {
                            dmgZone.Position = new Vector2(player.position.X + (player.width), player.position.Y - 50);
                        }
                        if (player.direction == -1)
                        {
                            dmgZone.Position = new Vector2(player.position.X - dmgZone.rectangle.Width, player.position.Y - 50);
                        }
                        for (int i = 0; i < 20; i++)
                        {
                            int random = rand.Next(0, 10);
                            Particle.Static plasmaSpew;
                            if (random >= 6)
                            {
                                plasmaSpew = new Particle.Static(room.gameObjects.Count, spr_smallParticle, rand, ShootOrigin, new Vector2((rand.Next(3, 12) + Math.Abs(player.movement.X)) * player.direction, rand.Next(-8, 8) - player.movement.Y), new Vector2(0.93f, 0.93f), 1f, 0.15f, (int)(rand.NextDouble() * 30), Color.Red);
                            }
                            else
                            {
                                plasmaSpew = new Particle.Static(room.gameObjects.Count, spr_smallerParticle, rand, ShootOrigin, new Vector2((rand.Next(3, 20) + Math.Abs(player.movement.X)) * player.direction, rand.Next(-10, 10) - player.movement.Y), new Vector2(0.93f, 0.93f), 1f, 0.15f, (int)(rand.NextDouble() * 60), Color.Red);
                            }

                            room.particles.Add(plasmaSpew);
                        }

                    }
                    else if (AttackUpgradeList[1][0] == 1)
                    {
                        if (player.state != Player.State.running && player.state != Player.State.back)
                        {
                            state = WeaponState.shoot1_1;
                        }
                        if (attackTimer >= (float)(60 / (float)(AttackSpeed * 0.5f)))
                        {
                            attackTimer = 0;
                            for (int i = 0; i < 100; i++)
                            {
                                int random = rand.Next(0, 10);
                                Particle.Static plasmaSpew;
                                if (random >= 6)
                                {
                                    plasmaSpew = new Particle.Static(room.gameObjects.Count, spr_smallParticle, rand, ShootOrigin, new Vector2((rand.Next(3, 12) + Math.Abs(player.movement.X)) * player.direction, rand.Next(-8, 8) - player.movement.Y), new Vector2(0.93f, 0.93f), 1f, 0.15f, (int)(rand.NextDouble() * 30), Color.Red);
                                }
                                else
                                {
                                    plasmaSpew = new Particle.Static(room.gameObjects.Count, spr_smallerParticle, rand, ShootOrigin, new Vector2((rand.Next(3, 20) + Math.Abs(player.movement.X)) * player.direction, rand.Next(-10, 10) - player.movement.Y), new Vector2(0.93f, 0.93f), 1f, 0.15f, (int)(rand.NextDouble() * 60), Color.Red);
                                }

                                room.particles.Add(plasmaSpew);
                            }
                            if (player.direction == 1)
                            {
                                room.damageZones.Add(new DamageZone(new Rectangle((int)ShootOrigin.X, (int)ShootOrigin.Y - 50, 200, 100), Damage / 4, 10, 10, room.damageZones.Count, true, false, attackable));
                            }
                            else if (player.direction == -1)
                            {
                                room.damageZones.Add(new DamageZone(new Rectangle((int)ShootOrigin.X - 200, (int)ShootOrigin.Y - 50, 200, 100), Damage / 4, 10, 10, room.damageZones.Count, true, false, attackable));
                            }

                        }
                        if (SpriteHandler.sprites[spr_rShoot1_1].animationEnd) { SpriteHandler.sprites[spr_rShoot1_1].speed = 0f; }
                        if (SpriteHandler.sprites[spr_lShoot1_1].animationEnd) { SpriteHandler.sprites[spr_lShoot1_1].speed = 0f; }
                    }

                }
                else if (e_attacksPrev == e_Attacks.a2 && e_attacks != e_Attacks.a2 && (AttackUpgradeList[1][1] == 1 || AttackUpgradeList[1][2] == 1))
                {
                    firstTime = true;
                    dmgZone.hp = -1;
                }
                if (e_attacks == e_Attacks.a3)
                {
                    if (AttackUpgradeList[2][1] == 1)
                    {
                        if (player.state != Player.State.running && player.state != Player.State.back)
                        {
                            state = WeaponState.shoot1_1;
                        }
                        if (attackTimer >= (float)(60 / (float)(AttackSpeed * 0.1f)))
                        {
                            room.gameObjects.Add(new ExplodingBall(room.gameObjects.Count, new Animation(spr_plasmaUlt), ShootOrigin, new Vector2(rand.Next(45, 50) * player.direction, rand.Next(-9, -8)), Damage, 2, 60, room, true, false, true, true, 0f, true, spriteEffect, attackable, true));
                            room.gameObjects.Add(new ExplodingBall(room.gameObjects.Count, new Animation(spr_plasmaUlt), ShootOrigin, new Vector2(rand.Next(45, 50) * player.direction, rand.Next(-9, -8)), Damage, 2, 60, room, true, false, true, true, 0f, true, spriteEffect, attackable, true));
                            attackTimer = 0;
                        }
                    }
                    else if (AttackUpgradeList[2][0] == 1)
                    {
                        if (player.state != Player.State.running && player.state != Player.State.back)
                        {
                            state = WeaponState.shoot1_1;
                        }
                        if (attackTimer >= (float)(60 / (float)(AttackSpeed * 0.1f)))
                        {
                            room.gameObjects.Add(new ExplodingBall(room.gameObjects.Count, new Animation(spr_plasmaUlt), ShootOrigin, new Vector2(rand.Next(45, 50) * player.direction, rand.Next(-9, -8)), Damage, 2, 60, room, true, false, true, true, 0f, true, spriteEffect, attackable, true));
                            attackTimer = 0;
                        }
                    }
                }

                if (hasCharge && releaseCharge)
                {
                    ChargeRelease(chargedProj, ShootOrigin, room);
                }
                keyboardPrev = Keyboard.GetState();
                e_attacksPrev = e_attacks;
            }


            public override void Draw(Player player, Random rand, SpriteBatch spriteBatch, GraphicsDevice graphics, Camera camera, bool leftHand)
            {
                base.Draw(player, rand, spriteBatch, graphics, camera, leftHand);
                if (player.direction < 0)
                    spriteEffect = SpriteEffects.FlipHorizontally;
                else
                    spriteEffect = SpriteEffects.None;

                Vector2 drawPos = new Vector2(player.Origin.X - SpriteHandler.sprites[spr_lIdle].width / 2, player.Rectangle.Bottom - player.height) + player.textureOffset;
                if (leftHand)
                {
                    switch (state)
                    {
                        case WeaponState.idle:
                            currentSprite = spr_lIdle;
                            SpriteHandler.Draw(spr_lIdle, rand, spriteBatch, camera, drawPos - new Vector2(1 * player.direction, 0), spriteEffect, leftDepth);
                            break;
                        case WeaponState.jump:
                            currentSprite = spr_lJump;
                            SpriteHandler.Draw(spr_lJump, rand, spriteBatch, camera, drawPos, spriteEffect, leftDepth);
                            break;
                        case WeaponState.run:
                            currentSprite = spr_lRunning;
                            SpriteHandler.Draw(spr_lRunning, rand, spriteBatch, camera, drawPos, spriteEffect, leftDepth);
                            break;
                        case WeaponState.fall:
                            currentSprite = spr_lFall;
                            SpriteHandler.Draw(spr_lFall, rand, spriteBatch, camera, drawPos, spriteEffect, leftDepth);
                            break;
                        case WeaponState.shoot1_1:
                            currentSprite = spr_lShoot1_1;
                            SpriteHandler.Draw(spr_lShoot1_1, rand, spriteBatch, camera, drawPos, spriteEffect, leftDepth);
                            break;
                        case WeaponState.shoot1_2:
                            currentSprite = spr_lShoot1_1;
                            SpriteHandler.Draw(spr_lShoot1_1, rand, spriteBatch, camera, drawPos, spriteEffect, leftDepth);
                            break;
                        case WeaponState.shoot1_3:
                            currentSprite = spr_lShoot1_1;
                            SpriteHandler.Draw(spr_lShoot1_1, rand, spriteBatch, camera, drawPos, spriteEffect, leftDepth);
                            break;
                        case WeaponState.shoot2_1:
                            currentSprite = spr_lShoot2_1;
                            SpriteHandler.Draw(spr_lShoot2_1, rand, spriteBatch, camera, drawPos, spriteEffect, leftDepth);
                            break;
                        case WeaponState.shoot2_2:
                            currentSprite = spr_lShoot2_1;
                            SpriteHandler.Draw(spr_lShoot2_1, rand, spriteBatch, camera, drawPos, spriteEffect, leftDepth);
                            break;
                        case WeaponState.shoot2_3:
                            currentSprite = spr_lShoot2_1;
                            SpriteHandler.Draw(spr_lShoot2_1, rand, spriteBatch, camera, drawPos, spriteEffect, leftDepth);
                            break;
                        case WeaponState.shoot3_1:
                            currentSprite = spr_lShoot2_1;
                            SpriteHandler.Draw(spr_lShoot2_1, rand, spriteBatch, camera, drawPos, spriteEffect, leftDepth);
                            break;
                        case WeaponState.shoot3_2:
                            currentSprite = spr_lShoot2_1;
                            SpriteHandler.Draw(spr_lShoot2_1, rand, spriteBatch, camera, drawPos, spriteEffect, leftDepth);
                            break;
                        default:
                            break;
                    }

                }
                else
                {
                    switch (state)
                    {
                        case WeaponState.idle:
                            currentSprite = spr_rIdle;
                            SpriteHandler.Draw(spr_rIdle, rand, spriteBatch, camera, drawPos, spriteEffect, rightDepth);
                            break;
                        case WeaponState.jump:
                            currentSprite = spr_rJump;
                            SpriteHandler.Draw(spr_rJump, rand, spriteBatch, camera, drawPos, spriteEffect, rightDepth);
                            break;
                        case WeaponState.run:
                            currentSprite = spr_rRunning;
                            SpriteHandler.Draw(spr_rRunning, rand, spriteBatch, camera, drawPos, spriteEffect, rightDepth);
                            break;
                        case WeaponState.fall:
                            currentSprite = spr_rFall;
                            SpriteHandler.Draw(spr_rFall, rand, spriteBatch, camera, drawPos, spriteEffect, rightDepth);
                            break;
                        case WeaponState.shoot1_1:
                            currentSprite = spr_rShoot1_1;
                            SpriteHandler.Draw(spr_rShoot1_1, rand, spriteBatch, camera, drawPos, spriteEffect, rightDepth);
                            break;
                        case WeaponState.shoot1_2:
                            currentSprite = spr_rShoot1_1;
                            SpriteHandler.Draw(spr_rShoot1_1, rand, spriteBatch, camera, drawPos, spriteEffect, rightDepth);
                            break;
                        case WeaponState.shoot1_3:
                            currentSprite = spr_rShoot1_1;
                            SpriteHandler.Draw(spr_rShoot1_1, rand, spriteBatch, camera, drawPos, spriteEffect, rightDepth);
                            break;
                        case WeaponState.shoot2_1:
                            currentSprite = spr_rShoot2_1;
                            SpriteHandler.Draw(spr_rShoot2_1, rand, spriteBatch, camera, drawPos, spriteEffect, rightDepth);
                            break;
                        case WeaponState.shoot2_2:
                            currentSprite = spr_rShoot2_1;
                            SpriteHandler.Draw(spr_rShoot2_1, rand, spriteBatch, camera, drawPos, spriteEffect, rightDepth);
                            break;
                        case WeaponState.shoot2_3:
                            currentSprite = spr_rShoot2_1;
                            SpriteHandler.Draw(spr_rShoot2_1, rand, spriteBatch, camera, drawPos, spriteEffect, rightDepth);
                            break;
                        case WeaponState.shoot3_1:
                            currentSprite = spr_rShoot2_1;
                            SpriteHandler.Draw(spr_rShoot2_1, rand, spriteBatch, camera, drawPos, spriteEffect, rightDepth);
                            break;
                        case WeaponState.shoot3_2:
                            currentSprite = spr_rShoot2_1;
                            SpriteHandler.Draw(spr_rShoot2_1, rand, spriteBatch, camera, drawPos, spriteEffect, rightDepth);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public class BAMF : Weapon
        {

            Random rand;
            bool firstTimeAttack1 = true;
            int attack1Counter = 0;
            string spr_Attack1_1, spr_Attack2_1, spr_Attack3_1, spr_Particle;

            public BAMF(GraphicsDevice graphicsDevice) : base()
            {

                rand = new Random();
                spr_lFall = "BAMF_lFall";
                spr_lIdle = "BAMF_lIdle";
                spr_lJump = "BAMF_lJump";
                spr_lRunning = "BAMF_lRunning";
                spr_lShoot1_1 = "BAMF_lShoot1_1";
                spr_lShoot2_1 = "BAMF_lShoot2_1";
                spr_lShoot3_1 = "BAMF_lShoot1_1";
                spr_rShoot1_1 = "BAMF_rShoot1_1";
                spr_rShoot2_1 = "BAMF_rShoot2_1";
                spr_rShoot3_1 = "BAMF_rShoot1_1";
                spr_rFall = "BAMF_rFall";
                spr_rIdle = "BAMF_rIdle";
                spr_rJump = "BAMF_rJump";
                spr_rRunning = "BAMF_rRunning";
                spr_Attack1_1 = "BAMF_Attack1_1";
                spr_Attack2_1 = "BAMF_Attack2_1";
                spr_Attack3_1 = "BAMF_Attack3_1";
                spr_Particle = "BAMF_Particle";
                attackable.Add(GameObject.Types.Enemy);
                attackable.Add(GameObject.Types.Prop);
                SpriteHandler.sprites[spr_lIdle].speed = SpriteHandler.sprites["player_idle"].speed;
                SpriteHandler.sprites[spr_rIdle].speed = SpriteHandler.sprites["player_idle"].speed;
                SpriteHandler.sprites[spr_lRunning].speed = SpriteHandler.sprites["player_running"].speed;
                SpriteHandler.sprites[spr_rRunning].speed = SpriteHandler.sprites["player_running"].speed;

                this.Name = "BAMF";
                this.Level = 1;
                this.Experience = 0;
                this.ExperienceCap = 1000;
                this.Damage = 1;
                this.AttackSpeed = 5;
                AttackUpgradeList[0][0] = 1;
                AttackUpgradeList[1][0] = 1;
                AttackUpgradeList[2][1] = 1;
                hp = 25;


            }
            public class Shuriken : Projectile
            {
                public class Stun : DamageZone
                {
                    public Stun(Rectangle rectangle, float damage, int coolDown, int duration, int ID, bool friendly, bool linked, List<GameObject.Types> attackableTypes) : base(rectangle, damage, coolDown, duration, ID, friendly, linked, attackableTypes)
                    {

                    }
                    public override void Update(Room room)
                    {
                        if (hp > 0)
                        {

                            for (int i = 0; i < room.gameObjects.Count; i++)
                            {
                                if (attackable.Contains(room.gameObjects[i].type))
                                {
                                    if (rectangle.Intersects(room.gameObjects[i].GetHitBox()) && !IDs.Contains(i))
                                    {
                                        room.gameObjects[i].stunned = true;
                                        room.gameObjects[i].stunTime = 60;
                                        hp = -1;
                                    }
                                }
                            }

                            if (attackable.Contains(GameObject.Types.Player))
                            {
                                if (rectangle.Intersects(room.player.GetHitBox()) && !IDs.Contains(room.player.ID))
                                {
                                    room.player.TakeDamage(room, sourceID, damage);
                                    IDs.Add(room.player.ID);
                                    coolDowns.Add(coolDown);
                                }
                            }

                            for (int i = 0; i < IDs.Count; i++)
                            {
                                if (coolDowns[i] <= 0)
                                {
                                    coolDowns.RemoveAt(i);
                                    IDs.RemoveAt(i);
                                }
                                if (coolDowns.Count > 0) { coolDowns[i]--; }
                            }
                        }
                        if (die)
                        { hp--; }
                    }

                }
                public Stun stunZone;
                public bool stun;
                public List<GameObject.Types> attackable = new List<GameObject.Types>();
                public Shuriken(int ID, Animation ani, Vector2 pos, Vector2 vel, float damage, float knockback, float lifespan, Room room, bool friendly, bool useFriction, bool useGravity, bool useResistance, float bounceFactor, bool dieOnCollision, bool stun, SpriteEffects spriteEffect, List<GameObject.Types> attackable, bool lookAtMovement) : base(ID, ani, pos, vel, damage, knockback, lifespan, room, friendly, useFriction, useGravity, useResistance, 0f, true, spriteEffect, attackable, lookAtMovement)
                {
                    this.stun = stun;
                    if (stun)
                        this.stunZone = new Stun(damageZone.rectangle, 0, damageZone.coolDown, damageZone.hp, room.damageZones.Count, true, true, attackable);
                    this.attackable = attackable;
                }

                public override void Update(Room room, Random rand)
                {
                    if (stun)
                        stunZone.Position = damageZone.Position;
                    if (stun)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            room.particles.Add(new Particle.Static(room.particles.Count, "BAMF_Particle", rand, position, new Vector2(movement.X - (movement.X / Math.Abs(movement.X) * rand.Next(Math.Abs(Game1.FloorAdv(movement.X * 0.9f)), Math.Abs(Game1.FloorAdv(movement.X * 1.2f)))), movement.X - (movement.X / Math.Abs(movement.X) * rand.Next(Math.Abs(Game1.FloorAdv(movement.X * 0.9f)), Math.Abs(Game1.FloorAdv(movement.X * 1.2f))))), new Vector2(1f, 1f), new Vector2(rand.Next(1, 3), 1), 0.3f, rand.Next(2, 10), Color.LightGreen));
                            room.particles[room.particles.Count - 1].opacityLight = 0.4f;
                        }
                    }
                    angle += 5;
                    base.Update(room, rand);
                }

            }

            public class Rocket : Projectile
            {
                public List<GameObject.Types> attackable = new List<GameObject.Types>();
                public bool unstable;
                public int rocketNum;
                public int reverse = -1;
                public int twirlTimer = 0;
                /// <summary>
                /// 
                /// </summary>
                /// <param name="ID"></param>
                /// <param name="ani"></param>
                /// <param name="pos"></param>
                /// <param name="vel"></param>
                /// <param name="damage"></param>
                /// <param name="knockback"></param>
                /// <param name="lifespan"></param>
                /// <param name="room"></param>
                /// <param name="friendly"></param>
                /// <param name="useFriction"></param>
                /// <param name="useGravity"></param>
                /// <param name="useResistance"></param>
                /// <param name="bounceFactor"></param>
                /// <param name="dieOnCollision"></param>
                /// <param name="unstable"></param>
                /// <param name="rocketNum">The number of the rocket, either 1 or 2</param>
                /// <param name="spriteEffect"></param>
                /// <param name="attackable"></param>
                public Rocket(int ID, Animation ani, Vector2 pos, Vector2 vel, float damage, float knockback, float lifespan, Room room, bool friendly, bool useFriction, bool useGravity, bool useResistance, float bounceFactor, bool dieOnCollision, bool unstable, int rocketNum, SpriteEffects spriteEffect, List<GameObject.Types> attackable, bool lookAtMovement) : base(ID, ani, pos, vel, damage, knockback, lifespan, room, friendly, useFriction, useGravity, useResistance, 0f, true, spriteEffect, attackable, lookAtMovement)
                {
                    this.attackable = attackable;
                    this.unstable = unstable;
                    this.rocketNum = rocketNum;
                    if (rocketNum == 1)
                    {
                        movement.Y = 5f;
                    }
                    else if (rocketNum == 2)
                    {
                        movement.Y = -5f;
                    }
                }

                public override void Update(Room room, Random rand)
                {
                    base.Update(room, rand);

                    for (int i = 0; i < 6; i++)
                    {
                        room.particles.Add(new Particle.Static(room.particles.Count, "BAMF_Particle", rand, position, new Vector2(movement.X - (movement.X / Math.Abs(movement.X) * rand.Next(Math.Abs(Game1.FloorAdv(movement.X * 0.9f)), Math.Abs(Game1.FloorAdv(movement.X * 1.2f)))), movement.Y - (movement.Y / Math.Abs(movement.Y) * rand.Next(Math.Abs(Game1.FloorAdv(movement.Y * 0.9f)), Math.Abs(Game1.FloorAdv(movement.Y * 1.2f))))), new Vector2(1f, 1f), new Vector2(rand.Next(1, 3), 1), 0.3f, rand.Next(1, 2) * 10, new Color(rand.Next(200, 255), rand.Next(115, 150), 55)));
                        room.particles[room.particles.Count - 1].opacityLight = 0.4f;
                    }
                    if (live)
                    {
                        if (unstable)
                        {
                            movement.Y += ((float)rand.NextDouble() * rand.Next(-3, 3));
                            if (movement.Y < 1) { movement.Y += 0.4f; }
                            else if (movement.Y > 1) { movement.Y -= 0.4f; }
                        }
                        else
                        {
                            //1 = the upper one
                            twirlTimer++;
                            if (rocketNum == 1)
                            {
                                movement.Y += (-0.5f * direction);
                            }
                            if (rocketNum == 2)
                            {
                                movement.Y += (0.5f * direction);
                            }
                            if (twirlTimer % 20 == 0)
                            {
                                direction *= -1;
                            }
                        }
                    }
                    else if (!live)
                    {
                        //PARTICLE EFFECTS
                        for (int i = 0; i < 50; i++)
                        {
                            room.particles.Add(new Particle.Static(room.particles.Count, "BAMF_Particle", rand, position, new Vector2(rand.Next(-25, 25), rand.Next(-25, 25)), new Vector2(0.93f, 0.93f), new Vector2(rand.Next(1, 5), 1), 0.3f, rand.Next(1, 10), new Color(rand.Next(200, 255), rand.Next(115, 150), 55)));
                        }
                        Game1.camera.AddShake(new Vector2(3, 4), 2);
                        room.damageZones.Add(new DamageZone(new Rectangle((int)position.X - 50, (int)position.Y - 50, 200, 200), Damage, 120, 30, ID, true, false, attackable));
                    }
                }
            }
            public override void Update(Player player, Room room, bool leftHand)
            {
                base.Update(player, room, leftHand);
                keyboard = Keyboard.GetState();
                SpriteHandler.sprites[spr_lIdle].currentFrame = SpriteHandler.sprites["player_idle"].currentFrame;
                SpriteHandler.sprites[spr_rIdle].currentFrame = SpriteHandler.sprites["player_idle"].currentFrame;
                SpriteHandler.sprites[spr_lRunning].currentFrame = SpriteHandler.sprites["player_running"].currentFrame;
                SpriteHandler.sprites[spr_rRunning].currentFrame = SpriteHandler.sprites["player_running"].currentFrame;
                if (spriteEffect != SpriteEffects.FlipHorizontally)
                {

                    if (leftHand)
                    {
                        if (player.state == Player.State.running || player.state == Player.State.back)
                        {
                            ShootOrigin = HandOrigin(player.position, 50 + 16, 50) + player.textureOffset;
                        }
                        else
                        {
                            ShootOrigin = HandOrigin(player.position, 50 + 16, 30) + player.textureOffset;
                        }
                    }
                    else
                    {
                        if (player.state == Player.State.running || player.state == Player.State.back)
                        {
                            ShootOrigin = HandOrigin(player.position, 50 - 6, 53) + player.textureOffset;
                        }
                        else
                        {
                            ShootOrigin = HandOrigin(player.position, 50 - 5, 33) + player.textureOffset;
                        }
                    }
                }
                if (spriteEffect == SpriteEffects.FlipHorizontally)
                {
                    if (leftHand)
                    {
                        if (player.state == Player.State.running || player.state == Player.State.back)
                        {
                            ShootOrigin = HandOrigin(player.position, 50 + 16, 53) + player.textureOffset;
                        }
                        else
                        {
                            ShootOrigin = HandOrigin(player.position, 50 - 75, 33) + player.textureOffset;
                        }
                    }
                    else
                    {
                        if (player.state == Player.State.running || player.state == Player.State.back)
                        {
                            ShootOrigin = HandOrigin(player.position, 10, 53) + player.textureOffset;
                        }
                        else
                        {
                            ShootOrigin = HandOrigin(player.position, 5, 33) + player.textureOffset;
                        }
                    }
                }

                Check_Attack1(leftHand);
                Check_Attack2(leftHand);
                Check_Attack3(leftHand);

                if (e_attacks == e_Attacks.a1)
                {
                    if (state != WeaponState.run)
                    {
                        state = WeaponState.shoot1_1;
                    }
                    if (AttackUpgradeList[0][2] == 1)
                    {
                        int attackspdDeviation = 2;
                        if (attackTimer >= (float)(60 / (float)(AttackSpeed * attackspdDeviation)))
                        {
                            ShootBullet(room, ShootOrigin, new Vector2(player.direction, 0), Damage, 1000, room.platforms, room.gameObjects, Color.Yellow, player);
                            attackTimer = 0;
                        }
                    }
                    else if (AttackUpgradeList[0][1] == 1)
                    {
                        float attackspdDeviation = 0.7f;
                        if (attackTimer >= (float)(60 / (float)(AttackSpeed * attackspdDeviation)))
                        {
                            if (firstTimeAttack1)
                            {
                                firstTimeAttack1 = false;
                                attack1Counter = 0;
                            }
                            else
                            {
                                attack1Counter++;
                                if (attack1Counter % 7 == 0)
                                {
                                    ShootBullet(room, ShootOrigin, new Vector2(player.direction, 0), Damage, 1000, room.platforms, room.gameObjects, Color.Yellow, player);
                                }
                                if (attack1Counter == 21)
                                {
                                    firstTimeAttack1 = true;
                                    attackTimer = 0;
                                }
                            }
                        }
                    }
                    else if (AttackUpgradeList[0][0] == 1)
                    {
                        int attackspdDeviation = 1;
                        if (attackTimer >= (float)(60 / (float)(AttackSpeed * attackspdDeviation)))
                        {
                            attackTimer = 0;
                            ShootBullet(room, ShootOrigin, new Vector2(player.direction, 0), Damage, 1000, room.platforms, room.gameObjects, Color.Yellow, player);
                        }
                    }
                }
                if (e_attacks == e_Attacks.a2)
                {

                    if (AttackUpgradeList[1][2] == 1)
                    {

                    }
                    else if (AttackUpgradeList[1][1] == 1)
                    {
                        if (state != WeaponState.run)
                        {
                            state = WeaponState.shoot2_1;
                        }
                        float attackspdDeviation = 0.5f;
                        if (attackTimer >= (float)(60 / (float)(AttackSpeed * attackspdDeviation)))
                        {
                            SpriteHandler.sprites[spr_lShoot2_1].currentFrame = 0;
                            SpriteHandler.sprites[spr_rShoot2_1].currentFrame = 0;
                            attackTimer = 0;
                            room.gameObjects.Add(new Shuriken(room.gameObjects.Count, new Animation("BAMF_Attack2_1"), ShootOrigin, new Vector2(13 * player.direction, 0), Damage, 0, 60 * 2, room, true, false, false, false, 1f, true, true, spriteEffect, attackable, false));
                        }
                    }
                    else if (AttackUpgradeList[1][0] == 1)
                    {
                        if (state != WeaponState.run)
                        {
                            state = WeaponState.shoot2_1;
                        }
                        float attackspdDeviation = 0.5f;
                        if (attackTimer >= (float)(60 / (float)(AttackSpeed * attackspdDeviation)))
                        {
                            SpriteHandler.sprites[spr_lShoot2_1].currentFrame = 0;
                            SpriteHandler.sprites[spr_rShoot2_1].currentFrame = 0;
                            attackTimer = 0;
                            room.gameObjects.Add(new Shuriken(room.gameObjects.Count, new Animation("BAMF_Attack2_1"), ShootOrigin, new Vector2(13 * player.direction, 0), Damage, 0, 60 * 2, room, true, false, false, false, 1f, true, false, spriteEffect, attackable, false));
                        }
                    }
                }
                if (e_attacks == e_Attacks.a3)
                {

                    if (AttackUpgradeList[2][1] == 1)
                    {
                        if (state != WeaponState.run)
                        {
                            state = WeaponState.shoot3_1;
                        }
                        float attackspdDeviation = 0.1f;
                        if (attackTimer >= (float)(60 / (float)(AttackSpeed * attackspdDeviation)))
                        {
                            attackTimer = 0;
                            room.gameObjects.Add(new Rocket(room.gameObjects.Count, new Animation("BAMF_Attack3_1"), ShootOrigin + new Vector2(10 * player.direction, 0), new Vector2(10 * player.direction, 0), Damage * 2, 3, 60 * 3, room, true, false, false, false, 1f, true, false, 1, spriteEffect, attackable, true));
                            room.gameObjects.Add(new Rocket(room.gameObjects.Count, new Animation("BAMF_Attack3_1"), ShootOrigin, new Vector2(10 * player.direction, 0), Damage * 2, 3, 60 * 3, room, true, false, false, false, 1f, true, false, 2, spriteEffect, attackable, true));
                        }
                    }
                    else if (AttackUpgradeList[2][0] == 1)
                    {
                        if (state != WeaponState.run)
                        {
                            state = WeaponState.shoot3_1;
                        }
                        float attackspdDeviation = 0.1f;
                        if (attackTimer >= (float)(60 / (float)(AttackSpeed * attackspdDeviation)))
                        {
                            attackTimer = 0;
                            room.gameObjects.Add(new Rocket(room.gameObjects.Count, new Animation("BAMF_Attack3_1"), ShootOrigin, new Vector2(10 * player.direction, 0), Damage * 2, 3, 60 * 3, room, true, false, false, false, 1f, true, true, 1, spriteEffect, attackable, true));
                        }
                    }
                }
                keyboardPrev = Keyboard.GetState();
            }
            public override void Draw(Player player, Random rand, SpriteBatch spriteBatch, GraphicsDevice graphics, Camera camera, bool leftHand)
            {
                base.Draw(player, rand, spriteBatch, graphics, camera, leftHand);
                if (player.direction < 0)
                    spriteEffect = SpriteEffects.FlipHorizontally;
                else
                    spriteEffect = SpriteEffects.None;

                Vector2 drawPos = new Vector2(player.Origin.X - SpriteHandler.sprites[spr_lIdle].width / 2, player.Rectangle.Bottom - player.height) + player.textureOffset;
                if (leftHand)
                {
                    switch (state)
                    {
                        case WeaponState.idle:
                            currentSprite = spr_lIdle;
                            SpriteHandler.Draw(spr_lIdle, rand, spriteBatch, camera, drawPos - new Vector2(1 * player.direction, 0), spriteEffect, leftDepth);
                            break;
                        case WeaponState.jump:
                            currentSprite = spr_lJump;
                            SpriteHandler.Draw(spr_lJump, rand, spriteBatch, camera, drawPos, spriteEffect, leftDepth);
                            break;
                        case WeaponState.run:
                            currentSprite = spr_lRunning;
                            SpriteHandler.Draw(spr_lRunning, rand, spriteBatch, camera, drawPos, spriteEffect, leftDepth);
                            break;
                        case WeaponState.fall:
                            currentSprite = spr_lFall;
                            SpriteHandler.Draw(spr_lFall, rand, spriteBatch, camera, drawPos, spriteEffect, leftDepth);
                            break;
                        case WeaponState.shoot1_1:
                            currentSprite = spr_lShoot1_1;
                            SpriteHandler.Draw(spr_lShoot1_1, rand, spriteBatch, camera, drawPos, spriteEffect, leftDepth);
                            break;
                        case WeaponState.shoot1_2:
                            currentSprite = spr_lShoot1_1;
                            SpriteHandler.Draw(spr_lShoot1_1, rand, spriteBatch, camera, drawPos, spriteEffect, leftDepth);
                            break;
                        case WeaponState.shoot1_3:
                            currentSprite = spr_lShoot1_1;
                            SpriteHandler.Draw(spr_lShoot1_1, rand, spriteBatch, camera, drawPos, spriteEffect, leftDepth);
                            break;
                        case WeaponState.shoot2_1:
                            currentSprite = spr_lShoot2_1;
                            SpriteHandler.Draw(spr_lShoot2_1, rand, spriteBatch, camera, drawPos, spriteEffect, leftDepth);
                            break;
                        case WeaponState.shoot2_2:
                            currentSprite = spr_lShoot2_1;
                            SpriteHandler.Draw(spr_lShoot2_1, rand, spriteBatch, camera, drawPos, spriteEffect, leftDepth);
                            break;
                        case WeaponState.shoot2_3:
                            currentSprite = spr_lShoot2_1;
                            SpriteHandler.Draw(spr_lShoot2_1, rand, spriteBatch, camera, drawPos, spriteEffect, leftDepth);
                            break;
                        case WeaponState.shoot3_1:
                            currentSprite = spr_lShoot3_1;
                            SpriteHandler.Draw(spr_lShoot3_1, rand, spriteBatch, camera, drawPos, spriteEffect, leftDepth);
                            break;
                        case WeaponState.shoot3_2:
                            currentSprite = spr_lShoot3_1;
                            SpriteHandler.Draw(spr_lShoot3_1, rand, spriteBatch, camera, drawPos, spriteEffect, leftDepth);
                            break;
                        default:
                            break;
                    }

                }
                else
                {
                    switch (state)
                    {
                        case WeaponState.idle:
                            currentSprite = spr_rIdle;
                            SpriteHandler.Draw(spr_rIdle, rand, spriteBatch, camera, drawPos, spriteEffect, rightDepth);
                            break;
                        case WeaponState.jump:
                            currentSprite = spr_rJump;
                            SpriteHandler.Draw(spr_rJump, rand, spriteBatch, camera, drawPos, spriteEffect, rightDepth);
                            break;
                        case WeaponState.run:
                            currentSprite = spr_rRunning;
                            SpriteHandler.Draw(spr_rRunning, rand, spriteBatch, camera, drawPos, spriteEffect, rightDepth);
                            break;
                        case WeaponState.fall:
                            currentSprite = spr_rFall;
                            SpriteHandler.Draw(spr_rFall, rand, spriteBatch, camera, drawPos, spriteEffect, rightDepth);
                            break;
                        case WeaponState.shoot1_1:
                            currentSprite = spr_rShoot1_1;
                            SpriteHandler.Draw(spr_rShoot1_1, rand, spriteBatch, camera, drawPos, spriteEffect, rightDepth);
                            break;
                        case WeaponState.shoot1_2:
                            currentSprite = spr_rShoot1_1;
                            SpriteHandler.Draw(spr_rShoot1_1, rand, spriteBatch, camera, drawPos, spriteEffect, rightDepth);
                            break;
                        case WeaponState.shoot1_3:
                            currentSprite = spr_rShoot1_1;
                            SpriteHandler.Draw(spr_rShoot1_1, rand, spriteBatch, camera, drawPos, spriteEffect, rightDepth);
                            break;
                        case WeaponState.shoot2_1:
                            currentSprite = spr_rShoot2_1;
                            SpriteHandler.Draw(spr_rShoot2_1, rand, spriteBatch, camera, drawPos, spriteEffect, rightDepth);
                            break;
                        case WeaponState.shoot2_2:
                            currentSprite = spr_rShoot2_1;
                            SpriteHandler.Draw(spr_rShoot2_1, rand, spriteBatch, camera, drawPos, spriteEffect, rightDepth);
                            break;
                        case WeaponState.shoot2_3:
                            currentSprite = spr_rShoot2_1;
                            SpriteHandler.Draw(spr_rShoot2_1, rand, spriteBatch, camera, drawPos, spriteEffect, rightDepth);
                            break;
                        case WeaponState.shoot3_1:
                            currentSprite = spr_rShoot3_1;
                            SpriteHandler.Draw(spr_rShoot3_1, rand, spriteBatch, camera, drawPos, spriteEffect, rightDepth);
                            break;
                        case WeaponState.shoot3_2:
                            currentSprite = spr_rShoot2_1;
                            SpriteHandler.Draw(spr_rShoot2_1, rand, spriteBatch, camera, drawPos, spriteEffect, rightDepth);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public class FlameWeapon : Weapon
        {
            Random rand;
            bool firstTimeAttack1 = true;
            int attack1Counter = 0;
            bool firstTime = true;
            public DamageZone dmgZone;
            public FlameWeapon(GraphicsDevice graphicsDevice) : base()
            {

                rand = new Random();
                spr_lFall = "Red_lFall";
                spr_lIdle = "Red_lIdle";
                spr_lJump = "Red_lJump";
                spr_lRunning = "Red_lRunning";
                spr_lShoot1_1 = "Red_lShoot1_1";
                spr_lShoot2_1 = "Red_lShoot2_1";
                spr_rShoot1_1 = "Red_rShoot1_1";
                spr_rShoot2_1 = "Red_rShoot2_1";
                spr_rFall = "Red_rFall";
                spr_rIdle = "Red_rIdle";
                spr_rJump = "Red_rJump";
                spr_rRunning = "Red_rRunning";
                attackable.Add(GameObject.Types.Enemy);
                attackable.Add(GameObject.Types.Prop);




                this.Name = "Flame";
                this.Level = 1;
                this.Experience = 0;
                this.ExperienceCap = 1000;
                this.Damage = 1;
                this.AttackSpeed = 5;
                AttackUpgradeList[0][0] = 1;
                AttackUpgradeList[1][0] = 1;
                AttackUpgradeList[2][0] = 1;
                hp = 25;
            }


            public override void Update(Player player, Room room, bool leftHand)
            {
                base.Update(player, room, leftHand);

                if (spriteEffect != SpriteEffects.FlipHorizontally)
                {

                    if (leftHand)
                    {
                        if (player.state == Player.State.running || player.state == Player.State.back)
                        {
                            ShootOrigin = HandOrigin(player.position, 50 + 16, 50) + player.textureOffset;
                        }
                        else
                        {
                            ShootOrigin = HandOrigin(player.position, 50 + 16, 30) + player.textureOffset;
                        }
                    }
                    else
                    {
                        if (player.state == Player.State.running || player.state == Player.State.back)
                        {
                            ShootOrigin = HandOrigin(player.position, 50 - 6, 53) + player.textureOffset;
                        }
                        else
                        {
                            ShootOrigin = HandOrigin(player.position, 50 - 5, 33) + player.textureOffset;
                        }
                    }
                }
                if (spriteEffect == SpriteEffects.FlipHorizontally)
                {
                    if (leftHand)
                    {
                        if (player.state == Player.State.running || player.state == Player.State.back)
                        {
                            ShootOrigin = HandOrigin(player.position, 50 + 16, 53) + player.textureOffset;
                        }
                        else
                        {
                            ShootOrigin = HandOrigin(player.position, 50 - 75, 33) + player.textureOffset;
                        }
                    }
                    else
                    {
                        if (player.state == Player.State.running || player.state == Player.State.back)
                        {
                            ShootOrigin = HandOrigin(player.position, 10, 53) + player.textureOffset;
                        }
                        else
                        {
                            ShootOrigin = HandOrigin(player.position, 5, 33) + player.textureOffset;
                        }
                    }
                }

                Check_Attack1(leftHand);
                Check_Attack2(leftHand);
                Check_Attack3(leftHand);

                if (e_attacks == e_Attacks.a1)
                {
                    if (AttackUpgradeList[0][2] == 1)
                    {

                    }
                    else if (AttackUpgradeList[0][1] == 1)
                    {

                    }
                    else if (AttackUpgradeList[0][0] == 1)
                    {

                    }
                }
                if (e_attacks == e_Attacks.a2)
                {
                    if (AttackUpgradeList[1][2] == 1)
                    {

                    }
                    else if (AttackUpgradeList[1][1] == 1)
                    {
                        if (firstTime)
                        {
                            dmgZone = new DamageZone(new Rectangle((int)(player.position.X + player.width), (int)(player.position.Y), 300, player.height), Damage, 10, 0, player.ID, true, true, attackable);
                            room.damageZones.Add(dmgZone);
                            firstTime = false;
                        }
                        if (player.direction == 1)
                        {
                            dmgZone.Position = new Vector2(player.position.X + (player.width), player.position.Y);
                        }
                        if (player.direction == -1)
                        {
                            dmgZone.Position = new Vector2(player.position.X - dmgZone.rectangle.Width, player.position.Y);
                        }
                        hasCharge = false;
                        if (player.state != Player.State.running && player.state != Player.State.back)
                        {
                            state = WeaponState.shoot1_1;
                        }
                    }
                    else if (AttackUpgradeList[1][0] == 1)
                    {

                    }
                }
                else if (e_attacksPrev == e_Attacks.a2 && e_attacks != e_Attacks.a2)
                {
                    firstTime = true;
                    room.damageZones[room.damageZones.IndexOf(dmgZone)].hp = -1;
                }
                if (e_attacks == e_Attacks.a3)
                {
                    if (AttackUpgradeList[2][1] == 1)
                    {

                    }
                    else if (AttackUpgradeList[2][0] == 1)
                    {

                    }
                }
            }
        }
    }
}








/*
public class TutorialWeapon : Weapon
{
    private Random rand;
    private string spr_bullet, spr_grenade, spr_rocket;

    public class Grenade : Projectile
    {
        public Grenade(int ID, string sprite, Vector2 pos, Vector2 vel, float damage, float knockback, float lifespan, Room room, bool friendly, bool useFriction, bool useGravity, bool useResistance, float bounceFactor, bool dieOnCollision, SpriteEffects spriteEffect, float friction, float resistance,float scale) : base(sprite, pos, vel, damage, knockback, lifespan, room, friendly, useFriction, useGravity, useResistance, bounceFactor, dieOnCollision, spriteEffect, friction, resistance, scale)
        {

        }

        public override void Update(Room room, Random rand)
        {
            base.Update(room, rand);
            if(!live)
            {

            }
        }
    }
    public class Rocket : Projectile
    {
        public Rocket(int ID, string sprite, Vector2 pos, Vector2 vel, float damage, float knockback, float lifespan, Room room, bool friendly, bool useFriction, bool useGravity, bool useResistance, float bounceFactor, bool dieOnCollision, SpriteEffects spriteEffect, float friction, float resistance, float scale) : base(sprite, pos, vel, damage, knockback, lifespan, room, friendly, useFriction, useGravity, useResistance, bounceFactor, dieOnCollision, spriteEffect, friction, resistance, scale)
        {

        }

        public override void Update(Room room, Random rand)
        {
            base.Update(room, rand);

            movement += new Vector2(0, ((float)(rand.NextDouble() - 1) * 0.1f));
            if (!live)
            {

            }
        }
    }

    public TutorialWeapon(GraphicsDevice graphicsDevice) : base()
    {
        rand = new Random();
        this.Name = "Noob's Gun";
        this.Level = 1;
        this.Experience = 0;
        this.ExperienceCap = 1000;
        this.Damage = 1;
        this.AttackSpeed = 5;
        this.CritChance = 0;
        this.CritHit = 1.2f;
        AttackUpgradeList[0][0] = 1;
        AttackUpgradeList[1][0] = 1;
        AttackUpgradeList[2][0] = 1;

        spr_bullet = "spr_bullet";
        spr_grenade = "spr_grenade";
        spr_rocket = "spr_rocket";

    }

    public override void Update(Player player, Room room, bool leftHand)
    {
        base.Update(player, room, leftHand);
        keyboard = Keyboard.GetState();

        Check_Attack1(leftHand);
        Check_Attack2(leftHand);
        Check_Attack3(leftHand);

        if (e_attacks == e_Attacks.a1)
        {
            if (AttackUpgradeList[0][2] == 1)
            {

            }
            else if (AttackUpgradeList[0][1] == 1)
            {

            }
            else if (AttackUpgradeList[0][0] == 1)
            {

            }
        }
        if (e_attacks == e_Attacks.a2)
        {
            if (AttackUpgradeList[1][2] == 1)
            {

            }
            else if (AttackUpgradeList[1][1] == 1)
            {

            }
            else if (AttackUpgradeList[1][0] == 1)
            {

            }
        }
        if (e_attacks == e_Attacks.a3)
        {
            if (AttackUpgradeList[2][1] == 1)
            {

            }
            else if (AttackUpgradeList[2][0] == 1)
            {

            }
        }

    }

}
*/



/*if (e_attacks == e_Attacks.a1)
            {
                if (AttackUpgradeList[0][2] == 1)
                {

                }
                else if (AttackUpgradeList[0][1] == 1)
                {

                }
                else if (AttackUpgradeList[0][0] == 1)
                {

                }
            }
            if (e_attacks == e_Attacks.a2)
            {
                if (AttackUpgradeList[1][2] == 1)
                {

                }
                else if (AttackUpgradeList[1][1] == 1)
                {

                }
                else if (AttackUpgradeList[1][0] == 1)
                {

                }
            }
            if (e_attacks == e_Attacks.a3)
            {
                if (AttackUpgradeList[1][1] == 1)
                {

                }
                else if (AttackUpgradeList[1][0] == 1)
                {

                }
            }
            

 * 
 * 
 */

