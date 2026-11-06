namespace coreAI.DeepSeek;

/// <summary>Chat access to a DeepSeek model.</summary>
public interface IDeepSeekClient
{
    /// <summary>Effective connection settings (model, endpoint, limits).</summary>
    DeepSeekOptions Options { get; }

    /// <summary>True when an API key is available and calls can be issued.</summary>
    bool IsConfigured { get; }

    /// <summary>Sends a full conversation and returns the raw API response (including usage).</summary>
    Task<DeepSeekChatResponse> CompleteAsync(IReadOnlyList<DeepSeekMessage> messages,
                                            CancellationToken cancellationToken = default);

    /// <summary>Single prompt → answer. <paramref name="systemPrompt"/> defaults to <see cref="DeepSeekOptions.SystemPrompt"/>.</summary>
    Task<string> AskAsync(string prompt, string? systemPrompt = null, CancellationToken cancellationToken = default);

    /// <summary>Streams answer fragments (SSE) as they are produced.</summary>
    IAsyncEnumerable<string> StreamAsync(IReadOnlyList<DeepSeekMessage> messages,
                                         CancellationToken cancellationToken = default);
}