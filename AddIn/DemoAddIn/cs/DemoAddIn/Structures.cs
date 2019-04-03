﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace DemoAddIn
{
    struct BoundingBoxInfo
    {
        public float LineWidth;
        public Color LineColor;
        public bool Visible;
    }

    struct Vector3d
    {
        public double X;
        public double Y;
        public double Z;
    }

    struct HoleInfo
    {
        public double diameter;
        public double nx;
        public double ny;
        public double nz;
        public double x;
        public double y;
        public double z;
    }
}
