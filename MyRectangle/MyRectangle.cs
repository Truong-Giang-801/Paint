
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using Shapes;
using System.Windows.Shapes;

namespace MyRectangle
{
    public class MyRectangle : IShape
    {
        private Point _topLeft;
        private Point _rightBottom;
        public string Name => "Rectangle";
        public void AddFirst(Point point)
        {
            _topLeft = point;
        }

        public void AddSecond(Point point)
        {
            _rightBottom = point;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public UIElement Convert()
        {
            var item = new Rectangle()
            {   // TODO: end luon luon lon hon start
                Width = Math.Abs(_rightBottom.X - _topLeft.X),
                Height = Math.Abs(_rightBottom.Y - _topLeft.Y),
                StrokeThickness = 1,
                Stroke = new SolidColorBrush(Colors.Green)
            };
            Canvas.SetLeft(item, Math.Min(_topLeft.X, _rightBottom.X));
            Canvas.SetTop(item, Math.Min(_topLeft.Y, _rightBottom.Y));
            return item;
        }
    }

}
