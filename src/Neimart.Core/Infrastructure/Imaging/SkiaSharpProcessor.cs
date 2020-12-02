using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Neimart.Core;
using Neimart.Core.Utilities.Helpers;
using SkiaSharp;

namespace Neimart.Core.Infrastructure.Imaging
{
    public class SkiaSharpProcessor : IImageProcessor
    {
        public SkiaSharpProcessor()
        {

        }

        public Task<(int width, int height)> GetImageSizeAsync(Stream stream)
        {
            using var managedStream = new SKManagedStream(stream);
            using var codec = SKCodec.Create(managedStream);
            return Task.FromResult((codec.Info.Width, codec.Info.Height));
        }

        public Task<Stream> ProcessAsync(Stream source, ImageResizeInfo imageResizeInfo)
        {
            return ProcessImageAsync(source, imageResizeInfo.Width ?? 0, imageResizeInfo.Height ?? 0, imageResizeInfo.Quality ?? 95, imageResizeInfo.Mode ?? ImageMode.CropScale, imageResizeInfo.Format);
        }

        private Task<Stream> ProcessImageAsync(Stream source, int width, int height, int quality, ImageMode mode, ImageFormat? format)
        {
            // this represents the EXIF orientation
            var bitmap = LoadBitmap(source, out var encodedOrigin, out var encodedFormat); // always load as 32bit (to overcome issues with indexed color)

            // if either w or h is 0, set it based on ratio of original image
            if (height == 0)
                height = (int)Math.Round(bitmap.Height * (float)width / bitmap.Width);

            else if (width == 0)
                width = (int)Math.Round(bitmap.Width * (float)height / bitmap.Height);

            // if we need to crop, crop the original before resizing
            if (mode == ImageMode.Crop)
                bitmap = CropBitmap(bitmap, width, height);

            if (mode == ImageMode.Scale)
                bitmap = ScaleBitmap(bitmap, width);

            if (mode == ImageMode.CropScale)
                bitmap = CropScaleBitmap(bitmap, width, height);

            // store padded height and width
            int paddedWidth = width;
            int paddedHeight = height;

            // if we need to pad, or max, set the height or width according to ratio
            if (mode == ImageMode.Pad || mode == ImageMode.Max)
            {
                float bitmapRatio = (float)bitmap.Width / bitmap.Height;
                float resizeRatio = (float)width / height;

                if (bitmapRatio > resizeRatio) // original is more "landscape"
                    height = (int)Math.Round(bitmap.Height * ((float)width / bitmap.Width));
                else
                    width = (int)Math.Round(bitmap.Width * ((float)height / bitmap.Height));
            }

            if (mode == ImageMode.Pad)
            {
                bitmap = ScaleBitmap(bitmap, width);
                bitmap = PadBitmap(bitmap, paddedWidth, paddedHeight, format != ImageFormat.Png);
            }

            var image = SKImage.FromBitmap(bitmap);

            switch (format)
            {
                case ImageFormat.Bmp: encodedFormat = SKEncodedImageFormat.Bmp; break;
                case ImageFormat.Gif: encodedFormat = SKEncodedImageFormat.Gif; break;
                case ImageFormat.Ico: encodedFormat = SKEncodedImageFormat.Ico; break;
                case ImageFormat.Jpg: encodedFormat = SKEncodedImageFormat.Jpeg; break;
                case ImageFormat.Png: encodedFormat = SKEncodedImageFormat.Png; break;
                case ImageFormat.Wbmp: encodedFormat = SKEncodedImageFormat.Wbmp; break;
                case ImageFormat.Webp: encodedFormat = SKEncodedImageFormat.Webp; break;
                case ImageFormat.Pkm: encodedFormat = SKEncodedImageFormat.Pkm; break;
                case ImageFormat.Ktx: encodedFormat = SKEncodedImageFormat.Ktx; break;
                case ImageFormat.Astc: encodedFormat = SKEncodedImageFormat.Astc; break;
                case ImageFormat.Dng: encodedFormat = SKEncodedImageFormat.Dng; break;
                case ImageFormat.Heif: encodedFormat = SKEncodedImageFormat.Heif; break;
            }

            var imageData = image.Encode(encodedFormat, quality);

            // cleanup
            image.Dispose();
            bitmap.Dispose();
            source.Dispose();

            return Task.FromResult(imageData.AsStream(true));
        }

        private SKBitmap LoadBitmap(Stream stream, out SKEncodedOrigin origin, out SKEncodedImageFormat format)
        {
            using (var s = new SKManagedStream(stream))
            {
                using (var codec = SKCodec.Create(s))
                {
                    origin = codec.EncodedOrigin;
                    format = codec.EncodedFormat;

                    // decode the bitmap at the nearest size
                    var bitmap = SKBitmap.Decode(codec);

                    var result = codec.GetPixels(bitmap.Info, bitmap.GetPixels(out IntPtr length));
                    if (result == SKCodecResult.Success || result == SKCodecResult.IncompleteInput)
                    {
                        return bitmap;
                    }
                    else
                    {
                        throw new ArgumentException("Unable to load bitmap from provided data.");
                    }
                }
            }
        }

        private SKBitmap CropBitmap(SKBitmap original, int width, int height)
        {
            var cropRect = new SKRectI
            {
                Size = new SKSizeI(width, height),
                Location = new SKPointI(width < original.Width ? (original.Width - width) / 2 : 0,
                                        height < original.Height ? (original.Height - height) / 2 : 0)
            };

            var bitmap = new SKBitmap(cropRect.Width, cropRect.Height, original.ColorType, original.AlphaType);
            original.ExtractSubset(bitmap, cropRect);
            original.Dispose();

            return bitmap;
        }

        private SKBitmap ResizeBitmap(SKBitmap original, int width, int height)
        {
            var resizedImageInfo = new SKImageInfo(width, height, original.ColorType, original.AlphaType);
            var bitmap = original.Resize(resizedImageInfo, SKFilterQuality.None);

            original.Dispose();

            return bitmap;
        }

        private SKBitmap PadBitmap(SKBitmap original, int paddedWidth, int paddedHeight, bool isOpaque)
        {
            // setup new bitmap and optionally clear
            var bitmap = new SKBitmap(paddedWidth, paddedHeight, isOpaque);
            var canvas = new SKCanvas(bitmap);
            if (isOpaque)
                canvas.Clear(new SKColor(255, 255, 255)); // we could make this color a resizeParam
            else
                canvas.Clear(SKColor.Empty);

            // find co-ords to draw original at
            var left = original.Width < paddedWidth ? (paddedWidth - original.Width) / 2 : 0;
            var top = original.Height < paddedHeight ? (paddedHeight - original.Height) / 2 : 0;

            var drawRect = new SKRectI
            {
                Left = left,
                Top = top,
                Right = original.Width + left,
                Bottom = original.Height + top
            };

            // draw original onto padded version
            canvas.DrawBitmap(original, drawRect);
            canvas.Flush();

            canvas.Dispose();
            original.Dispose();

            return bitmap;
        }

        private SKBitmap ScaleBitmap(SKBitmap original, int width)
        {
            int height = (int)Math.Round(width * ((float)original.Height / original.Width));
            return ResizeBitmap(original, width, height);
        }

        private SKBitmap CropScaleBitmap(SKBitmap original, int width, int height)
        {
            var oldRatio = (float)original.Height / original.Width;
            var newRatio = (float)height / width;
            var cropWidth = original.Width;
            var cropHeight = original.Height;

            if (newRatio < oldRatio)
            {
                // We making the image lower
                cropHeight = (int)Math.Round(original.Width * newRatio);
            }
            else
            {
                // We're making the image thinner
                cropWidth = (int)Math.Round(original.Height / newRatio);
            }

            original = CropBitmap(original, cropWidth, cropHeight);
            original = ResizeBitmap(original, width, height);

            return original;
        }
    }

    public static class UrlHelperExtensions
    {
        public static string ProcessImage(this IUrlHelper urlHelper, string url, int? width = null, int? height = null, int? quality = null,
                                      ImageMode? mode = null, ImageFormat? format = null)
        {
            string queryString = "?";
            if (width != null) queryString += $"width={width}&";
            if (height != null) queryString += $"height={height}&";
            if (quality != null) queryString += $"quality={quality}&";
            if (mode != null) queryString += $"format={format}&";
            if (format != null) queryString += $"mode={mode}&";
            queryString = queryString.TrimEnd('?', '&');

            return url != null ? WebPathHelper.CombineUrlParts(url, queryString) : null;
        }
    }
}