using OpenCvSharp;

namespace LibImageProcessing
{
    public class BlurProcessor : CvImageProcessor
    {
        public BlurProcessor(int inputWidth, int inputHeight, BlurMethod method, int blurSize) : base(inputWidth, inputHeight)
        {
            BlurSize = blurSize;
            Method = method;
            OutputWidth = inputWidth;
            OutputHeight = inputHeight;
        }

        public override int OutputWidth { get; }

        public override int OutputHeight { get; }

        public BlurMethod Method { get; }
        public int BlurSize { get; }

        protected override void Process(Mat inputMat, Mat outputMat)
        {
            if (Method == BlurMethod.Box)
            {
                Cv2.Blur(inputMat, outputMat, new Size(BlurSize, BlurSize));
            }
            else if (Method == BlurMethod.Gaussian)
            {
                Cv2.GaussianBlur(inputMat, outputMat, new Size(BlurSize, BlurSize), 0);
            }
        }
    }
}
