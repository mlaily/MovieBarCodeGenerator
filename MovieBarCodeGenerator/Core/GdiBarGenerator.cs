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
    public enum GdiAverage
    {
        No,
        /// <summary>
        /// Old method. Scale the width, then the height.
        /// </summary>
        TwoPasses,
        /// <summary>
        /// New method. Scale both the width and height at once.
        /// Probably quicker, and more faithul than the two pass.
        /// </summary>
        OnePass,
    }

    public enum ScalingMode
    {
        /// <summary>
        /// Use sane settings for scaling with GDI.
        /// </summary>
        Sane,
        /// <summary>
        /// Default GDI settings.
        /// </summary>
        Legacy,
    }
    public class GdiBarGenerator : IBarGenerator
    {
        public GdiAverage Average { get; }
        public InterpolationMode InterpolationMode { get; }
        public ScalingMode ScalingMode { get; }

        public static GdiBarGenerator CreateLegacy(bool average)
            => new GdiBarGenerator(
                average ? GdiAverage.TwoPasses : GdiAverage.No,
                ScalingMode.Legacy,
                average ? InterpolationMode.NearestNeighbor : InterpolationMode.Default);

        public GdiBarGenerator(
            GdiAverage average = GdiAverage.No,
            ScalingMode scalingMode = ScalingMode.Sane,
            InterpolationMode interpolationMode = InterpolationMode.HighQualityBicubic)
        {
            Average = average;
            ScalingMode = scalingMode;
            InterpolationMode = interpolationMode;
        }

        public string Name =>
            $"GDI"
            + $"-Average={Average}"
            + $"-ScalingMode={ScalingMode}"
            + $"-InterpolationMode={InterpolationMode}";

        public Image GetBar(BitmapStream source, int barWidth, int barHeight)
        {
            var sourceImage = Image.FromStream(source, true, false);
            var useSaneDefaults = ScalingMode == ScalingMode.Sane;

            if (Average == GdiAverage.TwoPasses) // Scale the width first, then the height
            {
                using (sourceImage)
                using (var widthResized = GetResizedImage(sourceImage, barWidth, barHeight, useSaneDefaults))
                using (var heightResized = GetResizedImage(widthResized, barWidth, 1))
                    return GetResizedImage(heightResized, barWidth, barHeight);
            }
            else if (Average == GdiAverage.OnePass) // Scale everything at the same time
            {
                using (sourceImage)
                using (var bothResized = GetResizedImage(sourceImage, barWidth, 1, useSaneDefaults))
                    return GetResizedImage(bothResized, barWidth, barHeight);
            }
            else
            {
                using (sourceImage)
                    return GetResizedImage(sourceImage, barWidth, barHeight, useSaneDefaults);
            }
        }

        // https://stackoverflow.com/a/24199315/755986
        // https://photosauce.net/blog/post/image-scaling-with-gdi-part-3-drawimage-and-the-settings-that-affect-it
        public Bitmap GetResizedImage(Image source, int newWidth, int newHeight, bool useSaneDefaults = true)
        {
            var destImage = new Bitmap(newWidth, newHeight);
            var destRect = new Rectangle(0, 0, newWidth, newHeight);

            destImage.SetResolution(source.HorizontalResolution, source.VerticalResolution);

            using (var g = Graphics.FromImage(destImage))
            {
                using (var attributes = new ImageAttributes())
                {
                    if (useSaneDefaults)
                    {
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.InterpolationMode = InterpolationMode;
                        g.CompositingMode = CompositingMode.SourceCopy;
                        g.CompositingQuality = CompositingQuality.HighSpeed; // Has no effect for our usage (no transparence)
                        g.SmoothingMode = SmoothingMode.None; // Useless too since we are not dealing with vector graphics

                        attributes.SetWrapMode(WrapMode.TileFlipXY);
                    }
                    g.DrawImage(source, destRect, 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, attributes);
                }
            }

            return destImage;
        }
    }
}
