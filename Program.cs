using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

var jwtKey = builder.Configuration["Jwt:Key"];
if (!string.IsNullOrEmpty(jwtKey))
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ValidateIssuer = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = builder.Configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });
    builder.Services.AddAuthorization();
}

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    if (builder.Environment.IsProduction())
        throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required. Set ConnectionStrings__DefaultConnection on Render.");
    connectionString = "Host=localhost;Port=5432;Database=my_cv_gen_api;Username=postgres;Password=postgres";
}
// Render (and others) often provide postgresql:// URL; Npgsql needs key=value format
else if (connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase) ||
         connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
{
    var uri = new Uri(connectionString);
    var userPass = uri.UserInfo?.Split(':', 2) ?? [];
    var user = userPass.Length > 0 ? Uri.UnescapeDataString(userPass[0]) : "";
    var pass = userPass.Length > 1 ? Uri.UnescapeDataString(userPass[1]) : "";
    var db = uri.AbsolutePath.TrimStart('/');
    var dbPort = uri.Port > 0 ? uri.Port : 5432;
    connectionString = $"Host={uri.Host};Port={dbPort};Database={db};Username={user};Password={pass};SSL Mode=Require;";
}
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var redisConnection = builder.Configuration["Redis:Configuration"] ?? builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConnection))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
    });
}

var app = builder.Build();

// Apply pending EF Core migrations (creates/updates tables on Render and elsewhere)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        logger.LogInformation("Applying database migrations...");
        db.Database.Migrate();
        logger.LogInformation("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to apply database migrations.");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();
