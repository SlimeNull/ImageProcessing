using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LibImageProcessing;

namespace ImageProcessingWpf.Controls
{
    public class HueSaturationIndicator : FrameworkElement
    {
        private WriteableBitmap? _hueWheelBitmap;

        public HueSaturationRange? Range
        {
            get { return (HueSaturationRange?)GetValue(RangeProperty); }
            set { SetValue(RangeProperty, value); }
        }

        public Pen RangeStroke
        {
            get { return (Pen)GetValue(RangeStrokeProperty); }
            set { SetValue(RangeStrokeProperty, value); }
        }



        protected override unsafe void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            int renderWidth = (int)RenderSize.Width;
            int renderHeight = (int)RenderSize.Height;

            if (_hueWheelBitmap is null ||
                _hueWheelBitmap.PixelWidth != renderWidth ||
                _hueWheelBitmap.PixelHeight != renderHeight)
            {
                _hueWheelBitmap = new WriteableBitmap(
                    renderWidth, renderHeight, 96, 96, PixelFormats.Bgra32, null);
            }

            _hueWheelBitmap.Lock();
            var buffer = new Span<byte>((byte*)_hueWheelBitmap.BackBuffer, _hueWheelBitmap.BackBufferStride * renderHeight);
            using var hueWheelRenderer = new HueSaturationWheelRenderer(renderWidth, renderHeight);
            hueWheelRenderer.Render(buffer);

            _hueWheelBitmap.AddDirtyRect(new Int32Rect(0, 0, renderWidth, renderHeight));
            _hueWheelBitmap.Unlock();

            var centerX = renderWidth / 2;
            var centerY = renderHeight / 2;
            var radius = Math.Min(centerX, centerY);

            drawingContext.PushClip(new EllipseGeometry(new Point(centerX, centerY), radius, radius));
            drawingContext.DrawImage(_hueWheelBitmap, new Rect(0, 0, renderWidth, renderHeight));
            drawingContext.Pop();

            if (Range is { } range)
            {
                // 创建一个黑色画笔，粗度为1
                var pen = new Pen(Brushes.Black, 1);

                // 计算四个角点（极坐标转笛卡尔坐标）
                // Hue范围是0~1，需要转换为0~2π弧度
                double minHueRad = range.HueMin * 2 * Math.PI;
                double maxHueRad = range.HueMax * 2 * Math.PI;

                // 计算四个角点
                Point innerMin = PolarToCartesian(centerX, centerY, range.SaturationMin * radius, minHueRad);
                Point innerMax = PolarToCartesian(centerX, centerY, range.SaturationMin * radius, maxHueRad);
                Point outerMax = PolarToCartesian(centerX, centerY, range.SaturationMax * radius, maxHueRad);
                Point outerMin = PolarToCartesian(centerX, centerY, range.SaturationMax * radius, minHueRad);

                // 创建路径几何图形
                var pathGeometry = new PathGeometry();
                var pathFigure = new PathFigure { StartPoint = innerMin, IsClosed = true };

                // 添加外弧（从topLeft到topRight，沿着小圆弧）
                bool isLargeArc = Math.Abs(range.HueMax - range.HueMin) > 0.5;

                pathFigure.Segments.Add(new ArcSegment(
                    innerMax,
                    new Size(range.SaturationMin * radius, range.SaturationMin * radius),
                    0, // 旋转角度
                    isLargeArc,
                    SweepDirection.Counterclockwise, // 顺时针方向
                    true // 是否为笔画
                ));

                // 添加从topRight到bottomRight的线段（径向线）
                pathFigure.Segments.Add(new LineSegment(outerMax, true));

                // 添加内弧（从bottomRight到bottomLeft，沿着大圆弧）
                pathFigure.Segments.Add(new ArcSegment(
                    outerMin,
                    new Size(range.SaturationMax * radius, range.SaturationMax * radius),
                    0, // 旋转角度
                    isLargeArc,
                    SweepDirection.Clockwise, // 逆时针方向
                    true // 是否为笔画
                ));

                // 添加从bottomLeft到topLeft的线段（径向线）
                pathFigure.Segments.Add(new LineSegment(innerMin, true));

                pathGeometry.Figures.Add(pathFigure);
                drawingContext.DrawGeometry(null, pen, pathGeometry);
            }
        }

        // 辅助方法：将极坐标转换为笛卡尔坐标
        private Point PolarToCartesian(double centerX, double centerY, double radius, double angleInRadians)
        {
            return new Point(
                centerX + radius * Math.Cos(angleInRadians),
                centerY - radius * Math.Sin(angleInRadians) // Y坐标向下为正，所以取负
            );
        }

        public static readonly DependencyProperty RangeProperty =
            DependencyProperty.Register("Range", typeof(HueSaturationRange?), typeof(HueSaturationIndicator), 
                new FrameworkPropertyMetadata(null, flags: FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty RangeStrokeProperty =
            DependencyProperty.Register("RangeStroke", typeof(Pen), typeof(HueSaturationIndicator), 
                new FrameworkPropertyMetadata(new Pen(Brushes.Black, 1), flags: FrameworkPropertyMetadataOptions.AffectsRender));


    }
}
