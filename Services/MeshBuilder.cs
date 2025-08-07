using MIConvexHull;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Tiff3DViewer.Models;

namespace Tiff3DViewer.Services
{
    public class MeshBuilder
    {
        public static List<MeshTriangle> FromPointCloud(List<Point3D> cloud)
        {
            var vertices = cloud.Select(p => new MeshVertex(p)).ToList();

            var result = ConvexHull.Create<MeshVertex, MeshFace>(vertices);
            var faces = result.Result.Faces; 

            var triangles = new List<MeshTriangle>();

            foreach (var face in faces)
            {
                if (face.Vertices.Length >= 3)
                {
                    var a = face.Vertices[0];
                    var b = face.Vertices[1];
                    var c = face.Vertices[2];

                    var avgColor = AverageColor(new[] { a.Color, b.Color, c.Color });

                    triangles.Add(new MeshTriangle(
                        new Point3D((float)a.Position[0], (float)a.Position[1], (float)a.Position[2], a.Color),
                        new Point3D((float)b.Position[0], (float)b.Position[1], (float)b.Position[2], b.Color),
                        new Point3D((float)c.Position[0], (float)c.Position[1], (float)c.Position[2], c.Color),
                        avgColor
                    ));
                }
            }

            return triangles;
        }

        private static Color AverageColor(IEnumerable<Color> colors)
        {
            var list = colors.ToList();
            int r = (int)list.Average(c => c.R);
            int g = (int)list.Average(c => c.G);
            int b = (int)list.Average(c => c.B);
            return Color.FromArgb(r, g, b);
        
    }
}
}
