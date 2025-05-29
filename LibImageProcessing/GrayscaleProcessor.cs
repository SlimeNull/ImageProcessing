namespace LibImageProcessing
{
    public class GrayscaleProcessor : RgbFilterProcessor
    {
        public GrayscaleProcessor(int inputWidth, int inputHeight, GrayscaleMethod method) : base(inputWidth, inputHeight, method switch
        {
            GrayscaleMethod.Average => "(rgb.r + rgb.g + rgb.b) / 3, (rgb.r + rgb.g + rgb.b) / 3, (rgb.r + rgb.g + rgb.b) / 3, rgba.a",
            GrayscaleMethod.Maximum => "max(rgb), max(rgb), max(rgb), rgba.a",
            GrayscaleMethod.Minimum => "min(rgb), min(rgb), min(rgb), rgba.a",
            GrayscaleMethod.RedOnly => "rgb.r, rgb.r, rgb.r, rgba.a",
            GrayscaleMethod.GreenOnly => "rgb.g, rgb.g, rgb.g, rgba.a",
            GrayscaleMethod.BlueOnly => "rgb.b, rgb.b, rgb.b, rgba.a",

            GrayscaleMethod.HsvValue => "hsv.v, hsv.v, hsv.v, rgba.a",
            GrayscaleMethod.HslLightness => "hsl.l, hsl.l, hsl.l, rgba.a",
            _ => throw new ArgumentException("Invalid method", nameof(method))
        })
        {

        }
    }
}
