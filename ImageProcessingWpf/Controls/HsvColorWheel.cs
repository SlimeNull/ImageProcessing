using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ImageProcessingWpf.Controls
{
    public class HsvColorWheel : Control
    {
        private Point _centerPoint;
        private double _radius;
        private bool _isDragging;
        private Point _selectedPoint;

        // 选中颜色
        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register("SelectedColor", typeof(Color), typeof(HsvColorWheel),
                new FrameworkPropertyMetadata(Colors.Red, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedColorChanged));

        public Color SelectedColor
        {
            get { return (Color)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }

        // HSV 值
        public static readonly DependencyProperty HueProperty =
            DependencyProperty.Register("Hue", typeof(double), typeof(HsvColorWheel),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnHSVChanged));

        public double Hue
        {
            get { return (double)GetValue(HueProperty); }
            set { SetValue(HueProperty, value); }
        }

        public static readonly DependencyProperty SaturationProperty =
            DependencyProperty.Register("Saturation", typeof(double), typeof(HsvColorWheel),
                new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnHSVChanged));

        public double Saturation
        {
            get { return (double)GetValue(SaturationProperty); }
            set { SetValue(SaturationProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(HsvColorWheel),
                new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnHSVChanged));

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // 选择点标记尺寸
        public static readonly DependencyProperty SelectorSizeProperty =
            DependencyProperty.Register("SelectorSize", typeof(double), typeof(HsvColorWheel),
                new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public double SelectorSize
        {
            get { return (double)GetValue(SelectorSizeProperty); }
            set { SetValue(SelectorSizeProperty, value); }
        }

        static HsvColorWheel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HsvColorWheel),
                new FrameworkPropertyMetadata(typeof(HsvColorWheel)));
        }

        public HsvColorWheel()
        {
            this.Background = Brushes.Transparent;
            this.Focusable = true;

            // 注册事件处理
            this.MouseDown += HSVColorWheel_MouseDown;
            this.MouseMove += HSVColorWheel_MouseMove;
            this.MouseUp += HSVColorWheel_MouseUp;
            this.SizeChanged += HSVColorWheel_SizeChanged;
        }

        private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var wheel = (HsvColorWheel)d;
            Color color = (Color)e.NewValue;

            // 从颜色更新 HSV 值
            var hsv = RGBtoHSV(color.R, color.G, color.B);

            wheel.Hue = hsv.Item1;
            wheel.Saturation = hsv.Item2;
            wheel.Value = hsv.Item3;

            // 更新选中点位置
            wheel.UpdateSelectedPoint();
            wheel.InvalidateVisual();
        }

        private static void OnHSVChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var wheel = (HsvColorWheel)d;

            // 更新 RGB 颜色
            var rgb = HSVtoRGB(wheel.Hue, wheel.Saturation, wheel.Value);
            wheel.SelectedColor = Color.FromRgb(rgb.Item1, rgb.Item2, rgb.Item3);

            // 更新选中点位置
            wheel.UpdateSelectedPoint();
            wheel.InvalidateVisual();
        }

        private void HSVColorWheel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateDimensions();
            UpdateSelectedPoint();
            InvalidateVisual();
        }

        private void HSVColorWheel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _isDragging = true;
                Point pos = e.GetPosition(this);
                UpdateColorFromMousePosition(pos);
                CaptureMouse();
                e.Handled = true;
            }
        }

        private void HSVColorWheel_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                Point pos = e.GetPosition(this);
                UpdateColorFromMousePosition(pos);
                e.Handled = true;
            }
        }

        private void HSVColorWheel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                ReleaseMouseCapture();
                e.Handled = true;
            }
        }

        private void UpdateDimensions()
        {
            _centerPoint = new Point(ActualWidth / 2, ActualHeight / 2);
            _radius = Math.Min(ActualWidth, ActualHeight) / 2 - SelectorSize;
        }

        private void UpdateSelectedPoint()
        {
            double angleRad = Hue * Math.PI / 180;
            double distanceFromCenter = Saturation * _radius;

            double x = _centerPoint.X + distanceFromCenter * Math.Cos(angleRad);
            double y = _centerPoint.Y - distanceFromCenter * Math.Sin(angleRad);

            _selectedPoint = new Point(x, y);
        }

        private void UpdateColorFromMousePosition(Point mousePos)
        {
            // 计算鼠标到中心的矢量
            Vector v = mousePos - _centerPoint;

            // 计算距离和角度
            double distance = v.Length;
            double angleRad = Math.Atan2(-v.Y, v.X);

            // 将角度转换为度（0-360）
            double angleDeg = angleRad * 180 / Math.PI;
            if (angleDeg < 0) angleDeg += 360;

            // 计算饱和度（限制在色轮半径内）
            double saturation = Math.Min(distance / _radius, 1.0);

            // 更新 HSV 属性
            Hue = angleDeg;
            Saturation = saturation;
            // Value 保持不变，由另一个控件控制
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (ActualWidth <= 0 || ActualHeight <= 0)
                return;

            UpdateDimensions();

            // 绘制 HSV 色轮
            DrawColorWheel(drawingContext);

            // 绘制选中点标记
            DrawSelector(drawingContext);
        }

        private void DrawColorWheel(DrawingContext drawingContext)
        {
            // 绘制色轮的半径步长
            double radiusStep = 1.0;

            // 绘制色轮的角度步长（度）
            double angleStep = 1.0;

            // 从外到内绘制色轮
            for (double r = _radius; r >= 0; r -= radiusStep)
            {
                double saturation = r / _radius;

                // 对于每个半径，绘制整圆
                for (double angle = 0; angle < 360; angle += angleStep)
                {
                    double hue = angle;

                    // 计算 RGB 值
                    var rgb = HSVtoRGB(hue, saturation, Value);
                    Color color = Color.FromRgb(rgb.Item1, rgb.Item2, rgb.Item3);

                    // 计算当前点的坐标
                    double angleRad = angle * Math.PI / 180;
                    double x = _centerPoint.X + r * Math.Cos(angleRad);
                    double y = _centerPoint.Y - r * Math.Sin(angleRad);

                    // 绘制点
                    double pixelSize = radiusStep * 1.5; // 稍微大一点以避免小空隙
                    Rect pixelRect = new Rect(
                        x - pixelSize / 2,
                        y - pixelSize / 2,
                        pixelSize,
                        pixelSize);

                    drawingContext.DrawRectangle(new SolidColorBrush(color), null, pixelRect);
                }
            }
        }

        private void DrawSelector(DrawingContext drawingContext)
        {
            // 绘制白色外圈
            drawingContext.DrawEllipse(
                null,
                new Pen(Brushes.White, 2),
                _selectedPoint,
                SelectorSize,
                SelectorSize);

            // 绘制黑色内圈
            drawingContext.DrawEllipse(
                null,
                new Pen(Brushes.Black, 1),
                _selectedPoint,
                SelectorSize - 1,
                SelectorSize - 1);
        }

        // HSV 转 RGB 的辅助方法
        private static Tuple<byte, byte, byte> HSVtoRGB(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            byte v = Convert.ToByte(value);
            byte p = Convert.ToByte(value * (1 - saturation));
            byte q = Convert.ToByte(value * (1 - f * saturation));
            byte t = Convert.ToByte(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return new Tuple<byte, byte, byte>(v, t, p);
            else if (hi == 1)
                return new Tuple<byte, byte, byte>(q, v, p);
            else if (hi == 2)
                return new Tuple<byte, byte, byte>(p, v, t);
            else if (hi == 3)
                return new Tuple<byte, byte, byte>(p, q, v);
            else if (hi == 4)
                return new Tuple<byte, byte, byte>(t, p, v);
            else
                return new Tuple<byte, byte, byte>(v, p, q);
        }

        // RGB 转 HSV 的辅助方法
        private static Tuple<double, double, double> RGBtoHSV(byte r, byte g, byte b)
        {
            double red = r / 255.0;
            double green = g / 255.0;
            double blue = b / 255.0;

            double max = Math.Max(red, Math.Max(green, blue));
            double min = Math.Min(red, Math.Min(green, blue));
            double delta = max - min;

            double hue = 0;
            if (delta != 0)
            {
                if (max == red)
                    hue = ((green - blue) / delta) % 6;
                else if (max == green)
                    hue = (blue - red) / delta + 2;
                else
                    hue = (red - green) / delta + 4;

                hue *= 60;
                if (hue < 0)
                    hue += 360;
            }

            double saturation = (max == 0) ? 0 : delta / max;
            double value = max;

            return new Tuple<double, double, double>(hue, saturation, value);
        }
    }
}
