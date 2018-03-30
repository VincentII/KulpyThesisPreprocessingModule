using Microsoft.Kinect;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace ViewModule
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Used to display 1,000 points on screen.
        private List<Ellipse> _points = new List<Ellipse>();
        private List<CameraSpacePoint> _vertices = new List<CameraSpacePoint>();
        private KinectSensor _sensor = KinectSensor.GetDefault();

        private List<double> _columnsCSV;
        private List<List<double>> _rowsCSV;

        private Boolean _isLoad = false;

        public MainWindow()
        {
            this.InitializeComponent();
            Initialize_Sensor();
           

            Thread.Sleep(5000);
            //UpdateFacePoints();
        }

        private void Initialize_Sensor()
        {
            _sensor = KinectSensor.GetDefault();

            if (_sensor != null)
            {
                Console.WriteLine("Sensor Ready");

                // Start tracking!        
                _sensor.Open();
            }
        }

        private void FaceDataToVetices(int rowIndex)
        {

            if (rowIndex < 0 || rowIndex >= _rowsCSV.Count) rowIndex = 0;
            List<double> row = _rowsCSV[rowIndex];

            _vertices = new List<CameraSpacePoint>();

            for(int i = 0; i < row.Count; i += 3)
            {
                CameraSpacePoint vert = new CameraSpacePoint();
                vert.X = (float)row[i];
                vert.Y = (float)row[i+1];
                vert.Z = (float)row[i + 2];

                _vertices.Add(vert);
            }
        }

        private void UpdateFacePoints()
        {

            if (!_isLoad) return;
            //TODO Read Vertices here, pass to _vertices

            //Console.WriteLine("11111111111111111111111111111111111111111");
            FaceDataToVetices((int)(Slider_frame.Value/10*_rowsCSV.Count()));

            if (_points.Count == 0)
            {

                //Console.WriteLine("222222222222222222222222222");
                for (int index = 0; index < _vertices.Count(); index++)
                {
                    Ellipse ellipse = new Ellipse
                    {
                        Width = 2.0,
                        Height = 2.0,
                        Fill = new SolidColorBrush(Colors.Yellow)
                    };

                    _points.Add(ellipse);


                    //Console.WriteLine("333333333333333333333333333333333");

                }

                foreach (Ellipse ellipse in _points)
                {

                    //Console.WriteLine("444444444444444444444444444");
                    canvas.Children.Add(ellipse);
                }
            }
            /*
                            if (_pointTextBlocks.Count == 0)
                            {
                                for (int index = 0; index < vertices.Count(); index++)
                                {
                                    TextBlock textBlock = new TextBlock
                                    {
                                        Text = _keyPoints.ElementAt(index).ToString(),

                                        Foreground = new SolidColorBrush(Colors.White),
                                        TextAlignment = TextAlignment.Left,
                                        FontSize = 5
                                    };


                                    _pointTextBlocks.Add(textBlock);
                                }

                                foreach (TextBlock textBlock in _pointTextBlocks)
                                {
                                    canvas.Children.Add(textBlock);
                                }
                            }

            */
            for (int index = 0; index < _vertices.Count(); index++)
            {


                //Console.WriteLine("5555555555555555555555");
                CameraSpacePoint vertice = _vertices[index];
                //Console.WriteLine("VEEEEEEEEERT" +vertice.X);
                DepthSpacePoint point = _sensor.CoordinateMapper.MapCameraPointToDepthSpace(vertice);

                

                //Console.WriteLine("YESSSSSSSSS"+point.X +" "+point.Y);
                if (float.IsInfinity(point.X) || float.IsInfinity(point.Y)) return;

                Ellipse ellipse = _points[index];

                ////Console.WriteLine("77777777777777777777");

                //TextBlock textBlock = _pointTextBlocks[index];

                Canvas.SetLeft(ellipse, point.X);
                Canvas.SetTop(ellipse, point.Y);

                //Canvas.SetLeft(textBlock, point.X + 5);
                //Canvas.SetTop(textBlock, point.Y);

            }


            



        }

        private void Dir_Click(object sender, RoutedEventArgs e)
        {

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV (*.csv)|*.csv|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
               Box_File.Text = (openFileDialog.FileName);
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            using (var reader = new StreamReader(Box_File.Text))
            {
                reader.ReadLine();
                reader.ReadLine();

                _rowsCSV = new List<List<double>>();

                while (!reader.EndOfStream)
                {

                    _columnsCSV = new List<double>();
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    for(int i = 6; i < values.Length; i++)
                    {
                        if(values[i]!=null && values[i] != "")
                        _columnsCSV.Add(Double.Parse(values[i]));
                    }

                    _rowsCSV.Add(_columnsCSV);
                }
            }

            _isLoad = true;
            Slider_frame.Value = 0;
            UpdateFacePoints();
        }

        private void Slider_frame_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateFacePoints();
        }
    }
}