using System;

using System.Linq;

namespace TheFinalBattle
{
    internal class Command // Character actions
    {
        public Command(Character commandAttacker) // Action for character staying idle
        {
            commandAttacker.DoNothing();
        }
        public Command(Character commandAttacker, Party commandDefendingParty, bool auto) // Action when character attacks; and if attacker auto-targets or not
        {
            if (!commandDefendingParty.Member.Any()) { Console.WriteLine("There are no more targets."); } // Detects if there are zero party members on defending party
            else
            {
                if (auto == false) // If NPC enemy chooses random target
                {
                    Random r = new Random();
                    try
                    {
                        Character commandTarget = commandDefendingParty?.Member?[r.Next(0, commandDefendingParty.Member.Count)];
                        commandAttacker?.Attack(commandTarget);
                        commandTarget?.DeathCheck(); // Check if attacked target is now dead immediately after attacking
                    }
                    catch (ArgumentOutOfRangeException) // Catches error if defending side has no party members in list to attack
                    {
                        return; // returns to game loop to proceed to next attacker's turn
                    }
                }
                else // If Player has to specify a target
                {
                    Console.WriteLine("Choose a Target");
                    int targetNumber = 0; // Initialization of target index
                    foreach (Character c in commandDefendingParty.Member) // Lists out current defending party roster
                    {
                        targetNumber++;
                        Console.WriteLine($"{targetNumber} for {c.Name}");
                    }
                    Character commandTarget = commandDefendingParty.Member[Utility.GetTextToInt(commandDefendingParty.Member.Count) - 1]; // Gets user input on target selection
                    if (commandAttacker == null) { return; }
                    commandAttacker.Attack(commandTarget);
                    commandTarget.DeathCheck();
                }
            }
        }
    }
}
