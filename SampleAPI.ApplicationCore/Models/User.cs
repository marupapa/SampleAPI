namespace SampleAPI.ApplicationCore.Models;

/// <summary>
/// ユーザーエンティティ
/// </summary>
public class User
{
    /// <summary>
    /// ユーザーID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ユーザー名
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// メールアドレス
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 名前
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// 電話番号
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// パスワードハッシュ
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新日時
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 最終ログイン日時
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// アクティブフラグ
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 削除フラグ
    /// </summary>
    public bool IsDeleted { get; set; }
}
