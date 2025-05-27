using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibImageProcessing.Helpers;

namespace LibImageProcessing
{
    public class RgbFilterProcessor : DxImageProcessor
    {
        private readonly string? _shaderColorExpression;

        public RgbFilterProcessor(int inputWidth, int inputHeight, string filter) : base(inputWidth, inputHeight)
        {
            ArgumentNullException.ThrowIfNull(filter);
            if (!ShaderColorExpressionParser.GetShaderExpressionForFilter(filter, out _shaderColorExpression))
            {
                throw new ArgumentException(nameof(filter));
            }
            
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
                    float3 rgb = rgba.xyz;
                    float3 hsv = rgbToHsv(rgb);
                    float4 hsva = float4(hsv, rgba.w);
                    float3 hsl = rgbToHsl(rgb);
                    float4 hsla = float4(hsl, rgba.w);

                    return {{_shaderColorExpression}};
                }
                """;
        }
    }
}
