using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiff3DViewer.Models
{
    public class MeshTriangle
    {
        public Point3D A { get; set; }
        public Point3D B { get; set; }
        public Point3D C { get; set; }

        public Color Color { get; set; }

        public MeshTriangle(Point3D a, Point3D b, Point3D c, Color color)
        {
            A = a;
            B = b;
            C = c;
            Color = color;
        }
    }
}
