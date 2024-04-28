using System.Drawing;
using System.Windows;

namespace EditIShapeDisplay
{
    public class IShapeDisplay
    {
        private List<int>? strokeType;

        public Color Color { get; set; } = Color.Black;
        public int Thickness { get; set; }
        public List<int>? StrokeType;
    }
}
