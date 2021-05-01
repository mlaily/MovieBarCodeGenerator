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

using PhotoSauce.MagicScaler;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MovieBarCodeGenerator.Core
{
    public interface IBarGenerator
    {
        string Name { get; }
        string DisplayName { get; }
        string FileNameSuffix { get; }
        Image GetBar(FileStream source, int barWidth, int barHeight);
    }

    public class ImageStreamProcessor
    {
        public IReadOnlyDictionary<IBarGenerator, Bitmap> CreateBarCodes(
            BarCodeParameters parameters,
            ImageProvider imageProvider,
            CancellationToken cancellationToken,
            IProgress<double> progress = null,
            Action<string> log = null)
        {
            var barCount = (int)Math.Round((double)parameters.Width / parameters.BarWidth);
            var bitmapStreamSource = imageProvider.GetImagesFromMedia(parameters.InputPath, barCount, cancellationToken, log);

            var barGenerators = parameters.GeneratorOutputPaths.Keys.ToArray();
            Bitmap[] finalBitmaps = new Bitmap[barGenerators.Length];
            Graphics[] finalBitmapGraphics = new Graphics[barGenerators.Length];

            int actualBarHeight = 0;

            int x = 0;
            foreach (var bitmapStream in bitmapStreamSource)
            {
                if (x == 0)
                {
                    var imageInfo = ImageFileInfo.Load(bitmapStream);

                    actualBarHeight = parameters.Height ?? imageInfo.Frames.First().Height;

                    for (int i = 0; i < barGenerators.Length; i++)
                    {
                        finalBitmaps[i] = new Bitmap(parameters.Width, actualBarHeight);
                        finalBitmapGraphics[i] = Graphics.FromImage(finalBitmaps[i]);
                    }
                }

                using (bitmapStream)
                {
                    for (int i = 0; i < barGenerators.Length; i++)
                    {
                        bitmapStream.Position = 0;
                        var bar = barGenerators[i].GetBar(bitmapStream, parameters.BarWidth, actualBarHeight);
                        var srcRect = new Rectangle(0, 0, bar.Width, bar.Height);
                        var destRect = new Rectangle(x, 0, parameters.BarWidth, actualBarHeight);
                        finalBitmapGraphics[i].DrawImage(bar, destRect, srcRect, GraphicsUnit.Pixel);
                    }

                    x += parameters.BarWidth;

                    progress?.Report((double)x / parameters.Width);
                }
            }

            for (int i = 0; i < barGenerators.Length; i++)
            {
                finalBitmapGraphics[i]?.Dispose();
            }

            var result = new Dictionary<IBarGenerator, Bitmap>();
            for (int i = 0; i < barGenerators.Length; i++)
            {
                result.Add(barGenerators[i], finalBitmaps[i]);
            }

            return result;
        }
    }
}
