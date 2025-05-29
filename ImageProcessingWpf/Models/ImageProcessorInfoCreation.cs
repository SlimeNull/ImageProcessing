namespace ImageProcessingWpf.Models
{
    public class ImageProcessorInfoCreation
    {
        private readonly Func<ImageProcessorInfo> _factory;

        public string Name { get; }
        public string Description { get; }

        public ImageProcessorInfoCreation(string name, string description, Func<ImageProcessorInfo> factory)
        {
            Name = name;
            Description = description;
            _factory = factory;
        }

        public ImageProcessorInfo Instantiate()
        {
            return _factory.Invoke();
        }

        public static IReadOnlyList<ImageProcessorInfoCreation> All { get; } =
        [
            new ImageProcessorInfoCreation("RGB Filter", "fjaowiejfioawjegoie", () => new RgbFilterProcessorInfo()),
            new ImageProcessorInfoCreation("HSV Filter", "fjaowiejfioawjegoie", () => new HsvFilterProcessorInfo()),
            new ImageProcessorInfoCreation("HSL Filter", "fjaowiejfioawjegoie", () => new HslFilterProcessorInfo()),
            new ImageProcessorInfoCreation("HSV Range", "fjaowiejfioweof", () => new HsvRangeProcessorInfo()),
            new ImageProcessorInfoCreation("Box Blur", "fjaowiejfioweof", () => new BoxBlurProcessorInfo()),
            new ImageProcessorInfoCreation("Gradient", "fjaowiejfioweof", () => new GradientProcessorInfo()),
        ];
    }
}
