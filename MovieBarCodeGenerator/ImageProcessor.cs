using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieBarCodeGenerator
{
    public class BarCodeParameters
    {
        public int Width { get; set; } = 1000;
        public int? Height { get; set; } = null;
        public int BarWidth { get; set; } = 1;
    }

    public class ImageProcessor
    {
        public Bitmap CreateBarCode(string inputPath, BarCodeParameters parameters, FfmpegWrapper ffmpeg)
        {
            Bitmap finalBitmap = null;
            Graphics finalBitmapGraphics = null;

            Graphics GetDrawingSurface(int width, int height)
            {
                if (finalBitmap == null)
                {
                    finalBitmap = new Bitmap(width, height);
                    finalBitmapGraphics = Graphics.FromImage(finalBitmap);
                }
                return finalBitmapGraphics;
            }

            var barCount = (int)Math.Ceiling((double)parameters.Width / parameters.BarWidth);
            var source = ffmpeg.GetImagesFromMedia(inputPath, barCount);

            int? finalBitmapHeight = null;

            int x = 0;
            foreach (var image in source)
            {
                if (finalBitmapHeight == null)
                {
                    finalBitmapHeight = parameters.Height ?? image.Height;
                }

                var surface = GetDrawingSurface(parameters.Width, finalBitmapHeight.Value);
                surface.DrawImage(image, x, 0, parameters.BarWidth, finalBitmapHeight.Value);

                x += parameters.BarWidth;

                image.Dispose();
            }

            finalBitmapGraphics?.Dispose();

            return finalBitmap;
        }
    }
}
