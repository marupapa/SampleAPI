using Dapper;
using SampleAPI.ApplicationCore.Interfaces;
using SampleAPI.ApplicationCore.Models;
using SampleAPI.Common.Logging;
using System.Data;

namespace SampleAPI.Infrastructure.Data;

/// <summary>
/// ユーザーリポジトリ実装
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly IDapperHelper _dapperHelper;
    private readonly IProcedureHelper _procedureHelper;
    private readonly ILoggerService _logger;

    public UserRepository(
        IDapperHelper dapperHelper,
        IProcedureHelper procedureHelper,
        ILoggerService logger)
    {
        _dapperHelper = dapperHelper;
        _procedureHelper = procedureHelper;
        _logger = logger;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        const string sql = @"
            SELECT Id, Username, Email, FullName, PhoneNumber, 
                   CreatedAt, UpdatedAt, LastLoginAt, IsActive, IsDeleted
            FROM Users 
            WHERE IsDeleted = 0
            ORDER BY CreatedAt DESC";

        return await _dapperHelper.QueryAsync<User>(sql);
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT Id, Username, Email, FullName, PhoneNumber, 
                   CreatedAt, UpdatedAt, LastLoginAt, IsActive, IsDeleted
            FROM Users 
            WHERE Id = @Id AND IsDeleted = 0";

        return await _dapperHelper.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        const string sql = @"
            SELECT Id, Username, Email, FullName, PhoneNumber, 
                   CreatedAt, UpdatedAt, LastLoginAt, IsActive, IsDeleted
            FROM Users 
            WHERE Username = @Username AND IsDeleted = 0";

        return await _dapperHelper.QueryFirstOrDefaultAsync<User>(sql, new { Username = username });
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        const string sql = @"
            SELECT Id, Username, Email, FullName, PhoneNumber, 
                   CreatedAt, UpdatedAt, LastLoginAt, IsActive, IsDeleted
            FROM Users 
            WHERE Email = @Email AND IsDeleted = 0";

        return await _dapperHelper.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
    }

    public async Task<User> CreateAsync(User user)
    {
        try
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Username", user.Username);
            parameters.Add("@Email", user.Email);
            parameters.Add("@FullName", user.FullName);
            parameters.Add("@PhoneNumber", user.PhoneNumber);
            parameters.Add("@PasswordHash", user.PasswordHash);
            parameters.Add("@IsActive", user.IsActive);
            parameters.Add("@NewUserId", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = await _procedureHelper.ExecuteProcedureWithOutputAsync("sp_CreateUser", parameters);

            user.Id = _procedureHelper.GetOutputValue<int>(result.OutputParameters, "@NewUserId");
            _logger.Info($"User created with ID: {user.Id}");

            return user;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Error creating user: {user.Username}");
            throw;
        }
    }

    public async Task<User> UpdateAsync(User user)
    {
        try
        {
            var parameters = new
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive
            };

            await _procedureHelper.ExecuteProcedureAsync("sp_UpdateUser", parameters);
            
            _logger.Info($"User updated: {user.Id}");
            return user;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Error updating user: {user.Id}");
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var rowsAffected = await _procedureHelper.ExecuteProcedureAsync(
                "sp_DeleteUser",
                new { Id = id });

            var deleted = rowsAffected > 0;
            if (deleted)
            {
                _logger.Info($"User deleted: {id}");
            }

            return deleted;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Error deleting user: {id}");
            throw;
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        const string sql = "SELECT COUNT(1) FROM Users WHERE Id = @Id AND IsDeleted = 0";
        var count = await _dapperHelper.ExecuteScalarAsync<int>(sql, new { Id = id });
        return count > 0;
    }
}
