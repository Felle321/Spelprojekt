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
    class World
    {
        #region ScanColors
        //List of different colors representing different rectangles
        public static Color colorBasicCollision = new Color(0, 0, 0);
        public static Color colorJumpCollision = new Color(255, 0, 0);
        public static Color colorSlopeRight = new Color(0, 255, 0);
        public static Color colorSlopeLeft = new Color(0, 255, 1);
        //Doors (255:255:100-199)
        #endregion

        public static string locationLibrary = Environment.CurrentDirectory + @"..\..\..\..\..\..\..\..\..\Library\";

        public const int screenWidth = 1280;
        public const int screenHeight = 720;
        public static Vector2 gravity = new Vector2(0, 1f);
        //The size of each tile
        public const int tileOffsetX = 1024;
        public const int tileOffsetY = 1024;
        //Percentage
        public static float cameraSpeed = .1f;
        public static float cameraTorque = .2f;
        public static float cameraTorqueMaxSpeed = 1000;
        public static float cameraSlackFactor = 2;
        public const int playerWidth = 64;
        public const int playerHeight = 128;
        public const float playerHpFactor = 1f;
        //The lower the stronger 0 < x <= 1
        public const float airResitance = .95f;
        public const float friction = .90f;
        public const int maxWind = 3;
        public static int smokeSpreadRange = 16;
        public static int smokeSpread = 107;

        //SETTINGS
        public static bool displayDamageGameObjects = true;
        public static bool displayDamagePlayer = true;
        public static bool displayDamageEnemies = true;

        public static Color displayDamageGameObjectsColor = Color.White;
        public static Color displayDamagePlayerColor = Color.Red;
        public static Color displayDamageEnemiesColor = Color.Yellow;

        public static SpriteFont fontDamage;

        public static int levelSize = 50;
        public static int doorHeight = 240;

        //In Game Values
        public const int AttackCD = 10;
    }
}