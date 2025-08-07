using MIConvexHull;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Tiff3DViewer.Models;

namespace Tiff3DViewer.Services
{
    public class MeshVertex : IVertex
    {
        public double[] Position { get; set; }
        public Color Color { get; set; }

        public MeshVertex(Point3D point)
        {
            Position = new double[] { point.X, point.Y, point.Z };
            Color = point.Color;
        }
    }

    public class MeshFace : ConvexFace<MeshVertex, MeshFace> { }

}
