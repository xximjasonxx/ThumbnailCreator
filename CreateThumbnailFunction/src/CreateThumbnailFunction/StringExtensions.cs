using System;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;

namespace CreateThumbnailFunction.Extensions
{
    public static class StringExtensions
    {
        public static IImageEncoder AsEncoder(this string str)
        {
            switch (str)
            {
                case "image/jpeg":
                case "image/jpg":
                    return new JpegEncoder();

                case "image/png":
                    return new PngEncoder();
            }

            throw new InvalidOperationException("Unsupported MimeType detected");
        }
    }
}