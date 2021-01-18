using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MovieBarCodeGenerator.Core;
using NUnit.Framework;
using PhotoSauce.MagicScaler;

namespace MovieBarCodeGenerator.Tests
{
    [TestFixture]
    public class DebugTests
    {

        public string FfmpegExecutablePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "../../ffmpeg.exe");
        public string TestVideoFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.mkv");
        public const int TestVideoWidth = 1280;
        public const int TestVideoHeight = 720;
        public const int TestVideoDuration = 10;

        private void CreateTestVideoIfNecessary()
        {
            if (!File.Exists(TestVideoFilePath))
            {
                var commandArguments = $"-f lavfi -i testsrc=duration={TestVideoDuration}:size={TestVideoWidth}x{TestVideoHeight}:rate=30 {TestVideoFilePath}";
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = FfmpegExecutablePath,
                    Arguments = commandArguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                });
                process.WaitForExit(10000);
            }
        }

        [Test]
        public void FfFmpegExecutableExists()
        {
            Assert.IsTrue(File.Exists(FfmpegExecutablePath));
        }

        [Test]
        public void TestFileCanBeCreated()
        {
            CreateTestVideoIfNecessary();

            Assert.IsTrue(File.Exists(TestVideoFilePath));
        }

        [Test]
        public void Generate_All_Barcode_Settings_Combinations()
        {
            Assert.Inconclusive("Uncomment me to run the test");

            var ffmpegWrapper = new FfmpegWrapper(FfmpegExecutablePath);
            var streamProcessor = new ImageStreamProcessor();

            var allGenerators = new List<IBarGenerator>();

            // Legacy (reference)
            allGenerators.Add(GdiBarGenerator.CreateLegacy(false));
            // Legacy (reference) smoothed
            allGenerators.Add(GdiBarGenerator.CreateLegacy(true));

            foreach (var average in new[] { GdiAverage.No, GdiAverage.OnePass })
            {
                foreach (var interpolationMode in new[] { InterpolationMode.HighQualityBicubic, InterpolationMode.NearestNeighbor, InterpolationMode.Bicubic })
                {
                    allGenerators.Add(new GdiBarGenerator(average: average, scalingMode: ScalingMode.Sane, interpolationMode: interpolationMode));
                }
            }

            foreach (var average in new[] { false, true })
            {
                foreach (var interpolation in new[]
                {
                        InterpolationSettings.Average,
                        InterpolationSettings.CubicSmoother,
                        InterpolationSettings.NearestNeighbor,
                    })
                {
                    allGenerators.Add(new MagicScalerBarGenerator("noname", average: average, interpolation: interpolation));
                }
            }

            CreateTestVideoIfNecessary();
            var inputPath = TestVideoFilePath;

            inputPath = @"G:\videos\In Bruges (2008)\In.Bruges.2008.720p.BrRip.x264.YIFY.mp4";

            {
                var results = streamProcessor.CreateBarCodes(
                    new BarCodeParameters
                    {
                        Width = 1280,
                        BarWidth = 1,
                        InputPath = inputPath,
                        GeneratorOutputPaths = allGenerators.ToDictionary(x => x, x => x.Name)
                    },
                    ffmpegWrapper,
                     CancellationToken.None,
                     progress: null,
                     log: x => TestContext.WriteLine(x));

                foreach (var result in results)
                {
                    result.Value.Save($"{result.Key.Name}.png");
                }
            }

            //{
            //    var results = streamProcessor.CreateBarCodes(
            //        inputPath,
            //        new BarCodeParameters { Width = 500, BarWidth = 50 },
            //        ffmpegWrapper,
            //         CancellationToken.None,
            //         progress: null,
            //         log: x => TestContext.WriteLine(x),
            //         allGenerators.ToArray());

            //    foreach (var result in results)
            //    {
            //        result.Value.Save($"50-{result.Key.Name}.png");
            //    }
            //}
        }
    }
}
