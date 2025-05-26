using Silk.NET.Direct3D11;
using SkiaSharp;

namespace LibImageProcessing
{
    public class RgbRangeProcessor : DxImageProcessor
    {
        public RgbRangeProcessor(
            int inputWidth, int inputHeight,
            float redMin, float greenMin, float blueMin, float alphaMin,
            float redMax, float greenMax, float blueMax, float alphaMax,
            SKColor other) : base(inputWidth, inputHeight)
        {
            RedMin = redMin;
            GreenMin = greenMin;
            BlueMin = blueMin;
            AlphaMin = alphaMin;
            RedMax = redMax;
            GreenMax = greenMax;
            BlueMax = blueMax;
            AlphaMax = alphaMax;
            Other = other;
        }

        public override int OutputWidth => InputWidth;
        public override int OutputHeight => InputHeight;

        public float RedMin { get; }
        public float GreenMin { get; }
        public float BlueMin { get; }
        public float AlphaMin { get; }
        public float RedMax { get; }
        public float GreenMax { get; }
        public float BlueMax { get; }
        public float AlphaMax { get; }
        public SKColor Other { get; }

        protected internal override string GetShaderCode()
        {
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
                    
                    if (rgba.x < {{RedMin}} || rgba.x > {{RedMax}} ||
                        rgba.y < {{GreenMin}} || rgba.y > {{GreenMax}} ||
                        rgba.z < {{BlueMin}} || rgba.z > {{BlueMax}} ||
                        rgba.w < {{AlphaMin}} || rgba.w > {{AlphaMax}})
                    {
                        return float4({{Other.Red / 255.0f}}, {{Other.Green / 255.0f}}, {{Other.Blue / 255.0f}}, rgba.w);
                    }

                    return rgba;
                }
                """;
        }
    }
}
