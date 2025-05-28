using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibImageProcessing.Helpers
{
    internal static class HlslCommonFunctions
    {
        public static string HsvToRgb(string funcName = "hsvToRgb")
        {
            // HSV转RGB函数
            // 输入: hsv (色调[0-1], 饱和度[0-1], 亮度[0-1])
            // 输出: rgb (红[0-1], 绿[0-1], 蓝[0-1])

            return $$"""
                float3 {{funcName}}(float3 hsv)
                {
                    float h = hsv.x;
                    float s = hsv.y;
                    float v = hsv.z;

                    if (s == 0)
                        return float3(v, v, v);

                    h = frac(h);
                    h *= 6.0;

                    int i = floor(h);
                    float f = h - i;
                    float p = v * (1.0 - s);
                    float q = v * (1.0 - s * f);
                    float t = v * (1.0 - s * (1.0 - f));

                    float3 rgb;

                    switch (i)
                    {
                        case 0: rgb = float3(v, t, p); break;
                        case 1: rgb = float3(q, v, p); break;
                        case 2: rgb = float3(p, v, t); break;
                        case 3: rgb = float3(p, q, v); break;
                        case 4: rgb = float3(t, p, v); break;
                        default: rgb = float3(v, p, q); break; // case 5
                    }

                    return rgb;
                }
                """;
        }

        public static string RgbToHsv(string funcName = "rgbToHsv")
        {
            // RGB转HSV函数
            // 输入: rgb (红[0-1], 绿[0-1], 蓝[0-1])
            // 输出: hsv (色调[0-1], 饱和度[0-1], 亮度[0-1])

            return $$"""
                float3 {{funcName}}(float3 rgb)
                {
                    float r = rgb.r;
                    float g = rgb.g;
                    float b = rgb.b;

                    float maxVal = max(max(r, g), b);
                    float minVal = min(min(r, g), b);
                    float delta = maxVal - minVal;

                    // 亮度
                    float v = maxVal;

                    // 饱和度
                    float s = (maxVal != 0.0) ? (delta / maxVal) : 0.0;

                    // 色调
                    float h = 0.0;

                    if (s != 0.0)
                    {
                        if (maxVal == r)
                            h = (g - b) / delta + (g < b ? 6.0 : 0.0);
                        else if (maxVal == g)
                            h = (b - r) / delta + 2.0;
                        else // maxVal == b
                            h = (r - g) / delta + 4.0;

                        h /= 6.0; // 转换到0-1范围
                    }

                    return float3(h, s, v);
                }
                """;
        }

        public static string HslToRgb(string funcName = "hslToRgb")
        {
            return $$"""
                float3 {{funcName}}(float3 hsl)
                {
                    float h = hsl.x;
                    float s = hsl.y;
                    float l = hsl.z;

                    if (s == 0.0)
                        return float3(l, l, l); // 灰度

                    float q = l < 0.5 ? l * (1.0 + s) : l + s - l * s;
                    float p = 2.0 * l - q;

                    // 将h值归一化到0-1
                    h = frac(h);

                    // 计算红色分量
                    float r;
                    float tR = h + 1.0/3.0;
                    tR = frac(tR);

                    if (tR < 1.0/6.0) r = p + (q - p) * 6.0 * tR;
                    else if (tR < 1.0/2.0) r = q;
                    else if (tR < 2.0/3.0) r = p + (q - p) * (2.0/3.0 - tR) * 6.0;
                    else r = p;

                    // 计算绿色分量
                    float g;
                    float tG = h;

                    if (tG < 1.0/6.0) g = p + (q - p) * 6.0 * tG;
                    else if (tG < 1.0/2.0) g = q;
                    else if (tG < 2.0/3.0) g = p + (q - p) * (2.0/3.0 - tG) * 6.0;
                    else g = p;

                    // 计算蓝色分量
                    float b;
                    float tB = h - 1.0/3.0;
                    tB = frac(tB);

                    if (tB < 1.0/6.0) b = p + (q - p) * 6.0 * tB;
                    else if (tB < 1.0/2.0) b = q;
                    else if (tB < 2.0/3.0) b = p + (q - p) * (2.0/3.0 - tB) * 6.0;
                    else b = p;

                    return float3(r, g, b);
                }
                """;
        }

        public static string RgbToHsl(string funcName = "rgbToHsl")
        {
            return $$"""
                float3 {{funcName}}(float3 rgb)
                {
                    float r = rgb.r;
                    float g = rgb.g;
                    float b = rgb.b;

                    float maxVal = max(max(r, g), b);
                    float minVal = min(min(r, g), b);
                    float delta = maxVal - minVal;

                    // 亮度
                    float l = (maxVal + minVal) / 2.0;

                    // 饱和度
                    float s = 0.0;
                    if (l > 0.0 && l < 1.0)
                        s = delta / (l < 0.5 ? (2.0 * l) : (2.0 - 2.0 * l));

                    // 色调
                    float h = 0.0;

                    if (delta > 0.0)
                    {
                        if (maxVal == r)
                            h = (g - b) / delta + (g < b ? 6.0 : 0.0);
                        else if (maxVal == g)
                            h = (b - r) / delta + 2.0;
                        else // maxVal == b
                            h = (r - g) / delta + 4.0;

                        h /= 6.0; // 转换到0-1范围
                    }

                    return float3(h, s, l);
                }
                """;
        }
    }
}
