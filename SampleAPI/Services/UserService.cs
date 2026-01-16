using SampleAPI.Interfaces;
using SampleAPI.Models;
using SampleAPI.ApplicationCore.Interfaces;
using SampleAPI.ApplicationCore.Models;
using SampleAPI.Common.Logging;

namespace SampleAPI.Services;

/// <summary>
/// ユーザーサービス実装
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILoggerService _logger;

    public UserService(IUserRepository userRepository, ILoggerService logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<UserResponseModel>> GetAllUsersAsync()
    {
        try
        {
            _logger.Info("Getting all users");
            var users = await _userRepository.GetAllAsync();
            
            return users.Select(u => MapToResponseModel(u));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error getting all users");
            throw;
        }
    }

    public async Task<UserResponseModel?> GetUserByIdAsync(int id)
    {
        try
        {
            _logger.Info($"Getting user by ID: {id}");
            var user = await _userRepository.GetByIdAsync(id);
            
            return user != null ? MapToResponseModel(user) : null;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Error getting user by ID: {id}");
            throw;
        }
    }

    public async Task<UserResponseModel> CreateUserAsync(UserRequestModel request)
    {
        try
        {
            _logger.Info($"Creating user: {request.Username}");
            
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var createdUser = await _userRepository.CreateAsync(user);
            
            _logger.Info($"User created successfully: {createdUser.Id}");
            return MapToResponseModel(createdUser);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Error creating user: {request.Username}");
            throw;
        }
    }

    public async Task<UserResponseModel> UpdateUserAsync(int id, UserRequestModel request)
    {
        try
        {
            _logger.Info($"Updating user: {id}");
            
            var existingUser = await _userRepository.GetByIdAsync(id);
            if (existingUser == null)
            {
                throw new KeyNotFoundException($"User with ID {id} not found");
            }

            existingUser.Username = request.Username;
            existingUser.Email = request.Email;
            existingUser.FullName = request.FullName;
            existingUser.PhoneNumber = request.PhoneNumber;
            existingUser.UpdatedAt = DateTime.UtcNow;

            var updatedUser = await _userRepository.UpdateAsync(existingUser);
            
            _logger.Info($"User updated successfully: {id}");
            return MapToResponseModel(updatedUser);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Error updating user: {id}");
            throw;
        }
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        try
        {
            _logger.Info($"Deleting user: {id}");
            
            var result = await _userRepository.DeleteAsync(id);
            
            if (result)
            {
                _logger.Info($"User deleted successfully: {id}");
            }
            else
            {
                _logger.Warning($"User not found for deletion: {id}");
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Error deleting user: {id}");
            throw;
        }
    }

    private static UserResponseModel MapToResponseModel(User user)
    {
        return new UserResponseModel
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            IsActive = user.IsActive
        };
    }
}
