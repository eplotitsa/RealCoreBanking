using System.Text.Json.Serialization;

namespace coreAI.DeepSeek;

// ── Request ───────────────────────────────────────────────────────────────────

/// <summary>Body of POST {BaseUrl}/chat/completions.</summary>
public sealed class DeepSeekChatRequest
{
    [JsonPropertyName("model")]       public string Model { get; set; } = "deepseek-chat";
    [JsonPropertyName("messages")]    public List<DeepSeekMessage> Messages { get; set; } = new();
    [JsonPropertyName("temperature")] public double Temperature { get; set; } = 1.0;
    [JsonPropertyName("max_tokens")]  public int MaxTokens { get; set; } = 1024;
    [JsonPropertyName("stream")]      public bool Stream { get; set; }
}

public sealed class DeepSeekMessage
{
    [JsonPropertyName("role")]    public string Role { get; set; } = "user";
    [JsonPropertyName("content")] public string Content { get; set; } = "";

    public static DeepSeekMessage System(string content)    => new() { Role = "system",    Content = content };
    public static DeepSeekMessage User(string content)      => new() { Role = "user",      Content = content };
    public static DeepSeekMessage Assistant(string content) => new() { Role = "assistant", Content = content };
}

// ── Response ──────────────────────────────────────────────────────────────────

public sealed class DeepSeekChatResponse
{
    [JsonPropertyName("id")]      public string Id { get; set; } = "";
    [JsonPropertyName("model")]   public string Model { get; set; } = "";
    [JsonPropertyName("created")] public long Created { get; set; }
    [JsonPropertyName("choices")] public List<DeepSeekChoice> Choices { get; set; } = new();
    [JsonPropertyName("usage")]   public DeepSeekUsage? Usage { get; set; }

    /// <summary>Content of the first choice — "" when the response carries no text.</summary>
    [JsonIgnore]
    public string Text => Choices.Count > 0 ? Choices[0].Message?.Content ?? "" : "";
}

public sealed class DeepSeekChoice
{
    [JsonPropertyName("index")]         public int Index { get; set; }

    /// <summary>Present in non-streaming responses.</summary>
    [JsonPropertyName("message")]       public DeepSeekDelta? Message { get; set; }

    /// <summary>Present in streaming (SSE) chunks.</summary>
    [JsonPropertyName("delta")]         public DeepSeekDelta? Delta { get; set; }

    [JsonPropertyName("finish_reason")] public string? FinishReason { get; set; }
}

/// <summary>Message payload; the same shape is used for <c>message</c> and <c>delta</c>.</summary>
public sealed class DeepSeekDelta
{
    [JsonPropertyName("role")]              public string? Role { get; set; }
    [JsonPropertyName("content")]           public string? Content { get; set; }

    /// <summary>Chain-of-thought text returned by <c>deepseek-reasoner</c>.</summary>
    [JsonPropertyName("reasoning_content")] public string? ReasoningContent { get; set; }
}

public sealed class DeepSeekUsage
{
    [JsonPropertyName("prompt_tokens")]     public int PromptTokens { get; set; }
    [JsonPropertyName("completion_tokens")] public int CompletionTokens { get; set; }
    [JsonPropertyName("total_tokens")]      public int TotalTokens { get; set; }
}

// ── Error payload ─────────────────────────────────────────────────────────────

public sealed class DeepSeekErrorResponse
{
    [JsonPropertyName("error")] public DeepSeekError? Error { get; set; }
}

public sealed class DeepSeekError
{
    [JsonPropertyName("message")] public string Message { get; set; } = "";
    [JsonPropertyName("type")]    public string? Type { get; set; }
    [JsonPropertyName("code")]    public string? Code { get; set; }
}