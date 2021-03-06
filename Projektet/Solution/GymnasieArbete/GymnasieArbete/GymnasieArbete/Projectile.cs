﻿using System;
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
    public class Projectile : GameObject
    {
        public float Damage { get; set; }
        public float Knockback { get; set; }
        public float Lifespan { get; set; }
        public SpriteEffects spriteEffect;
        public Animation animation;
        /// <summary>
        /// If it will look at where is going or not
        /// </summary>
        public bool lookAtMovement;

        /// <summary>
        /// True = Hurts enemies
        /// False = Hurts player
        /// </summary>
        public bool isFriendly = true;
        private float lifeTimer = 0;
        private bool dieOnCol = false;
        public DamageZone damageZone;




        /// <summary>
        /// 
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="sprite"></param>
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
        /// <param name="spriteEffect"></param>
        public Projectile(int ID, string sprite, Vector2 pos, Vector2 vel, float damage, float knockback, float lifespan, Room room, bool friendly, bool useFriction, bool useGravity, bool useResistance, float bounceFactor, bool dieOnCollision, SpriteEffects spriteEffect, List<GameObject.Types> attackable, bool lookAtMovement) : base(new Rectangle((int)pos.X, (int)pos.Y, SpriteHandler.sprites[sprite].width, SpriteHandler.sprites[sprite].height), 0, room, sprite)
        {
            this.lookAtMovement = lookAtMovement;
            type = Types.Projectile;
            this.spriteEffect = spriteEffect;
            this.isFriendly = friendly;
            this.ID = ID;
            movement = vel;
            this.Damage = damage;
            this.Knockback = knockback;
            this.Lifespan = lifespan * 60;
            base.useFriction = useFriction;
            base.useGravity = useGravity;
            base.useResistance = useResistance;
            base.bounceFactor = bounceFactor;
            dieOnCol = dieOnCollision;
            damageZone = new DamageZone(new Rectangle((int)pos.X, (int)pos.Y, (int)((float)(SpriteHandler.sprites[sprite].width)), (int)((float)(SpriteHandler.sprites[sprite].height))), Damage, 60, 0, ID, true, true, attackable);
            room.damageZones.Add(damageZone);
            angle = (float)Math.Atan2(movement.Y, movement.X);
        }
        public Projectile(int ID, string sprite, Vector2 pos, Vector2 vel, float damage, float knockback, float lifespan, Room room, bool friendly, bool useFriction, bool useGravity, bool useResistance, float bounceFactor, bool dieOnCollision, SpriteEffects spriteEffect, List<GameObject.Types> attackable, bool lookAtMovement, float friction, float resistance, float scale) : base(new Rectangle((int)pos.X, (int)pos.Y, (int)((float)(SpriteHandler.sprites[sprite].width * scale)), (int)((float)(SpriteHandler.sprites[sprite].height * scale))), 0, room, sprite)
        {
            this.lookAtMovement = lookAtMovement;
            this.ID = ID;
            type = Types.Projectile;
            this.spriteEffect = spriteEffect;
            this.isFriendly = friendly;
            base.friction = friction;
            base.airResitance = resistance;
            movement = vel;
            this.Damage = damage;
            this.Knockback = knockback;
            this.Lifespan = lifespan * 60;
            base.useFriction = useFriction;
            base.useGravity = useGravity;
            base.useResistance = useResistance;
            base.bounceFactor = bounceFactor;
            dieOnCol = dieOnCollision;
            damageZone = new DamageZone(new Rectangle((int)pos.X, (int)pos.Y, (int)((float)(SpriteHandler.sprites[sprite].width * scale)), (int)((float)(SpriteHandler.sprites[sprite].height * scale))), Damage, 60, 0, ID, true, true, attackable);
            room.damageZones.Add(damageZone);
            angle = (float)Math.Atan2(movement.Y, movement.X);
        }
        public Projectile(int ID, Animation ani, Vector2 pos, Vector2 vel, float damage, float knockback, float lifespan, Room room, bool friendly, bool useFriction, bool useGravity, bool useResistance, float bounceFactor, bool dieOnCollision, SpriteEffects spriteEffect, List<GameObject.Types> attackable, bool lookAtMovement) : base(new Rectangle((int)pos.X, (int)pos.Y, ani.Width, ani.Height), 0, room, "")
        {
            this.lookAtMovement = lookAtMovement;
            this.ID = ID;
            type = Types.Projectile;
            this.spriteEffect = spriteEffect;
            animation = ani;
            this.isFriendly = friendly;
            movement = vel;
            this.Damage = damage;
            this.Knockback = knockback;
            this.Lifespan = lifespan * 60;
            base.useFriction = useFriction;
            base.useGravity = useGravity;
            base.useResistance = useResistance;
            base.bounceFactor = bounceFactor;
            dieOnCol = dieOnCollision;
            damageZone = new DamageZone(new Rectangle((int)pos.X, (int)pos.Y, ani.Width, ani.Height), Damage, 60, 0, ID, true, true, attackable);
            room.damageZones.Add(damageZone);
            angle = (float)Math.Atan2(movement.Y, movement.X);
        }
        public Projectile(int ID, Animation ani, Vector2 pos, Vector2 vel, float damage, float knockback, float lifespan, Room room, bool friendly, bool useFriction, bool useGravity, bool useResistance, float bounceFactor, bool dieOnCollision, SpriteEffects spriteEffect, List<GameObject.Types> attackable, bool lookAtMovement, float friction, float resistance, float scale) : base(new Rectangle((int)pos.X, (int)pos.Y, (int)(ani.Width * scale), (int)(ani.Height * scale)), 0, room, "")
        {
            this.lookAtMovement = lookAtMovement;
            this.ID = ID;
            type = Types.Projectile;
            this.spriteEffect = spriteEffect;
            animation = ani;
            this.isFriendly = friendly;
            base.friction = friction;
            base.airResitance = resistance;
            movement = vel;
            this.Damage = damage;
            this.Knockback = knockback;
            this.Lifespan = lifespan * 60;
            base.useFriction = useFriction;
            base.useGravity = useGravity;
            base.useResistance = useResistance;
            base.bounceFactor = bounceFactor;
            dieOnCol = dieOnCollision;
            damageZone = new DamageZone(new Rectangle((int)pos.X, (int)pos.Y, (int)((float)(ani.Width * scale)), (int)((float)(ani.Height * scale))), Damage, 60, 0, ID, true, true, attackable);
            room.damageZones.Add(damageZone);
            angle = (float)Math.Atan2(movement.Y, movement.X);
        }
        public override void TakeDamage(Room room, int sourceID, float damage)
        {
            base.TakeDamage(room, sourceID, damage);
        }

        public override void Update(Room room, Random rand)
        {
            damageZone.Position = position;
            if (lookAtMovement)
            {
                angle = (float)Math.Atan2(movement.Y, movement.X);
            }
            base.Update(room, rand);
            lifeTimer++;
            for (int i = 0; i < room.gameObjects.Count; i++)
            {
                if (lifeTimer >= Lifespan || (dieOnCol && (onGround || onWall || onSlope || Rectangle.Intersects(room.gameObjects[i].Rectangle))))
                {
                    if (dieOnCol && onGround || onWall || onSlope)
                    {
                        live = false;
                    }
                    if (isFriendly)
                    {
                        if (room.gameObjects[i].type == Types.Enemy)
                        { live = false; }
                    }
                    else
                    {
                        if (room.gameObjects[i].type == Types.Player)
                        { live = false; }
                    }
                }
            }
        }
        public override void Draw(Point offset, Random rand, SpriteBatch spriteBatch, GraphicsDevice graphics, Camera camera, Color color)
        {
            animation.Draw(rand, spriteBatch, camera, position, 1f, angle, new Vector2(animation.Width / 2, animation.Height / 2), Color.White, 1f, SpriteEffects.None, 0.5f);
        }

        public override void DrawCol(GraphicsDevice graphics, SpriteBatch spriteBatch)
        {
            Game1.DrawRectangle(graphics, spriteBatch, Rectangle, Color.Blue);
        }
    }

}
