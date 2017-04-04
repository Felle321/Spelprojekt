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

namespace Menu
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        KeyboardState keyboard, keyboardPrev;
        static Texture2D pixel;
        Random rand = new Random();
        public static Camera camera = new Camera(new Vector2(0, 0));
        MouseState mouse, mousePrev;
        private int mouseLeftClickTimer = 0;
        bool mouseLeftDoubleClick
        {
            get
            {
                if (mouse.LeftButton == ButtonState.Pressed && mouseLeftClickTimer > 0)
                    return true;
                else
                    return false;
            }
        }

        #region Paths
        public static string LocationRooms
        {
            get
            {
                return World.locationLibrary + @"rooms\";
            }
            set
            {
                return;
            }
        }
        public static string LocationSprites
        {
            get
            {
                return World.locationLibrary + @"sprites\";
            }
            set
            {
                return;
            }
        }
        public static string LocationMenus
        {
            get
            {
                return World.locationLibrary + @"menus\";
            }
        }
        #endregion

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new Color[1] { Color.White });

            SpriteHandler.AddSprite("", @"upgrade/bar_exp", LocationMenus)
            Bar

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {

        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            mouse = Mouse.GetState();
            if (mouse.LeftButton == ButtonState.Pressed)
                mouseLeftClickTimer = 1;

            if (mouseLeftClickTimer > 0 && mouseLeftClickTimer < World.doubleClickLimit)
                mouseLeftClickTimer++;
            else if (mouseLeftClickTimer != 0)
                mouseLeftClickTimer = 0;

            

            mousePrev = Mouse.GetState();
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();



            spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Floors the given number, reverses if negative
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static int FloorAdv(float number)
        {
            if (number < 0)
                return (int)Math.Ceiling(number);
            else
                return (int)Math.Floor(number);
        }

        /// <summary>
        /// Raises the given number to the nearest int, reversed if negative
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static int CeilAdv(float number)
        {
            if (number > 0)
                return (int)Math.Ceiling(number);
            else
                return (int)Math.Floor(number);
        }

        /// <summary>
        /// AngleBetween - the angle between 2 vectors
        /// </summary>
        /// <returns>
        /// Returns the the angle in degrees between vector1 and vector2
        /// </returns>
        /// <param name="vector1"> The first Vector </param>
        /// <param name="vector2"> The second Vector </param>
        public static float GetAngle(Vector2 a, Vector2 b)
        {
            if (a.X == b.X)
            {
                if (a.Y - b.Y < 0)
                    return MathHelper.ToRadians(270);
                else
                    return MathHelper.ToRadians(90);
            }
            else
                return (float)(Math.Atan((a.Y - b.Y) / (b.X - a.X)) + MathHelper.ToRadians(360)) % MathHelper.ToRadians(360);
        }

        public static float GetAngle(Vector2 vector)
        {
            return (GetAngle(vector, Vector2.Zero) + MathHelper.ToRadians(360)) % MathHelper.ToRadians(360);
        }

        public static float GetAngle(Point a, Point b)
        {
            if (a.X == b.X)
            {
                if (a.Y - b.Y < 0)
                    return MathHelper.ToRadians(270);
                else
                    return MathHelper.ToRadians(90);
            }
            else
                return (float)(Math.Atan((a.Y - b.Y) / (b.X - a.X)) + MathHelper.ToRadians(360)) % MathHelper.ToRadians(360);
        }

        /// <summary>
        /// Returns the distance between two Vector2 positions
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static float GetDistance(Vector2 source, Vector2 target)
        {
            //return (float)Math.Sqrt(Math.Pow(Math.Abs(source.X - target.X), 2) + Math.Pow(Math.Abs(source.Y - target.Y), 2));
            return new Vector2(target.X - source.X, target.Y - source.Y).Length();
        }

        /// <summary>
        /// Returns the distance between two Vector2 positions
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static float GetDistance(Point source, Point target)
        {
            return new Vector2(target.X - source.X, target.Y - source.Y).Length();
        }

        /// <summary>
        /// Returns the point in the middle of two vector2 positions
        /// </summary>
        /// <param name="point0"></param>
        /// <param name="point1"></param>
        /// <returns></returns>
        public static Vector2 GetMidpoint(Vector2 point0, Vector2 point1)
        {
            return new Vector2((point0.X + point1.X) / 2, (point0.Y + point1.Y) / 2);
        }

        /// <summary>
        /// Draw a line between two Vector2 positions in a chosen color
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="point0"></param>
        /// <param name="point1"></param>
        /// <param name="color"></param>
        public static void DrawLine(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, Vector2 point0, Vector2 point1, Color color)
        {
            float length = Game1.GetDistance(point0, point1);
            float deg = 0;
            bool possible = true;

            if (point0 == point1)
                possible = false;

            deg = (float)Math.Atan2(point1.Y - point0.Y, point1.X - point0.X);

            if (possible)
            {
                spriteBatch.Draw(pixel, point0, null, color, deg, Vector2.Zero, new Vector2(length, 2), SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws a rectangle with the given color
        /// </summary>
        /// <param name="graphicsDevice"></param>
        /// <param name="spriteBatch"></param>
        /// <param name="rectangle"></param>
        /// <param name="color"></param>
        public static void DrawRectangle(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, Rectangle rectangle, Color color)
        {
            DrawLine(graphicsDevice, spriteBatch, new Vector2(rectangle.X, rectangle.Y), new Vector2(rectangle.X + rectangle.Width, rectangle.Y), color);
            DrawLine(graphicsDevice, spriteBatch, new Vector2(rectangle.X + rectangle.Width, rectangle.Y), new Vector2(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height), color);
            DrawLine(graphicsDevice, spriteBatch, new Vector2(rectangle.X, rectangle.Y + rectangle.Height), new Vector2(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height), color);
            DrawLine(graphicsDevice, spriteBatch, new Vector2(rectangle.X, rectangle.Y), new Vector2(rectangle.X, rectangle.Y + rectangle.Height), color);
        }

        public static float StringToFloat(string text)
        {
            char dec;
            if (text.Contains('.'))
            {
                dec = '.';
            }
            else
                dec = ',';

            string[] array = text.Split(dec);

            if (array.Length < 2)
            {
                return int.Parse(text);
            }
            else return int.Parse(array[0]) + float.Parse(array[1]) / (float)Math.Pow(10, array[1].Length);

        }

        /// <summary>
        /// Loads a sprite from the main sprite folder in the library
        /// </summary>
        /// <param name="name">The name of the sprite</param>
        /// <returns></returns>
        public Sprite LoadSprite(string name)
        {
            return new Sprite(LocationSprites + name, GraphicsDevice);
        }

        /// <summary>
        /// Loads a sprite from a specified path in the sprite folder in the library
        /// </summary>
        /// <param name="subfolderPath">The path of the subfolder the sprite lies within, ends with a backslash</param>
        /// <param name="name">The name of the sprite</param>
        /// <returns></returns>
        public Sprite LoadSprite(string subfolderPath, string name)
        {
            return new Sprite(LocationSprites + subfolderPath + "\\" + name, GraphicsDevice);
        }

        /// <summary>
        /// Loads a sprite from the main sprite folder in the library
        /// </summary>
        /// <param name="name">The name of the sprite</param>
        /// <returns></returns>
        public static Sprite LoadSpriteStatic(string name, GraphicsDevice graphicsDevice)
        {
            return new Sprite(LocationSprites + name, graphicsDevice);
        }

        /// <summary>
        /// Loads a sprite from a specified path in the sprite folder in the library
        /// </summary>
        /// <param name="subfolderPath">The path of the subfolder the sprite lies within, ends with a backslash</param>
        /// <param name="name">The name of the sprite</param>
        /// <returns></returns>
        public static Sprite LoadSpriteStatic(string subfolderPath, string name, GraphicsDevice graphicsDevice)
        {
            return new Sprite(LocationSprites + subfolderPath + "\\" + name, graphicsDevice);
        }
    }
}
