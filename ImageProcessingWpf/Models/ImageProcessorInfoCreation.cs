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
            new ImageProcessorInfoCreation("RGB Filter", "fjaowiejfioawjegoie", () => new RgbFilterProcessorInfo())
        ];
    }
}
