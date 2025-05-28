using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessingWpf.Controls
{
    public record struct HueSaturationRange(float HueMin, float SaturationMin, float HueMax, float SaturationMax);
}
