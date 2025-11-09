namespace gcj
{
    #region Using Directives
    using Microsoft.Extensions.Logging;
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

        public static Task<string?> GetInputFromConsoleAsync(this    string promptMessage) => promptMessage.GetInputFromConsoleAsync<string?>(string.Empty);
        public static Task<T?>      GetInputFromConsoleAsync<T>(this string promptMessage) => promptMessage.GetInputFromConsoleAsync<T>(default);

        public static Task<T?> GetInputFromConsoleAsync<T>(this string promptMessage, T? defaultValue) =>
            AnsiConsole.PromptAsync(new TextPrompt<T?>($"Please enter the {promptMessage} (ENTER for none):").AllowEmpty().DefaultValue(defaultValue ?? default));

        public static Task<string?> GetMultiLineInputAsync(this string promptMessage)
        {
            AnsiConsole.MarkupLineInterpolated($"Please enter the {promptMessage} (empty line to finish):");

            return Task.Run<string?>(async () =>
                                     {
                                         var lines = new List<string>();
                                         while (true)
                                         {
                                             var line = await AnsiConsole.PromptAsync(new TextPrompt<string?>(">").AllowEmpty()).ConfigureAwait(false);

                                             //var line = AnsiConsole.Console.ReadLine();
                                             if (string.IsNullOrWhiteSpace(line)) // EOF or input closed
                                             {
                                                 break;
                                             }

                                             lines.Add(line);
                                         }

                                         return lines.Count == 0 ? null : string.Join(Environment.NewLine, lines);
                                     });
        }

        public static void LogReturnToMenu(this ILogger logger) => logger.LogInformation("Returning to menu");
    }
}