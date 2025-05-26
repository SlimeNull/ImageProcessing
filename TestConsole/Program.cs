// See https://aka.ms/new-console-template for more information
using LibImageFilter;
using SkiaSharp;

Console.WriteLine("Hello, World!");

var bitmap = SKBitmap.Decode(@"C:\Users\SlimeNull\OneDrive\Pictures\QQ图片20210512155144.jpg");
var processor = new RgbFilterProcessor(bitmap.Width, bitmap.Height, "rgb.r");
var result = ImageProcessing.Process(bitmap, processor);

using var output = File.Create("output,png");
result.Encode(output, SKEncodedImageFormat.Png, 100);