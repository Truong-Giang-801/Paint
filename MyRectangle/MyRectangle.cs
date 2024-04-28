
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using Shapes;
using System.Windows.Shapes;

namespace MyRectangle
{
    public class MyRectangle : IShape
    {
        private Point start { get; set; }
        private Point end { get; set; }
        public string? text { get; set; }
        public Color Color { get; set; }
        public List<int>? StrokeType { get; set; }
        public int Thickness { get; set; }
        public string? Text
        {
            get
            {
                if (text == null)
                    return " ";
                else
                    return text;
            }
            set
            {
                text = value;
            }
        }
        public Point Start
        {
            get { return start; }
            set
            {
                start = value;
            }
        }
        public Point End
        {
            get { return end; }
            set
            {
                end = value;
            }
        }
        public string Name => "Rectangle";

        public object Clone()
        {
            return MemberwiseClone();
        }

        public UIElement Convert()
        {
            var item = new Rectangle()
            {   // TODO: end luon luon lon hon start
                Width = Math.Abs(end.X - start.X),
                Height = Math.Abs(end.Y - start.Y),
                StrokeThickness = Thickness,
                Stroke = new SolidColorBrush(Color)
            };
            if (StrokeType != null)
            {
                var dashArray = new DoubleCollection();
                foreach (int value in StrokeType)
                {
                    dashArray.Add(value);
                }
                item.StrokeDashArray = dashArray;
            }
            Canvas.SetLeft(item, Math.Min(start.X, end.X));
            Canvas.SetTop(item, Math.Min(start.Y, end.Y));
            return item;
        }
    }

}
