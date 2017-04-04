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
    public class Buff
    {
        public string buffTag = "";
        public bool canStack = false;
        public float duration = 0;
        public bool applied = false;
        public bool live = true;
        public bool remove = false;
        public float hp = 1f;
        /// <summary>
        /// If factor is true is will be multiplied
        /// If factor is false it will be added
        /// </summary>
        public bool hpFactor = true;
        public float dmg = 1f;
        /// <summary>
        /// If factor is true is will be multiplied
        /// If factor is false it will be added
        /// </summary>
        public bool dmgFactor = true;
        public float jumpSpeed = 1f;
        /// <summary>
        /// If factor is true is will be multiplied
        /// If factor is false it will be added
        /// </summary>
        public bool jumpFactor = true;
        public float speed = 1f;
        /// <summary>
        /// If factor is true is will be multiplied
        /// If factor is false it will be added
        /// </summary>
        public bool speedFactor = true;
        /// <summary>
        /// If you want to use other than only factors
        /// </summary>
        /// <param name="hp"></param>
        /// <param name="hpFactor"></param>
        /// <param name="speed"></param>
        /// <param name="speedFactor"></param>
        /// <param name="jumpSpeed"></param>
        /// <param name="jumpFactor"></param>
        public Buff(float hp, bool hpFactor, float speed, bool speedFactor, float jumpSpeed, bool jumpFactor, float duration, string buffTag, bool canStack)
        {
            this.hp = hp;
            this.hpFactor = hpFactor;
            this.speed = speed;
            this.speedFactor = speedFactor;
            this.jumpSpeed = jumpSpeed;
            this.jumpFactor = jumpFactor;
            this.duration = duration;
            this.buffTag = buffTag;
            this.canStack = canStack;
        }
        /// <summary>
        /// If you only want to use factors
        /// </summary>
        /// <param name="hp"></param>
        /// <param name="speed"></param>
        /// <param name="jumpSpeed"></param>
        public Buff(float hp, float speed, float jumpSpeed, float duration, string buffTag, bool canStack)
        {
            this.hp = hp;
            this.speed = speed;
            this.jumpSpeed = jumpSpeed;
            this.duration = duration;
            this.buffTag = buffTag;
            this.canStack = canStack;
        }

        public void Update()
        {
            duration--;
        }


    }
}
