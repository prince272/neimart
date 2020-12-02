using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Neimart.Core;
using Neimart.Core.Infrastructure.Imaging;
using Neimart.Core.Utilities.Extensions;
using Neimart.Core.Utilities.Helpers;

namespace Neimart.Core.Infrastructure.Imaging
{
    public class ImageProcessorMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IImageProcessor _imageProcessor;
        private static readonly string[] imageExtensions = new string[] {
            ".png",
            ".jpg",
            ".jpeg"
        };

        public ImageProcessorMiddleware(RequestDelegate next, IWebHostEnvironment webHostEnvironment, IImageProcessor imageProcessor)
        {
            _next = next;
            _webHostEnvironment = webHostEnvironment;
            _imageProcessor = imageProcessor;
        }

        public async Task Invoke(HttpContext context)
        {
            // TODO: Add support for caching.
            if (ShouldProcessImagePath(context.Request, out var imageResizeInfo))
            {
                var imagePath = context.Request.Path;
                var imageFileInfo = _webHostEnvironment.WebRootFileProvider.GetFileInfo(imagePath);

                if (imageFileInfo.Exists)
                {
                    var imageStream = imageFileInfo.CreateReadStream();

                    imageStream = await _imageProcessor.ProcessAsync(imageStream, imageResizeInfo);

                    var imageContentType = WebPathHelper.GetMimeType(Path.GetExtension(imagePath));
                    context.Response.ContentType = imageContentType;
                    context.Response.ContentLength = imageStream.Length;
                    await context.Response.Body.WriteAsync(await imageStream.ReadAllBytesAsync(), 0, (int)imageStream.Length);
                    return;
                }
            }

            await _next.Invoke(context);
        }

        public bool ShouldProcessImagePath(HttpRequest request, out ImageResizeInfo imageResizeInfo)
        {
            imageResizeInfo = null;

            if (request.Path == null || !request.Path.HasValue)
                return false;

            bool isImagePath = imageExtensions.Any(x => request.Path.Value.EndsWith(x, StringComparison.InvariantCultureIgnoreCase));

            if (isImagePath)
            {
                imageResizeInfo = new ImageResizeInfo
                {
                    Width = int.TryParse(request.Query["width"], out int width) ? width : (int?)null,
                    Height = int.TryParse(request.Query["height"], out int height) ? height : (int?)null,
                    Quality = int.TryParse(request.Query["quality"], out int quality) ? quality : (int?)null,
                    Mode = Enum.TryParse(request.Query["mode"], out ImageMode mode) ? mode : (ImageMode?)null,
                    Format = Enum.TryParse(request.Query["format"], out ImageFormat format) ? format : (ImageFormat?)null
                };

                return imageResizeInfo.HasResize;
            }

            return false;
        }
    }

    public static class ImageProcessorMiddlewareExtensions
    {
        public static IApplicationBuilder UseImageProcessor(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ImageProcessorMiddleware>();
        }
    }
}
