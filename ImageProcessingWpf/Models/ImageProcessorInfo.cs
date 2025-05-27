using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using LibImageProcessing;

namespace ImageProcessingWpf.Models
{
    public abstract class ImageProcessorInfo : ObservableObject
    {
        public abstract string Name { get; }
        public abstract IImageProcessor CreateProcessor(int inputWidth, int inputHeight);
    }
}
