using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using ImageProcessingWpf.Controls;
using LibImageProcessing;

namespace ImageProcessingWpf.Models
{
    public partial class HsvRangeProcessorInfo : ImageProcessorInfo
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ColorRange))]
        private float _hueMin = 0f;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ColorRange))]
        private float _hueMax = 1f;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ColorRange))]
        private float _saturationMin = 0f;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ColorRange))]
        private float _saturationMax = 1f;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ColorRange))]
        private float _valueMin = 0f;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ColorRange))]
        private float _valueMax = 1f;

        [ObservableProperty]
        private float _alphaMin = 0f;

        [ObservableProperty]
        private float _alphaMax = 1f;

        [ObservableProperty]
        private Color _otherColor = Colors.Transparent;

        public HsvRange ColorRange => new HsvRange(HueMin, SaturationMin, ValueMin, HueMax, SaturationMax, ValueMax);

        public override string Name => "HSV Range";

        public override IImageProcessor CreateProcessor(int inputWidth, int inputHeight)
        {
            return new HsvRangeProcessor(
                inputWidth, inputHeight,
                HueMin * 360, SaturationMin, ValueMin, AlphaMin, HueMax * 360, SaturationMax, ValueMax, AlphaMax,
                new SkiaSharp.SKColor(OtherColor.R, OtherColor.G, OtherColor.B, OtherColor.A));
        }
    }
}
