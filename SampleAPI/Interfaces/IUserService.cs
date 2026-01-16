using SampleAPI.Models;

namespace SampleAPI.Interfaces;

/// <summary>
/// ユーザーサービスインターフェース
/// </summary>
public interface IUserService
{
    /// <summary>
    /// すべてのユーザーを取得
    /// </summary>
    Task<IEnumerable<UserResponseModel>> GetAllUsersAsync();

    /// <summary>
    /// ユーザーIDでユーザーを取得
    /// </summary>
    Task<UserResponseModel?> GetUserByIdAsync(int id);

    /// <summary>
    /// 新しいユーザーを作成
    /// </summary>
    Task<UserResponseModel> CreateUserAsync(UserRequestModel request);

    /// <summary>
    /// ユーザー情報を更新
    /// </summary>
    Task<UserResponseModel> UpdateUserAsync(int id, UserRequestModel request);

    /// <summary>
    /// ユーザーを削除
    /// </summary>
    Task<bool> DeleteUserAsync(int id);
}
