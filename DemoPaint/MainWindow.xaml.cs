using System.Diagnostics;
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
using MyText;
using Microsoft.Win32;

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

            foreach (var fi in fis)
            {
                // Lấy tất cả kiểu dữ liệu trong dll
                var assembly = Assembly.LoadFrom(fi.FullName);
                var types = assembly.GetTypes();

                foreach (var type in types)
                {
                    if ((type.IsClass)
                        && (typeof(IShape).IsAssignableFrom(type)))
                    {
                        _prototypes.Add((IShape)Activator.CreateInstance(type)!);
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
            
                    
            count = 0;
            _painter = _prototypes[0];
        }
        private void Control_Click(object sender, RoutedEventArgs e)
        {
            IShape item = (IShape)(sender as Button)!.Tag;
            _painter = item;
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            _isDrawing = true;
            _start = e.GetPosition(myCanvas);

            if (_prePainter != null)
            {

                if (_prePainter.Convert() is TextBox && myCanvas.Children[myCanvas.Children.Count - 1] is TextBox textBox1)
                {
                    //Debug.WriteLine(textBox1.Text);
                    _prePainter.Text = textBox1.Text;
                    _painters.Add((IShape)_prePainter.Clone());
                }
            }
        }
        int count;
        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrawing)
            {
                _end = e.GetPosition(myCanvas);
                // **Handle potential null reference:**
                if (_lastElement != null && count < myCanvas.Children.Count)
                {
                    myCanvas.Children.Remove(_lastElement);
                }

                _painter.Start = _start;
                _painter.End = _end;
                UIElement newElement = _painter.Convert();
                myCanvas.Children.Add(newElement);
                _lastElement = newElement;

            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            count++;
            if (_isDrawing)
            {
                if (_painter.Convert() is not TextBox)
                {
                    _painters.Add((IShape)_painter.Clone());

                }
                else
                {
                    _prePainter = (IShape)_painter.Clone();

                }
            }
            _isDrawing = false;
            var viewModel = new TextViewModel();
            //if (myCanvas.Children[myCanvas.Children.Count] is TextBox textBox)
            //{
            //    _painter.Text = textBox.Text;
            //    Debug.WriteLine(_painter.Text);
            //}
        }


        IShape _painter = null;
        IShape _prePainter = null;

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
        public byte[] SerializeShapes(List<IShape> shapes)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(memoryStream))
                {
                    binaryWriter.Write(shapes.Count); // Write the number of shapes
                    foreach (var shape in shapes)
                    {
                        // Serialize the shape's name
                        var shapeNameBytes = Encoding.UTF8.GetBytes(shape.Name);
                        binaryWriter.Write(shapeNameBytes.Length); // Write the length of the serialized shape name
                        binaryWriter.Write(shapeNameBytes); // Write the serialized shape name

                        // Serialize the Start point
                        binaryWriter.Write(shape.Start.X); // Write the X coordinate of the Start point
                        binaryWriter.Write(shape.Start.Y); // Write the Y coordinate of the Start point

                        // Serialize the End point
                        binaryWriter.Write(shape.End.X); // Write the X coordinate of the End point
                        binaryWriter.Write(shape.End.Y); // Write the Y coordinate of the End point
                        binaryWriter.Write(shape.Text.ToString());
                    }
                }
                return memoryStream.ToArray();
            }
        }

        public List<IShape> DeserializeShapes(byte[] data)
        {
            var shapes = new List<IShape>();
            using (var memoryStream = new MemoryStream(data))
            {
                using (var binaryReader = new BinaryReader(memoryStream))
                {
                    int shapeCount = binaryReader.ReadInt32(); // Read the number of shapes
                    for (int i = 0; i < shapeCount; i++)
                    {
                        // Deserialize the shape's name
                        int shapeNameLength = binaryReader.ReadInt32();
                        byte[] shapeNameBytes = binaryReader.ReadBytes(shapeNameLength);
                        string shapeName = Encoding.UTF8.GetString(shapeNameBytes);

                        // Deserialize the Start point
                        double startX = binaryReader.ReadDouble();
                        double startY = binaryReader.ReadDouble();
                        Point startPoint = new Point(startX, startY);

                        // Deserialize the End point
                        double endX = binaryReader.ReadDouble();
                        double endY = binaryReader.ReadDouble();
                        Point endPoint = new Point(endX, endY);

                        string text = binaryReader.ReadString();
                        //Debug.WriteLine(text);

                        // Assuming you have a way to create a shape instance based on its name
                        // and set its Start and End points
                        foreach (var item in _prototypes)
                        {
                            if (item.Name == shapeName)
                            {
                                IShape shapeInstance = item as IShape;
                                shapeInstance.Start = startPoint;
                                shapeInstance.End = endPoint;
                                shapeInstance.Text = text;
                                //Debug.WriteLine(shapeInstance.Text);
                                // Add the reconstructed shape to your list
                                shapes.Add((IShape)shapeInstance.Clone());
                            }
                        }

                    }
                }
            }
            return shapes;
        }

        private void zoomCanvas_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Bin file|*.bin";
            saveFileDialog.DefaultExt = ".bin";
            saveFileDialog.AddExtension = true;
            if (saveFileDialog.ShowDialog() == true)
            {
                // Ensure the file name ends with .bin
                string fileName = saveFileDialog.FileName;
                if (!fileName.EndsWith(".bin", StringComparison.OrdinalIgnoreCase))
                {
                    fileName += ".bin";
                }

                Save(fileName, _painters);
            }
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            myCanvas.Children.Clear();
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Bin file|*.bin";
            if (openFileDialog.ShowDialog() == true)
            {
                _painters = Load(openFileDialog.FileName);
                count = _painters.Count;
            }
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            myCanvas.Children.Clear();
            _painters.Clear();
            count = 0;
        }

        private List<IShape> Load(string filePath)
        {
            // Load
            // If the user wants to load the file, read its contents and deserialize the shapes
            byte[] loadedData = File.ReadAllBytes(filePath);
            List<IShape> loadedShapes = DeserializeShapes(loadedData);
            foreach (var item in loadedShapes)
            {
                UIElement element = item.Convert();

                if (element is TextBox textBox)
                {
                    textBox.ReleaseMouseCapture();
                }
                myCanvas.Children.Add(item.Convert());
            }
            return loadedShapes;
        }
        private void Save(string filePath, List<IShape> shapes)
        {
            byte[] serializedShapes = SerializeShapes(shapes);
            File.Delete(filePath);
            File.WriteAllBytes(filePath, serializedShapes);
        }

        private void Save_Image_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Image Files (*.bmp;*.png;*.jpg;*.jpeg)|*.bmp;*.png;*.jpg;*.jpeg|All Files (*.*)|*.*";
            saveFileDialog.DefaultExt = ".png";
            saveFileDialog.AddExtension = true;
            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                // Render the canvas to a bitmap
                RenderTargetBitmap renderTarget = new RenderTargetBitmap(
                    (int)myCanvas.ActualWidth,
                    (int)myCanvas.ActualHeight,
                    96, 96, PixelFormats.Pbgra32);
                renderTarget.Render(myCanvas);

                // Save the bitmap to a file
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderTarget));
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    encoder.Save(fileStream);
                }
            }
        }

        private void Load_Image_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files (*.bmp;*.png;*.jpg;*.jpeg)|*.bmp;*.png;*.jpg;*.jpeg|All Files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                // Load the image from a file
                BitmapImage bitmapImage = new BitmapImage(new Uri(filePath));

                // Create an Image control and set its source to the loaded image
                Image imageControl = new Image
                {
                    Source = bitmapImage,
                    Width = bitmapImage.Width,
                    Height = bitmapImage.Height
                
                };

                count = 1;
                myCanvas.Children.Clear();
                // Add the Image control to the canvas
                myCanvas.Children.Add(imageControl);
            }
        }
    }
}