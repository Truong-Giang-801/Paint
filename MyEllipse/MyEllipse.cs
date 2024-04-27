
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using Shapes;
using System.Security.Cryptography;

namespace MyEllipse
{
    public class MyEllipse : IShape
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
        public string Name => "Ellipse";

        public object Clone()
        {
            return MemberwiseClone();
        }

        public UIElement Convert()
        {
            var item = new Ellipse()
            {
                Width = Math.Abs(end.X - start.X),
                Height = Math.Abs(end.Y - start.Y),
                StrokeThickness = 1,
                Stroke = new SolidColorBrush(Colors.Blue)
            };
            Canvas.SetLeft(item, Math.Min(start.X, end.X));
            Canvas.SetTop(item, Math.Min(start.Y, end.Y));
            return item;
        }
    }

}
