using CommunityToolkit.Mvvm.ComponentModel;
using LibImageProcessing;

namespace ImageProcessingWpf.Models
{
    public partial class HsvFilterProcessorInfo : ImageProcessorInfo
    {
        [ObservableProperty]
        private string _filter = "rgba";

        public override string Name => "HSV Filter";

        public override IImageProcessor CreateProcessor(int inputWidth, int inputHeight)
        {
            return new HsvFilterProcessor(inputWidth, inputHeight, Filter);
        }
    }
}
