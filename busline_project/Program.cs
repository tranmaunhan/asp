using Microsoft.EntityFrameworkCore;
using busline_project.Data;
var builder = WebApplication.CreateBuilder(args);

var connectionString = ResolveConnectionString(builder.Configuration, builder.Environment);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        connectionString
    )
);

builder.Services.AddCors(options =>
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:5175")
              .AllowAnyHeader()
              .AllowAnyMethod()
    )
);

var app = builder.Build();

// Auto-migrate database
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        app.Logger.LogCritical(ex, "Database migration failed during startup.");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsEnvironment("Production"))
{
    app.UseHttpsRedirection();
}

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();

static string ResolveConnectionString(IConfiguration configuration, IHostEnvironment environment)
{
    var configured = configuration.GetConnectionString("DefaultConnection");
    var databaseUrl = configuration["DATABASE_URL"];
    var candidate = !string.IsNullOrWhiteSpace(databaseUrl) ? databaseUrl : configured;

    if (string.IsNullOrWhiteSpace(candidate))
    {
        throw new InvalidOperationException("Missing database connection string. Set ConnectionStrings__DefaultConnection or DATABASE_URL.");
    }

    var normalized = candidate.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
                     candidate.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)
        ? ConvertPostgresUrlToConnectionString(candidate)
        : candidate;

    if (environment.IsProduction() &&
        (normalized.Contains("Host=localhost", StringComparison.OrdinalIgnoreCase) ||
         normalized.Contains("Host=127.0.0.1", StringComparison.OrdinalIgnoreCase)))
    {
        throw new InvalidOperationException(
            "Production deployment is using a localhost PostgreSQL connection string. Set ConnectionStrings__DefaultConnection in Render Environment.");
    }

    return normalized;
}

static string ConvertPostgresUrlToConnectionString(string connectionUrl)
{
    var uri = new Uri(connectionUrl);
    var userInfoParts = uri.UserInfo.Split(':', 2);

    if (userInfoParts.Length != 2)
    {
        throw new InvalidOperationException("Invalid PostgreSQL URL format. Expected username and password.");
    }

    var database = uri.AbsolutePath.Trim('/');
    var username = Uri.UnescapeDataString(userInfoParts[0]);
    var password = Uri.UnescapeDataString(userInfoParts[1]);

    return $"Host={uri.Host};Port={uri.Port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
}
