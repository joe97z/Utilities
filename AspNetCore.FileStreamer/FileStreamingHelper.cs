using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using System.IO;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Threading;

namespace AspNetCore.FileStreamer;
/// <summary>
/// Provides utility methods for streaming large files or data streams over HTTP in ASP.NET Core Web APIs.
/// </summary>
public static class FileStreamingHelper
{    /// <summary>
     /// Streams a file from the given file path to the HTTP response, with optional range request support.
     /// </summary>
     /// <param name="context">The current HTTP context.</param>
     /// <param name="filePath">The full physical path to the file.</param>
     /// <param name="contentType">Optional MIME type. If not provided, it is inferred from the file extension.</param>
     /// <param name="downloadFileName">Optional file name to include in the Content-Disposition header.</param>
     /// <param name="enableRangeProcessing">Whether to support HTTP Range requests (e.g., for video scrubbing).</param>
     /// <param name="configureResponse">Optional delegate to further configure the response (e.g., add custom headers).</param>
     /// <param name="cancellationToken">A cancellation token.</param>
     /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task StreamFileAsync(
        HttpContext context,
        string filePath,
        string contentType = null,
        string downloadFileName = null,
        bool enableRangeProcessing = true,
        Action<HttpResponse> configureResponse = null,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        var fileInfo = new FileInfo(filePath);
        contentType ??= GetMimeType(filePath);
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        await StreamAsync(context, stream, fileInfo.Length, contentType, downloadFileName, enableRangeProcessing, configureResponse, cancellationToken);
    }
    /// <summary>
    /// Streams a given <see cref="Stream"/> to the HTTP response with optional range processing.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="sourceStream">The stream to be written to the response.</param>
    /// <param name="totalLength">Total length of the stream in bytes.</param>
    /// <param name="contentType">The MIME type of the stream (e.g., video/mp4).</param>
    /// <param name="downloadFileName">Optional file name to include in the Content-Disposition header.</param>
    /// <param name="enableRangeProcessing">Whether to support HTTP Range requests.</param>
    /// <param name="configureResponse">Optional delegate to further configure the response.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task StreamAsync(
        HttpContext context,
        Stream sourceStream,
        long totalLength,
        string contentType = "application/octet-stream",
        string downloadFileName = null,
        bool enableRangeProcessing = true,
        Action<HttpResponse> configureResponse = null,
        CancellationToken cancellationToken = default)
    {
        var response = context.Response;
        var request = context.Request;

        long start = 0;
        long end = totalLength - 1;

        response.Headers.AcceptRanges = enableRangeProcessing ? "bytes" : "none";
        response.ContentType = contentType;

        if (enableRangeProcessing && request.Headers.TryGetValue("Range", out var rangeHeader))
        {
            var range = rangeHeader.ToString().Replace("bytes=", "").Split('-');
            if (long.TryParse(range[0], out var parsedStart)) start = parsedStart;
            if (range.Length > 1 && long.TryParse(range[1], out var parsedEnd)) end = parsedEnd;

            response.StatusCode = (int)HttpStatusCode.PartialContent;
            response.Headers.ContentRange = $"bytes {start}-{end}/{totalLength}";
        }

        long contentLength = end - start + 1;
        response.ContentLength = contentLength;

        if (!string.IsNullOrEmpty(downloadFileName))
        {
            response.Headers.ContentDisposition = $"inline; filename=\"{downloadFileName}\"";
        }

        configureResponse?.Invoke(response);

        sourceStream.Seek(start, SeekOrigin.Begin);

        const int bufferSize = 64 * 1024;
        var buffer = new byte[bufferSize];
        long remaining = contentLength;

        while (remaining > 0)
        {
            int read = await sourceStream.ReadAsync(buffer.AsMemory(0, (int)Math.Min(bufferSize, remaining)), cancellationToken);
            if (read == 0) break;

            await response.Body.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            await response.Body.FlushAsync(cancellationToken);
            remaining -= read;
        }
    }

    private static string GetMimeType(string filePath)
    {
        var provider = new FileExtensionContentTypeProvider();
        return provider.TryGetContentType(filePath, out var contentType)
            ? contentType
            : "application/octet-stream";
    }
}