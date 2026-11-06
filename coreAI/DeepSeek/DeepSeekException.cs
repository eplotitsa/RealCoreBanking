using System.Net;

namespace coreAI.DeepSeek;

/// <summary>Thrown when the DeepSeek API answers with a non-success status code.</summary>
public sealed class DeepSeekException : Exception
{
    /// <summary>HTTP status returned by DeepSeek, when the failure came from the API itself.</summary>
    public HttpStatusCode? StatusCode { get; }

    /// <summary>Raw response body (useful for diagnostics and logging).</summary>
    public string ResponseBody { get; }

    public DeepSeekException(string message, HttpStatusCode? statusCode = null, string responseBody = "",
                             Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode   = statusCode;
        ResponseBody = responseBody;
    }
}