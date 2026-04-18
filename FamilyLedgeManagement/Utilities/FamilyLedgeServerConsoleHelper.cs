namespace FamilyLedgeManagement.Utilities
{
    public class FamilyLedgeServerConsoleHelper
    {
        public static void GreenLog(string message)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{message} \r\n ");
            Console.ForegroundColor = oldColor;
        }

        public static void RedLog(string msg)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{msg} \r\n ");
            Console.ForegroundColor = oldColor;
        }

        public static void DarkYellowLog(string msg)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"{msg} \r\n ");
            Console.ForegroundColor = oldColor;
        }

        public static void DarkGreenLog(string msg)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($"{msg} \r\n ");
            Console.ForegroundColor = oldColor;
        }
    }
}
