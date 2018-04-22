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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MovieBarCodeGenerator
{
    public class FfmpegWrapper
    {
        public string FfmpegExecutablePath { get; }

        public FfmpegWrapper(string ffmpegExecutablePath)
        {
            FfmpegExecutablePath = ffmpegExecutablePath;
        }

        private Process StartFfmpegInstance(string args, bool redirectError = false)
        {
            try
            {
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = FfmpegExecutablePath,
                    Arguments = args,
                    // Warning: if a standard stread is redirected but is not read,
                    // its buffer might fill up and block the whole process.
                    // Only redirect a standard stream if you read it!
                    RedirectStandardOutput = true,
                    RedirectStandardError = redirectError,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                });
                return process;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while starting FFmpeg", ex);
            }
        }

        public TimeSpan GetMediaDuration(string inputPath, CancellationToken cancellationToken)
        {
            var args = $"-i \"{inputPath}\"";

            var process = StartFfmpegInstance(args, redirectError: true);

            using (cancellationToken.Register(() => process.Kill()))
            {
                var output = process.StandardError.ReadToEnd();

                cancellationToken.ThrowIfCancellationRequested();

                var match = Regex.Match(output, @"Duration: (.*?),");
                if (match.Success)
                {
                    var result = TimeSpan.Parse(match.Groups[1].Value);
                    return result;
                }
                else
                {
                    throw new FormatException();
                }
            }
        }

        public IEnumerable<Bitmap> GetImagesFromMedia(string inputPath, int frameCount, CancellationToken cancellationToken)
        {
            var length = GetMediaDuration(inputPath, cancellationToken);

            var fps = frameCount / length.TotalSeconds;

            // Output a raw stream of bitmap images taken at the specified frequency
            var args = $"-i \"{inputPath}\" -vf fps={fps} -c:v bmp -f rawvideo -an -";

            var process = StartFfmpegInstance(args);

            IEnumerable<Bitmap> GetLazyStream()
            {
                using (cancellationToken.Register(() => process.Kill()))
                {
                    foreach (var item in ReadBitmapStream(process.StandardOutput.BaseStream, cancellationToken))
                    {
                        yield return item;
                    }
                }
            }

            return GetLazyStream();
        }

        private IEnumerable<Bitmap> ReadBitmapStream(Stream stdout, CancellationToken cancellationToken)
        {
            using (var reader = new BinaryReader(stdout))
            {
                while (true)
                {
                    Bitmap bmp;
                    try
                    {
                        // https://en.wikipedia.org/wiki/BMP_file_format
                        var magicNumber = reader.ReadBytes(2);
                        if (magicNumber.Length != 2)
                        {
                            break;
                        }

                        if (magicNumber[0] != 0x42 || magicNumber[1] != 0x4D)
                        {
                            throw new InvalidDataException();
                        }

                        var bmpSizeBytes = reader.ReadBytes(4);
                        var bmpSize = BitConverter.ToInt32(bmpSizeBytes, 0);

                        var remainingDataLength = bmpSize - bmpSizeBytes.Length - magicNumber.Length;
                        var remainingData = reader.ReadBytes(remainingDataLength);

                        var ms = new MemoryStream();
                        ms.Write(magicNumber, 0, magicNumber.Length);
                        ms.Write(bmpSizeBytes, 0, bmpSizeBytes.Length);
                        ms.Write(remainingData, 0, remainingData.Length);

                        // We can't just give it our input stream,
                        // because it would not stop at the end of the first image.
                        bmp = new Bitmap(ms);
                    }
                    catch (Exception)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            yield break;
                        }
                        else
                        {
                            throw;
                        }
                    }

                    yield return bmp;
                }
            }
        }
    }
}
