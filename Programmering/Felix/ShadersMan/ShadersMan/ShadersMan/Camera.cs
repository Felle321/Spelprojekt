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

namespace ShadersMan
{
    public class Camera
    {
        public float zoom; // Camera Zoom
        public Matrix transform; // Matrix Transform
        public Vector2 pos; // Camera Position
        public Vector2 posPrev; // Camera Position
        public Vector2 movement = new Vector2(0, 0);
        public Rectangle rectangle = new Rectangle(0, 0, 0, 0);
        protected float RotationTarget
        {
            get
            {
                if (movement.X > 0)
                    return rotationTarget + (Game1.GetAngle(new Vector2(-movement.X, movement.Y)) * movement.X / World.cameraTorqueMaxSpeed);
                else
                    return rotationTarget + (Game1.GetAngle(new Vector2(movement.X, movement.Y)) * movement.X / World.cameraTorqueMaxSpeed);
            }
            set
            {
                rotationTarget = value;
            }
        }
        public Vector2 Target
        {
            get
            {
                return target - movement;
            }
            set
            {
                target = value;
            }
        }
        protected Vector2 target;
        private float rotationTarget = 0;
        public float rotation = 0;
        private Vector2 shakeOffset = Vector2.Zero;

        public List<Vector2> shakes = new List<Vector2>();
        public List<int> shakeDurations = new List<int>();

        public void AddShake(Vector2 magnitude, int duration)
        {
            shakes.Add(magnitude);
            shakeDurations.Add(duration);
        }

        public void StopShake()
        {
            shakes = new List<Vector2>();
            shakeDurations = new List<int>();
        }

        public void Move(Vector2 newMove)
        {
            pos += newMove;
            target += newMove;
        }

        public void MoveSmooth(Vector2 newMove)
        {
            Target += newMove;
        }

        public Camera(Vector2 newPos)
        {
            zoom = 1;
            pos = newPos;
            posPrev = newPos;
            rotation = 0;
            Target = pos;
        }

        public void Update(Random rand)
        {
            if (Game1.GetDistance(pos, Target) < 1)
            {
                pos = Target;
            }
            else
            {
                movement.X = (Target.X - pos.X) * World.cameraSpeed;
                movement.Y = (Target.Y - pos.Y) * World.cameraSpeed;

                pos += movement;
            }

            if (Math.Abs(rotation - RotationTarget) < 0.001f)
            {
                rotation = RotationTarget;
            }
            else
            {
                rotation += (RotationTarget - rotation) * World.cameraTorque;
            }

            shakeOffset = Vector2.Zero;

            for (int i = 0; i < shakes.Count; i++)
            {
                shakeOffset += new Vector2(shakes[i].X * ((float)rand.NextDouble() * 2 - 1), shakes[i].Y * ((float)rand.NextDouble() * 2 - 1));
                shakeDurations[i]--;
                if (shakeDurations[i] <= 0)
                {
                    shakeDurations.RemoveAt(i);
                    shakes.RemoveAt(i);
                }
            }

            pos += shakeOffset;

            if (pos.X == float.NaN)
                pos.X = posPrev.X;
            if (pos.Y == float.NaN)
                pos.Y = posPrev.Y;

            rectangle = new Rectangle(Game1.CeilAdv(pos.X - zoom * World.screenWidth / 2) - 32, Game1.CeilAdv(pos.Y - zoom * World.screenHeight / 2) - 32, Game1.CeilAdv(World.screenWidth * zoom) + 64, Game1.CeilAdv(World.screenHeight * zoom) + 64);
            //rectangle = new Rectangle(Game1.CeilAdv(pos.X), Game1.CeilAdv(pos.Y), Game1.CeilAdv(World.screenWidth * zoom), Game1.CeilAdv(World.screenHeight * zoom));

            posPrev = pos;
        }

        public Matrix get_transformation(GraphicsDevice graphicsDevice)
        {
            transform =       // Thanks to o KB o for this solution
                Matrix.CreateTranslation(new Vector3(-pos.X, -pos.Y, 0)) *
                Matrix.CreateRotationZ(rotation) *
                Matrix.CreateScale(new Vector3(zoom, zoom, 1)) *
                Matrix.CreateTranslation(new Vector3(World.screenWidth * 0.5f, World.screenHeight * 0.5f, 0));
            return transform;
        }
    }
}
