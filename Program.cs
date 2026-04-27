using CollabSpace.Data;
using CollabSpace.Hubs;
using CollabSpace.Middleware;
using CollabSpace.Models.Settings;
using CollabSpace.Services;
using CollabSpace.Services.Interfaces;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

var builder = WebApplication.CreateBuilder(args);

// TEMPORARY DEBUG — remove after fixing
Console.WriteLine("=== CONNECTION STRING DEBUG ===");
Console.WriteLine($"Raw env var: {Environment.GetEnvironmentVariable("ConnectionStrings__DatabaseConnection") ?? "NULL"}");
Console.WriteLine($"Config value: {builder.Configuration.GetConnectionString("DatabaseConnection") ?? "NULL"}");
Console.WriteLine($"DATABASE_URL: {Environment.GetEnvironmentVariable("DATABASE_URL") ?? "NULL"}");
Console.WriteLine("================================");

// Read connection string with fallback to DATABASE_URL
// Railway PostgreSQL exposes DATABASE_URL automatically
// We try our configured variable first, then fall back to DATABASE_URL
var connectionString =
    builder.Configuration.GetConnectionString("DatabaseConnection")
    ?? Environment.GetEnvironmentVariable("DATABASE_URL");

if (string.IsNullOrEmpty(connectionString))
    throw new InvalidOperationException(
        "No database connection string found. " +
        "Set ConnectionStrings__DatabaseConnection or DATABASE_URL.");

// Fix for Railway's PostgreSQL URL format.
// Railway provides postgresql:// but Npgsql needs Host=...;Database=...
// This converts the URL format to the connection string format Npgsql expects.
if (connectionString.StartsWith("postgresql://")
    || connectionString.StartsWith("postgres://"))
{
    var uri = new Uri(connectionString);
    var userInfo = uri.UserInfo.Split(':');
    connectionString = $"Host={uri.Host};" +
        $"Port={uri.Port};" +
        $"Database={uri.AbsolutePath.TrimStart('/')};" +
        $"Username={userInfo[0]};" +
        $"Password={userInfo[1]};" +
        $"SSL Mode=Require;" +
        $"Trust Server Certificate=true;";
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// JWT Settings — binds appsettings.json section to the JwtSettings class
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

// JWT Authentication middleware
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);
// Configure JWT for SignalR.
// WebSocket connections cannot set HTTP headers in the browser.
// SignalR passes the token as a query parameter instead.
// This block tells the JWT middleware to read it from there.
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero // tokens expire exactly on time, no grace period

    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            // Only read from query string for SignalR hub connections
            if (!string.IsNullOrEmpty(accessToken)
                && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };

});

builder.Services.AddAuthorization();

// Register services
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<INotificationEventService, NotificationEventService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IChatEventService, ChatEventService>();
builder.Services.AddSignalR();
builder.Services.AddScoped<IBoardEventService, BoardEventService>();
builder.Services.AddScoped<IBoardService, BoardService>();
builder.Services.AddScoped<ICardService, CardService>();
builder.Services.AddScoped<IWorkspaceService, WorkspaceService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IWorkspaceAuthService, WorkspaceAuthService>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var allowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins")
    .Get<string[]>()
    ?? new[] { "http://localhost:5173" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowReactApp");

// Auto-apply migrations on startup in production.
// In development you run migrations manually via the CLI.
// In production the app migrates itself on each deploy.
if (app.Environment.IsProduction())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider
        .GetRequiredService<AppDbContext>();

    // Retry logic handles the brief delay between the app
    // starting and the database being ready to accept connections
    var retries = 0;
    while (retries < 5)
    {
        try
        {
            db.Database.Migrate();
            break;
        }
        catch (Exception ex)
        {
            retries++;
            Console.WriteLine(
                $"Migration attempt {retries} failed: {ex.Message}");
            await Task.Delay(2000);
        }
    }
}

// In the middleware pipeline, this must come FIRST
// before UseAuthentication and UseAuthorization
// Map the hub to a URL after app.UseAuthorization()
app.MapHub<CollabHub>("/hubs/collab");
app.UseMiddleware<GlobalExceptionHandler>();
app.UseAuthentication(); // must come before UseAuthorization
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.Run();
