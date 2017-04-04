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

namespace Menu
{
    public class Bar
    {
        Sprite overlay, background, bar;
        Vector2 position;
        float value;
        bool visible;
        float depth;

        public Bar(Sprite overlay, Sprite background, Sprite bar, float depth)
        {
            this.overlay = overlay;
            this.background = background;
            this.bar = bar;
        }

        public void Draw(Random rand, SpriteBatch spriteBatch, Camera camera)
        {
            background.Draw(rand, spriteBatch, camera, position, SpriteEffects.None, depth);
            bar.Draw(rand, spriteBatch, camera, position, SpriteEffects.None, depth);
            overlay.Draw(rand, spriteBatch, camera, position, SpriteEffects.None, depth);
        }

        public static Bar CreateBar(string path)
        {
            return new Bar()
        }
    }
}
