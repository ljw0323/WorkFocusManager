using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace WorkFocusManager.Converters
{
    public class ProgressArcConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double angle = 0;

            if (value is double d)
                angle = d;

            double radius = 250;
            Point center = new Point(300, 300);

            double startAngle = -90;
            double endAngle = startAngle + angle;

            Point startPoint = PointOnCircle(radius, startAngle, center);
            Point endPoint = PointOnCircle(radius, endAngle, center);

            bool isLargeArc = angle > 180;

            var figure = new PathFigure
            {
                StartPoint = startPoint,
                IsClosed = false,
                IsFilled = false
            };

            figure.Segments.Add(new ArcSegment
            {
                Point = endPoint,
                Size = new Size(radius, radius),
                SweepDirection = SweepDirection.Clockwise,
                IsLargeArc = isLargeArc
            });

            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);

            return geometry;
        }

        private Point PointOnCircle(double radius, double angleDegrees, Point center)
        {
            double angleRadians = angleDegrees * Math.PI / 180.0;

            double x = center.X + radius * Math.Cos(angleRadians);
            double y = center.Y + radius * Math.Sin(angleRadians);

            return new Point(x, y);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}