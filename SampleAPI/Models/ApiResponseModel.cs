namespace SampleAPI.Models;

/// <summary>
/// API統一レスポンスモデル
/// </summary>
/// <typeparam name="T">データの型</typeparam>
public class ApiResponseModel<T>
{
    /// <summary>
    /// 成功フラグ
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// メッセージ
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// データ
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// エラー詳細
    /// </summary>
    public List<string>? Errors { get; set; }

    /// <summary>
    /// タイムスタンプ
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 成功レスポンスを作成
    /// </summary>
    public static ApiResponseModel<T> SuccessResponse(T data, string message = "Success")
    {
        return new ApiResponseModel<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    /// <summary>
    /// エラーレスポンスを作成
    /// </summary>
    public static ApiResponseModel<T> ErrorResponse(string message, List<string>? errors = null)
    {
        return new ApiResponseModel<T>
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }
}
