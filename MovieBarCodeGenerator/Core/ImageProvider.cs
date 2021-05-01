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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MovieBarCodeGenerator.Core
{
    public class ImageProvider
    {
        public IEnumerable<FileStream> GetImagesFromMedia(string inputPath, int frameCount, CancellationToken cancellationToken, Action<string> log = null)
        {
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            var imageFiles = Directory.EnumerateFiles(inputPath)
                .Where(x =>
                {
                    var extension = Path.GetExtension(x)?.ToLowerInvariant();
                    return imageExtensions.Contains(extension);
                })
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToList();

            log?.Invoke($"{inputPath} contains {imageFiles.Count} images...");

            double fps = (double)imageFiles.Count / frameCount;
            var indexToPathMap = Enumerable.Repeat((string)null, frameCount).Select((s, i) =>
            {
                var imageIndex = (int)Math.Min(imageFiles.Count - 1, i * fps);
                return imageFiles[imageIndex];
            });

            return GetLazyStream();

            // Output a raw stream of bitmap images taken at the specified frequency
            IEnumerable<FileStream> GetLazyStream()
            {
                foreach (var path in indexToPathMap)
                {
                    FileStream stream = null;
                    try
                    {
                        stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                    }
                    catch (Exception ex)
                    {
                        log?.Invoke($"Opening {path} failed: {ex}");
                    }

                    yield return stream;
                }
            }
        }
    }
}
