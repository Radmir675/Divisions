using Devisions.Infrastructure.Postgres;
using Devisions.Web;
using Devisions.Web.Extensions;
using Devisions.Web.Middlewares;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<AppDbContext>(_ =>
    new AppDbContext(builder.Configuration.GetConnectionString("DefaultConnection")!));

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

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