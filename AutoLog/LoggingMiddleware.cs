using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

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
            var (logLevelOption, customHeaders, logBody, logQuery) = GetLogLevelOptions(actionDescriptor);

            if (logLevelOption == LogLevelOption.None)
            {
                await _next(context);
                return;
            }

            await LogRequest(context, logLevelOption, customHeaders, logBody, logQuery);

            var originalResponseBody = context.Response.Body;
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            await _next(context);

            await LogResponse(context, memoryStream, logLevelOption, customHeaders, logBody);

            memoryStream.Seek(0, SeekOrigin.Begin);
            await memoryStream.CopyToAsync(originalResponseBody);
        }
        else
        {
            await _next(context);
        }
    }

    private static (LogLevelOption LogLevel, string[] CustomHeaders, bool LogBody, bool logQuery) GetLogLevelOptions(ControllerActionDescriptor actionDescriptor)
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
            attribute?.LogBody ?? false,
            attribute?.LogQuery ?? false
        );
    }

    private void LogRequestPath(HttpContext context, LogLevelOption logLevel)
    {
        _logger.LogInformation("Request: {Method} {Url}", context.Request.Method, context.Request.Path);
    }

    private void LogRequestHeader(HttpContext context, LogLevelOption logLevel, string[] customHeaders)
    {
        var shouldLogHeader = logLevel == LogLevelOption.Headers || logLevel == LogLevelOption.Full ||
                              logLevel == LogLevelOption.Custom && customHeaders.Length > 0;
        if (shouldLogHeader)
        {
            var shouldFilterHeaders = customHeaders.Length > 0;
            string headersToLog;
            if (shouldFilterHeaders)
            {
                headersToLog = string.Join(", ", context.Request.Headers
                     .Where(h => customHeaders.Contains(h.Key, StringComparer.OrdinalIgnoreCase))
                     .Select(h => $"{h.Key}: {h.Value}"));
            }
            else
            {
                headersToLog = string.Join(", ", context.Request.Headers.Select(h => $"{h.Key}: {h.Value}"));
            }

            _logger.LogInformation("Request Headers: {Headers}", headersToLog);
        }
    }

    private async Task LogRequestBody(HttpContext context, LogLevelOption logLevel, bool logBody)
    {
        var shouldLogBody = logLevel == LogLevelOption.Full || logLevel == LogLevelOption.All || 
                            logLevel == LogLevelOption.Custom && logBody;
        if (shouldLogBody)
        {
            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body);
            var body = await reader.ReadToEndAsync();
            if (!string.IsNullOrEmpty(body))
            {
                _logger.LogInformation("Request Body: {Body}", body);
            }
            context.Request.Body.Position = 0;
        }
    }
    private void LogRequestQuery(HttpContext context, LogLevelOption logLevel, bool logQuery)
    {
        var shouldLogQuery = logLevel == LogLevelOption.Query || logLevel == LogLevelOption.All || 
                             logLevel == LogLevelOption.Full || logLevel == LogLevelOption.Custom && logQuery;
        if (shouldLogQuery)
        {
            var queryParams = context.Request.QueryString.HasValue
                ? context.Request.QueryString.Value
                : "No query parameters";
            _logger.LogInformation("Query Parameters: {Query}", queryParams);
        }
    }

    private async Task LogRequest(HttpContext context, LogLevelOption logLevel, string[] customHeaders, bool logBody, bool logQuery)
    {
        LogRequestPath(context, logLevel);
        LogRequestHeader(context, logLevel, customHeaders);
        LogRequestQuery(context, logLevel, logQuery);
        await LogRequestBody(context, logLevel, logBody);
    }
    private void LogResponseStatusCode(HttpContext context, LogLevelOption logLevel)
    {
        _logger.LogInformation("Response: {StatusCode}", context.Response.StatusCode);
    }

    private void LogResponseHeader(HttpContext context, LogLevelOption logLevel, string[] customHeaders)
    {
        var shouldLogHeader = logLevel == LogLevelOption.Headers || logLevel == LogLevelOption.Full ||
                              logLevel == LogLevelOption.Custom && customHeaders.Length > 0;
        var shouldFilterHeaders = customHeaders.Length > 0;
        string headersToLog;
        if (shouldFilterHeaders)
        {
            headersToLog = string.Join(", ", context.Response.Headers
                 .Where(h => customHeaders.Contains(h.Key, StringComparer.OrdinalIgnoreCase))
                 .Select(h => $"{h.Key}: {h.Value}"));
        }
        else
        {
            headersToLog = string.Join(", ", context.Response.Headers.Select(h => $"{h.Key}: {h.Value}"));
        }
        _logger.LogInformation("Response Headers: {Headers}", headersToLog);

    }

    private async Task LogResponseBody(MemoryStream memoryStream, LogLevelOption logLevel, bool logBody)
    {
        var shouldLogBody = logLevel == LogLevelOption.Full || logLevel == LogLevelOption.All ||
                            logLevel == LogLevelOption.Custom && logBody;
        if (shouldLogBody)
        {
            memoryStream.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();
            if (!string.IsNullOrEmpty(responseBody))
            {
                _logger.LogInformation("Response Body: {ResponseBody}", responseBody);
            }
        }
    }

    private async Task LogResponse(HttpContext context, MemoryStream memoryStream, LogLevelOption logLevel, string[] customHeaders, bool logBody)
    {
        LogResponseStatusCode(context, logLevel);
        LogResponseHeader(context, logLevel, customHeaders);
        await LogResponseBody(memoryStream, logLevel, logBody);

    }
}