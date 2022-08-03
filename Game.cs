using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace TheFinalBattle
{
    internal class Game
    {
        public bool IsGameWon { get; set; } = false; //Condition to proceed beyond individual battle loops
        public Party WinningParty { get; set; } = null; //Surviving party from most recent battle
        public Party LosingParty { get; set; } = null; //Defeated party from most recent battle
        public GameType Gametype { get; set; } //Enumerator dictating the mode of gameplay, set at GameSetup() method, used during Contiuation() method
        public void GameSetup() //Sets gamemode and initial parties
        {
            Party heroParty; //Initializer for the first party (Not necessarily the actual hero party)
            Party villianParty; //Initializer for the first enemy party
            Console.Write("Select a Game Mode:\n1 for PvM | 2 for PvP | 3 for MvM:  ");
            switch (Utility.GetTextToInt(3)) //Gets string to int entry using custom utility that mitigates user error
            {
                case 1: //Player VS Monsters (Normal Play)
                    Gametype = GameType.PVM;
                    heroParty = new(PartyType.hero);
                    villianParty = new(PartyType.villian);
                    TheGame(heroParty, villianParty);
                    break;
                case 2: //Player VS Player (Full Manual Play)
                    Gametype = GameType.PVP;
                    heroParty = new(PartyType.hero);
                    villianParty = new(PartyType.villian);
                    TheGame(heroParty, villianParty);
                    break;
                case 3: //Monster VS Monster (Auto Play)
                    Gametype = GameType.MVM;
                    heroParty = new(PartyType.villian); // special case where hero party is not present and monsters simply fight eachother
                    villianParty = new(PartyType.villian);
                    TheGame(heroParty, villianParty);
                    break;
            }
            Console.WriteLine();
        }
        public void TheGame(Party hero, Party villian) // Main Gameplay Method that contains round loops and detects win/loss conditions
        {
            Console.Clear();
            List<Party> partyList = new List<Party> { hero, villian }; // Creation of list of the two contending parties
            Battle battle = new(partyList); // Battle instance of contending parties that will later contain winner/loser
            Display display = new(hero, villian); // User interface that displays party population and health data
            battle.PartyDefeat += OnBattleVictory; //Subscription to detected defeat and reaction method
            Console.WriteLine();
            do
            {
                for (int i = 0; i < battle.Parties.Count; i++)
                {
                    for (int j = 0; j < battle.Parties[i].Member.Count; j++)
                    {
                        bool isAuto = false;
                        display.DisplayGame();
                        Character attacker = battle.Parties[i].Member[j];
                        Party defendingTeam = battle.Parties[GetOtherTeam(i, battle)];
                        Console.WriteLine($"It is {attacker}'s turn...");
                        if (attacker is Player) { isAuto = true; }
                        else { isAuto = false; }
                        GetChoice(attacker, defendingTeam, isAuto);
                        Thread.Sleep(2000);
                        battle.DefeatCheck();
                        Thread.Sleep(2000);
                        Console.WriteLine();
                    }
                }
            } while (IsGameWon == false);
            Console.WriteLine("The battle has ended");
            AfterBattle();
        }
        public static int GetOtherTeam(int currentTeam, Battle currentBattle) // Detects opposite party when active party's turn is active
        {
            int otherTeam;
            if (currentTeam + 1 >= currentBattle.Parties.Count)
            {
                otherTeam = 0;
            }
            else
            {
                otherTeam = currentTeam + 1;
            }
            return otherTeam;
        }
        public bool GetIsPlayer(int currentPlayer)
        {
            if (currentPlayer == 0) { return true; }
            else { return false; }
        } // Detects first character in game
        public void GetChoice(Character currentAttacker, Party opposingTeam, bool isAuto) // Method for determining main action a character from a party invokes, including attacks and remaining idle.
        {
            if (isAuto == true) // Will allow for manual input if enabled; for Player actions
            {
                int moveNumber = 0;
                foreach (string s in currentAttacker.Moveset)
                {
                    moveNumber++;
                    Console.WriteLine($"Press {moveNumber} to {s}.");
                }
                int MoveChoice = Utility.GetTextToInt(currentAttacker.Moveset.Count);
                switch (MoveChoice)
                {
                    case 1:
                        new Command(currentAttacker); //do nothing action
                        break;
                    case 2:
                        new Command(currentAttacker, opposingTeam, isAuto); //attack action
                        break;
                }
            }
            else // Auto-selection for NPC Actions
            {
                Random r = new Random();
                int MoveChoice = r.Next(1, currentAttacker.Moveset.Count + 1);
                switch (MoveChoice)
                {
                    case 1:
                        new Command(currentAttacker); //do nothing action
                        break;
                    case 2:
                        new Command(currentAttacker, opposingTeam, isAuto); //attack action
                        break;
                }
            }

        }
        public void OnBattleVictory(object theBattleResult, BattleWinnerArgs args) // Reaction to party losing all members; Sets Winners/Losers and changes the gamestate to escape main game loop
        {
            IsGameWon = true;
            WinningParty = args.Winners;
            LosingParty = args.Losers;
        }
        void AfterBattle() // Method for deciding if the game will continue with new enemies, or end based on the loser of a battle
        {
            Party nextParty;
            switch (LosingParty.Partytype)
            {
                case PartyType.hero: LosingCeremony(); break; // Lose condition
                case PartyType.villian: Continuation(nextParty = new(PartyType.villian2)); break; // Battle continuation
                case PartyType.villian2: Continuation(nextParty = new(PartyType.boss)); break; // Battle continuation
                case PartyType.boss: WinningCeremony(); break; // Win condition 
            }
        }
        void Continuation(Party newParty) // Battle result dialogue and creating a new Battle instance without initial set-up
        {
            IsGameWon = false;
            Console.WriteLine($"{WinningParty} has defeated {LosingParty}");
            Thread.Sleep(2000);
            Console.WriteLine("A NEW ENEMY GROUP APPROACHES!!!\nPress any key to continue...");
            Console.ReadKey();
            TheGame(WinningParty, newParty);
        }
        public void WinningCeremony()
        {
            Console.WriteLine($"{WinningParty} has defeated {LosingParty}!\nThe End!");
        } // Win condition result if Heroes or NPC Monsters win
        public void LosingCeremony() // Lose condition result if exlusive Heroes lose
        {
            Console.WriteLine("The heroes have lost.\nGame Over!");
        }
    }

    internal class Battle // Instance that holds Party population data and winner/loser results
    {
        public List<Party> Parties { get; set; }
        public Party BattleWinners { get; set; } = null;
        public Party BattleLosers { get; set; } = null;

        public event EventHandler<BattleWinnerArgs> PartyDefeat; // Event that triggers when a party is fully dead

        public Battle(List<Party> parties)
        {
            Parties = parties;
        }
        protected virtual void OnPartyDeath(Party winners, Party losers) { if (PartyDefeat != null) PartyDefeat(this, new BattleWinnerArgs { Winners = BattleWinners, Losers = BattleLosers }); } // Sets Battle Winners/Losers
        public void DefeatCheck() // Detects loss of a party when the party member list is empty (Characters would have been removed on individual death)
        {
            foreach (Party p in Parties)
            {
                if (!p.Member.Any()) { BattleLosers = p; }
            }
            if (BattleLosers != null)
            {
                foreach (Party p in Parties)
                {
                    if (p != BattleLosers) { BattleWinners = p; }
                }
                OnPartyDeath(BattleWinners, BattleLosers);
            }
        }
    }

    internal class Display
    {
        public Party TopParty { get; set; } // Party that is displayed on the top of the UI
        public Party BottomParty { get; set; } // Party that is displayed on the bottom of the UI

        public Display(Party heroParty, Party villianParty)
        {
            TopParty = heroParty;
            BottomParty = villianParty;
        }
        public void DisplayGame()
        {
            Console.Clear();
            Console.WriteLine("==============================Battle=============================");
            for (int i = 0; i < TopParty.Member.Count; i++)
            {
                Character topPlayer = TopParty.Member[i];
                Console.ForegroundColor = CharacterColor(topPlayer);
                Console.WriteLine($"{topPlayer}                              {topPlayer.HP}/{topPlayer.MaxHP}");
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("================================VS===============================");
            for (int i = 0; i < BottomParty.Member.Count; i++)
            {
                Character bottomPlayer = BottomParty.Member[i];
                Console.ForegroundColor = CharacterColor(bottomPlayer);
                Console.WriteLine($"{bottomPlayer.HP}/{bottomPlayer.MaxHP}                              {bottomPlayer}");
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("=================================================================");
            Console.WriteLine();
        } // Displays all live members of both contending parties, with health data
        public ConsoleColor CharacterColor(Character character) // Gives a custom color to each individual type of character
        {
            if (character is Player) { return ConsoleColor.Green; }
            else if (character is VinFletcher) { return ConsoleColor.Cyan; }
            else if (character is Skeleton) { return ConsoleColor.Yellow; }
            else if (character is StoneAmarok) { return ConsoleColor.Gray; }
            else if (character is Boss) { return ConsoleColor.Red; }
            else { return ConsoleColor.White; }
        }

    }
    internal class BattleWinnerArgs : EventArgs // Container for holding winner/loser data for an events argument
    {
        public Party Winners { get; set; }
        public Party Losers { get; set; }
    }

    public enum GameType { PVM, PVP, MVM } // The different Game Types the player can choose for separate game modes
}
