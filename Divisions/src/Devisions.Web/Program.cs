using Devisions.Infrastructure.Postgres.BackgroundServices;
using Devisions.Web;
using Devisions.Web.Extensions;
using Devisions.Web.Middlewares;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.AddDbContext();

builder.AddLogging();

builder.Services.AddProgramDependencies();
builder.Services.AddHostedService<DivisionCleanerBackgroundService>();

string environment = builder.Environment.EnvironmentName;
builder.Configuration.AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true);
// builder.Services.AddConfiguration(builder.Configuration);

var app = builder.Build();
app.UseExceptionMiddleware();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
    await app.AddMigrateAsync();

    if (args.Contains("--seeder"))
    {
        await app.Services.RunSeeding();
    }
}

app.MapControllers();

app.Run();

namespace Devisions.Web
{
    public partial class Program;
}