using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using LibImageProcessing;

namespace ImageProcessingWpf.Models
{
    public partial class GrayscaleProcessorInfo : ImageProcessorInfo
    {
        [ObservableProperty]
        private GrayscaleMethod _method;

        public override string Name => "Grayscale";

        public override IImageProcessor CreateProcessor(int inputWidth, int inputHeight)
        {
            return new GrayscaleProcessor(inputWidth, inputHeight, Method);
        }
    }
}
