using Serilog;
using Serilog.Events;

namespace Devisions.Web.Extensions;

public static class LoggingExtension
{
    public static void AddLogging(this WebApplicationBuilder app)
    {
        app.Host.UseSerilog((context, confuguration) =>
        {
            confuguration.ReadFrom.Configuration(context.Configuration)
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.Seq("http://localhost:5341", restrictedToMinimumLevel: LogEventLevel.Information)
                .WriteTo.Console(
                    theme: ConsoleColor.GetCustomTheme(),
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {NewLine}{Exception}");
        });
    }
}