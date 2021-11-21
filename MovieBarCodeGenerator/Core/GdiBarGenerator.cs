//Copyright 2011-2021 Melvyn Laily
//https://zerowidthjoiner.net

//This file is part of MovieBarCodeGenerator.

//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace MovieBarCodeGenerator.Core;

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
            average ? "Legacy (smoothed)" : "Legacy",
            average ? "_legacy_smoothed" : "_legacy",
            average ? GdiAverage.TwoPasses : GdiAverage.No,
            ScalingMode.Legacy,
            average ? InterpolationMode.NearestNeighbor : InterpolationMode.Default);

    public GdiBarGenerator(
        string displayName = null,
        string fileNameSuffix = null,
        GdiAverage average = GdiAverage.No,
        ScalingMode scalingMode = ScalingMode.Sane,
        InterpolationMode interpolationMode = InterpolationMode.HighQualityBicubic)
    {
        _displayName = displayName;
        FileNameSuffix = fileNameSuffix ?? "";
        Average = average;
        ScalingMode = scalingMode;
        InterpolationMode = interpolationMode;
    }

    public string Name =>
        $"GDI"
        + $"-Average={Average}"
        + $"-ScalingMode={ScalingMode}"
        + $"-InterpolationMode={InterpolationMode}";
    private string _displayName;
    public string DisplayName => _displayName ?? Name;
    public string FileNameSuffix { get; }

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
