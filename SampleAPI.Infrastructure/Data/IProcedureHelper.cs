using Dapper;
using System.Data;

namespace SampleAPI.Infrastructure.Data;

/// <summary>
/// ストアドプロシージャヘルパーインターフェース
/// </summary>
public interface IProcedureHelper
{
    /// <summary>
    /// ストアドプロシージャを実行
    /// </summary>
    Task<int> ExecuteProcedureAsync(string procedureName, object? parameters = null);

    /// <summary>
    /// ストアドプロシージャを実行して結果を取得
    /// </summary>
    Task<IEnumerable<T>> ExecuteProcedureWithResultAsync<T>(string procedureName, object? parameters = null);

    /// <summary>
    /// ストアドプロシージャを実行して単一の結果を取得
    /// </summary>
    Task<T?> ExecuteProcedureSingleAsync<T>(string procedureName, object? parameters = null);

    /// <summary>
    /// ストアドプロシージャを実行してスカラー値を取得
    /// </summary>
    Task<T?> ExecuteProcedureScalarAsync<T>(string procedureName, object? parameters = null);

    /// <summary>
    /// 出力パラメータ付きでストアドプロシージャを実行
    /// </summary>
    Task<(int RowsAffected, DynamicParameters OutputParameters)> ExecuteProcedureWithOutputAsync(
        string procedureName, 
        DynamicParameters parameters);

    /// <summary>
    /// DynamicParametersから出力パラメータの値を取得
    /// </summary>
    T? GetOutputValue<T>(DynamicParameters parameters, string parameterName);
}
