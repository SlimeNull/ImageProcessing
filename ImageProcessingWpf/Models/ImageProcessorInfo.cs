using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using LibImageProcessing;

namespace ImageProcessingWpf.Models
{
    public abstract partial class ImageProcessorInfo : ObservableObject
    {
        public abstract string Name { get; }

        [ObservableProperty]
        private bool _isExpanded = false;

        public abstract IImageProcessor CreateProcessor(int inputWidth, int inputHeight);
    }
}
