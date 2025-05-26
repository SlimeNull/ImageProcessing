using SkiaSharp;

namespace LibImageProcessing
{
    public class HslRangeProcessor : DxImageProcessor
    {
        public HslRangeProcessor(
            int inputWidth, int inputHeight,
            float hueMin, float saturationMin, float lightnessMin, float alphaMin,
            float hueMax, float saturationMax, float lightnessMax, float alphaMax,
            SKColor other) : base(inputWidth, inputHeight)
        {
            HMin = hueMin;
            SMin = saturationMin;
            LMin = lightnessMin;
            AMin = alphaMin;
            HMax = hueMax;
            SMax = saturationMax;
            LMax = lightnessMax;
            AMax = alphaMax;
            Other = other;
        }

        public float HMin { get; }
        public float SMin { get; }
        public float LMin { get; }
        public float AMin { get; }
        public float HMax { get; }
        public float SMax { get; }
        public float LMax { get; }
        public float AMax { get; }
        public SKColor Other { get; }

        public override int OutputWidth => InputWidth;

        public override int OutputHeight => InputHeight;

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
                
                float3 rgbToHsl(float3 rgb)
                {
                    float3 hsl = float3(0.0, 0.0, 0.0);
                
                    float r = rgb.r;
                    float g = rgb.g;
                    float b = rgb.b;
                
                    float maxVal = max(r, max(g, b));
                    float minVal = min(r, min(g, b));
                    float delta = maxVal - minVal;
                
                    // Calculate Lightness (L)
                    hsl.z = (maxVal + minVal) * 0.5;
                
                    // Calculate Saturation (S)
                    if (delta > 0)
                    {
                        if (hsl.z < 0.5)
                        {
                            hsl.y = delta / (maxVal + minVal);
                        }
                        else
                        {
                            hsl.y = delta / (2.0 - maxVal - minVal);
                        }
                    }
                    else
                    {
                        hsl.y = 0.0;
                    }
                
                    // Calculate Hue (H)
                    if (delta > 0)
                    {
                        if (maxVal == r)
                        {
                            hsl.x = (g - b) / delta;
                        }
                        else if (maxVal == g)
                        {
                            hsl.x = 2.0 + (b - r) / delta;
                        }
                        else
                        {
                            hsl.x = 4.0 + (r - g) / delta;
                        }
                
                        hsl.x *= 60.0;
                        if (hsl.x < 0.0)
                        {
                            hsl.x += 360.0;
                        }
                    }
                    else
                    {
                        hsl.x = 0.0;
                    }
                
                    return hsl;
                }

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

                    if (hsla.x < {{HMin}} || hsla.x > {{HMax}} ||
                        hsla.y < {{SMin}} || hsla.y > {{SMax}} ||
                        hsla.z < {{LMin}} || hsla.z > {{LMax}} ||
                        hsla.w < {{AMin}} || hsla.w > {{AMax}})
                    {
                        return float4({{Other.Red / 255.0f}}, {{Other.Green / 255.0f}}, {{Other.Blue / 255.0f}}, rgba.w);
                    }

                    return rgba;
                }
                """;
        }
    }
}
