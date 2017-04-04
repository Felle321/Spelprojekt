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
        Camera camera = new Camera(new Vector2(0, 0));
        List<Door> doors = new List<Door>();
        Dictionary<string, Room> rooms = new Dictionary<string, Room>();
        static Texture2D pixel;
        KeyboardState keyboard, keyboardPrev;
        Player player;
        public static SpriteFont fontDebug;
        public static Sprite spr_debug_test, spr_debug_player, spr_debug_shot;
        RenderTarget2D fogTarget;
        Color fogColor = new Color(100, 100, 100);

        public static string roomString = "";
        public string roomStringPrev = "";
        public static int level = 0;
        public int delay = 0;
        public int delayTime = 0;

        public Dictionary<string, Weapon> Weapons;
        #region Paths
        public static string LocationLevel
        {
            get
            {
                return World.locationLibrary + @"levels\" + level.ToString() + @"\";
            }
            set
            {
                return;
            }
        }
        public static string LocationRooms
        {
            get
            {
                return LocationLevel + @"rooms\";
            }
            set
            {
                return;
            }
        }
        public static string LocationRoom
        {
            get
            {
                return LocationRooms + roomString + @"\";
            }
            set
            {
                return;
            }
        }
        public static string LocationDoors
        {
            get
            {
                return LocationLevel + "doors.txt";
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
        }


        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            fogTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            LoadLevel(Content);

            //Sprites
            #region Player
            SpriteHandler.AddSprite("player_idle", @"player\player", "idle", GraphicsDevice);
            SpriteHandler.AddSprite("player_running", @"player\player", "running", GraphicsDevice);
            SpriteHandler.AddSprite("player_dash", @"player\player", "dash", GraphicsDevice);
            SpriteHandler.AddSprite("player_stall", @"player\player\jump", "stall", GraphicsDevice);
            SpriteHandler.AddSprite("player_jump", @"player\player\jump", "jump", GraphicsDevice);
            SpriteHandler.AddSprite("player_fall", @"player\player\jump", "fall", GraphicsDevice);
            spr_debug_shot = LoadSprite(@"player\weapons\test", "shotSprite");
            Particle.pixel = new Texture2D(GraphicsDevice, 1, 1);
            Particle.pixel.SetData(new Color[1] { Color.White });
            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new Color[1] { Color.White });
            SpriteHandler.AddSprite("debug_test", "teste", GraphicsDevice);
            SpriteHandler.sprites["debug_test"].HUD = true;
            SpriteHandler.AddSprite("weapon_test_shot", @"player\weapons\test", "shotSprite", GraphicsDevice);
            #endregion

            #region Particles
            SpriteHandler.AddSprite("particle_smoke", "particles", "smoke", GraphicsDevice);
            SpriteHandler.AddSprite("particle_fire", "particles", "fire", GraphicsDevice);
            SpriteHandler.AddSprite("gradient", "gradient", GraphicsDevice);
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

            player = new Player(new Rectangle(200, 50, 50, 110), 1, rooms[roomString]);
            fontDebug = Content.Load<SpriteFont>("fontDebug");
            Weapons = new Dictionary<string, Weapon>();
            Weapons.Add("EnergyBlaster", new EnergyBlaster(GraphicsDevice));
            Weapons.Add("EnergyBlaster2", new EnergyBlaster(GraphicsDevice));
            player.leftHand = Weapons["EnergyBlaster"];
            player.rightHand = Weapons["EnergyBlaster2"];

            //MAIN FONTS
            World.fontDamage = Content.Load<SpriteFont>("FontDamage");

        }


        protected override void UnloadContent()
        {

        }


        protected override void Update(GameTime gameTime)
        {
            if (delay == 0)
            {
                keyboard = Keyboard.GetState();

                rooms[roomString].Update(rand, player);
                player.leftHand.Update(player, rooms[roomString], true);
                player.rightHand.Update(player, rooms[roomString], false);
                player.Update(rooms[roomString], rand);

                for (int i = 0; i < doors.Count; i++)
                {
                    if (doors[i].room == roomString)
                    {
                        if (player.Rectangle.Intersects(doors[i].rectangle))
                        {
                            //TEMP
                            //Use door
                            if (keyboard.IsKeyDown(Keys.Enter) && keyboardPrev.IsKeyUp(Keys.Enter))
                            {
                                int targetId = doors[i].targetId;
                                roomString = doors[targetId].room;
                                player.Origin = new Vector2(doors[targetId].Origin.X, doors[targetId].rectangle.Y + doors[targetId].rectangle.Height - player.Rectangle.Height / 2);
                                break;
                            }
                        }
                    }
                }
                //TEMP
                if (keyboard.IsKeyDown(Keys.M) && keyboardPrev.IsKeyUp(Keys.M))
                {
                    for (int i = 0; i < 1; i++)
                    {
                        rooms[roomString].gameObjects.Add(new Enemy.GroundTroop(new Vector2(100, 100), rooms[roomString], 1, new Animation("GroundTroop_Fall"), 2, 2));
                    }
                }

                if (keyboard.IsKeyDown(Keys.N) && keyboardPrev.IsKeyUp(Keys.N))
                {
                    for (int i = 0; i < rooms[roomString].gameObjects.Count; i++)
                    {
                        if (rooms[roomString].gameObjects[i].type == GameObject.Types.Enemy)
                        {
                            rooms[roomString].gameObjects.RemoveAt(i);
                            i--;
                        }
                    }
                }

                if (keyboard.IsKeyDown(Keys.OemComma) && keyboardPrev.IsKeyUp(Keys.OemComma))
                {
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                }

                //TEMP
                if (keyboard.IsKeyDown(Keys.H))
                {
                    camera.zoom += 0.05f;
                }
                else if(keyboard.IsKeyDown(Keys.J))
                {
                    camera.zoom -= 0.05f;
                }

                if (keyboard.IsKeyDown(Keys.X))
                {
                    player.fallThrough = true;
                }
                else
                    player.fallThrough = false;

                ////TEMP
                //if (keyboard.IsKeyDown(Keys.Right))
                //    player.movement.X = 8;
                //if (keyboard.IsKeyDown(Keys.Left))
                //    player.movement.X = -8;
                //if (keyboard.IsKeyDown(Keys.Up) && player.onGround)
                //    player.movement.Y = -40;

                //TEMPTEMPTEMP
                if (keyboard.IsKeyDown(Keys.B) && keyboardPrev.IsKeyUp(Keys.B))
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        Particle.Dynamic particle = new Particle.Dynamic(new Rectangle((int)player.Origin.X, (int)player.Origin.Y, 10, 10), 0, rooms[roomString], null, true, Color.Green, 1, 60, false, false);
                        particle.movement = new Vector2((float)(rand.NextDouble() * 20 - 10) * 3f, (float)(rand.NextDouble() * 10 - 10) * 3f);
                        particle.useResistance = false;
                        particle.bounceFactor = .4f;
                        rooms[roomString].gameObjects.Add(particle);
                    }
                    //rooms[roomString].particles.Add(new Particle.Smoke(new Point((int)player.Origin.X, (int)player.Origin.Y), Vector2.Zero, 1, 1, 180, Color.White, null));
                }
                if (keyboard.IsKeyDown(Keys.Y))
                {
                    rooms[roomString].particles.Add(new Particle.Smoke(rooms[roomString].particles.Count, rand, player.Origin, Vector2.Zero, 0, 1, 1, 120, Color.Red));
                }
                if (keyboard.IsKeyDown(Keys.T))
                {
                    for (int i = 0; i < 10; i++)
                    {
                        rooms[roomString].particles.Add(new Particle.Fire(rooms[roomString].particles.Count, rooms[roomString], rand, player.Origin + new Vector2(rand.Next(80) - 40, rand.Next(80) - 40), Vector2.Zero, 1, 1, 20, new Color(rand.Next(180, 220), rand.Next(100, 120), rand.Next(80, 100))));
                    }
                }

                if (player.onGround && !player.onGroundPrev)
                {
                    camera.AddShake(new Vector2(player.movementPrev.Y / 2, (player.movementPrev.Y) / 4), 5);
                }

                delay = delayTime;

                camera.Target = player.Origin;
                camera.Update(rand);
                roomStringPrev = roomString;
                keyboardPrev = Keyboard.GetState();
                base.Update(gameTime);
            }
            else
                delay--;
        }


        protected override void Draw(GameTime gameTime)
        {
            //Initialize the fog and lightsources
            InitializeLights(gameTime, camera);

            //Reset rendertarget
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.White);

            //Draws the interactive parts of the game
            DrawGame(gameTime);

            //Draws the fog and lightsources
            //DrawDarkness(gameTime);

            //Draw HUD
            DrawHUD(gameTime, player);

            base.Draw(gameTime);
        }

        /// <summary>
        /// Draws everything that is affected by the camera, light, etc.
        /// </summary>
        /// <param name="gameTime"></param>
        public void DrawGame(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, null, null, null, camera.get_transformation(GraphicsDevice));
            //spriteBatch.Draw(sky, new Vector2(0,0), Color.White);
            rooms[roomString].Draw(rand, spriteBatch, camera, GraphicsDevice);
            rooms[roomString].DrawCollisions(GraphicsDevice, spriteBatch);
            //rooms[roomString].DrawPlatformNodes(GraphicsDevice, spriteBatch);
            //DrawDoors(GraphicsDevice, spriteBatch);
            player.Draw(rand, spriteBatch, camera, GraphicsDevice);
            
            for(int i = 0; i < rooms[roomString].gameObjects.Count; i++)
            {
                //rooms[roomString].gameObjects[i].DrawCol(GraphicsDevice, spriteBatch);
            }
            for (int i = 0; i < rooms[roomString].damageZones.Count; i++)
            {
                //DrawRectangle(GraphicsDevice, spriteBatch, rooms[roomString].damageZones[i].rectangle, Color.Green);
            }
            player.DrawCol(GraphicsDevice, spriteBatch);
            spriteBatch.End();
        }

        /// <summary>
        /// Draws everything not affected by the camera, light, etc.
        /// </summary>
        /// <param name="gameTime"></param>
        public void DrawHUD(GameTime gameTime, Player player)
        {
            spriteBatch.Begin();
            //TEMP
            SpriteHandler.Draw("debug_test", rand, spriteBatch, camera, new Vector2(500, 200), SpriteEffects.None, 0.5f);
            #region Debug
            spriteBatch.DrawString(fontDebug, player.hp.ToString(), new Vector2(100, 80), Color.Red);
            spriteBatch.DrawString(fontDebug, player.onGround.ToString(), new Vector2(100, 100), Color.Red);
            spriteBatch.DrawString(fontDebug, player.movement.X.ToString() + "_" + player.movement.Y.ToString(), new Vector2(100, 140), Color.Red);
            spriteBatch.DrawString(fontDebug, player.position.X.ToString() + "_" + player.position.Y.ToString(), new Vector2(100, 180), Color.Red);
            spriteBatch.DrawString(fontDebug, player.Origin.X.ToString() + "_" + player.Origin.Y.ToString(), new Vector2(100, 220), Color.Red);
            spriteBatch.DrawString(fontDebug, camera.pos.X.ToString() + "_" + camera.pos.Y.ToString(), new Vector2(100, 260), Color.Red);
            spriteBatch.DrawString(fontDebug, (player.position.X - player.positionPrev.X).ToString() + "_" + (player.position.Y - player.positionPrev.Y).ToString(), new Vector2(100, 300), Color.Red);
            spriteBatch.DrawString(fontDebug, "FPS: " + (1f / (float)gameTime.ElapsedGameTime.TotalSeconds).ToString(), new Vector2(20, 30), Color.Red);
            spriteBatch.DrawString(fontDebug, "Plyr: " + player.platformID.ToString(), new Vector2(100, 340), Color.Red);
            if(rooms[roomString].gameObjects.Count > 0)
                spriteBatch.DrawString(fontDebug, "Enmy: " + rooms[roomString].gameObjects[0].platformID.ToString(), new Vector2(100, 380), Color.Red);
            spriteBatch.DrawString(fontDebug, GC.GetTotalMemory(false).ToString(), new Vector2(100, 600), Color.Red);
            rooms[roomString].DrawDebug(GraphicsDevice, spriteBatch);
            #endregion
            spriteBatch.End();
        }

        /// <summary>
        /// Initializes and "predraws" the lightsources. Here you "draw" all the lightsources
        /// </summary>
        /// <param name="gameTime"></param>
        public void InitializeLights(GameTime gameTime, Camera camera)
        {
            GraphicsDevice.SetRenderTarget(fogTarget);
            GraphicsDevice.Clear(fogColor);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, camera.get_transformation(GraphicsDevice));
            rooms[roomString].DrawLights(rand, spriteBatch, camera);
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
            spriteBatch.Draw(fogTarget, Vector2.Zero, fogColor);
            spriteBatch.End();
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
        /// Draws every door in the current room
        /// </summary>
        /// <param name="graphicsDevice"></param>
        /// <param name="spriteBatch"></param>
        public void DrawDoors(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            for (int i = 0; i < doors.Count; i++)
            {
                if (doors[i].room == roomString)
                    DrawRectangle(graphicsDevice, spriteBatch, doors[i].rectangle, Color.Blue);
            }
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
            if(a.X == b.X)
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

        public void ScanImageRectangles(Point offset, Texture2D image, out List<Rectangle> rectanglesBasic, out List<Rectangle> rectanglesJump, out List<Slope> slopes, out List<Door> outDoors)
        {
            List<Rectangle> returnBasic = new List<Rectangle>();
            List<Rectangle> returnJump = new List<Rectangle>();
            List<Rectangle> returnSlopeRight = new List<Rectangle>();
            List<Rectangle> returnSlopeLeft = new List<Rectangle>();
            List<Door> returnDoors = new List<Door>();

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
            Door door = new Door(new Rectangle(0, 0, 0, 0), 0, "", 0);
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
                    if (ScanColorDoor(data, i, j, offset, returnDoors, out door))
                    {
                        returnDoors.Add(door);
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
            outDoors = returnDoors;
        }

        public bool ScanColorDoor(Color[,] data, int i, int j, Point offset, List<Door> doorsExisting, out Door outDoor)
        {
            int x;
            List<Rectangle> rectanglesExisting = new List<Rectangle>();
            Rectangle rect;

            for (int k = 0; k < doorsExisting.Count; k++)
            {
                rectanglesExisting.Add(doorsExisting[k].rectangle);
            }

            if (!PointIntersectsRectangles(new Point(i + offset.X, j + offset.Y), rectanglesExisting, out x))
            {
                if ((data[i, j].R == 255 && data[i, j].G == 255) && (data[i, j].B >= 100 && data[i, j].B < 200))
                {
                    int width = 0;
                    int height = 0;
                    Vector2 position = new Vector2(i, j);
                    //Gets the width of the rectangle
                    while (data[i + width, j] == data[i, j])
                    {
                        width++;
                    }
                    //Checks every horisontal layer for holes and sets the height of the rectangle
                    bool hollow = false;
                    while (data[i, j + height] == data[i, j] && !hollow)
                    {
                        //Detects if the layer is solid or not
                        for (int k = 0; k < width; k++)
                        {
                            if (data[i + k, j + height] != data[i, j])
                            {
                                hollow = true;
                                break;
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
                        int id = int.Parse((data[i, j].B).ToString().Remove(0, 1));
                        int targetId = 0;

                        //Finds the ID of the door and sets the doors target to its ID's respective targetId
                        StreamReader sr = new StreamReader(LocationDoors);
                        string allText = sr.ReadToEnd();
                        string[] arrayText = allText.Split('\r');
                        for (int k = 0; k < arrayText.Length; k++)
                        {
                            if (int.Parse(arrayText[k].Split(':')[0]) == id)
                            {
                                targetId = int.Parse(arrayText[k].Split(':')[1]);
                            }
                        }
                        sr.Close();

                        outDoor = new Door(rect, id, roomString, targetId);
                        return true;
                    }
                }
            }
            rect = new Rectangle(0, 0, 0, 0);
            outDoor = new Door(rect, 0, "", 0);
            return false;
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
                if(rectangles[i].Contains(point))
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

        public void LoadLevel(ContentManager Content)
        {
            //Resets the lists of doors and rooms, removing the last level
            doors = new List<Door>();
            rooms = new Dictionary<string, Room>();

            //sky = Texture2D.FromStream(GraphicsDevice, File.OpenRead(LocationLevel + "sky.png"));

            //Go through each rooms folder
            string[] roomFolders = Directory.GetDirectories(LocationRooms);
            for (int i = 0; i < roomFolders.Length; i++)
            {
                roomString = roomFolders[i].Split('\\')[roomFolders[i].Split('\\').Length - 1];

                //INFO
                string ait = File.ReadAllText(LocationRoom + @"info.txt");
                string[] infoStrings = ait.Split('\n');
                bool windBool = false;
                float windMagnitude = 0f;
                if (infoStrings[0].Contains("true"))
                    windBool = true;
                if (infoStrings.Length > 1)
                    windMagnitude = int.Parse(infoStrings[1].Split('_')[0]);

                windMagnitude /= 100;

                rooms.Add(roomString, new Room(rand, windBool, windMagnitude));

                //Get collisionrectangles
                string[] collisionmapsFiles = Directory.GetFiles(LocationRoom + @"collisionmaps\");
                for (int j = 0; j < collisionmapsFiles.Length; j++)
                {
                    List<Rectangle> roomRectangles = new List<Rectangle>();
                    List<Rectangle> roomRectanglesJump = new List<Rectangle>();
                    List<Slope> roomSlopes = new List<Slope>();
                    List<Door> roomDoors = new List<Door>();

                    int offsetX = int.Parse(Path.GetFileNameWithoutExtension(collisionmapsFiles[j]).Split('_')[0]);
                    int offsetY = int.Parse(Path.GetFileNameWithoutExtension(collisionmapsFiles[j]).Split('_')[1]);
                    ScanImageRectangles(new Vector2(offsetX * World.tileOffsetX, offsetY * World.tileOffsetY).ToPoint(), Texture2D.FromStream(GraphicsDevice, File.OpenRead(collisionmapsFiles[j])), out roomRectangles, out roomRectanglesJump, out roomSlopes, out roomDoors);

                    for (int k = 0; k < roomRectangles.Count; k++)
                    {
                        rooms[roomString].platforms.Add(new Platform(rooms[roomString].platforms.Count, roomRectangles[k], Platform.Type.Rectangle));
                    }
                    for (int k = 0; k < roomRectanglesJump.Count; k++)
                    {
                        rooms[roomString].platforms.Add(new Platform(rooms[roomString].platforms.Count, roomRectanglesJump[k], Platform.Type.JumpThrough));
                    }

                    rooms[roomString].slopes.AddRange(roomSlopes);


                    for (int k = 0; k < roomDoors.Count; k++)
                    {
                        roomDoors[k].room = roomString;
                    }
                    doors.AddRange(roomDoors);
                }

                //Split platforms
                for (int j = 0; j < rooms[roomString].platforms.Count; j++)
                {
                    rooms[roomString].platforms[j].ID = j;
                    for (int k = 0; k < rooms[roomString].platforms.Count; k++)
                    {
                        Rectangle rect = rooms[roomString].platforms[j].rectangle;
                        Rectangle rectTarget = rooms[roomString].platforms[k].rectangle;

                        if(rectTarget.Right < rect.Right && rectTarget.X > rect.X && rect.Y - rectTarget.Bottom < 100 && rect.Y - rectTarget.Bottom > -1)
                        {
                            rooms[roomString].platforms.Add(new Platform(rooms[roomString].platforms.Count - 1, new Rectangle(rect.X, rect.Y, rect.Width / 2, rect.Height), rooms[roomString].platforms[j].type));
                            rooms[roomString].platforms.Add(new Platform(rooms[roomString].platforms.Count - 1, new Rectangle(rect.X + rect.Width / 2, rect.Y, rect.Width / 2, rect.Height), rooms[roomString].platforms[j].type));
                            rooms[roomString].platforms.RemoveAt(j);
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

                for (int j = 0; j < rooms[roomString].slopes.Count; j++)
                {
                    Slope slope = rooms[roomString].slopes[j];
                    for (int k = 0; k < rooms[roomString].platforms.Count; k++)
                    {
                        if (GetDistance(slope.rectangle.Center, rooms[roomString].platforms[k].rectangle.Center) < 1024)
                        {
                            Rectangle rect = rooms[roomString].platforms[k].rectangle;
                            if (Math.Abs(slope.rectangle.Bottom - rect.Y) < 5 && slope.rectangle.X < rect.Right && slope.rectangle.Right > rect.X)
                            {
                                rooms[roomString].slopes[j].platformID = k;
                            }
                        }
                    }
                }

                for (int j = 0; j < rooms[roomString].platforms.Count; j++)
                {
                    rooms[roomString].platforms[j].Initialize(rooms[roomString].platforms, rooms[roomString].slopes);
                }

                for (int j = 0; j < rooms[roomString].platforms.Count; j++)
                {
                    rooms[roomString].platforms[j].GetNodes(rooms[roomString].platforms);
                }

                for (int j = 0; j < rooms[roomString].platforms.Count; j++)
                {
                    for (int k = 0; k < rooms[roomString].platforms[j].nodes.Count; k++)
                    {
                        rooms[roomString].platforms[j].nodes[k].Initialize(rooms[roomString].platforms);
                    }
                }

                //Get backgrounds
                string[] backgroundFiles = Directory.GetFiles(LocationRoom + @"backgrounds\");
                for (int j = 0; j < collisionmapsFiles.Length; j++)
                {
                    int offsetX = int.Parse(Path.GetFileNameWithoutExtension(collisionmapsFiles[j]).Split('_')[0]);
                    int offsetY = int.Parse(Path.GetFileNameWithoutExtension(collisionmapsFiles[j]).Split('_')[1]);

                    //rooms[roomString].backgrounds.Add(new Vector2(offsetX, offsetY), Texture2D.FromStream(GraphicsDevice, File.OpenRead(backgroundFiles[j])));
                }
            }

            /*
            //Sort the doors
            for (int i = 0; i < doors.Count; i++)
            {
                Door temp = doors[doors[i].id];
                doors[doors[i].id] = doors[i];
                doors[i] = temp;
            }
            */
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