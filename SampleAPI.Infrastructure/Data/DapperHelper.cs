using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SampleAPI.Common.Logging;
using SampleAPI.Infrastructure.Configurations;
using System.Data;

namespace SampleAPI.Infrastructure.Data;

/// <summary>
/// Dapperヘルパー実装
/// </summary>
public class DapperHelper : IDapperHelper
{
    private readonly string _connectionString;
    private readonly ILoggerService _logger;
    private readonly int _commandTimeout;

    public DapperHelper(IConfiguration configuration, ILoggerService logger)
    {
        _logger = logger;
        var secretsManager = new SecretsManagerHelper(configuration, logger);
        _connectionString = secretsManager.GetConnectionStringAsync().GetAwaiter().GetResult();
        
        var timeoutValue = configuration["DatabaseSettings:CommandTimeout"];
        _commandTimeout = !string.IsNullOrEmpty(timeoutValue) ? int.Parse(timeoutValue) : 30;
    }

    private IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null, CommandType commandType = CommandType.Text)
    {
        try
        {
            _logger.Debug($"Executing query: {sql}");
            
            using var connection = CreateConnection();
            var result = await connection.QueryAsync<T>(
                sql,
                parameters,
                commandType: commandType,
                commandTimeout: _commandTimeout);
            
            _logger.Debug($"Query executed successfully. Rows returned: {result.Count()}");
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Error executing query: {sql}");
            throw;
        }
    }

    public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? parameters = null, CommandType commandType = CommandType.Text)
    {
        try
        {
            _logger.Debug($"Executing query (first or default): {sql}");
            
            using var connection = CreateConnection();
            var result = await connection.QueryFirstOrDefaultAsync<T>(
                sql,
                parameters,
                commandType: commandType,
                commandTimeout: _commandTimeout);
            
            _logger.Debug($"Query executed successfully");
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Error executing query: {sql}");
            throw;
        }
    }

    public async Task<T> QuerySingleAsync<T>(string sql, object? parameters = null, CommandType commandType = CommandType.Text)
    {
        try
        {
            _logger.Debug($"Executing query (single): {sql}");
            
            using var connection = CreateConnection();
            var result = await connection.QuerySingleAsync<T>(
                sql,
                parameters,
                commandType: commandType,
                commandTimeout: _commandTimeout);
            
            _logger.Debug($"Query executed successfully");
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Error executing query: {sql}");
            throw;
        }
    }

    public async Task<int> ExecuteAsync(string sql, object? parameters = null, CommandType commandType = CommandType.Text)
    {
        try
        {
            _logger.Debug($"Executing command: {sql}");
            
            using var connection = CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(
                sql,
                parameters,
                commandType: commandType,
                commandTimeout: _commandTimeout);
            
            _logger.Debug($"Command executed successfully. Rows affected: {rowsAffected}");
            return rowsAffected;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Error executing command: {sql}");
            throw;
        }
    }

    public async Task<T?> ExecuteScalarAsync<T>(string sql, object? parameters = null, CommandType commandType = CommandType.Text)
    {
        try
        {
            _logger.Debug($"Executing scalar: {sql}");
            
            using var connection = CreateConnection();
            var result = await connection.ExecuteScalarAsync<T>(
                sql,
                parameters,
                commandType: commandType,
                commandTimeout: _commandTimeout);
            
            _logger.Debug($"Scalar executed successfully");
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Error executing scalar: {sql}");
            throw;
        }
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<IDbConnection, IDbTransaction, Task<T>> action)
    {
        using var connection = CreateConnection();
        connection.Open();
        
        using var transaction = connection.BeginTransaction();
        try
        {
            _logger.Debug("Starting transaction");
            
            var result = await action(connection, transaction);
            
            transaction.Commit();
            _logger.Debug("Transaction committed successfully");
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Transaction failed, rolling back");
            transaction.Rollback();
            throw;
        }
    }

    public T? GetValue<T>(object parameters, string parameterName)
    {
        try
        {
            if (parameters is DynamicParameters dynamicParams)
            {
                return dynamicParams.Get<T>(parameterName);
            }

            // リフレクションを使用してプロパティから値を取得
            var property = parameters.GetType().GetProperty(parameterName);
            if (property != null)
            {
                var value = property.GetValue(parameters);
                if (value is T typedValue)
                {
                    return typedValue;
                }
                
                // 型変換を試みる
                return (T?)Convert.ChangeType(value, typeof(T));
            }

            _logger.Warning($"Parameter not found: {parameterName}");
            return default;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Error getting parameter value: {parameterName}");
            return default;
        }
    }
}
