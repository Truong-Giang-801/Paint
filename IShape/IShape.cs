
using System.Windows;

namespace Shapes
{
    public interface IShape : ICloneable
    {
        Point Start { get; set; }
        Point End { get; set; }
        UIElement Convert();
        string Name { get; }
        public string? Text { get; set; }
    }

}
