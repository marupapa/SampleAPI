namespace SampleAPI.Common.Extensions;

/// <summary>
/// DateTime拡張メソッド
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Unixタイムスタンプに変換
    /// </summary>
    public static long ToUnixTimestamp(this DateTime dateTime)
    {
        return ((DateTimeOffset)dateTime).ToUnixTimeSeconds();
    }

    /// <summary>
    /// Unixタイムスタンプから変換
    /// </summary>
    public static DateTime FromUnixTimestamp(this long timestamp)
    {
        return DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
    }

    /// <summary>
    /// ISO 8601形式の文字列に変換
    /// </summary>
    public static string ToIso8601String(this DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
    }

    /// <summary>
    /// 日本標準時に変換
    /// </summary>
    public static DateTime ToJapanStandardTime(this DateTime dateTime)
    {
        var jstTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
        return TimeZoneInfo.ConvertTime(dateTime, jstTimeZone);
    }

    /// <summary>
    /// 営業日かどうかを判定（土日を除く）
    /// </summary>
    public static bool IsBusinessDay(this DateTime dateTime)
    {
        return dateTime.DayOfWeek != DayOfWeek.Saturday && 
               dateTime.DayOfWeek != DayOfWeek.Sunday;
    }

    /// <summary>
    /// 月初の日付を取得
    /// </summary>
    public static DateTime GetFirstDayOfMonth(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, 1);
    }

    /// <summary>
    /// 月末の日付を取得
    /// </summary>
    public static DateTime GetLastDayOfMonth(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, DateTime.DaysInMonth(dateTime.Year, dateTime.Month));
    }

    /// <summary>
    /// 年齢を計算
    /// </summary>
    public static int CalculateAge(this DateTime birthDate, DateTime? referenceDate = null)
    {
        var reference = referenceDate ?? DateTime.Today;
        var age = reference.Year - birthDate.Year;
        
        if (birthDate.Date > reference.AddYears(-age))
            age--;

        return age;
    }
}
