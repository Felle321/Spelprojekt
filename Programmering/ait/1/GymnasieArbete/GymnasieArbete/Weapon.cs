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
        public e_Attacks e_attacks = e_Attacks.a0;
        /// <summary>
        /// True =  Left Hand
        /// False = Right Hand
        /// </summary>
        public float leftDepth = 0.49f;
        public float rightDepth = 0.51f;
        private bool leftHand;
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
        public float CritChance { get; set; }
        public float CritHit { get; set; }
        public float RoomCharge { get; set; }
        public List<List<int>> AttackUpgradeList { get; set; }
        public Vector2 ShootOrigin { get; set; }
        public Vector2 LeftOrigin { get; set; }
        public Vector2 RightOrigin { get; set; } 
        public float attackTimer = 0;
        public string spr_lJump, spr_lStall, spr_lFall, spr_lShoot1_1, spr_lShoot1_2, spr_lShoot1_3, spr_lShoot2_1, spr_lShoot2_2, spr_lShoot2_3, spr_lShoot3_1, spr_lShoot3_2, spr_lIdle, spr_lRunning, spr_rJump, spr_rStall, spr_rFall, spr_rShoot1_1, spr_rShoot1_2, spr_rShoot1_3, spr_rShoot2_1, spr_rShoot2_2, spr_rShoot2_3, spr_rShoot3_1, spr_rShoot3_2, spr_rIdle, spr_rRunning;
        protected SpriteEffects spriteEffect;
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


        public virtual void Check_Attack1(bool leftHand)
        {
            if (leftHand)
            {
                if (keyboard.IsKeyDown(Keys.E) && (e_attacks == e_Attacks.a0 || e_attacks == e_Attacks.a3))
                {
                    e_attacks = e_Attacks.a3;
                    Attack1();
                }
                else if (keyboardPrev.IsKeyDown(Keys.E) && keyboard.IsKeyUp(Keys.E) && e_attacks == e_Attacks.a3)
                {
                    releaseCharge = true;
                    e_attacks = e_Attacks.a0;
                }
            }
            else
            {
                if (keyboard.IsKeyDown(Keys.D) && (e_attacks == e_Attacks.a0 || e_attacks == e_Attacks.a3))
                {
                    e_attacks = e_Attacks.a3;
                    Attack1();
                }
                else if (keyboardPrev.IsKeyDown(Keys.D) && keyboard.IsKeyUp(Keys.D) && e_attacks == e_Attacks.a3)
                {
                    releaseCharge = true;
                    e_attacks = e_Attacks.a0;
                }
            }
        }
        public virtual void Check_Attack2(bool leftHand)
        {
            if (leftHand)
            {
                if (keyboard.IsKeyDown(Keys.W) && (e_attacks == e_Attacks.a0 || e_attacks == e_Attacks.a2))
                {
                    e_attacks = e_Attacks.a2;
                    Attack2();
                }
                else if (keyboardPrev.IsKeyDown(Keys.W) && keyboard.IsKeyUp(Keys.W) && e_attacks == e_Attacks.a2)
                {
                    releaseCharge = true;
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
                else if (keyboardPrev.IsKeyDown(Keys.S) && keyboard.IsKeyUp(Keys.S) && e_attacks == e_Attacks.a2)
                {
                    releaseCharge = true;
                    e_attacks = e_Attacks.a0;
                }
            }
        }
        public virtual void Check_Attack3(bool leftHand)
        {
            if (leftHand)
            {
                if (keyboard.IsKeyDown(Keys.Q) && (e_attacks == e_Attacks.a0 || e_attacks == e_Attacks.a3))
                {
                    e_attacks = e_Attacks.a1;
                    Attack3();
                }
                else if (keyboardPrev.IsKeyDown(Keys.Q) && keyboard.IsKeyUp(Keys.Q) && e_attacks == e_Attacks.a1)
                {
                    e_attacks = e_Attacks.a0;
                    releaseCharge = true;
                }
            }
            else
            {
                if (keyboard.IsKeyDown(Keys.A) && (e_attacks == e_Attacks.a0 || e_attacks == e_Attacks.a1))
                {
                    e_attacks = e_Attacks.a1;
                    Attack3();
                }
                else if (keyboardPrev.IsKeyDown(Keys.A) && keyboard.IsKeyUp(Keys.A) && e_attacks == e_Attacks.a1)
                {
                    e_attacks = e_Attacks.a0;
                    releaseCharge = true;
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

            if (player.movement.X <= 0.6f && player.movement.X >= -0.6f && player.movement.Y <= 0.1f && player.movement.Y >= -0.1f)
            {
                state = WeaponState.idle;
            }

            if ((player.movement.X >= 0.6f || player.movement.X <= -0.6f && player.onGround))
            {
                state = WeaponState.run;
            }


            if (keyboard.IsKeyDown(Keys.E) || keyboard.IsKeyDown(Keys.W) || keyboard.IsKeyDown(Keys.Q))
            {
                player.usingLeft = true;
            }
            else
            {
                player.usingLeft = false;
            }

            if (keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.S) || keyboard.IsKeyDown(Keys.D))
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
            Damage += 3 + Level / 2;
            AttackSpeed += 0.1f;
            if (CritChance <= 50)
            { CritChance += 1.5f; }
            else { CritChance += (100 - CritChance) / 9; }
            CritHit += 0.1f;

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

        public virtual void Draw(Player player, Random rand, SpriteBatch spriteBatch, Camera camera, bool leftHand)
        {
        }
    }



    public class EnergyBlaster : Weapon
    {
        public class ExplodingEnergyBall : Projectile
        {
            public string smallerBall;
            List<GameObject.Types> attackable = new List<Types>();
            public ExplodingEnergyBall(int ID, string smallerball, string sprite, Vector2 pos, Vector2 vel, float damage, float knockback, float lifespan, Room room, bool friendly, bool useFriction, bool useGravity, bool useResistance, float bounceFactor, bool dieOnCollision,SpriteEffects spriteEffect, List<GameObject.Types> attackable, float friction, float resistance, float scale) : base(ID, sprite, pos, vel, damage, knockback, lifespan, room, friendly, useFriction, useGravity, useResistance, bounceFactor, dieOnCollision, spriteEffect, attackable, friction, resistance, scale)
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
                        room.gameObjects.Add(new Projectile(room.gameObjects.Count, smallerBall, position, new Vector2((float)rand.NextDouble() * rand.Next(-1, 2), (float)rand.NextDouble() * rand.Next(-1, 2)) * new Vector2(20, 20), Damage, Knockback, 0.1f + (float)rand.NextDouble() * 0.1f, room, true, false, false, false, 0, true, spriteEffect, attackable));
                    }
                    Animation animation = new Animation("sprite");
                    if (animation.AnimationEnd)
                        animation = new Animation("andra sprite");
                }
            }
        }

        public class SmallWave : Projectile
        {
            Animation startAnimation, midAnimation;
            int aniController = 0;
            public SmallWave(int ID, Animation startAni, Animation midAni, Vector2 pos, Vector2 vel, float damage, float knockback, float lifespan, Room room, bool friendly, bool useFriction, bool useGravity, bool useResistance, float bounceFactor, bool dieOnCollision, SpriteEffects spriteEffect, List<GameObject.Types> attackable, float friction, float resistance, float scale) : base(ID, startAni, pos, vel, damage, knockback, lifespan, room, friendly, useFriction, useGravity, useResistance, bounceFactor, dieOnCollision, spriteEffect, attackable, friction, resistance, scale)
            {
                startAnimation = startAni;
                midAnimation = midAni;
                position = pos;

            }
            public override void Draw(Random rand, SpriteBatch spriteBatch, Camera camera, GraphicsDevice graphicsDevice)
            {
                if (aniController == 0)
                {
                    startAnimation.Draw(rand, spriteBatch, camera, position, spriteEffect, 0.50f);
                    if(startAnimation.AnimationEnd)
                    {
                        aniController++;
                    }
                }
                if(aniController == 1)
                {
                    midAnimation.Draw(rand, spriteBatch, camera, position, spriteEffect, 0.50f);
                }

            }
        }
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
            spr_lRunning = "Energy_lRunning";
            spr_lShoot1_1 = "Energy_lShoot1_1";
            spr_lShoot2_1 = "Energy_lShoot2_1";
            spr_rShoot1_1 = "Energy_rShoot1_1";
            spr_rShoot2_1 = "Energy_rShoot2_1";
            spr_rFall = "Energy_rFall";
            spr_rIdle = "Energy_rIdle";
            spr_rJump = "Energy_rJump";
            spr_rRunning = "Energy_rRunning";
            
            attackable.Add(GameObject.Types.Enemy);
            attackable.Add(GameObject.Types.Prop);




            this.Name = "Energiya pushki";
            this.Level = 1;
            this.Experience = 0;
            this.ExperienceCap = 1000;
            this.Damage = 10;
            this.AttackSpeed = 5;
            this.CritChance = 0;
            this.CritHit = 1.2f;
            AttackUpgradeList[0][0] = 1;
            AttackUpgradeList[1][0] = 1;
            AttackUpgradeList[2][0] = 1;
            


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
                    ShootOrigin = HandOrigin(player.position, 50 + 16, 30) + player.textureOffset;
                }
                else
                {
                    ShootOrigin = HandOrigin(player.position, 50 -1, 30) + player.textureOffset;
                }
            }
            if (spriteEffect == SpriteEffects.FlipHorizontally)
            {
                if (leftHand)
                {
                    ShootOrigin = HandOrigin(player.position, 50 - 75, 30) + player.textureOffset; ;
                }
                else
                {
                    ShootOrigin = HandOrigin(player.position, -5, 30) + player.textureOffset; ;
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
                    state = WeaponState.shoot1_1;
                    ChargeTimer++;
                    ChargeTimerCap = 60 * 2;
                    chargedProj = new ExplodingEnergyBall(room.gameObjects.Count, spr_energyballSmall, spr_energyballCharged, ShootOrigin - new Vector2(0, SpriteHandler.sprites[spr_energyballCharged].height /2), new Vector2(10 * player.direction, 0), Damage, 0, 0.2f + (float)(ChargeTimer / ChargeTimerCap) * 1.5f, room, true, false, false, false, 0, true, spriteEffect, attackable, 0, 0, (float)(ChargeTimer / ChargeTimerCap));
                }
                else if (AttackUpgradeList[0][1] == 1)
                {
                    hasCharge = true;
                    state = WeaponState.shoot1_1;
                    ChargeTimer++;
                    ChargeTimerCap = 90/AttackSpeed;
                    chargedProj = new Projectile(room.gameObjects.Count, spr_energyballCharged, ShootOrigin - new Vector2(0, SpriteHandler.sprites[spr_energyballCharged].height / 2), new Vector2(10 * player.direction, 0), Damage, 0, 0.2f + (float)(ChargeTimer / ChargeTimerCap) * 1.5f, room, true, false, false, false, 0, true, spriteEffect, attackable, 0, 0, (float)(ChargeTimer / ChargeTimerCap));
                }
                else if (AttackUpgradeList[0][0] == 1)
                {
                    hasCharge = false;
                    state = WeaponState.shoot1_1;
                    if (attackTimer >= (float)(60 / (float)(AttackSpeed * 1.2)))
                    {
                        Damage /= 10;
                        SpriteHandler.sprites[spr_lShoot1_1].currentFrame = 0;
                        SpriteHandler.sprites[spr_rShoot1_1].currentFrame = 0;
                        SpriteHandler.sprites[spr_rShoot1_1].speed = 0.7f;
                        SpriteHandler.sprites[spr_lShoot1_1].speed = 0.7f;
                        float temp = (float)rand.NextDouble() + 0.5f;
                        Animation ani = new Animation(spr_energyballSmall);
                        ani.speed = (6 / temp)/60;
                        room.gameObjects.Add(new Projectile(room.gameObjects.Count, ani, ShootOrigin - new Vector2(0, SpriteHandler.sprites[spr_energyballSmall].height / 4), new Vector2(10 * player.direction, 0), Damage, 0, temp, room, true, false, false, false, 0, true, spriteEffect, attackable));
                        attackTimer = 0;
                        Damage *= 10;
                    }
                }
                if (SpriteHandler.sprites[spr_rShoot1_1].AnimationEnd) { SpriteHandler.sprites[spr_rShoot1_1].speed = 0f; }
                if (SpriteHandler.sprites[spr_lShoot1_1].AnimationEnd) { SpriteHandler.sprites[spr_lShoot1_1].speed = 0f; }
            }
            if (e_attacks == e_Attacks.a2)
            {
                if (AttackUpgradeList[1][2] == 1)
                {
                    hasCharge = false;
                    state = WeaponState.shoot2_1;
                    room.gameObjects.Add(new Projectile(room.gameObjects.Count, spr_xWave, ShootOrigin - new Vector2(0, SpriteHandler.sprites[spr_xWave].height / 3), new Vector2(10 * player.direction, 0), Damage, 1, 2, room, true, false, false, false, 0, true, spriteEffect, attackable));
                }
                else if (AttackUpgradeList[1][1] == 1)
                {
                    hasCharge = false;
                    state = WeaponState.shoot2_1;
                    room.gameObjects.Add(new Projectile(room.gameObjects.Count, spr_biggerWave, ShootOrigin - new Vector2(0, SpriteHandler.sprites[spr_biggerWave].height / 3), new Vector2(13 * player.direction, 0), Damage, 2, 3, room, true, false, false, false, 0, true, spriteEffect, attackable));
                }
                else if (AttackUpgradeList[1][0] == 1)
                {
                    float attackspdDeviation = 0.5f;
                    hasCharge = false;
                    state = WeaponState.shoot2_1;
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
                        room.gameObjects.Add(new SmallWave(room.gameObjects.Count, startAni, midAni, ShootOrigin - new Vector2(0, startAni.Height / 3), new Vector2(15 * player.direction, 0), Damage, 2, rand.Next(1,3) * 0.5f, room, true, false, false, false, 0, true, spriteEffect, attackable, 0, 0, 1));
                        attackTimer = 0;
                    }
                    if (SpriteHandler.sprites[spr_rShoot2_1].AnimationEnd) { SpriteHandler.sprites[spr_rShoot2_1].speed = 0f; }
                    if (SpriteHandler.sprites[spr_lShoot2_1].AnimationEnd) { SpriteHandler.sprites[spr_lShoot2_1].speed = 0f; }
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


        public override void Draw(Player player, Random rand, SpriteBatch spriteBatch, Camera camera, bool leftHand)
        {
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
                        SpriteHandler.Draw(spr_lIdle, rand, spriteBatch, camera, drawPos - new Vector2(1 * player.direction,0), spriteEffect, leftDepth);
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
                    room.gameObjects.Add(new Projectile(spr_bullet, ShootOrigin, new Vector2(10 * player.direction, (float)(rand.NextDouble() - 0.5f)), Damage, 1, 4, room, true, false, false, false, 0f, true, spriteEffect));
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
                    room.gameObjects.Add(new Grenade(spr_grenade, ShootOrigin, new Vector2(6 * player.direction, (float)rand.NextDouble()), Damage, 3, 1, room, true, true, true, false, 0.1f, false, spriteEffect, 0.98f, 0, 0));
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
            
        }
        
    }
    */
}


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

