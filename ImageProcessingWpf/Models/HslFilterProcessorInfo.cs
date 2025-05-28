using CommunityToolkit.Mvvm.ComponentModel;
using LibImageProcessing;

namespace ImageProcessingWpf.Models
{
    public partial class HslFilterProcessorInfo : ImageProcessorInfo
    {
        [ObservableProperty]
        private string _filter = "rgba";

        public override string Name => "HSL Filter";

        public override IImageProcessor CreateProcessor(int inputWidth, int inputHeight)
        {
            return new HslFilterProcessor(inputWidth, inputHeight, Filter);
        }
    }
}
