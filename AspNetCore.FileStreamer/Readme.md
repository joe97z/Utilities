
# StreamFlex.AspNetCore – ASP.NET Core Streaming Utility

## Overview
StreamFlex.AspNetCore is a lightweight, production-ready utility for efficiently streaming large files or data streams (e.g., videos, documents) over HTTP in ASP.NET Core Web APIs.

It provides support for:
- Range requests for seeking through large files (e.g., videos)
- Automatic MIME type detection based on file extensions
- Streaming directly from streams (not just file paths)
- Customizable response headers, including optional Content-Disposition for downloads

## Installation
You can install the package from NuGet:

```bash
dotnet add package StreamFlex.AspNetCore
```

## Usage

### Controller Example – Stream Video
```csharp
[HttpGet("video/{fileName}")]
public async Task<IActionResult> StreamVideo(string fileName, CancellationToken ct)
{
    try
    {
        var filePath = Path.Combine("Media", "Videos", fileName);

        await FileStreamingHelper.StreamFileAsync(
            HttpContext,
            filePath,
            downloadFileName: fileName,
            enableRangeProcessing: true,
            cancellationToken: ct
        );

        return new EmptyResult();
    }
    catch (OperationCanceledException) when (ct.IsCancellationRequested)
    {
        _logger.LogInformation("Video stream was canceled by the client: {FileName}", fileName);
        return new EmptyResult();
    }
}
```

### Controller Example – Download Document
```csharp
[HttpGet("document/{fileName}")]
public async Task<IActionResult> DownloadFile(string fileName, CancellationToken ct)
{
    try
    {
        var filePath = Path.Combine("Media", "Documents", fileName);

        await FileStreamingHelper.StreamFileAsync(
            HttpContext,
            filePath,
            downloadFileName: fileName,
            enableRangeProcessing: false,
            cancellationToken: ct
        );

        return new EmptyResult();
    }
    catch (OperationCanceledException) when (ct.IsCancellationRequested)
    {
        _logger.LogInformation("Document download was canceled by the client: {FileName}", fileName);
        return new EmptyResult();
    }
}

```

## API Reference

### StreamFileAsync(...)
```csharp
Task StreamFileAsync(
    HttpContext context,
    string filePath,
    string? contentType = null,
    string? downloadFileName = null,
    bool enableRangeProcessing = true,
    Action<HttpResponse>? configureResponse = null,
    CancellationToken cancellationToken = default
)
```
Streams a file from the given file path to the HTTP response with optional range support.

### StreamAsync(...)
```csharp
Task StreamAsync(
    HttpContext context,
    Stream sourceStream,
    long totalLength,
    string contentType = "application/octet-stream",
    string? downloadFileName = null,
    bool enableRangeProcessing = true,
    Action<HttpResponse>? configureResponse = null,
    CancellationToken cancellationToken = default
)
```
Streams from any provided Stream, useful for blob storage or in-memory data.


## HTML5 Video Player Example
You can use the API to stream video directly in HTML5 players:

```html
<video width="720" controls>
  <source src="https://yourdomain.com/api/media/video/sample.mp4" type="video/mp4">
  Your browser does not support the video tag.
</video>
```
This enables seeking through the video stream.

## Security Considerations
- Validate input to ensure file paths are safe and do not allow directory traversal.
- Limit access to certain folders (e.g., private content) via authorization checks.
- Rate-limit or throttle requests for large files to avoid excessive load.

## License
StreamingFileHandler is licensed under the MIT License. See the LICENSE file for more details.

## 🛠️ Contributing
Feel free to fork the repository, file issues, or submit pull requests for improvements.

## 📄 Changelog
**v1.0.0** - Initial release with support for file streaming and HTTP range requests.
