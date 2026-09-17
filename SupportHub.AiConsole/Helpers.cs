namespace SupportHub.AiConsole;

/// <summary>
/// Used to implement helper methods for the SupportHub.AiConsole application.
/// </summary>
public static class Helpers
{
    public static void Heading(string text)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(text);
        Console.ResetColor();
    }

    public static void Usage(string ModelId, long?  inputTokenCount, long? outputTokenCount)
    {
        Console.WriteLine($"  [{ModelId} · {inputTokenCount} tokens in · {outputTokenCount} out]");
    }
}
