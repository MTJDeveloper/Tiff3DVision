using System.Drawing;

namespace Tiff3DViewer.Models
{
    public class Point3D
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public Color Color { get; set; }

        public Point3D() { }

        public Point3D(float x, float y, float z, Color color)
        {
            X = x;
            Y = y;
            Z = z;
            Color = color;
        }
    }

}
