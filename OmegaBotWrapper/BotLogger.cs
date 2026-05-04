namespace OmegaBotWrapper
{
    internal static class BotLogger
    {
        public static void Log(int botId, string state, string message) =>
            Console.WriteLine($"[Bot {botId}] [{state}] {message}");

        public static void Warn(int botId, string message) =>
            Console.WriteLine($"[Bot {botId}] [WARN] {message}");

        public static void Error(int botId, string message, Exception? ex = null)
        {
            Console.WriteLine($"[Bot {botId}] [ERROR] {message}");
            if (ex != null)
                Console.WriteLine($"[Bot {botId}] [ERROR] {ex}");
        }
    }
}
