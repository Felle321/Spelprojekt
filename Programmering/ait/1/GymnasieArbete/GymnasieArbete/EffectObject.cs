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
    public class EffectObject
    {
        public enum Type
        {
            idle,
            text
        }
        Type type = Type.idle;
        public Vector2 position;
        public Vector2 movement = Vector2.Zero;
        public string sprite;
        public string text;
        public SpriteFont font;
        public int hp = 1;
        public float scale = 0;
        public float angle = 0;
        public float rotation = 0;
        public Color color = Color.White;

        public EffectObject(Vector2 position, Vector2 movement, string sprite)
        {
            this.position = position;
            this.movement = movement;
            this.sprite = sprite;
        }

        public EffectObject(Vector2 position, Vector2 movement, SpriteFont font, string text, int duration)
        {
            this.position = position;
            this.movement = movement;
            this.font = font;
            this.text = text;
            type = Type.text;
            this.hp = duration;
        }

        public EffectObject(Vector2 position, Vector2 movement, string sprite, float scale, float angle, float rotation, Color color)
        {
            this.position = position;
            this.movement = movement;
            this.sprite = sprite;
            this.scale = scale;
            this.rotation = rotation;
            this.angle = angle;
            this.color = color;
        }

        public EffectObject(Vector2 position, Vector2 movement, SpriteFont font, string text, int duration, float scale, float angle, float rotation, Color color)
        {
            this.position = position;
            this.movement = movement;
            this.font = font;
            this.text = text;
            type = Type.text;
            this.hp = duration;
            this.scale = scale;
            this.rotation = rotation;
            this.angle = angle;
            this.color = color;
        }

        public void Draw(Random rand, SpriteBatch spriteBatch, Camera camera)
        {
            position += movement;
            angle += rotation;
            switch (type)
            {
                case Type.idle:
                    if (hp > 0)
                        SpriteHandler.Draw(sprite, rand, spriteBatch, camera, position - new Vector2(SpriteHandler.sprites[sprite].width / 2 * scale, SpriteHandler.sprites[sprite].height / 2 * scale), scale, angle, position, color, 0f, SpriteEffects.None, 0.50f);
                    if (SpriteHandler.sprites[sprite].AnimationEnd)
                        hp = 0;
                    break;
                case Type.text:
                    hp--;
                    spriteBatch.DrawString(font, text, position, color, angle, position, scale, SpriteEffects.None, 0.50f);
                    break;
                default:
                    break;
            }
        }
    }
}
