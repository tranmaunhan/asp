using Microsoft.EntityFrameworkCore;
using busline_project.Data;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Đảm bảo ứng dụng lắng nghe trên cổng 10000 (khớp với Docker của bạn)
builder.WebHost.UseUrls("http://+:10000");

var connectionString = ResolveConnectionString(builder.Configuration, builder.Environment);

builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();

// Cấu hình Swagger chi tiết hơn
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Busline API", Version = "v1" });
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

app.UseAuthorization();

app.MapControllers();

app.Run();

// --- Giữ nguyên các hàm ResolveConnectionString và ConvertPostgresUrlToConnectionString của bạn ---
static string ResolveConnectionString(IConfiguration configuration, IHostEnvironment environment) { /* ... giữ nguyên code cũ ... */ return ""; }
static string ConvertPostgresUrlToConnectionString(string connectionUrl) { /* ... giữ nguyên code cũ ... */ return ""; }