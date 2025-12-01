using PanierService.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// ===== REDIS CONFIGURATION =====
var redisConnection = Environment.GetEnvironmentVariable("REDIS_CONNECTION")
                      ?? builder.Configuration.GetConnectionString("Redis")
                      ?? "localhost:6379";

Console.WriteLine($"🔗 Redis Connection: {redisConnection}"); // ✅ Log pour debug

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
    options.InstanceName = "PanierService_"; // ✅ Préfixe pour les clés
});

// ===== SERVICES =====
builder.Services.AddScoped<IRedisService, RedisService>();
builder.Services.AddScoped<IPanierService, PanierServiceImpl>();

// ===== CORS =====
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
                     ?? new[] { "https://localhost:7139", "http://localhost:5015", "https://localhost:7193" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ===== CONTROLLERS & SWAGGER =====
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ===== MIDDLEWARE =====
app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// ✅ Health Check
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();