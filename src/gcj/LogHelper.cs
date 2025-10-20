// gcj

namespace gcj;

#region Using Directives
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
#endregion

/// <summary>
///     Provides extension methods for logging messages with structured logging and
///     displaying them to the console with appropriate formatting and colors based on the log level.
/// </summary>
/// <remarks>
///     The methods in this class are extension methods for <see cref="ILogger" /> that both emit
///     a log entry using the configured logging pipeline and write the same message to the console
///     while temporarily changing the console foreground color according to the <see cref="LogLevel" />.
/// </remarks>
[UsedImplicitly]
public static class LogHelper
{
    /// <summary>
    ///     Logs a message at <see cref="LogLevel.Critical" /> and displays it to the console.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger" /> used to emit the structured log entry. Cannot be <c>null</c>.</param>
    /// <param name="messageTemplate">The message template (supports structured logging placeholders). Cannot be <c>null</c>.</param>
    /// <param name="args">
    ///     The optional structured logging arguments that correspond to placeholders in
    ///     <paramref name="messageTemplate" />.
    /// </param>
    [UsedImplicitly]
    public static void LogAndDisplayToConsoleCritical(this ILogger logger, string messageTemplate, params object?[] args) =>
        logger.LogAndDisplayToConsole(LogLevel.Critical, messageTemplate, args);

    /// <summary>
    ///     Logs a message at <see cref="LogLevel.Debug" /> and displays it to the console.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger" /> used to emit the structured log entry. Cannot be <c>null</c>.</param>
    /// <param name="messageTemplate">The message template (supports structured logging placeholders). Cannot be <c>null</c>.</param>
    /// <param name="args">
    ///     The optional structured logging arguments that correspond to placeholders in
    ///     <paramref name="messageTemplate" />.
    /// </param>
    [UsedImplicitly]
    public static void LogAndDisplayToConsoleDebug(this ILogger logger, string messageTemplate, params object?[] args) =>
        logger.LogAndDisplayToConsole(LogLevel.Debug, messageTemplate, args);

    /// <summary>
    ///     Logs a message at <see cref="LogLevel.Error" /> and displays it to the console.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger" /> used to emit the structured log entry. Cannot be <c>null</c>.</param>
    /// <param name="messageTemplate">The message template (supports structured logging placeholders). Cannot be <c>null</c>.</param>
    /// <param name="args">
    ///     The optional structured logging arguments that correspond to placeholders in
    ///     <paramref name="messageTemplate" />.
    /// </param>
    [UsedImplicitly]
    public static void LogAndDisplayToConsoleError(this ILogger logger, string messageTemplate, params object?[] args) =>
        logger.LogAndDisplayToConsole(LogLevel.Error, messageTemplate, args);

    /// <summary>
    ///     Logs a message at <see cref="LogLevel.Information" /> and displays it to the console.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger" /> used to emit the structured log entry. Cannot be <c>null</c>.</param>
    /// <param name="messageTemplate">The message template (supports structured logging placeholders). Cannot be <c>null</c>.</param>
    /// <param name="args">
    ///     The optional structured logging arguments that correspond to placeholders in
    ///     <paramref name="messageTemplate" />.
    /// </param>
    [UsedImplicitly]
    public static void LogAndDisplayToConsoleInfo(this ILogger logger, string messageTemplate, params object?[] args) =>
        logger.LogAndDisplayToConsole(LogLevel.Information, messageTemplate, args);

    /// <summary>
    ///     Logs a message at <see cref="LogLevel.Trace" /> and displays it to the console.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger" /> used to emit the structured log entry. Cannot be <c>null</c>.</param>
    /// <param name="messageTemplate">The message template (supports structured logging placeholders). Cannot be <c>null</c>.</param>
    /// <param name="args">
    ///     The optional structured logging arguments that correspond to placeholders in
    ///     <paramref name="messageTemplate" />.
    /// </param>
    [UsedImplicitly]
    public static void LogAndDisplayToConsoleTrace(this ILogger logger, string messageTemplate, params object?[] args) =>
        logger.LogAndDisplayToConsole(LogLevel.Trace, messageTemplate, args);

    /// <summary>
    ///     Logs a message at <see cref="LogLevel.Warning" /> and displays it to the console.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger" /> used to emit the structured log entry. Cannot be <c>null</c>.</param>
    /// <param name="messageTemplate">The message template (supports structured logging placeholders). Cannot be <c>null</c>.</param>
    /// <param name="args">
    ///     The optional structured logging arguments that correspond to placeholders in
    ///     <paramref name="messageTemplate" />.
    /// </param>
    [UsedImplicitly]
    public static void LogAndDisplayToConsoleWarning(this ILogger logger, string messageTemplate, params object?[] args) =>
        logger.LogAndDisplayToConsole(LogLevel.Warning, messageTemplate, args);

    // Convert named placeholders like {Path} -> positional {0}, {1}, ... preserving format specifiers
    private static string ConvertNamedToPositional(this string template)
    {
        var map  = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var next = 0;
        return Regex.Replace(
            template,
            @"\{([a-zA-Z0-9_]+)([^}]*)\}",
            m =>
            {
                var name = m.Groups[1].Value;
                if (!map.TryGetValue(name, out var idx))
                {
                    idx       = next++;
                    map[name] = idx;
                }

                var tail = m.Groups[2].Value; // may contain :format or ,alignment
                return "{" + idx + tail + "}";
            });
    }

    /// <summary>
    ///     Logs a message at the specified <see cref="LogLevel" /> using structured logging and
    ///     temporarily changes the console foreground colour while emitting the message.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger" /> used to emit the structured log entry. Cannot be <c>null</c>.</param>
    /// <param name="level">The <see cref="LogLevel" /> at which to log the message.</param>
    /// <param name="messageTemplate">
    ///     A message template that may contain structured logging placeholders. Cannot be
    ///     <c>null</c>.
    /// </param>
    /// <param name="args">
    ///     Optional structured logging arguments that correspond to placeholders in
    ///     <paramref name="messageTemplate" />.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="logger" /> or <paramref name="messageTemplate" />
    ///     is <c>null</c>.
    /// </exception>
    /// <remarks>
    ///     This method temporarily sets <see cref="Console.ForegroundColor" /> according to the provided
    ///     <paramref name="level" />.
    ///     The mapping used is:
    ///     - <see cref="LogLevel.Trace" />       => <see cref="ConsoleColor.DarkGray" />
    ///     - <see cref="LogLevel.Debug" />       => <see cref="ConsoleColor.Gray" />
    ///     - <see cref="LogLevel.Information" /> => <see cref="ConsoleColor.White" />
    ///     - <see cref="LogLevel.Warning" />     => <see cref="ConsoleColor.Yellow" />
    ///     - <see cref="LogLevel.Error" />       => <see cref="ConsoleColor.Red" />
    ///     - <see cref="LogLevel.Critical" />    => <see cref="ConsoleColor.Magenta" />
    ///     After logging the console color is restored to its original value even if an exception occurs.
    ///     The method uses the generic
    ///     <see
    ///         cref="Serilog.Log" />
    ///     overload (via <see cref="ILogger.Log" /> with an explicit <see cref="EventId" /> and the provided arguments) to
    ///     preserve structured logging semantics.
    /// </remarks>
    /// <example>
    ///     Example usage:
    ///     <code>
    /// logger.LogAndDisplayToConsole(LogLevel.Warning, "Could not find database {Path}", dbPath);
    /// </code>
    /// </example>
    private static void LogAndDisplayToConsole(this ILogger logger, LogLevel level, string messageTemplate, params object?[] args)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(messageTemplate);

        var originalColor = Console.ForegroundColor;
        try
        {
            Console.ForegroundColor = level switch
            {
                LogLevel.Trace       => ConsoleColor.DarkGray,
                LogLevel.Debug       => ConsoleColor.Gray,
                LogLevel.Information => ConsoleColor.White,
                LogLevel.Warning     => ConsoleColor.Yellow,
                LogLevel.Error       => ConsoleColor.Red,
                LogLevel.Critical    => ConsoleColor.Magenta,
                _                    => originalColor
            };

            // Use the generic Log extension to preserve structured logging arguments
            logger.Log(level, new EventId(0), null, messageTemplate.Trim(), args);
            Console.WriteLine(messageTemplate.ConvertNamedToPositional(), args);
        }
        finally
        {
            Console.ForegroundColor = originalColor;
        }
    }
}