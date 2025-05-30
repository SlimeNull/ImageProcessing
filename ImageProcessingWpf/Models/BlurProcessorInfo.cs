using CommunityToolkit.Mvvm.ComponentModel;
using LibImageProcessing;

namespace ImageProcessingWpf.Models
{
    public partial class BlurProcessorInfo : ImageProcessorInfo
    {
        [ObservableProperty]
        private BlurMethod _method;

        [ObservableProperty]
        private int _blurSize = 3;

        public override string Name => "Blur";

        public override IImageProcessor CreateProcessor(int inputWidth, int inputHeight)
        {
            return new BlurProcessor(inputWidth, inputHeight, Method, BlurSize);
        }
    }
}
