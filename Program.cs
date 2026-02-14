using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using my_cv_gen_api.Data;
using my_cv_gen_api.Repositories;
using my_cv_gen_api.Services;
using Npgsql;

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
builder.Services.AddScoped<IResumeRepository, ResumeRepository>();
builder.Services.AddScoped<ICvPdfService, CvPdfService>();
builder.Services.Configure<TailorOptions>(builder.Configuration.GetSection(TailorOptions.SectionName));
builder.Services.AddScoped<IResumeTailorService, GroqResumeTailorService>();

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
var dataSource = new NpgsqlDataSourceBuilder(connectionString).Build();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(dataSource, npgsql => npgsql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

var redisConnection = builder.Configuration["Redis:Configuration"] ?? builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConnection))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
    });
}

var app = builder.Build();

// Ensure database schema exists: if Users table is missing, create it (and related tables) via SQL so it works even when __EFMigrationsHistory already exists
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    var usersTableExists = db.Database.SqlQueryRaw<bool>(
        "SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'Users') AS \"Value\"").FirstOrDefault();
    if (!usersTableExists)
    {
        logger.LogWarning("Users table not found. Creating tables via SQL...");
        db.Database.ExecuteSqlRaw(@"CREATE TABLE ""Users"" (""Id"" serial PRIMARY KEY, ""FirstName"" varchar(100) NOT NULL, ""LastName"" varchar(100) NOT NULL, ""Email"" varchar(256) NOT NULL, ""PasswordHash"" bytea NOT NULL, ""PasswordSalt"" bytea NOT NULL, ""CreatedAt"" timestamptz NOT NULL, ""UpdatedAt"" timestamptz NOT NULL, ""IsActive"" boolean NOT NULL, ""PhoneNumber"" varchar(50) NULL, ""GitHubUrl"" varchar(500) NULL, ""Location"" varchar(200) NULL, ""Website"" varchar(500) NULL)");
        db.Database.ExecuteSqlRaw(@"CREATE TABLE ""Resumes"" (""Id"" serial PRIMARY KEY, ""UserId"" integer NOT NULL REFERENCES ""Users""(""Id"") ON DELETE CASCADE, ""Title"" varchar(200) NOT NULL, ""Description"" text NOT NULL, ""Skills"" jsonb NOT NULL DEFAULT '[]', ""CreatedAt"" timestamptz NOT NULL, ""UpdatedAt"" timestamptz NOT NULL, ""IsActive"" boolean NOT NULL DEFAULT true, ""ImageUrl"" varchar(500) NULL)");
        db.Database.ExecuteSqlRaw(@"CREATE INDEX ""IX_Resumes_UserId"" ON ""Resumes"" (""UserId"")");
        db.Database.ExecuteSqlRaw(@"CREATE TABLE ""Educations"" (""Id"" serial PRIMARY KEY, ""ResumeId"" integer NOT NULL REFERENCES ""Resumes""(""Id"") ON DELETE CASCADE, ""School"" varchar(200) NOT NULL, ""Degree"" varchar(100) NOT NULL, ""FieldOfStudy"" varchar(200) NOT NULL, ""StartDate"" timestamptz NOT NULL, ""EndDate"" timestamptz NULL)");
        db.Database.ExecuteSqlRaw(@"CREATE INDEX ""IX_Educations_ResumeId"" ON ""Educations"" (""ResumeId"")");
        db.Database.ExecuteSqlRaw(@"CREATE TABLE ""Languages"" (""Id"" serial PRIMARY KEY, ""ResumeId"" integer NOT NULL REFERENCES ""Resumes""(""Id"") ON DELETE CASCADE, ""Name"" varchar(100) NOT NULL, ""Level"" varchar(50) NOT NULL)");
        db.Database.ExecuteSqlRaw(@"CREATE INDEX ""IX_Languages_ResumeId"" ON ""Languages"" (""ResumeId"")");
        db.Database.ExecuteSqlRaw(@"CREATE TABLE ""Projects"" (""Id"" serial PRIMARY KEY, ""ResumeId"" integer NOT NULL REFERENCES ""Resumes""(""Id"") ON DELETE CASCADE, ""Title"" varchar(200) NOT NULL, ""Description"" text NOT NULL, ""Link"" varchar(500) NULL, ""CreatedAt"" timestamptz NOT NULL, ""UpdatedAt"" timestamptz NOT NULL)");
        db.Database.ExecuteSqlRaw(@"CREATE INDEX ""IX_Projects_ResumeId"" ON ""Projects"" (""ResumeId"")");
        db.Database.ExecuteSqlRaw(@"CREATE TABLE ""WorkExperiences"" (""Id"" serial PRIMARY KEY, ""ResumeId"" integer NOT NULL REFERENCES ""Resumes""(""Id"") ON DELETE CASCADE, ""Company"" varchar(200) NOT NULL, ""Position"" varchar(200) NOT NULL, ""Description"" text NOT NULL, ""StartDate"" timestamptz NOT NULL, ""EndDate"" timestamptz NULL, ""IsCurrent"" boolean NOT NULL DEFAULT false)");
        db.Database.ExecuteSqlRaw(@"CREATE INDEX ""IX_WorkExperiences_ResumeId"" ON ""WorkExperiences"" (""ResumeId"")");
        logger.LogInformation("Tables created successfully.");
    }
    else
    {
        // Existing DB: add new columns to Resumes if they were added to the model after the table was created
        var imageUrlExists = db.Database.SqlQueryRaw<bool>(
            "SELECT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'Resumes' AND column_name = 'ImageUrl') AS \"Value\"").FirstOrDefault();
        if (!imageUrlExists)
        {
            logger.LogInformation("Adding ImageUrl column to Resumes table...");
            db.Database.ExecuteSqlRaw(@"ALTER TABLE ""Resumes"" ADD COLUMN ""ImageUrl"" varchar(500) NULL");
        }
        var isActiveExists = db.Database.SqlQueryRaw<bool>(
            "SELECT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'Resumes' AND column_name = 'IsActive') AS \"Value\"").FirstOrDefault();
        if (!isActiveExists)
        {
            logger.LogInformation("Adding IsActive column to Resumes table...");
            db.Database.ExecuteSqlRaw(@"ALTER TABLE ""Resumes"" ADD COLUMN ""IsActive"" boolean NOT NULL DEFAULT true");
        }
        
        // Add optional User profile columns if missing
        var userPhoneNumberExists = db.Database.SqlQueryRaw<bool>(
            "SELECT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'Users' AND column_name = 'PhoneNumber') AS \"Value\"").FirstOrDefault();
        if (!userPhoneNumberExists)
        {
            logger.LogInformation("Adding PhoneNumber, GitHubUrl, Location, Website columns to Users table...");
            db.Database.ExecuteSqlRaw(@"ALTER TABLE ""Users"" ADD COLUMN ""PhoneNumber"" varchar(50) NULL");
            db.Database.ExecuteSqlRaw(@"ALTER TABLE ""Users"" ADD COLUMN ""GitHubUrl"" varchar(500) NULL");
            db.Database.ExecuteSqlRaw(@"ALTER TABLE ""Users"" ADD COLUMN ""Location"" varchar(200) NULL");
            db.Database.ExecuteSqlRaw(@"ALTER TABLE ""Users"" ADD COLUMN ""Website"" varchar(500) NULL");
        }

        // Check if WorkExperiences table exists and add IsCurrent column if missing
        var workExperiencesTableExists = db.Database.SqlQueryRaw<bool>(
            "SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'WorkExperiences') AS \"Value\"").FirstOrDefault();
        if (workExperiencesTableExists)
        {
            var isCurrentExists = db.Database.SqlQueryRaw<bool>(
                "SELECT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'WorkExperiences' AND column_name = 'IsCurrent') AS \"Value\"").FirstOrDefault();
            if (!isCurrentExists)
            {
                logger.LogInformation("Adding IsCurrent column to WorkExperiences table...");
                db.Database.ExecuteSqlRaw(@"ALTER TABLE ""WorkExperiences"" ADD COLUMN ""IsCurrent"" boolean NOT NULL DEFAULT false");
            }
        }
    }

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
