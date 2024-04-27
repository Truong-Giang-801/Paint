using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using Shapes;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Windows.Data;
using System.Reflection;

namespace MyText
{
    public class MyText : IShape 
    {
        private Point start { get; set; }
        private Point end { get; set; }
        public string? text { get; set; }
        
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
        public string Name => "Text";
        public string? Text
        {
            get { if (text == null)
                    return " ";
                    else
                return text; }
            set
            {
                text = value;
            }
        }
        public object Clone()
        {
            return MemberwiseClone();
        }

        public UIElement Convert()
        {
            // Assuming you have an instance of TextViewModel named viewModel
            var viewModel = new TextViewModel(); // Create a new ViewModel instance

            var newTextBox = new TextBox()
            {
                Width = Math.Abs(end.X - start.X),
                Height = Math.Abs(end.Y - start.Y),
                Text =text,// Initial empty text
            };

            // Bind Text property to ViewModel

            Canvas.SetLeft(newTextBox, Math.Min(start.X, end.X));
            Canvas.SetTop(newTextBox, Math.Min(start.Y, end.Y));

            newTextBox.Dispatcher.BeginInvoke(new Action(() =>
            {
                newTextBox.Focus();
            }), System.Windows.Threading.DispatcherPriority.Background);

            // TextChanged event handler

            return newTextBox;

        }
    }

}

