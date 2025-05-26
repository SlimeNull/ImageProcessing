using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace LibImageFilter
{
    public class ImageProcessing
    {
        public static void Process(SKBitmap source, SKBitmap dest, IImageProcessor processor)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(dest);
            ArgumentNullException.ThrowIfNull(processor);

            if (source.ColorType != SKColorType.Bgra8888)
            {
                throw new ArgumentException("Invalid color type. It must be Bgra8888.", nameof(source));
            }

            if (dest.ColorType != SKColorType.Bgra8888)
            {
                throw new ArgumentException("Invalid color type. It must be Bgra8888.", nameof(dest));
            }

            if (processor.InputWidth != source.Width ||
                processor.InputHeight != source.Height)
            {
                throw new ArgumentException("Invalid processor input size.", nameof(processor));
            }

            processor.Process(source.GetPixelSpan(), dest.GetPixelSpan());
        }

        public static void Process(SKBitmap source, SKBitmap dest, params IImageProcessor[] processors)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(dest);
            ArgumentNullException.ThrowIfNull(processors);

            if (source.ColorType != SKColorType.Bgra8888)
            {
                throw new ArgumentException("Invalid color type. It must be Bgra8888.", nameof(source));
            }

            if (dest.ColorType != SKColorType.Bgra8888)
            {
                throw new ArgumentException("Invalid color type. It must be Bgra8888.", nameof(dest));
            }

            SKSizeI finalSize = new SKSizeI(source.Width, source.Height);
            for (int processorIndex = 0; processorIndex < processors.Length; processorIndex++)
            {
                IImageProcessor? processor = processors[processorIndex];
                if (processor.InputWidth != finalSize.Width ||
                    processor.InputHeight != finalSize.Height)
                {
                    throw new ArgumentException($"Invalid processor input size. Processor index: {processorIndex}", nameof(processors));
                }

                finalSize = new SKSizeI(processor.OutputWidth, processor.OutputHeight);
            }

            if (dest.Width != finalSize.Width ||
                dest.Height != finalSize.Height)
            {
                throw new ArgumentException($"Invalid destination size. Expected: {finalSize}, Actual: {new SKSizeI(dest.Width, dest.Height)}", nameof(dest));
            }

            Dictionary<SKSizeI, SKBitmap> buffers = new Dictionary<SKSizeI, SKBitmap>();
            Dictionary<SKSizeI, SKBitmap> alterBuffers = new Dictionary<SKSizeI, SKBitmap>();
            buffers[finalSize] = dest;

            foreach (var processor in processors)
            {
                var processorInputSize = new SKSizeI(processor.InputWidth, processor.InputHeight);
                var processorOutputSize = new SKSizeI(processor.OutputWidth, processor.OutputHeight);

                if (!buffers.ContainsKey(processorInputSize))
                {
                    buffers[processorInputSize] = new SKBitmap(processorInputSize.Width, processorInputSize.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
                }

                if (!buffers.ContainsKey(processorOutputSize))
                {
                    buffers[processorOutputSize] = new SKBitmap(processorOutputSize.Width, processorOutputSize.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
                }

                if (processorInputSize == processorOutputSize &&
                    !alterBuffers.ContainsKey(processorOutputSize))
                {
                    alterBuffers[processorOutputSize] = new SKBitmap(processorOutputSize.Width, processorOutputSize.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
                }
            }

            SKBitmap currentInput = source;
            foreach (var processor in processors)
            {

            }
        }

        public static SKBitmap Process(SKBitmap source, IImageProcessor processor)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(processor);

            if (source.ColorType != SKColorType.Bgra8888)
            {
                throw new ArgumentException("Invalid color type. It must be Bgra8888.", nameof(source));
            }

            if (processor.InputWidth != source.Width ||
                processor.InputHeight != source.Height)
            {
                throw new ArgumentException("Invalid processor input size.", nameof(processor));
            }

            SKBitmap dest = new SKBitmap(processor.OutputWidth, processor.OutputHeight, SKColorType.Bgra8888, SKAlphaType.Unpremul);
            processor.Process(source.GetPixelSpan(), dest.GetPixelSpan());

            return dest;
        }

        public static SKBitmap Process(SKBitmap source, params IImageProcessor[] processors)
        {
            throw new NotImplementedException();
        }
    }
}
