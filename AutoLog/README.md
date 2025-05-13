# AutoLogger

A flexible and customizable logging middleware for ASP.NET Core applications. This NuGet package provides a robust system to log HTTP requests and responses with configurable granularity, using attributes to control logging behavior at the controller or action level.

## Features
- **Configurable Log Levels**: Choose from multiple logging levels (`None`, `Basic`, `Headers`, `Query`, `Body`, `Full`, `All`, `Custom`) to control the amount of information logged.
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
In your `Program.cs`, register the `LoggingMiddleware` in the ASP.NET Core pipeline.

```csharp
using AutoLogger;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddLogging(logging => logging.AddConsole());

var app = builder.Build();
app.UseMiddleware<LoggingMiddleware>();
app.UseRouting();
app.UseEndpoints(endpoints => endpoints.MapControllers());
app.Run();
```

### Step 2: Apply the `MustLog` Attribute
Use the `MustLog` attribute on controllers or action methods to enable logging with the desired log level.

```csharp
using AutoLogger;

// Basic logging: Logs HTTP method, URL, and status code
[MustLog(LogLevelOption.Basic)]
public class BasicController : ControllerBase
{
    [HttpGet("basic")]
    public IActionResult GetBasic() => Ok("Basic response");
}

// Custom logging: Logs specific headers, body and query
[MustLog(LogLevelOption.Custom, new[] { "Authorization", "Content-Type" }, logBody: true, logQuery: true)]
public class CustomController : ControllerBase
{
    [HttpPost("custom")]
    public IActionResult PostCustom([—fromBody] string data) => Ok("Custom response");
}
```

### Step 3: Configure Logging
Ensure your `appsettings.json` enables the `Information` log level for the middleware.

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
| `Body`    | Body only | Body | Body |
| `Full`    | Detailed info | Method, URL, headers, body | Status code, headers, body |
| `All`     | Comprehensive | Method, URL, query params, body | Status code, body |
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

### Example 2: Body Logging
Log only the request and response bodies.

```csharp
[MustLog(LogLevelOption.Body)]
public class BodyController : ControllerBase
{
    [HttpPost("body")]
    public IActionResult PostBody([FromBody] string data) => Ok("Body response");
}
```

**Log Output**:
```
INFO: Request Body: {"data":"example"}
INFO: Response Body: Body response
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
The middleware uses `ILogger<LoggingMiddleware>`. Configure your preferred logging provider (e.g., Console, Serilog, NLog) in the service configuration.

### Security Considerations
- **Sensitive Data**: Avoid logging sensitive headers (e.g., `Authorization`) or bodies containing personal data. Use the `Custom` log level to selectively log non-sensitive information.
- **Redaction**: Consider implementing custom redaction logic for sensitive fields if needed.

### Performance Tuning
- **Body Logging**: Logging large request/response bodies can increase memory usage. Limit body logging to specific endpoints or use sampling.
- **Buffering**: The middleware enables request buffering for body logging, which may impact performance for large payloads.

## Troubleshooting

- **No Logs**: Verify the `MustLog` attribute is applied and the middleware is registered. Ensure the logger’s log level is `Information` or lower.
- **Body Not Logged**: Confirm the request/response content type is supported (e.g., JSON, text) and `logBody` is `true` for `Custom` or `Body` levels.
- **Stream Issues**: Ensure streams are properly reset after logging to avoid pipeline errors.

## Changelog
### Version 1.1.0
- **Added**: `Body` log level to log only request/response bodies.
- **Added**: `logQuery` parameter to log queries if custom
- **Fixed**: Bug in body reading that caused unexpected errors.
- **Changed**: `LogLevelOption.All` now ignores headers instead of query parameters, logging method, URL, query params, and bodies.

See the full [Changelog](CHANGELOG.md) for details.

## License
This package is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.