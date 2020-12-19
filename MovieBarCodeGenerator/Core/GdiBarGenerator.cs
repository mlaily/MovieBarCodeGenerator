using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieBarCodeGenerator.Core
{
    public class GdiBarGenerator : IBarGenerator
    {
        public bool Smoothed { get; }

        public GdiBarGenerator(bool smoothed)
        {
            Smoothed = smoothed;
        }

        public Image GetBar(BitmapStream source, int barWidth, int barHeight)
        {
            var sourceImage = Image.FromStream(source, true, false);

            if (Smoothed)
            {
                var bar = new Bitmap(barWidth, barHeight);
                using (var g = Graphics.FromImage(bar))
                {
                    var srcRect = new Rectangle(0, 0, sourceImage.Width, sourceImage.Height);
                    var destRect = new Rectangle(0, 0, bar.Width, bar.Height);
                    g.DrawImage(sourceImage, destRect, srcRect, GraphicsUnit.Pixel);
                }

                using (bar)
                using (var onePixelHeight = GetResizedImage(bar, barWidth, 1))
                {
                    var smoothed = GetResizedImage(onePixelHeight, barWidth, barHeight);
                    return smoothed;
                }
            }
            else
            {
                // TODO
                return sourceImage;
            }
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
