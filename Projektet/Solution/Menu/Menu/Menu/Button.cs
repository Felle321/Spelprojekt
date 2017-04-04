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
    public class Button
    {
        string id;
        public Rectangle rectangle;
        Sprite idle, hover, click, mark, inactive, currentSprite;
        bool visible;
        enum State
        {
            idle, hover, click, mark, inactive
        }
        State state = State.idle;
        State statePrev = State.idle;

        public Button(string id, Rectangle rectangle, Sprite idle, Sprite hover, Sprite click, Sprite mark, Sprite inactive, bool visible, bool active)
        {
            this.id = id;
            this.rectangle = rectangle;
            this.idle = idle;
            this.hover = hover;
            this.click = click;
            this.mark = mark;
            this.inactive = inactive;
            this.visible = visible;
            if (!active)
                state = State.inactive;
        }

        public void Update(MouseState mouse, MouseState mousePrev, bool doubleClick)
        {
            if (state != State.inactive && state != State.click)
            {
                if (doubleClick)
                    Click();
                else
                {
                    if (rectangle.Contains(new Point(mouse.X, mouse.Y)) && state == State.idle)
                        Enter();
                    else if (!rectangle.Contains(new Point(mouse.X, mouse.Y)) && state == State.hover)
                        Exit();

                    if (state == State.hover && mouse.LeftButton == ButtonState.Pressed && mousePrev.LeftButton == ButtonState.Released)
                    {
                        if (mark == null)
                            Click();
                        else
                            Mark();
                    }
                }
            }
            else if (state == State.click && currentSprite.animationEnd)
                ClickExecute();

            switch (id)
            {
                default:
                    break;
            }

            if(state != statePrev)
                switch (state)
                {
                    case State.idle:
                        currentSprite = idle;
                        break;
                    case State.hover:
                        currentSprite = hover;
                        break;
                    case State.click:
                        currentSprite = click;
                        break;
                    case State.mark:
                        currentSprite = mark;
                        break;
                    case State.inactive:
                        currentSprite = inactive;
                        break;
                    default:
                        break;
                }
        }

        public void Draw(Random rand, SpriteBatch spriteBatch, Camera camera)
        {
            if (visible)
                currentSprite.Draw(rand, spriteBatch, camera, rectangle.ToVector2(), SpriteEffects.None, 0f);
        }

        public void Click()
        {
            state = State.click;

            switch (id)
            {
                default:
                    break;
            }
        }

        public void ClickExecute()
        {
            switch (id)
            {
                default:
                    break;
            }
        }

        public void Mark()
        {
            state = State.mark;

            switch (id)
            {
                default:
                    break;
            }
        }

        public void Enter()
        {
            state = State.hover;

            switch (id)
            {
                default:
                    break;
            }
        }

        public void Exit()
        {
            state = State.idle;

            switch (id)
            {
                default:
                    break;
            }
        }
    }
}
