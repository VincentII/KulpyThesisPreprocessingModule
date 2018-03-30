using System;
using System.Windows;

namespace PreProcessModule
{ 
    public partial class Point3d 
    {
        float x;
        float y;
        float z;

        public Point3d(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public float X
        {
            get
            {
                return x;
            }

            set
            {
                x = value;
            }
        }

        public float Y
        {
            get
            {
                return y;
            }

            set
            {
                y = value;
            }
        }

        public float Z
        {
            get
            {
                return z;
            }

            set
            {
                z = value;
            }
        }
    }
}