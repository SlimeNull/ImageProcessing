using LibImageProcessing.Helpers;

namespace LibImageProcessing
{
    public class HslFilterProcessor : DxImageProcessor
    {
        private readonly string _shaderColorExpression;

        public HslFilterProcessor(int inputWidth, int inputHeight, string filter) : base(inputWidth, inputHeight)
        {
            ArgumentNullException.ThrowIfNull(filter);

            _shaderColorExpression = ColorFilterExpressionParser.GetShaderExpressionForHslFilter(filter);
            Filter = filter;
        }

        public override int OutputWidth => InputWidth;

        public override int OutputHeight => InputHeight;

        public string Filter { get; }

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
                
                
                {{HlslCommonFunctions.RgbToHsv()}}
                
                {{HlslCommonFunctions.RgbToHsl()}}
                
                {{HlslCommonFunctions.HslToRgb()}}

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
                    float3 rgb = rgba.xyz;
                    float3 hsv = rgbToHsv(rgb);
                    float4 hsva = float4(hsv, rgba.w);
                    float3 hsl = rgbToHsl(rgb);
                    float4 hsla = float4(hsl, rgba.w);
                
                    float4 finalHsla = {{_shaderColorExpression}};
                    float3 finalRgb = hslToRgb(finalHsla.xyz);
                
                    return float4(finalRgb, finalHsla.w);
                }
                """;
        }
    }
}
