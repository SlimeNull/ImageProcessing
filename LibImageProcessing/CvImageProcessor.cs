using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibImageProcessing
{
    public abstract class CvImageProcessor : IImageProcessor
    {
        public int InputWidth { get; }

        public int InputHeight { get; }

        public abstract int OutputWidth { get; }

        public abstract int OutputHeight { get; }

        public CvImageProcessor(int inputWidth, int inputHeight)
        {
            InputWidth = inputWidth;
            InputHeight = inputHeight;
        }

        protected abstract void Process(Mat inputMat, Mat outputMat);

        public unsafe void Process(ReadOnlySpan<byte> inputBgraData, Span<byte> outputBgraData)
        {
            var inputDataSize = InputWidth * InputHeight * 4;
            var outputDataSize = OutputWidth * OutputHeight * 4;

            if (inputBgraData.Length != inputDataSize)
            {
                throw new ArgumentException("Input data size not match", nameof(inputBgraData));
            }

            if (outputBgraData.Length != outputDataSize)
            {
                throw new ArgumentException("Output data size not match", nameof(outputBgraData));
            }

            fixed (byte* inputDataPtr = inputBgraData)
            {
                fixed (byte* outputDataPtr = outputBgraData)
                {
                    var inputMat = Mat.FromPixelData(InputHeight, InputWidth, MatType.CV_8UC4, (nint)inputDataPtr);
                    var outputMat = Mat.FromPixelData(InputHeight, InputWidth, MatType.CV_8UC4, (nint)inputDataPtr);

                    Process(inputMat, outputMat);
                }
            }
        }

        public void Dispose()
        {
            // do nothing
        }
    }
}
