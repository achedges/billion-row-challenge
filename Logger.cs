namespace BillionRowChallenge;

public static class Logger
{
    public static void Log(string message)
    {
        Console.WriteLine($"{DateTime.Now:u} {message}");
    }
}