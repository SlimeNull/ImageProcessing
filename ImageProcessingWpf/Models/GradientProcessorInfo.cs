using LibImageProcessing;

namespace ImageProcessingWpf.Models
{
    public partial class GradientProcessorInfo : ImageProcessorInfo
    {
        public override string Name => "Gradient";

        public override IImageProcessor CreateProcessor(int inputWidth, int inputHeight)
        {
            return new GradientProcessor(inputWidth, inputHeight);
        }
    }
}
