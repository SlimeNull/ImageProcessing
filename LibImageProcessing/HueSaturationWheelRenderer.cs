using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibImageProcessing.Helpers;

namespace LibImageProcessing
{
    public sealed class HueSaturationWheelRenderer : DxImageRenderer
    {
        public HueSaturationWheelRenderer(int outputWidth, int outputHeight)
        {
            OutputWidth = outputWidth;
            OutputHeight = outputHeight;
        }

        public override int OutputWidth { get; }

        public override int OutputHeight { get; }

        protected internal override string GetShaderCode()
        {
            var centerX = OutputWidth / 2f;
            var centerY = OutputHeight / 2f;
            var radius = Math.Min(centerX, centerY);

            return $$"""
                struct vs_in 
                {
                    float3 position : POSITION;
                };

                struct vs_out
                {
                    float4 position : SV_POSITION;
                };

                {{HlslCommonFunctions.HsvToRgb()}}

                vs_out vs_main(vs_in input) 
                {
                    vs_out output = 
                    {
                        float4(input.position, 1.0)
                    };

                    return output;
                }

                float4 ps_main(vs_out input) : SV_TARGET
                {
                    float centerX = {{centerX}};
                    float centerY = {{centerY}};
                    float radius = {{radius}};

                    float offsetX = input.position.x - centerX;
                    float offsetY = centerY - input.position.y;
                    float radian = atan2(offsetY, offsetX);
                    float distance = length(float2(offsetX, offsetY));
                
                    float pi = 3.14159265358979323846;
                    float hue = radian / pi / 2;
                    float saturation = clamp(distance / radius, 0.0, 1.0);
                    float value = 1.0; // Full brightness for the hue wheel

                    float3 rgb = hsvToRgb(float3(hue, saturation, value));

                    return float4(rgb, 1);
                }
                """;
        }
    }
}
