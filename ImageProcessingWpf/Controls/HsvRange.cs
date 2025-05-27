using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessingWpf.Controls
{
    public record struct HsvRange(float HueMin, float SaturationMin, float ValueMin, float HueMax, float SaturationMax, float ValueMax);
}
