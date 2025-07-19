using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RealTimeChatApp.Application.Interfaces;
using RealTimeChatApp.Infrastructure.Services;
using RealTimeChatApp.Infrastructure.Settings;
using Microsoft.Extensions.FileProviders;
using RealTimeChatApp.Domain.Entities;
using System.Text;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

using Microsoft.AspNetCore.SignalR;
using RealTimeChatApp.API.Hubs;
using StackExchange.Redis;
using Microsoft.AspNetCore.SignalR.StackExchangeRedis;


using RealTimeChatApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ Add controller support
builder.Services.AddControllers(); // REQUIRED for [ApiController]s to work

builder.Services.AddSignalR()
    .AddStackExchangeRedis("redis:6379", options =>
    {
        options.Configuration.ChannelPrefix = "ChatApp"; // Optional: helps namespace pub/sub
    });



// Redis Connection
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var config = ConfigurationOptions.Parse("redis:6379", true);
    return ConnectionMultiplexer.Connect(config);
});


// Register JwtSettings from appsettings.json
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Register AuthService
builder.Services.AddScoped<IAuthService, AuthService>();

// JWT Authentication
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
        };
    });

builder.Services.AddAuthorization();


// Swagger & OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Add JWT Auth to Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token like this: Bearer {your token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});



// Make sure it loads configuration from appsettings.json
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Bind CloudinarySettings
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("Cloudinary"));


    var cloudinarySettings = builder.Configuration.GetSection("Cloudinary").Get<CloudinarySettings>();


if (cloudinarySettings == null)
{
    Console.WriteLine("=== DEBUG: Dumping appsettings.json Cloudinary section ===");
    Console.WriteLine(builder.Configuration.GetSection("Cloudinary").Value ?? "Section is NULL");
    Console.WriteLine("CloudName: " + builder.Configuration.GetSection("Cloudinary:CloudName").Value);
    Console.WriteLine("ApiKey: " + builder.Configuration.GetSection("Cloudinary:ApiKey").Value);
    Console.WriteLine("ApiSecret: " + builder.Configuration.GetSection("Cloudinary:ApiSecret").Value);
}


var account = new Account(
    cloudinarySettings.CloudName,
    cloudinarySettings.ApiKey,
    cloudinarySettings.ApiSecret);

var cloudinary = new Cloudinary(account);
builder.Services.AddSingleton(cloudinary);



var app = builder.Build();
app.UseStaticFiles(); // enable serving wwwroot files

var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");

if (!Directory.Exists(uploadPath))
{
    Directory.CreateDirectory(uploadPath);
}


app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles")),
    RequestPath = "/UploadedFiles"
});


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed(_ => true); // Replace with allowed frontend domain in production
    });
});

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

// ✅ Map controller endpoints
app.MapControllers(); // 🔧 This enables AuthController, etc.
app.MapHub<ChatHub>("/chat");

// Optional: Keep test WeatherForecast endpoint
app.MapGet("/weatherforecast", () =>
{
    var summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast(
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        )).ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();




app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
