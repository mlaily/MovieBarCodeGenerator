//Copyright 2011-2018 Melvyn Laily
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

using PhotoSauce.MagicScaler;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MovieBarCodeGenerator.Core
{
    public interface IBarGenerator
    {
        void Initialize(int barWidth, int barHeight);
        Image GetBar(BitmapStream source);
    }

    public class ImageStreamProcessor
    {
        public Bitmap CreateBarCode(
            string inputPath,
            BarCodeParameters parameters,
            FfmpegWrapper ffmpeg,
            IBarGenerator barGenerator,
            CancellationToken cancellationToken,
            IProgress<double> progress = null,
            Action<string> log = null)
        {
            var barCount = (int)Math.Round((double)parameters.Width / parameters.BarWidth);
            var bitmapStreamSource = ffmpeg.GetImagesFromMedia(inputPath, barCount, cancellationToken, log);

            Bitmap finalBitmap = null;
            Graphics finalBitmapGraphics = null;
            int actualHeight = 0;

            int x = 0;
            foreach (var bitmapStream in bitmapStreamSource)
            {
                if (x == 0)
                {
                    var imageInfo = ImageFileInfo.Load(bitmapStream);
                    bitmapStream.Position = 0;

                    actualHeight = parameters.Height ?? imageInfo.Frames.First().Height;

                    finalBitmap = new Bitmap(parameters.Width, actualHeight);
                    finalBitmapGraphics = Graphics.FromImage(finalBitmap);

                    barGenerator.Initialize(parameters.BarWidth, actualHeight);
                }

                using (bitmapStream)
                {

                    var bar = barGenerator.GetBar(bitmapStream);
                    var srcRect = new Rectangle(0, 0, bar.Width, bar.Height);
                    var destRect = new Rectangle(x, 0, parameters.BarWidth, actualHeight);
                    finalBitmapGraphics.DrawImage(bar, destRect, srcRect, GraphicsUnit.Pixel);

                    x += parameters.BarWidth;

                    progress?.Report((double)x / parameters.Width);
                }
            }

            finalBitmapGraphics?.Dispose();

            return finalBitmap;
        }

        public Bitmap GetSmoothedCopy(Bitmap inputImage)
        {
            using (var onePixelHeight = GetResizedImage(inputImage, inputImage.Width, 1))
            {
                var smoothed = GetResizedImage(onePixelHeight, inputImage.Width, inputImage.Height);
                return smoothed;
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
