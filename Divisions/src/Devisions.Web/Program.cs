using Devisions.Web;
using Devisions.Web.Extensions;
using Devisions.Web.Middlewares;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.AddDbContext();

builder.AddLogging();

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

namespace Devisions.Web
{
    public partial class Program;
}