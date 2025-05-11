using AspNetCore.FileStreamer;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

[ApiController]
[Route("[controller]")]
public class VideosController() : ControllerBase
{
    [HttpGet("{fileName}")]
    public async Task<IActionResult> Get(string fileName, CancellationToken ct)
    {
        var filePath = Path.Combine( "Videos", fileName);
        await FileStreamingHelper.StreamFileAsync(
            HttpContext,
            filePath,
            contentType: null,
            downloadFileName: fileName,
            enableRangeProcessing: true,
            configureResponse: res => res.Headers["X-Stream-Type"] = "video",
            cancellationToken: ct
        );

        return new EmptyResult();
    }
}
