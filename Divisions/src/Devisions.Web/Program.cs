using Devisions.Infrastructure.Postgres;
using Devisions.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<AppDbContext>(_ =>
    new AppDbContext(builder.Configuration.GetConnectionString("DefaultConnection")!));
builder.Services.AddProgramDependencies();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();