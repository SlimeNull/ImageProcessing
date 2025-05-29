using CommunityToolkit.Mvvm.ComponentModel;
using LibImageProcessing;

namespace ImageProcessingWpf.Models
{
    public partial class BoxBlurProcessorInfo : ImageProcessorInfo
    {
        [ObservableProperty]
        private int _blurSize = 3;

        public override string Name => "Box Blur";

        public override IImageProcessor CreateProcessor(int inputWidth, int inputHeight)
        {
            return new BoxBlurProcessor(inputWidth, inputHeight, BlurSize);
        }
    }
}
