using Serilog.Sinks.SystemConsole.Themes;

namespace Devisions.Web.Extensions;

public static class ConsoleColor
{
    public static AnsiConsoleTheme GetCustomTheme()
    {
        var customTheme = new AnsiConsoleTheme(
            new Dictionary<ConsoleThemeStyle, string>
            {
                [ConsoleThemeStyle.LevelError] = "\x1b[1;31m", // Красный
                [ConsoleThemeStyle.LevelWarning] = "\x1b[1;33m", // Желтый
                [ConsoleThemeStyle.LevelInformation] = "\x1b[1;32m", // Зеленый
                [ConsoleThemeStyle.LevelDebug] = "\x1b[1;37m", // БЕЛЫЙ для Debug
                [ConsoleThemeStyle.LevelVerbose] = "\x1b[1;90m", // Серый
                [ConsoleThemeStyle.Text] = "\x1b[0m", // Сброс цвета
            });
        return customTheme;
    }
}