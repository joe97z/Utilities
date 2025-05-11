# AspNetCore.Logging

A flexible and customizable logging middleware for ASP.NET Core applications. This NuGet package provides a robust system to log HTTP requests and responses with configurable granularity, using attributes to control logging behavior at the controller or action level.

## Features
- **Configurable Log Levels**: Choose from multiple logging levels (`None`, `Basic`, `Headers`, `Query`, `Full`, `All`, `Custom`) to control the amount of information logged.
- **Attribute-Based Configuration**: Use the `MustLog` attribute to specify logging behavior for controllers or actions.
- **Custom Logging**: Log specific headers or request/response bodies with the `Custom` log level.
- **Seamless Integration**: Integrates with ASP.NET Core’s middleware pipeline and logging infrastructure.
- **Performance Optimized**: Efficient handling of request/response streams to minimize overhead.
- **Security Considerations**: Supports selective logging to avoid capturing sensitive data.

## Installation

### Prerequisites
- .NET 9
- ASP.NET Core application with dependency injection configured
- A logging provider (e.g., `Microsoft.Extensions.Logging.Console`)

### Install via NuGet
Install the `AutoLogger` package using the NuGet Package Manager or the .NET CLI:

```bash
dotnet add package AutoLogger
```

Or, via the Package Manager Console:

```powershell
Install-Package AutoLogger
```

## Getting Started

### Step 1: Register the Middleware
In your `Startup.cs` or `Program.cs`, register the `LoggingMiddleware` in the ASP.NET Core pipeline.

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        // Ensure a logging provider is configured
        services.AddLogging(logging => logging.AddConsole());
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseMiddleware<AutoLogger.LoggingMiddleware>();
        app.UseRouting();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}
```

### Step 2: Apply the `MustLog` Attribute
Use the `MustLog` attribute on controllers or action methods to enable logging with the desired log level.

```csharp
using AutoLogger.Logging;

// Basic logging: Logs HTTP method, URL, and status code
[MustLog(LogLevelOption.Basic)]
public class BasicController : ControllerBase
{
    [HttpGet("basic")]
    public IActionResult GetBasic() => Ok("Basic response");
}

// Custom logging: Logs specific headers and body
[MustLog(LogLevelOption.Custom, new[] { "Authorization", "Content-Type" }, logBody: true)]
public class CustomController : ControllerBase
{
    [HttpPost("custom")]
    public IActionResult PostCustom([FromBody] string data) => Ok("Custom response");
}
```

### Step 3: Configure Logging
Ensure your `appsettings.json` or logging configuration enables the `Information` log level for the middleware.

```json
{
    "Logging": {
        "LogLevel": {
            "Default": "Information",
            "AspNetCore.Logging.LoggingMiddleware": "Information"
        }
    }
}
```

## Log Levels
The package supports the following log levels, defined in the `LogLevelOption` enum:

| Log Level | Description | Request Logging | Response Logging |
|-----------|-------------|-----------------|------------------|
| `None`    | No logging | None | None |
| `Basic`   | Minimal info | Method, URL | Status code |
| `Headers` | Headers only | Headers | Headers |
| `Query`   | Query params | Query parameters | None |
| `Full`    | Detailed info | Method, URL, headers, body | Status code, headers, body |
| `All`     | Everything | Method, URL, headers, body, query params | Status code, headers, body |
| `Custom`  | Selective logging | Specific headers/body (configurable) | Specific headers/body (configurable) |

## Usage Examples

### Example 1: Basic Logging
Log only the HTTP method, URL, and status code.

```csharp
[MustLog(LogLevelOption.Basic)]
public class BasicController : ControllerBase
{
    [HttpGet("basic")]
    public IActionResult GetBasic() => Ok("Basic response");
}
```

**Log Output**:
```
INFO: Request: GET /basic
INFO: Response: 200
```

### Example 2: Headers Logging
Log request and response headers.

```csharp
[MustLog(LogLevelOption.Headers)]
public class HeadersController : ControllerBase
{
    [HttpGet("headers")]
    public IActionResult GetHeaders() => Ok("Headers response");
}
```

**Log Output**:
```
INFO: Request Headers: [Host: localhost, Accept: */*]
INFO: Response Headers: [Content-Type: text/plain; charset=utf-8]
```

### Example 3: Custom Logging
Log specific headers and the request/response body.

```csharp
[MustLog(LogLevelOption.Custom, new[] { "Authorization", "Content-Type" }, logBody: true)]
public class CustomController : ControllerBase
{
    [HttpPost("custom")]
    public IActionResult PostCustom([FromBody] string data) => Ok("Custom response");
}
```

**Log Output**:
```
INFO: Request Headers: Authorization: Bearer token, Content-Type: application/json
INFO: Request Body: {"data":"example"}
INFO: Response Headers: Content-Type: text/plain; charset=utf-8
INFO: Response Body: Custom response
```

## Configuration

### Logging Provider
The middleware uses `ILogger<LoggingMiddleware>`. Configure your preferred logging provider (e.g., Console, Serilog, NLog) in `ConfigureServices`.

### Security Considerations
- **Sensitive Data**: Avoid logging sensitive headers (e.g., `Authorization`) or bodies containing personal data. Use the `Custom` log level to selectively log non-sensitive information.
- **Redaction**: Consider implementing custom redaction logic for sensitive fields if needed.

### Performance Tuning
- **Body Logging**: Logging large request/response bodies can increase memory usage. Limit body logging to specific endpoints or use sampling.
- **Buffering**: The middleware enables request buffering for body logging, which may impact performance for large payloads.

## Troubleshooting

- **No Logs**: Verify the `MustLog` attribute is applied and the middleware is registered. Ensure the logger’s log level is `Information` or lower.
- **Body Not Logged**: Confirm the request/response content type is supported (e.g., JSON, text) and `logBody` is `true` for `Custom` level.
- **Stream Issues**: Ensure streams are properly reset after logging to avoid pipeline errors.


## License
This package is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
