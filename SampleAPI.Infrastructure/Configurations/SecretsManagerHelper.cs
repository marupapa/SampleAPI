using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SampleAPI.Common.Logging;

namespace SampleAPI.Infrastructure.Configurations;

/// <summary>
/// AWS Secrets Managerヘルパー
/// </summary>
public class SecretsManagerHelper
{
    private readonly IConfiguration _configuration;
    private readonly ILoggerService _logger;
    private string? _cachedConnectionString;

    public SecretsManagerHelper(IConfiguration configuration, ILoggerService logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// AWS Secrets Managerから接続文字列を取得
    /// </summary>
    public async Task<string> GetConnectionStringAsync()
    {
        // キャッシュされた接続文字列がある場合は返す
        if (!string.IsNullOrEmpty(_cachedConnectionString))
        {
            return _cachedConnectionString;
        }

        var environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Local";
        
        // Local環境では設定ファイルから接続文字列を取得
        if (environment == "Local")
        {
            _cachedConnectionString = _configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string not found in configuration");
            
            _logger.Info("Using connection string from configuration (Local environment)");
            return _cachedConnectionString;
        }

        try
        {
            var region = _configuration["AWS:Region"] ?? "ap-northeast-1";
            var secretName = _configuration["AWS:SecretsManager:SecretName"]
                ?? throw new InvalidOperationException("AWS SecretName not configured");

            _logger.Info($"Retrieving secret from AWS Secrets Manager: {secretName}");

            var client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(region));

            var request = new GetSecretValueRequest
            {
                SecretId = secretName,
                VersionStage = "AWSCURRENT"
            };

            var response = await client.GetSecretValueAsync(request);
            
            if (response.SecretString != null)
            {
                var secret = JsonConvert.DeserializeObject<DatabaseSecret>(response.SecretString)
                    ?? throw new InvalidOperationException("Failed to parse secret");

                _cachedConnectionString = BuildConnectionString(secret);
                _logger.Info("Connection string retrieved successfully from AWS Secrets Manager");
                
                return _cachedConnectionString;
            }
            
            throw new InvalidOperationException("Secret string is null");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error retrieving secret from AWS Secrets Manager");
            
            // フォールバック: 設定ファイルから接続文字列を取得
            _cachedConnectionString = _configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string not found in configuration and Secrets Manager failed");
            
            _logger.Warning("Using fallback connection string from configuration");
            return _cachedConnectionString;
        }
    }

    private static string BuildConnectionString(DatabaseSecret secret)
    {
        return $"Server={secret.Host},{secret.Port};Database={secret.Database};" +
               $"User Id={secret.Username};Password={secret.Password};" +
               $"TrustServerCertificate=True;Encrypt=True;";
    }

    /// <summary>
    /// データベースシークレット
    /// </summary>
    private class DatabaseSecret
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 1433;
        public string Database { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
