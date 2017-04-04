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
    public class DamageZone
    {
        public Rectangle rectangle;
        public Vector2 Position
        {
            get
            {
                return new Vector2(rectangle.X, rectangle.Y);
            }
            set
            {
                rectangle.X = Room.FloorAdv(value.X);
                rectangle.Y = Room.FloorAdv(value.Y);
            }
        }
        public float damage;
        int coolDown = 0;
        bool die = true;
        public bool linked = false;
        
        public int hp = 1;
        public int sourceID = 0;
        List<int> coolDowns = new List<int>();
        public List<int> IDs = new List<int>();

        public List<GameObject.Types> attackable = new List<GameObject.Types>();

        /// <summary>
        /// Creates a new DamageZone
        /// </summary>
        /// <param name="rectangle">The hitbox</param>
        /// <param name="damage">The amount of damage it will deal</param>
        /// <param name="coolDown">The time inbetween each damage (induvidual)</param>
        /// <param name="duration">The amount of time the damagezone will live for (if = 0 then it will live forever</param>
        /// <param name="ID">The ID of the gameobject creating the DamageZone</param>
        /// <param name="friendly">True = Friendly : False = Unfriendly</param>
        /// <param name="linked">Decides if the damagezone's life will be linked to the source</param>
        /// <param name="attackableTypes">A list of types the zone can damage</param>
        public DamageZone(Rectangle rectangle, float damage, int coolDown, int duration, int ID, bool friendly, bool linked, List<GameObject.Types> attackableTypes)
        {
            this.sourceID = ID;
            this.rectangle = rectangle;
            this.damage = damage;
            this.coolDown = coolDown;
            if (duration == 0)
            {
                die = false;
            }
            else
            {
                this.hp = duration;
            }
            this.linked = linked;
            this.attackable = attackableTypes;
        }

        public void Update(Room room, Player player)
        {
            if (hp > 0)
            {
                
                for (int i = 0; i < room.gameObjects.Count; i++)
                {
                    if (attackable.Contains(room.gameObjects[i].type))
                    {
                        if (rectangle.Intersects(room.gameObjects[i].GetHitBox()) && !IDs.Contains(i))
                        {
                            room.gameObjects[i].Damage(room, sourceID, damage);
                            IDs.Add(i);
                            coolDowns.Add(coolDown);
                        }
                    }
                }

                if(attackable.Contains(GameObject.Types.Player))
                {
                    if(rectangle.Intersects(player.GetHitBox()) && !IDs.Contains(player.ID))
                    {
                        player.Damage(room, sourceID, damage);
                        IDs.Add(player.ID);
                        coolDowns.Add(coolDown);
                    }
                }

                for (int i = 0; i < IDs.Count; i++)
                {
                    if (coolDowns[i] <= 0)
                    {
                        coolDowns.RemoveAt(i);
                        IDs.RemoveAt(i);
                    }
                    if (coolDowns.Count > 0) { coolDowns[i]--; }
                }
            }
            if (die)
            { hp--; } 
        }
    }
}
