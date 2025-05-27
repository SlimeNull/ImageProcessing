using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using SkiaSharp;

namespace ImageProcessingWpf.Extensions
{
    internal static class ImagingExtensions
    {
        public static PixelFormat ToWpf(this SKColorType colorType)
        {
            return colorType switch
            {
                SKColorType.Bgra8888 => PixelFormats.Bgra32,
                _ => throw new NotSupportedException()
            };
        }
    }
}
