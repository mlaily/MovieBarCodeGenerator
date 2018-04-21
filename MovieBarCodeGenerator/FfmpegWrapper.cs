using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        private Process StartFfmpegInstance(string args)
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = FfmpegExecutablePath,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            });
            return process;
        }

        public TimeSpan GetMediaDuration(string inputPath)
        {
            var args = $"-i \"{inputPath}\"";

            var process = StartFfmpegInstance(args);

            var output = process.StandardError.ReadToEnd();

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

        public IEnumerable<Bitmap> GetImagesFromMedia(string inputPath, int frameCount)
        {
            var length = GetMediaDuration(inputPath);

            var fps = frameCount / length.TotalSeconds;

            // Output a raw stream of bitmap images taken at the specified frequency
            var args = $"-i \"{inputPath}\" -vf fps={fps} -c:v bmp -f rawvideo -an -";

            var process = StartFfmpegInstance(args);

            return ReadBitmapStream(process.StandardOutput.BaseStream);
        }

        private IEnumerable<Bitmap> ReadBitmapStream(Stream stdout)
        {
            using (var reader = new BinaryReader(stdout))
            {
                while (true)
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
                    var bmp = new Bitmap(ms);
                    yield return bmp;
                }
            }
        }
    }
}
