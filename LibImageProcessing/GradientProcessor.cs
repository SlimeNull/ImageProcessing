using OpenCvSharp;

namespace LibImageProcessing
{
    public class GradientProcessor : CvImageProcessor
    {
        public GradientProcessor(int inputWidth, int inputHeight) : base(inputWidth, inputHeight)
        {
        }

        public override int OutputWidth => InputWidth;

        public override int OutputHeight => InputHeight;

        protected override void Process(Mat inputMat, Mat outputMat)
        {
            // TODO: Scharr 不能用在 8UC4 Mat 上

            //Cv2.Scharr(inputMat, outputMat, MatType.CV_8U, 0, 1);
            Cv2.GaussianBlur(inputMat, outputMat, new Size(3, 3), 0);
        }
    }

    public enum GradientDirection
    {
        Both,
        Vertical,
        Horizontal,
    }
}
