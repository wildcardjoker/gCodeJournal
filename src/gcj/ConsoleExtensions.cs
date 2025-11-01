namespace gcj
{
    #region Using Directives
    using Spectre.Console;
    #endregion

    internal static class ConsoleExtensions
    {
        #region Fields
        private static readonly string[] EmojiWithExtraSpace = [Emoji.Known.Information, Emoji.Known.Warning];
        #endregion

        public static void DisplayConsoleMessageWithLeadingEmoji(this string message, string emoji)
        {
            var spacing = EmojiWithExtraSpace.Contains(emoji) ? "  " : " ";
            AnsiConsole.MarkupLineInterpolated($"{emoji}{spacing}{message}");
        }

        public static void DisplayInfoMessage(this string message) => message.DisplayConsoleMessageWithLeadingEmoji(Emoji.Known.Information);
    }
}