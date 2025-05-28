using LibImageProcessing.Helpers;
using SkiaSharp;

namespace LibImageProcessing
{
    public class HslRangeProcessor : DxImageProcessor
    {
        public HslRangeProcessor(
            int inputWidth, int inputHeight,
            float hueMin, float saturationMin, float lightnessMin, float alphaMin,
            float hueMax, float saturationMax, float lightnessMax, float alphaMax,
            bool flipHueRange, bool flipSaturationRange, bool flipLighnessRange, bool flipAlphaRange,
            SKColor other) : base(inputWidth, inputHeight)
        {
            HueMin = hueMin;
            SaturationMin = saturationMin;
            LightnessMin = lightnessMin;
            AlphaMin = alphaMin;
            HueMax = hueMax;
            SaturationMax = saturationMax;
            LightnessMax = lightnessMax;
            AlphaMax = alphaMax;
            FlipHueRange = flipHueRange;
            FlipSaturationRange = flipSaturationRange;
            FlipLighnessRange = flipLighnessRange;
            FlipAlphaRange = flipAlphaRange;
            Other = other;
        }

        public float HueMin { get; }
        public float SaturationMin { get; }
        public float LightnessMin { get; }
        public float AlphaMin { get; }
        public float HueMax { get; }
        public float SaturationMax { get; }
        public float LightnessMax { get; }
        public float AlphaMax { get; }
        public bool FlipHueRange { get; }
        public bool FlipSaturationRange { get; }
        public bool FlipLighnessRange { get; }
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

            if (FlipLighnessRange)
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

                {{HlslCommonFunctions.RgbToHsl()}}

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
                    float4 hsla = float4(rgbToHsl(rgba.xyz), rgba.w);
                
                    if (hsla.x {{hueOp1}} {{HueMin}} || hsla.x {{hueOp2}} {{HueMax}} ||
                        hsla.y {{saturationOp1}} {{SaturationMin}} || hsla.y {{saturationOp2}} {{SaturationMax}} ||
                        hsla.z {{valueOp1}} {{LightnessMin}} || hsla.z {{valueOp2}} {{LightnessMax}} ||
                        hsla.w {{alphaOp1}} {{AlphaMin}} || hsla.w {{alphaOp2}} {{AlphaMax}})
                    {
                        return float4({{Other.Red / 255.0f}}, {{Other.Green / 255.0f}}, {{Other.Blue / 255.0f}}, rgba.w);
                    }

                    return rgba;
                }
                """;
        }
    }
}
