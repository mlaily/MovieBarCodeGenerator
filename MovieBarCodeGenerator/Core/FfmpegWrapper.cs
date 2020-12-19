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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MovieBarCodeGenerator.Core
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

        private static void TryKill(Process process)
        {
            try
            {
                process.Kill();
            }
            catch
            {
                // The call may fail if the process already exited, and we don't want to crash for that...
            }
        }

        public TimeSpan GetMediaDuration(string inputPath, CancellationToken cancellationToken, Action<string> log = null)
        {
            log?.Invoke("Getting input duration from FFmpeg...");

            var args = $"-i \"{inputPath}\"";

            var process = StartFfmpegInstance(args, redirectError: true);

            using (cancellationToken.Register(() => TryKill(process)))
            {
                var output = process.StandardError.ReadToEnd();

                foreach (var item in output.Split(new[] { '\n' }))
                {
                    // mimic the behaviour of ErrorDataReceived and BeginErrorReadLine:
                    log?.Invoke(item);
                }

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

        public IEnumerable<BitmapStream> GetImagesFromMedia(string inputPath, int frameCount, CancellationToken cancellationToken, Action<string> log = null)
        {
            var length = GetMediaDuration(inputPath, cancellationToken, log);

            log?.Invoke("Reading images from FFmpeg...");

            var fps = frameCount / length.TotalSeconds;

            // Output a raw stream of bitmap images taken at the specified frequency
            var args = $"-i \"{inputPath}\" -vf fps={fps.ToInvariantString()} -c:v bmp -f rawvideo -an -";

            var process = StartFfmpegInstance(args, redirectError: log != null);

            if (log != null)
            {
                process.ErrorDataReceived += (s, e) => log(e.Data);
                process.BeginErrorReadLine();
            }

            IEnumerable<BitmapStream> GetLazyStream()
            {
                using (cancellationToken.Register(() => TryKill(process)))
                using (var reader = new BinaryReader(process.StandardOutput.BaseStream))
                {
                    while (BitmapStream.TryCreate(reader, out var bitmapStream, cancellationToken))
                    {
                        yield return bitmapStream;
                    }
                }
            }

            return GetLazyStream();
        }
    }
}
