using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;

namespace Neimart.Core.Infrastructure.Imaging
{
    public interface IImageProcessor
    {
        Task<(int width, int height)> GetImageSizeAsync(Stream source);

        Task<Stream> ProcessAsync(Stream source, ImageResizeInfo imageResizeInfo);
    }

    public class ImageResizeInfo
    {
        public int? Width { get; set; }

        public int? Height { get; set; }

        public int? Quality { get; set; }

        public ImageMode? Mode { get; set; }

        public ImageFormat? Format { get; set; }

        public bool HasResize => (Width != null ||
                                  Height != null ||
                                  Quality != null ||
                                  Mode != null ||
                                  Format != null);
    }

    public enum ImageFormat
    {
        Bmp = 0,
        Gif = 1,
        Ico = 2,
        Jpg = 3,
        Png = 4,
        Wbmp = 5,
        Webp = 6,
        Pkm = 7,
        Ktx = 8,
        Astc = 9,
        Dng = 10,
        Heif = 11
    }

    public enum ImageMode
    {
        Pad,
        Max,
        Crop,
        Scale,
        CropScale,
        Stretch
    }
}