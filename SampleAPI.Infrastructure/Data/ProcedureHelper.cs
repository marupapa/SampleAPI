using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SampleAPI.Common.Logging;
using SampleAPI.Infrastructure.Configurations;
using System.Data;

namespace SampleAPI.Infrastructure.Data;

/// <summary>
/// ストアドプロシージャヘルパー実装
/// </summary>
public class ProcedureHelper : IProcedureHelper
{
    private readonly SecretsManagerHelper _secretsManager;
    private readonly ILoggerService _logger;
    private readonly int _commandTimeout;

    public ProcedureHelper(IConfiguration configuration, ILoggerService logger)
    {
        _logger = logger;
        _secretsManager = new SecretsManagerHelper(configuration, logger);
        
        var timeoutValue = configuration["DatabaseSettings:CommandTimeout"];
        _commandTimeout = !string.IsNullOrEmpty(timeoutValue) ? int.Parse(timeoutValue) : 30;
    }

    private static IDbConnection CreateConnection(string connectionString)
    {
        return new SqlConnection(connectionString);
    }

    public async Task<int> ExecuteProcedureAsync(string procedureName, object? parameters = null)
    {
        try
        {
            _logger.Debug($"Executing stored procedure: {procedureName}");
            
            var connectionString = await _secretsManager.GetConnectionStringAsync();
            using var connection = CreateConnection(connectionString);
            var rowsAffected = await connection.ExecuteAsync(
                procedureName,
                parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: _commandTimeout);
            
            _logger.Debug($"Stored procedure executed successfully. Rows affected: {rowsAffected}");
            return rowsAffected;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Error executing stored procedure: {procedureName}");
            throw;
        }
    }

    public async Task<IEnumerable<T>> ExecuteProcedureWithResultAsync<T>(string procedureName, object? parameters = null)
    {
        try
        {
            _logger.Debug($"Executing stored procedure with result: {procedureName}");
            
            var connectionString = await _secretsManager.GetConnectionStringAsync();
            using var connection = CreateConnection(connectionString);
            var result = await connection.QueryAsync<T>(
                procedureName,
                parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: _commandTimeout);
            
            _logger.Debug($"Stored procedure executed successfully. Rows returned: {result.Count()}");
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Error executing stored procedure: {procedureName}");
            throw;
        }
    }

    public async Task<T?> ExecuteProcedureSingleAsync<T>(string procedureName, object? parameters = null)
    {
        try
        {
            _logger.Debug($"Executing stored procedure (single result): {procedureName}");
            
            var connectionString = await _secretsManager.GetConnectionStringAsync();
            using var connection = CreateConnection(connectionString);
            var result = await connection.QueryFirstOrDefaultAsync<T>(
                procedureName,
                parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: _commandTimeout);
            
            _logger.Debug($"Stored procedure executed successfully");
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Error executing stored procedure: {procedureName}");
            throw;
        }
    }

    public async Task<T?> ExecuteProcedureScalarAsync<T>(string procedureName, object? parameters = null)
    {
        try
        {
            _logger.Debug($"Executing stored procedure (scalar): {procedureName}");
            
            var connectionString = await _secretsManager.GetConnectionStringAsync();
            using var connection = CreateConnection(connectionString);
            var result = await connection.ExecuteScalarAsync<T>(
                procedureName,
                parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: _commandTimeout);
            
            _logger.Debug($"Stored procedure executed successfully");
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Error executing stored procedure: {procedureName}");
            throw;
        }
    }

    public async Task<(int RowsAffected, DynamicParameters OutputParameters)> ExecuteProcedureWithOutputAsync(
        string procedureName, 
        DynamicParameters parameters)
    {
        try
        {
            _logger.Debug($"Executing stored procedure with output parameters: {procedureName}");
            
            var connectionString = await _secretsManager.GetConnectionStringAsync();
            using var connection = CreateConnection(connectionString);
            var rowsAffected = await connection.ExecuteAsync(
                procedureName,
                parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: _commandTimeout);
            
            _logger.Debug($"Stored procedure executed successfully. Rows affected: {rowsAffected}");
            return (rowsAffected, parameters);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Error executing stored procedure: {procedureName}");
            throw;
        }
    }

    public T? GetOutputValue<T>(DynamicParameters parameters, string parameterName)
    {
        try
        {
            return parameters.Get<T>(parameterName);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Error getting output parameter value: {parameterName}");
            return default;
        }
    }
}
