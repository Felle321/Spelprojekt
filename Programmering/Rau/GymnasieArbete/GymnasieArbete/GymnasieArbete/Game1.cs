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
        RenderTarget2D fogRenderTarget, mainRenderTarget, lightsRenderTarget;
        Color fogColor = new Color(100, 100, 100);
        static Texture2D pixel;
        public int level = 0;
        public int delay = 0;
        public int delayTime = 0;
        public static bool paused = false;
        public static bool lightsExtraEnabled = false;
        public float lightsExtraOpacityFactor = .15f;
        public float LightsExtraOpactiy
        {
            get
            {
                return ((activeRooms[0].fogColor.A + activeRooms[0].fogColor.B + activeRooms[0].fogColor.G + activeRooms[0].fogColor.R) * .25f / 255) * lightsExtraOpacityFactor;
            }
        }
        public int[,] tiles = new int[World.levelSize, World.levelSize];
        public static GamePadState Controller { get; set; }
        public static GamePadState ControllerPrev { get; set; }
        public static bool Controller_Connected { get; set; }
        public static float playerAcceleration, playerDeacceleration, playerTurnFactor;
        public static string saveName = "save";
        public bool doorInteraction = false;

        //REASS

        //Overlay (Getting hurt etc)
        public List<Overlay> overlays = new List<Overlay>();

        //FADE
        public static bool reassEffects = true;
        /// <summary>
        /// 1f - The main draw will be fully opaque. 0f - The entire screen is black
        /// </summary>
        public float mainDrawAlpha = 1f;
        public int fadeTimer = 0;
        protected int fadeTimerStart = 0;
        public float fadeStrength = 1f;
        public int fadeInPoint = 0;
        public int fadeOutPoint = 0;
        public Color mainDrawColor = new Color(255, 255, 255);
        public Color MainDrawColor
        {
            get
            {
                if (fadeTimer == 0 || fadeTimer == fadeTimerStart)
                {
                    mainDrawAlpha = 1;
                }
                else if (fadeInPoint > 0 && fadeTimer > fadeInPoint)
                {
                    mainDrawAlpha = 1 - ((fadeTimerStart - fadeTimer) / (float)(fadeTimerStart - fadeInPoint)) * (1 - fadeStrength);
                }
                else if (fadeOutPoint > 0 && fadeTimer < fadeOutPoint)
                {
                    mainDrawAlpha = fadeStrength + (1 - fadeStrength) * (1 - (fadeTimer / (float)fadeOutPoint));
                }
                else mainDrawAlpha = fadeStrength;

                return mainDrawColor * mainDrawAlpha;
            }
            set
            {
                mainDrawColor = value;
            }
        }

        public Dictionary<string, Weapon> Weapons;
        #region Paths
        public static string LocationData
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\";
            }
        }
        public string LocationSaves
        {
            get
            {
                return LocationData + @"A bird in a cage or something\";
            }
        }
        public string LocationSave
        {
            get
            {
                return LocationSaves + saveName + @"\";
            }
        }
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

            playerAcceleration = .5f;
            playerDeacceleration = .6f;
            playerTurnFactor = .34f;
        }


        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            fogRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            mainRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            lightsRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            InitializeGame(Content);
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

            #region GemeniBot

            SpriteHandler.AddSprite("GeminiBot_WalkLeft", @"enemies\GeminiBot", "WalkLeft", GraphicsDevice);
            SpriteHandler.AddSprite("GeminiBot_WalkRight", @"enemies\GeminiBot", "WalkRight", GraphicsDevice);
            SpriteHandler.AddSprite("GeminiBot_DestroyRight", @"enemies\GeminiBot", "DestroyRight", GraphicsDevice);
            SpriteHandler.AddSprite("GeminiBot_DestroyLeft", @"enemies\GeminiBot", "DestroyLeft", GraphicsDevice);
            SpriteHandler.AddSprite("GeminiBot_DieLeft", @"enemies\GeminiBot", "DieLeft", GraphicsDevice);
            SpriteHandler.AddSprite("GeminiBot_DieRight", @"enemies\GeminiBot", "DieRight", GraphicsDevice);
            SpriteHandler.AddSprite("GeminiBot_ShootRight", @"enemies\GeminiBot", "ShootRight", GraphicsDevice);
            SpriteHandler.AddSprite("GeminiBot_ShootLeft", @"enemies\GeminiBot", "ShootLeft", GraphicsDevice);
            SpriteHandler.AddSprite("GeminiBot_ShootRight_Broken", @"enemies\GeminiBot", "ShootRight_Broken", GraphicsDevice);
            SpriteHandler.AddSprite("GeminiBot_ShootLeft_Broken", @"enemies\GeminiBot", "ShootLeft_Broken", GraphicsDevice);
            SpriteHandler.AddSprite("GeminiBot_Idle", @"enemies\GeminiBot", "Idle", GraphicsDevice);

            #endregion

            #region BlizzardBot

            SpriteHandler.AddSprite("BlizzardBot_Snow", @"enemies\BlizzardBot", "SnowTemp", GraphicsDevice);
            SpriteHandler.AddSprite("BlizzardBot_Spray", @"enemies\BlizzardBot", "Spray", GraphicsDevice);
            SpriteHandler.AddSprite("BlizzardBot_Jump", @"enemies\BlizzardBot", "Jump", GraphicsDevice);
            SpriteHandler.AddSprite("BlizzardBot_Fall", @"enemies\BlizzardBot", "Fall", GraphicsDevice);
            SpriteHandler.AddSprite("BlizzardBot_Stall", @"enemies\BlizzardBot", "Stall", GraphicsDevice);
            SpriteHandler.AddSprite("BlizzardBot_Walking", @"enemies\BlizzardBot", "Walking", GraphicsDevice);

            #endregion

            #region LavaBot

            SpriteHandler.AddSprite("LavaBot_Lava", @"enemies\LavaBot", "Lava", GraphicsDevice);
            SpriteHandler.AddSprite("LavaBot_Spray", @"enemies\LavaBot", "Spray", GraphicsDevice);
            SpriteHandler.AddSprite("LavaBot_Jump", @"enemies\LavaBot", "Jump", GraphicsDevice);
            SpriteHandler.AddSprite("LavaBot_Fall", @"enemies\LavaBot", "Fall", GraphicsDevice);
            SpriteHandler.AddSprite("LavaBot_Stall", @"enemies\LavaBot", "Stall", GraphicsDevice);
            SpriteHandler.AddSprite("LavaBot_Walking", @"enemies\LavaBot", "Walking", GraphicsDevice);

            #endregion

            SpriteHandler.AddSprite("Gear1", @"scrap\Gear1", GraphicsDevice);
            SpriteHandler.AddSprite("Gear2", @"scrap\Gear2", GraphicsDevice);
            SpriteHandler.AddSprite("Gear3", @"scrap\Gear3", GraphicsDevice);

            SpriteHandler.AddSprite("overlay_damage", "GUI", "overlay_damage", GraphicsDevice);
            SpriteHandler.sprites["overlay_damage"].HUD = true;

            SpriteHandler.AddSprite("door_light", "Door", "door_light", GraphicsDevice);
            SpriteHandler.AddSprite("door_lamp", "Door", "door_lamp", GraphicsDevice);
            SpriteHandler.AddSprite("door_lampLight", "Door", "door_lampLight", GraphicsDevice);

            fontDebug = Content.Load<SpriteFont>("fontDebug");
            Weapons = new Dictionary<string, Weapon>();
            Weapons.Add("EnergyBlaster", new Weapon.EnergyBlaster(GraphicsDevice));
            Weapons.Add("EnergyBlaster2", new Weapon.EnergyBlaster(GraphicsDevice));
            Weapons.Add("BAMF", new Weapon.BAMF(GraphicsDevice));
            Weapons.Add("RedBlaster", new Weapon.RedBlaster(GraphicsDevice));
            SpriteHandler.AddSprite("gradient", "gradient", GraphicsDevice);
            SpriteHandler.AddSprite("scrap", "scrap", GraphicsDevice);
            SpriteHandler.AddSprite("particle_fire", "particles", "fire", GraphicsDevice);
            SpriteHandler.AddSprite("particle_spark", "particles", "spark", GraphicsDevice);
            SpriteHandler.AddSprite("particle_spark_light", "particles", "spark_light", GraphicsDevice);
            //MAIN FONTS
            World.fontDamage = Content.Load<SpriteFont>("fontDamage");


            InitializeLevel();
        }


        protected override void UnloadContent()
        {

        }


        protected override void Update(GameTime gameTime)
        {
            keyboard = Keyboard.GetState();
            if (fadeTimer > 0)
                fadeTimer--;
            else
                fadeTimer = 0;

            DoorInteractionCheck();

            if (delay == 0)
            {
                if (keyboard.IsKeyDown(Keys.Escape) && keyboardPrev.IsKeyUp(Keys.Escape))
                {
                    paused = !paused;
                }

                if (!paused)
                {

                    if (reassEffects)
                    {
                        for (int i = 0; i < activeRooms[0].overlays.Count; i++)
                        {
                            AddOverlay(new Overlay(activeRooms[0].overlays[i].sprite, activeRooms[0].overlays[i].duration, activeRooms[0].overlays[i].strength, activeRooms[0].overlays[i].color));
                            activeRooms[0].overlays.RemoveAt(i);
                        }

                        if (activeRooms[0].shakes.Count != activeRooms[0].shakeDurations.Count)
                        {
                            activeRooms[0].shakes.Clear();
                            activeRooms[0].shakeDurations.Clear();
                        }

                        for (int i = 0; i < activeRooms[0].shakes.Count; i++)
                        {
                            camera.AddShake(activeRooms[0].shakes[i], activeRooms[0].shakeDurations[i]);
                            activeRooms[0].shakes.RemoveAt(i);
                            activeRooms[0].shakeDurations.RemoveAt(i);
                        }

                        for (int i = 0; i < overlays.Count; i++)
                        {
                            if (overlays[i].remove)
                            {
                                overlays.RemoveAt(i);
                                i--;
                            }
                            else
                                overlays[i].Update();
                        }
                    }

                    if (keyboard.IsKeyDown(Keys.X))
                        player.fallThrough = true;
                    else if (player.fallThrough)
                        player.fallThrough = false;

                    if (keyboard.IsKeyDown(Keys.L) && keyboardPrev.IsKeyUp(Keys.L))
                    {
                        player.leftHand.LevelUp();
                        player.rightHand.LevelUp();
                        player.LevelUp();
                        player.hp = player.maxHp;
                    }
                    if (keyboard.IsKeyDown(Keys.G) && keyboardPrev.IsKeyUp(Keys.G))
                    {
                        activeRooms[0].gameObjects.Add(new Enemy.LavaBot(rand, player.position, activeRooms[0], player.leftHand.Level, new Animation("LavaBot_Jump"), 1, 1));

                    }
                    if (keyboard.IsKeyDown(Keys.H) && keyboardPrev.IsKeyUp(Keys.H))
                    {
                        activeRooms[0].gameObjects.Add(new Enemy.BlizzardBot(rand, player.position, activeRooms[0], player.leftHand.Level, new Animation("BlizzardBot_Jump"), 1, 1));

                    }
                    if (keyboard.IsKeyDown(Keys.J) && keyboardPrev.IsKeyUp(Keys.J))
                    {
                        activeRooms[0].gameObjects.Add(new Enemy.GeminiBot(rand, player.position, activeRooms[0], player.leftHand.Level, new Animation("GeminiBot_WalkLeft"), 1, 1));

                    }

                    if (keyboard.IsKeyDown(Keys.C) && keyboardPrev.IsKeyUp(Keys.C))
                        Save();

                    if (keyboard.IsKeyDown(Keys.V) && keyboardPrev.IsKeyUp(Keys.V))
                        Load();

                    if (keyboard.IsKeyDown(Keys.B) && keyboardPrev.IsKeyUp(Keys.B))
                        InitializeLevel();

                    if (keyboard.IsKeyDown(Keys.N) && keyboardPrev.IsKeyUp(Keys.N))
                        Fade(100, .4f, .6f, .8f);

                    if (keyboard.IsKeyDown(Keys.M) && keyboardPrev.IsKeyUp(Keys.M))
                        AddOverlay(new Overlay("overlay_damage", 10, .6f, Color.Red));

                    if (keyboard.IsKeyDown(Keys.F) && keyboardPrev.IsKeyUp(Keys.F))
                        activeRooms[0].gameObjects.Add(new Enemy.GroundTroop(rand, player.position, activeRooms[0], player.leftHand.Level, new Animation("GroundTroop_Idle"), 1, 1));

                    if (keyboard.IsKeyDown(Keys.Y) && keyboardPrev.IsKeyUp(Keys.Y))
                    {
                        if (lightsExtraEnabled)
                        {
                            lightsExtraEnabled = false;
                        }
                        else
                        {
                            lightsExtraEnabled = true;
                        }
                    }

                    if (keyboard.IsKeyDown(Keys.U))
                        activeRooms[0].gameObjects.Add(new Particle.Spark(rand, new Rectangle(player.Rectangle.X, player.Rectangle.Y, 8, 8), 0, activeRooms[0], new Vector2(rand.Next(10) - 5, rand.Next(10) - 8), Vector2.Zero, .7f, true, false, false, Color.White, 40, false, false, .9f));

                    if (keyboard.IsKeyDown(Keys.O) && keyboardPrev.IsKeyUp(Keys.O) && lightsExtraOpacityFactor < 1)
                        lightsExtraOpacityFactor += .5f;

                    if (keyboard.IsKeyDown(Keys.P) && keyboardPrev.IsKeyUp(Keys.P) && lightsExtraOpacityFactor > 0)
                        lightsExtraOpacityFactor -= .05f;

                    if(keyboard.IsKeyDown(Keys.Z))
                        activeRooms[0].scraps.Add(new GameObject.Scrap(new Rectangle((int)player.position.X + 700, (int)player.position.Y, SpriteHandler.sprites["Gear3"].width, SpriteHandler.sprites["Gear3"].height), 1, activeRooms[0], "Gear3", new Vector2(rand.Next(-10, 10), rand.Next(-7, -1)), 100, 0, rand));

                    if (keyboard.IsKeyDown(Keys.OemComma) && keyboardPrev.IsKeyUp(Keys.OemComma))
                    {
                        if (reassEffects)
                            reassEffects = false;
                        else
                            reassEffects = true;

                        camera.zoom = 1;
                        StopFade();
                    }

                    UpdateActiveRooms();

                //Room specific
                for (int i = 0; i < activeRooms.Count; i++)
                {
                    activeRooms[i].Update(rand, player);
                    if (activeRooms[i].cleared != rooms[activeRooms[i].ID].cleared)
                    {
                        rooms[activeRooms[i].ID].cleared = activeRooms[i].cleared;
                    }
                }

                player.Update(activeRooms[0], rand);
                player.leftHand.Update(player, activeRooms[0], true);
                player.rightHand.Update(player, activeRooms[0], false);

                if (reassEffects)
                {
                    if (player.onGround && !player.onGroundPrev)
                    {
                        camera.AddShake(new Vector2(player.movementPrev.Y, (player.movementPrev.Y) / 2), 5);
                    }

                    if (player.onWall && !player.onWallPrev)
                    {
                        camera.AddShake(new Vector2(player.movementPrev.X, (player.movementPrev.X) / 2), 5);
                    }
                }


                delay = delayTime;
                player.roomPrev = player.room;

                if (!UpdateCameraTarget())
                {
                    //Camera AI
                    camera.zoomTarget = 1;
                    camera.Target = player.Origin;
                }

                camera.Update(rand);
            }

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
            GraphicsDevice.SetRenderTarget(mainRenderTarget);
            GraphicsDevice.Clear(Color.Black);

            //Draws the interactive parts of the game
            DrawGame(gameTime);

            //Draws the fog and lightsources
            DrawDarkness(gameTime);

            //Draws some extra lights
            DrawLights(gameTime, .3f);

            //Draw the final scene
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();
            spriteBatch.Draw(mainRenderTarget, Vector2.Zero, MainDrawColor);
            if(lightsExtraEnabled)
                spriteBatch.Draw(lightsRenderTarget, Vector2.Zero, MainDrawColor * LightsExtraOpactiy);
            spriteBatch.End();

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
            
            activeRooms[0].Draw(Point.Zero, rand, spriteBatch, GraphicsDevice, camera);
            for (int i = 0; i < activeRooms[0].damageZones.Count; i++)
            {
                DrawRectangle(GraphicsDevice, spriteBatch, activeRooms[0].damageZones[i].rectangle, Color.Green);
            }
            //activeRooms[0].DrawCollisions(Point.Zero, GraphicsDevice, spriteBatch);

            //player.DrawCol(GraphicsDevice, spriteBatch);

            //for (int i = 0; i < activeRooms[0].doors.Count; i++)
            //{
            //    if(activeRooms[0].doors[i].active)
            //        DrawRectangle(GraphicsDevice, spriteBatch, activeRooms[0].doors[i].rectangle, Color.Violet);
            //}

            player.Draw(Point.Zero, rand, spriteBatch, GraphicsDevice, camera, Color.White);
            
            spriteBatch.End();
        }

        /// <summary>
        /// Draws everything not affected by the camera, light, etc.
        /// </summary>
        /// <param name="gameTime"></param>
        public void DrawHUD(GameTime gameTime, Player player)
        {
            spriteBatch.Begin();

            if(reassEffects)
            {
                for (int i = 0; i < overlays.Count; i++)
                {
                    overlays[i].Draw(rand, spriteBatch, camera);
                }
            }

            spriteBatch.DrawString(fontDebug, World.version, new Vector2(20, World.screenHeight - 50), Color.Red);

            //TEMP
            
            DrawETEA(gameTime);
            //SpriteHandler.Draw("debug_test", rand, spriteBatch, camera, new Vector2(500, 200), SpriteEffects.None, 0.5f);
            #region Debug

            spriteBatch.DrawString(fontDebug, "Rooms.Count: " + rooms.Count.ToString(), new Vector2(10, 220), Color.Red);
            spriteBatch.DrawString(fontDebug, "Current Room: " + player.room.ToString(), new Vector2(10, 250), Color.Red);
            spriteBatch.DrawString(fontDebug, "Current Room Reference: " + rooms[player.room].reference.ToString(), new Vector2(10, 280), Color.Red);
            spriteBatch.DrawString(fontDebug, "Tile, relative to room: " + new Point((int)Math.Floor(player.position.X / (float)World.tileOffsetX), (int)Math.Floor(player.position.Y / (float)World.tileOffsetY)).ExportString(), new Vector2(10, 310), Color.Red);
            spriteBatch.DrawString(fontDebug, "GameObjects.Count: " + activeRooms[0].gameObjects.Count.ToString(), new Vector2(10, 340), Color.Red);
            spriteBatch.DrawString(fontDebug, "Particles.Count: " + activeRooms[0].particles.Count.ToString(), new Vector2(10, 370), Color.Red);
            spriteBatch.DrawString(fontDebug, "enemyCount: " + activeRooms[0].enemyCount.ToString(), new Vector2(10, 400), Color.Red);
            spriteBatch.DrawString(fontDebug, "LightsExtraFactor: " + lightsExtraOpacityFactor.ToString(), new Vector2(10, 430), Color.Red);
            spriteBatch.DrawString(fontDebug, "LightsExtraOpacity: " + LightsExtraOpactiy, new Vector2(10, 460), Color.Red);
            spriteBatch.DrawString(fontDebug, "Player.Position: " + player.position, new Vector2(10, 500), Color.Red);

            //spriteBatch.DrawString(fontDebug, player.room.ToString(), new Vector2(100, 40), Color.Red);
            //spriteBatch.DrawString(fontDebug, player.hp.ToString(), new Vector2(100, 80), Color.Red);
            //spriteBatch.DrawString(fontDebug, player.onGround.ToString(), new Vector2(100, 100), Color.Red);
            //spriteBatch.DrawString(fontDebug, player.movement.X.ToString() + "_" + player.movement.Y.ToString(), new Vector2(100, 140), Color.Red);
            //spriteBatch.DrawString(fontDebug, player.position.X.ToString() + "_" + player.position.Y.ToString(), new Vector2(100, 180), Color.Red);
            //spriteBatch.DrawString(fontDebug, player.Origin.X.ToString() + "_" + player.Origin.Y.ToString(), new Vector2(100, 220), Color.Red);
            //spriteBatch.DrawString(fontDebug, camera.pos.X.ToString() + "_" + camera.pos.Y.ToString(), new Vector2(100, 260), Color.Red);
            //spriteBatch.DrawString(fontDebug, (player.position.X - player.positionPrev.X).ToString() + "_" + (player.position.Y - player.positionPrev.Y).ToString(), new Vector2(100, 300), Color.Red);
            //spriteBatch.DrawString(fontDebug, "FPS: " + (1f / (float)gameTime.ElapsedGameTime.TotalSeconds).ToString(), new Vector2(20, 30), Color.Red);
            //spriteBatch.DrawString(fontDebug, "GameObjects. PlayerRoome: " + activeRooms[0].gameObjects.Count.ToString(), new Vector2(400, 50), Color.Red);

            //spriteBatch.DrawString(fontDebug, "onWall: " + player.onWall.ToString(), new Vector2(400, 310), Color.Red);
            //spriteBatch.DrawString(fontDebug, "wallJumpDelay: " + player.wallJumpDelay.ToString(), new Vector2(400, 350), Color.Red);

            #endregion
            spriteBatch.End();
        }

        /// <summary>
        /// Initializes and "predraws" the lightsources. Here you "draw" all the lightsources
        /// </summary>
        /// <param name="gameTime"></param>
        public void InitializeLights(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(fogRenderTarget);
            GraphicsDevice.Clear(activeRooms[0].fogColor);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, camera.get_transformation(GraphicsDevice));

            activeRooms[0].DrawLights(Point.Zero, rand, spriteBatch, camera);

            spriteBatch.End();
        }

        /// <summary>
        /// Initializes and "predraws" the lightsources. Here you "draw" all the lightsources
        /// </summary>
        /// <param name="gameTime"></param>
        public void DrawLights(GameTime gameTime, float opacity)
        {
            GraphicsDevice.SetRenderTarget(lightsRenderTarget);
            GraphicsDevice.Clear(Color.Transparent);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.get_transformation(GraphicsDevice));

            activeRooms[0].DrawLights(Point.Zero, rand, spriteBatch, camera);

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
            spriteBatch.Draw(fogRenderTarget, Vector2.Zero, Color.White);
            spriteBatch.End();
        }
        
        protected void DrawETEA(GameTime gameTime)
        {
            for (int i = 0; i < World.levelSize; i++)
            {
                for (int j = 0; j < World.levelSize; j++)
                {
                    if (tiles[i, j] != -1)
                        spriteBatch.Draw(pixel, new Rectangle(i * 2, j * 2, 2, 2), Color.Black * .7f);
                    else
                        spriteBatch.Draw(pixel, new Rectangle(i * 2, j * 2, 2, 2), Color.White * .7f);
                }
            }

            spriteBatch.Draw(pixel, new Rectangle((rooms[player.room].position.X + (int)Math.Floor(player.position.X / 1024)) * 2, (rooms[player.room].position.Y + (int)Math.Floor(player.position.Y / 1024)) * 2, 2, 2), Color.Red);
        }

        public bool UpdateCameraTarget()
        {
            if (!reassEffects)
                return false;

            List<Point> targets = new List<Point>();

            targets.Add(player.Rectangle.Center);

            Point minTarget = player.Rectangle.Center;
            Point maxTarget = player.Rectangle.Center;

            for (int i = 0; i < activeRooms[0].gameObjects.Count; i++)
            {
                if (Math.Abs(player.Rectangle.Center.X - activeRooms[0].gameObjects[i].Rectangle.Center.X) + 250 < World.screenWidth / Camera.minZoom
                    && Math.Abs(player.Rectangle.Center.Y - activeRooms[0].gameObjects[i].Rectangle.Center.Y) + 250 < World.screenHeight / Camera.minZoom
                    && activeRooms[0].gameObjects[i].type == GameObject.Types.Enemy)
                {
                    Point newTarget = new Point();

                    if (activeRooms[0].gameObjects[i].Rectangle.X < minTarget.X)
                    {
                        minTarget.X = activeRooms[0].gameObjects[i].Rectangle.X;
                        newTarget.X = activeRooms[0].gameObjects[i].Rectangle.X;
                    }
                    else if (activeRooms[0].gameObjects[i].Rectangle.Right > maxTarget.X)
                    {
                        maxTarget.X = activeRooms[0].gameObjects[i].Rectangle.Right;
                        newTarget.X = activeRooms[0].gameObjects[i].Rectangle.Right;
                    }
                    else
                        newTarget.X = activeRooms[0].gameObjects[i].Rectangle.Center.X;

                    if (activeRooms[0].gameObjects[i].Rectangle.Y < minTarget.Y)
                    {
                        minTarget.Y = activeRooms[0].gameObjects[i].Rectangle.Y;
                        newTarget.Y = activeRooms[0].gameObjects[i].Rectangle.Y;
                    }
                    else if (activeRooms[0].gameObjects[i].Rectangle.Bottom > maxTarget.Y)
                    {
                        maxTarget.Y = activeRooms[0].gameObjects[i].Rectangle.Bottom;
                        newTarget.Y = activeRooms[0].gameObjects[i].Rectangle.Bottom;
                    }
                    else
                        newTarget.Y = activeRooms[0].gameObjects[i].Rectangle.Center.Y;


                    targets.Add(newTarget);
                }
            }

            if (targets.Count > 1)
            {
                Rectangle limit = new Rectangle(0,0, (int)(World.screenWidth / Camera.minZoom) + 100, (int)(World.screenHeight / Camera.minZoom) + 100);

                if (Math.Abs(player.Rectangle.Center.X - minTarget.X) < Math.Abs(player.Rectangle.Center.X - maxTarget.X))
                    limit.X = minTarget.X - 50;
                else
                    limit.X = (int)(maxTarget.X - World.screenWidth / Camera.minZoom) + 50;

                if (Math.Abs(player.Rectangle.Center.Y - minTarget.Y) < Math.Abs(player.Rectangle.Center.Y - maxTarget.Y))
                    limit.Y = minTarget.Y - 50;
                else
                    limit.Y = (int)(maxTarget.Y - World.screenHeight / Camera.minZoom) + 50;

                minTarget = player.Rectangle.Center;
                maxTarget = player.Rectangle.Center;

                for (int i = 0; i < targets.Count; i++)
                {
                    if (!limit.Contains(targets[i]))
                    {
                        targets.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        if (targets[i].X < minTarget.X)
                            minTarget.X = targets[i].X;
                        else if (targets[i].X > maxTarget.X)
                            maxTarget.X = targets[i].X;

                        if (targets[i].Y < minTarget.Y)
                            minTarget.Y = targets[i].Y;
                        else if (targets[i].Y > maxTarget.Y)
                            maxTarget.Y = targets[i].Y;
                    }
                }

                if (targets.Count > 0)
                {
                    float zoomX, zoomY;

                    Point playerPos = new Point();

                    if (player.Rectangle.X < minTarget.X)
                    {
                        minTarget.X = player.Rectangle.X - 50;
                        playerPos.X = player.Rectangle.X - 50;
                    }
                    else if (player.Rectangle.Right > maxTarget.X)
                    {
                        maxTarget.X = player.Rectangle.Right + 50;
                        playerPos.X = player.Rectangle.Right + 50;
                    }
                    else
                        playerPos.X = player.Rectangle.Center.X;

                    if (player.Rectangle.Center.Y < minTarget.Y)
                    {
                        minTarget.Y = player.Rectangle.Y - 50;
                        playerPos.Y = player.Rectangle.Y - 50;
                    }
                    else if (player.Rectangle.Bottom > maxTarget.Y)
                    {
                        maxTarget.Y = player.Rectangle.Bottom + 50;
                        playerPos.Y = player.Rectangle.Bottom + 50;
                    }
                    else
                        playerPos.Y = player.Rectangle.Center.Y;

                    targets[0] = playerPos;

                    if (maxTarget.X == minTarget.X)
                        zoomX = Camera.maxZoom;
                    else
                        zoomX = World.screenWidth / (maxTarget.X - minTarget.X);

                    if (maxTarget.Y == minTarget.Y)
                        zoomY = Camera.maxZoom;
                    else
                        zoomY = World.screenHeight / (maxTarget.Y - minTarget.Y);

                    camera.ZoomTarget = Math.Min(zoomX, zoomY);
                    camera.Target = GetMidpoint(minTarget.ToVector2(), maxTarget.ToVector2());
                    return true;
                }
                else
                    return false;

            }
            else
                return false;
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
        /// Returns a resulting vector, pointing from v0 to v1
        /// </summary>
        /// <param name="v0">Source</param>
        /// <param name="v1">Target</param>
        /// <returns></returns>
        public static Vector2 GetVector2(Vector2 v0, Vector2 v1)
        {
            return new Vector2(v1.X - v0.X, v1.Y - v0.Y);
        }

        /// <summary>
        /// Returns a resulting vector, pointing from v0 to v1
        /// </summary>
        /// <param name="v0">Source</param>
        /// <param name="v1">Target</param>
        /// <returns></returns>
        public static Vector2 GetVector2(Point v0, Point v1)
        {
            return new Vector2(v1.X - v0.X, v1.Y - v0.Y);
        }

        /// <summary>
        /// With the corners defined by the points. The order of the points does not affect the output.
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <returns></returns>
        public static Rectangle CreateRectangle(Point p0, Point p1)
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

        public void ScanImageRectangles(Point offset, Texture2D image, out List<Rectangle> rectanglesBasic, out List<Rectangle> rectanglesJump, out List<Slope> slopes, out List<Rectangle> doors)
        {
            List<Rectangle> returnBasic = new List<Rectangle>();
            List<Rectangle> returnJump = new List<Rectangle>();
            List<Rectangle> returnSlopeRight = new List<Rectangle>();
            List<Rectangle> returnSlopeLeft = new List<Rectangle>();
            List<Rectangle> returnDoors = new List<Rectangle>();

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
                    if (ScanColorRectangle(data, i, j, World.colorForcedDoor, offset, returnDoors, out rect))
                    {
                        returnDoors.Add(rect);
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

            doors = returnDoors;
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

        /// <summary>
        /// Creates a fade-effect
        /// </summary>
        /// <param name="duration">The duration of the total effect</param>
        /// <param name="fadeInPoint">The point where the fade-in ends (percentage. 0-1)</param>
        /// <param name="fadeOutPoint">The point where the fade-out begins</param>
        /// <param name="strength">The strength of the effect at max. 1 means pitch black, 0 means nothing</param>
        public void Fade(int duration, float fadeInPoint, float fadeOutPoint, float strength)
        {
            doorInteraction = false;
            if (reassEffects)
            {
                this.fadeTimer = duration;
                this.fadeTimerStart = duration;
                this.fadeInPoint = duration - (int)(duration * fadeInPoint);
                this.fadeOutPoint = duration - (int)(duration * fadeOutPoint);
                this.fadeStrength = 1 - strength;
            }
        }

        /// <summary>
        /// Stops the fade-effect
        /// </summary>
        public void StopFade()
        {
            doorInteraction = false;
            fadeStrength = 0;
            fadeTimer = 0;
            fadeTimerStart = 0;
        }

        public void AddOverlay(Overlay overlay)
        {
            for (int i = 0; i < overlays.Count; i++)
            {
                if (overlay.sprite == overlays[i].sprite && overlays[i].timer > 0)
                {
                    overlays[i].AddOverlay(overlay);
                    return;
                }
            }

            overlays.Add(overlay);
        }

        public void InitializeGame(ContentManager Content)
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

                Room newRoom = new Room(0, rand, windBool, windMagnitude, null, null, player);

                string[] colorString = infoStrings[2].Split('_');
                newRoom.fogColor = new Color(Game1.StringToFloat(colorString[0]), Game1.StringToFloat(colorString[1]), Game1.StringToFloat(colorString[2]), Game1.StringToFloat(colorString[3]));

                newRoom.enemyPerTile = Game1.StringToFloat(infoStrings[3]);

                //Get collisionrectangles
                string[] collisionmapsFiles = Directory.GetFiles(LocationRooms + i.ToString() + @"\collisionmaps\");
                for (int j = 0; j < collisionmapsFiles.Length; j++)
                {
                    List<Rectangle> roomRectangles = new List<Rectangle>();
                    List<Rectangle> roomRectanglesJump = new List<Rectangle>();
                    List<Slope> roomSlopes = new List<Slope>();
                    List<Rectangle> roomDoors = new List<Rectangle>();

                    int offsetX = int.Parse(Path.GetFileNameWithoutExtension(collisionmapsFiles[j]).Split('_')[0]);
                    int offsetY = int.Parse(Path.GetFileNameWithoutExtension(collisionmapsFiles[j]).Split('_')[1]);
                    ScanImageRectangles(new Vector2(offsetX * World.tileOffsetX, offsetY * World.tileOffsetY).ToPoint(), Texture2D.FromStream(GraphicsDevice, File.OpenRead(collisionmapsFiles[j])), out roomRectangles, out roomRectanglesJump, out roomSlopes, out roomDoors);

                    for (int k = 0; k < roomRectangles.Count; k++)
                    {
                        newRoom.platforms.Add(new Platform(newRoom.platforms.Count, roomRectangles[k], Platform.Type.Rectangle));
                    }
                    for (int k = 0; k < roomRectanglesJump.Count; k++)
                    {
                        newRoom.platforms.Add(new Platform(newRoom.platforms.Count, roomRectanglesJump[k], Platform.Type.JumpThrough));
                    }

                    newRoom.slopes.AddRange(roomSlopes);

                    for (int k = 0; k < roomDoors.Count; k++)
                    {
                        newRoom.doors.Add(new Door(new Point(offsetX, offsetY), roomDoors[k]));
                        newRoom.doors[newRoom.doors.Count - 1].baseID = newRoom.doors.Count - 1;
                        newRoom.doors[newRoom.doors.Count - 1].baseRoom = i;

                        if (newRoom.doors[newRoom.doors.Count - 1].rectangle.X % World.tileOffsetX > World.tileOffsetX / 2)
                            newRoom.doors[newRoom.doors.Count - 1].direction = 1;
                        else
                            newRoom.doors[newRoom.doors.Count - 1].direction = -1;
                    }
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
                if (File.Exists(LocationRooms + i.ToString() + @"\lightsources\"))
                {
                    string[] lightSourcesFiles = Directory.GetFiles(LocationRooms + i.ToString() + @"\lightsources\");
                    for (int j = 0; j < lightSourcesFiles.Length; j++)
                    {
                        int offsetX = int.Parse(Path.GetFileNameWithoutExtension(lightSourcesFiles[j]).Split('_')[0]);
                        int offsetY = int.Parse(Path.GetFileNameWithoutExtension(lightSourcesFiles[j]).Split('_')[1]);

                        newRoom.lightSources.Add(new Point(offsetX, offsetY), Texture2D.FromStream(GraphicsDevice, File.OpenRead(lightSourcesFiles[j])));
                    }
                }

                //Join platforms
                for (int j = 0; j < newRoom.platforms.Count; j++)
                {
                    for (int k = 0; k < newRoom.platforms.Count; k++)
                    {
                        int hejesan = 1;
                        //TEST
                        if(j == 7 && k == 20)
                        {
                            hejesan = 2;
                        }

                        if(j == 6 && k == 21)
                        {
                            hejesan = 3;
                        }

                        if(j != k && newRoom.platforms[j].type == newRoom.platforms[k].type)
                        {
                            if(newRoom.platforms[j].rectangle.Y == newRoom.platforms[k].rectangle.Y && newRoom.platforms[j].rectangle.Height == newRoom.platforms[k].rectangle.Height)
                            {
                                if (newRoom.platforms[j].rectangle.X - 1 == newRoom.platforms[k].rectangle.Right)
                                {
                                    newRoom.platforms[k].rectangle = new Rectangle(newRoom.platforms[k].rectangle.X, newRoom.platforms[k].rectangle.Y, newRoom.platforms[j].rectangle.Width + newRoom.platforms[k].rectangle.Width, newRoom.platforms[j].rectangle.Height);
                                    newRoom.platforms.RemoveAt(j);
                                    j--;
                                }
                                else if (newRoom.platforms[j].rectangle.Right == newRoom.platforms[k].rectangle.X - 1)
                                {
                                    newRoom.platforms[j].rectangle = new Rectangle(newRoom.platforms[j].rectangle.X, newRoom.platforms[j].rectangle.Y, newRoom.platforms[j].rectangle.Width + newRoom.platforms[k].rectangle.Width, newRoom.platforms[j].rectangle.Height);
                                    newRoom.platforms.RemoveAt(k);
                                    k--;
                                }
                            }
                            else if(newRoom.platforms[j].rectangle.X == newRoom.platforms[k].rectangle.X && newRoom.platforms[j].rectangle.Width == newRoom.platforms[k].rectangle.Width)
                            {
                                if(newRoom.platforms[j].rectangle.Y - 1 == newRoom.platforms[k].rectangle.Bottom)
                                {
                                    newRoom.platforms[k].rectangle = new Rectangle(newRoom.platforms[k].rectangle.X, newRoom.platforms[k].rectangle.Y, newRoom.platforms[k].rectangle.Width, newRoom.platforms[j].rectangle.Height + newRoom.platforms[k].rectangle.Height);
                                    newRoom.platforms.RemoveAt(j);
                                    j--;
                                }
                                else if(newRoom.platforms[j].rectangle.Bottom == newRoom.platforms[k].rectangle.Y - 1)
                                {
                                    newRoom.platforms[j].rectangle = new Rectangle(newRoom.platforms[j].rectangle.X, newRoom.platforms[j].rectangle.Y, newRoom.platforms[j].rectangle.Width, newRoom.platforms[j].rectangle.Height + newRoom.platforms[k].rectangle.Height);
                                    newRoom.platforms.RemoveAt(k);
                                    k--;
                                }
                            }
                        }
                    }
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
                    newRoom.platforms[j].Initialize(newRoom.platforms, newRoom.slopes, newRoom.tiles);
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
                
                baseRooms.Add(newRoom);
            }
        }


        public void InitializeLevel()
        {
            //Clears the last lists of rooms and tiles
            rooms = new List<RoomReference>();
            tiles = new int[World.levelSize, World.levelSize];
            activeRooms = new List<Room>();
        
            for (int i = 0; i < World.levelSize; i++)
            {
                for (int j = 0; j < World.levelSize; j++)
                {
                    tiles[i, j] = -1;
                }
            }

            int startingRoom = 0;

            //Generate level
            List<Door> newDoors = new List<Door>();
            TilesAddRoom(new RoomReference(new Point(World.levelSize / 2, World.levelSize / 2), startingRoom), out newDoors);
            
            List<Door> possibleDoors = new List<Door>();

            possibleDoors.AddRange(newDoors);

            int iteration, counter, baseDoor;
            iteration = counter = 0;

            while (possibleDoors.Count > 0 || counter > 10000 || rooms.Count > 500)
            {
                iteration = rand.Next(possibleDoors.Count);
                counter++;

                if (possibleDoors.Count <= 0)
                    break;

                //The door the possibleDoors[i] will target
                Point target = new Point(possibleDoors[iteration].position.X + possibleDoors[iteration].direction, possibleDoors[iteration].position.Y);

                if (target.X < World.levelSize && target.X >= 0 && target.Y < World.levelSize && target.Y >= 0)
                {
                    if (tiles[target.X, target.Y] == -1)
                    {
                        newDoors.Clear();
                        if (TryAddRoom(possibleDoors[iteration], out newDoors, out baseDoor))
                        {
                            LinkDoors(rooms[possibleDoors[iteration].sourceRoom].doors[possibleDoors[iteration].baseID], rooms[rooms.Count - 1].doors[baseDoor]);

                            possibleDoors.AddRange(newDoors);
                        }
                        else
                        {
                            
                        }
                    }
                    else
                    {
                        for (int i = 0; i < rooms[tiles[target.X, target.Y]].doors.Count; i++)
                        {
                            if (rooms[tiles[target.X, target.Y]].doors[i].position == target)
                            {
                                if (Math.Abs((rooms[tiles[target.X, target.Y]].doors[i].rectangle.Bottom % 1024) - (possibleDoors[iteration].rectangle.Bottom % 1024)) < 100 && rooms[tiles[target.X, target.Y]].doors[i].direction + possibleDoors[iteration].direction == 0)
                                {
                                    LinkDoors(rooms[possibleDoors[iteration].sourceRoom].doors[possibleDoors[iteration].baseID], rooms[tiles[target.X, target.Y]].doors[i]);
                                }
                            }
                        }
                    }
                }
                possibleDoors.RemoveAt(iteration);
            }

            //PLAYER 
            player = new Player(new Rectangle(200, 500, 50, 110), 0, baseRooms[0]);
            player.room = tiles[World.levelSize / 2, World.levelSize / 2];

            player.leftHand = new Weapon.BAMF(GraphicsDevice);
            player.rightHand = new Weapon.RedBlaster(GraphicsDevice);

            player.roomPrev = -1;
            activeRooms.Add(new Room(0, rand, false, 0f, rooms[0], baseRooms[rooms[0].reference], player));

            

            UpdateActiveRooms();
        }

        public void LinkDoors(Door door0, Door door1)
        {
            door0.targetRoom = door1.sourceRoom;
            door1.targetRoom = door0.sourceRoom;
            door0.targetDoor = door1.baseID;
            door1.targetDoor = door0.baseID;
            door0.active = true;
            door1.active = true;
        }

        public bool TryAddRoom(Door door, out List<Door> newDoors, out int baseDoor)
        {
            int i, targetDoor, targetRoom;
            newDoors = new List<Door>();

            List<int> possibleRooms = new List<int>();

            for (int j = 0; j < baseRooms.Count; j++)
            {
                possibleRooms.Add(j);
            }

            while (possibleRooms.Count > 0)
            {
                i = rand.Next(possibleRooms.Count);

                if(CheckDoorCompatibility( door, baseRooms[possibleRooms[i]], out targetDoor))
                {
                    RoomReference newRoom = new RoomReference(new Point(door.position.X + door.direction - (int)Math.Floor(baseRooms[possibleRooms[i]].doors[targetDoor].rectangle.X / (float)World.tileOffsetX), door.position.Y - (int)Math.Floor(baseRooms[possibleRooms[i]].doors[targetDoor].rectangle.Y / (float)World.tileOffsetY)), possibleRooms[i]);
                    if (TilesAddRoom(newRoom, out newDoors))
                    {
                        targetRoom = rooms.Count - 1;
                        baseDoor = targetDoor;
                        return true;
                    }
                }

                possibleRooms.RemoveAt(i);
            }

            baseDoor = -1;
            targetRoom = -1;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roomPosition"></param>
        /// <param name="door"></param>
        /// <param name="room"></param>
        /// <param name="targetDoor">The id of the target door relative to the baseRoom</param>
        /// <returns></returns>
        public bool CheckDoorCompatibility(Door door, Room room, out int targetDoor)
        {
            List<int> possibleDoors = new List<int>();

            for (int i = 0; i < room.doors.Count; i++)
            {
                possibleDoors.Add(i);
            }

            while(possibleDoors.Count > 0)
            {
                int i = rand.Next(possibleDoors.Count);

                if(Math.Abs((room.doors[possibleDoors[i]].rectangle.Bottom % 1024) - (door.rectangle.Bottom % 1024)) < 100 && room.doors[possibleDoors[i]].direction + door.direction == 0)
                {
                    targetDoor = possibleDoors[i];
                    return true;
                }

                possibleDoors.RemoveAt(i);
            }

            targetDoor = -1;
            return false;
        }


        public void UpdateActiveRooms()
        {
            if (player.room != player.roomPrev)
            {
                List<Room> prevRooms = new List<Room>();

                for (int i = 0; i < activeRooms.Count; i++)
                {
                    prevRooms.Add(activeRooms[i]);
                    rooms[activeRooms[i].ID].cleared = activeRooms[i].cleared;
                    rooms[activeRooms[i].ID].enemyCount = activeRooms[i].enemyCount;
                }

                activeRooms.Clear();
                activeRooms.Add(new Room(player.room, rand, false, 0f, rooms[player.room], baseRooms[rooms[player.room].reference], player));
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
                                    {
                                        activeRooms.Add(new Room(tiles[positions[i].X + j, positions[i].Y + k], rand, false, 0f, rooms[tiles[positions[i].X + j, positions[i].Y + k]], baseRooms[rooms[tiles[positions[i].X + j, positions[i].Y + k]].reference], player));
                                    }
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

        /// <summary>
        /// Spawns enemies in given room
        /// </summary>
        /// <param name="room">The position of the room in the list of ACTIVE ROOMS</param>
        public void SpawnEnemies(int room, float enemyPerTile, int level)
        {
            int spawnedEnemies = 0;
            bool done = false;

            while (spawnedEnemies < enemyPerTile * activeRooms[room].tiles.Count)
            {
                Enemy enemy;
                done = false;

                switch (rand.Next(1))
                {
                    case (0):
                        enemy = new Enemy.LavaBot(rand, player.position, activeRooms[0], player.leftHand.Level, new Animation("LavaBot_Jump"), 1, 1);
                        break;
                    default:
                        return;
                }

                List<int> possiblePlatforms = new List<int>();

                int i;

                for (i = 0; i < activeRooms[room].platforms.Count; i++)
                {
                    possiblePlatforms.Add(i);
                }


                if (!done)
                {
                    while(possiblePlatforms.Count > 0)
                    {
                        int r = rand.Next(possiblePlatforms.Count);
                        i = possiblePlatforms[r];

                        if (!done && activeRooms[room].platforms[i].usable)
                        {
                            for (int j = 0; j < activeRooms[room].platforms[i].rectangle.Width; j += 6)
                            {
                                if (!done)
                                {
                                    for (int k = 0; k < activeRooms[room].platforms.Count; k++)
                                    {
                                        if (i != k)
                                            if (!new Rectangle(activeRooms[room].platforms[i].rectangle.X + j, activeRooms[room].platforms[i].rectangle.Y - enemy.Rectangle.Height, enemy.Rectangle.Width, enemy.Rectangle.Height).Intersects(activeRooms[room].platforms[k].rectangle))
                                            {
                                                spawnedEnemies++;
                                                activeRooms[room].gameObjects.Add(enemy);
                                                done = true;
                                                break;
                                            }
                                    }
                                }
                                else break;
                            }
                        }
                        else if (done)
                            break;
                        else
                            possiblePlatforms.RemoveAt(r);
                    }
                }
            }
        }


        /*
        public bool TileCheckDoorCompatability(Tile t0, Tile t1, out int offsetX, out int platform0, out int platform1)
        {
            if(t0.edgeRight && t1.edgeLeft)
            {
                int[] doorOrderRight = new int[t0.possibleDoorsRight.Count];
                int[] doorOrderLeft = new int[t1.possibleDoorsLeft.Count];

                for (int i = 0; i < doorOrderLeft.Length; i++)
                {
                    doorOrderLeft[i] = i;
                }

                for (int i = 0; i < doorOrderRight.Length; i++)
                {
                    doorOrderRight[i] = i;
                }

                for (int i = 0; i < doorOrderLeft.Length; i++)
                {
                    int random = rand.Next(doorOrderLeft.Length);
                    int temp = doorOrderLeft[random];
                    doorOrderLeft[random] = doorOrderLeft[i];
                    doorOrderLeft[i] = temp;
                }

                for (int i = 0; i < doorOrderRight.Length; i++)
                {
                    int random = rand.Next(doorOrderRight.Length);
                    int temp = doorOrderRight[random];
                    doorOrderRight[random] = doorOrderRight[i];
                    doorOrderRight[i] = temp;
                }

                for (int i = 0; i < t0.possibleDoorsRight.Count; i++)
                {
                    for (int j = 0; j < t1.possibleDoorsLeft.Count; j++)
                    {
                        if(t0.possibleDoorsRight[doorOrderRight[i]].X == t1.possibleDoorsLeft[doorOrderLeft[j]].X)
                        {
                            offsetX = 1;
                            platform0 = t0.possibleDoorsRight[doorOrderRight[i]].Y;
                            platform1 = t1.possibleDoorsLeft[doorOrderLeft[j]].Y;
                            return true;
                        }
                    }
                }
            }
            else if(t0.edgeLeft && t1.edgeRight)
            {
                int[] doorOrderRight = new int[t1.possibleDoorsRight.Count];
                int[] doorOrderLeft = new int[t0.possibleDoorsLeft.Count];

                for (int i = 0; i < doorOrderLeft.Length; i++)
                {
                    doorOrderLeft[i] = i;
                }

                for (int i = 0; i < doorOrderRight.Length; i++)
                {
                    doorOrderRight[i] = i;
                }

                for (int i = 0; i < doorOrderLeft.Length; i++)
                {
                    int random = rand.Next(doorOrderLeft.Length);
                    int temp = doorOrderLeft[random];
                    doorOrderLeft[random] = doorOrderLeft[i];
                    doorOrderLeft[i] = temp;
                }

                for (int i = 0; i < doorOrderRight.Length; i++)
                {
                    int random = rand.Next(doorOrderRight.Length);
                    int temp = doorOrderRight[random];
                    doorOrderRight[random] = doorOrderRight[i];
                    doorOrderRight[i] = temp;
                }

                for (int i = 0; i < t0.possibleDoorsLeft.Count; i++)
                {
                    for (int j = 0; j < t1.possibleDoorsRight.Count; j++)
                    {
                        if (t0.possibleDoorsLeft[doorOrderLeft[i]].X == t1.possibleDoorsRight[doorOrderRight[j]].X)
                        {
                            offsetX = -1;
                            platform0 = t0.possibleDoorsLeft[doorOrderLeft[i]].Y;
                            platform1 = t1.possibleDoorsRight[doorOrderRight[j]].Y;
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
        */
        /*
        public bool TileCheckDoorCompatability(Tile t0, Tile t1, out int offsetX, out int door0, out int door1)
        {
            if (t0.edgeRight && t1.edgeLeft)
            {
                int[] doorOrderRight = new int[t0.possibleDoorsRight.Count];
                int[] doorOrderLeft = new int[t1.possibleDoorsLeft.Count];

                for (int i = 0; i < doorOrderLeft.Length; i++)
                {
                    doorOrderLeft[i] = i;
                }

                for (int i = 0; i < doorOrderRight.Length; i++)
                {
                    doorOrderRight[i] = i;
                }

                for (int i = 0; i < doorOrderLeft.Length; i++)
                {
                    int random = rand.Next(doorOrderLeft.Length);
                    int temp = doorOrderLeft[random];
                    doorOrderLeft[random] = doorOrderLeft[i];
                    doorOrderLeft[i] = temp;
                }

                for (int i = 0; i < doorOrderRight.Length; i++)
                {
                    int random = rand.Next(doorOrderRight.Length);
                    int temp = doorOrderRight[random];
                    doorOrderRight[random] = doorOrderRight[i];
                    doorOrderRight[i] = temp;
                }

                for (int i = 0; i < t0.possibleDoorsRight.Count; i++)
                {
                    for (int j = 0; j < t1.possibleDoorsLeft.Count; j++)
                    {
                        if (Math.Abs(baseRooms[rooms[t0.roomID].reference].doors[t0.possibleDoorsRight[doorOrderRight[i]]].rectangle.Bottom - baseRooms[rooms[t1.roomID].reference].doors[t1.possibleDoorsLeft[doorOrderLeft[j]]].rectangle.Bottom) < 100)
                        {
                            offsetX = 1;
                            door0 = t0.possibleDoorsRight[doorOrderRight[i]];
                            door1 = t1.possibleDoorsLeft[doorOrderLeft[j]];
                            return true;
                        }
                    }
                }
            }

            if (t0.edgeLeft && t1.edgeRight)
            {
                int[] doorOrderRight = new int[t1.possibleDoorsRight.Count];
                int[] doorOrderLeft = new int[t0.possibleDoorsLeft.Count];

                for (int i = 0; i < doorOrderLeft.Length; i++)
                {
                    doorOrderLeft[i] = i;
                }

                for (int i = 0; i < doorOrderRight.Length; i++)
                {
                    doorOrderRight[i] = i;
                }

                for (int i = 0; i < doorOrderLeft.Length; i++)
                {
                    int random = rand.Next(doorOrderLeft.Length);
                    int temp = doorOrderLeft[random];
                    doorOrderLeft[random] = doorOrderLeft[i];
                    doorOrderLeft[i] = temp;
                }

                for (int i = 0; i < doorOrderRight.Length; i++)
                {
                    int random = rand.Next(doorOrderRight.Length);
                    int temp = doorOrderRight[random];
                    doorOrderRight[random] = doorOrderRight[i];
                    doorOrderRight[i] = temp;
                }

                for (int i = 0; i < t0.possibleDoorsLeft.Count; i++)
                {
                    for (int j = 0; j < t1.possibleDoorsRight.Count; j++)
                    {
                        if (Math.Abs(baseRooms[rooms[t0.roomID].reference].doors[t0.possibleDoorsLeft[doorOrderLeft[i]]].rectangle.Bottom - baseRooms[rooms[t1.roomID].reference].doors[t1.possibleDoorsRight[doorOrderRight[j]]].rectangle.Bottom) < 100)
                        {
                            offsetX = -1;
                            door0 = t0.possibleDoorsLeft[doorOrderLeft[i]];
                            door1 = t1.possibleDoorsRight[doorOrderRight[j]];
                            return true;
                        }
                    }
                }
            }
            offsetX = 0;
            door0 = -1;
            door1 = -1;
            return false;
        }
        */

        public bool TilesAddRoom(RoomReference room, out List<Door> doors)
        {
            doors = new List<Door>();

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

            for (int i = 0; i < baseRooms[room.reference].doors.Count; i++)
            {
                doors.Add(new Door(new Point(room.position.X + (int)Math.Floor(baseRooms[room.reference].doors[i].rectangle.X / (float)1024), room.position.Y + (int)Math.Floor(baseRooms[room.reference].doors[i].rectangle.Y / (float)1024)), baseRooms[room.reference].doors[i].rectangle));
                doors[i].baseID = i;
                doors[i].baseRoom = room.reference;
                doors[i].direction = baseRooms[room.reference].doors[i].direction;
                doors[i].sourceRoom = rooms.Count;
            }

            for (int i = 0; i < baseRooms[room.reference].doors.Count; i++)
            {
                room.doors.Add(new Door(new Point(room.position.X + (int)Math.Floor(baseRooms[room.reference].doors[i].rectangle.X / (float)1024), room.position.Y + (int)Math.Floor(baseRooms[room.reference].doors[i].rectangle.Y / (float)1024)), baseRooms[room.reference].doors[i].rectangle));
                room.doors[i].baseID = i;
                room.doors[i].baseRoom = room.reference;
                room.doors[i].sourceRoom = rooms.Count;
                room.doors[i].direction = baseRooms[room.reference].doors[i].direction;
            }

            rooms.Add(room);

            return true;
        }


        public void DoorInteractionCheck()
        {
            if (!doorInteraction && reassEffects)
            {
                doorInteraction = false;
                for (int i = 0; i < activeRooms[0].doors.Count; i++)
                {
                    if (activeRooms[0].doors[i].active && activeRooms[0].doors[i].rectangle.Intersects(player.Rectangle))
                    {
                        Fade(15, .3f, .7f, 1f);
                        doorInteraction = true;
                        paused = true;
                        break;
                    }
                }
            }
            else if (mainDrawAlpha == fadeStrength || !reassEffects)
            {
                for (int i = 0; i < activeRooms[0].doors.Count; i++)
                {
                    if (activeRooms[0].doors[i].active && activeRooms[0].doors[i].rectangle.Intersects(player.Rectangle))
                    {
                        Door door = activeRooms[0].doors[i];
                        camera.pos += new Vector2((activeRooms[0].position.X - rooms[activeRooms[0].doors[i].targetRoom].position.X) * 1024, (activeRooms[0].position.Y - rooms[activeRooms[0].doors[i].targetRoom].position.Y) * 1024);
                        player.room = activeRooms[0].doors[i].targetRoom;
                        UpdateActiveRooms();

                        float newX;

                        if (door.direction > 0)
                            newX = activeRooms[0].doors[door.targetDoor].rectangle.Right + 5;
                        else
                            newX = activeRooms[0].doors[door.targetDoor].rectangle.X - player.Rectangle.Width - 5;

                        player.position = new Vector2(newX, activeRooms[0].doors[door.targetDoor].rectangle.Bottom - player.Rectangle.Height - 2);
                        paused = false;
                    }
                }
            }
            else if (fadeTimer < 1)
                doorInteraction = false;
        }

        public void Save()
        {
            if(!Directory.Exists(LocationSaves))
                Directory.CreateDirectory(LocationSaves);
            if (!Directory.Exists(LocationSave))
                Directory.CreateDirectory(LocationSave);

            //LEVEL
            if(File.Exists(LocationSave + "level.lvl"))
                File.Delete(LocationSave + "level.lvl");

            for (int i = 0; i < activeRooms.Count; i++)
            {
                rooms[activeRooms[i].ID].cleared = activeRooms[i].cleared;
                rooms[activeRooms[i].ID].enemyCount = activeRooms[i].enemyCount;
            }

            StreamWriter sw = new StreamWriter(LocationSave + "level.lvl");
            for (int i = 0; i < rooms.Count; i++)
            {
                string line = rooms[i].reference.ToString() + " " + rooms[i].position.ExportString() + " " + rooms[i].cleared.ToString() + " " + rooms[i].enemyCount.ToString();

                for (int j = 0; j < rooms[i].doors.Count; j++)
                {
                    if (rooms[i].doors[j].active)
                    {
                        line += ":" + j.ToString() + " " + rooms[i].doors[j].targetRoom.ToString() + " " + rooms[i].doors[j].targetDoor.ToString();
                    }
                }
                
                sw.WriteLine(line);
            }
            sw.Close();

            //Player
            SavePlayer();
        }

        public void SavePlayer()
        {
            if (File.Exists(LocationSave + "player.sav"))
                File.Delete(LocationSave + "player.sav");

            StreamWriter sw = new StreamWriter(LocationSave + "player.sav");

            //ALL PLAYER DATA
            string line = player.position.ToPoint().ExportString() + " " + player.room.ToString();

            sw.WriteLine(line);

            sw.Close();
        }

        public void Load()
        {
            if (Directory.Exists(LocationSaves) && Directory.Exists(LocationSave))
            {
                LoadLevel();

                //PLAYER 
                player = new Player(new Rectangle(200, 500, 50, 110), 0, baseRooms[0]);

                player.leftHand = new Weapon.BAMF(GraphicsDevice);
                player.rightHand = new Weapon.RedBlaster(GraphicsDevice);

                UpdateActiveRooms();

                LoadPlayer();
            }
        }

        public void LoadPlayer()
        {
            if (File.Exists(LocationSave + "player.sav"))
            {
                StreamReader sr = new StreamReader(LocationSave + "player.sav");

                string info = sr.ReadLine();

                string[] infoArray = info.Split(' ');

                player.position = CreatePoint(infoArray[0]).ToVector2();
                player.room = int.Parse(infoArray[1]);
                player.roomPrev = -1;


                camera.pos = player.Origin;

                UpdateActiveRooms();

                sr.Close();
            }
        }

        public void LoadLevel()
        {
            //Clears the last lists of rooms and tiles
            rooms = new List<RoomReference>();
            tiles = new int[World.levelSize, World.levelSize];
            activeRooms.Clear();

            for (int i = 0; i < World.levelSize; i++)
            {
                for (int j = 0; j < World.levelSize; j++)
                {
                    tiles[i, j] = -1;
                }
            }

            if (File.Exists(LocationSave + "level.lvl"))
            {
                bool done = false;
                StreamReader sr = new StreamReader(LocationSave + "level.lvl");

                while(!done)
                {
                    string line = sr.ReadLine();
                    if (line != "" && line != null)
                    {
                        RoomReference newRoomRef = CreateRoomReference(line);
                        if (newRoomRef != null)
                            rooms.Add(newRoomRef);
                        else
                            line = "";
                    }
                    else
                        done = true;
                }

                for (int i = 0; i < rooms.Count; i++)
                {
                    for (int j = 0; j < baseRooms[rooms[i].reference].tiles.Count; j++)
                    {
                        tiles[rooms[i].position.X + baseRooms[rooms[i].reference].tiles[j].ID.X, rooms[i].position.Y + baseRooms[rooms[i].reference].tiles[j].ID.Y] = i;
                    }
                }

                sr.Close();
            }
        }

        public RoomReference CreateRoomReference(string info)
        {
            if (info != "")
            {
                string[] doorsInfo = info.Split(':');
                if (doorsInfo.Length >= 1)
                {
                    string[] infoArray = doorsInfo[0].Split(' ');

                    RoomReference newRoom = new RoomReference(CreatePoint(infoArray[1]), int.Parse(infoArray[0]));

                    newRoom.cleared = bool.Parse(infoArray[2]);
                    newRoom.enemyCount = int.Parse(infoArray[3]);


                    for (int i = 0; i < baseRooms[newRoom.reference].doors.Count; i++)
                    {
                        newRoom.doors.Add(new Door(new Point(newRoom.position.X + (int)Math.Floor(baseRooms[newRoom.reference].doors[i].rectangle.X / (float)1024), newRoom.position.Y + (int)Math.Floor(baseRooms[newRoom.reference].doors[i].rectangle.Y / (float)1024)), baseRooms[newRoom.reference].doors[i].rectangle));
                        newRoom.doors[i].baseID = i;
                        newRoom.doors[i].baseRoom = newRoom.reference;
                        newRoom.doors[i].sourceRoom = rooms.Count;
                        newRoom.doors[i].direction = baseRooms[newRoom.reference].doors[i].direction;
                    }

                    for (int i = 1; i < doorsInfo.Length; i++)
                    {
                        infoArray = doorsInfo[i].Split(' ');

                        newRoom.doors[int.Parse(infoArray[0])].active = true;
                        newRoom.doors[int.Parse(infoArray[0])].targetRoom = int.Parse(infoArray[1]);
                        newRoom.doors[int.Parse(infoArray[0])].targetDoor = int.Parse(infoArray[2]);
                    }

                    return newRoom;
                }
            }

            return null;
        }

        /// <summary>
        /// Creates a Point from a string containing info, seperated by '_'
        /// </summary>
        /// <param name="info">"X_Y"</param>
        /// <returns></returns>
        public Point CreatePoint(string info)
        {
            return new Point(int.Parse(info.Split('_')[0]), int.Parse(info.Split('_')[1]));
        }

        /// <summary>
        /// Creates a Rectangle from a string containing info, seperated by '_'
        /// </summary>
        /// <param name="info">"X_Y_Width_Height"</param>
        /// <returns></returns>
        public Rectangle CreateRectangle(string info)
        {
            string[] infoArray = info.Split('_');
            return new Rectangle(int.Parse(infoArray[0]), int.Parse(infoArray[1]), int.Parse(infoArray[2]), int.Parse(infoArray[3]));
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