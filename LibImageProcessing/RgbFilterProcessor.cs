using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibImageProcessing
{
    public class RgbFilterProcessor : DxImageProcessor
    {
        public RgbFilterProcessor(int inputWidth, int inputHeight, string filter) : base(inputWidth, inputHeight)
        {
            ArgumentNullException.ThrowIfNull(filter);
            
            Filter = filter;
        }

        public override int OutputWidth => InputWidth;

        public override int OutputHeight => InputHeight;

        public string Filter { get; }

        protected internal override string GetShaderCode()
        {
            var filterCompoments = Filter.Split(',');

            var colorExpression = filterCompoments.Length switch
            {
                1 => $"float4({filterCompoments[0]}, {filterCompoments[0]}, {filterCompoments[0]}, 1.0)",
                3 => $"float4({filterCompoments[0]}, {filterCompoments[1]}, {filterCompoments[2]}, 1.0)",
                4 => $"float4({filterCompoments[0]}, {filterCompoments[1]}, {filterCompoments[2]}, {filterCompoments[3]})",
                _ => "float4(0, 0, 0, 0)"
            };

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
                    float3 rgb = rgba.xyz;

                    return {{colorExpression}};
                }
                """;
        }
    }
}
