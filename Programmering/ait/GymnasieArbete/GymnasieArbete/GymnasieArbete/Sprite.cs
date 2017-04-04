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
using System.IO;

namespace GymnasieArbete
{
    public class Sprite
    {
        public bool animationEnd = false;
        public bool HUD = false;
        public float speed = 1;
        public float scale = 1;
        public float angle = 0;
        public Vector2 origin = Vector2.Zero;
        public Color color = Color.White;
        public int width, height;
        public bool animated = false;
        public bool random = false;
        public int randFrequency = 60;
        public int randDeviation = 30;
        public int coolDown = 60;
        public bool draw = true;
        public bool drawOnce = false;
        public int framesTotal;
        float currentTexture = 0;
        public float currentFrame = 0;
        public float opacity = 1f;

        public List<Texture2D> textures = new List<Texture2D>();
        public List<bool> spriteSheet = new List<bool>();
        public List<int> frames = new List<int>();
        public List<Sprite> children = new List<Sprite>();
        public List<int> odds = new List<int>();


        public Sprite()
        {

        }
        
        public Sprite(string directory, GraphicsDevice graphicsDevice)
        {
            if (!File.Exists(directory + ".png"))
            {
                string[] files = Directory.GetFiles(directory);
                int j = 0;
                while (textures.Count < files.Length - 1 || width == 0)
                {
                    if (Path.GetExtension(files[j]) == ".png" && int.Parse(Path.GetFileNameWithoutExtension(files[j]).Split('_')[0]) == textures.Count)
                    {
                        textures.Add(Texture2D.FromStream(graphicsDevice, File.OpenRead(files[j])));

                        if (j < files.Length)
                            j++;
                        else
                            j = 0;
                    }
                    else if (Path.GetExtension(files[j]) == ".txt")
                    {
                        StreamReader sr = new StreamReader(files[j]);
                        string[] info = sr.ReadToEnd().Split('\n');
                        width = int.Parse(info[0]);
                        height = int.Parse(info[1]);
                        speed = Game1.StringToFloat(info[2]);
                        scale = Game1.StringToFloat(info[3]);
                        if (info[4].Contains("true"))
                            random = true;
                        randFrequency = (int)Math.Round(Game1.StringToFloat(info[5]) * 60);
                        randDeviation = (int)Math.Round(Game1.StringToFloat(info[6]) * 60);

                        if (j < files.Length)
                            j++;
                        else
                            j = 0;
                    }
                }

                animated = true;

                for (int i = 0; i < textures.Count; i++)
                {
                    if (textures[i].Width > width || textures[i].Height > height)
                    {
                        if (Path.GetFileNameWithoutExtension(files[i]).Split('_').Length < 2)
                            frames.Add((int)(Math.Floor(textures[i].Width / (float)width) * Math.Floor(textures[i].Height / (float)height)));
                        else
                            frames.Add(int.Parse(Path.GetFileNameWithoutExtension(files[i]).Split('_')[1]) - 1);
                        spriteSheet.Add(true);
                    }
                    else
                    {
                        frames.Add(1);
                        spriteSheet.Add(false);
                    }
                }

                //Random
                if (random)
                {
                    string[] directories = Directory.GetDirectories(directory + "\\randoms");
                    for (int i = 0; i < directories.Length; i++)
                    {
                        children.Add(new Sprite(directories[i], graphicsDevice));
                        odds.Add(int.Parse(directories[i].Split('\\')[directories[i].Split('\\').Length - 1]));
                    }
                }
                
                coolDown = randFrequency;
            }
            else
            {
                textures.Add(Texture2D.FromStream(graphicsDevice, File.OpenRead(directory + ".png")));
                speed = 0;
                width = textures[0].Width;
                height = textures[0].Height;
                frames.Add(1);
                spriteSheet.Add(false);
            }

            for (int i = 0; i < frames.Count; i++)
            {
                framesTotal += frames[i];
            }

            for (int i = 0; i < textures.Count; i++)
            {
                Color[] data = new Color[textures[i].Width * textures[i].Height];
                textures[i].GetData(data);
                for (int j = 0; j < data.Length; j++)
                {
                    data[j] = Color.FromNonPremultiplied(data[j].ToVector4());
                }
                textures[i].SetData(data);
            }
        }


        #region WithoutAnimation
        /// <summary>
        /// Draws the sprite
        /// </summary>
        /// <param name="rand">An instance of the class Random</param>
        /// <param name="spriteBatch">The currently used SpriteBatch</param>
        /// <param name="camera">The active camera</param>
        /// <param name="position">The position of the Sprite</param>
        public void Draw(Random rand, SpriteBatch spriteBatch, Camera camera, Vector2 position, SpriteEffects spriteEffect, float depth)
        {
            if (currentTexture == textures.Count - 1 && ((int)Math.Floor(currentFrame) == frames[(int)Math.Floor(currentTexture)] || !spriteSheet[(int)Math.Floor(currentTexture)]))
                animationEnd = true;
            else
                animationEnd = false;

            if (draw && (camera.rectangle.Intersects(new Rectangle(Game1.CeilAdv(position.X), Game1.CeilAdv(position.Y), width, height)) || HUD))
            {
                if (!animated)
                {
                    spriteBatch.Draw(textures[0], position, null, GetColorOpaque(color, opacity), angle, origin, scale, spriteEffect, depth);
                }
                else
                {
                    if (spriteSheet[(int)Math.Floor(currentTexture)])
                    {
                        int x = (int)(currentFrame % Math.Floor((float)(textures[(int)Math.Floor(currentTexture)].Width / width))) * width;
                        int y = (int)Math.Floor(currentFrame / (textures[(int)Math.Floor(currentTexture)].Width / width)) * height;
                        spriteBatch.Draw(textures[(int)Math.Floor(currentTexture)], position, new Rectangle(x, y, width, height), GetColorOpaque(color, opacity), angle, origin, scale, spriteEffect, depth);
                    }
                    else
                    {
                        spriteBatch.Draw(textures[(int)Math.Floor(currentTexture)], position, null, GetColorOpaque(color, opacity), angle, origin, scale, spriteEffect, depth);
                    }
                }

                if (random)
                {
                    if (coolDown > 0)
                        coolDown--;
                    else if (animationEnd)
                    {
                        coolDown = randFrequency + rand.Next(randFrequency * 2) - randFrequency;
                        int total = 0;
                        for (int i = 0; i < odds.Count; i++)
                        {
                            total += odds[i];
                        }
                        int chance = rand.Next(total);
                        total = 0;
                        for (int i = 0; i < odds.Count; i++)
                        {
                            total += odds[i];
                            if (chance < total)
                            {
                                children[i].DrawOnce();
                                draw = false;
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                if (random)
                {
                    for (int i = 0; i < children.Count; i++)
                    {
                        children[i].scale = scale;
                        children[i].angle = angle;
                        children[i].origin = origin;
                        children[i].color = color;
                        if (children[i].draw)
                        {
                            children[i].Draw(rand, spriteBatch, camera, position, spriteEffect, depth);
                            if (children[i].animationEnd)
                            {
                                draw = true;
                                children[i].draw = false;
                            }
                        }
                    }
                }
            }

            if (animated)
            {
                if (spriteSheet[(int)Math.Floor(currentTexture)])
                {
                    if (currentFrame >= frames[(int)Math.Floor(currentTexture)])
                    {
                        currentFrame = 0;
                        currentTexture = (currentTexture + 1) % (textures.Count);
                    }
                    else
                        currentFrame = (currentFrame + speed);
                }
                else
                    currentTexture = (currentTexture + speed) % (textures.Count);
            }

            if (drawOnce && animationEnd)
                draw = false;
        }


        /// <summary>
        /// Draws the sprite
        /// </summary>
        /// <param name="rand">An instance of the class Random</param>
        /// <param name="spriteBatch">The currently used SpriteBatch</param>
        /// <param name="camera">The active camera</param>
        /// <param name="position">The position of the Pprite</param>
        /// <param name="vector2Scale">The Vector2 value to rescale the sprite with</param>
        public void Draw(Random rand, SpriteBatch spriteBatch, Camera camera, Vector2 position, Vector2 vector2Scale, SpriteEffects spriteEffect, float depth)
        {
            if (currentTexture == textures.Count - 1 && ((int)Math.Floor(currentFrame) == frames[(int)Math.Floor(currentTexture)] || !spriteSheet[(int)Math.Floor(currentTexture)]))
                animationEnd = true;
            else
                animationEnd = false;

            if (draw && (camera.rectangle.Intersects(new Rectangle(Game1.CeilAdv(position.X), Game1.CeilAdv(position.Y), width, height)) || HUD))
            {
                if (!animated)
                {
                    spriteBatch.Draw(textures[0], position, null, GetColorOpaque(color, opacity), angle, Vector2.Zero, scale, spriteEffect, depth);
                }
                else
                {
                    if (spriteSheet[(int)Math.Floor(currentTexture)])
                    {
                        int x = (int)(currentFrame % Math.Floor((float)(textures[(int)Math.Floor(currentTexture)].Width / width))) * width;
                        int y = (int)Math.Floor(currentFrame / (textures[(int)Math.Floor(currentTexture)].Width / width)) * height;
                        spriteBatch.Draw(textures[(int)Math.Floor(currentTexture)], position, new Rectangle(x, y, width, height), GetColorOpaque(color, opacity), angle, origin, vector2Scale, spriteEffect, depth);
                    }
                    else
                    {
                        spriteBatch.Draw(textures[(int)Math.Floor(currentTexture)], position, null, GetColorOpaque(color, opacity), angle, origin, vector2Scale, spriteEffect, depth);
                    }
                }

                if (random)
                {
                    if (coolDown > 0)
                        coolDown--;
                    else if (animationEnd)
                    {
                        coolDown = randFrequency + rand.Next(randFrequency * 2) - randFrequency;
                        int total = 0;
                        for (int i = 0; i < odds.Count; i++)
                        {
                            total += odds[i];
                        }
                        int chance = rand.Next(total);
                        total = 0;
                        for (int i = 0; i < odds.Count; i++)
                        {
                            total += odds[i];
                            if (chance < total)
                            {
                                children[i].DrawOnce();
                                draw = false;
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                if (random)
                {
                    for (int i = 0; i < children.Count; i++)
                    {
                        if (children[i].draw)
                        {
                            children[i].angle = angle;
                            children[i].origin = origin;
                            children[i].color = color;
                            children[i].Draw(rand, spriteBatch, camera, position, vector2Scale, spriteEffect, depth);
                            if (children[i].animationEnd)
                            {
                                draw = true;
                                children[i].draw = false;
                            }
                        }
                    }
                }
            }

            if (animated)
            {
                if (spriteSheet[(int)Math.Floor(currentTexture)])
                {
                    if (currentFrame >= frames[(int)Math.Floor(currentTexture)])
                    {
                        currentFrame = 0;
                        currentTexture = (currentTexture + 1) % (textures.Count);
                    }
                    else
                        currentFrame = (currentFrame + speed);
                }
                else
                    currentTexture = (currentTexture + speed) % (textures.Count);
            }

            if (drawOnce && animationEnd)
                draw = false;
        }

        /// <summary>
        /// Draws the sprite
        /// </summary>
        /// <param name="rand">An instance of the class Random</param>
        /// <param name="spriteBatch">The currently used SpriteBatch</param>
        /// <param name="camera">The active camera</param>
        /// <param name="position">The position of the Sprite</param>
        /// <param name="scale">The scale the texture will be drawn with (this doesn't exclude the base-scale)</param>
        /// <param name="angle">The angle with which the texture is to be drawn</param>
        /// <param name="origin">The origin of the rotation</param>
        /// <param name="color">The color of the sprite</param>
        public void Draw(Random rand, SpriteBatch spriteBatch, Camera camera, Vector2 position, float scale, float angle, Vector2 origin, Color color, float opacity, SpriteEffects spriteEffect, float depth)
        {
            if (currentTexture == textures.Count - 1 && ((int)Math.Floor(currentFrame) == frames[(int)Math.Floor(currentTexture)] || !spriteSheet[(int)Math.Floor(currentTexture)]))
                animationEnd = true;
            else
                animationEnd = false;

            if (draw && (camera.rectangle.Intersects(new Rectangle(Game1.CeilAdv(position.X), Game1.CeilAdv(position.Y), width, height)) || HUD))
            {
                if (!animated)
                {
                    spriteBatch.Draw(textures[0], position, null, GetColorOpaque(color, opacity), angle, origin, scale * this.scale, spriteEffect, depth);
                }
                else
                {
                    if (spriteSheet[(int)Math.Floor(currentTexture)])
                    {
                        int x = (int)(currentFrame % Math.Floor((float)(textures[(int)Math.Floor(currentTexture)].Width / width))) * width;
                        int y = (int)Math.Floor(currentFrame / (textures[(int)Math.Floor(currentTexture)].Width / width)) * height;
                        bool temp = animationEnd;
                        spriteBatch.Draw(textures[(int)Math.Floor(currentTexture)], position, new Rectangle(x, y, width, height), GetColorOpaque(color, opacity), angle, origin, scale * this.scale, spriteEffect, depth);
                    }
                    else
                    {
                        spriteBatch.Draw(textures[(int)Math.Floor(currentTexture)], position, null, GetColorOpaque(color, opacity), angle, origin, scale * this.scale, spriteEffect, depth);
                    }
                }

                if (random)
                {
                    if (coolDown > 0)
                        coolDown--;
                    else if (animationEnd)
                    {
                        coolDown = randFrequency + rand.Next(randFrequency * 2) - randFrequency;
                        int total = 0;
                        for (int i = 0; i < odds.Count; i++)
                        {
                            total += odds[i];
                        }
                        int chance = rand.Next(total);
                        total = 0;
                        for (int i = 0; i < odds.Count; i++)
                        {
                            total += odds[i];
                            if (chance < total)
                            {
                                children[i].DrawOnce();
                                draw = false;
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                if (random)
                {
                    for (int i = 0; i < children.Count; i++)
                    {
                        children[i].scale = scale * scale;
                        children[i].angle = angle;
                        children[i].origin = origin;
                        children[i].color = color;
                        if (children[i].draw)
                        {
                            children[i].Draw(rand, spriteBatch, camera, position, spriteEffect, depth);
                            if (children[i].animationEnd)
                            {
                                draw = true;
                                children[i].draw = false;
                            }
                        }
                    }
                }
            }

            if (animated)
            {
                if (spriteSheet[(int)Math.Floor(currentTexture)])
                {
                    if (currentFrame >= frames[(int)Math.Floor(currentTexture)])
                    {
                        currentFrame = 0;
                        currentTexture = (currentTexture + 1) % (textures.Count);
                    }
                    else
                        currentFrame = (currentFrame + speed);
                }
                else
                    currentTexture = (currentTexture + speed) % (textures.Count);
            }

            if (drawOnce && animationEnd)
                draw = false;
        }

        /// <summary>
        /// Draws the sprite
        /// </summary>
        /// <param name="rand">An instance of the class Random</param>
        /// <param name="spriteBatch">The currently used SpriteBatch</param>
        /// <param name="camera">The active camera</param>
        /// <param name="position">The position of the Sprite</param>
        /// <param name="scale">The scale the texture will be drawn with (this doesn't exclude the base-scale)</param>
        /// <param name="angle">The angle with which the texture is to be drawn</param>
        /// <param name="origin">The origin of the rotation</param>
        /// <param name="color">The color of the sprite</param>
        public void Draw(Random rand, SpriteBatch spriteBatch, Camera camera, Vector2 position, Vector2 scale, float angle, Vector2 origin, Color color, float opacity, SpriteEffects spriteEffect, float depth)
        {
            if (currentTexture == textures.Count - 1 && ((int)Math.Floor(currentFrame) == frames[(int)Math.Floor(currentTexture)] || !spriteSheet[(int)Math.Floor(currentTexture)]))
                animationEnd = true;
            else
                animationEnd = false;

            if (draw && (camera.rectangle.Intersects(new Rectangle(Game1.CeilAdv(position.X), Game1.CeilAdv(position.Y), width, height)) || HUD))
            {
                if (!animated)
                {
                    spriteBatch.Draw(textures[0], position, null, GetColorOpaque(color, opacity), angle, origin, scale * this.scale, spriteEffect, depth);
                }
                else
                {
                    if (spriteSheet[(int)Math.Floor(currentTexture)])
                    {
                        int x = (int)(currentFrame % Math.Floor((float)(textures[(int)Math.Floor(currentTexture)].Width / width))) * width;
                        int y = (int)Math.Floor(currentFrame / (textures[(int)Math.Floor(currentTexture)].Width / width)) * height;
                        spriteBatch.Draw(textures[(int)Math.Floor(currentTexture)], position, new Rectangle(x, y, width, height), GetColorOpaque(color, opacity), angle, origin, scale * this.scale, spriteEffect, depth);
                    }
                    else
                    {
                        spriteBatch.Draw(textures[(int)Math.Floor(currentTexture)], position, null, GetColorOpaque(color, opacity), angle, origin, scale * this.scale, spriteEffect, depth);
                    }
                }

                if (random)
                {
                    if (coolDown > 0)
                        coolDown--;
                    else if (animationEnd)
                    {
                        coolDown = randFrequency + rand.Next(randFrequency * 2) - randFrequency;
                        int total = 0;
                        for (int i = 0; i < odds.Count; i++)
                        {
                            total += odds[i];
                        }
                        int chance = rand.Next(total);
                        total = 0;
                        for (int i = 0; i < odds.Count; i++)
                        {
                            total += odds[i];
                            if (chance < total)
                            {
                                children[i].DrawOnce();
                                draw = false;
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                if (random)
                {
                    for (int i = 0; i < children.Count; i++)
                    {
                        children[i].angle = angle;
                        children[i].origin = origin;
                        children[i].color = color;
                        if (children[i].draw)
                        {
                            children[i].Draw(rand, spriteBatch, camera, position, this.scale * scale, spriteEffect, depth);
                            if (children[i].animationEnd)
                            {
                                draw = true;
                                children[i].draw = false;
                            }
                        }
                    }
                }
            }

            if (animated)
            {
                if (spriteSheet[(int)Math.Floor(currentTexture)])
                {
                    if (currentFrame >= frames[(int)Math.Floor(currentTexture)])
                    {
                        currentFrame = 0;
                        currentTexture = (currentTexture + 1) % (textures.Count);
                    }
                    else
                        currentFrame = (currentFrame + speed);
                }
                else
                    currentTexture = (currentTexture + speed) % (textures.Count);
            }

            if (drawOnce && animationEnd)
                draw = false;
        }

#endregion


        /// <summary>
        /// Draws the sprite
        /// </summary>
        /// <param name="rand">An instance of the class Random</param>
        /// <param name="spriteBatch">The currently used SpriteBatch</param>
        /// <param name="camera">The active camera</param>
        /// <param name="position">The position of the Sprite</param>
        public void Draw(Random rand, SpriteBatch spriteBatch, Camera camera, Vector2 position, Animation animation, SpriteEffects spriteEffect, float depth)
        {
            if (currentTexture == textures.Count - 1 && ((int)Math.Floor(currentFrame) == frames[(int)Math.Floor(currentTexture)] || !spriteSheet[(int)Math.Floor(currentTexture)]))
                animationEnd = true;
            else
                animationEnd = false;

            if (draw && (camera.rectangle.Intersects(new Rectangle(Game1.CeilAdv(position.X), Game1.CeilAdv(position.Y), width, height)) || HUD))
            {
                if (!animated)
                {
                    spriteBatch.Draw(textures[0], position, null, GetColorOpaque(color, opacity), angle, origin, scale, spriteEffect, depth);
                }
                else
                {
                    if (spriteSheet[(int)Math.Floor(animation.currentTexture)])
                    {
                        int x = (int)(animation.currentFrame % Math.Floor((float)(textures[(int)Math.Floor(animation.currentTexture)].Width / width))) * width;
                        int y = (int)Math.Floor(animation.currentFrame / (textures[(int)Math.Floor(animation.currentTexture)].Width / width)) * height;
                        spriteBatch.Draw(textures[(int)Math.Floor(animation.currentTexture)], position, new Rectangle(x, y, width, height), GetColorOpaque(color, opacity), angle, origin, scale, spriteEffect, depth);
                    }
                    else
                    {
                        spriteBatch.Draw(textures[(int)Math.Floor(animation.currentTexture)], position, null, GetColorOpaque(color, opacity), angle, origin, scale, spriteEffect, depth);
                    }
                }

                if (random)
                {
                    if (coolDown > 0)
                        coolDown--;
                    else if (animationEnd)
                    {
                        coolDown = randFrequency + rand.Next(randFrequency * 2) - randFrequency;
                        int total = 0;
                        for (int i = 0; i < odds.Count; i++)
                        {
                            total += odds[i];
                        }
                        int chance = rand.Next(total);
                        total = 0;
                        for (int i = 0; i < odds.Count; i++)
                        {
                            total += odds[i];
                            if (chance < total)
                            {
                                children[i].DrawOnce();
                                draw = false;
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                if (random)
                {
                    for (int i = 0; i < children.Count; i++)
                    {
                        children[i].scale = scale;
                        children[i].angle = angle;
                        children[i].origin = origin;
                        children[i].color = color;
                        if (children[i].draw)
                        {
                            children[i].Draw(rand, spriteBatch, camera, position, spriteEffect, depth);
                            if (children[i].animationEnd)
                            {
                                draw = true;
                                children[i].draw = false;
                            }
                        }
                    }
                }
            }


            if (animated)
            {
                if (spriteSheet[(int)Math.Floor(animation.currentTexture)])
                {
                    if (animation.currentFrame >= frames[(int)Math.Floor(animation.currentTexture)])
                    {
                        animation.currentFrame = 0;
                        animation.currentTexture = (animation.currentTexture + 1) % (textures.Count);
                    }
                    else
                        animation.currentFrame = (animation.currentFrame + animation.speed);
                }
                else
                    animation.currentTexture = (animation.currentTexture + animation.speed) % (textures.Count);
            }

            if (drawOnce && animationEnd)
                draw = false;
        }


        /// <summary>
        /// Draws the sprite
        /// </summary>
        /// <param name="rand">An instance of the class Random</param>
        /// <param name="spriteBatch">The currently used SpriteBatch</param>
        /// <param name="camera">The active camera</param>
        /// <param name="position">The position of the Pprite</param>
        /// <param name="vector2Scale">The Vector2 value to rescale the sprite with</param>
        public void Draw(Random rand, SpriteBatch spriteBatch, Camera camera, Vector2 position, Vector2 vector2Scale, Animation animation, SpriteEffects spriteEffect, float depth)
        {
            if (currentTexture == textures.Count - 1 && ((int)Math.Floor(currentFrame) == frames[(int)Math.Floor(currentTexture)] || !spriteSheet[(int)Math.Floor(currentTexture)]))
                animationEnd = true;
            else
                animationEnd = false;

            if (draw && (camera.rectangle.Intersects(new Rectangle(Game1.CeilAdv(position.X), Game1.CeilAdv(position.Y), width, height)) || HUD))
            {
                if (!animated)
                {
                    spriteBatch.Draw(textures[0], position, null, GetColorOpaque(color, opacity), angle, Vector2.Zero, scale, spriteEffect, depth);
                }
                else
                {
                    if (spriteSheet[(int)Math.Floor(animation.currentTexture)])
                    {
                        int x = (int)(animation.currentFrame % Math.Floor((float)(textures[(int)Math.Floor(animation.currentTexture)].Width / width))) * width;
                        int y = (int)Math.Floor(animation.currentFrame / (textures[(int)Math.Floor(animation.currentTexture)].Width / width)) * height;
                        spriteBatch.Draw(textures[(int)Math.Floor(animation.currentTexture)], position, new Rectangle(x, y, width, height), GetColorOpaque(color, opacity), angle, origin, vector2Scale, spriteEffect, depth);
                    }
                    else
                    {
                        spriteBatch.Draw(textures[(int)Math.Floor(animation.currentTexture)], position, null, GetColorOpaque(color, opacity), angle, origin, vector2Scale, spriteEffect, depth);
                    }
                }

                if (random)
                {
                    if (coolDown > 0)
                        coolDown--;
                    else if (animationEnd)
                    {
                        coolDown = randFrequency + rand.Next(randFrequency * 2) - randFrequency;
                        int total = 0;
                        for (int i = 0; i < odds.Count; i++)
                        {
                            total += odds[i];
                        }
                        int chance = rand.Next(total);
                        total = 0;
                        for (int i = 0; i < odds.Count; i++)
                        {
                            total += odds[i];
                            if (chance < total)
                            {
                                children[i].DrawOnce();
                                draw = false;
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                if (random)
                {
                    for (int i = 0; i < children.Count; i++)
                    {
                        if (children[i].draw)
                        {
                            children[i].angle = angle;
                            children[i].origin = origin;
                            children[i].color = color;
                            children[i].Draw(rand, spriteBatch, camera, position, vector2Scale, spriteEffect, depth);
                            if (children[i].animationEnd)
                            {
                                draw = true;
                                children[i].draw = false;
                            }
                        }
                    }
                }
            }

            if (animated)
            {
                if (spriteSheet[(int)Math.Floor(animation.currentTexture)])
                {
                    if (animation.currentFrame >= frames[(int)Math.Floor(animation.currentTexture)])
                    {
                        animation.currentFrame = 0;
                        animation.currentTexture = (animation.currentTexture + 1) % (textures.Count);
                    }
                    else
                        animation.currentFrame = (animation.currentFrame + animation.speed);
                }
                else
                    animation.currentTexture = (animation.currentTexture + animation.speed) % (textures.Count);
            }

            if (drawOnce && animationEnd)
                draw = false;
        }

        /// <summary>
        /// Draws the sprite
        /// </summary>
        /// <param name="rand">An instance of the class Random</param>
        /// <param name="spriteBatch">The currently used SpriteBatch</param>
        /// <param name="camera">The active camera</param>
        /// <param name="position">The position of the Sprite</param>
        /// <param name="scale">The scale the texture will be drawn with (this doesn't exclude the base-scale)</param>
        /// <param name="angle">The angle with which the texture is to be drawn</param>
        /// <param name="origin">The origin of the rotation</param>
        /// <param name="color">The color of the sprite</param>
        public void Draw(Random rand, SpriteBatch spriteBatch, Camera camera, Vector2 position, float scale, float angle, Vector2 origin, Color color, float opacity, Animation animation, SpriteEffects spriteEffect, float depth)
        {
            if (currentTexture == textures.Count - 1 && ((int)Math.Floor(currentFrame) == frames[(int)Math.Floor(currentTexture)] || !spriteSheet[(int)Math.Floor(currentTexture)]))
                animationEnd = true;
            else
                animationEnd = false;

            if (draw && (camera.rectangle.Intersects(new Rectangle(Game1.CeilAdv(position.X), Game1.CeilAdv(position.Y), width, height)) || HUD))
            {
                if (!animated)
                {
                    spriteBatch.Draw(textures[0], position, null, GetColorOpaque(color, opacity), angle, origin, scale * this.scale, spriteEffect, depth);
                }
                else
                {
                    if (spriteSheet[(int)Math.Floor(animation.currentTexture)])
                    {
                        int x = (int)(animation.currentFrame % Math.Floor((float)(textures[(int)Math.Floor(animation.currentTexture)].Width / width))) * width;
                        int y = (int)Math.Floor(animation.currentFrame / (textures[(int)Math.Floor(animation.currentTexture)].Width / width)) * height;
                        spriteBatch.Draw(textures[(int)Math.Floor(animation.currentTexture)], position, new Rectangle(x, y, width, height), GetColorOpaque(color, opacity), angle, origin, scale * this.scale, spriteEffect, depth);
                    }
                    else
                    {
                        spriteBatch.Draw(textures[(int)Math.Floor(animation.currentTexture)], position, null, GetColorOpaque(color, opacity), angle, origin, scale * this.scale, spriteEffect, depth);
                    }
                }

                if (random)
                {
                    if (coolDown > 0)
                        coolDown--;
                    else if (animationEnd)
                    {
                        coolDown = randFrequency + rand.Next(randFrequency * 2) - randFrequency;
                        int total = 0;
                        for (int i = 0; i < odds.Count; i++)
                        {
                            total += odds[i];
                        }
                        int chance = rand.Next(total);
                        total = 0;
                        for (int i = 0; i < odds.Count; i++)
                        {
                            total += odds[i];
                            if (chance < total)
                            {
                                children[i].DrawOnce();
                                draw = false;
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                if (random)
                {
                    for (int i = 0; i < children.Count; i++)
                    {
                        children[i].scale = scale * scale;
                        children[i].angle = angle;
                        children[i].origin = origin;
                        children[i].color = color;
                        if (children[i].draw)
                        {
                            children[i].Draw(rand, spriteBatch, camera, position, spriteEffect, depth);
                            if (children[i].animationEnd)
                            {
                                draw = true;
                                children[i].draw = false;
                            }
                        }
                    }
                }
            }

            if (animated)
            {
                if (spriteSheet[(int)Math.Floor(animation.currentTexture)])
                {
                    if (animation.currentFrame >= frames[(int)Math.Floor(animation.currentTexture)])
                    {
                        animation.currentFrame = 0;
                        animation.currentTexture = (animation.currentTexture + 1) % (textures.Count);
                    }
                    else
                        animation.currentFrame = (animation.currentFrame + animation.speed);
                }
                else
                    animation.currentTexture = (animation.currentTexture + animation.speed) % (textures.Count);
            }

            if (drawOnce && animationEnd)
                draw = false;
        }

        /// <summary>
        /// Draws the sprite
        /// </summary>
        /// <param name="rand">An instance of the class Random</param>
        /// <param name="spriteBatch">The currently used SpriteBatch</param>
        /// <param name="camera">The active camera</param>
        /// <param name="position">The position of the Sprite</param>
        /// <param name="scale">The scale the texture will be drawn with (this doesn't exclude the base-scale)</param>
        /// <param name="angle">The angle with which the texture is to be drawn</param>
        /// <param name="origin">The origin of the rotation</param>
        /// <param name="color">The color of the sprite</param>
        public void Draw(Random rand, SpriteBatch spriteBatch, Camera camera, Vector2 position, Vector2 scale, float angle, Vector2 origin, Color color, float opacity, Animation animation, SpriteEffects spriteEffect, float depth)
        {
            if (currentTexture == textures.Count - 1 && ((int)Math.Floor(currentFrame) == frames[(int)Math.Floor(currentTexture)] || !spriteSheet[(int)Math.Floor(currentTexture)]))
                animationEnd = true;
            else
                animationEnd = false;

            if (draw && (camera.rectangle.Intersects(new Rectangle(Game1.CeilAdv(position.X), Game1.CeilAdv(position.Y), width, height)) || HUD))
            {
                if (!animated)
                {
                    spriteBatch.Draw(textures[0], position, null, GetColorOpaque(color, opacity), angle, origin, scale * this.scale, spriteEffect, depth);
                }
                else
                {
                    if (spriteSheet[(int)Math.Floor(animation.currentTexture)])
                    {
                        int x = (int)(animation.currentFrame % Math.Floor((float)(textures[(int)Math.Floor(animation.currentTexture)].Width / width))) * width;
                        int y = (int)Math.Floor(animation.currentFrame / (textures[(int)Math.Floor(animation.currentTexture)].Width / width)) * height;
                        spriteBatch.Draw(textures[(int)Math.Floor(animation.currentTexture)], position, new Rectangle(x, y, width, height), GetColorOpaque(color, opacity), angle, origin, scale * this.scale, spriteEffect, depth);
                    }
                    else
                    {
                        spriteBatch.Draw(textures[(int)Math.Floor(animation.currentTexture)], position, null, GetColorOpaque(color, opacity), angle, origin, scale * this.scale, spriteEffect, depth);
                    }
                }

                if (random)
                {
                    if (coolDown > 0)
                        coolDown--;
                    else if (animationEnd)
                    {
                        coolDown = randFrequency + rand.Next(randFrequency * 2) - randFrequency;
                        int total = 0;
                        for (int i = 0; i < odds.Count; i++)
                        {
                            total += odds[i];
                        }
                        int chance = rand.Next(total);
                        total = 0;
                        for (int i = 0; i < odds.Count; i++)
                        {
                            total += odds[i];
                            if (chance < total)
                            {
                                children[i].DrawOnce();
                                draw = false;
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                if (random)
                {
                    for (int i = 0; i < children.Count; i++)
                    {
                        children[i].angle = angle;
                        children[i].origin = origin;
                        children[i].color = color;
                        if (children[i].draw)
                        {
                            children[i].Draw(rand, spriteBatch, camera, position, this.scale * scale, spriteEffect, depth);
                            if (children[i].animationEnd)
                            {
                                draw = true;
                                children[i].draw = false;
                            }
                        }
                    }
                }
            }

            if (animated)
            {
                if (spriteSheet[(int)Math.Floor(animation.currentTexture)])
                {
                    if (animation.currentFrame >= frames[(int)Math.Floor(animation.currentTexture)])
                    {
                        animation.currentFrame = 0;
                        animation.currentTexture = (animation.currentTexture + 1) % (textures.Count);
                    }
                    else
                        animation.currentFrame = (animation.currentFrame + animation.speed);
                }
                else
                    animation.currentTexture = (animation.currentTexture + animation.speed) % (textures.Count);
            }

            if (drawOnce && animationEnd)
                draw = false;
        }

        public void DrawOnce()
        {
            draw = true;
            drawOnce = true;
        }

        static Color GetColorOpaque(Color color, float opacity)
        {
            return Color.Lerp(color, Color.Transparent, 1 - opacity);
        }

        Color GetColorOpaque()
        {
            return Color.Lerp(color, Color.Transparent, 1 - opacity);
        }
    }
}
