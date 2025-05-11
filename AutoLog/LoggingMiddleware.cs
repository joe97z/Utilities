using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Logging;

namespace AutoLog;
/// <summary>
/// Middleware that logs HTTP requests and responses based on the <see cref="MustLogAttribute"/> configuration.
/// </summary>
/// <remarks>
/// This middleware intercepts HTTP requests and responses, logging details such as the HTTP method, URL, headers, query parameters, 
/// request/response bodies, and status code, depending on the <see cref="LogLevelOption"/> specified in the <see cref="MustLogAttribute"/>.
/// It integrates with ASP.NET Core’s logging infrastructure and supports custom logging configurations for fine-grained control.
/// </remarks>
public class LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
{
    /// <summary>
    /// The next middleware in the ASP.NET Core pipeline.
    /// </summary>
    private readonly RequestDelegate _next = next;

    /// <summary>
    /// The logger instance used to log request and response details.
    /// </summary>
    private readonly ILogger<LoggingMiddleware> _logger = logger;

    /// <summary>
    /// Processes an HTTP request, logging details based on the <see cref="MustLogAttribute"/> configuration.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <remarks>
    /// This method checks for a <see cref="MustLogAttribute"/> on the controller or action, determines the logging level,
    /// and logs the request and response accordingly. It captures the response body using a temporary stream to enable logging
    /// without disrupting the response pipeline. Logging is skipped if no attribute is found or if the log level is <see cref="LogLevelOption.None"/>.
    /// </remarks>
    public async Task InvokeAsync(HttpContext context)
    {
        var actionDescriptor = context.GetEndpoint()?.Metadata
            .OfType<ControllerActionDescriptor>()
            .FirstOrDefault();

        if (actionDescriptor != null)
        {
            var (logLevelOption, customHeaders, logBody) = GetLogLevelOptions(actionDescriptor);

            if (logLevelOption == LogLevelOption.None)
            {
                await _next(context);
                return;
            }

            LogRequest(context, logLevelOption, customHeaders, logBody);

            var originalResponseBody = context.Response.Body;
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            await _next(context);

            LogResponse(context, memoryStream, logLevelOption, customHeaders, logBody);

            memoryStream.Seek(0, SeekOrigin.Begin);
            await memoryStream.CopyToAsync(originalResponseBody);
        }
        else
        {
            await _next(context);
        }
    }

    private (LogLevelOption LogLevel, string[] CustomHeaders, bool LogBody) GetLogLevelOptions(ControllerActionDescriptor actionDescriptor)
    {
        var attribute = actionDescriptor.MethodInfo
            .GetCustomAttributes(typeof(MustLogAttribute), false)
            .Cast<MustLogAttribute>()
            .FirstOrDefault()
            ?? actionDescriptor.ControllerTypeInfo
            .GetCustomAttributes(typeof(MustLogAttribute), false)
            .Cast<MustLogAttribute>()
            .FirstOrDefault();

        return (
            attribute?.LogLevel ?? LogLevelOption.Basic,
            attribute?.CustomHeaders ?? [],
            attribute?.LogBody ?? false
        );
    }

    private void LogRequest(HttpContext context, LogLevelOption logLevel, string[] customHeaders, bool logBody)
    {
        if (logLevel == LogLevelOption.Basic || logLevel == LogLevelOption.Full || logLevel == LogLevelOption.All)
        {
            _logger.LogInformation("Request: {Method} {Url}", context.Request.Method, context.Request.Path);
        }

        if (logLevel == LogLevelOption.Headers || logLevel == LogLevelOption.Full || logLevel == LogLevelOption.All ||
            logLevel == LogLevelOption.Custom && customHeaders.Length > 0)
        {
            var headersToLog = logLevel == LogLevelOption.Custom && customHeaders.Length > 0
                ? string.Join(", ", context.Request.Headers
                    .Where(h => customHeaders.Contains(h.Key, StringComparer.OrdinalIgnoreCase))
                    .Select(h => $"{h.Key}: {h.Value}"))
                : context.Request.Headers.ToString();

            _logger.LogInformation("Request Headers: {Headers}", headersToLog);
        }

        if (logLevel == LogLevelOption.Query || logLevel == LogLevelOption.All)
        {
            var queryParams = context.Request.QueryString.HasValue
                ? context.Request.QueryString.Value
                : "No query parameters";
            _logger.LogInformation("Query Parameters: {Query}", queryParams);
        }

        if (logLevel == LogLevelOption.Full || logLevel == LogLevelOption.All || logLevel == LogLevelOption.Custom && logBody)
        {
            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body);
            var body = reader.ReadToEnd();
            if (!string.IsNullOrEmpty(body))
            {
                _logger.LogInformation("Request Body: {Body}", body);
            }
            context.Request.Body.Position = 0;
        }
    }

    private void LogResponse(HttpContext context, MemoryStream memoryStream, LogLevelOption logLevel, string[] customHeaders, bool logBody)
    {
        if (logLevel == LogLevelOption.Basic || logLevel == LogLevelOption.Full || logLevel == LogLevelOption.All)
        {
            _logger.LogInformation("Response: {StatusCode}", context.Response.StatusCode);
        }

        if (logLevel == LogLevelOption.Headers || logLevel == LogLevelOption.Full || logLevel == LogLevelOption.All ||
            logLevel == LogLevelOption.Custom && customHeaders.Length > 0)
        {
            var headersToLog = logLevel == LogLevelOption.Custom && customHeaders.Length > 0
                ? string.Join(", ", context.Response.Headers
                    .Where(h => customHeaders.Contains(h.Key, StringComparer.OrdinalIgnoreCase))
                    .Select(h => $"{h.Key}: {h.Value}"))
                : context.Response.Headers.ToString();

            _logger.LogInformation("Response Headers: {Headers}", headersToLog);
        }

        if (logLevel == LogLevelOption.Full || logLevel == LogLevelOption.All || logLevel == LogLevelOption.Custom && logBody)
        {
            memoryStream.Seek(0, SeekOrigin.Begin);
            var responseBody = new StreamReader(memoryStream).ReadToEnd();
            if (!string.IsNullOrEmpty(responseBody))
            {
                _logger.LogInformation("Response Body: {ResponseBody}", responseBody);
            }
        }
    }
}