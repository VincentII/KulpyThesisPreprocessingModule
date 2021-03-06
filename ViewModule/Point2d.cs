﻿using System;
using System.Windows;

namespace PreProcessModule
{
    public partial class Point2d
    {
        float x;
        float y;

        public Point2d(float x, float y)
        {
            this.x = x;
            this.y = y;
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
    }
}