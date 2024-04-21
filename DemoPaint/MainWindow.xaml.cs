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
    public partial class MainWindow : Window
    {
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
       
    }
}