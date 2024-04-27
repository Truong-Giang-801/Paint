
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
                StrokeThickness = 1,
                Stroke = new SolidColorBrush(Colors.Green)
            };
            Canvas.SetLeft(item, Math.Min(start.X, end.X));
            Canvas.SetTop(item, Math.Min(start.Y, end.Y));
            return item;
        }
    }

}
