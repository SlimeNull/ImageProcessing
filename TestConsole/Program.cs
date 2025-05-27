using LibImageProcessing;
using SkiaSharp;

List<IImageProcessor> imageProcessors = new List<IImageProcessor>();
SKBitmap input = new SKBitmap(2000, 2000, SKColorType.Bgra8888, SKAlphaType.Unpremul);
SKBitmap output = new SKBitmap(2000, 2000, SKColorType.Bgra8888, SKAlphaType.Unpremul);
for (int i = 0; i < 20; i++)
{
    imageProcessors.Add(new RgbFilterProcessor(2000, 2000, "rgba"));
}

foreach (var imageProcessor in imageProcessors)
{
    imageProcessor.Process(input.GetPixelSpan(), output.GetPixelSpan());
}

foreach (var imageProcessor in imageProcessors)
{
    imageProcessor.Dispose();
}

input.Dispose();
output.Dispose();

Console.WriteLine("END");
Console.ReadKey();