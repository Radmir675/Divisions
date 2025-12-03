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

var app = builder.Build();
app.UseExceptionMiddleware();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
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