using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using Tiff3DViewer.Models;

namespace Tiff3DViewer.Services
{
    public static class PointCloudBuilder
    {
      


        // Simple compact point struct
        public struct PPoint
        {
            public float X, Y, Z;    // positions
            public byte R, G, B;     // color
            public PPoint(float x, float y, float z, byte r, byte g, byte b) { X = x; Y = y; Z = z; R = r; G = g; B = b; }
        }

        // FromBitmap: create point list for one page (zIndex)
        // stride: sample every N pixels on x,y to downsample
        public enum FilterMode
        {
            None,
            LowPass,
            HighPass,
            BandPass
        }

        public static List<PPoint> FromBitmap(Bitmap bmp, int zIndex, int lowCut, int highCut, FilterMode mode, int stride = 1)
        {
            var result = new List<PPoint>();
            int w = bmp.Width, h = bmp.Height;
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            int strideB = bd.Stride;
            IntPtr scan0 = bd.Scan0;

            unsafe
            {
                byte* p = (byte*)scan0.ToPointer();

                for (int y = 0; y < h; y += stride)
                {
                    byte* row = p + y * strideB;
                    for (int x = 0; x < w; x += stride)
                    {
                        int idx = x * 3;
                        byte b = row[idx];
                        byte g = row[idx + 1];
                        byte r = row[idx + 2];

                        // grayscale luma
                        float lum = 0.299f * r + 0.587f * g + 0.114f * b;

                        bool keep = false;
                        switch (mode)
                        {
                            case FilterMode.LowPass:
                                keep = lum <= highCut;
                                break;
                            case FilterMode.HighPass:
                                keep = lum >= lowCut;
                                break;
                            case FilterMode.BandPass:
                                keep = lum >= lowCut && lum <= highCut;
                                break;
                            case FilterMode.None:
                                keep = true;
                                break;
                        }

                        if (!keep) continue;

                        float px = x;
                        float py = h - y;
                        float pz = zIndex;

                        result.Add(new PPoint(px, py, pz, r, g, b));
                    }
                }
            }

            bmp.UnlockBits(bd);
            return result;
        }




    }
}
