using System.Collections.Generic;
using System.Drawing;
using Tiff3DViewer.Models;

namespace Tiff3DViewer.Services
{
    public static class PointCloudBuilder
    {
        public static List<Point3D> FromBitmap(Bitmap bmp, int zIndex, int threshold)
        {
            var points = new List<Point3D>();

            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    Color color = bmp.GetPixel(x, y);

                    // Convert to grayscale intensity
                    int intensity = (color.R + color.G + color.B) / 3;

                    // Skip pixels below threshold
                    if (intensity < threshold)
                        continue;

                    points.Add(new Point3D(x, y, zIndex, color));
                }
            }

            return points;
        }




    }
}
