using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using Shapes;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Windows.Data;

namespace MyText
{
    public class MyText : IShape
    {
        private Point start { get; set; }
        private Point end { get; set; }
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

        public object Clone()
        {
            return MemberwiseClone();
        }

        public UIElement Convert()
        {
            // Assuming you have an instance of TextViewModel named viewModel
            var viewModel = new TextViewModel(); // Create a new ViewModel instance

            var item = new TextBox()
            {
                Width = Math.Abs(end.X - start.X),
                Height = Math.Abs(end.Y - start.Y),
                Text = "", // Initial empty text
            };

            // Bind Text property to ViewModel
            item.SetBinding(TextBox.TextProperty, new Binding("Text") { Source = viewModel });

            Canvas.SetLeft(item, Math.Min(start.X, end.X));
            Canvas.SetTop(item, Math.Min(start.Y, end.Y));

            // Ensure focus is set on the newly created textbox
            item.Dispatcher.BeginInvoke(new Action(() => item.Focus()), System.Windows.Threading.DispatcherPriority.Background);

            return item;
        }
    }

}
