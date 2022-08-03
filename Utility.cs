using System;

namespace TheFinalBattle
{
    internal static class Utility // Custom functionality
    {
        public static int GetTextToInt(int max) // Custom String to Integer input method that also takes selection option population into consideration when detecting errors
        {
            int userInput;
            try
            {
                userInput = Convert.ToInt32(Console.ReadLine());
                if (userInput <= 0 || userInput > max)
                {
                    throw new FormatException();
                }
            }
            catch (FormatException) { Console.WriteLine("Invalid entry, try again..."); return GetTextToInt(max); }
            return userInput;
        }
    }
}
