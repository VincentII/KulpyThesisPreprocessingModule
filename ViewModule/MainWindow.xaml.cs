using Microsoft.Kinect;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Media3D;
using Microsoft.Kinect.Face;
using LightBuzz.Vitruvius;


namespace PreProcessModule
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

        private String[] _listName;

        private List<double> _columnsCSV;
        private List<List<double>> _rowsCSV;
        private List<String> _timeStamps;
        private List<String> _annotations;

        private float _deltaX;
        private float _deltaY;
        private float _deltaZ;

        private List<double> _rotationW;
        private List<double> _rotationX;
        private List<double> _rotationY;
        private List<double> _rotationZ;

        private Boolean _printDebugs = false;

        private Boolean _isLoad = false;
        private Boolean _isLoadMany = false;

        private Boolean _doNormalize = false;

        private List<String> _outputList;

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

            _deltaX = (float)row[0];
            _deltaY = (float)row[1];
            _deltaZ = (float)row[2];

            //System.Diagnostics.Debug.WriteLine("INDEX: " + rowIndex);
            //System.Diagnostics.Debug.WriteLine("length rot W: " + _rotationW.Count);

            Quaternion rotation = new Quaternion(_rotationX[rowIndex], _rotationY[rowIndex], _rotationZ[rowIndex], _rotationW[rowIndex]);
            rotation.Conjugate();
            
            QuaternionRotation3D rotation3D = new QuaternionRotation3D(rotation);
            Point3D headLocation = new Point3D(0, 0, 0);
            RotateTransform3D rotate3D = new RotateTransform3D(rotation3D, headLocation);

            if (_printDebugs)
            {
                Console.WriteLine("HEADX: " + row[0]);
                Console.WriteLine("HEADY: " + row[1]);
                Console.WriteLine("HEADZ: " + row[2]);
            }

            for (int i = 0; i < row.Count; i += 3)
            {
                CameraSpacePoint vert = new CameraSpacePoint();

                if (_doNormalize)
                {
                    
                    vert.X = (float)row[i] - _deltaX;
                    vert.Y = (float)row[i + 1] - _deltaY;
                    vert.Z = (float)row[i + 2] - _deltaZ;
                    
                    Point3D facePoint = new Point3D(vert.X, vert.Y, vert.Z);

                    facePoint = rotate3D.Transform(facePoint);

                    vert.X = (float)facePoint.X;
                    vert.Y = (float)facePoint.Y;
                    vert.Z = (float)(facePoint.Z + 0.5);
                }
                else
                {
                    vert.X = (float)row[i];
                    vert.Y = (float)row[i + 1];
                    vert.Z = (float)row[i + 2];
                }

                if (_printDebugs || false)
                {
                    Console.WriteLine("VERTX: " + vert.X);
                    Console.WriteLine("VERTY: " + vert.Y);
                    Console.WriteLine("VERTZ: " + vert.Z);
                }

                _vertices.Add(vert);
            }
        }

        private void UpdateFacePoints()
        {

            if (!_isLoad) return;

            int rowIndex = (int)(Slider_frame.Value / 10 *( _rowsCSV.Count()-1));
            FaceDataToVetices(rowIndex);

            if (_annotations[rowIndex] == null)
            {
                Label_Annot.Content = "null";
            }
            else
                Label_Annot.Content = _annotations[rowIndex];

            if (_points.Count == 0)
            {
                for (int index = 0; index < _vertices.Count(); index++)
                {
                    Ellipse ellipse = new Ellipse
                    {
                        Width = 2.0,
                        Height = 2.0,
                        Fill = new SolidColorBrush(Colors.Yellow)
                    };

                    _points.Add(ellipse);

                }

                foreach (Ellipse ellipse in _points)
                {
                    canvas.Children.Add(ellipse);
                }
            }

            if (_printDebugs)
            {
                Console.WriteLine("===================================================");
                Console.WriteLine("|                   A  ROW                        |");
                Console.WriteLine("===================================================");
            }

            if(CheckBoxView.IsChecked == true) { 
                for (int index = 0; index < _vertices.Count(); index++)
                {
                    CameraSpacePoint vertice = _vertices[index];

                    if (_printDebugs)
                    {
                        Console.WriteLine("Vertex X:" + vertice.X);
                        Console.WriteLine("Vertex Y:" + vertice.Y);
                        Console.WriteLine("Vertex Z:" + vertice.Z);
                    }

                    DepthSpacePoint point = _sensor.CoordinateMapper.MapCameraPointToDepthSpace(vertice);
     
                    //CameraSpacePoint point = vertice;

                    if (float.IsInfinity(point.X) || float.IsInfinity(point.Y))
                    {
                        return;
                    }

                    Ellipse ellipse = _points[index];

                    Canvas.SetLeft(ellipse, point.X);
                    Canvas.SetTop(ellipse, point.Y);
                }
            }
        }

        private void Dir_Click(object sender, RoutedEventArgs e)
        {

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV (*.csv)|*.csv|All files (*.*)|*.*";
            openFileDialog.Multiselect = true;
            
            if (openFileDialog.ShowDialog() == true)
               Box_File.Text = (openFileDialog.FileName);

            _listName = openFileDialog.FileNames;

        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {

            if (_listName.Count() == 1) { 
                LoadPoints(_listName[0]);
            
                _isLoad = true;
                _isLoadMany = false;
                Slider_frame.Value = 0;
                UpdateFacePoints();
            }else if (_listName.Count() > 1)
            {
                _isLoad = false ;
                _isLoadMany = true;
            }
        }

        void LoadPoints(String file)
        {
            _rotationW = new List<double>();
            _rotationX = new List<double>();
            _rotationY = new List<double>();
            _rotationZ = new List<double>();
            _timeStamps = new List<String>();
            _annotations = new List<String>();

            using (var reader = new StreamReader(file))
            {
                reader.ReadLine();
                reader.ReadLine();

                _rowsCSV = new List<List<double>>();

                while (!reader.EndOfStream)
                {
                    _columnsCSV = new List<double>();


                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    _timeStamps.Add(values[0]);
                    _annotations.Add(values[1]);
                    _rotationW.Add(float.Parse(values[2]));
                    _rotationX.Add(float.Parse(values[3]));
                    _rotationY.Add(float.Parse(values[4]));
                    _rotationZ.Add(float.Parse(values[5]));

                    for (int i = 6; i < values.Length; i++)
                    {
                        if (values[i] != null && values[i] != "")
                            _columnsCSV.Add(Double.Parse(values[i]));
                    }

                    _rowsCSV.Add(_columnsCSV);
                }
            }
        }

        private void Slider_frame_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateFacePoints();
        }

        private void Normalize_Click(object sender, RoutedEventArgs e)
        {
            for(int i = 0; i < _listName.Count(); i++)
            {
                String fname = _listName[i];
                LoadPoints(fname);
                
                NormalizeWrite(fname);
            }
            
            MessageBox.Show("Normalization Complete: File Saved");
        }

        private void NormalizeWrite(String name)
        {
            CSVWriter csvWriter = new CSVWriter();
            _outputList = new List<string>();

            for (int rowIndex = 0; rowIndex < _rowsCSV.Count(); rowIndex++)
            {
                Normalize(rowIndex, csvWriter, name);
            }

            csvWriter.Stop(_outputList);
        }

        void Normalize(int rowIndex, CSVWriter csvWriter, String name)
        {
            csvWriter.Start(name);

            if (rowIndex < 0 || rowIndex >= _rowsCSV.Count) rowIndex = 0;
            List<double> row = _rowsCSV[rowIndex];

            _vertices = new List<CameraSpacePoint>();

            _deltaX = (float)row[0];
            _deltaY = (float)row[1];
            _deltaZ = (float)row[2];

            Vector4 vector4 = new Vector4();
            vector4.X = (float)_rotationX[rowIndex];
            vector4.Y = (float)_rotationY[rowIndex];
            vector4.Z = (float)_rotationZ[rowIndex];
            vector4.W = (float)_rotationW[rowIndex];

            Quaternion rotation = new Quaternion(_rotationX[rowIndex], _rotationY[rowIndex], _rotationZ[rowIndex], _rotationW[rowIndex]);
            rotation.Conjugate();

            QuaternionRotation3D rotation3D = new QuaternionRotation3D(rotation);
            Point3D headLocation = new Point3D(0, 0, 0);
            RotateTransform3D rotate3D = new RotateTransform3D(rotation3D, headLocation);

            
            for (int i = 3; i < row.Count-3; i += 3)
            {
                CameraSpacePoint vert = new CameraSpacePoint();

                vert.X = (float)row[i] - _deltaX;
                vert.Y = (float)row[i + 1] - _deltaY;
                vert.Z = (float)row[i + 2] - _deltaZ;

                Point3D facePoint = new Point3D(vert.X, vert.Y, vert.Z);

                facePoint = rotate3D.Transform(facePoint);

                vert.X = (float)facePoint.X;
                vert.Y = (float)facePoint.Y;
                vert.Z = (float)(facePoint.Z + 0.5);

                if (_printDebugs || false && rowIndex == 0)
                {
                    Console.WriteLine("VERTX: " + vert.X);
                    Console.WriteLine("VERTY: " + vert.Y);
                    Console.WriteLine("VERTZ: " + vert.Z);
                }

                _vertices.Add(vert);
            }

            CameraSpacePoint head = new CameraSpacePoint();
            head.X = (float)row[0];
            head.Y = (float)row[1];
            head.Z = (float)row[2];

            List<String> keyPointsNames = new List<String>();

            var a = Enum.GetValues(typeof(HighDetailFacePoints));
            foreach (HighDetailFacePoints m in a)
            {
                keyPointsNames.Add(m.ToString());

            }

            _outputList.Add(csvWriter.UpdatePoints(_vertices.ToArray(), keyPointsNames, _annotations[rowIndex], vector4, head, _timeStamps[rowIndex]));
        }

        void rotate_2D(float cx, float cy, float angle, Point2d p)
        {
            float s = (float)Math.Sin(angle);
            float c = (float)Math.Cos(angle);

            // translate point back to origin:
            p.X -= cx;
            p.Y -= cy;

            // rotate point
            float xnew = p.X * c - p.Y * s;
            float ynew = p.X * s + p.Y * c;

            // translate point back:
            p.X = xnew + cx;
            p.Y = ynew + cy;
        }

        Point3d rotate_3D(float headX, float headY, float headZ, float x, float y, float z, float yaw, float pitch, float roll)
        {

                Point2d tempPoint;

                    //yaw = z
                tempPoint = new Point2d(x, y);
                rotate_2D(headX, headY, -roll, tempPoint);
                x = tempPoint.X;
                y = tempPoint.Y;

                //pitch = y
                tempPoint = new Point2d(x, z);
                rotate_2D(headX, headZ, -yaw, tempPoint);
                x = tempPoint.X;
                z = tempPoint.Y;

                //roll = x
                tempPoint = new Point2d(y, z);
                rotate_2D(headY, headZ, -pitch, tempPoint);
                y = tempPoint.X;
                z = tempPoint.Y;


                return new Point3d(x, y, z);
        }

        void EnableNormalize(object sender, RoutedEventArgs e)
        {
            _doNormalize = true;
        }

        void DisableNormalize(object sender, RoutedEventArgs e)
        {
            _doNormalize = false;
        }

       
    }
}