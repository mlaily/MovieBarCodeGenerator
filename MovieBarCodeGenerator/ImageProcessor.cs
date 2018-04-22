using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MovieBarCodeGenerator
{
    public class BarCodeParameters
    {
        public int Width { get; set; } = 1000;
        public int? Height { get; set; } = null;
        public int BarWidth { get; set; } = 1;
        public bool Smoothen { get; set; } = false;
    }

    public class ImageProcessor
    {
        public Bitmap CreateBarCode(
            string inputPath,
            BarCodeParameters parameters,
            FfmpegWrapper ffmpeg,
            CancellationToken cancellationToken,
            IProgress<double> progress = null)
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

            var barCount = (int)Math.Round((double)parameters.Width / parameters.BarWidth);
            var source = ffmpeg.GetImagesFromMedia(inputPath, barCount, cancellationToken);

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

                progress?.Report((double)x / parameters.Width);

                image.Dispose();
            }

            finalBitmapGraphics?.Dispose();

            if (parameters.Smoothen && finalBitmap != null)
            {
                var onePixelHeight = GetResizedImage(finalBitmap, finalBitmap.Width, 1);
                var smoothened = GetResizedImage(onePixelHeight, finalBitmap.Width, finalBitmap.Height);

                onePixelHeight.Dispose();
                finalBitmap.Dispose();

                finalBitmap = smoothened;
            }

            return finalBitmap;
        }

        // https://stackoverflow.com/a/24199315/755986
        public static Bitmap GetResizedImage(Image source, int newWidth, int newHeight)
        {
            var destRect = new Rectangle(0, 0, newWidth, newHeight);
            var destImage = new Bitmap(newWidth, newHeight);

            destImage.SetResolution(source.HorizontalResolution, source.VerticalResolution);

            using (var g = Graphics.FromImage(destImage))
            {
                g.CompositingMode = CompositingMode.SourceCopy;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode = InterpolationMode.NearestNeighbor; // best results for barcodes
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    g.DrawImage(source, destRect, 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
    }
}
