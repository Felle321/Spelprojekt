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
    public static class SpriteHandler
    {
        public static Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

        /// <summary>
        /// Loads and adds a sprite to the SpriteHandler
        /// </summary>
        /// <param name="key">The key which will be used as a reference</param>
        /// <param name="pathName">The name of the sprite in the folder</param>
        /// <param name="graphics">GraphicsDevice</param>
        public static void AddSprite(string key, string pathName, GraphicsDevice graphics)
        {
            sprites.Add(key, Game1.LoadSpriteStatic(pathName, graphics));
        }

        /// <summary>
        /// Loads and adds a sprite to the SpriteHandler
        /// </summary>
        /// <param name="key">The key which will be used as a reference</param>
        /// <param name="subFolderPath">The subfolder of Library, in which the sprite lies</param>
        /// <param name="pathName">The name of the sprite in the folder</param>
        /// <param name="graphics">GraphicsDevice</param>
        public static void AddSprite(string key, string subFolderPath, string pathName, GraphicsDevice graphics)
        {
            sprites.Add(key, Game1.LoadSpriteStatic(subFolderPath, pathName, graphics));
        }

        /// <summary>
        /// Draws the sprite with the given key
        /// </summary>
        /// <param name="key">The key which decides what sprite to be used</param>
        /// <param name="rand"></param>
        /// <param name="spriteBatch"></param>
        /// <param name="camera"></param>
        /// <param name="position">The position of the sprite</param>
        public static void Draw(string key, Random rand, SpriteBatch spriteBatch, Camera camera, Vector2 position, SpriteEffects spriteEffect, float depth)
        {
            sprites[key].Draw(rand, spriteBatch, camera, position, spriteEffect, depth);
        }

        /// <summary>
        /// Draws the sprite with the given key
        /// </summary>
        /// <param name="key">The key which decides what sprite to be used</param>
        /// <param name="rand"></param>
        /// <param name="spriteBatch"></param>
        /// <param name="camera"></param>
        /// <param name="position">The position of the sprite</param>
        /// <param name="vector2Scale">The Vector2 scale of the sprite</param>
        public static void Draw(string key, Random rand, SpriteBatch spriteBatch, Camera camera, Vector2 position, Vector2 vector2Scale, SpriteEffects spriteEffect, float depth)
        {
            sprites[key].Draw(rand, spriteBatch, camera, position, vector2Scale, spriteEffect, depth);
        }

        /// <summary>
        /// Draws the sprite with the given key
        /// </summary>
        /// <param name="key">The key which decides what sprite to be used</param>
        /// <param name="rand"></param>
        /// <param name="spriteBatch"></param>
        /// <param name="camera"></param>
        /// <param name="position">The position of the sprite</param>
        /// <param name="scale"></param>
        /// <param name="angle"></param>
        /// <param name="color"></param>
        /// <param name="origin"></param>
        public static void Draw(string key, Random rand, SpriteBatch spriteBatch, Camera camera, Vector2 position, float scale, float angle, Vector2 origin, Color color, float opacity, SpriteEffects spriteEffect, float depth)
        {
            sprites[key].Draw(rand, spriteBatch, camera, position, scale, angle, origin, color, opacity, spriteEffect, depth);
        }

        /// <summary>
        /// Draws the sprite with the given key
        /// </summary>
        /// <param name="key">The key which decides what sprite to be used</param>
        /// <param name="rand"></param>
        /// <param name="spriteBatch"></param>
        /// <param name="camera"></param>
        /// <param name="position">The position of the sprite</param>
        /// <param name="vector2Scale"></param>
        /// <param name="angle"></param>
        /// <param name="color"></param>
        /// <param name="origin"></param>
        public static void Draw(string key, Random rand, SpriteBatch spriteBatch, Camera camera, Vector2 position, Vector2 vector2Scale, float angle, Vector2 origin, Color color, float opacity, SpriteEffects spriteEffect, float depth)
        {
            sprites[key].Draw(rand, spriteBatch, camera, position, vector2Scale, angle, origin, color, opacity, spriteEffect, depth);
        }


        public static Sprite InstantiateSprite(string key)
        {
            Sprite spr = new Sprite();
            Sprite oldSpr = sprites[key];
            spr.angle = oldSpr.angle;
            spr.color = oldSpr.color;
            spr.width = oldSpr.width;
            spr.height = oldSpr.height;
            spr.scale = oldSpr.scale;
            spr.speed = oldSpr.speed;
            spr.origin = oldSpr.origin;
            spr.HUD = oldSpr.HUD;
            spr.odds = oldSpr.odds;
            spr.children = oldSpr.children;
            spr.frames = oldSpr.frames;
            spr.framesTotal = oldSpr.framesTotal;
            spr.randDeviation = oldSpr.randDeviation;
            spr.randFrequency = oldSpr.randFrequency;
            spr.textures = oldSpr.textures;
            spr.opacity = oldSpr.opacity;

            return spr;
        }
    }
}
