using Microsoft.EntityFrameworkCore;
using my_cv_gen_api.Data;
using my_cv_gen_api.Repositories;
using my_cv_gen_api.Services;

// Use PORT from Render (or default 8080 for local/Docker)
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
var urls = $"http://0.0.0.0:{port}";

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls(urls);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));
}

var redisConnection = builder.Configuration["Redis:Configuration"] ?? builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConnection))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
    });
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();
