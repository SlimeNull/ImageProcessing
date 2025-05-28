using LibImageProcessing.Helpers;
using SkiaSharp;

namespace LibImageProcessing
{
    public class HsvRangeProcessor : DxImageProcessor
    {
        public HsvRangeProcessor(
            int inputWidth, int inputHeight,
            float hueMin, float saturationMin, float valueMin, float alphaMin,
            float hueMax, float saturationMax, float valueMax, float alphaMax,
            bool flipHueRange, bool flipSaturationRange, bool flipValueRange, bool flipAlphaRange,
            SKColor other) : base(inputWidth, inputHeight)
        {
            HueMin = hueMin;
            SaturationMin = saturationMin;
            ValueMin = valueMin;
            AlphaMin = alphaMin;
            HueMax = hueMax;
            SaturationMax = saturationMax;
            ValueMax = valueMax;
            AlphaMax = alphaMax;
            FlipHueRange = flipHueRange;
            FlipSaturationRange = flipSaturationRange;
            FlipValueRange = flipValueRange;
            FlipAlphaRange = flipAlphaRange;
            Other = other;
        }

        public float HueMin { get; }
        public float SaturationMin { get; }
        public float ValueMin { get; }
        public float AlphaMin { get; }
        public float HueMax { get; }
        public float SaturationMax { get; }
        public float ValueMax { get; }
        public float AlphaMax { get; }
        public bool FlipHueRange { get; }
        public bool FlipSaturationRange { get; }
        public bool FlipValueRange { get; }
        public bool FlipAlphaRange { get; }
        public SKColor Other { get; }

        public override int OutputWidth => InputWidth;
        public override int OutputHeight => InputHeight;

        protected internal override string GetShaderCode()
        {
            string
                hueOp1 = "<",
                hueOp2 = ">",
                saturationOp1 = "<",
                saturationOp2 = ">",
                valueOp1 = "<",
                valueOp2 = ">",
                alphaOp1 = "<",
                alphaOp2 = ">";

            if (FlipHueRange)
            {
                (hueOp1, hueOp2) = (">", "<");
            }

            if (FlipSaturationRange)
            {
                (saturationOp1, saturationOp2) = (">", "<");
            }

            if (FlipValueRange)
            {
                (valueOp1, valueOp2) = (">", "<");
            }

            if (FlipAlphaRange)
            {
                (alphaOp1, alphaOp2) = (">", "<");
            }

            return $$"""
                Texture2D    _texture;
                SamplerState _sampler;

                struct vs_in 
                {
                    float3 position : POSITION;
                };

                struct vs_out
                {
                    float4 position : SV_POSITION;
                };

                {{HlslCommonFunctions.RgbToHsv()}}

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
                    float4 rgba = _texture.Sample(_sampler, float2(input.position.x / {{InputWidth}}, input.position.y / {{InputHeight}}));
                    float4 hsva = float4(rgbToHsv(rgba.xyz), rgba.w);

                    if (hsva.x {{hueOp1}} {{HueMin}} || hsva.x {{hueOp2}} {{HueMax}} ||
                        hsva.y {{saturationOp1}} {{SaturationMin}} || hsva.y {{saturationOp2}} {{SaturationMax}} ||
                        hsva.z {{valueOp1}} {{ValueMin}} || hsva.z {{valueOp2}} {{ValueMax}} ||
                        hsva.w {{alphaOp1}} {{AlphaMin}} || hsva.w {{alphaOp2}} {{AlphaMax}})
                    {
                        return float4({{Other.Red / 255.0f}}, {{Other.Green / 255.0f}}, {{Other.Blue / 255.0f}}, rgba.w);
                    }

                    return rgba;
                }
                """;
        }
    }
}
