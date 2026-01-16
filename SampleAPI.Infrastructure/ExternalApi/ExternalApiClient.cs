using Microsoft.Extensions.Configuration;
using SampleAPI.Common.Logging;
using SampleAPI.Common.Helpers;
using System.Net;
using System.Text;

namespace SampleAPI.Infrastructure.ExternalApi;

/// <summary>
/// 外部APIクライアント実装
/// </summary>
public class ExternalApiClient : IExternalApiClient, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILoggerService _logger;
    private readonly string _baseUrl;
    private readonly int _timeout;

    public ExternalApiClient(IConfiguration configuration, ILoggerService logger)
    {
        _logger = logger;
        _baseUrl = configuration["ExternalApi:BaseUrl"] 
            ?? throw new InvalidOperationException("ExternalApi:BaseUrl not configured");
        _timeout = configuration.GetValue<int>("ExternalApi:Timeout", 30);

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_baseUrl),
            Timeout = TimeSpan.FromSeconds(_timeout)
        };

        _httpClient.DefaultRequestHeaders.Add("User-Agent", "SampleAPI/1.0");
    }

    public async Task<TResponse?> GetAsync<TResponse>(string endpoint, Dictionary<string, string>? headers = null)
    {
        try
        {
            _logger.Info($"Sending GET request to: {endpoint}");

            using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            AddHeaders(request, headers);

            var response = await _httpClient.SendAsync(request);
            return await ProcessResponse<TResponse>(response);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Error in GET request to: {endpoint}");
            throw;
        }
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(
        string endpoint, 
        TRequest data, 
        Dictionary<string, string>? headers = null)
    {
        try
        {
            _logger.Info($"Sending POST request to: {endpoint}");

            var json = JsonHelper.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = content
            };
            AddHeaders(request, headers);

            var response = await _httpClient.SendAsync(request);
            return await ProcessResponse<TResponse>(response);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Error in POST request to: {endpoint}");
            throw;
        }
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(
        string endpoint, 
        TRequest data, 
        Dictionary<string, string>? headers = null)
    {
        try
        {
            _logger.Info($"Sending PUT request to: {endpoint}");

            var json = JsonHelper.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Put, endpoint)
            {
                Content = content
            };
            AddHeaders(request, headers);

            var response = await _httpClient.SendAsync(request);
            return await ProcessResponse<TResponse>(response);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Error in PUT request to: {endpoint}");
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string endpoint, Dictionary<string, string>? headers = null)
    {
        try
        {
            _logger.Info($"Sending DELETE request to: {endpoint}");

            using var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
            AddHeaders(request, headers);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            _logger.Info($"DELETE request successful: {endpoint}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Error in DELETE request to: {endpoint}");
            return false;
        }
    }

    public async Task<string> GetRawResponseAsync(string endpoint, Dictionary<string, string>? headers = null)
    {
        try
        {
            _logger.Info($"Sending GET request (raw) to: {endpoint}");

            using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            AddHeaders(request, headers);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            _logger.Info($"GET request (raw) successful: {endpoint}");

            return content;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Error in GET request (raw) to: {endpoint}");
            throw;
        }
    }

    private static void AddHeaders(HttpRequestMessage request, Dictionary<string, string>? headers)
    {
        if (headers == null) return;

        foreach (var header in headers)
        {
            request.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
    }

    private async Task<TResponse?> ProcessResponse<TResponse>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.Error($"API request failed with status code: {response.StatusCode}. Content: {content}");
            throw new HttpRequestException(
                $"Request failed with status code {response.StatusCode}",
                null,
                response.StatusCode);
        }

        if (string.IsNullOrEmpty(content))
        {
            _logger.Warning("API response content is empty");
            return default;
        }

        try
        {
            var result = JsonHelper.Deserialize<TResponse>(content);
            _logger.Info($"API request successful. Status: {response.StatusCode}");
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Error deserializing response: {content}");
            throw;
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
        GC.SuppressFinalize(this);
    }
}
