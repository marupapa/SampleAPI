using System.Data;

namespace SampleAPI.Infrastructure.Data;

/// <summary>
/// Dapperヘルパーインターフェース
/// </summary>
public interface IDapperHelper
{
    /// <summary>
    /// クエリを実行して結果を取得
    /// </summary>
    Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null, CommandType commandType = CommandType.Text);

    /// <summary>
    /// クエリを実行して単一の結果を取得
    /// </summary>
    Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? parameters = null, CommandType commandType = CommandType.Text);

    /// <summary>
    /// クエリを実行して単一の結果を取得（結果が存在しない場合は例外）
    /// </summary>
    Task<T> QuerySingleAsync<T>(string sql, object? parameters = null, CommandType commandType = CommandType.Text);

    /// <summary>
    /// コマンドを実行して影響を受けた行数を取得
    /// </summary>
    Task<int> ExecuteAsync(string sql, object? parameters = null, CommandType commandType = CommandType.Text);

    /// <summary>
    /// スカラー値を取得
    /// </summary>
    Task<T?> ExecuteScalarAsync<T>(string sql, object? parameters = null, CommandType commandType = CommandType.Text);

    /// <summary>
    /// トランザクション内でコマンドを実行
    /// </summary>
    Task<T> ExecuteInTransactionAsync<T>(Func<IDbConnection, IDbTransaction, Task<T>> action);

    /// <summary>
    /// DynamicParametersから値を取得
    /// </summary>
    T? GetValue<T>(object parameters, string parameterName);
}
