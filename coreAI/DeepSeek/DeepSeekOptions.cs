using System.Globalization;

namespace coreAI.DeepSeek;

/// <summary>
/// Connection settings for the DeepSeek chat API (OpenAI-compatible REST endpoint).
/// </summary>
public sealed class DeepSeekOptions
{
    /// <summary>Configuration section name expected in appsettings.json.</summary>
    public const string SectionName = "DeepSeek";

    /// <summary>Environment variable used as a fallback when <see cref="ApiKey"/> is not set.</summary>
    public const string ApiKeyEnvironmentVariable = "DEEPSEEK_API_KEY";

    /// <summary>API root. The client posts to <c>{BaseUrl}/chat/completions</c>.</summary>
    public string BaseUrl { get; set; } = "https://api.deepseek.com";

    /// <summary>Secret API key. Leave empty in appsettings.json — use user secrets or the env var.</summary>
    public string ApiKey { get; set; } = "";

    /// <summary>Model name: <c>deepseek-chat</c> (V3) or <c>deepseek-reasoner</c> (R1).</summary>
    public string Model { get; set; } = "deepseek-chat";

    /// <summary>Default system prompt used when a call does not pass its own.</summary>
    public string SystemPrompt { get; set; } =
        "Ты — AI-ассистент core banking системы RealCoreBanking. Отвечай кратко и по делу.";

    public double Temperature { get; set; } = 1.0;

    public int MaxTokens { get; set; } = 1024;

    /// <summary>HTTP timeout in seconds.</summary>
    public int TimeoutSeconds { get; set; } = 100;

    /// <summary>True when an API key is available from configuration or the environment.</summary>
    public bool IsConfigured => ResolvedApiKey.Length > 0;

    /// <summary>Configured key, or <see cref="ApiKeyEnvironmentVariable"/> when the key is not set.</summary>
    public string ResolvedApiKey =>
        !string.IsNullOrWhiteSpace(ApiKey)
            ? ApiKey.Trim()
            : (Environment.GetEnvironmentVariable(ApiKeyEnvironmentVariable) ?? "").Trim();

    /// <summary>Base URL normalised to end with '/', so relative request paths compose correctly.</summary>
    public string NormalizedBaseUrl => BaseUrl.Trim().TrimEnd('/') + "/";

    /// <summary>
    /// Builds options from any key/value source (e.g. an <c>IConfigurationSection</c>),
    /// falling back to the defaults for every value that is missing or malformed.
    /// </summary>
    public static DeepSeekOptions FromValues(Func<string, string?> read)
    {
        ArgumentNullException.ThrowIfNull(read);

        var defaults = new DeepSeekOptions();
        return new DeepSeekOptions
        {
            BaseUrl        = NonEmpty(read(nameof(BaseUrl)))         ?? defaults.BaseUrl,
            ApiKey         = read(nameof(ApiKey))                    ?? defaults.ApiKey,
            Model          = NonEmpty(read(nameof(Model)))           ?? defaults.Model,
            SystemPrompt   = NonEmpty(read(nameof(SystemPrompt)))    ?? defaults.SystemPrompt,
            Temperature    = ParseDouble(read(nameof(Temperature)), defaults.Temperature),
            MaxTokens      = ParseInt(read(nameof(MaxTokens)), defaults.MaxTokens),
            TimeoutSeconds = ParseInt(read(nameof(TimeoutSeconds)), defaults.TimeoutSeconds),
        };
    }

    private static string? NonEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static double ParseDouble(string? value, double fallback) =>
        double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) ? parsed : fallback;

    private static int ParseInt(string? value, int fallback) =>
        int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : fallback;
}