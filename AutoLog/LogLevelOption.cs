namespace AutoLog;

/// <summary>
/// Specifies the level of detail for logging HTTP requests and responses in the logging middleware.
/// </summary>
/// <remarks>
/// This enum is used with the <see cref="MustLogAttribute"/> to control the amount and type of information logged.
/// Each option determines which parts of the request and response (e.g., headers, body, query parameters) are included in the logs.
/// </remarks>
public enum LogLevelOption
{
    /// <summary>
    /// Disables logging entirely. No request or response information is logged.
    /// </summary>
    /// <remarks>
    /// Useful for sensitive or high-traffic endpoints where logging is unnecessary or undesirable.
    /// </remarks>
    None,

    /// <summary>
    /// Logs basic information about the request and response, including the HTTP method, URL path, and status code.
    /// </summary>
    /// <remarks>
    /// Suitable for general monitoring with minimal overhead.
    /// </remarks>
    Basic,

    /// <summary>
    /// Logs only the request and response headers.
    /// </summary>
    /// <remarks>
    /// Ideal for debugging header-related issues, such as authentication or content negotiation.
    /// </remarks>
    Headers,

    /// <summary>
    /// Logs query parameters for GET requests.
    /// </summary>
    /// <remarks>
    /// Useful for analyzing query string data in GET requests, with no response logging.
    /// </remarks>
    Query,


    /// <summary>
    /// Logs request and response bodies.
    /// </summary>
    /// <remarks>
    /// Useful for analyzing bodies in both request and response.
    /// </remarks>
    Body,

    /// <summary>
    /// Logs detailed information, including the HTTP method, URL path, headers, and request/response bodies.
    /// </summary>
    /// <remarks>
    /// Provides comprehensive insights for debugging, but may increase resource usage due to body logging.
    /// </remarks>
    Full,

    /// <summary>
    /// Logs all available information, including the HTTP method, URL path, headers, request/response bodies, and query parameters.
    /// </summary>
    /// <remarks>
    /// Maximum verbosity, suitable for thorough debugging but should be used cautiously due to performance impact.
    /// </remarks>
    All,

    /// <summary>
    /// Allows custom logging of specific headers or request/response bodies, as configured in the <see cref="MustLogAttribute"/>.
    /// </summary>
    /// <remarks>
    /// Offers fine-grained control, enabling selective logging of headers or bodies to balance detail and performance.
    /// Use with <see cref="MustLogAttribute.CustomHeaders"/> and <see cref="MustLogAttribute.LogBody"/> for configuration.
    /// </remarks>
    Custom
}