
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace MyImage
{
    public class MyImage : IShape
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
        public string Name => "Image";

        public object Clone()
        {
            return MemberwiseClone();
        }

        public UIElement Convert()
        {
            return new Line()
            {
                X1 = start.X,
                Y1 = start.Y,
                X2 = end.X,
                Y2 = end.Y,
                StrokeThickness = 1,
                Stroke = new SolidColorBrush(Colors.Red)
            };

        }
    }

}
