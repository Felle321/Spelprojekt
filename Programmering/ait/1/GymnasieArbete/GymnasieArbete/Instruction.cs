using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace GymnasieArbete
{
    public class Instruction
    {
        public Point platform;
        public Enemy.State state;
        public Vector2 movement;
        public int xPos;
        public bool variableXPos;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="platform">Contains botch the source- and the targetplatform, in that order</param>
        /// <param name="state">State of the isntruction</param>
        /// <param name="movement">Movement assigned to the instruction</param>
        /// <param name="xPos">The preffered position to execute the instruction</param>
        /// <param name="variableXPos">True if the position to execute the instruction doesn't matter (as long as you're on the correct platform)</param>
        public Instruction(Point platform, Enemy.State state, Vector2 movement, int xPos, bool variableXPos)
        {
            this.platform = platform;
            this.state = state;
            this.movement = movement;
            this.xPos = xPos;
            this.variableXPos = variableXPos;
    }
    }
}
