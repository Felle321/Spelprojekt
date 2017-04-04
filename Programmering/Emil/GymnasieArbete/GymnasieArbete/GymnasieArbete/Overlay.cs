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
    public class Overlay
    {
        public string sprite;
        public float strength;
        public int duration;
        public int timer;
        public Color color = Color.White;
        public bool remove = false;
        public float Opacity
        {
            get
            {
                if (timer == 0 || timer == duration)
                {
                    return 0;
                }
                else if (timer > 0.6f * duration)
                {
                    return (float)Math.Pow((duration - timer) / (float)(duration - 0.6f * duration), 2) * strength;
                }
                else if (timer < 0.4f * duration)
                {
                    return strength * (float)(timer / (0.4f * duration));
                }
                else return strength;
            }
        }

        public Overlay(string sprite, int duration, float strength, Color color)
        {
            this.sprite = sprite;
            this.duration = duration;
            this.strength = strength;
            this.color = color;
            this.timer = duration;
        }

        public void Update()
        {
            if (timer < 1)
                remove = true;
            else
                timer--;
        }

        public void Draw(Random rand, SpriteBatch spriteBatch, Camera camera)
        {
            if(timer > 0)
                SpriteHandler.Draw(sprite, rand, spriteBatch, camera, Vector2.Zero, 1f, 0f, Vector2.Zero, color, Opacity, SpriteEffects.None, 0);
        }

        public void AddOverlay(Overlay newOverlay)
        {
            float newTimer = timer / (float)duration;
            duration = (newOverlay.duration + duration) / 2;
            strength = (strength + newOverlay.strength) / 2;
            color = Color.Lerp(color, newOverlay.color, .5f);
            timer = (int)((newTimer + 1) * .5f * duration);
        }
    }
}
