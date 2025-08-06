using System.Collections.Generic;
using System.Drawing;
using Tiff3DViewer.Models;

namespace Tiff3DViewer.Services
{
    public static class PointCloudBuilder
    {
        public static List<Point3D> FromBitmap(Bitmap bitmap)
        {
            var points = new List<Point3D>();
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color pixel = bitmap.GetPixel(x, y);


                    float z = pixel.GetBrightness() * 255f;

                    var point = new Point3D(x, y, z, pixel);
                    points.Add(point);
                }
            }

            return points;
        }
    }
}
