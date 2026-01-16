using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using SampleAPI.Common.Logging;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace SampleAPI.Handlers;

/// <summary>
/// カスタム認証ハンドラー
/// </summary>
public class AuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly ILoggerService _logger;

    public AuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory loggerFactory,
        UrlEncoder encoder,
        ILoggerService logger) : base(options, loggerFactory, encoder)
    {
        _logger = logger;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                _logger.Warning("Authorization header not found");
                return AuthenticateResult.Fail("Authorization header not found");
            }

            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader))
            {
                _logger.Warning("Authorization header is empty");
                return AuthenticateResult.Fail("Authorization header is empty");
            }

            // Bearer トークンの検証
            if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                _logger.Warning("Invalid authorization header format");
                return AuthenticateResult.Fail("Invalid authorization header format");
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            if (string.IsNullOrEmpty(token))
            {
                _logger.Warning("Token is empty");
                return AuthenticateResult.Fail("Token is empty");
            }

            // トークン検証ロジック（実際にはJWTトークンの検証を行う）
            var principal = ValidateToken(token);
            if (principal == null)
            {
                _logger.Warning("Invalid token");
                return AuthenticateResult.Fail("Invalid token");
            }

            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            _logger.Info("Authentication successful");
            
            return AuthenticateResult.Success(ticket);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Authentication error");
            return AuthenticateResult.Fail($"Authentication error: {ex.Message}");
        }
    }

    private ClaimsPrincipal? ValidateToken(string token)
    {
        // 実際のトークン検証ロジックをここに実装
        // これはサンプル実装です
        try
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "SampleUser"),
                new Claim(ClaimTypes.Role, "User"),
                new Claim(ClaimTypes.NameIdentifier, "1")
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);

            return principal;
        }
        catch
        {
            return null;
        }
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = 401;
        Response.Headers.Add("WWW-Authenticate", "Bearer");
        return Task.CompletedTask;
    }

    protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = 403;
        return Task.CompletedTask;
    }
}
