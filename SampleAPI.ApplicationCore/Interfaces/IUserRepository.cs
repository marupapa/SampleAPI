using SampleAPI.ApplicationCore.Models;

namespace SampleAPI.ApplicationCore.Interfaces;

/// <summary>
/// ユーザーリポジトリインターフェース
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// すべてのユーザーを取得
    /// </summary>
    Task<IEnumerable<User>> GetAllAsync();

    /// <summary>
    /// ユーザーIDでユーザーを取得
    /// </summary>
    Task<User?> GetByIdAsync(int id);

    /// <summary>
    /// ユーザー名でユーザーを取得
    /// </summary>
    Task<User?> GetByUsernameAsync(string username);

    /// <summary>
    /// メールアドレスでユーザーを取得
    /// </summary>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// 新しいユーザーを作成
    /// </summary>
    Task<User> CreateAsync(User user);

    /// <summary>
    /// ユーザー情報を更新
    /// </summary>
    Task<User> UpdateAsync(User user);

    /// <summary>
    /// ユーザーを削除
    /// </summary>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// ユーザーの存在確認
    /// </summary>
    Task<bool> ExistsAsync(int id);
}
