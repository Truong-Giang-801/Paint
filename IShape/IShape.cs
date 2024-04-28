
using System.Windows;
using System.Windows.Media;

namespace Shapes
{
    public interface IShape : ICloneable
    {
        Point Start { get; set; }
        Point End { get; set; }
        UIElement Convert();
        string Name { get; }
        public string? Text { get; set; }

        Color Color { get; set; }
        List<int>? StrokeType { get; set; }
        int Thickness { get; set; }
    }

}
