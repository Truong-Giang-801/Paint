
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using Shapes;

namespace MyLine
{
    public class MyLine : IShape
    {
        private Point _start;
        private Point _end;

        public string Name => "Line";

        public void AddFirst(Point point)
        {
            _start = point;
        }
        public void AddSecond(Point point)
        {
            _end = point;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public UIElement Convert()
        {
            return new Line()
            {
                X1 = _start.X,
                Y1 = _start.Y,
                X2 = _end.X,
                Y2 = _end.Y,
                StrokeThickness = 1,
                Stroke = new SolidColorBrush(Colors.Red)
            }; ;
        }
    }

}
