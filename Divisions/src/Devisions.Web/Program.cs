using Devisions.Infrastructure.Postgres.Database;
using Devisions.Web;
using Devisions.Web.Extensions;
using Devisions.Web.Middlewares;
using Serilog;
using Serilog.Events;
using ConsoleColor = Devisions.Web.Extensions.ConsoleColor;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<AppDbContext>(_ =>
    new AppDbContext(builder.Configuration.GetConnectionString("DefaultConnection")!));

builder.Host.UseSerilog((context, confuguration) =>
{
    confuguration.ReadFrom.Configuration(context.Configuration)
        .MinimumLevel.Information()
        .Enrich.FromLogContext()
        .WriteTo.Seq("http://localhost:5341", restrictedToMinimumLevel: LogEventLevel.Information)
        .WriteTo.Console(
            theme: ConsoleColor.GetCustomTheme(),
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} <s:{SourceContext}>{NewLine}{Exception}");
});

builder.Services.AddProgramDependencies();

var app = builder.Build();
app.UseExceptionMiddleware();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    await app.AddMigrateAsync();
}

app.MapControllers();

app.Run();