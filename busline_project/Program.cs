using Microsoft.EntityFrameworkCore;
using busline_project.Data;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Sử dụng cấu hình mặc định của ASP.NET (launchSettings / ASPNETCORE_URLS)
// Nếu bạn muốn override cho Docker, set ASPNETCORE_URLS hoặc giữ Dockerfile ENV.

var connectionString = ResolveConnectionString(builder.Configuration, builder.Environment);

// Configure JWT from configuration
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection.GetValue<string>("Key") ?? string.Empty;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtKey ?? "")),
        ValidateIssuer = true,
        ValidIssuer = jwtSection.GetValue<string>("Issuer") ?? "busline",
        ValidateAudience = true,
        ValidAudience = jwtSection.GetValue<string>("Audience") ?? "busline_clients",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30)
    };
});

builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();

// Cấu hình Swagger chi tiết hơn
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Busline API", Version = "v1" });

    // Add JWT support in Swagger
    var jwtSecurityScheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\r\n\r\nExample: 'Bearer eyJhb...'",
        Reference = new Microsoft.OpenApi.Models.OpenApiReference
        {
            Id = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme,
            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString)
);

// 2. Cấu hình CORS: Cho phép cả localhost (dev) và IP VPS (prod)
builder.Services.AddCors(options =>
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin() // Cho phép tất cả trong lúc đang triển khai, sau này nên siết lại IP cụ thể
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

        // If RESET_DB=true (environment variable), drop the database and re-apply migrations.
        var resetDb = Environment.GetEnvironmentVariable("RESET_DB");
        if (!string.IsNullOrWhiteSpace(resetDb) && resetDb.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            // Delete existing database (use carefully in production)
            db.Database.EnsureDeleted();
            app.Logger.LogInformation("RESET_DB detected: existing database deleted.");
        }

        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        app.Logger.LogCritical(ex, "Database migration failed during startup.");
    }
}

// 3. SỬA QUAN TRỌNG: Luôn bật Swagger để bạn có thể test trên VPS
// Bạn có thể thêm kiểm tra biến môi trường riêng nếu muốn bảo mật
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Busline API V1");
    c.RoutePrefix = "swagger"; // Truy cập tại: http://IP/swagger
});

// Chỉ dùng HTTPS Redirection nếu bạn đã cài SSL/Nginx, nếu không sẽ bị lỗi kết nối
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapGet("/health", () => Results.Ok(new { status = "healthy", time = DateTime.UtcNow }));

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// --- Resolve connection string and helper to convert DATABASE_URL (e.g., Heroku) ---
static string ResolveConnectionString(IConfiguration configuration, IHostEnvironment environment)
{
    // Prefer explicit DATABASE_URL if provided (useful on Heroku-like platforms)
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (!string.IsNullOrWhiteSpace(databaseUrl))
    {
        return ConvertPostgresUrlToConnectionString(databaseUrl);
    }

    // Fallback to configuration (appsettings.json or secrets)
    var cfg = configuration.GetConnectionString("DefaultConnection");
    return cfg ?? string.Empty;
}

static string ConvertPostgresUrlToConnectionString(string connectionUrl)
{
    // Expected format: postgres://username:password@host:port/database
    try
    {
        var uri = new Uri(connectionUrl);
        var userInfo = uri.UserInfo.Split(':');
        var username = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
        var host = uri.Host;
        var port = uri.Port;
        var database = uri.AbsolutePath.TrimStart('/');

        var builder = new Npgsql.NpgsqlConnectionStringBuilder()
        {
            Host = host,
            Port = port,
            Username = username,
            Password = password,
            Database = database,
            SslMode = Npgsql.SslMode.Disable,
            TrustServerCertificate = true
        };

        return builder.ToString();
    }
    catch
    {
        // If parsing fails, return the original value to let Npgsql try parsing it itself
        return connectionUrl;
    }
}