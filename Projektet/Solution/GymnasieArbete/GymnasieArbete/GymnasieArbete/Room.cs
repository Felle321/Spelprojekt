﻿using System;
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
    public class Room
    {
        public List<Platform> platforms = new List<Platform>();
        public List<Slope> slopes = new List<Slope>();
        public List<GameObject> gameObjects = new List<GameObject>();
        public List<Particle> particles = new List<Particle>();
        public List<EffectObject> effectObjects = new List<EffectObject>();
        public List<DamageZone> damageZones = new List<DamageZone>();
        public List<GameObject.Scrap> scraps = new List<GameObject.Scrap>();
        public Dictionary<int, int> IDToPos = new Dictionary<int, int>();
        public Player player;
        public Dictionary<Point, Texture2D> backgrounds = new Dictionary<Point, Texture2D>();
        public Dictionary<Point, Texture2D> lightSources = new Dictionary<Point, Texture2D>();
        public Color fogColor;
        public float enemyPerTile = 0;
        protected int nextID = 1;
        private float wind = 0f;
        public bool cleared;
        public float Wind
        {
            get
            {
                if (windPositive)
                    return wind;
                else
                    return -wind;
            }
        }
        public int enemyCount = 0;
        public float windMagnitude = 0;
        public bool windBool = true;
        private bool windPositive = true;
        public List<Tile> tiles = new List<Tile>();
        public int ID;
        public Point position;
        public List<Door> doors = new List<Door>();
        public List<Vector2> shakes = new List<Vector2>();
        public List<int> shakeDurations = new List<int>();

        public List<Overlay> overlays = new List<Overlay>();

        public Room(int ID, Random rand, bool windBool, float windMagnitude, RoomReference roomReference, Room baseRoom, Player player)
        {
            if (windBool)
            {
                this.windMagnitude = windMagnitude;
            }
            else
            {
                this.windBool = false;
            }

            //If a room is instanciated
            if (roomReference != null && baseRoom != null)
            {
                this.platforms = baseRoom.platforms;
                this.slopes = baseRoom.slopes;
                this.ID = ID;
                this.position = roomReference.position;
                this.backgrounds = baseRoom.backgrounds;
                this.lightSources = baseRoom.lightSources;
                this.tiles = baseRoom.tiles;
                this.doors = roomReference.doors;
                this.fogColor = baseRoom.fogColor;
                this.enemyPerTile = baseRoom.enemyPerTile;

                //SPAWN ENEMIES
                if (!roomReference.cleared)
                {
                    int spawnedEnemies = 0;
                    float enemyPerTile = 1;
                    List<int> platformSelector = new List<int>();

                    int platform = 0;
                    bool possible = true;
                    int enemy = 0;
                    int width, height;
                    int level = 0;
                    int counter = 0;

                    for (int i = 0; i < platforms.Count; i++)
                    {
                        if (platforms[i].usable)
                            platformSelector.Add(i);
                    }

                    while ((spawnedEnemies < roomReference.enemyCount || (roomReference.enemyCount < 0 && spawnedEnemies < enemyPerTile * tiles.Count)) && counter < 50)
                    {
                        platform = platformSelector[rand.Next(platformSelector.Count)];
                        possible = true;
                        counter++;

                        //Choose what enemy to spawn next
                        enemy = rand.Next(4);

                        switch (enemy)
                        {
                            case (0):
                                //Groundtroop
                                width = 100;
                                height = 100;
                                break;
                            case (1):
                                //Lava Bot
                                width = 100;
                                height = 100;
                                break;
                            case (2):
                                //Blizzard Bot
                                width = 100;
                                height = 100;
                                break;
                            case (3):
                                //Gemini Bot
                                width = 130;
                                height = 130;
                                break;
                            default:
                                width = 0;
                                height = 0;
                                break;
                        }

                        for (int i = 0; i < platforms[platform].rectangle.Width; i += 10)
                        {
                            for (int j = 0; j < platforms.Count; j++)
                            {
                                if (j != platform)
                                {
                                    if (new Rectangle(platforms[platform].rectangle.X + i, platforms[platform].rectangle.Y - height, width, height).Intersects(platforms[j].rectangle))
                                    {
                                        possible = false;
                                        break;
                                    }
                                }
                            }

                            if (possible)
                            {
                                spawnedEnemies++;
                                switch (enemy)
                                {
                                    case (0):
                                        //Troop Bot
                                        gameObjects.Add(new Enemy.GroundTroop(rand, new Vector2(platforms[platform].rectangle.X + i, platforms[platform].rectangle.Y - height), this, player.Level + rand.Next(-2, 3), new Animation("GroundTroop_Idle"), 1, 1));
                                        break;
                                    case (1):
                                        //Lava Bot
                                        gameObjects.Add(new Enemy.LavaBot(rand, new Vector2(platforms[platform].rectangle.X + i, platforms[platform].rectangle.Y - height), this, player.Level + rand.Next(-2, 3), new Animation("LavaBot_Walking"), 1, 1));
                                        break;
                                    case (2):
                                        //Blizzard Bot
                                        gameObjects.Add(new Enemy.BlizzardBot(rand, new Vector2(platforms[platform].rectangle.X + i, platforms[platform].rectangle.Y - height), this, player.Level + rand.Next(-2, 3), new Animation("BlizzardBot_Walking"), 1, 1));
                                        break;
                                    case (3):
                                        //Gemini Bot
                                        //gameObjects.Add(new Enemy.GeminiBot(rand, new Vector2(platforms[platform].rectangle.X + i, platforms[platform].rectangle.Y - height), this, player.Level + rand.Next(-2, 3), new Animation("GeminiBot_WalkLeft"), 1, 1));
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }

        public void Initialize(int ID, Point position)
        {
            this.ID = ID;
            this.position = position;
            for (int i = 0; i < tiles.Count; i++)
            {
                tiles[i].roomID = ID;
                tiles[i].position = new Point(tiles[i].ID.X + position.X, tiles[i].ID.Y + position.Y);
            }
        }

        public void Update(Random rand, Player player)
        {
            cleared = true;
            enemyCount = 0;

            for (int i = 0; i < gameObjects.Count; i++)
            {
                if (gameObjects[i].type == GameObject.Types.Enemy)
                {
                    cleared = false;
                    enemyCount++;
                }
            }

            this.player = player;

            //Update wind
            if (windBool)
            {
                if (Math.Floor(rand.NextDouble() * (100 * windMagnitude)) == 0)
                {
                    windPositive = !windPositive;
                }

                wind = ((rand.Next(5) / (float)10) + .25f) * windMagnitude;
            }
            for (int i = 0; i < scraps.Count; i++)
            {
                if (scraps[i].remove)
                    scraps.RemoveAt(i);
                else
                    scraps[i].Update(this, rand);
            }

            for (int i = 0; i < gameObjects.Count; i++)
            {
                if (!gameObjects[i].remove)
                {
                    gameObjects[i].Update(this, rand);

                    IDToPos[gameObjects[i].ID] = i;
                }
                else
                {
                    if (gameObjects[i].type == GameObject.Types.Enemy || gameObjects[i].type == GameObject.Types.Projectile)
                    {
                        for (int j = 0; j < damageZones.Count; j++)
                        {
                            if (damageZones[j].sourceID == gameObjects[i].ID && damageZones[j].linked)
                                damageZones.RemoveAt(j);
                        }
                    }
                    IDToPos.Remove(gameObjects[i].ID);
                    gameObjects.RemoveAt(i);
                }
            }

            for (int i = 0; i < damageZones.Count; i++)
            {
                if (damageZones[i].hp < 0)
                {
                    damageZones.RemoveAt(i);
                }
                else
                {
                    damageZones[i].Update(this);
                }
            }

            for (int i = 0; i < particles.Count; i++)
            {
                if (particles[i].remove)
                    particles.RemoveAt(i);
                else
                    particles[i].Update(i, this, rand);
            }
        }

        public void Draw(Point offset, Random rand, SpriteBatch spriteBatch, GraphicsDevice graphics, Camera camera)
        {

            foreach (Point key in backgrounds.Keys)
            {
                spriteBatch.Draw(backgrounds[key], new Rectangle(key.X * 1024 + offset.X, key.Y * 1024 + offset.Y, 1024, 1024), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1f);
            }

            for (int i = 0; i < gameObjects.Count; i++)
            {
                gameObjects[i].Draw(offset, rand, spriteBatch, graphics, camera, gameObjects[i].color);
            }
            for (int i = 0; i < scraps.Count; i++)
            {
                scraps[i].Draw(offset, rand, spriteBatch, graphics, camera, scraps[i].color);
            }
            for (int i = 0; i < particles.Count; i++)
            {
                particles[i].Draw(offset, rand, spriteBatch, camera, 0.5f);
            }

            for (int i = 0; i < effectObjects.Count; i++)
            {
                effectObjects[i].Draw(offset, rand, spriteBatch, camera);
            }

            for (int i = 0; i < doors.Count; i++)
            {
                doors[i].Draw(offset, rand, spriteBatch, camera);
            }
        }

        public void DrawLights(Point offset, Random rand, SpriteBatch spriteBatch, Camera camera)
        {
            foreach (Point key in lightSources.Keys)
            {
                spriteBatch.Draw(lightSources[key], new Rectangle(key.X * 1024 + offset.X, key.Y * 1024 + offset.Y, 1024, 1024), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1f);
            }
            for (int i = 0; i < scraps.Count; i++)
            {
                scraps[i].DrawLight(offset, rand, spriteBatch, camera);
            }
            for (int i = 0; i < gameObjects.Count; i++)
            {
                gameObjects[i].DrawLight(offset, rand, spriteBatch, camera);
            }
            for (int i = 0; i < particles.Count; i++)
            {
                particles[i].DrawLight(offset, rand, spriteBatch, camera);
            }
            for (int i = 0; i < doors.Count; i++)
            {
                doors[i].DrawLight(offset, rand, spriteBatch, camera);
            }
        }

        public void DrawCollisions(Point offset, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            for (int i = 0; i < platforms.Count; i++)
            {
                platforms[i].Draw(offset, graphicsDevice, spriteBatch);
                platforms[i].DrawID(offset, spriteBatch);
            }

            for (int i = 0; i < gameObjects.Count; i++)
            {
                gameObjects[i].DrawCol(graphicsDevice, spriteBatch);
            }

            for (int i = 0; i < scraps.Count; i++)
            {
                scraps[i].DrawCol(graphicsDevice, spriteBatch);
            }

            for (int i = 0; i < particles.Count; i++)
            {
                //Game1.DrawLine(graphicsDevice, spriteBatch, particles[i].position, particles[i].position + new Vector2((float)Math.Cos(particles[i].angle) * 5, (float)Math.Sin(particles[i].angle) * 5), Color.Red);
                //Game1.DrawRectangle(graphicsDevice, spriteBatch, particles[i].Rectangle, Color.White);
            }
        }

        public void DrawPlatformNodes(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            for (int i = 0; i < platforms.Count; i++)
            {
                for (int j = 0; j < platforms[i].nodes.Count; j++)
                {
                    Game1.DrawLine(graphicsDevice, spriteBatch, platforms[i].origin, platforms[platforms[i].nodes[j].targetID].origin, Color.Blue);
                }
            }
        }

        public void DrawDebug(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            Vector2 windPosition = new Vector2(150, 300);
            Game1.DrawLine(graphicsDevice, spriteBatch, windPosition, windPosition + new Vector2(Wind * 200, 0), Color.Black);
            spriteBatch.DrawString(Game1.fontDebug, "GameObjects: " + gameObjects.Count.ToString(), new Vector2(20, 500), Color.Red);
            spriteBatch.DrawString(Game1.fontDebug, "Particles: " + particles.Count.ToString(), new Vector2(20, 550), Color.Red);
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
        /// Moves an object according to a Vector2 and returns an offset for the texture (slopes)
        /// </summary>
        /// <param name="position">Position of the object</param>
        /// <param name="rectangle">Rectangle of the object</param>
        /// <param name="movement">Vector to move by</param>
        /// <param name="onGround">OnGround bool</param>
        /// <returns></returns>
        public Vector2 MoveObject(ref Vector2 position, Rectangle rectangle, ref Vector2 movement, ref bool onGround, ref bool onWall, ref bool onWallRight, bool onGroundPrev, ref Slope slope, ref bool onSlope, float bounceFactor, ref int platformID, ref int platformIDPrev, bool fallThrough)
        {
            int platformPrev = platformIDPrev;
            if (platformID != -1)
                platformPrev = platformID;
            onGround = false;
            onWall = false;
            onSlope = false;
            bool skip = true;
            Vector2 oldPosition = new Vector2(position.X, position.Y);
            Vector2 newPosition = new Vector2(position.X + movement.X, position.Y + movement.Y);
            Rectangle oldRectangle = rectangle;
            Rectangle newRectangle = new Rectangle(FloorAdv(newPosition.X), FloorAdv(newPosition.Y), rectangle.Width, rectangle.Height);

            //Checks if we can skip the entire process
            #region Skip
            for (int i = 0; i < platforms.Count; i++)
            {
                if (newRectangle.Intersects(platforms[i].rectangle))
                {
                    skip = false;
                    break;
                }
            }
            if (skip)
            {
                for (int i = 0; i < slopes.Count; i++)
                {
                    if (newRectangle.Intersects(slopes[i].rectangle))
                    {
                        skip = false;
                        break;
                    }
                }
            }
            #endregion

            if (!skip)
            {
                //Checks for collision with slopes
                for (int j = 0; j < slopes.Count; j++)
                {
                    if (rectangle.Intersects(slopes[j].rectangle) || Game1.LineIntersectsLine(new Vector2(slopes[j].rectangle.X, slopes[j].rectangle.Y + slopes[j].rectangle.Height), new Vector2(slopes[j].rectangle.X + slopes[j].rectangle.Width, slopes[j].rectangle.Y), oldPosition, newPosition))
                    {
                        if (slopes[j].faceRight)
                        {
                            //RIGHT
                            float anglePosition = Game1.GetAngle(new Vector2(slopes[j].rectangle.X, slopes[j].rectangle.Bottom), new Vector2(position.X + FloorAdv(movement.X) + rectangle.Width, position.Y + FloorAdv(movement.Y)));

                            if (Game1.LineIntersectsRect(new Vector2(slopes[j].rectangle.X, slopes[j].rectangle.Y + slopes[j].rectangle.Height), new Vector2(slopes[j].rectangle.X + slopes[j].rectangle.Width, slopes[j].rectangle.Y), newRectangle)
                                || (onGroundPrev && movement.Y == 0 && Game1.LineIntersectsRect(new Vector2(slopes[j].rectangle.X, slopes[j].rectangle.Y + slopes[j].rectangle.Height), new Vector2(slopes[j].rectangle.X + slopes[j].rectangle.Width, slopes[j].rectangle.Y), new Rectangle((int)position.X + FloorAdv(movement.X), (int)position.Y + FloorAdv(-movement.X), rectangle.Width, rectangle.Height)))
                                || slopes[j].angle > anglePosition
                                || Game1.LineIntersectsLine(new Vector2(slopes[j].rectangle.X, slopes[j].rectangle.Y + slopes[j].rectangle.Height), new Vector2(slopes[j].rectangle.X + slopes[j].rectangle.Width, slopes[j].rectangle.Y), oldPosition, newPosition))
                            {
                                onSlope = true;
                                slope = slopes[j];
                                float offset = slopes[j].Function(position.X - slopes[j].rectangle.X + rectangle.Width);
                                if (offset < 0)
                                    offset = -offset;
                                offset = FloorAdv(offset);
                                position.X = position.X + movement.X;
                                position.Y = slopes[j].rectangle.Y + slopes[j].rectangle.Height - offset - rectangle.Height;

                                if (bounceFactor == 0)
                                {
                                    onGround = true;
                                    platformID = slopes[j].platformID;
                                    movement.Y = 0;
                                    return new Vector2(0, slopes[j].GetOffset(position.X, rectangle.Width));
                                }
                                else
                                {
                                    float length = movement.Length() * bounceFactor * World.friction * 1 / rectangle.Width * rectangle.Height;
                                    if (length < 1)
                                    {
                                        movement = Vector2.Zero;
                                    }
                                    Vector2 normal = Game1.GetVector2(slopes[j].angle + MathHelper.ToRadians(90)) * movement.Length() / 2;
                                    movement = new Vector2(normal.X + movement.X, normal.Y + movement.Y);
                                    if (movement.X < 0)
                                        movement.X *= World.friction * 0.8f;
                                    return Vector2.Zero;
                                }
                            }
                            else if (bounceFactor == 0 && onGroundPrev && movement.Y >= 0)
                            {
                                onSlope = true;
                                slope = slopes[j];
                                position.X = position.X + movement.X;
                                int offset = FloorAdv(slopes[j].Function(position.X - slopes[j].rectangle.X + rectangle.Width));
                                if (offset < 0)
                                    offset = -offset;
                                offset = FloorAdv(offset);
                                position.Y = slopes[j].rectangle.Y + slopes[j].rectangle.Height - offset - rectangle.Height;
                                onGround = true;
                                platformID = slopes[j].platformID;
                                movement.Y = 0;
                                return new Vector2(0, slopes[j].GetOffset(position.X, rectangle.Width));
                            }
                        }
                        else
                        {
                            //LEFT
                            float anglePosition = Game1.GetAngle(new Vector2(slopes[j].rectangle.Right, slopes[j].rectangle.Bottom), position + new Vector2(FloorAdv(movement.X), FloorAdv(movement.Y))) + MathHelper.ToRadians(180);

                            if (Game1.LineIntersectsRect(new Vector2(slopes[j].rectangle.X, slopes[j].rectangle.Y), new Vector2(slopes[j].rectangle.X + slopes[j].rectangle.Width, slopes[j].rectangle.Y + slopes[j].rectangle.Height), new Rectangle((int)position.X + FloorAdv(movement.X), (int)position.Y + FloorAdv(movement.Y), rectangle.Width, rectangle.Height))
                                || (onGroundPrev && movement.Y == 0 && Game1.LineIntersectsRect(new Vector2(slopes[j].rectangle.X, slopes[j].rectangle.Y), new Vector2(slopes[j].rectangle.X + slopes[j].rectangle.Width, slopes[j].rectangle.Y + slopes[j].rectangle.Height), new Rectangle((int)position.X + FloorAdv(movement.X), (int)position.Y + FloorAdv(movement.X), rectangle.Width, rectangle.Height)))
                                || anglePosition > slopes[j].angle)
                            {
                                slope = slopes[j];
                                onSlope = true;
                                position.X = position.X + movement.X;
                                float offset = slopes[j].Function(position.X - slopes[j].rectangle.X);
                                if (offset > 0)
                                    offset = -offset;
                                offset = FloorAdv(offset);
                                position.Y = slopes[j].rectangle.Y - offset - rectangle.Height;

                                if (bounceFactor == 0)
                                {
                                    onGround = true;
                                    platformID = slopes[j].platformID;
                                    movement.Y = 0;
                                    return new Vector2(0, -slopes[j].GetOffset(position.X, rectangle.Width));
                                }
                                else
                                {
                                    float length = movement.Length() * bounceFactor * World.friction * 1 / rectangle.Width * rectangle.Height;
                                    if (length < 1)
                                    {
                                        movement = Vector2.Zero;
                                    }
                                    Vector2 normal = Game1.GetVector2(slopes[j].angle - MathHelper.ToRadians(90)) * movement.Length() / 2;
                                    movement = new Vector2(normal.X + movement.X, normal.Y + movement.Y);
                                    if (movement.X > 0)
                                        movement.X *= World.friction * 0.8f;
                                    return Vector2.Zero;
                                }
                            }
                            else if (bounceFactor == 0 && onGroundPrev && movement.Y >= 0)
                            {
                                slope = slopes[j];
                                onSlope = true;
                                position.X = position.X + movement.X;
                                float offset = (float)Math.Floor(slopes[j].Function(position.X - slopes[j].rectangle.X));
                                if (offset > 0)
                                    offset = -offset;
                                offset = FloorAdv(offset);
                                position.Y = slopes[j].rectangle.Y - offset - rectangle.Height;
                                onGround = true;
                                platformID = slopes[j].platformID;
                                movement.Y = 0;
                                return new Vector2(0, -slopes[j].GetOffset(position.X, rectangle.Width));
                            }
                        }
                    }
                    else if (new Rectangle(FloorAdv(newPosition.X), FloorAdv(newPosition.Y + 1), rectangle.Width, rectangle.Height).Intersects(slopes[j].rectangle))
                    {
                        if ((slopes[j].faceRight && rectangle.Right >= slopes[j].rectangle.Right) || (!slopes[j].faceRight && rectangle.Left <= slopes[j].rectangle.Left))
                        {
                            onGround = true;
                            platformID = slopes[j].platformID;
                            movement.Y = 0;
                        }
                    }
                }


                //Checks for collisions with normal rectangles
                for (int j = 0; j < platforms.Count; j++)
                {
                    if (new Rectangle(FloorAdv(oldPosition.X), FloorAdv(newPosition.Y), rectangle.Width, rectangle.Height).Intersects(platforms[j].rectangle) && platforms[j].type == Platform.Type.Rectangle)
                    {
                        if (movement.Y > 0 && rectangle.Bottom - platforms[j].rectangle.Y < movement.Y)
                        {
                            position.Y = platforms[j].rectangle.Y - rectangle.Height;
                            onGround = true;
                            platformID = j;
                        }
                        else if (movement.Y < 0)
                        {
                            position.Y = platforms[j].rectangle.Y + platforms[j].rectangle.Height;
                        }
                        if (bounceFactor <= 0)
                            movement.Y = 0;
                        else
                        {
                            movement.Y = movement.Y * bounceFactor * -1;
                        }
                    }
                    else if (movement.Y != 0)
                    {
                        onGround = false;
                    }
                    else if (new Rectangle(FloorAdv(oldPosition.X), FloorAdv(newPosition.Y + 1), rectangle.Width, rectangle.Height).Intersects(platforms[j].rectangle))
                    {
                        onGround = true;
                        platformID = j;
                    }

                    if (new Rectangle(FloorAdv(newPosition.X), FloorAdv(oldPosition.Y), rectangle.Width, rectangle.Height).Intersects(platforms[j].rectangle) && platforms[j].type == Platform.Type.Rectangle)
                    {
                        if (movement.X > 0 && (rectangle.Right - platforms[j].rectangle.X) / 2 < movement.X)
                        {
                            position.X = platforms[j].rectangle.X - rectangle.Width;
                            onWall = true;
                            onWallRight = true;
                        }
                        else if (movement.X < 0 && (platforms[j].rectangle.Right - rectangle.X) / 2 < Math.Abs(movement.X))
                        {
                            position.X = platforms[j].rectangle.X + platforms[j].rectangle.Width;
                            onWall = true;
                            onWallRight = false;
                        }

                        if (bounceFactor <= 0)
                        {
                            movement.X = 0;
                        }
                        else
                        {
                            movement.X = movement.X * bounceFactor * -1;
                        }

                        if (movement.Y > 0)
                            movement.Y *= World.friction;
                    }
                    else if (platforms[j].rectangle.Intersects(new Rectangle(FloorAdv(newPosition.X), FloorAdv(oldPosition.Y), rectangle.Width + 1, rectangle.Height)) && platforms[j].type == Platform.Type.Rectangle)
                    {
                        onWall = true;
                        onWallRight = true;
                        if (bounceFactor <= 0)
                        {
                            movement.X = 0;
                        }
                        else
                        {
                            movement.X = movement.X = movement.X * bounceFactor * -1;
                        }
                    }
                    else if (platforms[j].rectangle.Intersects(new Rectangle(FloorAdv(newPosition.X - 1), FloorAdv(oldPosition.Y), rectangle.Width + 1, rectangle.Height)) && platforms[j].type == Platform.Type.Rectangle)
                    {
                        onWall = true;
                        onWallRight = false;
                        if (bounceFactor <= 0)
                        {
                            movement.X = 0;
                        }
                        else
                        {
                            movement.X = movement.X = movement.X * bounceFactor * -1;
                        }
                    }
                }


                //Checks for collision with jump-through rectangles
                for (int j = 0; j < platforms.Count; j++)
                {
                    if (newRectangle.Intersects(platforms[j].rectangle) && platforms[j].type == Platform.Type.JumpThrough && !fallThrough)
                    {
                        if (oldRectangle.Bottom - 1 < platforms[j].rectangle.Y && movement.Y > 0 && rectangle.Center.Y < platforms[j].rectangle.Center.Y)
                        {
                            position.Y = platforms[j].rectangle.Y - rectangle.Height;
                            movement.Y = 0;
                            onGround = true;
                            platformID = j;
                        }
                    }
                }
            }
            else
            {
                if (movement.Y >= 0)
                {
                    for (int i = 0; i < platforms.Count; i++)
                    {
                        if (new Rectangle((int)(newPosition.X), (int)(newPosition.Y + 1), rectangle.Width, rectangle.Height).Intersects(platforms[i].rectangle) && (platforms[i].type == Platform.Type.Rectangle || !fallThrough))
                        {
                            onGround = true;
                            platformID = i;
                            movement.Y = 0;
                            position.Y = platforms[i].rectangle.Y - rectangle.Height;
                            break;
                        }
                    }


                    for (int i = 0; i < slopes.Count; i++)
                    {
                        if (slopes[i].faceRight)
                        {
                            if (Game1.LineIntersectsRect(new Vector2(slopes[i].rectangle.X, slopes[i].rectangle.Y + slopes[i].rectangle.Height), new Vector2(slopes[i].rectangle.X + slopes[i].rectangle.Width, slopes[i].rectangle.Y), new Rectangle((int)position.X + FloorAdv(movement.X), (int)position.Y + 1, rectangle.Width, rectangle.Height)))
                            {
                                onGround = true;
                                movement.Y = 0;
                                //platformID = i;
                            }
                        }
                        else
                        {
                            if (Game1.LineIntersectsRect(new Vector2(slopes[i].rectangle.X, slopes[i].rectangle.Y), new Vector2(slopes[i].rectangle.X + slopes[i].rectangle.Width, slopes[i].rectangle.Y + slopes[i].rectangle.Height), new Rectangle((int)position.X + FloorAdv(movement.X), (int)position.Y + 1, rectangle.Width, rectangle.Height)))
                            {
                                onGround = true;
                                movement.Y = 0;
                                //platformID = i;
                            }
                        }
                    }
                }

                for (int i = 0; i < platforms.Count; i++)
                {
                    if (platforms[i].type == Platform.Type.Rectangle)
                    {
                        if (platforms[i].rectangle.Intersects(new Rectangle(FloorAdv(newPosition.X), FloorAdv(oldPosition.Y), rectangle.Width + 1, rectangle.Height)))
                        {
                            onWall = true;
                            onWallRight = true;
                            if (bounceFactor <= 0)
                            {
                                movement.X = 0;
                            }
                            else
                            {
                                movement.X = movement.X = movement.X * bounceFactor * -1;
                            }
                        }
                        else if (platforms[i].rectangle.Intersects(new Rectangle(FloorAdv(newPosition.X - 1), FloorAdv(oldPosition.Y), rectangle.Width + 1, rectangle.Height)))
                        {
                            onWall = true;
                            onWallRight = false;
                            if (bounceFactor <= 0)
                            {
                                movement.X = 0;
                            }
                            else
                            {
                                movement.X = movement.X = movement.X * bounceFactor * -1;
                            }
                        }
                    }
                }
            }
            position += movement;
            platformIDPrev = platformPrev;
            return Vector2.Zero;
        }


        /// <summary>
        /// Gets the next ID and updates the counter
        /// </summary>
        /// <returns></returns>
        public int GetID()
        {
            if (nextID > 16383)
                nextID = 0;
            else
                nextID++;
            bool done = false;

            while (!done)
            {
                if (!IDToPos.Keys.Contains(nextID))
                {
                    done = true;
                }
                else if (nextID > 16383)
                    nextID = 0;
                else
                    nextID++;
            }

            return nextID;
        }

        public struct PathNode
        {
            public int g, h, f, x;
            public Point parent;
            public Instruction instruction;

            public void SetInstruction(Instruction value)
            {
                instruction = value;
            }
        }

        /// <summary>
        /// Returns a list of Points representing the platforms and nodes
        /// </summary>
        /// <param name="sourceID">Platform</param>
        /// <param name="targetID">Platform</param>
        /// <returns></returns>
        public List<Point> GetPath(int sourceID, int targetID)
        {
            if (sourceID == -1 || targetID == -1 || sourceID > platforms.Count || targetID > platforms.Count)
                return new List<Point>();

            List<Point> path = new List<Point>();

            //Key represents the platform
            Dictionary<int, PathNode> open = new Dictionary<int, PathNode>();
            Dictionary<int, PathNode> closed = new Dictionary<int, PathNode>();
            int current = sourceID;
            PathNode newPathNode = new PathNode();
            newPathNode.f = 0;
            newPathNode.g = 0;
            newPathNode.h = 0;
            newPathNode.x = 0;
            open.Add(current, newPathNode);

            bool complete = false;

            while (!complete)
            {
                if (!closed.ContainsKey(current))
                    closed.Add(current, open[current]);
                else
                    closed[current] = open[current];

                open.Remove(current);

                if (current == targetID)
                {
                    while (current != sourceID)
                    {
                        path.Add(closed[current].parent);
                        current = closed[current].parent.X;
                    }

                    path.Reverse();
                    complete = true;
                }
                else
                {
                    for (int i = 0; i < platforms[current].nodes.Count; i++)
                    {
                        int newID = platforms[current].nodes[i].targetID;
                        //Distance from current
                        newPathNode.g = (int)Math.Pow(Game1.GetDistance(platforms[current].origin, platforms[newID].origin), 2);
                        //Distance from target
                        newPathNode.h = (int)Game1.GetDistance(platforms[newID].origin, platforms[targetID].origin) * 2;
                        //Distance from source
                        newPathNode.x = closed[current].x + closed[current].g;
                        //g + h
                        newPathNode.f = newPathNode.g + newPathNode.h + newPathNode.x;
                        newPathNode.parent = new Point(current, i);

                        if (open.ContainsKey(newID))
                        {
                            if (newPathNode.f < open[newID].f)
                                open[newID] = newPathNode;
                        }
                        else
                        {
                            open.Add(newID, newPathNode);
                        }
                    }

                    List<Point> fCosts = new List<Point>();
                    foreach (KeyValuePair<int, PathNode> i in open)
                    {
                        if (i.Key != current && !closed.ContainsKey(i.Key))
                            fCosts.Add(new Point(i.Key, i.Value.f));
                    }

                    if (fCosts.Count < 1)
                        return new List<Point>();
                    else if (fCosts.Count > 1)
                    {
                        fCosts = fCosts.OrderBy(p => p.Y).ToList();
                        if (fCosts[0] == fCosts[1])
                        {
                            List<Point> hCosts = new List<Point>();
                            foreach (KeyValuePair<int, PathNode> i in open)
                            {
                                hCosts.Add(new Point(i.Key, i.Value.h));
                            }

                            hCosts = hCosts.OrderByDescending(p => p.Y).ToList();

                            current = hCosts[0].X;
                        }
                        else
                        {
                            current = fCosts[0].X;
                        }
                    }
                    else
                        current = fCosts[0].X;
                }
            }

            return path;
        }

        /// <summary>
        /// Returns a list of instructions. Dependent on the hitbox, speed, etc.
        /// </summary>
        /// <param name="sourceID">Platform</param>
        /// <param name="targetID">Platform</param>
        /// <param name="measurements">The width and height of the rectangle to use when checking for collision, in that order </param>
        /// <param name="movement">The maximum movement (absolute numbers)</param>
        /// <returns></returns>
        public List<Instruction> GetPathInstructions(int sourceID, int targetID, Point measurements, Vector2 movement, int maxTry)
        {
            if (sourceID == -1 || targetID == -1 || sourceID > platforms.Count|| targetID > platforms.Count)
                return new List<Instruction>();

            List<Instruction> instructions = new List<Instruction>();

            //Key represents the platform
            Dictionary<int, PathNode> open = new Dictionary<int, PathNode>();
            Dictionary<int, PathNode> closed = new Dictionary<int, PathNode>();
            int current = sourceID;
            int prevCurrent = sourceID;
            PathNode newPathNode = new PathNode();
            newPathNode.f = 0;
            newPathNode.g = 0;
            newPathNode.h = 0;
            newPathNode.x = 0;
            List<int> blacklist = new List<int>();
            Instruction instruction = new Instruction(new Point(sourceID, sourceID), Enemy.State.idle, Vector2.Zero, 0, true, 1);
            newPathNode.SetInstruction(instruction);
            open.Add(current, newPathNode);

            bool complete = false;
            int counter = 0;

            while (!complete)
            {
                counter++;
                if (counter >= maxTry)
                    return new List<Instruction>();

                if (!closed.ContainsKey(current))
                    closed.Add(current, open[current]);
                else if (open.ContainsKey(current))
                    closed[current] = open[current];

                open.Remove(current);

                if (current == targetID)
                {
                    while (current != sourceID)
                    {
                        instructions.Add(closed[current].instruction);
                        current = closed[current].parent.X;
                    }

                    instructions.Reverse();
                    complete = true;
                }
                else
                {
                    for (int i = 0; i < platforms[current].nodes.Count; i++)
                    {
                        if (!blacklist.Contains(i))
                        {
                            int newID = platforms[current].nodes[i].targetID;
                            //Distance from current
                            newPathNode.g = (int)Game1.GetDistance(platforms[current].origin, platforms[newID].origin);
                            //Distance from target
                            newPathNode.h = (int)Game1.GetDistance(platforms[newID].origin, platforms[targetID].origin);
                            //Distance from source
                            newPathNode.x = closed[current].x + closed[current].g;
                            //g + h
                            newPathNode.f = newPathNode.g + newPathNode.h + newPathNode.x;
                            newPathNode.parent = new Point(current, i);

                            if (open.ContainsKey(newID))
                            {
                                //if (newPathNode.f < open[newID].f)
                                open[newID] = newPathNode;
                            }
                            else
                            {
                                if (closed.ContainsKey(newID))
                                {
                                    if (newPathNode.parent != closed[newID].parent && newPathNode.f < closed[newID].f)
                                    {
                                        closed.Remove(newID);
                                        open.Add(newID, newPathNode);
                                    }
                                }
                                else
                                    open.Add(newID, newPathNode);
                            }
                        }
                    }

                    List<Point> fCosts = new List<Point>();
                    foreach (KeyValuePair<int, PathNode> i in open)
                    {
                        if (i.Key != current && !closed.ContainsKey(i.Key))
                            fCosts.Add(new Point(i.Key, i.Value.f));
                    }

                    if (fCosts.Count < 1)
                    {
                        //Move current to closed
                        if (closed.ContainsKey(current))
                        {
                            open.Remove(current);
                        }
                        else
                        {
                            closed.Add(current, open[current]);
                            open.Remove(current);
                        }

                        if (open.Count <= 0)
                            return new List<Instruction>();
                    }
                    else if (fCosts.Count > 1)
                    {
                        fCosts = fCosts.OrderBy(p => p.Y).ToList();
                        if (fCosts[0] == fCosts[1])
                        {
                            List<Point> hCosts = new List<Point>();
                            foreach (KeyValuePair<int, PathNode> i in open)
                            {
                                hCosts.Add(new Point(i.Key, i.Value.h));
                            }

                            hCosts = hCosts.OrderByDescending(p => p.Y).ToList();

                            current = hCosts[0].X;
                        }
                        else
                        {
                            current = fCosts[0].X;
                        }
                    }
                    else
                        current = fCosts[0].X;


                    if (open.ContainsKey(current))
                    {
                        //Movement requirements.
                        Node node = platforms[open[current].parent.X].nodes[open[current].parent.Y];
                        Point instPlatform = new Point(open[current].parent.X, current);

                        //Gravity
                        float g = World.gravity.Y;
                        //Height of jump/fall
                        float h = node.h;
                        //Width of jump
                        float w = node.w;
                        //Maximum speed
                        float maxX = movement.X;
                        //Maximum jump
                        float maxY = movement.Y;

                        float Ym = 0;
                        float Xm = 0;

                        int xPos = node.xPos;

                        if (node.direction.X > 0)
                            xPos -= measurements.X;

                        if (node.connected)
                        {
                            instruction = new Instruction(instPlatform, Enemy.State.walking, new Vector2(movement.X * node.direction.X, 0), xPos, true, node.direction.X);
                        }
                        else
                        {
                            if (node.direction.Y > 0)
                            {
                                //If the target platform is underneath the source

                                if ((maxX * g) / h > w)
                                {
                                    if (platforms[instPlatform.X].type == Platform.Type.JumpThrough && platforms[instPlatform.Y].rectangle.Right - measurements.X - 1 > platforms[instPlatform.X].rectangle.X && platforms[instPlatform.Y].rectangle.X + measurements.X + 1 < platforms[instPlatform.X].rectangle.Right)
                                    {
                                        instruction = new Instruction(instPlatform, Enemy.State.fall, Vector2.Zero, xPos, true, node.direction.X);
                                    }
                                    else
                                    {
                                        //Xm = w / (float)(Math.Sqrt(h / g));
                                        Ym = 0;

                                        float time = (float)Math.Sqrt(g / h);

                                        if (time * maxX > w && w > 0)
                                        {
                                            Xm = w / time;
                                        }
                                        else
                                            Xm = maxX;

                                        if(InstructionCollisionCheck(node, instPlatform, xPos, new Vector2(node.direction.X * Xm, Ym), measurements))
                                            instruction = new Instruction(instPlatform, Enemy.State.walking, new Vector2(node.direction.X * Xm, 0), xPos + (int)(measurements.X * .8f) * node.direction.X, false, node.direction.X);
                                        else
                                            instruction = new Instruction(instPlatform, Enemy.State.walking, new Vector2(node.direction.X, 0), xPos + (int)(measurements.X * .8f) * node.direction.X, false, node.direction.X);
                                    }
                                }
                                else
                                {
                                    //You gotta jump higher to compensate
                                    //Jump based on the maximum X-speed
                                    Xm = maxX;
                                    Ym = ((h * Xm) / w) - ((w * g) / Xm);

                                    if (Math.Abs(Ym) <= maxY)
                                    {
                                        //Return

                                        if (InstructionCollisionCheck(node, instPlatform, xPos, new Vector2(Xm, Ym), measurements))
                                            instruction = new Instruction(instPlatform, Enemy.State.jump, new Vector2(node.direction.X * Xm, Ym), xPos, false, node.direction.X);

                                    }
                                    else
                                    {
                                        //Not possible
                                        blacklist.Add(open[current].parent.Y);
                                        open.Remove(current);

                                        current = prevCurrent;
                                    }
                                }
                            }
                            else
                            {
                                if (((node.direction.X < 0 && platforms[node.targetID].rectangle.Right >= platforms[node.platformID].rectangle.X) || (node.direction.X > 0 && platforms[node.targetID].rectangle.X <= platforms[node.platformID].rectangle.Right)) || node.direction.X == 0)
                                {
                                    //If the target platform is above the source and the platforms intersects
                                    Ym = (float)-Math.Sqrt(2 * h * g) - 1;

                                    if (platforms[instPlatform.Y].type == Platform.Type.Rectangle)
                                    {
                                        Xm = 1;

                                        //Width
                                        w = (float)(Xm * (float)Math.Ceiling(Math.Sqrt(h / (2 * g)))) + 1;

                                        if (node.direction.X > 0)
                                            xPos -= (int)Math.Ceiling(w);
                                        else
                                            xPos += (int)Math.Ceiling(w);

                                        if (Math.Abs(Ym) < movement.Y)
                                        {
                                            //Return
                                            if(InstructionCollisionCheck(node, instPlatform, xPos, new Vector2(node.direction.X * Xm, Ym), measurements))
                                                instruction = new Instruction(instPlatform, Enemy.State.jump, new Vector2(node.direction.X * Xm, Ym), xPos, false, node.direction.X);
                                        }
                                        else
                                        {
                                            //Not possible
                                            blacklist.Add(open[current].parent.Y);
                                            open.Remove(current);

                                            current = prevCurrent;
                                        }
                                    }
                                    else
                                    {
                                        //If the target platform is a jump-through type of platform the x- position and speed doesn't matter
                                        Xm = 0;

                                        //Return
                                        if (InstructionCollisionCheck(node, instPlatform, xPos, new Vector2(Xm, Ym), measurements))
                                            instruction = new Instruction(instPlatform, Enemy.State.jump, new Vector2(Xm, Ym), xPos, true, node.direction.X);
                                    }
                                }
                                else
                                {
                                    //If the target platform is above the source and they do not intersect

                                    //x in these scenarios represents time and y represents y-/x-positions
                                    //y = x * (Ym + x * g)
                                    //y = x * Xm

                                    Ym = (float)-Math.Sqrt(2 * h * g) - 1;

                                    //XMovement
                                    Xm = w / (-Ym / (2 * g));

                                    //YMovement if the x-movement would be maxX. Time based on maxX
                                    float time = (w + 10) / Xm;

                                    if (Math.Abs(Xm) <= maxX)
                                    {
                                        //if (InstructionCollisionCheck(node, instPlatform, xPos, new Vector2(node.direction.X * maxX, -((h / time) + g * time)), measurements))
                                        //{
                                        //    //MaxX is possible
                                        //    Ym = -((h / time) + g * time);
                                        //    instruction = new Instruction(instPlatform, Enemy.State.jump, new Vector2(node.direction.X * maxX, Ym), xPos, false);
                                        //} else
                                        if(InstructionCollisionCheck(node, instPlatform, xPos, new Vector2(node.direction.X * Xm, Ym), measurements))
                                            instruction = new Instruction(instPlatform, Enemy.State.jump, new Vector2(node.direction.X * Xm, Ym), xPos, false, node.direction.X);
                                        else
                                        {
                                            //Not possible
                                            blacklist.Add(open[current].parent.Y);
                                            open.Remove(current);

                                            current = prevCurrent;
                                        }
                                    }
                                    else
                                    {
                                        //You gotta jump higher to compensate
                                        //Jump based on the maximum X-speed
                                        Xm = maxX;
                                        time = (w) / Xm;
                                        Ym = (float)-Math.Sqrt(2 * h * g) - 1;

                                        if (Math.Abs(Ym) <= maxY && InstructionCollisionCheck(node, instPlatform, xPos, new Vector2(node.direction.X * Xm, Ym), measurements))
                                        {
                                            //Return
                                            instruction = new Instruction(instPlatform, Enemy.State.jump, new Vector2(node.direction.X * Xm, Ym), xPos, false, node.direction.X);
                                        }
                                        else
                                        {
                                            //Not possible
                                            blacklist.Add(open[current].parent.Y);
                                            open.Remove(current);

                                            current = prevCurrent;
                                        }
                                    }
                                }
                            }
                        }

                        
                        //Apply the instruction
                        if (open.ContainsKey(current))
                        {
                            newPathNode = new PathNode();
                            newPathNode = open[current];
                            newPathNode.instruction = instruction;
                            blacklist.Clear();

                            open[current] = newPathNode;
                        }
                    }
                }

                prevCurrent = current;
            }

            return instructions;
        }

        /// <summary>
        /// Returns true if the jump is possible
        /// </summary>
        /// <param name="node"></param>
        /// <param name="instPlatform"></param>
        /// <param name="xPos"></param>
        /// <param name="movement"></param>
        /// <param name="measurements"></param>
        /// <returns></returns>
        public bool InstructionCollisionCheck(Node node, Point instPlatform, int xPos, Vector2 movement, Point measurements)
        {
            //TEMPTEMPTEMPTEMPTEMPTEMPTEMPTEMP
            return true;


            //Collision
            //y = x * (Ym + x * g)
            //y = x * Xm

            float Ym = movement.Y;
            float Xm = movement.X;

            float h = node.h;
            float g = World.gravity.Y;

            float t = (float)((-Ym / 2 / g) + Math.Sqrt(Math.Pow((Ym / g) / 2, 2) + h / g));
            float peakY = t * (Ym + t * g);
            float peakX = (float)(Xm  *(float)Math.Ceiling(Math.Sqrt(h / (2 * g)))) + 1;

            //Rectangle collisionRect = Game1.RectangleCreate(new Point(xPos, (int)(platforms[instPlatform.X].rectangle.Y + peakY + measurements.Y)), new Point(xPos + (int)peakX, Math.Max(platforms[instPlatform.X].rectangle.Y, platforms[instPlatform.Y].rectangle.Y)));

            Rectangle collisionRect = Game1.CreateRectangle(new Point(xPos, platforms[instPlatform.X].rectangle.Y), platforms[instPlatform.Y].rectangle.Center);
            collisionRect.Y = platforms[instPlatform.X].rectangle.Y - (int)peakY;
            collisionRect.Height += (int)peakY;

            for (int i = 1; i <= 10; i++)
            {
                for (int j = 0; j < platforms.Count; j++)
                {
                    if(j != instPlatform.X && (platforms[instPlatform.Y].type == Platform.Type.JumpThrough || j != instPlatform.Y))
                    {
                        if (platforms[j].rectangle.Intersects(collisionRect))
                        {
                            //if(Game1.LineIntersectsRect(new Vector2(xPos + i * Xm + measurements.X * node.direction.X, platforms[instPlatform.X].rectangle.Y + i * (Ym + i* g)), new Vector2(xPos + (i + 1) * Xm + measurements.X * node.direction.X, platforms[instPlatform.X].rectangle.Y + (i + 1) * (Ym + (i + 1) * g)), platforms[j].rectangle)
                            //    || Game1.LineIntersectsRect(new Vector2(xPos + i * Xm + measurements.X, platforms[instPlatform.X].rectangle.Y + i * (Ym + i* g) - measurements.Y), new Vector2( xPos + (i + 1) * Xm, platforms[instPlatform.X].rectangle.Y + (i + 1) * (Ym + (i + 1) * g) - measurements.Y), platforms[j].rectangle))
                            //{
                            //    //Not possible
                            //    return false;
                            //}

                            if(new Rectangle((int)(xPos + (i * (t / 10) * Xm * node.direction.X)), (int)(platforms[instPlatform.X].rectangle.Y - (i * (t / 10) * (Ym + g * i * (t / 10)))) - measurements.Y, measurements.X, measurements.Y).Intersects(platforms[j].rectangle))
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Adds a screenshake
        /// </summary>
        /// <param name="magnitude"></param>
        /// <param name="duration"></param>
        public void AddShake(Vector2 magnitude, int duration)
        {
            shakes.Add(magnitude);
            shakeDurations.Add(duration);
        }

        /// <summary>
        /// Checks if the room and it's tiles contains a specified point. (Point relative to the room. The position of the room would be 0, 0 )
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool Contains(Point point)
        {
            if (point.X < 0 || point.Y < 0)
                return false;
            else
            {
                for (int i = 0; i < tiles.Count; i++)
                {
                    if(point.X > tiles[i].ID.X * 1024 && point.X < tiles[i].ID.X * 1024 + 1024 && point.Y > tiles[i].ID.Y * 1024 && point.Y < tiles[i].ID.Y * 1024 + 1024)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}