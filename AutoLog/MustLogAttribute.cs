namespace AutoLog;

/// <summary>
/// An attribute that specifies logging requirements for HTTP requests and responses on a controller or action method.
/// </summary>
/// <remarks>
/// Apply this attribute to a controller class or action method to enable logging with the specified <see cref="LogLevelOption"/>.
/// The attribute supports custom logging configurations, such as logging specific headers or the request/response body, when used with <see cref="LogLevelOption.Custom"/>.
/// </remarks>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class MustLogAttribute(LogLevelOption logLevel = LogLevelOption.Basic, string[]? customHeaders = null, bool logBody = false,bool logQuery = false) : Attribute
{
    /// <summary>
    /// Gets the logging level that determines the amount and type of information to log.
    /// </summary>
    /// <value>
    /// A <see cref="LogLevelOption"/> value specifying the logging level. Defaults to <see cref="LogLevelOption.Basic"/>.
    /// </value>
    public LogLevelOption LogLevel { get; } = logLevel;

    /// <summary>
    /// Gets the array of header names to log when using <see cref="LogLevelOption.Custom"/>.
    /// </summary>
    /// <value>
    /// An array of header names to include in the logs. Defaults to an empty array if not specified.
    /// </value>
    /// <remarks>
    /// Only the specified headers are logged when <see cref="LogLevel"/> is set to <see cref="LogLevelOption.Custom"/>.
    /// If empty or null, no headers are logged unless otherwise specified by the log level.
    /// </remarks>
    public string[] CustomHeaders { get; } = customHeaders ?? [];

    /// <summary>
    /// Gets a value indicating whether to log the request and response body when using <see cref="LogLevelOption.Custom"/>.
    /// </summary>
    /// <value>
    /// <c>true</c> if the request and response body should be logged; otherwise, <c>false</c>. Defaults to <c>false</c>.
    /// </value>
    /// <remarks>
    /// Body logging is only performed when <see cref="LogLevel"/> is <see cref="LogLevelOption.Custom"/> and this property is <c>true</c>,
    /// or when the log level is <see cref="LogLevelOption.Full"/>, <see cref="LogLevelOption.All"/> or <see cref="LogLevelOption.Body"/> .
    /// </remarks>
    public bool LogBody { get; } = logBody;

    /// <summary>
    /// Gets a value indicating whether to log the request query using <see cref="LogLevelOption.Custom"/>.
    /// </summary>
    /// <value>
    /// <c>true</c> if the request query should be logged; otherwise, <c>false</c>. Defaults to <c>false</c>.
    /// </value>
    /// <remarks>
    /// query logging is only performed when <see cref="LogLevel"/> is <see cref="LogLevelOption.Custom"/> and this property is <c>true</c>,
    /// or when the log level is <see cref="LogLevelOption.Full"/> , <see cref="LogLevelOption.All"/> or  or   <see cref="LogLevelOption.Query"/>.
    /// </remarks>
    public bool LogQuery { get; } = logQuery;
}