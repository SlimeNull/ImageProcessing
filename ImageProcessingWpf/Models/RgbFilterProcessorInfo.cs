using CommunityToolkit.Mvvm.ComponentModel;
using LibImageProcessing;

namespace ImageProcessingWpf.Models
{
    public partial class RgbFilterProcessorInfo : ImageProcessorInfo
    {
        [ObservableProperty]
        private string _filter = "rgba";

        public override string Name => "RGB Filter";

        public override IImageProcessor CreateProcessor(int inputWidth, int inputHeight)
        {
            return new RgbFilterProcessor(inputWidth, inputHeight, Filter);
        }
    }
}
