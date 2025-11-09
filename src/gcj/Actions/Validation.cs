namespace gcj
{
    #region Using Directives
    using Microsoft.Extensions.Logging;
    using Spectre.Console;
    #endregion

    public static partial class Program
    {
        private static async Task<string?> ValidateCustomerName(ILogger appLogger)
        {
            var customerName = await "customer's name".GetInputFromConsoleAsync().ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(customerName))
            {
                appLogger.LogError(Emoji.Known.Warning + "  Customer name cannot be empty");
            }

            return customerName;
        }
    }
}