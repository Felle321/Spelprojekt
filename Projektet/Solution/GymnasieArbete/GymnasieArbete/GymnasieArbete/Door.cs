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

namespace GymnasieArbete
{
    public class Door
    {
        public Rectangle rectangle;
        public int targetRoom, targetDoor, sourceRoom, baseRoom, baseID;
        public bool active = false;
        public int direction = 0;
        /// <summary>
        /// Referring to the position of the source Tile
        /// </summary>
        public Point position = Point.Zero;

        public Door(Point tilePosition, Rectangle rectangle)
        {
            this.position = tilePosition;
            this.rectangle = rectangle;
        }

        public Door(Point tilePosition, Rectangle rectangle, int sourceRoom, int targetRoom, int targetDoor, int direction)
        {
            this.position = tilePosition;
            this.rectangle = rectangle;
            this.sourceRoom = sourceRoom;
            this.targetRoom = targetRoom;
            this.targetDoor = targetDoor;
            this.direction = direction;
        }

        public void Draw(Point offset, Random rand, SpriteBatch spriteBatch, Camera camera)
        {
            if (direction > 0)
            {
                SpriteHandler.Draw("door_lamp", rand, spriteBatch, camera, new Vector2(offset.X + rectangle.Right - SpriteHandler.sprites["door_lamp"].width, offset.Y + rectangle.Y - 26), SpriteEffects.None, 0f);
            }
            else
                SpriteHandler.Draw("door_lamp", rand, spriteBatch, camera, new Vector2(offset.X + rectangle.X, offset.Y + rectangle.Y - 26), SpriteEffects.FlipHorizontally, 0f);
        }

        public void DrawLight(Point offset, Random rand, SpriteBatch spriteBatch, Camera camera)
        {
            if (direction > 0)
            {
                SpriteHandler.Draw("door_light", rand, spriteBatch, camera, new Vector2(offset.X + rectangle.Right - SpriteHandler.sprites["door_light"].width, offset.Y + rectangle.Y - 60), SpriteEffects.None, 0f);
                SpriteHandler.Draw("door_lampLight", rand, spriteBatch, camera, new Vector2(offset.X + rectangle.Right - SpriteHandler.sprites["door_lampLight"].width, offset.Y + rectangle.Y - 60), SpriteEffects.None, 0f);
            }
            else
            {
                SpriteHandler.Draw("door_light", rand, spriteBatch, camera, new Vector2(offset.X + rectangle.X, offset.Y + rectangle.Y - 60), SpriteEffects.FlipHorizontally, 0f);
                SpriteHandler.Draw("door_lampLight", rand, spriteBatch, camera, new Vector2(offset.X + rectangle.X, offset.Y + rectangle.Y - 60), SpriteEffects.FlipHorizontally, 0f);
            }
        }
    }
}
