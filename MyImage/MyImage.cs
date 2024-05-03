
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using Shapes;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

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
            // Check if 'text' is null or empty
            if (string.IsNullOrEmpty(text))
            {
                // Return null or a placeholder indicating no image is available
                return null;
            }

            // Assuming 'text' holds the URI of the image file
            var imageUri = new Uri(text, UriKind.RelativeOrAbsolute);

            var newImage = new System.Windows.Controls.Image()
            {
                Source = new BitmapImage(imageUri),
                Stretch = Stretch.None // Display the image at its original size
            };

            Canvas.SetLeft(newImage, start.X);
            Canvas.SetTop(newImage, start.Y);

            // No need to focus on an Image control as it's not interactive

            return newImage;
        }


    }

}
