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
        public Dictionary<Point, Texture2D> backgrounds = new Dictionary<Point, Texture2D>();
        public Dictionary<Point, Texture2D> lightSources = new Dictionary<Point, Texture2D>();
        protected int nextID = 1;
        private float wind = 0f;
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
        public float windMagnitude = 0;
        public bool windBool = true;
        private bool windPositive = true;

        //TEMP
        List<Point> path = new List<Point>();

        public Room(Random rand, bool windBool, float windMagnitude)
        {
            if (windBool)
            {
                this.windMagnitude = windMagnitude;
            }
            else
            {
                this.windBool = false;
            }
        }

        public void Update(Random rand, Player player)
        {
            //Update wind
            if (windBool)
            {
                if (Math.Floor(rand.NextDouble() * (100 * windMagnitude)) == 0)
                {
                    windPositive = !windPositive;
                }

                wind = ((rand.Next(5) / (float)10) + .25f) * windMagnitude;
            }

            for (int i = 0; i < gameObjects.Count; i++)
            {
                if (!gameObjects[i].remove)
                {
                    if (gameObjects[i].type == GameObject.Types.Enemy)
                    {
                        gameObjects[i].Update(this, rand, player);
                    }
                    else
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
                    damageZones[i].Update(this, player);
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

        public void Draw(Random rand, SpriteBatch spriteBatch, GraphicsDevice graphics, Camera camera)
        {

            foreach (Point key in backgrounds.Keys)
            {
                spriteBatch.Draw(backgrounds[key], new Rectangle(key.X * 1024, key.Y * 1024, 1024, 1024), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1f);
            }

            for (int i = 0; i < gameObjects.Count; i++)
            {
                gameObjects[i].Draw(rand, spriteBatch, graphics, camera);
            }
            for (int i = 0; i < scraps.Count; i++)
            {
                scraps[i].Draw(rand, spriteBatch, graphics, camera);
            }
            for (int i = 0; i < particles.Count; i++)
            {
                particles[i].Draw(rand, spriteBatch, camera, 0.5f);
            }

            for (int i = 0; i < effectObjects.Count; i++)
            {
                effectObjects[i].Draw(rand, spriteBatch, camera);
            }
        }

        public void DrawLights(Random rand, SpriteBatch spriteBatch, Camera camera)
        {
            foreach (Point key in lightSources.Keys)
            {
                spriteBatch.Draw(lightSources[key], new Rectangle(key.X * 1024, key.Y * 1024, 1024, 1024), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1f);
            }
            for (int i = 0; i < gameObjects.Count; i++)
            {
                gameObjects[i].DrawLight(rand, spriteBatch, camera);
            }
            for (int i = 0; i < particles.Count; i++)
            {
                particles[i].DrawLight(rand, spriteBatch, camera);
            }
        }

        public void DrawCollisions(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            for (int i = 0; i < platforms.Count; i++)
            {
                platforms[i].Draw(graphicsDevice, spriteBatch);
                platforms[i].DrawID(spriteBatch);
            }
            for (int i = 0; i < particles.Count; i++)
            {
                //Game1.DrawLine(graphicsDevice, spriteBatch, particles[i].position, particles[i].position + new Vector2((float)Math.Cos(particles[i].angle) * 5, (float)Math.Sin(particles[i].angle) * 5), Color.Red);
                //Game1.DrawRectangle(graphicsDevice, spriteBatch, particles[i].Rectangle, Color.White);
            }

            //TEMP
            for (int i = 0; i < path.Count; i++)
            {
                Game1.DrawLine(graphicsDevice, spriteBatch, platforms[path[i].X].origin, platforms[platforms[path[i].X].nodes[path[i].Y].targetID].origin, Color.Red);
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
                }


                //Checks for collision with jump-through rectangles
                for (int j = 0; j < platforms.Count; j++)
                {
                    if (newRectangle.Intersects(platforms[j].rectangle) && platforms[j].type == Platform.Type.JumpThrough && !fallThrough)
                    {
                        if (oldRectangle.Bottom - 1 < platforms[j].rectangle.Y && movement.Y > 0)
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
                                //platformID = i;
                            }
                        }
                        else
                        {
                            if (Game1.LineIntersectsRect(new Vector2(slopes[i].rectangle.X, slopes[i].rectangle.Y), new Vector2(slopes[i].rectangle.X + slopes[i].rectangle.Width, slopes[i].rectangle.Y + slopes[i].rectangle.Height), new Rectangle((int)position.X + FloorAdv(movement.X), (int)position.Y + 1, rectangle.Width, rectangle.Height)))
                            {
                                onGround = true;
                                //platformID = i;
                            }
                        }
                    }

                }
            }
            position += new Vector2((int)Game1.FloorAdv(movement.X), (int)Game1.FloorAdv(movement.Y));
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
            if (sourceID == -1 || targetID == -1)
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
            if (sourceID == -1 || targetID == -1)
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
            Instruction instruction = new Instruction(new Point(sourceID, sourceID), Enemy.State.idle, Vector2.Zero, 0, true);
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

                        float Ym, Xm;

                        int xPos = node.xPos;

                        if (node.direction.X > 0)
                            xPos -= measurements.X;

                        if (node.connected)
                        {
                            instruction = new Instruction(instPlatform, Enemy.State.walking, new Vector2(movement.X * node.direction.X, 0), xPos, true);
                        }
                        else
                        {
                            if (node.direction.Y > 0)
                            {
                                //If the target platform is underneath the source

                                if ((maxX * g) / h > w)
                                {
                                    if (platforms[instPlatform.X].type == Platform.Type.JumpThrough)
                                    {
                                        instruction = new Instruction(instPlatform, Enemy.State.fall, Vector2.Zero, xPos, true);
                                    }
                                    else
                                    {
                                        //Xm = w / (float)(Math.Sqrt(h / g));
                                        Xm = 1;
                                        Ym = 0;

                                        //Return
                                        instruction = new Instruction(instPlatform, Enemy.State.walking, new Vector2(node.direction.X * Xm, 0), xPos + (int)(measurements.X * .8f) * node.direction.X, false);
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
                                        instruction = new Instruction(instPlatform, Enemy.State.jump, new Vector2(node.direction.X * Xm, Ym), xPos, false);

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
                                            instruction = new Instruction(instPlatform, Enemy.State.jump, new Vector2(node.direction.X * Xm, Ym), xPos, false);
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
                                        instruction = new Instruction(instPlatform, Enemy.State.jump, new Vector2(Xm, Ym), xPos, true);
                                    }
                                }
                                else
                                {
                                    //If the target platform is above the source and they do not intersect

                                    //x in these scenarios represents time and y represents y-/x-positions
                                    //y = x * (Ym + x * g)
                                    //y = x * Xm

                                    //JumpStrengthte
                                    Ym = (float)-Math.Sqrt(2 * h * g) - 1;

                                    //XMovement
                                    Xm = w / (-Ym / 2);

                                    if (Math.Abs(Xm) <= maxX)
                                    {
                                        //Return
                                        instruction = new Instruction(instPlatform, Enemy.State.jump, new Vector2(node.direction.X * Xm, Ym), xPos, false);

                                    }
                                    else
                                    {
                                        //You gotta jump higher to compensate
                                        //Jump based on the maximum X-speed
                                        Xm = maxX;
                                        Ym = ((-h * Xm) / w) - ((w * g) / Xm);

                                        if (Math.Abs(Ym) <= maxY)
                                        {
                                            //Return
                                            instruction = new Instruction(instPlatform, Enemy.State.jump, new Vector2(node.direction.X * Xm, Ym), xPos, false);
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

                        //Collision


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
    }
}