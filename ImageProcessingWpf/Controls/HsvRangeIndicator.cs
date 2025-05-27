using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ImageProcessingWpf.Controls
{
    public class HsvRangeIndicator : Control
    {
        private Point _centerPoint;
        private double _radius;
        private WriteableBitmap _colorWheelBitmap;
        private bool _needsRedrawColorWheel = true;
        // HSV Range 属性
        public static readonly DependencyProperty RangeProperty =
            DependencyProperty.Register("Range", typeof(HsvRange), typeof(HsvRangeIndicator),
                new FrameworkPropertyMetadata(new HsvRange(0, 0, 0, 1, 1, 1),
                    FrameworkPropertyMetadataOptions.AffectsRender));
        public HsvRange Range
        {
            get { return (HsvRange)GetValue(RangeProperty); }
            set { SetValue(RangeProperty, value); }
        }
        // 范围线条颜色
        public static readonly DependencyProperty RangeStrokeProperty =
            DependencyProperty.Register("RangeStroke", typeof(Brush), typeof(HsvRangeIndicator),
                new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));
        public Brush RangeStroke
        {
            get { return (Brush)GetValue(RangeStrokeProperty); }
            set { SetValue(RangeStrokeProperty, value); }
        }
        // 范围线条粗细
        public static readonly DependencyProperty RangeStrokeThicknessProperty =
            DependencyProperty.Register("RangeStrokeThickness", typeof(double), typeof(HsvRangeIndicator),
                new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));
        public double RangeStrokeThickness
        {
            get { return (double)GetValue(RangeStrokeThicknessProperty); }
            set { SetValue(RangeStrokeThicknessProperty, value); }
        }
        // 是否显示动画效果
        public static readonly DependencyProperty ShowAnimationProperty =
            DependencyProperty.Register("ShowAnimation", typeof(bool), typeof(HsvRangeIndicator),
                new FrameworkPropertyMetadata(true));
        public bool ShowAnimation
        {
            get { return (bool)GetValue(ShowAnimationProperty); }
            set { SetValue(ShowAnimationProperty, value); }
        }
        // Value 值（用于确定色轮亮度）
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(HsvRangeIndicator),
                new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender,
                    (d, e) => ((HsvRangeIndicator)d)._needsRedrawColorWheel = true));
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        private double _dashOffset;
        private DispatcherTimer _animationTimer;
        static HsvRangeIndicator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HsvRangeIndicator),
                new FrameworkPropertyMetadata(typeof(HsvRangeIndicator)));
        }
        public HsvRangeIndicator()
        {
            this.Background = Brushes.Transparent;
            this.SizeChanged += HsvRangeIndicator_SizeChanged;

            // 设置动画计时器
            _animationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(50)
            };

            _animationTimer.Tick += (sender, e) =>
            {
                if (ShowAnimation)
                {
                    _dashOffset = (_dashOffset + 0.5) % 10;
                    // 只重绘范围指示器，不重绘整个色轮
                    InvalidateVisual();
                }
            };

            // 控件可见时启动动画
            this.IsVisibleChanged += (s, e) =>
            {
                if ((bool)e.NewValue && ShowAnimation)
                {
                    if (!_animationTimer.IsEnabled)
                        _animationTimer.Start();
                }
                else if (!ShowAnimation || !(bool)e.NewValue)
                {
                    if (_animationTimer.IsEnabled)
                        _animationTimer.Stop();
                }
            };

            // 显示或隐藏动画时更新状态
            this.IsEnabledChanged += (s, e) =>
            {
                if ((bool)e.NewValue && ShowAnimation && this.IsVisible)
                {
                    if (!_animationTimer.IsEnabled)
                        _animationTimer.Start();
                }
                else
                {
                    if (_animationTimer.IsEnabled)
                        _animationTimer.Stop();
                }
            };
        }
        private void HsvRangeIndicator_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateDimensions();
            _colorWheelBitmap = null; // 尺寸改变时重新创建位图
            _needsRedrawColorWheel = true;
            InvalidateVisual();
        }
        private void UpdateDimensions()
        {
            _centerPoint = new Point(ActualWidth / 2, ActualHeight / 2);
            _radius = Math.Min(ActualWidth, ActualHeight) / 2 - 2; // 留点空间给边框
        }
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == ShowAnimationProperty)
            {
                bool showAnimation = (bool)e.NewValue;
                if (showAnimation && this.IsVisible && this.IsEnabled)
                {
                    if (!_animationTimer.IsEnabled)
                        _animationTimer.Start();
                }
                else if (!showAnimation)
                {
                    if (_animationTimer.IsEnabled)
                        _animationTimer.Stop();
                }
            }
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            if (ActualWidth <= 0 || ActualHeight <= 0)
                return;
            UpdateDimensions();
            // 绘制 HSV 色轮（使用缓存的位图提高性能）
            if (_colorWheelBitmap == null || _needsRedrawColorWheel)
            {
                CreateColorWheelBitmap();
                _needsRedrawColorWheel = false;
            }

            drawingContext.DrawImage(_colorWheelBitmap, new Rect(0, 0, ActualWidth, ActualHeight));

            // 绘制 HSV 范围
            DrawHsvRange(drawingContext);
        }
        private void CreateColorWheelBitmap()
        {
            int width = (int)ActualWidth;
            int height = (int)ActualHeight;

            if (width <= 0 || height <= 0)
                return;

            // 创建位图
            _colorWheelBitmap = new WriteableBitmap(
                width, height, 96, 96, PixelFormats.Bgra32, null);

            // 创建像素数组
            int stride = width * 4; // 4 bytes per pixel (BGRA)
            byte[] pixels = new byte[height * stride];

            // 填充像素
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // 相对于中心的位置
                    double dx = x - _centerPoint.X;
                    double dy = y - _centerPoint.Y;

                    // 距离中心的距离
                    double distance = Math.Sqrt(dx * dx + dy * dy);

                    // 如果在半径内
                    if (distance <= _radius)
                    {
                        // 计算 HSV
                        // 注意：这里将角度转换为 0-1 范围
                        double angle = Math.Atan2(-dy, dx);
                        double hue = ((angle / (2 * Math.PI)) + 0.5) % 1.0; // 转换为 0-1 范围

                        double saturation = distance / _radius;

                        // 计算 RGB
                        var rgb = HSVtoRGB(hue, saturation, Value);

                        int index = y * stride + x * 4;
                        pixels[index] = rgb.Item3;     // B
                        pixels[index + 1] = rgb.Item2; // G
                        pixels[index + 2] = rgb.Item1; // R
                        pixels[index + 3] = 255;       // A (不透明)
                    }
                    // 如果在半径外，保持透明
                }
            }

            // 写入位图
            _colorWheelBitmap.WritePixels(
                new Int32Rect(0, 0, width, height),
                pixels, stride, 0);

            //// 绘制边框
            //using (DrawingContext dc = _colorWheelBitmap.GetDrawingContext())
            //{
            //    dc.DrawEllipse(null, new Pen(Brushes.LightGray, 1), _centerPoint, _radius, _radius);
            //}
        }
        private void DrawHsvRange(DrawingContext drawingContext)
        {
            HsvRange range = Range;
            // 如果范围无效，则不绘制
            if (range.HueMin > range.HueMax ||
                range.SaturationMin > range.SaturationMax ||
                range.ValueMin > range.ValueMax)
            {
                return;
            }
            // 创建虚线笔
            Pen rangePen = new Pen(RangeStroke, RangeStrokeThickness)
            {
                DashStyle = new DashStyle(new double[] { 3, 3 }, ShowAnimation ? _dashOffset : 0),
                LineJoin = PenLineJoin.Round
            };
            // 创建路径几何
            StreamGeometry geometry = new StreamGeometry();

            using (StreamGeometryContext ctx = geometry.Open())
            {
                // 两个半径圆构成的环
                double outerRadius = _radius * range.SaturationMax;
                double innerRadius = _radius * range.SaturationMin;

                // 开始和结束的角度（弧度）
                // 将 0-1 范围的色相转换为弧度
                double startAngle = range.HueMin * 2 * Math.PI;
                double endAngle = range.HueMax * 2 * Math.PI;

                // 调整角度以使色相 0 位于右侧（惯例）
                startAngle = (1.5 * Math.PI - startAngle) % (2 * Math.PI);
                endAngle = (1.5 * Math.PI - endAngle) % (2 * Math.PI);

                // 确保 endAngle > startAngle
                if (endAngle <= startAngle)
                {
                    endAngle += 2 * Math.PI;
                }

                // 计算起点（外圆上）
                double x1 = _centerPoint.X + outerRadius * Math.Cos(startAngle);
                double y1 = _centerPoint.Y + outerRadius * Math.Sin(startAngle);

                // 开始绘制
                ctx.BeginFigure(new Point(x1, y1), false, true);

                // 绘制外圆弧
                bool isLargeArc = (range.HueMax - range.HueMin) > 0.5;
                ctx.ArcTo(
                    new Point(
                        _centerPoint.X + outerRadius * Math.Cos(endAngle),
                        _centerPoint.Y + outerRadius * Math.Sin(endAngle)
                    ),
                    new Size(outerRadius, outerRadius),
                    0, // 旋转角度
                    isLargeArc, // 是否是大弧
                    SweepDirection.Counterclockwise, // 顺时针方向
                    true, // 是否是曲线
                    true  // 是否是平滑连接
                );

                // 画内径连线
                ctx.LineTo(
                    new Point(
                        _centerPoint.X + innerRadius * Math.Cos(endAngle),
                        _centerPoint.Y + innerRadius * Math.Sin(endAngle)
                    ),
                    true, // 是否是平滑连接
                    true  // 是否是平滑连接
                );

                // 绘制内圆弧（反方向）
                ctx.ArcTo(
                    new Point(
                        _centerPoint.X + innerRadius * Math.Cos(startAngle),
                        _centerPoint.Y + innerRadius * Math.Sin(startAngle)
                    ),
                    new Size(innerRadius, innerRadius),
                    0, // 旋转角度
                    isLargeArc, // 是否是大弧
                    SweepDirection.Clockwise, // 顺时针方向
                    true, // 是否是曲线
                    true  // 是否是平滑连接
                );

                // 闭合路径
                ctx.LineTo(
                    new Point(x1, y1),
                    true, // 是否是平滑连接
                    true  // 是否是平滑连接
                );
            }

            // 绘制路径
            drawingContext.DrawGeometry(null, rangePen, geometry);

            // 如果有Value范围限制，绘制一个半透明遮罩
            if (range.ValueMin > 0 || range.ValueMax < 1)
            {
                // 创建一个表示整个色轮的几何
                EllipseGeometry wheelGeometry = new EllipseGeometry(_centerPoint, _radius, _radius);

                // 创建一个表示范围的几何（不考虑Value）
                CombinedGeometry rangeGeometry;

                if (range.HueMax - range.HueMin >= 1.0 || (range.HueMin == 0 && range.HueMax == 1))
                {
                    // 如果是整个色轮的范围，使用两个环
                    EllipseGeometry outerCircle = new EllipseGeometry(_centerPoint, _radius * range.SaturationMax, _radius * range.SaturationMax);
                    EllipseGeometry innerCircle = new EllipseGeometry(_centerPoint, _radius * range.SaturationMin, _radius * range.SaturationMin);

                    rangeGeometry = new CombinedGeometry(
                        GeometryCombineMode.Exclude,
                        outerCircle,
                        innerCircle
                    );
                }
                else
                {
                    rangeGeometry = new CombinedGeometry(
                        GeometryCombineMode.Intersect,
                        wheelGeometry,
                        geometry
                    );
                }

                // 绘制半透明遮罩
                SolidColorBrush maskBrush = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0));

                // 计算哪部分需要被遮罩
                CombinedGeometry maskGeometry = new CombinedGeometry(
                    GeometryCombineMode.Exclude,
                    wheelGeometry,
                    rangeGeometry
                );

                drawingContext.DrawGeometry(maskBrush, null, maskGeometry);

                // 根据Value范围进一步添加遮罩
                if (range.ValueMin > 0 || range.ValueMax < 1)
                {
                    SolidColorBrush valueMaskBrush = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0));
                    drawingContext.DrawGeometry(valueMaskBrush, null, rangeGeometry);

                    // 添加Value范围文本标签
                    FormattedText valueText = new FormattedText(
                        $"V: {range.ValueMin:F1}-{range.ValueMax:F1}",
                        System.Globalization.CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface("Segoe UI"),
                        12,
                        Brushes.White,
                        VisualTreeHelper.GetDpi(this).PixelsPerDip);

                    drawingContext.DrawText(
                        valueText,
                        new Point(
                            _centerPoint.X - valueText.Width / 2,
                            _centerPoint.Y - valueText.Height / 2
                        )
                    );
                }
            }
        }
        // HSV 转 RGB 的辅助方法 (Hue 在 0-1 范围内)
        private static ValueTuple<byte, byte, byte> HSVtoRGB(double hue, double saturation, double value)
        {
            // 首先将 hue 从 0-1 范围转换为 0-360 范围
            hue = hue * 360;

            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);
            value = value * 255;
            byte v = Convert.ToByte(value);
            byte p = Convert.ToByte(value * (1 - saturation));
            byte q = Convert.ToByte(value * (1 - f * saturation));
            byte t = Convert.ToByte(value * (1 - (1 - f) * saturation));
            if (hi == 0)
                return new ValueTuple<byte, byte, byte>(v, t, p);
            else if (hi == 1)
                return new ValueTuple<byte, byte, byte>(q, v, p);
            else if (hi == 2)
                return new ValueTuple<byte, byte, byte>(p, v, t);
            else if (hi == 3)
                return new ValueTuple<byte, byte, byte>(p, q, v);
            else if (hi == 4)
                return new ValueTuple<byte, byte, byte>(t, p, v);
            else
                return new ValueTuple<byte, byte, byte>(v, p, q);
        }
    }
}
