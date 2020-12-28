// game-test (c) by mjt
using System;
using System.Collections.Generic;

namespace GameTest
{
    public class Item
    {
        public string Name;
        public int Weight = 0;

    }

    public class Armor : Item
    {
        public static Armor Fists = new Armor("Fists", 0, 0.1f, 0.1f);

        public float Attack = 0.1f, Def = 0.1f;

        public Armor(string name, int weight, float attack, float def)
        {
            Name = name;
            Weight = weight;
            Attack = attack;
            Def = def;
        }
    }

    public class Actor : Movable
    {
        public string Name = "";
        public float Energy = 100;  // max energy 100
        public float Weight = 80;   // 70-120
        public float Strength = 1f;
        public float Skill = 1f;
        public float Speed = 0.05f;
        public float Luck = 5;
        float hitTime = 1;

        public List<Item> Items = new List<Item>();
        public Armor CurrentArmor = Armor.Fists;
        public Animation Anim;

        string[] nameParts = { "ka", "me", "si", "pil", "te", "ju", "er", "al", "mo", "lo", "ta", "vi", "vit", "kul", "he", "tu", "li", "me", "la", "pas", "ru", "lu", "ma", "ka", "ho" };

        public Actor()
        {
            Anim = new Animation(this);
            int tavut = BaseGame.rnd.Next(3) + 2;
            for (int q = 0; q < tavut; q++) Name += nameParts[BaseGame.rnd.Next(nameParts.Length)];
            Name = Name.Substring(0, 1).ToUpper() + Name.Substring(1, Name.Length - 1);
        }

        /// <summary>
        /// kun this.Actor lyö enemyä
        /// </summary>
        /// <param name="enemy"></param>
        public string Hit(Actor enemy)
        {
            string str = "";
            // jos actor voi jo lyödä
            if (hitTime >= 1)
            {
                hitTime = 0;

                str = "You attacked " + enemy.Name + " and ";

                // hit or miss
                if (BaseGame.rnd.Next(10) < (int)Luck)
                {
                    Skill += 0.5f;

                    float hit = (Weight * 0.2f) + ((1 - enemy.CurrentArmor.Def) * CurrentArmor.Attack * Skill);
                    enemy.Energy -= hit;
                    if (enemy.Energy < 0) enemy.Energy = 0;

                    str += "HIT!";

#if DEBUG
                    Console.WriteLine("hit=" + hit + "  energy=" + enemy.Energy);
#endif
                }
                else str += "missed!";
            }
            return str;
        }

        /// <summary>
        /// tietyt arvot palaa alkuperäisiin arvoihin pikkuhiljaa (ellei actori ole kuollut)
        /// </summary>
        public void Update(float time)
        {
            if (Energy <= 0) return;

            hitTime += Speed * (121 - Weight) * time;

            //if (Energy < 100) Energy += 0.01f;

            if (Luck < 5) Luck += 0.01f;
            else if (Luck > 5) Luck -= 0.01f;
        }

    }
}
