using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using LibImageProcessing;

namespace ImageProcessingWpf.Models
{
    public partial class HsvRangeProcessorInfo : ImageProcessorInfo
    {
        [ObservableProperty]
        private float _hueMin = 0f;

        [ObservableProperty]
        private float _hueMax = 1f;

        [ObservableProperty]
        private float _saturationMin = 0f;

        [ObservableProperty]
        private float _saturationMax = 1f;

        [ObservableProperty]
        private float _valueMin = 0f;

        [ObservableProperty]
        private float _valueMax = 1f;

        [ObservableProperty]
        private float _alphaMin = 0f;

        [ObservableProperty]
        private float _alphaMax = 1f;

        [ObservableProperty]
        private Color _otherColor = Colors.Transparent;

        public override string Name => "HSV Range";

        public override IImageProcessor CreateProcessor(int inputWidth, int inputHeight)
        {
            return new HsvRangeProcessor(
                inputWidth, inputHeight,
                HueMin, SaturationMin, ValueMin, AlphaMin, HueMax, SaturationMax, ValueMax, AlphaMax,
                new SkiaSharp.SKColor(OtherColor.R, OtherColor.G, OtherColor.B, OtherColor.A));
        }
    }
}
