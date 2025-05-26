using SkiaSharp;

namespace LibImageFilter
{
    public class HsvRangeProcessor : DxImageProcessor
    {
        public HsvRangeProcessor(
            int inputWidth, int inputHeight,
            float hueMin, float saturationMin, float valueMin, float alphaMin,
            float hueMax, float saturationMax, float valueMax, float alphaMax,
            SKColor other) : base(inputWidth, inputHeight)
        {
            HMin = hueMin;
            SMin = saturationMin;
            VMin = valueMin;
            AMin = alphaMin;
            HMax = hueMax;
            SMax = saturationMax;
            VMax = valueMax;
            AMax = alphaMax;
            Other = other;
        }

        public float HMin { get; }
        public float SMin { get; }
        public float VMin { get; }
        public float AMin { get; }
        public float HMax { get; }
        public float SMax { get; }
        public float VMax { get; }
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
                
                float3 rgbToHsv(float3 rgb)
                {
                    float3 hsv = float3(0.0, 0.0, 0.0);
                
                    float r = rgb.r;
                    float g = rgb.g;
                    float b = rgb.b;
                
                    float maxVal = max(r, max(g, b));
                    float minVal = min(r, min(g, b));
                    float delta = maxVal - minVal;
                
                    // Calculate Value (V)
                    hsv.z = maxVal;
                
                    // Calculate Saturation (S)
                    if (maxVal > 0)
                    {
                        hsv.y = delta / maxVal;
                    }
                    else
                    {
                        hsv.y = 0.0;
                    }
                
                    // Calculate Hue (H)
                    if (delta > 0)
                    {
                        if (maxVal == r)
                        {
                            hsv.x = (g - b) / delta;
                        }
                        else if (maxVal == g)
                        {
                            hsv.x = 2.0 + (b - r) / delta;
                        }
                        else
                        {
                            hsv.x = 4.0 + (r - g) / delta;
                        }
                
                        hsv.x *= 60.0;
                        if (hsv.x < 0.0)
                        {
                            hsv.x += 360.0;
                        }
                    }
                    else
                    {
                        hsv.x = 0.0;
                    }
                
                    return hsv;
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
                    float4 hsva = float4(rgbToHsv(rgba.xyz), rgba.w);

                    if (hsva.x < {{HMin}} || hsva.x > {{HMax}} ||
                        hsva.y < {{SMin}} || hsva.y > {{SMax}} ||
                        hsva.z < {{VMin}} || hsva.z > {{VMax}} ||
                        hsva.w < {{AMin}} || hsva.w > {{AMax}})
                    {
                        return float4({{Other.Red / 255.0f}}, {{Other.Green / 255.0f}}, {{Other.Blue / 255.0f}}, rgba.w);
                    }

                    return rgba;
                }
                """;
        }
    }
}
