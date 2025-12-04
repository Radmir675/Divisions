using Serilog;

namespace Devisions.Web.Extensions;

public static class LoggingExtension
{
    public static void AddLogging(this WebApplicationBuilder app)
    {
        app.Host.UseSerilog((context, configuration) =>
        {
            configuration.ReadFrom.Configuration(context.Configuration);
            configuration.WriteTo.Console(
                theme: ConsoleColor.GetCustomTheme(),
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");
        });
    }
}