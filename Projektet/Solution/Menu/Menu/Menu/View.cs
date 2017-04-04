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

namespace Menu
{
    public abstract class View
    {
        bool visible, active;
        List<Button> buttons = new List<Button>();
        Sprite sprite;
        Vector2 position = Vector2.Zero;

        public View()
        {
            visible = false;
            active = false;
        }

        public virtual void Update(MouseState mouse, MouseState mousePrev, bool doubleClick)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].Update(mouse, mousePrev, doubleClick);
            }
        }

        public virtual void Draw(Random rand, SpriteBatch spriteBatch, Camera camera)
        {
            sprite.Draw(rand, spriteBatch, camera, position, SpriteEffects.None, 0f);

            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].Draw(rand, spriteBatch, camera);
            }
        }
    }
}
