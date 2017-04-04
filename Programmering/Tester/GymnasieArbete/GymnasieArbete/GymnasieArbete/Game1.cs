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
    public class Game1 : Microsoft.Xna.Framework.Game
    {

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Random rand = new Random();
        public static Camera camera = new Camera(new Vector2(0, 0));
        List<RoomReference> rooms = new List<RoomReference>();
        List<Room> activeRooms = new List<Room>();
        List<Room> baseRooms = new List<Room>();
        Texture2D sky;
        KeyboardState keyboard, keyboardPrev;
        Player player;
        public static SpriteFont fontDebug;
        public static Sprite spr_debug_test, spr_debug_player, spr_debug_shot;
        RenderTarget2D fogTarget;
        Color fogColor = new Color(100, 100, 100);
        static Texture2D pixel;
        public int level = 0;
        public int delay = 0;
        public int delayTime = 0;
        public static bool paused = false;
        public int[,] tiles = new int[World.levelSize, World.levelSize];
        //Analyze
        float[] cameraSpeed = new float[5];
        float[] cameraTorque = new float[5];
        float[] cameraTorqueMaxSpeed = new float[5];
        float[] cameraSlackFactor = new float[5];

        int cameraSpeedChoice = 2;
        int cameraTorqueChoice = 2;
        int cameraTorqueMaxSpeedChoice = 2;
        int cameraSlackFactorChoice = 2;

        public Dictionary<string, Weapon> Weapons;
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
        #endregion


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = World.screenWidth;
            graphics.PreferredBackBufferHeight = World.screenHeight;
        }

        protected override void Initialize()
        {
            base.Initialize();

            //ANALYZE
            for (int i = 0; i < 5; i++)
            {
                cameraSlackFactor[i] = World.cameraSlackFactor * (i + .5f) * .33f;
            }
            for (int i = 0; i < 5; i++)
            {
                cameraSpeed[i] = World.cameraSpeed * i * (i + .5f) * .33f;
            }
            for (int i = 0; i < 5; i++)
            {
                cameraTorque[i] = World.cameraTorque * (i + .5f) * .33f;
            }
            for (int i = 0; i < 5; i++)
            {
                cameraTorqueMaxSpeed[i] = World.cameraTorqueMaxSpeed * (i + 1) * .33f;
            }
        }


        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            fogTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            LoadGame(Content);
            Particle.pixel = new Texture2D(GraphicsDevice, 1, 1);
            Particle.pixel.SetData(new Color[1] { Color.White });
            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new Color[1] { Color.White });
            //Sprites
            #region Player
            SpriteHandler.AddSprite("player_idle", @"player\player", "idle", GraphicsDevice);
            SpriteHandler.AddSprite("player_running", @"player\player", "running", GraphicsDevice);
            SpriteHandler.AddSprite("player_dash", @"player\player", "dash", GraphicsDevice);
            SpriteHandler.AddSprite("player_stall", @"player\player\jump", "stall", GraphicsDevice);
            SpriteHandler.AddSprite("player_jump", @"player\player\jump", "jump", GraphicsDevice);
            SpriteHandler.AddSprite("player_fall", @"player\player\jump", "fall", GraphicsDevice);
            SpriteHandler.AddSprite("player_back", @"player\player", "back", GraphicsDevice);
            spr_debug_shot = LoadSprite(@"player\weapons\test", "shotSprite");
            SpriteHandler.AddSprite("particle_smoke", "particles", "smoke", GraphicsDevice);
            Particle.pixel = new Texture2D(GraphicsDevice, 1, 1);
            Particle.pixel.SetData(new Color[1] { Color.White });
            SpriteHandler.AddSprite("debug_test", "teste", GraphicsDevice);
            SpriteHandler.sprites["debug_test"].HUD = true;
            SpriteHandler.AddSprite("weapon_test_shot", @"player\weapons\test", "shotSprite", GraphicsDevice);
            #endregion

            #region EnergyBlaster Sprites
            SpriteHandler.AddSprite("Energy_lIdle", @"player\weapons\EnergyBlaster", "LeftIdle", GraphicsDevice);
            SpriteHandler.AddSprite("Energy_lRunning", @"player\weapons\EnergyBlaster", "LeftRunning", GraphicsDevice);
            SpriteHandler.AddSprite("Energy_lShoot1_1", @"player\weapons\EnergyBlaster", "LeftShoot1_1", GraphicsDevice);
            SpriteHandler.AddSprite("Energy_lShoot2_1", @"player\weapons\EnergyBlaster", "LeftShoot2_1", GraphicsDevice);
            SpriteHandler.AddSprite("Energy_lJump", @"player\weapons\EnergyBlaster\LeftAir", "jump", GraphicsDevice);
            SpriteHandler.AddSprite("Energy_lStall", @"player\weapons\EnergyBlaster\LeftAir", "stall", GraphicsDevice);
            SpriteHandler.AddSprite("Energy_lFall", @"player\weapons\EnergyBlaster\LeftAir", "fall", GraphicsDevice);
            SpriteHandler.AddSprite("Energy_rShoot1_1", @"player\weapons\EnergyBlaster", "RightShoot1_1", GraphicsDevice);
            SpriteHandler.AddSprite("Energy_rShoot2_1", @"player\weapons\EnergyBlaster", "RightShoot2_1", GraphicsDevice);
            SpriteHandler.AddSprite("Energy_rIdle", @"player\weapons\EnergyBlaster", "RightIdle", GraphicsDevice);
            SpriteHandler.AddSprite("Energy_rRunning", @"player\weapons\EnergyBlaster", "RightRunning", GraphicsDevice);
            SpriteHandler.AddSprite("Energy_rJump", @"player\weapons\EnergyBlaster\RightAir", "jump", GraphicsDevice);
            SpriteHandler.AddSprite("Energy_rStall", @"player\weapons\EnergyBlaster\RightAir", "stall", GraphicsDevice);
            SpriteHandler.AddSprite("Energy_rFall", @"player\weapons\EnergyBlaster\RightAir", "fall", GraphicsDevice);
            SpriteHandler.AddSprite("Energy_Attack1_1", @"player\weapons\EnergyBlaster\Particles", "Attack1_1", GraphicsDevice);
            SpriteHandler.AddSprite("Energy_Attack2_1Start", @"player\weapons\EnergyBlaster\Particles\Attack2_1", "WaveStart", GraphicsDevice);
            SpriteHandler.AddSprite("Energy_Attack2_1Live", @"player\weapons\EnergyBlaster\Particles\Attack2_1", "WaveLive", GraphicsDevice);
            SpriteHandler.AddSprite("Energy_Attack2_1End", @"player\weapons\EnergyBlaster\Particles\Attack2_1", "WaveEnd", GraphicsDevice);
            #endregion

            #region RedBlaster
            SpriteHandler.AddSprite("Red_lIdle", @"player\weapons\RedBlaster", "LeftIdle", GraphicsDevice);
            SpriteHandler.AddSprite("Red_lRunning", @"player\weapons\RedBlaster", "LeftRunning", GraphicsDevice);
            SpriteHandler.AddSprite("Red_lShoot1_1", @"player\weapons\RedBlaster", "LeftShoot1_1", GraphicsDevice);
            SpriteHandler.AddSprite("Red_lJump", @"player\weapons\RedBlaster\LeftAir", "jump", GraphicsDevice);
            SpriteHandler.AddSprite("Red_lStall", @"player\weapons\RedBlaster\LeftAir", "stall", GraphicsDevice);
            SpriteHandler.AddSprite("Red_lFall", @"player\weapons\RedBlaster\LeftAir", "fall", GraphicsDevice);
            SpriteHandler.AddSprite("Red_rShoot1_1", @"player\weapons\RedBlaster", "RightShoot1_1", GraphicsDevice);
            SpriteHandler.AddSprite("Red_rIdle", @"player\weapons\RedBlaster", "RightIdle", GraphicsDevice);
            SpriteHandler.AddSprite("Red_rRunning", @"player\weapons\RedBlaster", "RightRunning", GraphicsDevice);
            SpriteHandler.AddSprite("Red_rJump", @"player\weapons\RedBlaster\RightAir", "jump", GraphicsDevice);
            SpriteHandler.AddSprite("Red_rStall", @"player\weapons\RedBlaster\RightAir", "stall", GraphicsDevice);
            SpriteHandler.AddSprite("Red_rFall", @"player\weapons\RedBlaster\RightAir", "fall", GraphicsDevice);
            SpriteHandler.AddSprite("Red_Attack1_1", @"player\weapons\RedBlaster", "Attack1_1", GraphicsDevice);
            SpriteHandler.AddSprite("Red_Attack3_1", @"player\weapons\RedBlaster", "Attack3_1", GraphicsDevice);
            SpriteHandler.AddSprite("Red_SmallParticle", @"player\weapons\RedBlaster\Particles", "smallParticle", GraphicsDevice);
            SpriteHandler.AddSprite("Red_SmallerParticle", @"player\weapons\RedBlaster\Particles", "smallerParticle", GraphicsDevice);



            #endregion

            #region BAMF

            SpriteHandler.AddSprite("BAMF_lIdle", @"player\weapons\BAMF", "LeftIdle", GraphicsDevice);
            SpriteHandler.AddSprite("BAMF_lRunning", @"player\weapons\BAMF", "LeftRunning", GraphicsDevice);
            SpriteHandler.AddSprite("BAMF_lShoot1_1", @"player\weapons\BAMF", "LeftShoot1_1", GraphicsDevice);
            SpriteHandler.AddSprite("BAMF_lShoot2_1", @"player\weapons\BAMF", "LeftShoot2_1", GraphicsDevice);
            SpriteHandler.AddSprite("BAMF_lShoot3_1", @"player\weapons\BAMF", "LeftShoot3_1", GraphicsDevice);
            SpriteHandler.AddSprite("BAMF_lJump", @"player\weapons\BAMF\LeftAir", "jump", GraphicsDevice);
            SpriteHandler.AddSprite("BAMF_lStall", @"player\weapons\BAMF\LeftAir", "stall", GraphicsDevice);
            SpriteHandler.AddSprite("BAMF_lFall", @"player\weapons\BAMF\LeftAir", "fall", GraphicsDevice);
            SpriteHandler.AddSprite("BAMF_rShoot1_1", @"player\weapons\BAMF", "RightShoot1_1", GraphicsDevice);
            SpriteHandler.AddSprite("BAMF_rShoot2_1", @"player\weapons\BAMF", "RightShoot2_1", GraphicsDevice);
            SpriteHandler.AddSprite("BAMF_rShoot3_1", @"player\weapons\BAMF", "RightShoot3_1", GraphicsDevice);
            SpriteHandler.AddSprite("BAMF_rIdle", @"player\weapons\BAMF", "RightIdle", GraphicsDevice);
            SpriteHandler.AddSprite("BAMF_rRunning", @"player\weapons\BAMF", "RightRunning", GraphicsDevice);
            SpriteHandler.AddSprite("BAMF_rJump", @"player\weapons\BAMF\RightAir", "jump", GraphicsDevice);
            SpriteHandler.AddSprite("BAMF_rStall", @"player\weapons\BAMF\RightAir", "stall", GraphicsDevice);
            SpriteHandler.AddSprite("BAMF_rFall", @"player\weapons\BAMF\RightAir", "fall", GraphicsDevice);
            SpriteHandler.AddSprite("BAMF_Attack1_1", @"player\weapons\BAMF", "Attack1_1", GraphicsDevice);
            SpriteHandler.AddSprite("BAMF_Attack2_1", @"player\weapons\BAMF", "Attack2_1", GraphicsDevice);
            SpriteHandler.AddSprite("BAMF_Attack3_1", @"player\weapons\BAMF", "Attack3_1", GraphicsDevice);
            SpriteHandler.AddSprite("BAMF_Particle", @"player\weapons\BAMF\BAMFParticles", "Particle", GraphicsDevice);


            #endregion

            #region GroundTroop
            SpriteHandler.AddSprite("GroundTroop_Idle", @"enemies\GroundTroop", "Idle", GraphicsDevice);
            SpriteHandler.AddSprite("GroundTroop_Walking", @"enemies\GroundTroop", "Walking", GraphicsDevice);
            SpriteHandler.AddSprite("GroundTroop_Jump", @"enemies\GroundTroop", "Jump", GraphicsDevice);
            SpriteHandler.AddSprite("GroundTroop_Fall", @"enemies\GroundTroop", "Fall", GraphicsDevice);
            SpriteHandler.AddSprite("GroundTroop_ChargedBullet", @"enemies\GroundTroop", "ChargedBullet", GraphicsDevice);
            SpriteHandler.AddSprite("GroundTroop_Charging", @"enemies\GroundTroop", "Charging", GraphicsDevice);
            SpriteHandler.AddSprite("GroundTroop_Shooting", @"enemies\GroundTroop", "Shooting", GraphicsDevice);
            SpriteHandler.AddSprite("GroundTroop_Bullet", @"enemies\GroundTroop", "Bullet", GraphicsDevice);
            #endregion
            
            fontDebug = Content.Load<SpriteFont>("fontDebug");
            Weapons = new Dictionary<string, Weapon>();
            Weapons.Add("EnergyBlaster", new Weapon.EnergyBlaster(GraphicsDevice));
            Weapons.Add("EnergyBlaster2", new Weapon.EnergyBlaster(GraphicsDevice));
            Weapons.Add("BAMF", new Weapon.BAMF(GraphicsDevice));
            Weapons.Add("RedBlaster", new Weapon.RedBlaster(GraphicsDevice));
            SpriteHandler.AddSprite("gradient", "gradient", GraphicsDevice);
            SpriteHandler.AddSprite("scrap", "scrap", GraphicsDevice);
            SpriteHandler.AddSprite("particle_fire", "particles", "fire", GraphicsDevice);
            //MAIN FONTS
            World.fontDamage = Content.Load<SpriteFont>("fontDamage");


            LoadLevel();

            activeRooms.Add(new Room(0, rand, false, 0f, rooms[0], baseRooms[rooms[0].reference]));

            player = new Player(new Rectangle(100, 100, World.playerWidth, World.playerHeight), 0, activeRooms[0]);
            player.room = tiles[World.levelSize / 2, World.levelSize / 2];

            player.leftHand = new Weapon.BAMF(GraphicsDevice);
            player.rightHand = new Weapon.RedBlaster(GraphicsDevice);

            UpdateActiveRooms();
        }


        protected override void UnloadContent()
        {

        }


        protected override void Update(GameTime gameTime)
        {
            keyboard = Keyboard.GetState();

            //ANALYZE
            if (keyboard.IsKeyDown(Keys.Y) && keyboardPrev.IsKeyUp(Keys.Y) && cameraSpeedChoice < 4)
                cameraSpeedChoice++;
            else if (keyboard.IsKeyDown(Keys.H) && keyboardPrev.IsKeyUp(Keys.H) && cameraSpeedChoice > 0)
                cameraSpeedChoice--;

            if (keyboard.IsKeyDown(Keys.U) && keyboardPrev.IsKeyUp(Keys.U) && cameraTorqueChoice < 4)
                cameraTorqueChoice++;
            else if (keyboard.IsKeyDown(Keys.J) && keyboardPrev.IsKeyUp(Keys.J) && cameraTorqueChoice > 0)
                cameraTorqueChoice--;

            if (keyboard.IsKeyDown(Keys.I) && keyboardPrev.IsKeyUp(Keys.I) && cameraTorqueMaxSpeedChoice < 4)
                cameraTorqueMaxSpeedChoice++;
            else if (keyboard.IsKeyDown(Keys.K) && keyboardPrev.IsKeyUp(Keys.K) && cameraTorqueMaxSpeedChoice > 0)
                cameraTorqueMaxSpeedChoice--;

            if (keyboard.IsKeyDown(Keys.O) && keyboardPrev.IsKeyUp(Keys.O) && cameraSlackFactorChoice < 4)
                cameraSlackFactorChoice++;
            else if (keyboard.IsKeyDown(Keys.L) && keyboardPrev.IsKeyUp(Keys.L) && cameraSlackFactorChoice > 0)
                cameraSlackFactorChoice--;

            World.cameraSpeed = cameraSpeed[cameraSpeedChoice];
            World.cameraTorque = cameraTorque[cameraTorqueChoice];
            World.cameraTorqueMaxSpeed = cameraTorqueMaxSpeed[cameraTorqueMaxSpeedChoice];
            World.cameraSlackFactor = cameraSlackFactor[cameraSlackFactorChoice];

            if (delay == 0)
            {
                if (keyboard.IsKeyDown(Keys.Escape) && keyboardPrev.IsKeyUp(Keys.Escape))
                {
                    paused = !paused;
                }
                if (!paused)
                {
                    UpdateActiveRooms();

                    //Room specific
                    for (int i = 0; i < activeRooms.Count; i++)
                    {
                        activeRooms[i].Update(rand, player);
                    }

                    player.leftHand.Update(player, activeRooms[0], true);
                    player.rightHand.Update(player, activeRooms[0], false);

                    player.Update(activeRooms[0], rand);

                    if (keyboard.IsKeyDown(Keys.M))
                        activeRooms[0].gameObjects.Add(new Particle.Dynamic(new Rectangle((int)player.Origin.X, (int)player.Origin.Y, 5, 5), 0, activeRooms[0], new Vector2(rand.Next(-10, 10), rand.Next(-10, -2)), Vector2.Zero, "", true, Color.Green, 2f, rand.Next(200, 500), false, false));

                    if (player.onGround && !player.onGroundPrev)
                    {
                        camera.AddShake(new Vector2(player.movementPrev.Y / 2, (player.movementPrev.Y) / 4), 5);
                    }

                    delay = delayTime;

                    camera.Target = player.Origin;
                    camera.Update(rand);
                }


                DoorInteractionCheck();

                keyboardPrev = Keyboard.GetState();
                base.Update(gameTime);
            }
            else
                delay--;
        }
        

        protected override void Draw(GameTime gameTime)
        {
            //Initialize the fog and lightsources
            InitializeLights(gameTime);

            //Reset rendertarget
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            //Draws the interactive parts of the game
            DrawGame(gameTime);
            //DrawDebugTiles(spriteBatch);

            //Draws the fog and lightsources
            DrawDarkness(gameTime);

            //Draw HUD
            DrawHUD(gameTime, player);

            base.Draw(gameTime);
        }

        protected void DrawDebugTiles(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            for (int i = 0; i < tiles.GetLength(0); i++)
            {
                for (int j = 0; j < tiles.GetLength(1); j++)
                {
                    if(tiles[i,j] != -1)
                        spriteBatch.Draw(pixel, new Rectangle(250 + i * 4, j * 4, 4, 4), Color.Black);
                    else
                        spriteBatch.Draw(pixel, new Rectangle(250 + i * 4, j * 4, 4, 4), Color.White);
                }
            }
            spriteBatch.End();
        }

        /// <summary>
        /// Draws everything that is affected by the camera, light, etc.
        /// </summary>
        /// <param name="gameTime"></param>
        public void DrawGame(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, null, null, null, camera.get_transformation(GraphicsDevice));
            
            for (int i = 0; i < activeRooms.Count; i++)
            {
                activeRooms[i].Draw(new Point(1024 * (activeRooms[i].position.X - rooms[player.room].position.X), 1024 * (activeRooms[i].position.Y - rooms[player.room].position.Y)), rand, spriteBatch, GraphicsDevice, camera);
                activeRooms[i].DrawCollisions(new Point(1024* (activeRooms[i].position.X - rooms[player.room].position.X), 1024 * (activeRooms[i].position.Y - rooms[player.room].position.Y)), GraphicsDevice, spriteBatch);
            }
            
            for (int i = 0; i < activeRooms[0].doors.Count; i++)
            {
                DrawRectangle(GraphicsDevice, spriteBatch, activeRooms[0].doors[i].rectangle, Color.Violet);
            }

            player.Draw(Point.Zero, rand, spriteBatch, GraphicsDevice, camera);
            
            spriteBatch.End();
        }

        /// <summary>
        /// Draws everything not affected by the camera, light, etc.
        /// </summary>
        /// <param name="gameTime"></param>
        public void DrawHUD(GameTime gameTime, Player player)
        {
            spriteBatch.Begin();
            //ANALYZE            
            spriteBatch.DrawString(fontDebug, cameraSpeedChoice.ToString(), new Vector2(100, 140), Color.Red);
            spriteBatch.DrawString(fontDebug, cameraTorqueChoice.ToString(), new Vector2(100, 180), Color.Red);
            spriteBatch.DrawString(fontDebug, cameraTorqueMaxSpeedChoice.ToString(), new Vector2(100, 220), Color.Red);
            spriteBatch.DrawString(fontDebug, cameraSlackFactorChoice.ToString(), new Vector2(100, 260), Color.Red);

            //TEMP
            //DrawETEA(gameTime);
            //SpriteHandler.Draw("debug_test", rand, spriteBatch, camera, new Vector2(500, 200), SpriteEffects.None, 0.5f);
            #region Debug
            /*
            spriteBatch.DrawString(fontDebug, player.room.ToString(), new Vector2(100, 40), Color.Red);
            spriteBatch.DrawString(fontDebug, player.hp.ToString(), new Vector2(100, 80), Color.Red);
            spriteBatch.DrawString(fontDebug, player.onGround.ToString(), new Vector2(100, 100), Color.Red);
            spriteBatch.DrawString(fontDebug, player.movement.X.ToString() + "_" + player.movement.Y.ToString(), new Vector2(100, 140), Color.Red);
            spriteBatch.DrawString(fontDebug, player.position.X.ToString() + "_" + player.position.Y.ToString(), new Vector2(100, 180), Color.Red);
            spriteBatch.DrawString(fontDebug, player.Origin.X.ToString() + "_" + player.Origin.Y.ToString(), new Vector2(100, 220), Color.Red);
            spriteBatch.DrawString(fontDebug, camera.pos.X.ToString() + "_" + camera.pos.Y.ToString(), new Vector2(100, 260), Color.Red);
            spriteBatch.DrawString(fontDebug, (player.position.X - player.positionPrev.X).ToString() + "_" + (player.position.Y - player.positionPrev.Y).ToString(), new Vector2(100, 300), Color.Red);
            spriteBatch.DrawString(fontDebug, "FPS: " + (1f / (float)gameTime.ElapsedGameTime.TotalSeconds).ToString(), new Vector2(20, 30), Color.Red);
            spriteBatch.DrawString(fontDebug, "GameObjects. PlayerRoome: " + activeRooms[0].gameObjects.Count.ToString(), new Vector2(400, 50), Color.Red);
            for (int i = 0; i < activeRooms.Count; i++)
            {
                activeRooms[i].DrawDebug(GraphicsDevice, spriteBatch);
            }
            */
            #endregion
            spriteBatch.End();
        }

        /// <summary>
        /// Initializes and "predraws" the lightsources. Here you "draw" all the lightsources
        /// </summary>
        /// <param name="gameTime"></param>
        public void InitializeLights(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(fogTarget);
            GraphicsDevice.Clear(fogColor);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, camera.get_transformation(GraphicsDevice));
            for (int i = 0; i < activeRooms.Count; i++)
            {
                activeRooms[i].DrawLights(new Point(activeRooms[i].position.X - rooms[player.room].position.X, activeRooms[i].position.Y - rooms[player.room].position.Y), rand, spriteBatch, camera);
            }
            spriteBatch.End();
        }

        /// <summary>
        /// Actually draws the lightsources and "fog". Don't touch this Rau
        /// </summary>
        /// <param name="gameTime"></param>
        public void DrawDarkness(GameTime gameTime)
        {
            BlendState blendState = new BlendState();
            blendState.AlphaDestinationBlend = Blend.SourceColor;
            blendState.ColorDestinationBlend = Blend.SourceColor;
            blendState.AlphaSourceBlend = Blend.Zero;
            blendState.ColorSourceBlend = Blend.Zero;

            spriteBatch.Begin(SpriteSortMode.Deferred, blendState, null, null, null);
            spriteBatch.Draw(fogTarget, Vector2.Zero, Color.White);
            spriteBatch.End();
        }


        /*
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            keyboard = Keyboard.GetState();

            if (keyboard.IsKeyDown(Keys.H))
                camera.zoom *= .9f;
            else if (keyboard.IsKeyDown(Keys.J))
                camera.zoom *= 1.1f;

            if (keyboard.IsKeyDown(Keys.Right))
                camera.pos.X += 5 / camera.zoom;
            else if (keyboard.IsKeyDown(Keys.Left))
                camera.pos.X -= 5 / camera.zoom;

            if (keyboard.IsKeyDown(Keys.Down))
                camera.pos.Y += 5 / camera.zoom;
            else if (keyboard.IsKeyDown(Keys.Up))
                camera.pos.Y -= 5 / camera.zoom;
        }
        */
        
        protected void DrawETEA(GameTime gameTime)
        {
            //base.Draw(gameTime);

            //spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, null, null, null, camera.get_transformation(GraphicsDevice));
            for (int i = 0; i < World.levelSize; i++)
            {
                for (int j = 0; j < World.levelSize; j++)
                {
                    if (tiles[i, j] != -1)
                        spriteBatch.Draw(pixel, new Rectangle(i, j, 1, 1), Color.Black);
                    else
                        spriteBatch.Draw(pixel, new Rectangle(i, j, 1, 1), Color.White);
                }
            }
            //spriteBatch.End();
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

        /// <summary>
        /// Draw a circle with the given position, radius and color
        /// </summary>
        /// <param name="graphicsDevice"></param>
        /// <param name="spriteBatch"></param>
        /// <param name="position"></param>
        /// <param name="radius"></param>
        /// <param name="color"></param>
        public static void DrawCircle(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, Vector2 position, float radius, Color color)
        {
            Texture2D texture = new Texture2D(graphicsDevice, (int)radius * 2 + 2, (int)radius * 2 + 2);
            Color[] data = new Color[(int)Math.Pow(radius * 2 + 2, 2)];
            Color[,] tempData = new Color[(int)radius * 2 + 2, (int)radius * 2 + 2];

            for (int x = 0; x < (int)radius * 2 + 2; x++)
            {
                for (int y = 0; y < (int)radius * 2 + 2; y++)
                {
                    for (int i = 0; i < 360; i++)
                    {
                        float marginX = (float)Math.Abs((Math.Cos(i) * radius + radius + 1) - x);
                        float marginY = (float)Math.Abs((Math.Sin(i) * radius + radius + 1) - y);
                        if (marginX < 0.8 && marginY < 0.8)
                        {
                            tempData[x, y] = new Color(color.R, color.G, color.B, (int)((marginX + marginY) * 255 / 1.6f));
                        }
                    }
                }
            }

            for (int i = 0; i < tempData.GetLength(0); i++)
            {
                for (int j = 0; j < tempData.GetLength(1); j++)
                {
                    data[i * tempData.GetLength(0) + j] = tempData[i, j];
                }
            }
            texture.SetData(data);

            spriteBatch.Draw(texture, position - new Vector2(radius, radius), color);
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
        /// With the corners defined by the points. The order of the points does not affect the output.
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <returns></returns>
        public static Rectangle RectangleCreate(Point p0, Point p1)
        {
            return new Rectangle(Math.Min(p0.X, p1.X), Math.Min(p0.Y, p1.Y), Math.Abs(p0.X - p1.X), Math.Abs(p0.Y - p1.Y));
        }

        /// <summary>
        /// Returns the dot product of two 2D Vectors
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <returns></returns>
        public static float DotProduct(Vector2 v0, Vector2 v1)
        {
            return (v0.X * v1.X) + (v0.Y * v1.Y);
        }

        /// <summary>
        /// Returns the normalvector of a line
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <returns></returns>
        public static Vector2 GetNormalVector(Vector2 v0, Vector2 v1)
        {
            return new Vector2((float)(Math.Cos(GetAngle(v0, v1) * (v1.X - v0.X) - Math.Sin(GetAngle(v0, v1) * (v1.Y - v0.Y)))), (float)(Math.Sin(GetAngle(v0, v1) * (v1.X - v0.X) + Math.Cos(GetAngle(v0, v1) * (v1.Y - v0.Y)))));
        }

        /// <summary>
        /// Returns the normalvector of a Vector2d
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector2 GetNormalVector(Vector2 vector)
        {
            //return new Vector2((float)(Math.Cos(GetAngle(vector) * vector.X - Math.Sin(GetAngle(vector) * (vector.Y)))), (float)(Math.Sin(GetAngle(vector) * (vector.X) + Math.Cos(GetAngle(vector) * (vector.Y)))));
            return new Vector2((float)Math.Cos(GetAngle(vector)), (float)Math.Sin(GetAngle(vector)));
        }

        /// <summary>
        /// Checks if two lines intersects
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static bool LineIntersectsRect(Vector2 p1, Vector2 p2, Rectangle r)
        {
            return LineIntersectsLine(p1, p2, new Vector2(r.X, r.Y), new Vector2(r.X + r.Width, r.Y)) ||
                   LineIntersectsLine(p1, p2, new Vector2(r.X + r.Width, r.Y), new Vector2(r.X + r.Width, r.Y + r.Height)) ||
                   LineIntersectsLine(p1, p2, new Vector2(r.X + r.Width, r.Y + r.Height), new Vector2(r.X, r.Y + r.Height)) ||
                   LineIntersectsLine(p1, p2, new Vector2(r.X, r.Y + r.Height), new Vector2(r.X, r.Y)) ||
                   (r.Contains(new Point((int)p1.X, (int)p1.Y)) && r.Contains(new Point((int)p2.X, (int)p2.Y)));
        }

        /// <summary>
        /// Checks if a line intersects a rectangle
        /// </summary>
        /// <param name="l1p1"></param>
        /// <param name="l1p2"></param>
        /// <param name="l2p1"></param>
        /// <param name="l2p2"></param>
        /// <returns></returns>
        public static bool LineIntersectsLine(Vector2 l1p1, Vector2 l1p2, Vector2 l2p1, Vector2 l2p2)
        {
            float q = (l1p1.Y - l2p1.Y) * (l2p2.X - l2p1.X) - (l1p1.X - l2p1.X) * (l2p2.Y - l2p1.Y);
            float d = (l1p2.X - l1p1.X) * (l2p2.Y - l2p1.Y) - (l1p2.Y - l1p1.Y) * (l2p2.X - l2p1.X);

            if (d == 0)
            {
                return false;
            }

            float r = q / d;

            q = (l1p1.Y - l2p1.Y) * (l1p2.X - l1p1.X) - (l1p1.X - l2p1.X) * (l1p2.Y - l1p1.Y);
            float s = q / d;

            if (r < 0 || r > 1 || s < 0 || s > 1)
            {
                return false;
            }

            return true;
        }

        public void ScanImageRectangles(Point offset, Texture2D image, out List<Rectangle> rectanglesBasic, out List<Rectangle> rectanglesJump, out List<Slope> slopes)
        {
            List<Rectangle> returnBasic = new List<Rectangle>();
            List<Rectangle> returnJump = new List<Rectangle>();
            List<Rectangle> returnSlopeRight = new List<Rectangle>();
            List<Rectangle> returnSlopeLeft = new List<Rectangle>();

            //Gets a single-dimension array of every pixel in a texture represented with a color
            Color[,] data = new Color[image.Width, image.Height];
            Color[] tempData = new Color[image.Width * image.Height];
            image.GetData(tempData);
            //Converts the data to a two-dimensional array
            for (int i = 0; i < image.Height; i++)
            {
                for (int j = 0; j < image.Width; j++)
                {
                    data[j, i] = tempData[i * image.Width + j];
                }
            }

            Rectangle rect = new Rectangle(0, 0, 0, 0);
            //Scans the array for rectangles in a given color
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    if (ScanColorRectangle(data, i, j, World.colorBasicCollision, offset, returnBasic, out rect))
                    {
                        returnBasic.Add(rect);
                    }
                    if (ScanColorRectangle(data, i, j, World.colorJumpCollision, offset, returnJump, out rect))
                    {
                        returnJump.Add(rect);
                    }
                    if (ScanColorRectangle(data, i, j, World.colorSlopeRight, offset, returnSlopeRight, out rect))
                    {
                        returnSlopeRight.Add(rect);
                    }
                    if (ScanColorRectangle(data, i, j, World.colorSlopeLeft, offset, returnSlopeLeft, out rect))
                    {
                        returnSlopeLeft.Add(rect);
                    }
                }
            }

            //Assign the out parameters
            rectanglesBasic = returnBasic;
            rectanglesJump = returnJump;
            slopes = new List<Slope>();
            for (int i = 0; i < returnSlopeRight.Count; i++)
            {
                slopes.Add(new Slope(returnSlopeRight[i], true));
            }
            for (int i = 0; i < returnSlopeLeft.Count; i++)
            {
                slopes.Add(new Slope(returnSlopeLeft[i], false));
            }
        }

        public bool ScanColorRectangle(Color[,] data, int i, int j, Color target, Point offset, List<Rectangle> rectanglesExisting, out Rectangle rect)
        {
            int x;

            if (!PointIntersectsRectangles(new Point(i + offset.X, j + offset.Y), rectanglesExisting, out x))
                if (data[i, j] == target)
                {
                    int width = 0;
                    int height = 0;
                    Vector2 position = new Vector2(i, j);

                    int q = data.GetLength(0);

                    //Gets the width of the rectangle
                    while (data[i + width, j] == target && i + width < data.GetLength(0) - 1)
                    {
                        width++;
                    }
                    //Checks every horisontal layer for holes and sets the height of the rectangle
                    bool hollow = false;
                    while (data[i, j + height] == target && !hollow && j + height < data.GetLength(1) - 1)
                    {
                        //Scans for the same color to the left and right of the rectangle that is being created
                        if (i + width < data.GetLength(0) - 1)
                        {
                            //RIGHT
                            if (data[i + width + 1, j + height] == data[i, j])
                                hollow = true;
                        }
                        if (i > 0)
                        {
                            //LEFT
                            if (data[i - 1, j + height] == data[i, j])
                                hollow = true;
                        }

                        if (PointIntersectsRectangles(new Point(i + offset.X, j + height + offset.Y), rectanglesExisting, out x))
                            hollow = true;

                        //Detects if the layer is solid or not
                        if (!hollow)
                        {
                            for (int k = 0; k < width; k++)
                            {
                                if (data[i + k, j + height] != target)
                                {
                                    hollow = true;
                                    break;
                                }
                            }
                        }

                        if (hollow)
                            break;
                        height++;
                    }

                    if (width > 0 && height > 0)
                    {
                        //Adds the rectangle to a list
                        rect = new Rectangle(Convert.ToInt32(position.X + offset.X), Convert.ToInt32(position.Y + offset.Y), width, height);
                        return true;
                    }
                }
            rect = new Rectangle(0, 0, 0, 0);
            return false;
        }

        /// <summary>
        /// Checks if a point intersects with any rectangle in a list
        /// </summary>
        /// <param name="point"></param>
        /// <param name="rectangles"></param>
        /// <param name="id">Returns the number of which rectangle the intersection happened</param>
        /// <returns></returns>
        public static bool PointIntersectsRectangles(Point point, List<Rectangle> rectangles, out int id)
        {
            for (int i = 0; i < rectangles.Count; i++)
            {
                if (rectangles[i].Contains(point))
                {
                    id = i;
                    return true;
                }
            }
            id = -1;
            return false;
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

        public void LoadGame(ContentManager Content)
        {
            //Resets the lists of rooms, removing the last level
            baseRooms = new List<Room>();

            //Go through each rooms folder
            string[] roomFolders = Directory.GetDirectories(LocationRooms);
            for (int i = 0; i < roomFolders.Length; i++)
            {
                //INFO
                string ait = File.ReadAllText(LocationRooms + i.ToString() + @"\info.txt");
                string[] infoStrings = ait.Split('\n');
                bool windBool = false;
                float windMagnitude = 0f;
                if (infoStrings[0].Contains("true"))
                    windBool = true;
                if (infoStrings.Length > 1)
                    windMagnitude = int.Parse(infoStrings[1].Split('_')[0]);

                windMagnitude /= 100;

                Room newRoom = new Room(0, rand, windBool, windMagnitude, null, null);

                //Get collisionrectangles
                string[] collisionmapsFiles = Directory.GetFiles(LocationRooms + i.ToString() + @"\collisionmaps\");
                for (int j = 0; j < collisionmapsFiles.Length; j++)
                {
                    List<Rectangle> roomRectangles = new List<Rectangle>();
                    List<Rectangle> roomRectanglesJump = new List<Rectangle>();
                    List<Slope> roomSlopes = new List<Slope>();

                    int offsetX = int.Parse(Path.GetFileNameWithoutExtension(collisionmapsFiles[j]).Split('_')[0]);
                    int offsetY = int.Parse(Path.GetFileNameWithoutExtension(collisionmapsFiles[j]).Split('_')[1]);
                    ScanImageRectangles(new Vector2(offsetX * World.tileOffsetX, offsetY * World.tileOffsetY).ToPoint(), Texture2D.FromStream(GraphicsDevice, File.OpenRead(collisionmapsFiles[j])), out roomRectangles, out roomRectanglesJump, out roomSlopes);

                    for (int k = 0; k < roomRectangles.Count; k++)
                    {
                        newRoom.platforms.Add(new Platform(newRoom.platforms.Count, roomRectangles[k], Platform.Type.Rectangle));
                    }
                    for (int k = 0; k < roomRectanglesJump.Count; k++)
                    {
                        newRoom.platforms.Add(new Platform(newRoom.platforms.Count, roomRectanglesJump[k], Platform.Type.JumpThrough));
                    }

                    newRoom.slopes.AddRange(roomSlopes);
                }

                //Get backgrounds
                string[] backgroundFiles = Directory.GetFiles(LocationRooms + i.ToString() + @"\backgrounds\");
                for (int j = 0; j < backgroundFiles.Length; j++)
                {
                    int offsetX = int.Parse(Path.GetFileNameWithoutExtension(backgroundFiles[j]).Split('_')[0]);
                    int offsetY = int.Parse(Path.GetFileNameWithoutExtension(backgroundFiles[j]).Split('_')[1]);

                    newRoom.backgrounds.Add(new Point(offsetX, offsetY), Texture2D.FromStream(GraphicsDevice, File.OpenRead(backgroundFiles[j])));
                    newRoom.tiles.Add(new Tile(new Point(offsetX, offsetY)));
                }

                //Get lightSources
                //C: \Users\felix.dunmar\Dropbox\Skit\Rau\Library\levels\0\rooms\1\lightsources
                string[] lightSourcesFiles = Directory.GetFiles(LocationRooms + i.ToString() + @"\lightsources\");
                for (int j = 0; j < lightSourcesFiles.Length; j++)
                {
                    int offsetX = int.Parse(Path.GetFileNameWithoutExtension(lightSourcesFiles[j]).Split('_')[0]);
                    int offsetY = int.Parse(Path.GetFileNameWithoutExtension(lightSourcesFiles[j]).Split('_')[1]);

                    newRoom.lightSources.Add(new Point(offsetX, offsetY), Texture2D.FromStream(GraphicsDevice, File.OpenRead(lightSourcesFiles[j])));
                }

                //Split platforms
                for (int j = 0; j < newRoom.platforms.Count; j++)
                {
                    newRoom.platforms[j].ID = j;
                    for (int k = 0; k < newRoom.platforms.Count; k++)
                    {
                        Rectangle rect = newRoom.platforms[j].rectangle;
                        Rectangle rectTarget = newRoom.platforms[k].rectangle;

                        if (rectTarget.Right < rect.Right && rectTarget.X > rect.X && rect.Y - rectTarget.Bottom < 100 && rect.Y - rectTarget.Bottom > -1)
                        {
                            newRoom.platforms.Add(new Platform(newRoom.platforms.Count - 1, new Rectangle(rect.X, rect.Y, rect.Width / 2, rect.Height), newRoom.platforms[j].type));
                            newRoom.platforms.Add(new Platform(newRoom.platforms.Count - 1, new Rectangle(rect.X + rect.Width / 2, rect.Y, rect.Width / 2, rect.Height), newRoom.platforms[j].type));
                            newRoom.platforms.RemoveAt(j);
                            j--;
                            break;
                        }
                        /*else if(rect.X == rectTarget.X && rectTarget.Right == rect.Right && (Math.Abs(rect.Bottom - rectTarget.Y) < 2 || Math.Abs(rect.Y - rectTarget.Bottom) < 2) 
                            && rooms[roomString].platforms[j].type == rooms[roomString].platforms[k].type && rect.Height + rectTarget.Height < 1024 && j != k)
                        {
                            rooms[roomString].platforms[k].rectangle = new Rectangle(rect.X, Math.Min(rect.Y, rectTarget.Y), rect.Width, Math.Max(rect.Bottom, rectTarget.Bottom) - Math.Min(rect.Y, rectTarget.Y));
                            rooms[roomString].platforms.RemoveAt(j);
                            j--;
                            break;
                        }
                        */
                        /*
                        else if(rect.Y == rectTarget.Y && rectTarget.Bottom == rect.Bottom && (Math.Abs(rect.Right - rectTarget.X) < 2 || Math.Abs(rect.X - rectTarget.Right) < 2)
                            && rooms[roomString].platforms[j].type == rooms[roomString].platforms[k].type && rect.Width + rectTarget.Width < 1024 && j != k)
                        {
                            rooms[roomString].platforms[j].rectangle = new Rectangle(rect.X, Math.Min(rect.Y, rectTarget.Y), Math.Max(rect.Right, rectTarget.Right) - Math.Min(rect.X, rectTarget.X), rect.Height);
                            rooms[roomString].platforms.RemoveAt(k);
                            j--;
                            break;
                        }
                        */
                    }
                }

                for (int j = 0; j < newRoom.slopes.Count; j++)
                {
                    Slope slope = newRoom.slopes[j];
                    for (int k = 0; k < newRoom.platforms.Count; k++)
                    {
                        if (GetDistance(slope.rectangle.Center, newRoom.platforms[k].rectangle.Center) < 1024)
                        {
                            Rectangle rect = newRoom.platforms[k].rectangle;
                            if (Math.Abs(slope.rectangle.Bottom - rect.Y) < 5 && slope.rectangle.X < rect.Right && slope.rectangle.Right > rect.X)
                            {
                                newRoom.slopes[j].platformID = k;
                            }
                        }
                    }
                }

                for (int j = 0; j < newRoom.platforms.Count; j++)
                {
                    newRoom.platforms[j].Initialize(newRoom.platforms, newRoom.slopes);
                }

                for (int j = 0; j < newRoom.platforms.Count; j++)
                {
                    newRoom.platforms[j].GetNodes(newRoom.platforms);
                }

                for (int j = 0; j < newRoom.platforms.Count; j++)
                {
                    for (int k = 0; k < newRoom.platforms[j].nodes.Count; k++)
                    {
                        newRoom.platforms[j].nodes[k].Initialize(newRoom.platforms);
                    }
                }

                //FIND "DOORS"
                for (int j = 0; j < newRoom.tiles.Count; j++)
                {
                    if(!newRoom.backgrounds.ContainsKey(new Point(newRoom.tiles[j].ID.X + 1, newRoom.tiles[j].ID.Y)))
                    {
                        newRoom.tiles[j].edgeRight = true;
                    }
                    else if (!newRoom.backgrounds.ContainsKey(new Point(newRoom.tiles[j].ID.X - 1, newRoom.tiles[j].ID.Y)))
                    {
                        newRoom.tiles[j].edgeLeft = true;
                    }

                    if (newRoom.tiles[j].edgeRight)
                    {
                        for (int k = 0; k < newRoom.platforms.Count; k++)
                        {
                            if(newRoom.platforms[k].usable && Math.Abs((newRoom.platforms[k].rectangle.X + newRoom.platforms[k].rectangle.Width - newRoom.tiles[j].ID.X * 1024) - 1024) < 2)
                            {
                                //Edge of platform intersects and the platform is usable we've found the bottom of the door
                                int doorY = newRoom.platforms[k].rectangle.Y - newRoom.tiles[j].ID.Y * 1024;
                                int wallID = -1;
                                //Now we've got to find what platform to shorten to make the hole
                                for (int l = 0; l < newRoom.platforms.Count; l++)
                                {
                                    //If the Y distance between the bottom of the wall and top of the floor is less than 2 that's the one
                                    //We've also got to make sure that the wall is on the edge of the room
                                    if (Math.Abs(newRoom.platforms[l].rectangle.Bottom - newRoom.tiles[j].ID.Y * 1024 - doorY) < 2
                                        && Math.Abs((newRoom.platforms[l].rectangle.Right - newRoom.tiles[j].ID.X * 1024) - 1024) < 2)
                                    {
                                        //The bottom of the platform is touching the top of the door's base and it's on the edge of the tile
                                        wallID = l;
                                        break;
                                    }
                                }

                                if(wallID != -1)
                                {
                                    //We've found both the wall and the base
                                    newRoom.tiles[j].possibleDoorsRight.Add(new Point(doorY, wallID));
                                }
                            }
                        }
                    }

                    if(newRoom.tiles[j].edgeLeft)
                    {
                        for (int k = 0; k < newRoom.platforms.Count; k++)
                        {
                            if (newRoom.platforms[k].usable && Math.Abs(newRoom.platforms[k].rectangle.X - newRoom.tiles[j].ID.X * 1024) < 2)
                            {
                                //Edge of platform intersects and the platform is usable we've found the bottom of the door
                                int doorY = newRoom.platforms[k].rectangle.Y - newRoom.tiles[j].ID.Y * 1024;
                                int wallID = -1;
                                //Now we've got to find what platform to shorten to make the hole
                                for (int l = 0; l < newRoom.platforms.Count; l++)
                                {
                                    if (Math.Abs(newRoom.platforms[l].rectangle.Bottom - newRoom.tiles[j].ID.Y * 1024 - doorY) < 2
                                        && Math.Abs(newRoom.platforms[l].rectangle.X - newRoom.tiles[j].ID.X * 1024) < 2)
                                    {
                                        //The bottom of the platform is touching the top of the door's base and it's on the edge of the tile
                                        wallID = l;
                                        break;
                                    }
                                }

                                if (wallID != -1)
                                {
                                    //We've found both the wall and the base
                                    newRoom.tiles[j].possibleDoorsLeft.Add(new Point(doorY, wallID));
                                }
                            }
                        }
                    }
                }

                baseRooms.Add(newRoom);
            }
        }

        public void LoadLevel()
        {
            //Clears the last lists of rooms and tiles
            rooms = new List<RoomReference>();
            tiles = new int[World.levelSize, World.levelSize];

            for (int i = 0; i < World.levelSize; i++)
            {
                for (int j = 0; j < World.levelSize; j++)
                {
                    tiles[i, j] = -1;
                }
            }
            
            int startingRoom = 0;

            //Generate level
            TilesAddRoom(new RoomReference(new Point(World.levelSize / 2, World.levelSize / 2), startingRoom));

            bool done = false;
            List<Tile> open = new List<Tile>();

            for (int i = 0; i < baseRooms[rooms[0].reference].tiles.Count; i++)
            {
                open.Add(new Tile(baseRooms[rooms[0].reference].tiles[i].ID));
                open[open.Count - 1].edgeLeft = baseRooms[rooms[0].reference].tiles[i].edgeLeft;
                open[open.Count - 1].edgeRight = baseRooms[rooms[0].reference].tiles[i].edgeRight;
                open[open.Count - 1].possibleDoorsLeft = baseRooms[rooms[0].reference].tiles[i].possibleDoorsLeft;
                open[open.Count - 1].possibleDoorsRight = baseRooms[rooms[0].reference].tiles[i].possibleDoorsRight;
                open[open.Count - 1].position.X = open[open.Count - 1].ID.X + rooms[rooms.Count - 1].position.X;
                open[open.Count - 1].position.Y = open[open.Count - 1].ID.Y + rooms[rooms.Count - 1].position.Y;
            }

            int iteration = 0;

            while (!done)
            {
                iteration++;
                    if (open.Count <= 0)
                    done = true;

                for (int i = 0; i < open.Count; i++)
                {
                    if(open[i].edgeLeft || open[i].edgeRight)
                    {
                        int offsetX = 0;
                        bool found = false;
                        //Possible doors on the right side
                        //This is where it chooses what room to spawn next. In this case in ascending order
                        int[] roomOrder = new int[baseRooms.Count];

                        for (int j = 0; j < roomOrder.Length; j++)
                        {
                            roomOrder[j] = j;
                        }

                        //Randomizes the order of the rooms to get a variation
                        for (int j = 0; j < roomOrder.Length; j++)
                        {
                            int x = rand.Next(roomOrder.Length);
                            int y = roomOrder[j];
                            roomOrder[j] = roomOrder[x];
                            roomOrder[x] = y;
                        }

                        for (int j = 0; j < baseRooms.Count; j++)
                        {
                            int[] tileOrder = new int[baseRooms[roomOrder[j]].tiles.Count];

                            for (int l = 0; l < tileOrder.Length; l++)
                            {
                                tileOrder[l] = l;
                            }

                            //Randomizes the order of the rooms to get a variation
                            for (int l = 0; l < tileOrder.Length; l++)
                            {
                                int x = rand.Next(tileOrder.Length);
                                int y = tileOrder[l];
                                tileOrder[l] = tileOrder[x];
                                tileOrder[x] = y;
                            }

                            for (int k = 0; k < baseRooms[j].tiles.Count; k++)
                            {
                                int platform0Door, platform1Door;
                                platform0Door = -1;
                                platform1Door = -1;
                                if (TileCheckDoorCompatability(open[i], baseRooms[j].tiles[k], out offsetX, out platform0Door, out platform1Door))
                                {
                                    RoomReference newRoom = new RoomReference(new Point(open[i].position.X + offsetX - baseRooms[j].tiles[k].ID.X, open[i].position.Y - baseRooms[j].tiles[k].ID.Y), roomOrder[j]);
                                    if (TilesAddRoom(newRoom))
                                    {
                                        for (int l = 0; l < baseRooms[roomOrder[j]].tiles.Count; l++)
                                        {
                                            open.Add(new Tile(baseRooms[roomOrder[j]].tiles[tileOrder[l]].ID));
                                            open[open.Count - 1].edgeLeft = baseRooms[roomOrder[j]].tiles[tileOrder[l]].edgeLeft;
                                            open[open.Count - 1].edgeRight = baseRooms[roomOrder[j]].tiles[tileOrder[l]].edgeRight;
                                            open[open.Count - 1].possibleDoorsLeft = baseRooms[roomOrder[j]].tiles[tileOrder[l]].possibleDoorsLeft;
                                            open[open.Count - 1].possibleDoorsRight = baseRooms[roomOrder[j]].tiles[tileOrder[l]].possibleDoorsRight;
                                            open[open.Count - 1].position.X = baseRooms[roomOrder[j]].tiles[tileOrder[l]].ID.X + rooms[rooms.Count - 1].position.X;
                                            open[open.Count - 1].position.Y = baseRooms[roomOrder[j]].tiles[tileOrder[l]].ID.Y + rooms[rooms.Count - 1].position.Y;
                                        }
                                        

                                        rooms[open[i].roomID].doors.Add(new Door(platform0Door, rooms.Count - 1, rooms[rooms.Count - 1].doors.Count, offsetX));
                                        rooms[rooms.Count - 1].doors.Add(new Door(platform1Door, open[i].roomID, rooms[open[i].roomID].doors.Count - 1, -offsetX));

                                        if (offsetX == 1 && open[i].edgeLeft)
                                            open[i].edgeRight = false;
                                        else
                                        {
                                            open.RemoveAt(i);
                                            i--;
                                        }

                                        found = true;
                                        break;
                                    }
                                    else offsetX = 0;
                                }
                            }

                            if (found)
                                break;
                        }

                        if (offsetX == 0)
                        {
                            open.RemoveAt(i);
                            i--;
                        }
                    }
                    else
                    {
                        open.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        public void UpdateActiveRooms()
        {
            if (player.room != player.roomPrev)
            {
                List<Room> prevRooms = new List<Room>();

                for (int i = 0; i < activeRooms.Count; i++)
                {
                    prevRooms.Add(activeRooms[i]);
                }

                activeRooms.Clear();
                activeRooms.Add(new Room(player.room, rand, false, 0f, rooms[player.room], baseRooms[rooms[player.room].reference]));
                Room currentRoom = activeRooms[0];
                activeRooms[0].Initialize(player.room, rooms[player.room].position);

                Point[] positions = new Point[currentRoom.tiles.Count];

                for (int i = 0; i < positions.Length; i++)
                {
                    positions[i] = currentRoom.tiles[i].position;
                }

                for (int i = 0; i < positions.Length; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        for (int k = -1; k < 2; k++)
                        {
                            if (positions[i].X + j >= 0 && positions[i].X + j < World.levelSize && positions[i].Y + k >= 0 && positions[i].Y + k < World.levelSize)
                            {
                                if (tiles[positions[i].X + j, positions[i].Y + k] != -1 && !positions.Contains(new Point(positions[i].X + j, positions[i].Y + k)))
                                {
                                    bool exists = false;
                                    for (int l = 0; l < activeRooms.Count; l++)
                                    {
                                        if (activeRooms[l].ID == tiles[positions[i].X + j, positions[i].Y + k])
                                        {
                                            exists = true;
                                            break;
                                        }
                                    }
                                    if (!exists)
                                        activeRooms.Add(new Room(tiles[positions[i].X + j, positions[i].Y + k], rand, false, 0f, rooms[tiles[positions[i].X + j, positions[i].Y + k]], baseRooms[rooms[tiles[positions[i].X + j, positions[i].Y + k]].reference]));
                                }
                            }
                        }
                    }
                }

                for (int i = 0; i < activeRooms.Count; i++)
                {
                    for (int j = 0; j < prevRooms.Count; j++)
                    {
                        if (activeRooms[i].ID == prevRooms[j].ID)
                            activeRooms[i] = prevRooms[j];
                    }
                }
            }
        }

        public bool TileCheckDoorCompatability(Tile t0, Tile t1, out int offsetX, out int platform0, out int platform1)
        {
            if(t0.edgeRight && t1.edgeLeft)
            {
                for (int i = 0; i < t0.possibleDoorsRight.Count; i++)
                {
                    for (int j = 0; j < t1.possibleDoorsLeft.Count; j++)
                    {
                        if(t0.possibleDoorsRight[i].X == t1.possibleDoorsLeft[j].X)
                        {
                            offsetX = 1;
                            platform0 = t0.possibleDoorsRight[i].Y;
                            platform1 = t1.possibleDoorsLeft[j].Y;
                            return true;
                        }
                    }
                }
            }
            else if(t0.edgeLeft && t1.edgeRight)
            {
                for (int i = 0; i < t0.possibleDoorsLeft.Count; i++)
                {
                    for (int j = 0; j < t1.possibleDoorsRight.Count; j++)
                    {
                        if (t0.possibleDoorsLeft[i].X == t1.possibleDoorsRight[j].X)
                        {
                            offsetX = -1;
                            platform0 = t0.possibleDoorsLeft[i].Y;
                            platform1 = t1.possibleDoorsRight[j].Y;
                            return true;
                        }
                    }
                }
            }
            offsetX = 0;
            platform0 = -1;
            platform1 = -1;
            return false;
        }

        public bool TilesAddRoom(RoomReference room)
        {
            if (room.position.X < 0 || room.position.Y < 0 || room.position.X >= World.levelSize || room.position.Y >= World.levelSize)
                return false;

            for (int i = 0; i < baseRooms[room.reference].tiles.Count; i++)
            {
                Tile tile = baseRooms[room.reference].tiles[i];
                if (room.position.X + tile.ID.X >= World.levelSize || room.position.Y + tile.ID.Y >= World.levelSize)
                    return false;
                if (tiles[room.position.X + tile.ID.X, room.position.Y + tile.ID.Y] != -1 && room.position.X + tile.ID.X < World.levelSize && room.position.Y + tile.ID.Y < World.levelSize)
                {
                    return false;
                }
            }
            
            foreach (Tile tile in baseRooms[room.reference].tiles)
            {
                tiles[room.position.X + tile.ID.X, room.position.Y + tile.ID.Y] = rooms.Count;
            }

            rooms.Add(room);

            return true;
        }


        public void DoorInteractionCheck()
        {
            for (int i = 0; i < activeRooms[0].doors.Count; i++)
            {
                if (activeRooms[0].doors[i].rectangle.Intersects(player.Rectangle))
                {
                    Door door = activeRooms[0].doors[i];
                    camera.pos += new Vector2((activeRooms[0].position.X - rooms[activeRooms[0].doors[i].targetRoom].position.X) * 1024, (activeRooms[0].position.Y - rooms[activeRooms[0].doors[i].targetRoom].position.Y) * 1024);
                    player.room = activeRooms[0].doors[i].targetRoom;
                    UpdateActiveRooms();
                    player.position = activeRooms[0].doors[door.targetDoor].rectangle.Center.ToVector2() + new Vector2(door.direction * player.width * 1.5f - (door.rectangle.Width / 2), (World.doorHeight * 0.5f) - player.Rectangle.Height);
                }
            }
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