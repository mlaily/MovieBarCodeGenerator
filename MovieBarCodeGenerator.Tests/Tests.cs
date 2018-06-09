using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MovieBarCodeGenerator.Core;

namespace MovieBarCodeGenerator.Tests
{
    [TestClass]
    public class Tests
    {

        public const string FfmpegExecutablePath = "../../ffmpeg.exe";
        public const string TestVideoFileName = "test.mkv";
        public const int TestVideoWidth = 1280;
        public const int TestVideoHeight = 720;
        public const int TestVideoDuration = 10;

        public TestContext TestContext { get; set; }

        private void CreateTestVideoIfNecessary()
        {
            if (!File.Exists(TestVideoFileName))
            {
                var commandArguments = $"-f lavfi -i testsrc=duration={TestVideoDuration}:size={TestVideoWidth}x{TestVideoHeight}:rate=30 {TestVideoFileName}";
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

        [TestMethod]
        public void FfFmpegExecutableExists()
        {
            Assert.IsTrue(File.Exists(FfmpegExecutablePath));
        }

        [TestMethod]
        public void TestFileCanBeCreated()
        {
            CreateTestVideoIfNecessary();

            Assert.IsTrue(File.Exists(TestVideoFileName));
        }

        [TestMethod]
        public void FfmpegWrapper_GetMediaDuration_Returns_Correct_Value()
        {
            CreateTestVideoIfNecessary();
            var ffmpegWrapper = new FfmpegWrapper(FfmpegExecutablePath);

            var duration = ffmpegWrapper.GetMediaDuration(TestVideoFileName, CancellationToken.None);

            Assert.AreEqual(TimeSpan.FromSeconds(TestVideoDuration), duration);
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(10)]
        [DataRow(21)]
        public void FfmpegWrapper_GetImagesFromMedia_Returns_Expected_Values(int requestedFrameCount)
        {
            CreateTestVideoIfNecessary();
            var ffmpegWrapper = new FfmpegWrapper(FfmpegExecutablePath);

            var images = ffmpegWrapper.GetImagesFromMedia(
                TestVideoFileName,
                requestedFrameCount,
                CancellationToken.None)
                .ToList();

            CollectionAssert.AllItemsAreNotNull(images);
            Assert.AreEqual(requestedFrameCount, images.Count);
            foreach (var image in images)
            {
                Assert.AreEqual(TestVideoWidth, image.Width);
                Assert.AreEqual(TestVideoHeight, image.Height);
            }
        }

        // TODO: CLI tests (file patterns, various flags combinations...), parameters validation tests, and actual barcode creation tests.
    }
}
