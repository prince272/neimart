using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Neimart.Core;
using Neimart.Core.Entities;
using Neimart.Core.Filters;
using Neimart.Core.Infrastructure.Imaging;
using Neimart.Core.Infrastructure.Web;
using Neimart.Core.Services;
using Neimart.Core.Settings;
using Neimart.Core.Utilities.Extensions;
using Neimart.Core.Utilities.Helpers;
using Neimart.Web.Models;

namespace Neimart.Web.Areas.Portal.Controllers
{
    [Authorize]
    [PlanRequired]
    public class MediaController : Controller
    {
        private readonly MediaService _mediaService;
        private readonly ILogger<MediaController> _logger;
        private readonly AppSettings _appSettings;
        private readonly IImageProcessor _imageProcessor;

        public MediaController(IServiceProvider services)
        {
            _mediaService = services.GetRequiredService<MediaService>();
            _logger = services.GetRequiredService<ILogger<MediaController>>();
            _appSettings = services.GetRequiredService<IOptions<AppSettings>>().Value;
            _imageProcessor = services.GetRequiredService<IImageProcessor>();
        }

        [HttpPost]
        public async Task<IActionResult> Upload()
        {
            string fileExtension = System.IO.Path.GetExtension(Request.Headers["Upload-Name"]).ToLowerInvariant();
            long fileSize = long.Parse(Request.Headers["Upload-Size"]);
            long fileMaxSize = long.Parse(Request.Headers["Upload-Max-Size"]);
            string[] fileExtensions = Request.Headers["Upload-Extensions"].ToString().ToJsonObject<string[]>();

            if (!_appSettings.IsAllowedFileExtension(fileExtension) ||
                !_appSettings.IsAllowedFileExtension(fileExtension, fileExtensions))
            {
                var error = new ProblemDetails
                {
                    Detail = $"The file extension is not allowed."
                };
                return BadRequest(error.ToJsonString());
            }

            if (!_appSettings.IsAllowedFileSize(fileSize) ||
                !_appSettings.IsAllowedFileSize(fileSize, fileMaxSize))
            {
                var error = new ProblemDetails
                {
                    Detail = $"The file size is too large."
                };
                return BadRequest(error.ToJsonString());
            }

            var user = await HttpContext.GetMemberAsync();
            string directoryName = user.Id.ToString();
            string fileName = $"{Guid.NewGuid()}{fileExtension}";

            await _mediaService.PrepareSourceAsync(directoryName, fileName);
            return Ok(fileName);
        }

        [HttpPatch]
        public async Task<IActionResult> Upload(string fileName)
        {
            var user = await HttpContext.GetMemberAsync();

            long length = long.Parse(Request.Headers["Upload-Length"]);
            long offset = long.Parse(Request.Headers["Upload-Offset"]);

            string directoryName = user.Id.ToString();
            var source = (Stream)new MemoryStream(await Request.Body.ReadAllBytesAsync());

            var patchCompleted = await _mediaService.PatchSourceAsync(directoryName, fileName, source, offset, length);

            await source.DisposeAsync();

            if (patchCompleted)
            {
                var imageResizeJsonString = Request.Headers["Upload-Image-Resize"].ToString();
                var imageResize = !string.IsNullOrWhiteSpace(imageResizeJsonString) ? imageResizeJsonString.ToJsonObject<ImageResizeInfo>() : null;

                if (imageResize != null)
                {
                    source = await _mediaService.GetSourceAsync(directoryName, fileName);
                    source = await _imageProcessor.ProcessAsync(source, imageResize);
                    await _mediaService.ReplaceSourceAsync(directoryName, fileName, source);
                }
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Delete()
        {
            var user = await HttpContext.GetMemberAsync();

            var directoryName = user.Id.ToString();
            var fileName = await Request.Body.ReadAllTextAsync();
            await _mediaService.DeleteSourceAsync(directoryName, fileName);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> Load(string fileName)
        {
            var user = await HttpContext.GetMemberAsync();

            var directoryName = user.Id.ToString();

            var media = await _mediaService.GetAsync(directoryName, fileName);

            if (media == null) return NotFound();

            var source = await _mediaService.GetSourceAsync(media);

            if (source == null) return NotFound();

            return File(source, media.ContentType, media.FileTitle);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Download(string token)
        {
            var expiry = await _mediaService.GetExpiryFromTokenAsync(token);

            if (expiry.HasExpired())
            {
                return StatusCode(StatusCodes.Status410Gone);
            }

            var media = await _mediaService.GetAsync(expiry);

            if (media == null) return NotFound();

            var source = await _mediaService.GetSourceAsync(media);

            if (source == null) return NotFound();

            return File(source, media.ContentType, media.FileTitle);
        }
    }
}