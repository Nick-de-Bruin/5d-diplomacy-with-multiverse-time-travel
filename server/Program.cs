using Context;
using Factories;
using Mappers;
using Microsoft.EntityFrameworkCore;
using Repositories;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

switch (builder.Configuration["DatabaseProvider"])
{
    case "sqlite":
        builder.Services.AddDbContext<GameContext>(options => options
            .UseSqlite(builder.Configuration.GetConnectionString("Database"),
            x => x.MigrationsAssembly("SqliteMigrations")));
        break;
    case "sqlserver":
        builder.Services.AddDbContext<GameContext>(options => options
            .UseSqlServer(builder.Configuration.GetConnectionString("Database"),
            x => x.MigrationsAssembly("SqlServerMigrations")));
        break;
    default:
        throw new Exception("Unknown databse provider");
}

builder.Services.AddScoped<EntityMapper>();
builder.Services.AddScoped<ModelMapper>();
builder.Services.AddScoped<GameRepository>();
builder.Services.AddScoped<WorldRepository>();
builder.Services.AddScoped<MapFactory>();
builder.Services.AddScoped<DefaultWorldFactory>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(options => options
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<GameContext>();
    dbContext.Database.Migrate();
}

app.UseAuthorization();
app.MapControllers();
app.Run();
