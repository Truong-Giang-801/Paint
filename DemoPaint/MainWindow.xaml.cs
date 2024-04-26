using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
            scrollViewer.AddHandler(ScrollViewer.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(scrollViewer_PreviewMouseLeftButtonDown), true);

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

                _painter.AddFirst(_start);
                _painter.AddSecond(_end);
                myCanvas.Children.Add(_painter.Convert());
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = false;
            _painters.Add((IShape)_painter.Clone());                        
        }

        IShape _painter = null;
        private bool isDragging = false;
        private Point lastMousePosition;

        private void zoomCanvas_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            double zoomFactor = e.Delta > 0 ? 1.1 : 1 / 1.1;

            // Calculate the new scale
            double newScaleX = scaleTransform.ScaleX * zoomFactor;
            double newScaleY = scaleTransform.ScaleY * zoomFactor;

            // Calculate the zoomed width and height of the canvas
            double zoomedWidth = zoomCanvas.ActualWidth * newScaleX;
            double zoomedHeight = zoomCanvas.ActualHeight * newScaleY;

            // Calculate the new scroll offsets
            Point position = e.GetPosition(scrollViewer);
            double offsetX = (position.X + scrollViewer.HorizontalOffset) * (zoomFactor - 1);
            double offsetY = (position.Y + scrollViewer.VerticalOffset) * (zoomFactor - 1);

            // Apply the new scale
            scaleTransform.ScaleX = newScaleX;
            scaleTransform.ScaleY = newScaleY;

            // Update the size of the Canvas to reflect the scaled content
            myCanvas.Width = zoomedWidth;
            myCanvas.Height = zoomedHeight;

            // Adjust the scroll viewer's scroll bars considering the zoom level
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + offsetX);
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + offsetY);

            // Prevent the event from bubbling up to parent controls
            e.Handled = true;
        }

        private void scrollViewer_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point currentPosition = e.GetPosition(scrollViewer);
                double offsetX = currentPosition.X - lastMousePosition.X;
                double offsetY = currentPosition.Y - lastMousePosition.Y;

                // Adjust the scroll viewer's scroll bars
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - offsetX);
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - offsetY);

                lastMousePosition = currentPosition;
            }
        }

        private void scrollViewer_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDragging)
            {
                isDragging = false;
                scrollViewer.ReleaseMouseCapture();
                e.Handled = true;
            }

        }

        private void scrollViewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject obj = e.OriginalSource as DependencyObject;
            while (obj != null && obj != scrollViewer)
            {
                if (obj is ScrollBar)
                {
                    // Handle the click on the ScrollBar
                    // For example, you can set a flag to indicate that the scrollbar is being dragged
                    isDragging = true;
                    lastMousePosition = e.GetPosition(scrollViewer);
                    scrollViewer.CaptureMouse();
                    e.Handled = true; // Mark the event as handled to prevent it from bubbling further
                    return;
                }
                obj = VisualTreeHelper.GetParent(obj);
            }
        }

        private void scrollViewer_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is ScrollBar)
            {
                // Handle the click on the ScrollBar
                isDragging = true;
                lastMousePosition = e.GetPosition(scrollViewer);
                scrollViewer.CaptureMouse();
                e.Handled = true; // Mark the event as handled
            }
        }
    }
}