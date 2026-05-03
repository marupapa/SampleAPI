using SampleAPI.Handlers;
using SampleAPI.Interfaces;
using SampleAPI.Services;
using SampleAPI.ApplicationCore.Interfaces;
using SampleAPI.ApplicationCore.Models;
using SampleAPI.Infrastructure.Data;
using SampleAPI.Infrastructure.ExternalApi;
using SampleAPI.Common.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using NLog;
using NLog.Web;
using System.Text;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("Application Starting");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // 環境別設定の読み込み
    builder.Configuration
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables();

    // NLog設定
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    // Controllers
    builder.Services.AddControllers();

    // 認証設定
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var secretKey = jwtSettings["SecretKey"];
    var issuer = jwtSettings["Issuer"];
    var audience = jwtSettings["Audience"];

    if (string.IsNullOrWhiteSpace(secretKey))
    {
        throw new InvalidOperationException("JWT SecretKey is not configured");
    }

    if (string.IsNullOrWhiteSpace(issuer))
    {
        throw new InvalidOperationException("JWT Issuer is not configured");
    }

    if (string.IsNullOrWhiteSpace(audience))
    {
        throw new InvalidOperationException("JWT Audience is not configured");
    }

    if (secretKey.StartsWith("YourSecretKey", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException("JWT SecretKey must be provided from a secure source.");
    }

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
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

    builder.Services.AddAuthorization();

    // OpenAPI設定
    builder.Services.AddOpenApi("v1");

    // サービス登録
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IDapperHelper, DapperHelper>();
    builder.Services.AddScoped<IProcedureHelper, ProcedureHelper>();
    builder.Services.AddHttpClient<IExternalApiClient, ExternalApiClient>();
    builder.Services.AddSingleton<ILoggerService, NLogService>();
    builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

    // CORS設定
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    // グローバル例外ハンドラー
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    var app = builder.Build();

    // ミドルウェアパイプライン
    if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Local")
    {
        app.MapOpenApi();
    }

    app.UseExceptionHandler();

    app.UseHttpsRedirection();

    app.UseCors("AllowAll");

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapGet("/health", () => Results.Ok(new
    {
        status = "Healthy",
        timestamp = DateTime.UtcNow
    })).AllowAnonymous();

    app.MapGet("/api/v1/health", () => Results.Ok(new
    {
        status = "Healthy",
        timestamp = DateTime.UtcNow
    })).AllowAnonymous();

    app.MapControllers();

    logger.Info("Application Started Successfully");
    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "Application stopped because of exception");
    throw;
}
finally
{
    LogManager.Shutdown();
}
