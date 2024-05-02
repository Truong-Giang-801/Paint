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
using EditIShapeDisplay;
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
        IShapeDisplay DisplayShape = new IShapeDisplay();
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
                    _prePainter.Thickness = 0;
                    _painters.Add((IShape)_prePainter.Clone());
                    myCanvas.Children.RemoveAt(myCanvas.Children.Count -1);
                    myCanvas.Children.Add(_prePainter.Convert());

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
                _painter.Thickness = DisplayShape.Thickness;
                _painter.Color = Color.FromArgb(DisplayShape.Color.A, DisplayShape.Color.R, DisplayShape.Color.G, DisplayShape.Color.B);
                _painter.StrokeType = DisplayShape.StrokeType;

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
                        binaryWriter.Write(shape.Thickness);
                        binaryWriter.Write(shape.Color.ToString());

                        //Debug.WriteLine("Color origin: " + shape.Color.ToString());
                        (int alpha, int red, int green, int blue) = SplitHexColor(shape.Color.ToString());

                        //Debug.WriteLine("Alpha: " + (byte)alpha + "/red: " + red + "/blue: " + blue + "/green: " + green);

                        if (shape.StrokeType != null)
                        {

                            binaryWriter.Write(shape.StrokeType.Count);
                            Debug.WriteLine("Number of element to write: " + shape.StrokeType.Count);

                            int count = shape.StrokeType.Count;
                            for (int j = 0; j < count; j++)
                            {
                                binaryWriter.Write((Double)shape.StrokeType[j]);
                                Debug.WriteLine((Double)shape.StrokeType[j]);
                            }
                        }
                        if (shape.StrokeType == null)
                        {
                            Debug.WriteLine("Number of element to write: 0");

                            binaryWriter.Write(0);
                        }

                    }
                }
                return memoryStream.ToArray();
            }
        }

        public static (int Alpha, int Red, int Green, int Blue) SplitHexColor(string colorString)
        {
            if (!colorString.StartsWith("#") || colorString.Length != 9)
            {
                throw new ArgumentException("Invalid hex color string format.");
            }

            // Remove the # prefix and convert each hex pair to an integer (0-255)
            int alpha = Convert.ToInt32(colorString.Substring(1, 2), 16);
            int red = Convert.ToInt32(colorString.Substring(3, 2), 16);
            int green = Convert.ToInt32(colorString.Substring(5, 2), 16);
            int blue = Convert.ToInt32(colorString.Substring(7, 2), 16);

            return (alpha, red, green, blue);
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

                        int thickNess = binaryReader.ReadInt32();
                        string colorString = binaryReader.ReadString();
                        (int alpha, int red, int green, int blue) = SplitHexColor(colorString);

                        //Debug.WriteLine("Alpha: " + alpha + "/red: " + red + "/blue: " + blue + "/green: " + green);
                        //string temp = binaryReader.ReadString();
                        //string hexString = temp;
                        //if (hexString.StartsWith("#"))
                        //{
                        //    hexString = hexString.Substring(1); // Remove leading "#"
                        //}

                        //byte alpha = 255; // Assuming full opacity (modify if needed)
                        //byte red, green, blue;


                        //red = byte.Parse(hexString.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                        //green = byte.Parse(hexString.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                        //blue = byte.Parse(hexString.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

                        //Debug.WriteLine(temp);



                        Color color = Color.FromArgb((byte)alpha, (byte)red, (byte)green, (byte)blue);
                        int numDoubles = binaryReader.ReadInt32(); // Read the number of doubles written earlier
                        //Debug.WriteLine("Number of element to read: " + numDoubles);

                        List<double> strokeType = new List<double>();
                        if(numDoubles > 0)
                        {
                            for (int j = 0; j < numDoubles; j++)
                            {
                                double value = binaryReader.ReadDouble();
                                //Debug.WriteLine(value.ToString());
                                strokeType.Add(value);
                            }
                        }
                        List<int> intList = new List<int>();
                        if(strokeType.Count > 0)
                        {
                            foreach (double value in strokeType)
                            {
                                // Cast the double value to an integer using type conversion (truncation)
                                int intValue = (int)value;
                                intList.Add(intValue);
                            }
                        }
                        // Iterate through the list of doubles
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
                                shapeInstance.Color = color;
                                shapeInstance.Thickness = thickNess;
                                shapeInstance.StrokeType = intList;
                                //Debug.WriteLine(shapeInstance.Text);
                                // Add the reconstructed shape to your list
                                shapes.Add((IShape)shapeInstance.Clone());
                                Debug.WriteLine("Color: " + shapeInstance.Color + "/ Thickness: " + shapeInstance.Thickness + "/Start: " + shapeInstance.Start);
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

        private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {

            Color selectedColor = e.NewValue ?? Colors.Black; // Default to black if no color is selected

            // Convert the WPF color to a System.Drawing.Color
            System.Drawing.Color drawingColor = System.Drawing.Color.FromArgb(selectedColor.A, selectedColor.R, selectedColor.G, selectedColor.B);
            // Assign the converted color to your property
            DisplayShape.Color = drawingColor;
        }

        private void StrokeThickness_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            DisplayShape.Thickness = comboBox.SelectedIndex + 1;

        }

        private void StrokeType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmb = (ComboBox)sender;
            List<int> DashArray = null;
            
            if ( cmb.SelectedIndex == 1)
            {
                DashArray = [1,2];
            }
            else if ( cmb.SelectedIndex == 2)
            {
                DashArray = [2,2,2,2];

            }
            else if ( cmb.SelectedIndex == 3)
            {
                DashArray = [1, 1, 4];
            }
            else if( cmb.SelectedIndex == 4)
            {
                DashArray = [4, 1, 4];

            }
            DisplayShape.StrokeType = DashArray;

        }
        List<UIElement> UndoRedo = new List<UIElement>();
        List<IShape> UndoRedoShape = new List<IShape>();
        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            if(UndoRedo.Count > 0 && UndoRedoShape.Count > 0)
            {
                myCanvas.Children.Add(UndoRedo[UndoRedo.Count - 1]);
                UndoRedo.RemoveAt(UndoRedo.Count - 1);
                _painters.Add(UndoRedoShape[UndoRedoShape.Count - 1]);
                UndoRedoShape.RemoveAt(UndoRedoShape.Count - 1);
            }
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            if(myCanvas.Children.Count > 0 && _painters.Count > 0)
            {
                UndoRedo.Add( myCanvas.Children[myCanvas.Children.Count - 1]);
                myCanvas.Children.RemoveAt(myCanvas.Children.Count -1);
                UndoRedoShape.Add(_painters[_painters.Count - 1]);
                _painters.RemoveAt(_painters.Count - 1);
            }

        }
    }
}