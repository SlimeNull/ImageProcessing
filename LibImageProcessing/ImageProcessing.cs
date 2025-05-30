using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace LibImageProcessing
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

        public static void Process(SKBitmap source, SKBitmap dest, IProgress<int> progress, IReadOnlyList<IImageProcessor> processors, CancellationToken cancellationToken = default)
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

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            SKSizeI finalSize = new SKSizeI(source.Width, source.Height);
            for (int processorIndex = 0; processorIndex < processors.Count; processorIndex++)
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

            for (int processorIndex = 0; processorIndex < processors.Count; processorIndex++)
            {
                IImageProcessor? processor = processors[processorIndex];
                var isFirstProcessor = processorIndex == 0;
                var isLastProcessor = processorIndex == processors.Count - 1;

                var processorInputSize = new SKSizeI(processor.InputWidth, processor.InputHeight);
                var processorOutputSize = new SKSizeI(processor.OutputWidth, processor.OutputHeight);

                SKBitmap? inputBuffer = null;
                SKBitmap? outputBuffer = null;
                if (!isFirstProcessor &&
                    !buffers.ContainsKey(processorInputSize))
                {
                    inputBuffer = new SKBitmap(processorInputSize.Width, processorInputSize.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
                }

                if (!isLastProcessor &&
                    !buffers.ContainsKey(processorOutputSize))
                {
                    outputBuffer = new SKBitmap(processorOutputSize.Width, processorOutputSize.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
                }

                if (inputBuffer is not null)
                {
                    buffers[processorInputSize] = inputBuffer;
                }

                if (outputBuffer is not null)
                {
                    if (!buffers.ContainsKey(processorOutputSize))
                    {
                        buffers[processorOutputSize] = outputBuffer;
                    }
                    else
                    {
                        alterBuffers[processorOutputSize] = outputBuffer;
                    }
                }
            }

            try
            {
                SKBitmap currentInput = source;
                for (int processorIndex = 0; processorIndex < processors.Count; processorIndex++)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    IImageProcessor processor = processors[processorIndex];
                    var isFirstProcessor = processorIndex == 0;
                    var isLastProcessor = processorIndex == processors.Count - 1;

                    var inputSize = new SKSizeI(processor.InputWidth, processor.InputHeight);
                    var outputSize = new SKSizeI(processor.OutputWidth, processor.OutputHeight);

                    SKBitmap? outputBuffer;
                    if (!isLastProcessor)
                    {
                        if (buffers.TryGetValue(outputSize, out outputBuffer))
                        {
                            buffers.Remove(outputSize);
                        }
                        else if (alterBuffers.TryGetValue(outputSize, out outputBuffer))
                        {
                            alterBuffers.Remove(outputSize);
                        }
                        else
                        {
                            throw new InvalidOperationException("This would never happen");
                        }
                    }
                    else
                    {
                        outputBuffer = dest;
                    }

                    processor.Process(currentInput.GetPixelSpan(), outputBuffer.GetPixelSpan());
                    progress?.Report(processorIndex + 1);

                    if (!isFirstProcessor)
                    {
                        if (!buffers.ContainsKey(inputSize))
                        {
                            buffers[inputSize] = currentInput;
                        }
                        else if (!alterBuffers.ContainsKey(inputSize))
                        {
                            alterBuffers[inputSize] = currentInput;
                        }
                    }

                    currentInput = outputBuffer;
                }
            }
            finally
            {
                foreach (var buffer in buffers.Values)
                {
                    buffer.Dispose();
                }

                foreach (var buffer in alterBuffers.Values)
                {
                    buffer.Dispose();
                }
            }
        }
    }
}
