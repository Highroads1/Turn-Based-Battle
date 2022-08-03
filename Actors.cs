using System;
using System.Collections.Generic;

namespace TheFinalBattle
{
    internal class Party //Container for list of characters and their type of party
    {
        public List<Character> Member { get; set; }
        public PartyType Partytype { get; }
        public Party(PartyType type)
        {
            Partytype = type;
            switch (Partytype) // Initializes party and included members based on type it was given in constructor argument
            {
                case PartyType.hero:
                    Member = new List<Character>();
                    Member.Add(new Player(25));
                    Member.Add(new VinFletcher(15));
                    break;
                case PartyType.villian:
                    Member = new List<Character>();
                    Member.Add(new Skeleton(2));
                    break;
                case PartyType.villian2:
                    Member = new List<Character>();
                    Member.Add(new StoneAmarok(4));
                    Member.Add(new StoneAmarok(4));
                    break;
                case PartyType.boss:
                    Member = new List<Character>();
                    Member.Add(new Boss(15));
                    break;
            }
            foreach (Character m in Member) // Subscription for individual member death event to react to
            {
                m.CharacterDeath += OnMemberDeath;
            }
        }
        public void OnMemberDeath(object memb, EventArgs args) // Reaction when individual member dies in a party
        {
            for (int i = 0; i < Member.Count; i++)
            {
                if (Member[i] == memb)
                {
                    Console.WriteLine($"{memb} was killed.");
                    Member[i].CharacterDeath -= OnMemberDeath;
                    Member.RemoveAt(i);
                }
            }
        }
        public override string ToString() // Overiddes object naming convention to make a distinct party description
        {
            string partyName = null;
            switch (Partytype)
            {
                case PartyType.hero:
                    partyName = "The Heroes";
                    break;
                case PartyType.villian:
                    partyName = "The Lone Skeleton";
                    break;
                case PartyType.villian2:
                    partyName = "The Stone Amoraks";
                    break;
                case PartyType.boss:
                    partyName = "The Uncoded One";
                    break;
            }
            return partyName;
        }
    }
    public class Character
    {
        public int HP { get; set; } // Active Health
        public int MaxHP { get; set; } // Maximum Possible Health
        public string Name { get; set; } // Individual Character Description 
        public List<string> Moveset { get; set; } // List of Moves possible for character
        public int AttackDMG { get; set; } // Base Damage that is modified based on action selection
        public int DefenceModifier { get; set; } = 0; // Damage Reduction Modifier (Currently only one character has this above zero)
        public virtual void DoNothing() { Console.WriteLine($"{this} does NOTHING!"); } // Action Method for staying idle and not effecting game state
        public virtual void Attack(Character target)
        {
            Random r = new Random();
            switch (this)
            {
                case Player: AttackDMG = 2; break;
                case Skeleton: AttackDMG = r.Next(2); break;
                case VinFletcher: AttackDMG = r.Next(2) * 3; break;
                case StoneAmarok: AttackDMG = r.Next(3); break;
                case Boss: AttackDMG = r.Next(4); break;
            }
            Console.WriteLine($"{this} uses {this.Moveset[1]} on {target} for {this.AttackDMG} possible damage!");
            target.HP -= Modifier(AttackDMG, target);
            Console.WriteLine($"{target} health is now {target.HP}/{target.MaxHP}.");
        } // Action Method for attacking select or random characters; Advances game state
        public int Modifier(int rawDMG, Character targ)
        {
            int finalDamage;
            if (rawDMG - targ.DefenceModifier <= 0) { finalDamage = 0; }
            else { finalDamage = rawDMG - targ.DefenceModifier; }
            if (targ.DefenceModifier > 0) { Console.WriteLine($"But {targ}'s armor has reduced damage by {targ.DefenceModifier}"); }
            return finalDamage;
        } // Uses DefenceModifier to calculate final damage using attacker damage and defender's reduction modifier
        public override string ToString()
        {
            return Name;
        } // Overidde of toString to return cleaner description of individual character
        protected virtual void OnCharacterDeath() { if (CharacterDeath != null) CharacterDeath(this, null); } // Event invoked when THIS individual character's health reaches 0
        public event EventHandler<EventArgs> CharacterDeath; // Event Delegate for Character Death
        public virtual void DeathCheck() { if (HP <= 0) OnCharacterDeath(); } // Check if character's health is zero

    } // Base Character Class for every other derived character
    internal class Player : Character
    {
        public Player(int hp)
        {
            HP = hp;
            MaxHP = HP;
            Name = PlayerName();
            Moveset = new List<string>() { "Do Nothing", "Punch" };
            AttackDMG = 1;
        }
        public string PlayerName()
        {
            Console.Write("What is the name of the player? ");
            return Console.ReadLine();
        }
    } // Player Character
    internal class VinFletcher : Character
    {
        public VinFletcher(int hp)
        {
            HP = hp;
            MaxHP = HP;
            Name = "Vin Fletcher";
            Moveset = new List<string> { "Do Nothing", "Quick Shot" };
            AttackDMG = 1;
        }
    } // Friendly NPC Character
    internal class Skeleton : Character
    {
        public Skeleton(int hp)
        {
            HP = hp;
            MaxHP = hp;
            Name = "Skeleton";
            Moveset = new List<string>() { "Do Nothing", "BONECRUNCH" };
            AttackDMG = 0;
        }
    } // Basic Enemy Character
    internal class StoneAmarok : Character
    {
        public StoneAmarok(int hp)
        {
            HP = hp;
            MaxHP = HP;
            Name = "Stone Amarok";
            Moveset = new List<string>() { "Do Nothing", "BITE" };
            AttackDMG = 1;
            DefenceModifier = 1;
        }
    } // Advanced Enemy Character
    internal class Boss : Character // Endgame Enemy Character
    {
        public Boss(int hp)
        {
            HP = hp;
            MaxHP = HP;
            Name = "The Uncoded One";
            Moveset = new List<string>() { "Do Nothing", "UNRAVELING" };
            AttackDMG = 1;
        }
    }

    public enum PartyType { hero, villian, villian2, boss } //The different Party Types the game can create and make characters for
}
