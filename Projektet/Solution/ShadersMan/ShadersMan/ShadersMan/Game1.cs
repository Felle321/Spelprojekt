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

namespace ShadersMan
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        RenderTarget2D rtBase, rtBlurSource, rtBlur, rtBlurTemp, rtPostBlur, rtBlurInfo;
        Effect fx_textureToBlurSource, fx_combineTexturesDepth;
        Camera camera = new Camera(new Vector2(0, 0));
        Random rand = new Random();
        EffectParameter blurStrength, blurDirX, blurDirY, paramBlurSrcWidth, paramBlurSrcHeight, paramBlurSrcMovX, paramBlurSrcMovY, paramBlurSrcK;
        EffectPass passBlurSource;
        public GaussianBlur gaussianBlur = new GaussianBlur();
        private const int BLUR_RADIUS = 7;
        private const float BLUR_AMOUNT = 2.0f;
        public Texture2D cats, ballTexture;
        public MouseState mouse, mousePrev;
        public Ball ball = new Ball();
        public bool useBlur = false;
        int delay = 30;

        #region Paths
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
            mouse = Mouse.GetState();
            mousePrev = Mouse.GetState();
            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreferredBackBufferWidth = World.screenWidth;
            graphics.PreferredBackBufferHeight = World.screenHeight;
            graphics.ApplyChanges();
            
            gaussianBlur = new GaussianBlur(this);
            gaussianBlur.ComputeKernel(BLUR_RADIUS, BLUR_AMOUNT);

            Window.Title = "Rau suger kuk";
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
            fx_textureToBlurSource = Content.Load<Effect>("TextureToBlurSource");
            fx_combineTexturesDepth = Content.Load<Effect>("CombineTexturesDepth");
            cats = Content.Load<Texture2D>("cats");
            ballTexture = Content.Load<Texture2D>("ball");

            paramBlurSrcWidth = fx_textureToBlurSource.Parameters["w"];
            paramBlurSrcHeight = fx_textureToBlurSource.Parameters["h"];
            paramBlurSrcMovX = fx_textureToBlurSource.Parameters["movX"];
            paramBlurSrcMovY = fx_textureToBlurSource.Parameters["movY"];
            paramBlurSrcK = fx_textureToBlurSource.Parameters["k"];
            passBlurSource = fx_textureToBlurSource.CurrentTechnique.Passes[0];

            SpriteHandler.AddSprite("running", @"player\player", "running", GraphicsDevice);
            SpriteHandler.AddSprite("teste", "teste", GraphicsDevice);

            rtBlur = new RenderTarget2D(GraphicsDevice, World.screenWidth / 2, World.screenHeight / 2, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);
            rtBlurTemp = new RenderTarget2D(GraphicsDevice, World.screenWidth / 2, World.screenHeight / 2, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);
            rtBlurSource = new RenderTarget2D(GraphicsDevice, World.screenWidth, World.screenHeight, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);
            rtBase = new RenderTarget2D(GraphicsDevice, World.screenWidth, World.screenHeight, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);
            rtPostBlur = new RenderTarget2D(GraphicsDevice, World.screenWidth, World.screenHeight, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);
            rtBlurInfo = new RenderTarget2D(GraphicsDevice, World.screenWidth, World.screenHeight, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);

            gaussianBlur.ComputeOffsets(World.screenWidth / 2, World.screenHeight / 2);
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            mouse = Mouse.GetState();

            ball.Update(new Vector2(mouse.X, mouse.Y), new Vector2(mousePrev.X, mousePrev.Y));

            if (mouse.LeftButton == ButtonState.Pressed && mousePrev.LeftButton == ButtonState.Released)
                useBlur = !useBlur;

            camera.Update(rand);
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
                camera.Move(new Vector2(4, 0));
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
                camera.Move(new Vector2(-4, 0));
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
                camera.Move(new Vector2(0, -4));
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
                camera.Move(new Vector2(0, 4));
            mousePrev = Mouse.GetState();
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            DrawGame();

            if (useBlur)
            {
                ApplyEffects();
            }

            GraphicsDevice.SetRenderTarget(null);
            //GraphicsDevice.Textures[1] = (Texture2D)rtBase;
            GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, null, null, null, null);
            
            spriteBatch.Draw((Texture2D)rtPostBlur, new Rectangle(0, 0, World.screenWidth, World.screenHeight), Color.White);

            spriteBatch.End();
            base.Draw(gameTime);
        }

        public void DrawGame()
        {
            GraphicsDevice.SetRenderTarget(rtBase);
            //GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, null, null, null, null, camera.get_transformation(GraphicsDevice));

            //SpriteHandler.sprites["fall"].Draw(rand, spriteBatch, camera, new Vector2(0, 0), 1, 0, Vector2.Zero, Color.White, 1f, SpriteEffects.None);

            ////SpriteHandler.sprites["teste"].Draw(rand, spriteBatch, camera, new Vector2(40, 40), 1, 0, Vector2.Zero, Color.White, 1f, SpriteEffects.None);

            //SpriteHandler.sprites["teste"].Draw(rand, spriteBatch, camera, new Vector2(0, 0), 1, 0, Vector2.Zero, Color.White, 1f, SpriteEffects.None);

            //spriteBatch.Draw(cats, new Vector2(-400, -400), Color.White);

            //spriteBatch.Draw(ballTexture, ball.position, Color.White);
            SpriteHandler.Draw("teste", rand, spriteBatch, camera, ball.position, 1f, 0f, Vector2.Zero, Color.White, 1f, SpriteEffects.None);

            spriteBatch.End();
        }

        /// <summary>
        /// Here you draw every object and all of the extra motionblur-sprites
        /// </summary>
        protected void ApplyEffects()
        {
            DrawBlurSource();

            DrawBlur();
        }


        protected void DrawBlurSource()
        {
            GraphicsDevice.SetRenderTarget(rtBlurSource);
            GraphicsDevice.Clear(Color.Transparent);
            fx_textureToBlurSource.CurrentTechnique.Passes[0].Apply();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, fx_textureToBlurSource, camera.get_transformation(GraphicsDevice));

            SpriteHandler.Draw("teste", rand, spriteBatch, camera, ball.position - ball.movement, GetBlurSourceScale(SpriteHandler.sprites["teste"].width, SpriteHandler.sprites["teste"].height, (int)ball.movementPrev.X, (int)ball.movementPrev.Y), 0f, Vector2.Zero, Color.White, 1f, SpriteEffects.None);

            /*
            SpriteHandler.Draw("teste", rand, spriteBatch, camera, ball.position, GetBlurSourceScale(SpriteHandler.sprites["teste"].width, SpriteHandler.sprites["teste"].height, 100, 120), 0f, Vector2.Zero, Color.White, 1f, SpriteEffects.None);
            SpriteHandler.Draw("teste", rand, spriteBatch, camera, ball.position - new Vector2(0, 200), GetBlurSourceScale(SpriteHandler.sprites["teste"].width, SpriteHandler.sprites["teste"].height, 100, -120), 0f, Vector2.Zero, Color.White, 1f, SpriteEffects.None);
            SpriteHandler.Draw("teste", rand, spriteBatch, camera, ball.position - new Vector2(200, 0), GetBlurSourceScale(SpriteHandler.sprites["teste"].width, SpriteHandler.sprites["teste"].height, -100, 120), 0f, Vector2.Zero, Color.White, 1f, SpriteEffects.None);
            SpriteHandler.Draw("teste", rand, spriteBatch, camera, ball.position - new Vector2(200, 200), GetBlurSourceScale(SpriteHandler.sprites["teste"].width, SpriteHandler.sprites["teste"].height, -100, -120), 0f, Vector2.Zero, Color.White, 1f, SpriteEffects.None);
            */
            //Draw a blur source
            //SpriteHandler.Draw("teste", rand, spriteBatch, camera, ball.position, GetBlurSourceScale(SpriteHandler.sprites["teste"].width, SpriteHandler.sprites["teste"].height, 50, 120), 0f, Vector2.Zero, GetBlurSourceParams(SpriteHandler.sprites["teste"].width, SpriteHandler.sprites["teste"].height, 50, 120), 1f, SpriteEffects.None);
            //SpriteHandler.Draw("teste", rand, spriteBatch, camera, ball.position + new Vector2(150 - SpriteHandler.sprites["teste"].width, -120 + SpriteHandler.sprites["teste"].height), GetBlurSourceScale(SpriteHandler.sprites["teste"].width, SpriteHandler.sprites["teste"].height, 150, -120), 0f, Vector2.Zero, GetBlurSourceParams(SpriteHandler.sprites["teste"].width, SpriteHandler.sprites["teste"].height, 150, 120), 1f, SpriteEffects.None);
            //SpriteHandler.Draw("teste", rand, spriteBatch, camera, ball.position + new Vector2(-150 + SpriteHandler.sprites["teste"].width, -120 + SpriteHandler.sprites["teste"].height), GetBlurSourceScale(SpriteHandler.sprites["teste"].width, SpriteHandler.sprites["teste"].height, -150, -120), 0f, Vector2.Zero, GetBlurSourceParams(SpriteHandler.sprites["teste"].width, SpriteHandler.sprites["teste"].height, 150, 120), 1f, SpriteEffects.None);
            //SpriteHandler.Draw("teste", rand, spriteBatch, camera, ball.position + new Vector2(-150 + SpriteHandler.sprites["teste"].width, 120 - SpriteHandler.sprites["teste"].height), GetBlurSourceScale(SpriteHandler.sprites["teste"].width, SpriteHandler.sprites["teste"].height, -150, 120), 0f, Vector2.Zero, GetBlurSourceParams(SpriteHandler.sprites["teste"].width, SpriteHandler.sprites["teste"].height, 150, 120), 1f, SpriteEffects.None);

            spriteBatch.End();
        }

        public Vector2 GetBlurSourceScale(int width, int height, int movX, int movY)
        {
            paramBlurSrcMovX.SetValue((float)(movX / (float)(width + Math.Abs(movX))));
            paramBlurSrcMovY.SetValue((float)(movY / (float)(height + Math.Abs(movY))));
            float k = (float)((movY / (float)(height + Math.Abs(movY))) / (movX / (float)(width + Math.Abs(movX))));
            paramBlurSrcK.SetValue((float)((movY / (float)(height + Math.Abs(movY))) / (movX / (float)(width + Math.Abs(movX)))));

            passBlurSource.Apply();

            return new Vector2(1 / (float)(width / (float)(width + Math.Abs(movX))), 1 / (float)(height / (float)(height + Math.Abs(movY))));
        }

        /// <summary>
        /// Sets the parameters for the effect to indicate strength and direction
        /// </summary>
        /// <param name="strength">The amount of blur (0 - 1)</param>
        /// <param name="direction">A direction of the effect in radians</param>
        /// <param name="opacity">The opacity of the effect 0-1</param>
        public Color GetBlurInfo(float strength, float direction, float opacity)
        {
            if (strength > 1)
                strength = 1;
            Color clr = new Color((float)Math.Abs(Math.Cos(direction)), (float)Math.Abs(Math.Sin(direction)), strength * opacity, 0);
            return clr;
        }

        /// <summary>
        /// Sets the parameters for the effect to indicate strength and direction
        /// </summary>
        /// <param name="strength">The amount of blur (0 - 1)</param>
        /// <param name="direction">A direction of the effect</param>
        /// <param name="opacity">The opacity of the effect 0-1</param>
        public Color GetBlurInfo(float strength, Vector2 direction, float opacity)
        {
            if (strength > 1)
                strength = 1;
            return new Color((float)Math.Abs(direction.ToDirection().X), (float)Math.Abs(direction.ToDirection().Y), strength * opacity, 0);
        }
        
        protected void DrawBlur()
        {
            Texture2D blurredTexture = gaussianBlur.PerformGaussianBlur((Texture2D)rtBlurSource, rtBlurInfo, rtBlur, rtBlurTemp, spriteBatch, GraphicsDevice);
            Rectangle rectangle = new Rectangle(0, 0, rtBase.Width, rtBase.Height);
            GraphicsDevice.SetRenderTarget(rtPostBlur);
            GraphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            spriteBatch.Draw(blurredTexture, rectangle, Color.White);
            spriteBatch.End();
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
        /// Floors the given number, reverses if negative
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

        public static Vector2 GetVector2(float angle)
        {
            return new Vector2((float)Math.Cos(angle), -(float)Math.Sin(angle));
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
