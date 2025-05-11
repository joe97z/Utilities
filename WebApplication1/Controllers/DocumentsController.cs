using AspNetCore.FileStreamer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        [HttpGet("{fileName}")]
        public async Task<IActionResult> Get(string fileName, CancellationToken ct)
        {
            var filePath = Path.Combine("Documents", fileName);

            await FileStreamingHelper.StreamFileAsync(
                HttpContext,
                filePath,
                contentType: null,
                downloadFileName: fileName,
                enableRangeProcessing: false,
                cancellationToken: ct
            );

            return new EmptyResult();
        }
    }
}
