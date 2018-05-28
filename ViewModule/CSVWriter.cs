using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreProcessModule
{
    public class CSVWriter
    {
        //int _current = 0;

        bool _hasEnumeratedPoints = false;

        public bool IsRecording { get; protected set; }

        public string Folder { get; protected set; }

        public string Result { get; protected set; }


        public string Translation { get; protected set; }

        private string title = null;

        public void Start(String title)
        {
            IsRecording = true;

            

            if (title.Equals("") || title == null)
                title = "sample";

            

            Folder = title + " Normalized";
            
            this.title = title.Split('\\').ElementAt(title.Split('\\').Count() - 1);


            Directory.CreateDirectory(Folder);
        }

        public String UpdatePoints(CameraSpacePoint[] vertices, List<String> keyPointsNames, String annot, Vector4 rot, CameraSpacePoint head, String timestamp)
        {
            if (!IsRecording) return "null";
            if (vertices == null) return "null";
            if (annot == null)
                annot = "";

            //string path = Path.Combine(Folder, _current.ToString() + ".line");

            // using (StreamWriter writer = new StreamWriter(path))
            // {
            StringBuilder line = new StringBuilder();


            if (!_hasEnumeratedPoints)
            {
                line.Append("Time,Annotation,Vector-4,,,,Head,,,");
                for (int i = 0; i < vertices.Length; i++)
                {
                    line.Append(string.Format("{0},,,", keyPointsNames.ElementAt(i)));
                }
                line.AppendLine();

                line.Append("T,A,W,X,Y,Z,X,Y,Z,");
                for (int i = 0; i < vertices.Length; i++)
                {
                    line.Append("X,Y,Z,");
                }
                line.AppendLine();

                _hasEnumeratedPoints = true;
            }


            line.Append(string.Format("{0},", timestamp));
            line.Append(string.Format("{0},", annot));
            line.Append(string.Format("{0},{1},{2},{3},", rot.W, rot.X, rot.Y, rot.Z));
            line.Append(string.Format("{0},{1},{2},", head.X, head.Y, head.Z));

            for (int i = 0; i < vertices.Length; i++)
            {
                line.Append(string.Format("{0},{1},{2},", vertices[i].X, vertices[i].Y, vertices[i].Z));
            }

            //writer.Write(line);

            //    _current++;
            //}
            return line.ToString();

        }
        //This is edited for non-IO CSV Saving ish @Ralph
        public void Stop(List<String> list)
        {
            IsRecording = false;
            _hasEnumeratedPoints = false;
            if (list.Count == 0 || list == null) return;

            

            Result = this.Folder + "\\"+this.title + "(Normalized).csv";

            using (StreamWriter writer = new StreamWriter(Result))
            {
                foreach (String s in list)
                {
                    //string path = Path.Combine(Folder, index.ToString() + ".line");

                    //if (File.Exists(path))
                    //{
                    string line = string.Empty;

                    //using (StreamReader reader = new StreamReader(path))
                    {
                        line = s;//reader.ReadToEnd();
                    }

                    writer.WriteLine(line);
                    //}
                }
            }
        }
    }
}