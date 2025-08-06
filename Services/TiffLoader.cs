using BitMiracle.LibTiff.Classic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace Tiff3DViewer.Services
{
    public static class TiffLoader
    {
        public static List<Bitmap> LoadTiffPages(string filePath)
        {
            var images = new List<Bitmap>();

            using (Tiff image = Tiff.Open(filePath, "r"))
            {
                if (image == null)
                    return images;

                short page = 0;
                do
                {
                    image.SetDirectory(page);

                    int width = image.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
                    int height = image.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
                    int bitsPerSample = image.GetField(TiffTag.BITSPERSAMPLE)[0].ToInt();
                    int samplesPerPixel = image.GetField(TiffTag.SAMPLESPERPIXEL)[0].ToInt();

                    if (bitsPerSample != 8 || samplesPerPixel != 1)
                        throw new NotSupportedException("Only 8-bit grayscale TIFFs with 1 sample per pixel are supported.");

                    Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);

                    byte[] buffer = new byte[image.ScanlineSize()];
                    for (int y = 0; y < height; y++)
                    {
                        image.ReadScanline(buffer, y);
                        for (int x = 0; x < width; x++)
                        {
                            byte gray = buffer[x];
                            Color color = Color.FromArgb(gray, gray, gray);
                            bmp.SetPixel(x, y, color);
                        }
                    }

                    images.Add(bmp);
                    page++;
                }
                while (image.ReadDirectory());
            }

            return images;
        }
    }
}
