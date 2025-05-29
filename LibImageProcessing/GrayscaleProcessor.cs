namespace LibImageProcessing
{
    public class GrayscaleProcessor : RgbFilterProcessor
    {
        public GrayscaleProcessor(int inputWidth, int inputHeight, GrayscaleMethod method) : base(inputWidth, inputHeight, method switch
        {
            GrayscaleMethod.RedOnly => "max(rgb), max(rgb), max(rgb), rgba.a",
            GrayscaleMethod.Maximum => "max(rgb), max(rgb), max(rgb), rgba.a",
            GrayscaleMethod.Minimum => "min(rgb), min(rgb), min(rgb), rgba.a",
            GrayscaleMethod.Average => "(rgb.r + rgb.g + rgb.b) / 3, (rgb.r + rgb.g + rgb.b) / 3, (rgb.r + rgb.g + rgb.b) / 3, rgba.a",
            _ => throw new ArgumentException("Invalid method", nameof(method))
        })
        {

        }
    }
}
