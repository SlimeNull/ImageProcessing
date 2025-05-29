using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibImageProcessing
{
    public class BoxBlurProcessor : DxImageProcessor
    {
        public override int OutputWidth => InputWidth - BlurSize + 1;
        public override int OutputHeight => InputHeight - BlurSize + 1;

        public int BlurSize { get; }

        public BoxBlurProcessor(int inputWidth, int inputHeight, int blurSize) : base(inputWidth, inputHeight)
        {
            if (blurSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(blurSize));
            }

            if (inputWidth - blurSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(inputWidth));
            }

            if (inputHeight - blurSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(inputHeight));
            }

            BlurSize = blurSize;
        }

        protected internal override string GetShaderCode()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(
                """
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
                    float4 result = float4(0, 0, 0, 0);
                
                """);

            for (int x = 0; x < BlurSize; x++)
            {
                for (int y = 0; y < BlurSize; y++)
                {
                    sb.AppendLine(
                        $"""
                            result = result + _texture.Sample(_sampler, float2((input.position.x + {x}) / {InputWidth}, (input.position.y + {y}) / {InputHeight}));
                        """);
                }
            }

            var rectPixelCount = BlurSize * BlurSize;
            sb.AppendLine(
                $$"""
                    result = result / {{rectPixelCount}};

                    return result;
                }
                """);

            return sb.ToString();
        }
    }
}
