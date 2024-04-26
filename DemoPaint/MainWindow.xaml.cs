using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Shapes;

namespace DemoPaint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private double zoomFactor = 1.1; // Zoom factor for each mouse wheel delta
        private double currentZoom = 1.0; // Current zoom level

        public MainWindow()
        {
            InitializeComponent();
        }

        bool _isDrawing = false;
        Point _start;
        Point _end;

        List<UIElement> _list = new List<UIElement>();
        List<IShape> _painters = new List<IShape>();
        UIElement _lastElement;
        List<IShape> _prototypes = new List<IShape>();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Single configuration
            string folder = AppDomain.CurrentDomain.BaseDirectory;
            var fis = new DirectoryInfo(folder).GetFiles("*.dll");

            foreach(var fi in fis)
            {
                // Lấy tất cả kiểu dữ liệu trong dll
                var assembly = Assembly.LoadFrom(fi.FullName);
                var types = assembly.GetTypes();

                foreach(var type in types)
                {
                    if ((type.IsClass) 
                        && (typeof(IShape).IsAssignableFrom(type))) {
                        _prototypes.Add((IShape) Activator.CreateInstance(type)!);
                    }
                }
            }
            // ---------------------------------------------------

            // Tự tạo ra giao diện
            foreach (var item in _prototypes)
            {
                var control = new Button()
                {
                    Width = 80,
                    Height = 35,
                    Content = item.Name, 
                    Tag = item,
                };
                control.Click += Control_Click;
                actions.Children.Add(control);
            }
            _painter = _prototypes[0];
        }
        private void Control_Click(object sender, RoutedEventArgs e)  {
            IShape item = (IShape)(sender as Button)!.Tag;
            _painter = item; 
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = true;
            _start = e.GetPosition(myCanvas);
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrawing)
            {
                _end = e.GetPosition(myCanvas);
                myCanvas.Children.Clear(); 
                foreach(var item in _painters)
                {
                    myCanvas.Children.Add(item.Convert());
                }

                _painter.Start = _start;
                _painter.End = _end;
                myCanvas.Children.Add(_painter.Convert());
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = false;
            _painters.Add((IShape)_painter.Clone());                        
        }

        IShape _painter = null;

        private void Canvas_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && e.Delta != 0)
            {
                double zoomChange = e.Delta > 0 ? zoomFactor : 1 / zoomFactor; // Zoom in or out based on mouse wheel direction

                // Apply scale transform to each child element of the canvas
                foreach (UIElement element in myCanvas.Children)
                {
                    if (element is FrameworkElement frameworkElement)
                    {
                        // Retrieve the current scale transform or create a new one if not present
                        ScaleTransform currentTransform = frameworkElement.LayoutTransform as ScaleTransform ?? new ScaleTransform(1, 1);

                        // Apply the new zoom factor to the existing scale transform
                        currentTransform.ScaleX *= zoomChange;
                        currentTransform.ScaleY *= zoomChange;

                        // Apply the updated scale transform to the element
                        frameworkElement.LayoutTransform = currentTransform;

                        // Adjust the position of the element relative to the zoom level
                        double newX = Canvas.GetLeft(frameworkElement) * zoomChange;
                        double newY = Canvas.GetTop(frameworkElement) * zoomChange;

                        Canvas.SetLeft(frameworkElement, newX);
                        Canvas.SetTop(frameworkElement, newY);
                    }
                }

                // Consume the event to prevent it from being handled by other event handlers
                e.Handled = true;
            }
        }
    }
}