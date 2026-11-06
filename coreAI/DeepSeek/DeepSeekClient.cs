using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace coreAI.DeepSeek;

/// <summary>
/// Plain <see cref="HttpClient"/> client for the DeepSeek chat API. DeepSeek exposes an
/// OpenAI-compatible REST interface, so no vendor SDK package is needed.
/// </summary>
public sealed class DeepSeekClient : IDeepSeekClient, IDisposable
{
    private const string ChatCompletionsPath = "chat/completions";
    private const string SseDataPrefix       = "data:";
    private const string SseDoneMarker       = "[DONE]";
    private const int    MaxRawErrorLength   = 500;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly bool       _ownsHttpClient;

    public DeepSeekOptions Options { get; }
    public bool IsConfigured => Options.IsConfigured;

    /// <param name="httpClient">Caller-owned client; <see cref="HttpClient.BaseAddress"/> is set when missing.</param>
    /// <param name="options">Connection settings.</param>
    /// <param name="ownsHttpClient">When true the client is disposed together with this instance.</param>
    public DeepSeekClient(HttpClient httpClient, DeepSeekOptions options, bool ownsHttpClient = false)
    {
        _httpClient     = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        Options         = options    ?? throw new ArgumentNullException(nameof(options));
        _ownsHttpClient = ownsHttpClient;

        _httpClient.BaseAddress ??= new Uri(Options.NormalizedBaseUrl);
    }

    /// <summary>Creates a self-contained client (own <see cref="HttpClient"/>, base address and timeout).</summary>
    public static DeepSeekClient Create(DeepSeekOptions options, HttpMessageHandler? handler = null)
    {
        ArgumentNullException.ThrowIfNull(options);

        var httpClient = handler != null
            ? new HttpClient(handler, disposeHandler: true)
            : new HttpClient();

        httpClient.BaseAddress = new Uri(options.NormalizedBaseUrl);
        httpClient.Timeout     = TimeSpan.FromSeconds(options.TimeoutSeconds > 0 ? options.TimeoutSeconds : 100);

        return new DeepSeekClient(httpClient, options, ownsHttpClient: true);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public async Task<DeepSeekChatResponse> CompleteAsync(IReadOnlyList<DeepSeekMessage> messages,
                                                          CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        using var request  = CreateHttpRequest(BuildRequestPayload(messages, stream: false));
        using var response = await _httpClient
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            throw new DeepSeekException(BuildErrorMessage(response, body), response.StatusCode, body);

        var payload = JsonSerializer.Deserialize<DeepSeekChatResponse>(body, JsonOptions)
            ?? throw new DeepSeekException("DeepSeek returned an empty response.", response.StatusCode, body);

        return payload;
    }

    public async Task<string> AskAsync(string prompt, string? systemPrompt = null,
                                       CancellationToken cancellationToken = default)
    {
        var messages = new List<DeepSeekMessage>();

        var system = string.IsNullOrWhiteSpace(systemPrompt) ? Options.SystemPrompt : systemPrompt;
        if (!string.IsNullOrWhiteSpace(system)) messages.Add(DeepSeekMessage.System(system));
        messages.Add(DeepSeekMessage.User(prompt));

        var response = await CompleteAsync(messages, cancellationToken).ConfigureAwait(false);
        return response.Text;
    }

    public async IAsyncEnumerable<string> StreamAsync(IReadOnlyList<DeepSeekMessage> messages,
                                                      [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        using var request  = CreateHttpRequest(BuildRequestPayload(messages, stream: true));
        using var response = await _httpClient
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            throw new DeepSeekException(BuildErrorMessage(response, errorBody), response.StatusCode, errorBody);
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var reader = new StreamReader(stream, Encoding.UTF8);

        while (true)
        {
            var line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (line == null) yield break;
            if (!line.StartsWith(SseDataPrefix, StringComparison.Ordinal)) continue;

            var data = line[SseDataPrefix.Length..].Trim();
            if (data.Length == 0) continue;
            if (data == SseDoneMarker) yield break;

            // A malformed chunk must not kill an otherwise healthy stream.
            var delta = ParseChunkOrNull(data)?.Choices.FirstOrDefault()?.Delta?.Content;
            if (!string.IsNullOrEmpty(delta)) yield return delta;
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private DeepSeekChatRequest BuildRequestPayload(IReadOnlyList<DeepSeekMessage> messages, bool stream)
    {
        if (messages == null || messages.Count == 0)
            throw new ArgumentException("At least one chat message is required.", nameof(messages));

        return new DeepSeekChatRequest
        {
            Model       = Options.Model,
            Messages    = messages.ToList(),
            Temperature = Options.Temperature,
            MaxTokens   = Options.MaxTokens,
            Stream      = stream,
        };
    }

    private HttpRequestMessage CreateHttpRequest(DeepSeekChatRequest payload)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, ChatCompletionsPath)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json"),
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Options.ResolvedApiKey);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        return request;
    }

    private static DeepSeekChatResponse? ParseChunkOrNull(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<DeepSeekChatResponse>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string BuildErrorMessage(HttpResponseMessage response, string body)
    {
        var status = $"{(int)response.StatusCode} {response.ReasonPhrase}".Trim();
        var detail = ExtractErrorMessage(body);

        return detail.Length > 0
            ? $"DeepSeek API error {status}: {detail}"
            : $"DeepSeek API error {status}.";
    }

    private static string ExtractErrorMessage(string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return "";

        try
        {
            var error = JsonSerializer.Deserialize<DeepSeekErrorResponse>(body, JsonOptions)?.Error;
            if (error != null && !string.IsNullOrWhiteSpace(error.Message)) return error.Message.Trim();
        }
        catch (JsonException)
        {
            // Not a JSON error payload — fall through to the raw body.
        }

        var raw = body.Trim();
        return raw.Length > MaxRawErrorLength ? raw[..MaxRawErrorLength] : raw;
    }

    private void EnsureConfigured()
    {
        if (Options.IsConfigured) return;

        throw new InvalidOperationException(
            $"DeepSeek API key is not configured. Set \"{DeepSeekOptions.SectionName}:ApiKey\" in appsettings.json, " +
            $"use user secrets (dotnet user-secrets set \"{DeepSeekOptions.SectionName}:ApiKey\" \"<key>\") " +
            $"or define the {DeepSeekOptions.ApiKeyEnvironmentVariable} environment variable.");
    }

    public void Dispose()
    {
        if (_ownsHttpClient) _httpClient.Dispose();
    }
}