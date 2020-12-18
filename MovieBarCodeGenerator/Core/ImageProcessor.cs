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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MovieBarCodeGenerator.Core
{
    public class ImageProcessor
    {
        public Bitmap CreateBarCode(
            string inputPath,
            BarCodeParameters parameters,
            FfmpegWrapper ffmpeg,
            CancellationToken cancellationToken,
            IProgress<double> progress = null,
            Action<string> log = null)
        {
            var barCount = (int)Math.Round((double)parameters.Width / parameters.BarWidth);
            var source = ffmpeg.GetImagesFromMedia(inputPath, barCount, cancellationToken, log);

            int? finalBitmapHeight = null;
            Bitmap finalBitmap = null;
            Graphics finalBitmapGraphics = null;

            int x = 0;
            foreach (var image in source)
            {
                if (x == 0)
                {
                    finalBitmapHeight = parameters.Height ?? image.Height;
                    finalBitmap = new Bitmap(parameters.Width, finalBitmapHeight.Value);
                    finalBitmapGraphics = Graphics.FromImage(finalBitmap);
                }

                finalBitmapGraphics.DrawImage(image, x, 0, parameters.BarWidth, finalBitmapHeight.Value);

                x += parameters.BarWidth;

                progress?.Report((double)x / parameters.Width);

                image.Dispose();
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
