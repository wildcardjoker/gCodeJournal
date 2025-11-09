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

        public static async Task<T?> GetEntitySelectionAsync<T>(this IEnumerable<T> choices) where T : class
        {
#pragma warning disable CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.

            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            var prompt = new SelectionPrompt<T>().Title($"Please select from the following {typeof(T).Name}")
                                                 .PageSize(10)
                                                 .MoreChoicesText("Scroll up/down for more choices")
                                                 .UseConverter(item => item is null ? "Back to menu" : item?.ToString() ?? "<unknown>")!.AddChoices(
                                                     new[] {(T?) null}.Concat(choices.Select(T? (c) => c)));
#pragma warning restore CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.

            var response = await AnsiConsole.PromptAsync(prompt).ConfigureAwait(false);
            return response;
        }
    }
}